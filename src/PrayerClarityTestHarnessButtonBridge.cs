using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace PrayerClarityTestHarness
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("nikich.graveyardkeeper.prayerclarity.testharness", BepInDependency.DependencyFlags.HardDependency)]
    public sealed class PrayerClarityTestHarnessButtonBridgePlugin : BaseUnityPlugin
    {
        private const string PluginGuid = "nikich.graveyardkeeper.prayerclarity.testharness.buttonbridge";
        private const string PluginName = "PrayerClarity Test Harness Button Bridge";
        private const string PluginVersion = "0.1.3";

        private static readonly BindingFlags Inst = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly BindingFlags Stat = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private static ManualLogSource _log;

        private void Awake()
        {
            _log = Logger;
            try
            {
                MethodInfo target = typeof(PrayerClarityTestHarnessPlugin).GetMethod("RefreshSyntheticButtonText", Stat);
                MethodInfo postfix = typeof(PrayerClarityTestHarnessButtonBridgePlugin).GetMethod(nameof(RefreshSyntheticButtonTextPostfix), Stat);
                Patch(target, postfix);
                Logger.LogInfo("PrayerClarity Test Harness Button Bridge 0.1.3 loaded. Synthetic timed-buff previews explicitly enable the stock pulpit craft button; no polling is used.");
            }
            catch (Exception ex)
            {
                Logger.LogError("PrayerClarity Test Harness Button Bridge initialization failed: " + ex);
            }
        }

        private static void RefreshSyntheticButtonTextPostfix(object gui)
        {
            try
            {
                string buffId = SelectedBuffId(gui);
                bool enable = !string.IsNullOrEmpty(buffId);
                SetCraftButtonEnabled(gui, enable, buffId);
            }
            catch (Exception ex)
            {
                _log?.LogError("Could not update synthetic pulpit button enabled-state: " + ex);
            }
        }

        private static string SelectedBuffId(object gui)
        {
            object craft = Get(gui, "pray_craft");
            return craft == null ? null : Get(craft, "buff") as string;
        }

        private static void SetCraftButtonEnabled(object gui, bool enabled, string buffId)
        {
            GameObject guiObject = Get(gui, "gameObject") as GameObject;
            if (guiObject == null) throw new InvalidOperationException("PrayCraftGUI gameObject unavailable.");

            Transform craftButton = guiObject.transform.Find("window/craft button");
            if (craftButton == null)
            {
                craftButton = guiObject.GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(t => t != null && string.Equals(t.name, "craft button", StringComparison.Ordinal));
            }
            if (craftButton == null) throw new MissingMemberException("Pulpit stock 'craft button' transform not found.");

            Type uiButtonType = AnyType("UIButton");
            Component uiButton = uiButtonType == null ? null : craftButton.gameObject.GetComponent(uiButtonType);
            if (uiButton == null && uiButtonType != null)
                uiButton = craftButton.gameObject.GetComponentsInChildren(uiButtonType, true).FirstOrDefault();

            if (uiButton != null)
                Set(uiButton, "isEnabled", enabled);

            foreach (Collider collider in craftButton.GetComponentsInChildren<Collider>(true))
                collider.enabled = enabled;
            foreach (Collider2D collider in craftButton.GetComponentsInChildren<Collider2D>(true))
                collider.enabled = enabled;

            _log?.LogInfo("Synthetic pulpit stock button -> " + (enabled ? "ENABLED" : "disabled") +
                          (string.IsNullOrEmpty(buffId) ? string.Empty : " for buff " + buffId) +
                          "; UIButton=" + (uiButton == null ? "missing" : uiButton.GetType().FullName) + ".");
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
            throw new MissingMemberException(obj.GetType().FullName, name);
        }

        private static void Patch(MethodInfo target, MethodInfo postfix)
        {
            if (target == null || postfix == null) throw new MissingMethodException("Harness button bridge Harmony target/postfix missing.");
            Type harmonyType = AnyType("HarmonyLib.Harmony");
            Type harmonyMethodType = AnyType("HarmonyLib.HarmonyMethod");
            if (harmonyType == null || harmonyMethodType == null) throw new InvalidOperationException("Harmony unavailable.");

            object harmony = Activator.CreateInstance(harmonyType, new object[] { PluginGuid });
            object harmonyPostfix = Activator.CreateInstance(harmonyMethodType, new object[] { postfix });
            MethodInfo patch = harmonyType.GetMethods(Inst)
                .FirstOrDefault(m => m.Name == "Patch" && m.GetParameters().Length >= 5 && typeof(MethodBase).IsAssignableFrom(m.GetParameters()[0].ParameterType));
            if (patch == null) throw new MissingMethodException("Harmony.Patch");

            object[] args = new object[patch.GetParameters().Length];
            args[0] = target;
            args[1] = null;
            args[2] = harmonyPostfix;
            args[3] = null;
            args[4] = null;
            patch.Invoke(harmony, args);
        }
    }
}
