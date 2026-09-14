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
    public sealed class PrayerClarityAuditProbe : BaseUnityPlugin
    {
        public const string PluginGuid = "prayerclarity.auditprobe";
        public const string PluginName = "PrayerClarity Audit Probe";
        public const string PluginVersion = "0.1.0";

        private bool _completed;
        private float _startedAt;
        private static readonly OpCode[] OneByte = new OpCode[256];
        private static readonly OpCode[] TwoByte = new OpCode[256];

        static PrayerClarityAuditProbe()
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
            _startedAt = Time.realtimeSinceStartup;
            Logger.LogInfo("PrayerClarity Audit Probe 0.1.0 loaded: read-only reflection/IL only; no Harmony or game-state mutation.");
        }

        private void Update()
        {
            if (_completed || Time.realtimeSinceStartup - _startedAt < 8f) return;
            Assembly asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "Assembly-CSharp");
            if (asm == null) return;
            _completed = true;
            try { RunAudit(asm); }
            catch (Exception ex) { Logger.LogError("PrayerClarity audit failed: " + ex); }
        }

        private void RunAudit(Assembly asm)
        {
            StringBuilder sb = new StringBuilder(192 * 1024);
            sb.AppendLine("PRAYERCLARITY — TARGETED READ-ONLY PRAYER AUDIT");
            sb.AppendLine("ProbeVersion=" + PluginVersion);
            sb.AppendLine("GeneratedUtc=" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            sb.AppendLine("GameAssembly=" + asm.FullName);
            sb.AppendLine("ModuleVersionId=" + asm.ManifestModule.ModuleVersionId);
            sb.AppendLine("Contract=READ_ONLY_REFLECTION_IL_NO_MUTATION");
            sb.AppendLine("Questions=final PrayResult consumer; prayer buff application/duration; passive buff consumers; exact prayer selection/report presentation");
            sb.AppendLine();

            Type[] types = GetTypesSafe(asm);
            sb.AppendLine("=== TARGET METHODS ===");
            DumpType(sb, FindType(types, "PrayCraftGUI"), new[] { "RedrawTextValues", "OnResourcePickerClosed", "Open", "Redraw" });
            DumpType(sb, FindType(types, "PrayReportGUI"), null);
            DumpType(sb, FindType(types, "PrayLogics"), null);
            DumpType(sb, FindType(types, "BuffsLogics"), new[] { "AddBuff", "RemoveBuff", "GiveBuffIfNotExists" });
            DumpType(sb, FindType(types, "PlayerBuff"), new[] { "GetTimerText" });
            DumpType(sb, FindType(types, "ItemDefinition"), new[] { "GetDescription" });
            sb.AppendLine("=== END TARGET METHODS ===");
            sb.AppendLine();

            DumpCrossReferences(sb, types);
            DumpLocalization(sb, types);

            string path = Path.Combine(Paths.BepInExRootPath, "PrayerClarity-audit-0.1.0.txt");
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
            Logger.LogInfo("PrayerClarity audit complete: " + path);
        }

        private static Type[] GetTypesSafe(Assembly asm)
        {
            try { return asm.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null).ToArray(); }
        }

        private static Type FindType(IEnumerable<Type> types, string name)
        {
            return types.FirstOrDefault(t => t.Name == name || t.FullName == name);
        }

        private static void DumpType(StringBuilder sb, Type type, string[] names)
        {
            if (type == null) { sb.AppendLine("MISSING TYPE"); return; }
            sb.AppendLine("TYPE " + type.FullName);
            const BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            IEnumerable<MethodBase> methods = type.GetMethods(f).Cast<MethodBase>().Concat(type.GetConstructors(f).Cast<MethodBase>());
            if (names != null) methods = methods.Where(m => names.Contains(m.Name));
            foreach (MethodBase method in methods.OrderBy(m => m.MetadataToken)) DumpMethod(sb, method, null);
        }

        private static void DumpCrossReferences(StringBuilder sb, Type[] types)
        {
            sb.AppendLine("=== TARGETED CROSS-REFERENCES ===");
            List<FieldInfo> targets = new List<FieldInfo>();
            AddField(targets, types, "PrayLogics", "last_pray_result");
            AddField(targets, types, "PrayLogics", "_sermon_drops");
            AddField(targets, types, "CraftDefinition", "dur_parameter");
            AddField(targets, types, "CraftDefinition", "buff");
            AddField(targets, types, "BuffDefinition", "craft_q");
            AddField(targets, types, "PrayLogics+PrayResult", "faith");
            AddField(targets, types, "PrayLogics+PrayResult", "faith_bonus");
            AddField(targets, types, "PrayLogics+PrayResult", "money");
            AddField(targets, types, "PrayLogics+PrayResult", "money_bonus");
            AddField(targets, types, "PrayLogics+PrayResult", "success");

            string[] strings = { "buff_sins", "buff_pen", "buff_star", "increase_gp_gain", "preach_params", "btn_try_pray", "btn_pray" };
            Dictionary<string, Hit> hits = new Dictionary<string, Hit>();
            const BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

            foreach (Type type in types)
            {
                MethodBase[] methods;
                try { methods = type.GetMethods(f).Cast<MethodBase>().Concat(type.GetConstructors(f).Cast<MethodBase>()).ToArray(); }
                catch { continue; }

                foreach (MethodBase method in methods)
                {
                    List<Instruction> il;
                    try { il = Read(method); } catch { continue; }
                    List<string> reasons = new List<string>();
                    foreach (Instruction ins in il)
                    {
                        FieldInfo rf = ins.Member as FieldInfo;
                        if (rf != null)
                            foreach (FieldInfo target in targets)
                                if (Same(rf, target)) reasons.Add("field:" + target.DeclaringType.FullName + "." + target.Name);
                        if (ins.StringValue != null && strings.Contains(ins.StringValue)) reasons.Add("string:\"" + ins.StringValue + "\"");
                    }
                    if (reasons.Count == 0) continue;
                    string key = method.Module.ModuleVersionId + ":" + method.MetadataToken.ToString("X8", CultureInfo.InvariantCulture);
                    Hit hit;
                    if (!hits.TryGetValue(key, out hit)) { hit = new Hit { Method = method }; hits.Add(key, hit); }
                    foreach (string reason in reasons.Distinct()) if (!hit.Reasons.Contains(reason)) hit.Reasons.Add(reason);
                }
            }

            foreach (Hit hit in hits.Values.OrderBy(h => h.Method.DeclaringType.FullName).ThenBy(h => h.Method.MetadataToken))
                DumpMethod(sb, hit.Method, "HITS=" + string.Join(" | ", hit.Reasons.ToArray()));
            sb.AppendLine("CrossReferenceMethodCount=" + hits.Count);
            sb.AppendLine("=== END TARGETED CROSS-REFERENCES ===");
            sb.AppendLine();
        }

        private static void AddField(List<FieldInfo> list, Type[] types, string typeName, string fieldName)
        {
            Type type = FindType(types, typeName);
            if (type == null) return;
            FieldInfo field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (field != null) list.Add(field);
        }

        private static bool Same(MemberInfo a, MemberInfo b)
        {
            return a != null && b != null && a.Module == b.Module && a.MetadataToken == b.MetadataToken;
        }

        private static void DumpLocalization(StringBuilder sb, Type[] types)
        {
            sb.AppendLine("=== CURRENT LOCALIZATION ===");
            Type gjl = FindType(types, "GJL");
            if (gjl == null) { sb.AppendLine("GJL missing"); return; }
            MethodInfo[] methods = gjl.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Where(m => m.Name == "L").ToArray();
            foreach (MethodInfo m in methods) sb.AppendLine("GJL_SIGNATURE " + Signature(m));
            foreach (string key in new[] { "preach_params", "btn_try_pray", "btn_pray" })
                sb.AppendLine("LOC " + key + "=" + Quote(TryLocalize(methods, key)));
            sb.AppendLine("=== END CURRENT LOCALIZATION ===");
        }

        private static string TryLocalize(IEnumerable<MethodInfo> methods, string key)
        {
            foreach (MethodInfo method in methods)
            {
                ParameterInfo[] p = method.GetParameters();
                if (p.Length == 0 || p[0].ParameterType != typeof(string)) continue;
                object[] args = new object[p.Length];
                args[0] = key;
                bool ok = true;
                for (int i = 1; i < p.Length; i++)
                {
                    if (p[i].IsOptional) args[i] = Type.Missing;
                    else if (p[i].ParameterType == typeof(string)) args[i] = "";
                    else if (p[i].ParameterType == typeof(object[])) args[i] = new object[0];
                    else { ok = false; break; }
                }
                if (!ok) continue;
                try { object value = method.Invoke(null, args); if (value != null) return Convert.ToString(value, CultureInfo.InvariantCulture); }
                catch { }
            }
            return "<unresolved>";
        }

        private static string Quote(string value)
        {
            if (value == null) return "<null>";
            return "\"" + value.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\"") + "\"";
        }

        private static void DumpMethod(StringBuilder sb, MethodBase method, string reason)
        {
            sb.AppendLine("METHOD " + Signature(method));
            if (!string.IsNullOrEmpty(reason)) sb.AppendLine(reason);
            MethodBody body;
            try { body = method.GetMethodBody(); } catch (Exception ex) { sb.AppendLine("  <body error:" + ex.GetType().Name + ">"); return; }
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
                        int count = BitConverter.ToInt32(bytes, p); p += 4; int basePos = p + count * 4; string[] targets = new string[count];
                        for (int i = 0; i < count; i++) { int d = BitConverter.ToInt32(bytes, p); p += 4; targets[i] = "IL_" + (basePos + d).ToString("X4", CultureInfo.InvariantCulture); }
                        text = "[" + string.Join(",", targets) + "]"; break;
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

        private sealed class Instruction
        {
            public int Offset;
            public OpCode Op;
            public string Text;
            public MemberInfo Member;
            public string StringValue;
        }

        private sealed class Hit
        {
            public MethodBase Method;
            public readonly List<string> Reasons = new List<string>();
        }
    }
}
