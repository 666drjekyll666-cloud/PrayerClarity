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
    public sealed class PrayerClarityBuffTimerProbe : BaseUnityPlugin
    {
        public const string PluginGuid = "prayerclarity.auditprobe.bufftimer";
        public const string PluginName = "PrayerClarity Buff Timer Probe";
        public const string PluginVersion = "0.1.8";

        private static readonly string[] PrayerBuffIds =
        {
            "buff_plant", "buff_sins", "buff_skull", "buff_sword", "buff_shield",
            "buff_pen", "buff_star", "buff_gp_increase", "buff_sin_shard"
        };

        private static readonly OpCode[] OneByte = new OpCode[256];
        private static readonly OpCode[] TwoByte = new OpCode[256];

        private Assembly _game;
        private float _startedAt;
        private float _sampleAt = -1f;
        private object _sampleBuff;
        private string _sampleBuffId;
        private string _sampleA;
        private bool _completed;

        static PrayerClarityBuffTimerProbe()
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
            Logger.LogInfo("PrayerClarity Buff Timer Probe 0.1.8 loaded: read-only timer-state/IL audit; no Harmony and no game-state mutation. Waiting for an active prayer buff.");
        }

        private void Update()
        {
            if (_completed) return;
            if (_game == null)
            {
                _game = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "Assembly-CSharp");
                if (_game == null) return;
            }
            if (!GameStarted(_game)) return;

            if (_sampleBuff == null)
            {
                _sampleBuff = FindActivePrayerBuff(_game, out _sampleBuffId);
                if (_sampleBuff != null)
                {
                    _sampleA = DumpLiveBuffSnapshot(_sampleBuff, "A");
                    _sampleAt = Time.realtimeSinceStartup;
                    Logger.LogInfo("PrayerClarity Buff Timer Probe captured sample A for " + _sampleBuffId + "; waiting ~2 real seconds for sample B.");
                    return;
                }

                if (Time.realtimeSinceStartup - _startedAt < 120f) return;
                Complete(null, null, "No active recognized prayer buff appeared within 120 real seconds.");
                return;
            }

            if (Time.realtimeSinceStartup - _sampleAt < 2f) return;
            string sampleB = DumpLiveBuffSnapshot(_sampleBuff, "B");
            Complete(_sampleA, sampleB, null);
        }

        private void Complete(string sampleA, string sampleB, string note)
        {
            _completed = true;
            try
            {
                StringBuilder sb = new StringBuilder(128 * 1024);
                sb.AppendLine("PRAYERCLARITY — PRAYER BUFF TIMER AUDIT");
                sb.AppendLine("ProbeVersion=" + PluginVersion);
                sb.AppendLine("GeneratedUtc=" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
                sb.AppendLine("GameAssembly=" + _game.FullName);
                sb.AppendLine("ModuleVersionId=" + _game.ManifestModule.ModuleVersionId);
                sb.AppendLine("Contract=READ_ONLY_REFLECTION_IL_LIVE_STATE_NO_HARMONY_NO_MUTATION");
                sb.AppendLine("Question=exact numeric remaining-time state used by PlayerBuff.GetTimerText; effective day-length seam compatible with vanilla/LongerDays");
                if (!string.IsNullOrEmpty(note)) sb.AppendLine("NOTE=" + note);
                sb.AppendLine();

                DumpType(sb, _game, "PlayerBuff", new[] { ".ctor", "GetTimerText", "Update", "Redraw" });
                DumpType(sb, _game, "BuffsLogics", new[] { "AddBuff" });
                DumpType(sb, _game, "TimeOfDay", new[] { "FromTimeKToSeconds", "FromSecondsToTimeK" });
                DumpEffectiveDayLength(sb, _game);

                sb.AppendLine("=== LIVE PRAYER BUFF SAMPLE ===");
                sb.AppendLine("SelectedBuffId=" + (_sampleBuffId ?? "<none>"));
                if (sampleA != null) sb.Append(sampleA);
                if (sampleB != null) sb.Append(sampleB);
                sb.AppendLine("=== END LIVE PRAYER BUFF SAMPLE ===");

                string path = Path.Combine(Paths.BepInExRootPath, "PrayerClarity-buff-timer-0.1.8.txt");
                File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
                Logger.LogInfo("PrayerClarity buff-timer audit complete: " + path);
            }
            catch (Exception ex)
            {
                Logger.LogError("PrayerClarity buff-timer audit failed: " + ex);
            }
        }

        private static void DumpEffectiveDayLength(StringBuilder sb, Assembly game)
        {
            sb.AppendLine("=== EFFECTIVE DAY LENGTH ===");
            Type type = FindType(game, "TimeOfDay");
            MethodInfo method = type == null ? null : type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "FromTimeKToSeconds" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(float));
            if (method == null)
            {
                sb.AppendLine("FromTimeKToSeconds(float)=MISSING");
            }
            else
            {
                sb.AppendLine("Signature=" + Signature(method));
                sb.AppendLine("IsStatic=" + method.IsStatic);
                if (method.IsStatic)
                {
                    try
                    {
                        object value = method.Invoke(null, new object[] { 1f });
                        sb.AppendLine("FromTimeKToSeconds(1.0)=" + Convert.ToString(value, CultureInfo.InvariantCulture));
                    }
                    catch (Exception ex) { sb.AppendLine("InvokeFailed=" + ex.GetType().Name + ": " + ex.Message); }
                }
            }
            sb.AppendLine("=== END EFFECTIVE DAY LENGTH ===");
            sb.AppendLine();
        }

        private static object FindActivePrayerBuff(Assembly game, out string buffId)
        {
            buffId = null;
            object save = ResolveSave(game);
            IEnumerable buffs = Get(save, "buffs") as IEnumerable;
            if (buffs == null) return null;

            foreach (object buff in buffs)
            {
                if (buff == null) continue;
                string id = ResolveBuffId(buff);
                if (string.IsNullOrEmpty(id)) continue;
                if (!PrayerBuffIds.Contains(id, StringComparer.Ordinal)) continue;
                buffId = id;
                return buff;
            }
            return null;
        }

        private static object ResolveSave(Assembly game)
        {
            Type mainGame = FindType(game, "MainGame");
            if (mainGame == null) return null;
            object me = GetStatic(mainGame, "me");
            return Get(me, "save");
        }

        private static string ResolveBuffId(object buff)
        {
            object definition = Get(buff, "definition");
            string id = Get(definition, "id") as string;
            if (!string.IsNullOrEmpty(id)) return id;
            id = Get(buff, "id") as string;
            if (!string.IsNullOrEmpty(id)) return id;
            return null;
        }

        private static string DumpLiveBuffSnapshot(object buff, string label)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--- SAMPLE " + label + " ---");
            sb.AppendLine("RealtimeSinceStartup=" + Time.realtimeSinceStartup.ToString("R", CultureInfo.InvariantCulture));
            sb.AppendLine("BuffType=" + buff.GetType().FullName);
            sb.AppendLine("BuffId=" + (ResolveBuffId(buff) ?? "<unknown>"));

            MethodInfo timer = buff.GetType().GetMethod("GetTimerText", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
            if (timer != null)
            {
                try { sb.AppendLine("GetTimerText=" + Convert.ToString(timer.Invoke(buff, null), CultureInfo.InvariantCulture)); }
                catch (Exception ex) { sb.AppendLine("GetTimerTextFailed=" + ex.GetType().Name + ": " + ex.Message); }
            }

            foreach (FieldInfo field in AllFields(buff.GetType()))
            {
                if (field.IsStatic) continue;
                object value;
                try { value = field.GetValue(buff); }
                catch { continue; }
                if (!IsSimple(value)) continue;
                sb.AppendLine("FIELD " + field.DeclaringType.FullName + "." + field.Name + "=" + FormatSimple(value));
            }

            foreach (PropertyInfo property in buff.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (!property.CanRead || property.GetIndexParameters().Length != 0) continue;
                if (!IsSimpleType(property.PropertyType)) continue;
                object value;
                try { value = property.GetValue(buff, null); }
                catch { continue; }
                sb.AppendLine("PROP " + property.Name + "=" + FormatSimple(value));
            }

            sb.AppendLine("--- END SAMPLE " + label + " ---");
            return sb.ToString();
        }

        private static IEnumerable<FieldInfo> AllFields(Type type)
        {
            for (Type t = type; t != null; t = t.BaseType)
                foreach (FieldInfo field in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                    yield return field;
        }

        private static bool IsSimple(object value)
        {
            if (value == null) return true;
            return IsSimpleType(value.GetType());
        }

        private static bool IsSimpleType(Type type)
        {
            if (type == null) return false;
            Type t = Nullable.GetUnderlyingType(type) ?? type;
            return t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal);
        }

        private static string FormatSimple(object value)
        {
            if (value == null) return "<null>";
            IFormattable f = value as IFormattable;
            return f != null ? f.ToString(null, CultureInfo.InvariantCulture) : value.ToString();
        }

        private static bool GameStarted(Assembly game)
        {
            Type mainGame = FindType(game, "MainGame");
            FieldInfo field = mainGame == null ? null : mainGame.GetField("game_started", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            try { return field != null && field.FieldType == typeof(bool) && (bool)field.GetValue(null); }
            catch { return false; }
        }

        private static object Get(object obj, string name)
        {
            if (obj == null) return null;
            for (Type type = obj.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null) return field.GetValue(obj);
                PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (property != null && property.CanRead && property.GetIndexParameters().Length == 0) return property.GetValue(obj, null);
            }
            return null;
        }

        private static object GetStatic(Type type, string name)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                FieldInfo field = current.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (field != null) return field.GetValue(null);
                PropertyInfo property = current.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (property != null && property.CanRead && property.GetIndexParameters().Length == 0) return property.GetValue(null, null);
            }
            return null;
        }

        private static Type FindType(Assembly game, string name)
        {
            try { return game.GetTypes().FirstOrDefault(t => t != null && (t.Name == name || t.FullName == name)); }
            catch (ReflectionTypeLoadException ex) { return ex.Types.FirstOrDefault(t => t != null && (t.Name == name || t.FullName == name)); }
        }

        private static void DumpType(StringBuilder sb, Assembly game, string name, string[] methodNames)
        {
            sb.AppendLine("=== TYPE " + name + " ===");
            Type type = FindType(game, name);
            if (type == null) { sb.AppendLine("TYPE_MISSING"); sb.AppendLine(); return; }

            sb.AppendLine("FullName=" + type.FullName);
            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                sb.AppendLine("FIELD " + (field.IsStatic ? "static " : "instance ") + Friendly(field.FieldType) + " " + field.Name);
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                sb.AppendLine("PROP " + Friendly(property.PropertyType) + " " + property.Name);

            IEnumerable<MethodInfo> methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(m => methodNames == null || methodNames.Contains(m.Name));
            foreach (ConstructorInfo constructor in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                if (methodNames != null && methodNames.Contains(".ctor")) DumpMethod(sb, constructor);
            foreach (MethodInfo method in methods.OrderBy(m => m.MetadataToken)) DumpMethod(sb, method);
            sb.AppendLine("=== END TYPE " + name + " ===");
            sb.AppendLine();
        }

        private static void DumpMethod(StringBuilder sb, MethodBase method)
        {
            sb.AppendLine("METHOD " + Signature(method));
            MethodBody body;
            try { body = method.GetMethodBody(); }
            catch (Exception ex) { sb.AppendLine("  <body failed: " + ex.GetType().Name + ">"); return; }
            if (body == null) { sb.AppendLine("  <no IL>"); return; }
            try
            {
                foreach (Instruction instruction in Read(method))
                    sb.Append("  IL_").Append(instruction.Offset.ToString("X4", CultureInfo.InvariantCulture)).Append(' ').Append(instruction.Op.Name).Append(' ').AppendLine(instruction.Text ?? "");
            }
            catch (Exception ex) { sb.AppendLine("  <IL read failed: " + ex.GetType().Name + ">"); }
        }

        private static string Signature(MethodBase method)
        {
            string owner = method.DeclaringType == null ? "<global>" : method.DeclaringType.FullName;
            string ret = method is MethodInfo ? Friendly(((MethodInfo)method).ReturnType) : "System.Void";
            string pars = string.Join(", ", method.GetParameters().Select(p => Friendly(p.ParameterType) + " " + p.Name).ToArray());
            return ret + " " + owner + "." + method.Name + "(" + pars + ") token=0x" + method.MetadataToken.ToString("X8", CultureInfo.InvariantCulture);
        }

        private static string Friendly(Type type)
        {
            return type == null ? "<null>" : (type.FullName ?? type.Name);
        }

        private static List<Instruction> Read(MethodBase method)
        {
            List<Instruction> result = new List<Instruction>();
            MethodBody body = method.GetMethodBody();
            if (body == null) return result;
            byte[] bytes = body.GetILAsByteArray();
            Module module = method.Module;
            Type[] typeArgs = method.DeclaringType != null && method.DeclaringType.IsGenericType ? method.DeclaringType.GetGenericArguments() : null;
            Type[] methodArgs = method.IsGenericMethod ? method.GetGenericArguments() : null;
            int pos = 0;

            while (pos < bytes.Length)
            {
                int offset = pos;
                byte first = bytes[pos++];
                OpCode op = first == 0xFE ? TwoByte[bytes[pos++]] : OneByte[first];
                string text = "";

                switch (op.OperandType)
                {
                    case OperandType.InlineNone: break;
                    case OperandType.ShortInlineI: text = ((sbyte)bytes[pos++]).ToString(CultureInfo.InvariantCulture); break;
                    case OperandType.InlineI: text = BitConverter.ToInt32(bytes, pos).ToString(CultureInfo.InvariantCulture); pos += 4; break;
                    case OperandType.InlineI8: text = BitConverter.ToInt64(bytes, pos).ToString(CultureInfo.InvariantCulture); pos += 8; break;
                    case OperandType.ShortInlineR: text = BitConverter.ToSingle(bytes, pos).ToString("R", CultureInfo.InvariantCulture); pos += 4; break;
                    case OperandType.InlineR: text = BitConverter.ToDouble(bytes, pos).ToString("R", CultureInfo.InvariantCulture); pos += 8; break;
                    case OperandType.ShortInlineVar: text = bytes[pos++].ToString(CultureInfo.InvariantCulture); break;
                    case OperandType.InlineVar: text = BitConverter.ToUInt16(bytes, pos).ToString(CultureInfo.InvariantCulture); pos += 2; break;
                    case OperandType.ShortInlineBrTarget:
                    {
                        sbyte delta = unchecked((sbyte)bytes[pos++]);
                        text = "IL_" + (pos + delta).ToString("X4", CultureInfo.InvariantCulture);
                        break;
                    }
                    case OperandType.InlineBrTarget:
                    {
                        int delta = BitConverter.ToInt32(bytes, pos); pos += 4;
                        text = "IL_" + (pos + delta).ToString("X4", CultureInfo.InvariantCulture);
                        break;
                    }
                    case OperandType.InlineSwitch:
                    {
                        int count = BitConverter.ToInt32(bytes, pos); pos += 4;
                        int basePos = pos + count * 4;
                        string[] targets = new string[count];
                        for (int i = 0; i < count; i++) { int delta = BitConverter.ToInt32(bytes, pos); pos += 4; targets[i] = "IL_" + (basePos + delta).ToString("X4", CultureInfo.InvariantCulture); }
                        text = "[" + string.Join(",", targets) + "]";
                        break;
                    }
                    case OperandType.InlineString:
                    {
                        int token = BitConverter.ToInt32(bytes, pos); pos += 4;
                        try { text = "\"" + module.ResolveString(token).Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\"") + "\""; }
                        catch { text = "string-token:0x" + token.ToString("X8", CultureInfo.InvariantCulture); }
                        break;
                    }
                    case OperandType.InlineField:
                    case OperandType.InlineMethod:
                    case OperandType.InlineType:
                    case OperandType.InlineTok:
                    {
                        int token = BitConverter.ToInt32(bytes, pos); pos += 4;
                        try { text = FormatMember(module.ResolveMember(token, typeArgs, methodArgs)) + " token=0x" + token.ToString("X8", CultureInfo.InvariantCulture); }
                        catch { text = "member-token:0x" + token.ToString("X8", CultureInfo.InvariantCulture); }
                        break;
                    }
                    case OperandType.InlineSig:
                    {
                        int token = BitConverter.ToInt32(bytes, pos); pos += 4;
                        text = "sig-token:0x" + token.ToString("X8", CultureInfo.InvariantCulture);
                        break;
                    }
                    default: throw new NotSupportedException("Unsupported operand type " + op.OperandType);
                }
                result.Add(new Instruction(offset, op, text));
            }
            return result;
        }

        private static string FormatMember(MemberInfo member)
        {
            if (member == null) return "<null-member>";
            FieldInfo field = member as FieldInfo;
            if (field != null) return Friendly(field.FieldType) + " " + Friendly(field.DeclaringType) + "." + field.Name;
            MethodBase method = member as MethodBase;
            if (method != null) return Signature(method);
            Type type = member as Type;
            if (type != null) return Friendly(type);
            return member.ToString();
        }

        private sealed class Instruction
        {
            public readonly int Offset;
            public readonly OpCode Op;
            public readonly string Text;
            public Instruction(int offset, OpCode op, string text) { Offset = offset; Op = op; Text = text; }
        }
    }
}
