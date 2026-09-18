using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using UnityEngine;

namespace PrayerClarityResearch
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class PrayerClarityReposeOwnerProbe : BaseUnityPlugin
    {
        public const string PluginGuid = "prayerclarity.research.reposeowner";
        public const string PluginName = "PrayerClarity Repose Owner Probe";
        public const string PluginVersion = "0.1.0";

        private static readonly object Gate = new object();
        private static string _outputPath;
        private object _harmony;

        private void Awake()
        {
            _outputPath = Path.Combine(Paths.BepInExRootPath, "PrayerClarity-repose-owner-probe-0.1.0.txt");
            File.WriteAllText(
                _outputPath,
                "PRAYERCLARITY — REPOSE OWNER PROBE\n" +
                "ProbeVersion=" + PluginVersion + "\n" +
                "GeneratedUtc=" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture) + "\n" +
                "Contract=LOG_ONLY_HARMONY_PREFIX_NO_ARGUMENT_OR_RESULT_MUTATION_NO_SAVE_WRITE\n" +
                "Question=What owner/graph identity is visible from executing Flow_DropBody nodes during ordinary donkey delivery?\n\n",
                new UTF8Encoding(false));

            try
            {
                InstallPrefix();
                Append("PATCH_OK=true\n");
                Logger.LogInfo("Repose Owner Probe 0.1.0 installed. Wait for one ordinary donkey corpse delivery, then send " + _outputPath);
            }
            catch (Exception ex)
            {
                Append("PATCH_OK=false\nERROR=" + ex + "\n");
                Logger.LogError("Repose Owner Probe patch failed: " + ex);
            }
        }

        private void OnDestroy()
        {
            try
            {
                if (_harmony == null) return;
                MethodInfo unpatchSelf = _harmony.GetType().GetMethod("UnpatchSelf", BindingFlags.Public | BindingFlags.Instance);
                if (unpatchSelf != null) unpatchSelf.Invoke(_harmony, null);
            }
            catch { }
        }

        private void InstallPrefix()
        {
            Type dropType = FindType("FlowCanvas.Nodes.Flow_DropBody");
            if (dropType == null) throw new InvalidOperationException("Flow_DropBody type not found");

            Type callbackType = dropType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(t => t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Any(f => f.FieldType == dropType));
            if (callbackType == null) throw new InvalidOperationException("Flow_DropBody callback display class not found");

            MethodInfo target = callbackType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name.IndexOf("<RegisterPorts>b__", StringComparison.Ordinal) >= 0 && m.GetParameters().Length == 1);
            if (target == null) throw new InvalidOperationException("Flow_DropBody execution callback not found");

            Type harmonyType = FindType("HarmonyLib.Harmony");
            Type harmonyMethodType = FindType("HarmonyLib.HarmonyMethod");
            if (harmonyType == null || harmonyMethodType == null) throw new InvalidOperationException("HarmonyLib runtime types not found");

            _harmony = Activator.CreateInstance(harmonyType, new object[] { PluginGuid });
            MethodInfo prefixMethod = typeof(PrayerClarityReposeOwnerProbe).GetMethod(nameof(Prefix), BindingFlags.NonPublic | BindingFlags.Static);
            object harmonyPrefix = CreateHarmonyMethod(harmonyMethodType, prefixMethod);

            MethodInfo patch = harmonyType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == "Patch")
                .FirstOrDefault(m =>
                {
                    ParameterInfo[] p = m.GetParameters();
                    return p.Length >= 2 && typeof(MethodBase).IsAssignableFrom(p[0].ParameterType) && p[1].ParameterType == harmonyMethodType;
                });
            if (patch == null) throw new InvalidOperationException("Harmony.Patch overload not found");

            ParameterInfo[] pars = patch.GetParameters();
            object[] args = new object[pars.Length];
            args[0] = target;
            args[1] = harmonyPrefix;
            for (int i = 2; i < args.Length; i++) args[i] = null;
            patch.Invoke(_harmony, args);

            Append("Target=" + DescribeMethod(target) + "\n");
        }

        private static object CreateHarmonyMethod(Type harmonyMethodType, MethodInfo method)
        {
            ConstructorInfo ctor = harmonyMethodType.GetConstructor(new[] { typeof(MethodInfo) });
            if (ctor != null) return ctor.Invoke(new object[] { method });

            object hm = Activator.CreateInstance(harmonyMethodType);
            FieldInfo methodField = harmonyMethodType.GetField("method", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodField == null) throw new InvalidOperationException("HarmonyMethod.method field not found");
            methodField.SetValue(hm, method);
            return hm;
        }

        private static void Prefix(object __instance)
        {
            try
            {
                object node = FindCapturedNode(__instance);
                StringBuilder sb = new StringBuilder(4096);
                sb.AppendLine("=== FLOW_DROPBODY EXECUTION ===");
                sb.AppendLine("utc=" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
                sb.AppendLine("callback=" + DescribeObject(__instance));
                sb.AppendLine("node=" + DescribeObject(node));

                if (node != null)
                {
                    sb.AppendLine("node.ID=" + ReadMember(node, "ID", "_ID"));
                    sb.AppendLine("node.UID=" + ReadMember(node, "UID", "_UID"));
                    sb.AppendLine("node.name=" + ReadMember(node, "name", "_name", "_nodeName"));
                    sb.AppendLine("node.customName=" + ReadMember(node, "customName"));
                    sb.AppendLine("node.tag=" + ReadMember(node, "tag", "_tag"));

                    object graph = GetMemberObject(node, "graph", "_graph");
                    object agent = GetMemberObject(node, "graphAgent");
                    object cfs = GetMemberObject(node, "cfs");
                    object wgo = GetMemberObject(node, "wgo");

                    DumpContext(sb, "graph", graph);
                    DumpContext(sb, "graphAgent", agent);
                    DumpContext(sb, "cfs", cfs);
                    DumpContext(sb, "wgo", wgo);
                }

                sb.AppendLine("=== END FLOW_DROPBODY EXECUTION ===");
                Append(sb.ToString());
            }
            catch (Exception ex)
            {
                Append("PREFIX_ERROR=" + ex + "\n");
            }
        }

        private static object FindCapturedNode(object callback)
        {
            if (callback == null) return null;
            Type dropType = FindType("FlowCanvas.Nodes.Flow_DropBody");
            foreach (FieldInfo f in callback.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (dropType != null && f.FieldType == dropType)
                {
                    try { return f.GetValue(callback); } catch { }
                }
            }
            return null;
        }

        private static void DumpContext(StringBuilder sb, string label, object obj)
        {
            sb.AppendLine(label + "=" + DescribeObject(obj));
            if (obj == null) return;

            UnityEngine.Object unity = obj as UnityEngine.Object;
            if (unity != null)
            {
                sb.AppendLine(label + ".unityName=" + Quote(unity.name));
                Component comp = unity as Component;
                if (comp != null) sb.AppendLine(label + ".path=" + Quote(GetHierarchyPath(comp.transform)));
            }

            string[] interesting = { "name", "id", "graph", "owner", "agent", "wgo", "parent", "script", "source" };
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (FieldInfo f in obj.GetType().GetFields(flags).Where(f => interesting.Any(k => f.Name.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0)).Take(24))
            {
                try { sb.AppendLine(label + ".field." + f.Name + "=" + SafeValue(f.GetValue(obj))); } catch { }
            }
        }

        private static object GetMemberObject(object obj, params string[] names)
        {
            if (obj == null) return null;
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            Type t = obj.GetType();
            foreach (string name in names)
            {
                for (Type cur = t; cur != null; cur = cur.BaseType)
                {
                    PropertyInfo p = cur.GetProperty(name, flags | BindingFlags.DeclaredOnly);
                    if (p != null && p.GetIndexParameters().Length == 0)
                    {
                        try { return p.GetValue(obj, null); } catch { }
                    }
                    FieldInfo f = cur.GetField(name, flags | BindingFlags.DeclaredOnly);
                    if (f != null)
                    {
                        try { return f.GetValue(obj); } catch { }
                    }
                }
            }
            return null;
        }

        private static string ReadMember(object obj, params string[] names)
        {
            object value = GetMemberObject(obj, names);
            return SafeValue(value);
        }

        private static string DescribeObject(object value)
        {
            if (value == null) return "<null>";
            return value.GetType().FullName + " " + SafeValue(value);
        }

        private static string SafeValue(object value)
        {
            if (value == null) return "<null>";
            try
            {
                UnityEngine.Object unity = value as UnityEngine.Object;
                if (unity != null) return value.GetType().Name + "#" + unity.GetInstanceID() + " name=" + Quote(unity.name);
                return Convert.ToString(value, CultureInfo.InvariantCulture) ?? "<null-string>";
            }
            catch { return "<unprintable>"; }
        }

        private static string GetHierarchyPath(Transform tf)
        {
            if (tf == null) return "";
            List<string> parts = new List<string>();
            for (Transform cur = tf; cur != null; cur = cur.parent) parts.Add(cur.name);
            parts.Reverse();
            return string.Join("/", parts.ToArray());
        }

        private static string Quote(string s)
        {
            if (s == null) return "<null>";
            return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }

        private static string DescribeMethod(MethodBase m)
        {
            if (m == null) return "<null>";
            return (m.DeclaringType == null ? "<no-type>" : m.DeclaringType.FullName) + "." + m.Name + " token=0x" + m.MetadataToken.ToString("X8", CultureInfo.InvariantCulture);
        }

        private static Type FindType(string fullName)
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type t = asm.GetType(fullName, false);
                    if (t != null) return t;
                }
                catch { }
            }
            return null;
        }

        private static void Append(string text)
        {
            if (string.IsNullOrEmpty(_outputPath)) return;
            lock (Gate)
            {
                File.AppendAllText(_outputPath, text, new UTF8Encoding(false));
            }
        }
    }
}
