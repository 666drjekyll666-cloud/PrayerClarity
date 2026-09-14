using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace PrayerClarityPulpitFrameSliceProbe
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "nikich.graveyardkeeper.prayerclarity.pulpitframesliceprobe";
        private const string PluginName = "PrayerClarity Pulpit Frame Slice Probe";
        private const string PluginVersion = "0.1.0";
        private static readonly Guid SupportedMvid = new Guid("6f50b8e7-156b-49ac-bbe8-7505894b2364");
        private static readonly BindingFlags Inst = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly BindingFlags Stat = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private static Assembly _game;
        private static ManualLogSource _log;
        private static string _path;
        private static bool _dumped;

        private void Awake()
        {
            _log = Logger;
            try
            {
                _game = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => string.Equals(a.GetName().Name, "Assembly-CSharp", StringComparison.Ordinal));
                if (_game == null) throw new InvalidOperationException("Assembly-CSharp unavailable.");
                if (_game.ManifestModule.ModuleVersionId != SupportedMvid)
                    throw new InvalidOperationException("Unsupported game MVID: " + _game.ManifestModule.ModuleVersionId);

                _path = Path.Combine(Paths.BepInExRootPath, "PrayerClarity-pulpit-frame-slice-0.1.0.txt");
                File.WriteAllText(_path,
                    "PrayerClarity pulpit frame slice probe 0.1.0\r\n" +
                    "Read-only UI2DSprite / window geometry diagnostic.\r\n" +
                    "Game MVID: " + SupportedMvid + "\r\n" +
                    "UTC: " + DateTime.UtcNow.ToString("O") + "\r\n\r\n",
                    new UTF8Encoding(false));

                Type prayGui = GameType("PrayCraftGUI");
                Type worldGameObject = GameType("WorldGameObject");
                MethodInfo open = Method(prayGui, "Open", new[] { worldGameObject });
                Patch(open, StaticMethod(nameof(OpenPostfix)));

                Logger.LogInfo("PrayerClarity pulpit frame slice probe loaded. Open the pulpit once. Output: " + _path);
            }
            catch (Exception ex)
            {
                Logger.LogError("PrayerClarity pulpit frame slice probe disabled: " + ex);
            }
        }

        private static void OpenPostfix(object __instance)
        {
            if (_dumped) return;
            _dumped = true;

            try
            {
                object label = Get(__instance, "l_total_values");
                GameObject labelGo = GoOf(label);
                Transform container = labelGo == null ? null : labelGo.transform.parent;
                Transform window = container == null ? null : container.parent;
                if (window == null) throw new InvalidOperationException("Pray GUI window unavailable.");

                StringBuilder sb = new StringBuilder(8192);
                sb.AppendLine("=== WINDOW ROOT ===");
                DumpTransform(sb, window);
                DumpWidget(sb, window.gameObject);

                sb.AppendLine();
                sb.AppendLine("=== CONTAINER ===");
                DumpTransform(sb, container);
                DumpWidget(sb, container.gameObject);

                string[] paths =
                {
                    "back",
                    "decore_back",
                    "decore",
                    "header",
                    "header/pixel line",
                    "back for inactive stuff",
                    "craft button/craft button back",
                    "craft button/gamepad frame"
                };

                foreach (string path in paths)
                {
                    sb.AppendLine();
                    sb.AppendLine("=== " + path + " ===");
                    Transform t = window.Find(path);
                    if (t == null)
                    {
                        sb.AppendLine("<missing>");
                        continue;
                    }
                    DumpTransform(sb, t);
                    DumpWidget(sb, t.gameObject);
                    DumpSprite(sb, t.gameObject);
                }

                File.AppendAllText(_path, sb.ToString(), new UTF8Encoding(false));
                _log?.LogInfo("PrayerClarity pulpit frame slice probe complete: " + _path);
            }
            catch (Exception ex)
            {
                _log?.LogError("PrayerClarity pulpit frame slice probe failed: " + ex);
            }
        }

        private static void DumpTransform(StringBuilder sb, Transform t)
        {
            if (t == null) return;
            sb.AppendLine("path=" + PathOf(t));
            sb.AppendLine("localPosition=" + Vec3(t.localPosition));
            sb.AppendLine("localScale=" + Vec3(t.localScale));
            sb.AppendLine("activeSelf=" + t.gameObject.activeSelf + " activeInHierarchy=" + t.gameObject.activeInHierarchy);
        }

        private static void DumpWidget(StringBuilder sb, GameObject go)
        {
            if (go == null) return;
            object widget = FindComponentByName(go, "UIWidget");
            if (widget == null)
            {
                sb.AppendLine("UIWidget=<none>");
                return;
            }

            sb.AppendLine("UIWidget.type=" + widget.GetType().FullName);
            sb.AppendLine("width=" + S(Get(widget, "width")) + " height=" + S(Get(widget, "height")) + " pivot=" + S(Get(widget, "pivot")));
            sb.AppendLine("leftAnchor=" + Anchor(Get(widget, "leftAnchor")));
            sb.AppendLine("rightAnchor=" + Anchor(Get(widget, "rightAnchor")));
            sb.AppendLine("topAnchor=" + Anchor(Get(widget, "topAnchor")));
            sb.AppendLine("bottomAnchor=" + Anchor(Get(widget, "bottomAnchor")));
        }

        private static void DumpSprite(StringBuilder sb, GameObject go)
        {
            if (go == null) return;
            object sprite = FindComponentByName(go, "UI2DSprite");
            if (sprite == null)
            {
                sb.AppendLine("UI2DSprite=<none>");
                return;
            }

            sb.AppendLine("UI2DSprite.type=" + S(Get(sprite, "type")));
            sb.AppendLine("UI2DSprite.border=" + S(Get(sprite, "border")));
            sb.AppendLine("UI2DSprite.fixedAspect=" + S(Get(sprite, "fixedAspect")));
            sb.AppendLine("UI2DSprite.drawRegion=" + S(Get(sprite, "drawRegion")));
            sb.AppendLine("UI2DSprite.flip=" + S(Get(sprite, "flip")));
            sb.AppendLine("UI2DSprite.width=" + S(Get(sprite, "width")) + " height=" + S(Get(sprite, "height")) + " pivot=" + S(Get(sprite, "pivot")));

            Sprite unitySprite = Get(sprite, "sprite2D") as Sprite;
            if (unitySprite == null)
            {
                sb.AppendLine("sprite2D=<null>");
                return;
            }

            sb.AppendLine("sprite2D.name=" + unitySprite.name);
            sb.AppendLine("sprite2D.rect=" + RectS(unitySprite.rect));
            sb.AppendLine("sprite2D.textureRect=" + RectS(unitySprite.textureRect));
            sb.AppendLine("sprite2D.textureRectOffset=" + Vec2(unitySprite.textureRectOffset));
            sb.AppendLine("sprite2D.border=" + Vec4(unitySprite.border));
            sb.AppendLine("sprite2D.pixelsPerUnit=" + unitySprite.pixelsPerUnit.ToString("0.###"));
        }

        private static object FindComponentByName(GameObject go, string typeName)
        {
            if (go == null) return null;
            Component[] components;
            try { components = go.GetComponents<Component>(); }
            catch { return null; }
            return components.FirstOrDefault(c => c != null && string.Equals(c.GetType().Name, typeName, StringComparison.Ordinal));
        }

        private static string Anchor(object anchor)
        {
            if (anchor == null) return "<null>";
            object target = Get(anchor, "target");
            GameObject go = GoOf(target);
            return "target=" + (go == null ? S(target) : PathOf(go.transform)) +
                   ", relative=" + S(Get(anchor, "relative")) +
                   ", absolute=" + S(Get(anchor, "absolute"));
        }

        private static GameObject GoOf(object value)
        {
            if (value == null) return null;
            GameObject go = value as GameObject;
            if (go != null) return go;
            Component c = value as Component;
            if (c != null) return c.gameObject;
            return Get(value, "gameObject") as GameObject;
        }

        private static string PathOf(Transform t)
        {
            if (t == null) return "<null>";
            string path = t.name;
            Transform p = t.parent;
            int guard = 0;
            while (p != null && guard++ < 32)
            {
                path = p.name + "/" + path;
                p = p.parent;
            }
            return path;
        }

        private static object Get(object obj, string name)
        {
            if (obj == null) return null;
            try
            {
                for (Type type = obj.GetType(); type != null; type = type.BaseType)
                {
                    FieldInfo field = type.GetField(name, Inst);
                    if (field != null) return field.GetValue(obj);
                    PropertyInfo property = type.GetProperty(name, Inst);
                    if (property != null && property.CanRead) return property.GetValue(obj, null);
                }
            }
            catch { }
            return null;
        }

        private static Type GameType(string name)
        {
            Type exact = _game.GetType(name, false);
            if (exact != null) return exact;
            try { return _game.GetTypes().FirstOrDefault(t => t != null && t.Name == name); }
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

        private static MethodInfo Method(Type type, string name, Type[] signature)
        {
            return type == null || signature.Any(t => t == null)
                ? null
                : type.GetMethod(name, Inst, null, signature, null);
        }

        private static MethodInfo StaticMethod(string name)
        {
            return typeof(Plugin).GetMethod(name, Stat);
        }

        private static void Patch(MethodInfo target, MethodInfo postfix)
        {
            if (target == null) throw new MissingMethodException("Harmony target missing.");
            Type harmonyType = AnyType("HarmonyLib.Harmony");
            Type harmonyMethodType = AnyType("HarmonyLib.HarmonyMethod");
            if (harmonyType == null || harmonyMethodType == null) throw new InvalidOperationException("Harmony unavailable.");

            object harmony = Activator.CreateInstance(harmonyType, new object[] { PluginGuid });
            object post = postfix == null ? null : Activator.CreateInstance(harmonyMethodType, new object[] { postfix });
            MethodInfo patch = harmonyType.GetMethods(Inst)
                .FirstOrDefault(m => m.Name == "Patch" && m.GetParameters().Length >= 5);
            if (patch == null) throw new MissingMethodException("Harmony.Patch");

            object[] args = new object[patch.GetParameters().Length];
            args[0] = target;
            args[1] = null;
            args[2] = post;
            args[3] = null;
            args[4] = null;
            patch.Invoke(harmony, args);
        }

        private static string S(object value) { return value == null ? "<null>" : value.ToString(); }
        private static string Vec2(Vector2 v) { return "(" + v.x.ToString("0.###") + "," + v.y.ToString("0.###") + ")"; }
        private static string Vec3(Vector3 v) { return "(" + v.x.ToString("0.###") + "," + v.y.ToString("0.###") + "," + v.z.ToString("0.###") + ")"; }
        private static string Vec4(Vector4 v) { return "(" + v.x.ToString("0.###") + "," + v.y.ToString("0.###") + "," + v.z.ToString("0.###") + "," + v.w.ToString("0.###") + ")"; }
        private static string RectS(Rect r) { return "(" + r.x.ToString("0.###") + "," + r.y.ToString("0.###") + "," + r.width.ToString("0.###") + "," + r.height.ToString("0.###") + ")"; }
    }
}
