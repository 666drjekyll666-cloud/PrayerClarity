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
    public sealed class PrayerClarityConsumerProbe : BaseUnityPlugin
    {
        public const string PluginGuid = "prayerclarity.auditprobe.consumers";
        public const string PluginName = "PrayerClarity Consumer Audit Probe";
        public const string PluginVersion = "0.1.4";

        private bool _completed;
        private float _gameReadyAt = -1f;
        private static readonly OpCode[] OneByte = new OpCode[256];
        private static readonly OpCode[] TwoByte = new OpCode[256];

        static PrayerClarityConsumerProbe()
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
            Logger.LogInfo("PrayerClarity Consumer Audit Probe 0.1.4 loaded: read-only reflection/IL/serialized-graph inspection only; no Harmony, graph execution, or game-state mutation.");
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
            catch (Exception ex) { Logger.LogError("PrayerClarity consumer audit failed: " + ex); }
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
            Type[] allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(SafeGetTypes).ToArray();
            StringBuilder sb = new StringBuilder(1024 * 1024);

            sb.AppendLine("PRAYERCLARITY — TARGETED CONSUMER / ROUNDING AUDIT");
            sb.AppendLine("ProbeVersion=" + PluginVersion);
            sb.AppendLine("GeneratedUtc=" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            sb.AppendLine("GameAssembly=" + gameAsm.FullName);
            sb.AppendLine("ModuleVersionId=" + gameAsm.ManifestModule.ModuleVersionId);
            sb.AppendLine("Contract=READ_ONLY_REFLECTION_IL_SERIALIZED_GRAPH_NO_EXECUTION_NO_MUTATION");
            sb.AppendLine("Questions=buff_plant end-to-end consumer/scope; body_max live corpse-generation consumer; increase_gp_gain exact loaded soul_portal graph/rounding");
            sb.AppendLine();

            DumpPlantEvidence(sb, allTypes);
            DumpBodyEvidence(sb, allTypes);
            DumpGratitudeEvidence(sb, allTypes);

            string path = Path.Combine(Paths.BepInExRootPath, "PrayerClarity-audit-0.1.4.txt");
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
            Logger.LogInfo("PrayerClarity consumer audit complete: " + path);
        }

        private static void DumpPlantEvidence(StringBuilder sb, Type[] allTypes)
        {
            sb.AppendLine("=== BUFF_PLANT END-TO-END AUDIT ===");
            DumpStringLiteralUsers(sb, allTypes, new[] { "buff_plant", "WGOpar" }, true);
            DumpLoadedGraphKeywordHits(sb, allTypes, "buff_plant", false);
            DumpGameBalanceKeywordHits(sb, allTypes, "buff_plant");
            sb.AppendLine("=== END BUFF_PLANT END-TO-END AUDIT ===");
            sb.AppendLine();
        }

        private static void DumpBodyEvidence(StringBuilder sb, Type[] allTypes)
        {
            sb.AppendLine("=== BODY_MAX / BUFF_SKULL LIVE-CONSUMER AUDIT ===");
            DumpStringLiteralUsers(sb, allTypes, new[] { "body_max", "body_min", "cur_bodies_count", "buff_skull" }, false);
            DumpLoadedGraphKeywordHits(sb, allTypes, "body_max", false);
            DumpLoadedGraphKeywordHits(sb, allTypes, "body_min", false);
            DumpLoadedGraphKeywordHits(sb, allTypes, "buff_skull", false);
            DumpGameBalanceKeywordHits(sb, allTypes, "body_max");
            DumpGameBalanceKeywordHits(sb, allTypes, "body_min");
            DumpGameBalanceKeywordHits(sb, allTypes, "buff_skull");
            sb.AppendLine("=== END BODY_MAX / BUFF_SKULL LIVE-CONSUMER AUDIT ===");
            sb.AppendLine();
        }

        private static void DumpGratitudeEvidence(StringBuilder sb, Type[] allTypes)
        {
            sb.AppendLine("=== SOUL GRATITUDE +10% FULL LOADED-GRAPH AUDIT ===");
            DumpStringLiteralUsers(sb, allTypes, new[] { "increase_gp_gain", "gratitude_points" }, false);
            DumpLoadedGraphKeywordHits(sb, allTypes, "increase_gp_gain", true);
            DumpGameBalanceKeywordHits(sb, allTypes, "increase_gp_gain");
            sb.AppendLine("=== END SOUL GRATITUDE +10% FULL LOADED-GRAPH AUDIT ===");
            sb.AppendLine();
        }

        private static void DumpLoadedGraphKeywordHits(StringBuilder sb, Type[] allTypes, string needle, bool fullGraph)
        {
            Type graphType = FindType(allTypes, "NodeCanvas.Framework.Graph");
            if (graphType == null || !typeof(UnityEngine.Object).IsAssignableFrom(graphType))
            {
                sb.AppendLine("FLOWGRAPH_SCAN needle=" + Quote(needle) + " unavailable");
                return;
            }

            UnityEngine.Object[] graphs;
            try { graphs = Resources.FindObjectsOfTypeAll(graphType); }
            catch (Exception ex) { sb.AppendLine("FLOWGRAPH_SCAN needle=" + Quote(needle) + " failed=" + ex.GetType().Name); return; }

            int hits = 0;
            foreach (UnityEngine.Object graph in graphs)
            {
                if (graph == null) continue;
                string serialized = GetSerializedGraph(graph);
                if (string.IsNullOrEmpty(serialized) || serialized.IndexOf(needle, StringComparison.OrdinalIgnoreCase) < 0) continue;
                hits++;
                sb.AppendLine("FLOWGRAPH_HIT needle=" + Quote(needle) + " name=" + Quote(graph.name) + " type=" + graph.GetType().FullName + " length=" + serialized.Length);
                if (fullGraph)
                {
                    sb.AppendLine("FULL_GRAPH_SERIALIZED_BEGIN name=" + Quote(graph.name));
                    sb.AppendLine(serialized);
                    sb.AppendLine("FULL_GRAPH_SERIALIZED_END name=" + Quote(graph.name));
                }
                else
                {
                    int start = 0;
                    int occurrence = 0;
                    while (occurrence < 12)
                    {
                        int index = serialized.IndexOf(needle, start, StringComparison.OrdinalIgnoreCase);
                        if (index < 0) break;
                        occurrence++;
                        int left = Math.Max(0, index - 1400);
                        int len = Math.Min(serialized.Length - left, 2800);
                        sb.AppendLine("FLOWGRAPH_SNIPPET needle=" + Quote(needle) + " occurrence=" + occurrence);
                        sb.AppendLine(serialized.Substring(left, len));
                        start = index + needle.Length;
                    }
                }
            }
            sb.AppendLine("FLOWGRAPH_SCAN needle=" + Quote(needle) + " loaded=" + graphs.Length + " hits=" + hits + " full=" + fullGraph);
        }

        private static void DumpGameBalanceKeywordHits(StringBuilder sb, Type[] allTypes, string needle)
        {
            Type gameBalanceType = FindType(allTypes, "GameBalance");
            Type smartType = FindType(allTypes, "SmartExpression");
            if (gameBalanceType == null)
            {
                sb.AppendLine("BALANCE_SCAN needle=" + Quote(needle) + " GameBalance missing");
                return;
            }

            object gameBalance = null;
            try
            {
                PropertyInfo me = gameBalanceType.GetProperty("me", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (me != null) gameBalance = me.GetValue(null, null);
            }
            catch { }
            if (gameBalance == null)
            {
                sb.AppendLine("BALANCE_SCAN needle=" + Quote(needle) + " GameBalance.me unavailable");
                return;
            }

            MethodInfo rawExpression = smartType == null ? null : smartType.GetMethod("GetRawExpressionString", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            int hits = 0;
            const BindingFlags fields = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (FieldInfo collectionField in gameBalanceType.GetFields(fields).OrderBy(f => f.Name))
            {
                object collection;
                try { collection = collectionField.GetValue(gameBalance); } catch { continue; }
                IEnumerable enumerable = collection as IEnumerable;
                if (enumerable == null || collection is string) continue;

                int rowIndex = 0;
                foreach (object row in enumerable)
                {
                    rowIndex++;
                    if (row == null) continue;
                    string rowId = TryGetId(row);
                    ScanObjectFields(sb, row, collectionField.Name + "[" + rowIndex + "] id=" + Quote(rowId), needle, smartType, rawExpression, ref hits, 0);
                }
            }
            sb.AppendLine("BALANCE_SCAN needle=" + Quote(needle) + " hits=" + hits);
        }

        private static void ScanObjectFields(StringBuilder sb, object obj, string path, string needle, Type smartType, MethodInfo rawExpression, ref int hits, int depth)
        {
            if (obj == null || depth > 2) return;
            Type type = obj.GetType();
            const BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (FieldInfo field in type.GetFields(f))
            {
                object value;
                try { value = field.GetValue(obj); } catch { continue; }
                if (value == null) continue;
                string fieldPath = path + "." + field.Name;

                string s = value as string;
                if (s != null)
                {
                    if (s.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        sb.AppendLine("BALANCE_HIT needle=" + Quote(needle) + " path=" + fieldPath + " type=string value=" + Quote(s));
                        hits++;
                    }
                    continue;
                }

                if (smartType != null && smartType.IsInstanceOfType(value))
                {
                    string raw = "";
                    try { if (rawExpression != null) raw = Convert.ToString(rawExpression.Invoke(value, null), CultureInfo.InvariantCulture) ?? ""; } catch { }
                    if (raw.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        sb.AppendLine("BALANCE_HIT needle=" + Quote(needle) + " path=" + fieldPath + " type=SmartExpression raw=" + Quote(raw));
                        hits++;
                    }
                    continue;
                }

                IEnumerable nested = value as IEnumerable;
                if (nested != null && !(value is string) && depth < 2)
                {
                    int i = 0;
                    foreach (object child in nested)
                    {
                        i++;
                        if (child == null) continue;
                        string childString = child as string;
                        if (childString != null)
                        {
                            if (childString.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                sb.AppendLine("BALANCE_HIT needle=" + Quote(needle) + " path=" + fieldPath + "[" + i + "] type=string value=" + Quote(childString));
                                hits++;
                            }
                        }
                        else if (smartType != null && smartType.IsInstanceOfType(child))
                        {
                            string raw = "";
                            try { if (rawExpression != null) raw = Convert.ToString(rawExpression.Invoke(child, null), CultureInfo.InvariantCulture) ?? ""; } catch { }
                            if (raw.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                sb.AppendLine("BALANCE_HIT needle=" + Quote(needle) + " path=" + fieldPath + "[" + i + "] type=SmartExpression raw=" + Quote(raw));
                                hits++;
                            }
                        }
                        else if (child.GetType().Assembly == type.Assembly)
                        {
                            ScanObjectFields(sb, child, fieldPath + "[" + i + "]", needle, smartType, rawExpression, ref hits, depth + 1);
                        }
                    }
                }
            }
        }

        private static void DumpStringLiteralUsers(StringBuilder sb, Type[] allTypes, string[] needles, bool substring)
        {
            int hitsCount = 0;
            Assembly own = typeof(PrayerClarityConsumerProbe).Assembly;
            foreach (Type type in allTypes)
            {
                if (type == null || type.Assembly == own) continue;
                foreach (MethodBase method in GetDeclaredMethods(type))
                {
                    List<Instruction> il;
                    try { il = Read(method); } catch { continue; }
                    List<string> hits = il.Where(i => i.StringValue != null && needles.Any(n => substring
                            ? i.StringValue.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0
                            : string.Equals(i.StringValue, n, StringComparison.OrdinalIgnoreCase)))
                        .Select(i => i.StringValue).Distinct().ToList();
                    if (hits.Count == 0) continue;
                    hitsCount++;
                    DumpMethod(sb, method, "STRING_HITS=" + string.Join(" | ", hits.Select(Quote).ToArray()));
                }
            }
            sb.AppendLine("STRING_LITERAL_METHOD_HITS needles=" + string.Join(",", needles) + " substring=" + substring + " count=" + hitsCount);
        }

        private static string TryGetId(object row)
        {
            if (row == null) return "";
            Type type = row.GetType();
            FieldInfo field = type.GetField("id", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                try { return Convert.ToString(field.GetValue(row), CultureInfo.InvariantCulture) ?? ""; } catch { }
            }
            PropertyInfo prop = type.GetProperty("id", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop != null && prop.GetIndexParameters().Length == 0)
            {
                try { return Convert.ToString(prop.GetValue(row, null), CultureInfo.InvariantCulture) ?? ""; } catch { }
            }
            return "";
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
