using System;
using System.Collections;
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
    public sealed class PrayerClarityReposeGraphProbe : BaseUnityPlugin
    {
        public const string PluginGuid = "prayerclarity.research.reposegraph";
        public const string PluginName = "PrayerClarity Repose Graph Probe";
        public const string PluginVersion = "0.1.0";

        private static readonly Guid SupportedGameMvid = new Guid("6f50b8e7-156b-49ac-bbe8-7505894b2364");
        private bool _completed;
        private float _nextTry;

        private void Awake()
        {
            Logger.LogInfo(PluginName + " " + PluginVersion + " loaded. Load-only reflection audit; no Harmony, graph execution, balance mutation, or save writes.");
        }

        private void Update()
        {
            if (_completed || Time.realtimeSinceStartup < _nextTry) return;
            _nextTry = Time.realtimeSinceStartup + 1f;

            try
            {
                Type controllerType = FindType("FlowCanvas.FlowScriptController");
                Type dropType = FindType("FlowCanvas.Nodes.Flow_DropBody");
                Type nodeType = FindType("NodeCanvas.Framework.Node");
                if (controllerType == null || dropType == null || nodeType == null) return;

                UnityEngine.Object controller = FindDonkeyController(controllerType);
                if (controller == null) return;

                object graph = FindGraph(controller);
                if (graph == null) return;

                Assembly game = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => string.Equals(a.GetName().Name, "Assembly-CSharp", StringComparison.Ordinal));
                if (game == null || game.ManifestModule.ModuleVersionId != SupportedGameMvid)
                    throw new InvalidOperationException("Unsupported Assembly-CSharp MVID " + (game == null ? "<missing>" : game.ManifestModule.ModuleVersionId.ToString()));

                _completed = true;
                WriteReport(controller, graph, dropType, nodeType, game.ManifestModule.ModuleVersionId);
            }
            catch (Exception ex)
            {
                _completed = true;
                string path = Path.Combine(Paths.BepInExRootPath, "PrayerClarity-repose-graph-probe-0.1.0.txt");
                File.WriteAllText(path, "PRAYERCLARITY — REPOSE GRAPH PROBE\nProbeVersion=" + PluginVersion + "\nERROR=" + ex + "\n", new UTF8Encoding(false));
                Logger.LogError(PluginName + " failed: " + ex);
            }
        }

        private static UnityEngine.Object FindDonkeyController(Type controllerType)
        {
            UnityEngine.Object[] objects = Resources.FindObjectsOfTypeAll(controllerType);
            foreach (UnityEngine.Object obj in objects)
            {
                if (obj == null) continue;
                if (string.Equals(obj.name, "[wgo] donkey", StringComparison.Ordinal)) return obj;
                Component c = obj as Component;
                if (c != null && string.Equals(GetHierarchyPath(c.transform), "World/[wgo] donkey", StringComparison.Ordinal)) return obj;
            }
            return null;
        }

        private static object FindGraph(object controller)
        {
            object direct = GetMemberObject(controller, "graph", "_graph");
            if (direct != null) return direct;

            foreach (MemberInfo member in EnumerateMembers(controller.GetType()))
            {
                if (member.Name.IndexOf("graph", StringComparison.OrdinalIgnoreCase) < 0) continue;
                object value = ReadMemberValue(controller, member);
                if (value == null) continue;
                string fullName = value.GetType().FullName ?? string.Empty;
                if (fullName.IndexOf("FlowScript", StringComparison.OrdinalIgnoreCase) >= 0) return value;
            }
            return null;
        }

        private static void WriteReport(UnityEngine.Object controller, object graph, Type dropType, Type nodeType, Guid mvid)
        {
            StringBuilder sb = new StringBuilder(128 * 1024);
            sb.AppendLine("PRAYERCLARITY — REPOSE GRAPH PROBE");
            sb.AppendLine("ProbeVersion=" + PluginVersion);
            sb.AppendLine("GeneratedUtc=" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            sb.AppendLine("ModuleVersionId=" + mvid);
            sb.AppendLine("Contract=LOAD_ONLY_REFLECTION_NO_HARMONY_NO_GRAPH_EXECUTION_NO_MUTATION_NO_SAVE_WRITE");
            sb.AppendLine("Question=Map every live npc_donkey Flow_DropBody node and its connection fingerprint after runtime node-ID mismatch was observed.");
            sb.AppendLine();
            sb.AppendLine("controller=" + Describe(controller));
            Component comp = controller as Component;
            if (comp != null) sb.AppendLine("controller.path=" + Quote(GetHierarchyPath(comp.transform)));
            sb.AppendLine("graph=" + Describe(graph));
            sb.AppendLine("graph.name=" + ReadMember(graph, "name", "_name"));
            sb.AppendLine();

            List<object> nodes = CollectNodes(graph, nodeType);
            sb.AppendLine("graph.nodeCount=" + nodes.Count.ToString(CultureInfo.InvariantCulture));

            List<object> drops = nodes.Where(n => dropType.IsAssignableFrom(n.GetType())).OrderBy(GetNodeId).ToList();
            sb.AppendLine("dropNodeCount=" + drops.Count.ToString(CultureInfo.InvariantCulture));
            sb.AppendLine();

            foreach (object node in drops)
            {
                DumpDropNode(sb, node, nodeType);
                sb.AppendLine();
            }

            string path = Path.Combine(Paths.BepInExRootPath, "PrayerClarity-repose-graph-probe-0.1.0.txt");
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
        }

        private static List<object> CollectNodes(object graph, Type nodeType)
        {
            HashSet<object> seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
            List<object> result = new List<object>();

            foreach (MemberInfo member in EnumerateMembers(graph.GetType()))
            {
                object value = ReadMemberValue(graph, member);
                IEnumerable enumerable = value as IEnumerable;
                if (enumerable == null || value is string) continue;

                IEnumerator e;
                try { e = enumerable.GetEnumerator(); } catch { continue; }
                if (e == null) continue;
                while (true)
                {
                    bool moved;
                    try { moved = e.MoveNext(); } catch { break; }
                    if (!moved) break;
                    object item;
                    try { item = e.Current; } catch { continue; }
                    if (item == null || !nodeType.IsAssignableFrom(item.GetType()) || !seen.Add(item)) continue;
                    result.Add(item);
                }
            }

            return result;
        }

        private static void DumpDropNode(StringBuilder sb, object node, Type nodeType)
        {
            sb.AppendLine("=== DROP NODE ===");
            sb.AppendLine("type=" + node.GetType().FullName);
            sb.AppendLine("ID=" + ReadMember(node, "ID", "_ID"));
            sb.AppendLine("UID=" + ReadMember(node, "UID", "_UID"));
            sb.AppendLine("name=" + ReadMember(node, "name", "_name", "_nodeName"));
            sb.AppendLine("customName=" + ReadMember(node, "customName"));
            sb.AppendLine("tag=" + ReadMember(node, "tag", "_tag"));

            object inputValues = GetMemberObject(node, "_inputPortValues");
            DumpDictionary(sb, "inputPortValues", inputValues);
            object inputPorts = GetMemberObject(node, "inputPorts", "_inputPorts");
            DumpDictionary(sb, "inputPorts", inputPorts);

            object inConnections = GetMemberObject(node, "inConnections", "_inConnections");
            DumpConnections(sb, "inConnections", inConnections, nodeType, 0, new HashSet<object>(ReferenceEqualityComparer.Instance));
        }

        private static void DumpDictionary(StringBuilder sb, string label, object dictionary)
        {
            IDictionary dict = dictionary as IDictionary;
            if (dict == null)
            {
                sb.AppendLine(label + "=<not IDictionary> " + Describe(dictionary));
                return;
            }

            sb.AppendLine(label + ".count=" + dict.Count.ToString(CultureInfo.InvariantCulture));
            int i = 0;
            foreach (DictionaryEntry entry in dict)
            {
                sb.AppendLine(label + "[" + i.ToString(CultureInfo.InvariantCulture) + "].key=" + SafeValue(entry.Key));
                sb.AppendLine(label + "[" + i.ToString(CultureInfo.InvariantCulture) + "].value=" + Describe(entry.Value));
                DumpInterestingMembers(sb, label + "[" + i.ToString(CultureInfo.InvariantCulture) + "].value", entry.Value, 16);
                i++;
            }
        }

        private static void DumpConnections(StringBuilder sb, string label, object connections, Type nodeType, int depth, HashSet<object> visitedNodes)
        {
            IEnumerable enumerable = connections as IEnumerable;
            if (enumerable == null || connections is string)
            {
                sb.AppendLine(label + "=<not enumerable> " + Describe(connections));
                return;
            }

            List<object> list = new List<object>();
            foreach (object c in enumerable) if (c != null) list.Add(c);
            sb.AppendLine(label + ".count=" + list.Count.ToString(CultureInfo.InvariantCulture));

            for (int i = 0; i < list.Count; i++)
            {
                object connection = list[i];
                string prefix = label + "[" + i.ToString(CultureInfo.InvariantCulture) + "]";
                sb.AppendLine(prefix + "=" + Describe(connection));
                DumpInterestingMembers(sb, prefix, connection, 24);

                object source = GetMemberObject(connection, "sourceNode", "source", "_sourceNode");
                if (source == null)
                {
                    object sourcePort = GetMemberObject(connection, "sourcePort", "_sourcePort");
                    source = GetMemberObject(sourcePort, "parent", "parentNode", "node", "_parent");
                }

                if (source != null && nodeType.IsAssignableFrom(source.GetType()))
                {
                    sb.AppendLine(prefix + ".sourceNode=" + NodeBrief(source));
                    DumpInterestingMembers(sb, prefix + ".sourceNode", source, 28);
                    if (depth < 2 && visitedNodes.Add(source))
                    {
                        object upstream = GetMemberObject(source, "inConnections", "_inConnections");
                        DumpConnections(sb, prefix + ".sourceNode.inConnections", upstream, nodeType, depth + 1, visitedNodes);
                    }
                }
            }
        }

        private static void DumpInterestingMembers(StringBuilder sb, string label, object obj, int max)
        {
            if (obj == null) return;
            string[] needles = { "id", "uid", "name", "value", "param", "var", "port", "source", "target", "input", "output", "type" };
            int count = 0;
            foreach (MemberInfo member in EnumerateMembers(obj.GetType()))
            {
                if (!needles.Any(n => member.Name.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0)) continue;
                object value = ReadMemberValue(obj, member);
                if (!IsSimple(value)) continue;
                sb.AppendLine(label + "." + member.Name + "=" + SafeValue(value));
                count++;
                if (count >= max) break;
            }
        }

        private static IEnumerable<MemberInfo> EnumerateMembers(Type type)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            for (Type cur = type; cur != null; cur = cur.BaseType)
            {
                foreach (FieldInfo f in cur.GetFields(flags)) yield return f;
                foreach (PropertyInfo p in cur.GetProperties(flags))
                    if (p.CanRead && p.GetIndexParameters().Length == 0) yield return p;
            }
        }

        private static object ReadMemberValue(object obj, MemberInfo member)
        {
            try
            {
                FieldInfo f = member as FieldInfo;
                if (f != null) return f.GetValue(obj);
                PropertyInfo p = member as PropertyInfo;
                if (p != null) return p.GetValue(obj, null);
            }
            catch { }
            return null;
        }

        private static object GetMemberObject(object obj, params string[] names)
        {
            if (obj == null) return null;
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            foreach (string name in names)
            {
                for (Type cur = obj.GetType(); cur != null; cur = cur.BaseType)
                {
                    PropertyInfo p = cur.GetProperty(name, flags);
                    if (p != null && p.GetIndexParameters().Length == 0)
                    {
                        try { return p.GetValue(obj, null); } catch { }
                    }
                    FieldInfo f = cur.GetField(name, flags);
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
            return SafeValue(GetMemberObject(obj, names));
        }

        private static int GetNodeId(object node)
        {
            object value = GetMemberObject(node, "ID", "_ID");
            try { return Convert.ToInt32(value, CultureInfo.InvariantCulture); } catch { return int.MaxValue; }
        }

        private static string NodeBrief(object node)
        {
            return node.GetType().FullName + " ID=" + ReadMember(node, "ID", "_ID") + " UID=" + ReadMember(node, "UID", "_UID") + " name=" + ReadMember(node, "name", "_name", "_nodeName");
        }

        private static string Describe(object value)
        {
            if (value == null) return "<null>";
            UnityEngine.Object unity = value as UnityEngine.Object;
            if (unity != null) return value.GetType().FullName + "#" + unity.GetInstanceID().ToString(CultureInfo.InvariantCulture) + " name=" + Quote(unity.name);
            return value.GetType().FullName + " " + SafeValue(value);
        }

        private static bool IsSimple(object value)
        {
            if (value == null) return true;
            Type t = value.GetType();
            return value is string || t.IsPrimitive || t.IsEnum || value is decimal || value is Guid;
        }

        private static string SafeValue(object value)
        {
            if (value == null) return "<null>";
            try
            {
                if (value is string) return Quote((string)value);
                UnityEngine.Object unity = value as UnityEngine.Object;
                if (unity != null) return unity.GetType().Name + "#" + unity.GetInstanceID().ToString(CultureInfo.InvariantCulture) + " name=" + Quote(unity.name);
                return Convert.ToString(value, CultureInfo.InvariantCulture) ?? "<null-string>";
            }
            catch { return "<unprintable>"; }
        }

        private static string GetHierarchyPath(Transform tf)
        {
            if (tf == null) return string.Empty;
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

        private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();
            public new bool Equals(object x, object y) { return ReferenceEquals(x, y); }
            public int GetHashCode(object obj) { return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj); }
        }
    }
}
