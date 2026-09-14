using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace PrayerClarityTestHarness
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("nikich.graveyardkeeper.prayerclarity", BepInDependency.DependencyFlags.HardDependency)]
    public sealed class PrayerClarityTestHarnessPlugin : BaseUnityPlugin
    {
        private const string PluginGuid = "nikich.graveyardkeeper.prayerclarity.testharness";
        private const string PluginName = "PrayerClarity Test Harness";
        private const string PluginVersion = "0.1.0";
        private static readonly Guid SupportedGameMvid = new Guid("6f50b8e7-156b-49ac-bbe8-7505894b2364");

        private static readonly BindingFlags Inst = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly BindingFlags Stat = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private static PrayerClarityTestHarnessPlugin _instance;
        private static ManualLogSource _log;
        private static Assembly _gameAssembly;
        private static object _openPrayGui;
        private static bool _syntheticSelection;

        private ConfigEntry<PreviewPrayer> _prayer;
        private ConfigEntry<PreviewQuality> _quality;

        private enum PreviewPrayer
        {
            Off,
            Ordinary,
            Faith,
            Donations,
            Combo,
            ShootsAndRoots,
            Repentance,
            Repose,
            Retribution,
            Protection,
            Imagination,
            Excellence,
            Prosperity,
            SoulsRepose,
            SoulContentment,
            ThoroughCleansing
        }

        private enum PreviewQuality
        {
            Bronze = 1,
            Silver = 2,
            Gold = 3
        }

        private void Awake()
        {
            _instance = this;
            _log = Logger;

            _prayer = Config.Bind("Preview", "Prayer", PreviewPrayer.Off,
                "TEST ONLY. Select a synthetic prayer to preview at the pulpit. No item or technology is granted to the save.");
            _quality = Config.Bind("Preview", "Quality", PreviewQuality.Bronze,
                "Quality tier used for the synthetic prayer preview.");
            Config.SettingChanged += OnSettingChanged;

            try
            {
                _gameAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => string.Equals(a.GetName().Name, "Assembly-CSharp", StringComparison.Ordinal));
                if (_gameAssembly == null)
                {
                    Logger.LogError("Assembly-CSharp unavailable; test harness disabled.");
                    return;
                }

                Guid actualMvid = _gameAssembly.ManifestModule.ModuleVersionId;
                if (actualMvid != SupportedGameMvid)
                {
                    Logger.LogWarning("Unsupported Graveyard Keeper build; test harness disabled. MVID=" + actualMvid);
                    return;
                }

                Type prayGui = GameType("PrayCraftGUI");
                Patch(Method(prayGui, "Open", 1), null, Method(typeof(PrayerClarityTestHarnessPlugin), nameof(OpenPostfix), true));
                Patch(Method(prayGui, "OnPrayButtonPressed", 0), Method(typeof(PrayerClarityTestHarnessPlugin), nameof(OnPrayButtonPressedPrefix), true), null);

                Logger.LogInfo("PrayerClarity Test Harness loaded. It only synthesizes pulpit UI selections; save/inventory/tech state is not modified.");
            }
            catch (Exception ex)
            {
                Logger.LogError("PrayerClarity Test Harness initialization failed: " + ex);
            }
        }

        private void OnDestroy()
        {
            Config.SettingChanged -= OnSettingChanged;
            if (ReferenceEquals(_instance, this)) _instance = null;
        }

        private void OnSettingChanged(object sender, SettingChangedEventArgs e)
        {
            TryApplyPreview();
        }

        private static void OpenPostfix(object __instance)
        {
            _openPrayGui = __instance;
            _syntheticSelection = false;
            TryApplyPreview();
        }

        private static bool OnPrayButtonPressedPrefix(object __instance)
        {
            if (!_syntheticSelection || !ReferenceEquals(__instance, _openPrayGui)) return true;

            _log?.LogWarning("Synthetic prayer preview cannot be preached. Set Preview > Prayer to Off or reopen the pulpit with the harness disabled to perform a real sermon.");
            SetButtonText(__instance, "TEST PREVIEW — SERMON DISABLED");
            return false;
        }

        private static void TryApplyPreview()
        {
            if (_instance == null || _openPrayGui == null) return;
            if (!IsGuiLive(_openPrayGui)) return;

            PreviewPrayer prayer = _instance._prayer.Value;
            if (prayer == PreviewPrayer.Off)
            {
                if (_syntheticSelection) ClearSyntheticSelection();
                return;
            }

            string itemId = GetItemId(prayer, _instance._quality.Value);
            if (string.IsNullOrEmpty(itemId)) return;

            try
            {
                Type itemType = GameType("Item");
                ConstructorInfo ctor = itemType?.GetConstructor(Inst, null, new[] { typeof(string), typeof(int) }, null);
                if (ctor == null) throw new MissingMethodException("Item(string,int)");

                object item = ctor.Invoke(new object[] { itemId, 1 });
                object definition = Get(item, "definition");
                if (definition == null) throw new InvalidOperationException("Synthetic item definition missing: " + itemId);

                MethodInfo pick = Method(_openPrayGui.GetType(), "OnResourcePickerClosed", new[] { itemType });
                if (pick == null) throw new MissingMethodException("PrayCraftGUI.OnResourcePickerClosed(Item)");

                pick.Invoke(_openPrayGui, new[] { item });
                _syntheticSelection = true;
                SetButtonText(_openPrayGui, "TEST PREVIEW — SERMON DISABLED");
                _log?.LogInfo("Synthetic pulpit preview selected: " + itemId + ". No inventory/save mutation performed.");
            }
            catch (Exception ex)
            {
                _syntheticSelection = false;
                _log?.LogError("Could not apply synthetic prayer preview: " + ex);
            }
        }

        private static void ClearSyntheticSelection()
        {
            try
            {
                Set(_openPrayGui, "pray_craft", null);
                Set(_openPrayGui, "_selected_item", null);
                MethodInfo redraw = Method(_openPrayGui.GetType(), "RedrawTextValues", new[] { typeof(float), typeof(float) });
                redraw?.Invoke(_openPrayGui, new object[] { 0f, 0f });
            }
            catch (Exception ex)
            {
                _log?.LogWarning("Synthetic preview clear failed; close/reopen the pulpit to restore vanilla selection state. " + ex.Message);
            }
            finally
            {
                _syntheticSelection = false;
            }
        }

        private static string GetItemId(PreviewPrayer prayer, PreviewQuality quality)
        {
            if (prayer == PreviewPrayer.Ordinary) return "b_empty";

            string root;
            switch (prayer)
            {
                case PreviewPrayer.Faith: root = "b_faith"; break;
                case PreviewPrayer.Donations: root = "b_money"; break;
                case PreviewPrayer.Combo: root = "b_faith_money"; break;
                case PreviewPrayer.ShootsAndRoots: root = "b_plant"; break;
                case PreviewPrayer.Repentance: root = "b_sins"; break;
                case PreviewPrayer.Repose: root = "b_skull"; break;
                case PreviewPrayer.Retribution: root = "b_sword"; break;
                case PreviewPrayer.Protection: root = "b_shield"; break;
                case PreviewPrayer.Imagination: root = "b_pen"; break;
                case PreviewPrayer.Excellence: root = "b_star"; break;
                case PreviewPrayer.Prosperity: root = "b_village"; break;
                case PreviewPrayer.SoulsRepose: root = "b_souls"; break;
                case PreviewPrayer.SoulContentment: root = "b_grat_points_incr"; break;
                case PreviewPrayer.ThoroughCleansing: root = "b_sin_shard"; break;
                default: return null;
            }

            return root + ":" + ((int)quality).ToString();
        }

        private static bool IsGuiLive(object gui)
        {
            object active = Get(gui, "isActiveAndEnabled");
            if (active is bool && !(bool)active) return false;
            object shown = Get(gui, "is_shown");
            return !(shown is bool) || (bool)shown;
        }

        private static void SetButtonText(object gui, string text)
        {
            try
            {
                object label = Get(gui, "l_button");
                if (label != null) Set(label, "text", text);
            }
            catch { }
        }

        private static Type GameType(string name)
        {
            if (_gameAssembly == null) return null;
            Type exact = _gameAssembly.GetType(name, false);
            if (exact != null) return exact;
            try { return _gameAssembly.GetTypes().FirstOrDefault(t => t != null && t.Name == name); }
            catch (ReflectionTypeLoadException ex) { return ex.Types.FirstOrDefault(t => t != null && t.Name == name); }
        }

        private static Type AnyType(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type type = assembly.GetType(name, false) ?? assembly.GetTypes().FirstOrDefault(t => t != null && t.Name == name);
                    if (type != null) return type;
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Type type = ex.Types.FirstOrDefault(t => t != null && t.Name == name);
                    if (type != null) return type;
                }
                catch { }
            }
            return null;
        }

        private static MethodInfo Method(Type type, string name, int parameterCount)
        {
            if (type == null) return null;
            return type.GetMethods(Inst).FirstOrDefault(m => m.Name == name && m.GetParameters().Length == parameterCount);
        }

        private static MethodInfo Method(Type type, string name, bool isStatic)
        {
            if (type == null) return null;
            return type.GetMethods(isStatic ? Stat : Inst).FirstOrDefault(m => m.Name == name);
        }

        private static MethodInfo Method(Type type, string name, Type[] signature)
        {
            return type?.GetMethod(name, Inst, null, signature, null);
        }

        private static object Get(object obj, string name)
        {
            if (obj == null) return null;
            for (Type type = obj.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, Inst);
                if (field != null) return field.GetValue(obj);
                PropertyInfo property = type.GetProperty(name, Inst);
                if (property != null && property.CanRead) return property.GetValue(obj, null);
            }
            return null;
        }

        private static void Set(object obj, string name, object value)
        {
            if (obj == null) return;
            for (Type type = obj.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, Inst);
                if (field != null) { field.SetValue(obj, value); return; }
                PropertyInfo property = type.GetProperty(name, Inst);
                if (property != null && property.CanWrite) { property.SetValue(obj, value, null); return; }
            }
        }

        private static void Patch(MethodInfo target, MethodInfo prefix, MethodInfo postfix)
        {
            if (target == null) throw new MissingMethodException("Harmony target missing");
            Type harmonyType = AnyType("HarmonyLib.Harmony");
            Type harmonyMethodType = AnyType("HarmonyLib.HarmonyMethod");
            if (harmonyType == null || harmonyMethodType == null) throw new InvalidOperationException("Harmony unavailable");

            object harmony = Activator.CreateInstance(harmonyType, new object[] { PluginGuid });
            object harmonyPrefix = prefix == null ? null : Activator.CreateInstance(harmonyMethodType, new object[] { prefix });
            object harmonyPostfix = postfix == null ? null : Activator.CreateInstance(harmonyMethodType, new object[] { postfix });

            MethodInfo patch = harmonyType.GetMethods(Inst)
                .FirstOrDefault(m => m.Name == "Patch" && m.GetParameters().Length >= 5 && typeof(MethodBase).IsAssignableFrom(m.GetParameters()[0].ParameterType));
            if (patch == null) throw new MissingMethodException("Harmony.Patch");

            object[] args = new object[patch.GetParameters().Length];
            args[0] = target;
            args[1] = harmonyPrefix;
            args[2] = harmonyPostfix;
            args[3] = null;
            args[4] = null;
            patch.Invoke(harmony, args);
        }
    }
}
