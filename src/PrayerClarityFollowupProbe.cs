using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using BepInEx;
using UnityEngine;

namespace PrayerClarityResearch
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class PrayerClarityFollowupProbe : BaseUnityPlugin
    {
        public const string PluginGuid = "prayerclarity.auditprobe.flowgraph";
        public const string PluginName = "PrayerClarity FlowGraph Audit Probe";
        public const string PluginVersion = "0.1.2";

        private bool _completed;
        private float _gameReadyAt = -1f;
        private static readonly OpCode[] OneByte = new OpCode[256];
        private static readonly OpCode[] TwoByte = new OpCode[256];

        static PrayerClarityFollowupProbe()
        {
            foreach (FieldInfo field in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType != typeof(OpCode)) continue;
                OpCode op = (OpCode)field.GetValue(null);
                ushort value = unchecked((ushort)op.Value);
                if (value < 256) OneByte[value] = op;
                else if ((value & 0xFF00) == 0xFE00) TwoByte[value & 0xFF] = op;
            }
        }

        private void Awake()
        {
            Logger.LogInfo("PrayerClarity FlowGraph Audit Probe 0.1.2 loaded: read-only reflection only; no Harmony, graph execution, or game-state mutation.");
        }

        private void Update()
        {
            if (_completed) return;
            Assembly gameAsm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "Assembly-CSharp");
            if (gameAsm == null || !IsGameStarted(gameAsm)) return;
            if (_gameReadyAt < 0f) { _gameReadyAt = Time.realtimeSinceStartup; return; }
            if (Time.realtimeSinceStartup - _gameReadyAt < 5f) return;

            _completed = true;
            try { RunAudit(gameAsm); }
            catch (Exception ex) { Logger.LogError("PrayerClarity flowgraph audit failed: " + ex); }
        }

        private static bool IsGameStarted(Assembly gameAsm)
        {
            Type mainGame = SafeGetTypes(gameAsm).FirstOrDefault(t => t.Name == "MainGame" || t.FullName == "MainGame");
            if (mainGame == null) return false;
            FieldInfo field = mainGame.GetField("game_started", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (field == null || field.FieldType != typeof(bool)) return false;
            try { return (bool)field.GetValue(null); } catch { return false; }
        }

        private void RunAudit(Assembly gameAsm)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Type[] gameTypes = SafeGetTypes(gameAsm);
            Type[] allTypes = assemblies.SelectMany(SafeGetTypes).ToArray();
            StringBuilder sb = new StringBuilder(512 * 1024);

            sb.AppendLine("PRAYERCLARITY — FLOWGRAPH READ-ONLY PRAYER AUDIT");
            sb.AppendLine("ProbeVersion=" + PluginVersion);
            sb.AppendLine("GeneratedUtc=" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            sb.AppendLine("GameAssembly=" + gameAsm.FullName);
            sb.AppendLine("ModuleVersionId=" + gameAsm.ManifestModule.ModuleVersionId);
            sb.AppendLine("Contract=READ_ONLY_REFLECTION_NO_GRAPH_EXECUTION_NO_MUTATION");
            sb.AppendLine("Questions=pray graph reward/failure wiring; success-only buff/drop branch; gratitude gain arithmetic; buff_sins consumer");
            sb.AppendLine();

            sb.AppendLine("=== TARGET IL ===");
            DumpType(sb, FindType(allTypes, "FlowCanvas.Nodes.Souls.Flow_CalculateGratitudePoints"), null);
            DumpType(sb, FindType(allTypes, "SoulsHelper"), new[] { "CalculatePointsAfterSoulRelease" });
            DumpType(sb, FindType(allTypes, "CustomFlowScript"), new[] { "GetGraph" });
            DumpCallers(sb, allTypes, FindType(allTypes, "SoulsHelper"), "CalculatePointsAfterSoulRelease");
            DumpStringLiteralUsers(sb, allTypes, new[] { "buff_sins", "increase_gp_gain" });
            sb.AppendLine("=== END TARGET IL ===");
            sb.AppendLine();

            DumpPrayGraph(sb, allTypes);
            DumpLoadedGraphKeywordHits(sb, allTypes, new[] { "buff_sins", "increase_gp_gain", "gratitude_points" });

            string path = Path.Combine(Paths.BepInExRootPath, "PrayerClarity-audit-0.1.2.txt");
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
            Logger.LogInfo("PrayerClarity flowgraph audit complete: " + path);
        }

        private static void DumpPrayGraph(StringBuilder sb, Type[] allTypes)
        {
            sb.AppendLine("=== PRAY FLOW GRAPH ===");
            Type customFlow = FindType(allTypes, "CustomFlowScript");
            if (customFlow == null) { sb.AppendLine("CustomFlowScript missing"); sb.AppendLine("=== END PRAY FLOW GRAPH ==="); return; }

            MethodInfo getGraph = customFlow.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "GetGraph" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(string));
            if (getGraph == null) { sb.AppendLine("GetGraph(string) missing"); sb.AppendLine("=== END PRAY FLOW GRAPH ==="); return; }
            sb.AppendLine("GetGraph=" + Signature(getGraph) + " static=" + getGraph.IsStatic);
            if (!getGraph.IsStatic) { sb.AppendLine("Refusing to instantiate/execute CustomFlowScript for read-only audit."); sb.AppendLine("=== END PRAY FLOW GRAPH ==="); return; }

            object graph;
            try { graph = getGraph.Invoke(null, new object[] { "pray" }); }
            catch (Exception ex) { sb.AppendLine("GetGraph(pray) failed=" + ex.GetType().Name + ":" + ex.Message); sb.AppendLine("=== END PRAY FLOW GRAPH ==="); return; }
            if (graph == null) { sb.AppendLine("GetGraph(pray)=null"); sb.AppendLine("=== END PRAY FLOW GRAPH ==="); return; }

            sb.AppendLine("GraphType=" + graph.GetType().FullName);
            sb.AppendLine("GraphName=" + TryUnityName(graph));
            string serialized = GetSerializedGraph(graph);
            sb.AppendLine("SerializedLength=" + (serialized == null ? -1 : serialized.Length));
            if (!string.IsNullOrEmpty(serialized))
            {
                sb.AppendLine("PRAY_GRAPH_SERIALIZED_BEGIN");
                sb.AppendLine(serialized);
                sb.AppendLine("PRAY_GRAPH_SERIALIZED_END");
            }
            DumpNodeSummary(sb, graph);
            sb.AppendLine("=== END PRAY FLOW GRAPH ===");
            sb.AppendLine();
        }

        private static void DumpLoadedGraphKeywordHits(StringBuilder sb, Type[] allTypes, string[] needles)
        {
            sb.AppendLine("=== LOADED FLOW GRAPH KEYWORD HITS ===");
            Type graphType = FindType(allTypes, "NodeCanvas.Framework.Graph");
            if (graphType == null || !typeof(UnityEngine.Object).IsAssignableFrom(graphType))
            {
                sb.AppendLine("NodeCanvas.Framework.Graph unavailable");
                sb.AppendLine("=== END LOADED FLOW GRAPH KEYWORD HITS ===");
                return;
            }

            UnityEngine.Object[] graphs;
            try { graphs = Resources.FindObjectsOfTypeAll(graphType); }
            catch (Exception ex) { sb.AppendLine("FindObjectsOfTypeAll failed=" + ex.GetType().Name); sb.AppendLine("=== END LOADED FLOW GRAPH KEYWORD HITS ==="); return; }

            int graphHits = 0;
            foreach (UnityEngine.Object graph in graphs)
            {
                if (graph == null) continue;
                string serialized = GetSerializedGraph(graph);
                if (string.IsNullOrEmpty(serialized)) continue;
                List<string> hitNeedles = needles.Where(n => serialized.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                if (hitNeedles.Count == 0) continue;
                graphHits++;
                sb.AppendLine("GRAPH_HIT name=" + Quote(graph.name) + " type=" + graph.GetType().FullName + " needles=" + string.Join(",", hitNeedles.ToArray()) + " length=" + serialized.Length);
                foreach (string needle in hitNeedles)
                {
                    int start = 0;
                    int occurrence = 0;
                    while (occurrence < 5)
                    {
                        int index = serialized.IndexOf(needle, start, StringComparison.OrdinalIgnoreCase);
                        if (index < 0) break;
                        occurrence++;
                        int left = Math.Max(0, index - 900);
                        int len = Math.Min(serialized.Length - left, 1800);
                        sb.AppendLine("SNIPPET needle=" + needle + " occurrence=" + occurrence);
                        sb.AppendLine(serialized.Substring(left, len));
                        start = index + needle.Length;
                    }
                }
            }
            sb.AppendLine("LoadedGraphCount=" + graphs.Length + " MatchingGraphCount=" + graphHits);
            sb.AppendLine("=== END LOADED FLOW GRAPH KEYWORD HITS ===");
        }

        private static string GetSerializedGraph(object graph)
        {
            if (graph == null) return null;
            Type t = graph.GetType();
            while (t != null)
            {
                FieldInfo field = t.GetField("_serializedGraph", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (field != null && field.FieldType == typeof(string))
                {
                    try { return field.GetValue(graph) as string; } catch { return null; }
                }
                t = t.BaseType;
            }
            return null;
        }

        private static void DumpNodeSummary(StringBuilder sb, object graph)
        {
            if (graph == null) return;
            PropertyInfo prop = FindProperty(graph.GetType(), "allNodes");
            if (prop == null) { sb.AppendLine("allNodes property missing"); return; }
            IEnumerable nodes;
            try { nodes = prop.GetValue(graph, null) as IEnumerable; } catch { nodes = null; }
            if (nodes == null) { sb.AppendLine("allNodes unavailable"); return; }
            Dictionary<string, int> counts = new Dictionary<string, int>();
            int total = 0;
            foreach (object node in nodes)
            {
                if (node == null) continue;
                total++;
                string name = node.GetType().FullName;
                int count;
                counts.TryGetValue(name, out count);
                counts[name] = count + 1;
            }
            sb.AppendLine("NodeCount=" + total);
            foreach (KeyValuePair<string, int> kv in counts.OrderBy(k => k.Key)) sb.AppendLine("NODETYPE " + kv.Value + " x " + kv.Key);
        }

        private static PropertyInfo FindProperty(Type type, string name)
        {
            while (type != null)
            {
                PropertyInfo prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (prop != null) return prop;
                type = type.BaseType;
            }
            return null;
        }

        private static string TryUnityName(object value)
        {
            UnityEngine.Object obj = value as UnityEngine.Object;
            return obj == null ? "<not UnityEngine.Object>" : obj.name;
        }

        private static void DumpCallers(StringBuilder sb, Type[] allTypes, Type targetType, string targetName)
        {
            if (targetType == null) { sb.AppendLine("CALLERS target type missing: " + targetName); return; }
            MethodBase[] targets = targetType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                .Where(m => m.Name == targetName).Cast<MethodBase>().ToArray();
            foreach (MethodBase target in targets) sb.AppendLine("CALLER_TARGET " + Signature(target));
            foreach (Type type in allTypes)
            {
                MethodBase[] methods = GetDeclaredMethods(type);
                foreach (MethodBase method in methods)
                {
                    List<Instruction> il;
                    try { il = Read(method); } catch { continue; }
                    if (!il.Any(i => targets.Any(t => Same(i.Member as MethodBase, t)))) continue;
                    DumpMethod(sb, method, "CALLS=" + targetType.FullName + "." + targetName);
                }
            }
        }

        private static void DumpStringLiteralUsers(StringBuilder sb, Type[] allTypes, string[] needles)
        {
            foreach (Type type in allTypes)
            {
                foreach (MethodBase method in GetDeclaredMethods(type))
                {
                    List<Instruction> il;
                    try { il = Read(method); } catch { continue; }
                    List<string> hits = il.Where(i => i.StringValue != null && needles.Any(n => string.Equals(i.StringValue, n, StringComparison.OrdinalIgnoreCase)))
                        .Select(i => i.StringValue).Distinct().ToList();
                    if (hits.Count > 0) DumpMethod(sb, method, "STRING_HITS=" + string.Join(",", hits.ToArray()));
                }
            }
        }

        private static MethodBase[] GetDeclaredMethods(Type type)
        {
            if (type == null) return new MethodBase[0];
            const BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            try { return type.GetMethods(f).Cast<MethodBase>().Concat(type.GetConstructors(f).Cast<MethodBase>()).ToArray(); }
            catch { return new MethodBase[0]; }
        }

        private static Type[] SafeGetTypes(Assembly assembly)
        {
            if (assembly == null) return new Type[0];
            try { return assembly.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null).ToArray(); }
            catch { return new Type[0]; }
        }

        private static Type FindType(IEnumerable<Type> types, string fullOrShortName)
        {
            return types.FirstOrDefault(t => t != null && (t.FullName == fullOrShortName || t.Name == fullOrShortName));
        }

        private static void DumpType(StringBuilder sb, Type type, string[] names)
        {
            if (type == null) { sb.AppendLine("MISSING TYPE"); return; }
            sb.AppendLine("TYPE " + type.FullName + " assembly=" + type.Assembly.GetName().Name);
            IEnumerable<MethodBase> methods = GetDeclaredMethods(type);
            if (names != null) methods = methods.Where(m => names.Contains(m.Name));
            foreach (MethodBase method in methods.OrderBy(m => m.MetadataToken)) DumpMethod(sb, method, null);
        }

        private static bool Same(MemberInfo a, MemberInfo b)
        {
            return a != null && b != null && a.Module == b.Module && a.MetadataToken == b.MetadataToken;
        }

        private static void DumpMethod(StringBuilder sb, MethodBase method, string reason)
        {
            sb.AppendLine("METHOD " + Signature(method));
            if (!string.IsNullOrEmpty(reason)) sb.AppendLine(reason);
            MethodBody body;
            try { body = method.GetMethodBody(); } catch { return; }
            if (body == null) { sb.AppendLine("  <no IL>"); return; }
            foreach (Instruction ins in Read(method))
                sb.Append("  IL_").Append(ins.Offset.ToString("X4", CultureInfo.InvariantCulture)).Append(' ').Append(ins.Op.Name).Append(' ').AppendLine(ins.Text ?? "");
        }

        private static string Signature(MethodBase method)
        {
            string owner = method.DeclaringType == null ? "<global>" : method.DeclaringType.FullName;
            string ret = method is MethodInfo ? (((MethodInfo)method).ReturnType.FullName ?? ((MethodInfo)method).ReturnType.Name) : "System.Void";
            string pars = string.Join(", ", method.GetParameters().Select(p => (p.ParameterType.FullName ?? p.ParameterType.Name) + " " + p.Name).ToArray());
            return ret + " " + owner + "." + method.Name + "(" + pars + ") token=0x" + method.MetadataToken.ToString("X8", CultureInfo.InvariantCulture);
        }

        private static List<Instruction> Read(MethodBase method)
        {
            List<Instruction> result = new List<Instruction>();
            MethodBody body = method.GetMethodBody();
            if (body == null) return result;
            byte[] bytes = body.GetILAsByteArray();
            Module module = method.Module;
            Type[] ta = method.DeclaringType != null && method.DeclaringType.IsGenericType ? method.DeclaringType.GetGenericArguments() : null;
            Type[] ma = method.IsGenericMethod ? method.GetGenericArguments() : null;
            int p = 0;
            while (p < bytes.Length)
            {
                int offset = p;
                byte first = bytes[p++];
                OpCode op = first == 0xFE ? TwoByte[bytes[p++]] : OneByte[first];
                string text = "";
                MemberInfo member = null;
                string str = null;
                switch (op.OperandType)
                {
                    case OperandType.InlineNone: break;
                    case OperandType.ShortInlineI: text = ((sbyte)bytes[p]).ToString(CultureInfo.InvariantCulture); p++; break;
                    case OperandType.InlineI: text = BitConverter.ToInt32(bytes, p).ToString(CultureInfo.InvariantCulture); p += 4; break;
                    case OperandType.InlineI8: text = BitConverter.ToInt64(bytes, p).ToString(CultureInfo.InvariantCulture); p += 8; break;
                    case OperandType.ShortInlineR: text = BitConverter.ToSingle(bytes, p).ToString("R", CultureInfo.InvariantCulture); p += 4; break;
                    case OperandType.InlineR: text = BitConverter.ToDouble(bytes, p).ToString("R", CultureInfo.InvariantCulture); p += 8; break;
                    case OperandType.ShortInlineVar: text = bytes[p++].ToString(CultureInfo.InvariantCulture); break;
                    case OperandType.InlineVar: text = BitConverter.ToUInt16(bytes, p).ToString(CultureInfo.InvariantCulture); p += 2; break;
                    case OperandType.ShortInlineBrTarget: { sbyte d = unchecked((sbyte)bytes[p++]); text = "IL_" + (p + d).ToString("X4", CultureInfo.InvariantCulture); break; }
                    case OperandType.InlineBrTarget: { int d = BitConverter.ToInt32(bytes, p); p += 4; text = "IL_" + (p + d).ToString("X4", CultureInfo.InvariantCulture); break; }
                    case OperandType.InlineString:
                    {
                        int token = BitConverter.ToInt32(bytes, p); p += 4;
                        try { str = module.ResolveString(token); text = Quote(str); } catch { text = "string-token:0x" + token.ToString("X8", CultureInfo.InvariantCulture); }
                        break;
                    }
                    case OperandType.InlineField:
                    case OperandType.InlineMethod:
                    case OperandType.InlineType:
                    case OperandType.InlineTok:
                    case OperandType.InlineSig:
                    {
                        int token = BitConverter.ToInt32(bytes, p); p += 4;
                        try
                        {
                            if (op.OperandType == OperandType.InlineField) member = module.ResolveField(token, ta, ma);
                            else if (op.OperandType == OperandType.InlineMethod) member = module.ResolveMethod(token, ta, ma);
                            else if (op.OperandType == OperandType.InlineType) member = module.ResolveType(token, ta, ma);
                            else if (op.OperandType == OperandType.InlineTok) member = module.ResolveMember(token, ta, ma);
                            text = member == null ? "token:0x" + token.ToString("X8", CultureInfo.InvariantCulture) : Resolved(member);
                        }
                        catch { text = "token:0x" + token.ToString("X8", CultureInfo.InvariantCulture); }
                        break;
                    }
                    case OperandType.InlineSwitch:
                    {
                        int count = BitConverter.ToInt32(bytes, p); p += 4;
                        int basePos = p + count * 4;
                        string[] targets = new string[count];
                        for (int i = 0; i < count; i++) { int d = BitConverter.ToInt32(bytes, p); p += 4; targets[i] = "IL_" + (basePos + d).ToString("X4", CultureInfo.InvariantCulture); }
                        text = "[" + string.Join(",", targets) + "]";
                        break;
                    }
                    default: throw new NotSupportedException("operand " + op.OperandType);
                }
                result.Add(new Instruction { Offset = offset, Op = op, Text = text, Member = member, StringValue = str });
            }
            return result;
        }

        private static string Resolved(MemberInfo member)
        {
            FieldInfo f = member as FieldInfo;
            if (f != null) return f.DeclaringType.FullName + "." + f.Name + " : " + (f.FieldType.FullName ?? f.FieldType.Name);
            MethodBase m = member as MethodBase;
            if (m != null) return Signature(m);
            Type t = member as Type;
            return t != null ? t.FullName : member.ToString();
        }

        private static string Quote(string value)
        {
            if (value == null) return "<null>";
            return "\"" + value.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\"") + "\"";
        }

        private sealed class Instruction
        {
            public int Offset;
            public OpCode Op;
            public string Text;
            public MemberInfo Member;
            public string StringValue;
        }
    }
}
