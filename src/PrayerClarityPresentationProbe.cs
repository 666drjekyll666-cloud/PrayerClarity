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
    public sealed class PrayerClarityPresentationProbe : BaseUnityPlugin
    {
        public const string PluginGuid = "prayerclarity.auditprobe.presentation";
        public const string PluginName = "PrayerClarity Presentation Probe";
        public const string PluginVersion = "0.1.6";

        private bool _completed;
        private float _readyAt = -1f;
        private static readonly OpCode[] OneByte = new OpCode[256];
        private static readonly OpCode[] TwoByte = new OpCode[256];

        static PrayerClarityPresentationProbe()
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
            Logger.LogInfo("PrayerClarity Presentation Probe 0.1.6 loaded: read-only IL/runtime metadata/serialized-graph inspection; no Harmony, graph execution, or mutation.");
        }

        private void Update()
        {
            if (_completed) return;
            Assembly game = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "Assembly-CSharp");
            if (game == null || !GameStarted(game)) return;
            if (_readyAt < 0f) { _readyAt = Time.realtimeSinceStartup; return; }
            if (Time.realtimeSinceStartup - _readyAt < 5f) return;
            _completed = true;
            try { Run(game); }
            catch (Exception ex) { Logger.LogError("PrayerClarity presentation audit failed: " + ex); }
        }

        private static bool GameStarted(Assembly game)
        {
            Type t = SafeTypes(game).FirstOrDefault(x => x != null && x.Name == "MainGame");
            FieldInfo f = t == null ? null : t.GetField("game_started", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            try { return f != null && f.FieldType == typeof(bool) && (bool)f.GetValue(null); }
            catch { return false; }
        }

        private void Run(Assembly game)
        {
            Type[] all = AppDomain.CurrentDomain.GetAssemblies().SelectMany(SafeTypes).ToArray();
            StringBuilder sb = new StringBuilder(384 * 1024);
            sb.AppendLine("PRAYERCLARITY — FINAL PRESENTATION / CONFESSIONAL AUDIT");
            sb.AppendLine("ProbeVersion=" + PluginVersion);
            sb.AppendLine("GeneratedUtc=" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            sb.AppendLine("GameAssembly=" + game.FullName);
            sb.AppendLine("ModuleVersionId=" + game.ManifestModule.ModuleVersionId);
            sb.AppendLine("Contract=READ_ONLY_IL_RUNTIME_METADATA_SERIALIZED_GRAPH_NO_EXECUTION_NO_MUTATION");
            sb.AppendLine("Questions=exact active-buff UI/hover content; exact confession_probability/church_budka_roll path and any buff_sins linkage");
            sb.AppendLine();

            DumpUiTypes(sb, game);
            DumpNeedleUsers(sb, game, new[] { "confession_probability", "church_budka_roll", "buff_sins" });
            DumpConfessionalLogic(sb, game);
            DumpMatchingGraphs(sb, all, new[] { "confession_probability", "church_budka_roll", "buff_sins" });

            string path = Path.Combine(Paths.BepInExRootPath, "PrayerClarity-audit-0.1.6.txt");
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
            Logger.LogInfo("PrayerClarity final presentation audit complete: " + path);
        }

        private static void DumpUiTypes(StringBuilder sb, Assembly game)
        {
            sb.AppendLine("=== ACTIVE BUFF UI TYPES ===");
            string[] names = { "BuffsGUI", "BuffIcon", "BuffsBarGUI", "PlayerBuff", "Tooltip", "TooltipBubbleGUI" };
            foreach (string name in names)
            {
                Type t = SafeTypes(game).FirstOrDefault(x => x != null && x.Name == name)
                    ?? AppDomain.CurrentDomain.GetAssemblies().SelectMany(SafeTypes).FirstOrDefault(x => x != null && x.Name == name);
                if (t == null) { sb.AppendLine("TYPE_MISSING " + name); continue; }
                sb.AppendLine("TYPE " + t.FullName + " assembly=" + t.Assembly.GetName().Name);
                foreach (FieldInfo f in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                    sb.AppendLine("  FIELD " + (f.IsStatic ? "static " : "instance ") + (f.FieldType.FullName ?? f.FieldType.Name) + " " + f.Name);
                foreach (PropertyInfo p in t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                    sb.AppendLine("  PROP " + (p.PropertyType.FullName ?? p.PropertyType.Name) + " " + p.Name);
                foreach (MethodInfo m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                    DumpMethod(sb, m);
                sb.AppendLine();
            }
            sb.AppendLine("=== END ACTIVE BUFF UI TYPES ===");
            sb.AppendLine();
        }

        private static void DumpNeedleUsers(StringBuilder sb, Assembly game, string[] needles)
        {
            sb.AppendLine("=== STRING-LITERAL USERS ===");
            sb.AppendLine("Needles=" + string.Join(",", needles));
            int hits = 0;
            foreach (Type t in SafeTypes(game))
            {
                if (t == null) continue;
                MethodBase[] methods;
                try
                {
                    methods = t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                        .Cast<MethodBase>()
                        .Concat(t.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Cast<MethodBase>())
                        .ToArray();
                }
                catch { continue; }
                foreach (MethodBase m in methods)
                {
                    List<Instruction> il;
                    try { il = Read(m); } catch { continue; }
                    if (!il.Any(i => i.Op == OpCodes.Ldstr && needles.Any(n => string.Equals(Unquote(i.Text), n, StringComparison.Ordinal)))) continue;
                    hits++;
                    DumpMethod(sb, m);
                }
            }
            sb.AppendLine("STRING_LITERAL_METHOD_HITS=" + hits);
            sb.AppendLine("=== END STRING-LITERAL USERS ===");
            sb.AppendLine();
        }

        private static void DumpConfessionalLogic(StringBuilder sb, Assembly game)
        {
            sb.AppendLine("=== CONFESSIONAL GAMEBALANCE LOGIC ===");
            Type gbType = SafeTypes(game).FirstOrDefault(x => x != null && x.Name == "GameBalance");
            if (gbType == null) { sb.AppendLine("GameBalance missing"); return; }
            object gb = null;
            try
            {
                PropertyInfo p = gbType.GetProperty("me", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (p != null) gb = p.GetValue(null, null);
                if (gb == null)
                {
                    FieldInfo fi = gbType.GetField("_instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (fi != null) gb = fi.GetValue(null);
                }
            }
            catch { }
            if (gb == null) { sb.AppendLine("GameBalance instance missing"); return; }
            FieldInfo logicsField = gbType.GetField("logics_data", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            IEnumerable logics = logicsField == null ? null : logicsField.GetValue(gb) as IEnumerable;
            if (logics == null) { sb.AppendLine("logics_data unavailable"); return; }
            int found = 0;
            foreach (object row in logics)
            {
                if (row == null) continue;
                string flat = FlattenObject(row, 2);
                if (flat.IndexOf("church_budka_roll", StringComparison.OrdinalIgnoreCase) < 0 &&
                    flat.IndexOf("confession_probability", StringComparison.OrdinalIgnoreCase) < 0 &&
                    flat.IndexOf("buff_sins", StringComparison.OrdinalIgnoreCase) < 0) continue;
                found++;
                sb.AppendLine("LOGIC_HIT " + flat);
            }
            sb.AppendLine("LOGIC_HITS=" + found);
            sb.AppendLine("=== END CONFESSIONAL GAMEBALANCE LOGIC ===");
            sb.AppendLine();
        }

        private static void DumpMatchingGraphs(StringBuilder sb, Type[] all, string[] needles)
        {
            sb.AppendLine("=== MATCHING LOADED FLOW GRAPHS ===");
            Type graphType = all.FirstOrDefault(t => t != null && (t.FullName == "NodeCanvas.Framework.Graph" || t.Name == "Graph"));
            if (graphType == null || !typeof(UnityEngine.Object).IsAssignableFrom(graphType))
            {
                sb.AppendLine("Graph type unavailable");
                return;
            }
            UnityEngine.Object[] graphs;
            try { graphs = Resources.FindObjectsOfTypeAll(graphType); }
            catch (Exception ex) { sb.AppendLine("Graph scan failed=" + ex.GetType().Name); return; }
            int found = 0;
            foreach (UnityEngine.Object graph in graphs)
            {
                if (graph == null) continue;
                string serialized = SerializedGraph(graph);
                bool match = needles.Any(n => (graph.name ?? "").IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0 || (!string.IsNullOrEmpty(serialized) && serialized.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0));
                if (!match) continue;
                found++;
                sb.AppendLine("GRAPH_MATCH name=" + graph.name + " type=" + graph.GetType().FullName + " serializedLength=" + (serialized == null ? -1 : serialized.Length));
                if (!string.IsNullOrEmpty(serialized))
                {
                    sb.AppendLine("FULL_GRAPH_SERIALIZED_BEGIN name=" + graph.name);
                    sb.AppendLine(serialized);
                    sb.AppendLine("FULL_GRAPH_SERIALIZED_END name=" + graph.name);
                }
            }
            sb.AppendLine("MATCHING_GRAPH_COUNT=" + found + " loaded=" + graphs.Length);
            sb.AppendLine("=== END MATCHING LOADED FLOW GRAPHS ===");
        }

        private static string SerializedGraph(object graph)
        {
            Type t = graph.GetType();
            while (t != null)
            {
                FieldInfo f = t.GetField("_serializedGraph", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (f != null && f.FieldType == typeof(string))
                {
                    try { return f.GetValue(graph) as string; } catch { return null; }
                }
                t = t.BaseType;
            }
            return null;
        }

        private static string FlattenObject(object obj, int depth)
        {
            if (obj == null) return "null";
            Type t = obj.GetType();
            if (depth <= 0 || t.IsPrimitive || obj is string || obj is decimal || t.IsEnum) return Convert.ToString(obj, CultureInfo.InvariantCulture) ?? "null";
            if (obj is IEnumerable && !(obj is string))
            {
                List<string> vals = new List<string>();
                int n = 0;
                foreach (object item in (IEnumerable)obj)
                {
                    if (n++ >= 32) { vals.Add("..."); break; }
                    vals.Add(FlattenObject(item, depth - 1));
                }
                return "[" + string.Join(",", vals.ToArray()) + "]";
            }
            List<string> parts = new List<string>();
            foreach (FieldInfo f in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                object v;
                try { v = f.GetValue(obj); } catch { continue; }
                parts.Add(f.Name + "=" + FlattenObject(v, depth - 1));
            }
            return t.Name + "{" + string.Join(";", parts.ToArray()) + "}";
        }

        private static void DumpMethod(StringBuilder sb, MethodBase method)
        {
            sb.AppendLine("METHOD " + Signature(method));
            MethodBody body;
            try { body = method.GetMethodBody(); } catch { return; }
            if (body == null) { sb.AppendLine("  <no IL>"); return; }
            List<Instruction> instructions;
            try { instructions = Read(method); } catch (Exception ex) { sb.AppendLine("  <IL read failed: " + ex.GetType().Name + ">"); return; }
            foreach (Instruction ins in instructions)
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
                        try { text = Quote(module.ResolveString(token)); } catch { text = "string-token:0x" + token.ToString("X8", CultureInfo.InvariantCulture); }
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
                            MemberInfo member = op.OperandType == OperandType.InlineField ? module.ResolveField(token, ta, ma)
                                : op.OperandType == OperandType.InlineMethod ? module.ResolveMethod(token, ta, ma)
                                : op.OperandType == OperandType.InlineType ? module.ResolveType(token, ta, ma)
                                : op.OperandType == OperandType.InlineTok ? module.ResolveMember(token, ta, ma)
                                : null;
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
                result.Add(new Instruction { Offset = offset, Op = op, Text = text });
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

        private static Type[] SafeTypes(Assembly assembly)
        {
            if (assembly == null) return new Type[0];
            try { return assembly.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null).ToArray(); }
            catch { return new Type[0]; }
        }

        private static string Quote(string value)
        {
            if (value == null) return "<null>";
            return "\"" + value.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\"") + "\"";
        }

        private static string Unquote(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length < 2 || value[0] != '"' || value[value.Length - 1] != '"') return value ?? "";
            return value.Substring(1, value.Length - 2).Replace("\\\"", "\"").Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\\\", "\\");
        }

        private sealed class Instruction
        {
            public int Offset;
            public OpCode Op;
            public string Text;
        }
    }
}
