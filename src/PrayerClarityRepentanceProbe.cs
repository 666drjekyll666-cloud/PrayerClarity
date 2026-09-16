using System;
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
    public sealed class PrayerClarityRepentanceProbe : BaseUnityPlugin
    {
        public const string PluginGuid = "prayerclarity.research.repentance";
        public const string PluginName = "PrayerClarity Repentance Research Probe";
        public const string PluginVersion = "0.1.0";

        private bool _completed;
        private float _readyAt = -1f;
        private static readonly OpCode[] OneByte = new OpCode[256];
        private static readonly OpCode[] TwoByte = new OpCode[256];

        static PrayerClarityRepentanceProbe()
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
            Logger.LogInfo("PrayerClarity Repentance Research Probe 0.1.0 loaded: read-only graph/IL inspection; no Harmony and no intentional game/save mutation.");
        }

        private void Update()
        {
            if (_completed) return;

            Assembly game = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "Assembly-CSharp");
            if (game == null || !GameStarted(game)) return;

            if (_readyAt < 0f)
            {
                _readyAt = Time.realtimeSinceStartup;
                return;
            }

            if (Time.realtimeSinceStartup - _readyAt < 5f) return;

            _completed = true;
            try
            {
                Run(game);
            }
            catch (Exception ex)
            {
                Logger.LogError("PrayerClarity Repentance probe failed: " + ex);
            }
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
            Type[] allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(SafeTypes).ToArray();
            StringBuilder sb = new StringBuilder(512 * 1024);

            sb.AppendLine("PRAYERCLARITY — REPENTANCE / CONFESSIONAL NARROW RESEARCH AUDIT");
            sb.AppendLine("ProbeVersion=" + PluginVersion);
            sb.AppendLine("GeneratedUtc=" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            sb.AppendLine("GameAssembly=" + game.FullName);
            sb.AppendLine("ModuleVersionId=" + game.ManifestModule.ModuleVersionId);
            sb.AppendLine("Contract=READ_ONLY_GRAPH_AND_IL_INSPECTION_NO_HARMONY_NO_MUTATION");
            sb.AppendLine("Question=What exactly does church_budka_roll do, how is it invoked, and what stock seam should Repentance rework target?");
            sb.AppendLine();

            DumpGraph(sb, allTypes, "church_budka_roll");
            DumpStringLiteralUsers(sb, allTypes, new[] { "church_budka_roll", "buff_sins" });

            string path = Path.Combine(Paths.BepInExRootPath, "PrayerClarity-repentance-audit-0.1.0.txt");
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
            Logger.LogInfo("PrayerClarity Repentance research audit complete: " + path);
        }

        private static void DumpGraph(StringBuilder sb, Type[] allTypes, string graphName)
        {
            sb.AppendLine("=== FULL FLOW GRAPH: " + graphName + " ===");
            Type customFlow = allTypes.FirstOrDefault(t => t != null && t.Name == "CustomFlowScript");
            if (customFlow == null)
            {
                sb.AppendLine("CustomFlowScript missing");
                sb.AppendLine("=== END FULL FLOW GRAPH ===");
                return;
            }

            MethodInfo getGraph = customFlow.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "GetGraph" && m.IsStatic && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(string));
            if (getGraph == null)
            {
                sb.AppendLine("Static GetGraph(string) unavailable");
                sb.AppendLine("=== END FULL FLOW GRAPH ===");
                return;
            }

            object graph = null;
            try { graph = getGraph.Invoke(null, new object[] { graphName }); }
            catch (Exception ex) { sb.AppendLine("GetGraph failed=" + ex.GetType().Name + ":" + ex.Message); }

            if (graph == null)
            {
                sb.AppendLine("Graph=null");
                sb.AppendLine("=== END FULL FLOW GRAPH ===");
                return;
            }

            string serialized = GetSerializedGraph(graph);
            sb.AppendLine("GraphType=" + graph.GetType().FullName);
            sb.AppendLine("GraphName=" + TryUnityName(graph));
            sb.AppendLine("SerializedLength=" + (serialized == null ? -1 : serialized.Length));
            if (!string.IsNullOrEmpty(serialized))
            {
                sb.AppendLine("GRAPH_SERIALIZED_BEGIN");
                sb.AppendLine(serialized);
                sb.AppendLine("GRAPH_SERIALIZED_END");
            }

            sb.AppendLine("=== END FULL FLOW GRAPH ===");
            sb.AppendLine();
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
                    try { return field.GetValue(graph) as string; }
                    catch { return null; }
                }
                t = t.BaseType;
            }
            return null;
        }

        private static string TryUnityName(object value)
        {
            UnityEngine.Object obj = value as UnityEngine.Object;
            return obj == null ? "<not UnityEngine.Object>" : obj.name;
        }

        private static void DumpStringLiteralUsers(StringBuilder sb, Type[] allTypes, string[] needles)
        {
            sb.AppendLine("=== STRING-LITERAL USERS ===");
            Assembly own = typeof(PrayerClarityRepentanceProbe).Assembly;
            int hits = 0;

            foreach (Type type in allTypes)
            {
                if (type == null || type.Assembly == own) continue;
                foreach (MethodBase method in GetDeclaredMethods(type))
                {
                    List<Instruction> il;
                    try { il = Read(method); }
                    catch { continue; }

                    string[] found = il.Where(i => i.StringValue != null && needles.Any(n => i.StringValue.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0))
                        .Select(i => i.StringValue)
                        .Distinct()
                        .ToArray();
                    if (found.Length == 0) continue;

                    hits++;
                    sb.AppendLine("METHOD " + Signature(method));
                    sb.AppendLine("STRING_HITS=" + string.Join(" | ", found.Select(Quote).ToArray()));
                    foreach (Instruction ins in il)
                    {
                        sb.Append("  IL_").Append(ins.Offset.ToString("X4", CultureInfo.InvariantCulture)).Append(' ')
                            .Append(ins.Op.Name).Append(' ').AppendLine(ins.Text ?? "");
                    }
                }
            }

            sb.AppendLine("METHOD_HIT_COUNT=" + hits);
            sb.AppendLine("=== END STRING-LITERAL USERS ===");
        }

        private static MethodBase[] GetDeclaredMethods(Type type)
        {
            const BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            try
            {
                return type.GetMethods(f).Cast<MethodBase>().Concat(type.GetConstructors(f).Cast<MethodBase>()).ToArray();
            }
            catch
            {
                return new MethodBase[0];
            }
        }

        private static Type[] SafeTypes(Assembly assembly)
        {
            if (assembly == null) return new Type[0];
            try { return assembly.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null).ToArray(); }
            catch { return new Type[0]; }
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
                string stringValue = null;

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
                    case OperandType.ShortInlineBrTarget:
                    {
                        sbyte d = unchecked((sbyte)bytes[p++]);
                        text = "IL_" + (p + d).ToString("X4", CultureInfo.InvariantCulture);
                        break;
                    }
                    case OperandType.InlineBrTarget:
                    {
                        int d = BitConverter.ToInt32(bytes, p); p += 4;
                        text = "IL_" + (p + d).ToString("X4", CultureInfo.InvariantCulture);
                        break;
                    }
                    case OperandType.InlineString:
                    {
                        int token = BitConverter.ToInt32(bytes, p); p += 4;
                        try
                        {
                            stringValue = module.ResolveString(token);
                            text = Quote(stringValue);
                        }
                        catch { text = "string-token:0x" + token.ToString("X8", CultureInfo.InvariantCulture); }
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
                            text = member == null ? "token:0x" + token.ToString("X8", CultureInfo.InvariantCulture) : member.ToString();
                        }
                        catch { text = "token:0x" + token.ToString("X8", CultureInfo.InvariantCulture); }
                        break;
                    }
                    case OperandType.InlineSwitch:
                    {
                        int count = BitConverter.ToInt32(bytes, p); p += 4;
                        int basePos = p + count * 4;
                        string[] targets = new string[count];
                        for (int i = 0; i < count; i++)
                        {
                            int d = BitConverter.ToInt32(bytes, p); p += 4;
                            targets[i] = "IL_" + (basePos + d).ToString("X4", CultureInfo.InvariantCulture);
                        }
                        text = "[" + string.Join(",", targets) + "]";
                        break;
                    }
                    default:
                        throw new NotSupportedException("operand " + op.OperandType);
                }

                result.Add(new Instruction { Offset = offset, Op = op, Text = text, StringValue = stringValue });
            }

            return result;
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
            public string StringValue;
        }
    }
}
