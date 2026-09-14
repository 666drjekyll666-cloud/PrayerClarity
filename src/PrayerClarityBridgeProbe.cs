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
    public sealed class PrayerClarityBridgeProbe : BaseUnityPlugin
    {
        public const string PluginGuid = "prayerclarity.auditprobe.bridges";
        public const string PluginName = "PrayerClarity Final Bridge Probe";
        public const string PluginVersion = "0.1.5";

        private bool _completed;
        private float _readyAt = -1f;
        private static readonly OpCode[] OneByte = new OpCode[256];
        private static readonly OpCode[] TwoByte = new OpCode[256];

        static PrayerClarityBridgeProbe()
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
            Logger.LogInfo("PrayerClarity Final Bridge Probe 0.1.5 loaded: read-only IL/serialized-graph inspection; no Harmony, graph execution, or mutation.");
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
            catch (Exception ex) { Logger.LogError("PrayerClarity bridge audit failed: " + ex); }
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
            StringBuilder sb = new StringBuilder(256 * 1024);
            sb.AppendLine("PRAYERCLARITY — FINAL BRIDGE / CONNECTION AUDIT");
            sb.AppendLine("ProbeVersion=" + PluginVersion);
            sb.AppendLine("GeneratedUtc=" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            sb.AppendLine("GameAssembly=" + game.FullName);
            sb.AppendLine("ModuleVersionId=" + game.ManifestModule.ModuleVersionId);
            sb.AppendLine("Contract=READ_ONLY_IL_SERIALIZED_GRAPH_NO_EXECUTION_NO_MUTATION");
            sb.AppendLine("Questions=exact WGOpar getter semantics; exact npc_donkey body_min/body_max to Flow_DropBody connections");
            sb.AppendLine();

            DumpWgoParGetter(sb, game);
            DumpDonkeyGraph(sb, all);

            string path = Path.Combine(Paths.BepInExRootPath, "PrayerClarity-audit-0.1.5.txt");
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
            Logger.LogInfo("PrayerClarity final bridge audit complete: " + path);
        }

        private static void DumpWgoParGetter(StringBuilder sb, Assembly game)
        {
            sb.AppendLine("=== SMARTEXPRESSION WGOPAR GETTER ===");
            Type t = SafeTypes(game).FirstOrDefault(x => x != null && x.Name == "SmartExpression");
            if (t == null) { sb.AppendLine("SmartExpression missing"); return; }
            MethodInfo m = t.GetMethod("<CheckExpressionInit>b__16_0", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (m == null)
            {
                sb.AppendLine("WGOpar lambda missing");
                foreach (MethodInfo candidate in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                    .Where(x => x.Name.IndexOf("CheckExpressionInit", StringComparison.Ordinal) >= 0))
                    sb.AppendLine("CANDIDATE " + Signature(candidate));
                return;
            }
            DumpMethod(sb, m);
            sb.AppendLine("=== END SMARTEXPRESSION WGOPAR GETTER ===");
            sb.AppendLine();
        }

        private static void DumpDonkeyGraph(StringBuilder sb, Type[] all)
        {
            sb.AppendLine("=== FULL LOADED NPC_DONKEY GRAPH ===");
            Type graphType = all.FirstOrDefault(t => t != null && (t.FullName == "NodeCanvas.Framework.Graph" || t.Name == "Graph"));
            if (graphType == null || !typeof(UnityEngine.Object).IsAssignableFrom(graphType))
            {
                sb.AppendLine("Graph type unavailable");
                return;
            }
            UnityEngine.Object[] graphs;
            try { graphs = Resources.FindObjectsOfTypeAll(graphType); }
            catch (Exception ex) { sb.AppendLine("Graph scan failed=" + ex.GetType().Name); return; }

            UnityEngine.Object donkey = graphs.FirstOrDefault(g => g != null && string.Equals(g.name, "npc_donkey", StringComparison.OrdinalIgnoreCase));
            if (donkey == null)
            {
                sb.AppendLine("npc_donkey graph not loaded; loaded=" + graphs.Length);
                return;
            }
            string serialized = SerializedGraph(donkey);
            sb.AppendLine("GraphType=" + donkey.GetType().FullName);
            sb.AppendLine("SerializedLength=" + (serialized == null ? -1 : serialized.Length));
            if (!string.IsNullOrEmpty(serialized))
            {
                sb.AppendLine("FULL_GRAPH_SERIALIZED_BEGIN");
                sb.AppendLine(serialized);
                sb.AppendLine("FULL_GRAPH_SERIALIZED_END");
            }
            sb.AppendLine("=== END FULL LOADED NPC_DONKEY GRAPH ===");
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

        private static void DumpMethod(StringBuilder sb, MethodBase method)
        {
            sb.AppendLine("METHOD " + Signature(method));
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

        private sealed class Instruction
        {
            public int Offset;
            public OpCode Op;
            public string Text;
        }
    }
}
