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
    public sealed class PrayerClarityCraftingQualityProbe : BaseUnityPlugin
    {
        public const string PluginGuid = "prayerclarity.research.craftingquality";
        public const string PluginName = "PrayerClarity Crafting Quality Probe";
        public const string PluginVersion = "0.1.0";

        private static readonly Dictionary<short, OpCode> OneByte = new Dictionary<short, OpCode>();
        private static readonly Dictionary<short, OpCode> TwoByte = new Dictionary<short, OpCode>();
        private bool _completed;
        private float _readyAt = -1f;

        static PrayerClarityCraftingQualityProbe()
        {
            foreach (FieldInfo f in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (f.FieldType != typeof(OpCode)) continue;
                OpCode op = (OpCode)f.GetValue(null);
                ushort value = unchecked((ushort)op.Value);
                if (value < 0x100) OneByte[(short)value] = op;
                else if ((value & 0xff00) == 0xfe00) TwoByte[(short)(value & 0xff)] = op;
            }
        }

        private void Awake()
        {
            Logger.LogInfo("PrayerClarity Crafting Quality Probe 0.1.0 loaded: read-only runtime metadata/GameBalance inspection; no Harmony and no intentional mutation.");
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

            try { Run(game); }
            catch (Exception ex) { Logger.LogError("PrayerClarity crafting-quality probe failed: " + ex); }
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
            Type craftType = all.FirstOrDefault(t => t != null && t.Name == "CraftDefinition");
            Type balanceType = all.FirstOrDefault(t => t != null && t.Name == "GameBalance");

            StringBuilder sb = new StringBuilder(512 * 1024);
            sb.AppendLine("PRAYERCLARITY — WRITING / PRAYER QUALITY PATH AUDIT");
            sb.AppendLine("ProbeVersion=" + PluginVersion);
            sb.AppendLine("GeneratedUtc=" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            sb.AppendLine("GameAssembly=" + game.FullName);
            sb.AppendLine("ModuleVersionId=" + game.ManifestModule.ModuleVersionId);
            sb.AppendLine("Contract=READ_ONLY_METADATA_AND_GAMEBALANCE_INSPECTION_NO_HARMONY_NO_MUTATION");
            sb.AppendLine("Question=How does stock 1.407 derive Notes/Chapter/Book/prayer quality, and which row values/modifiers distinguish Chapter specialists from Book Combo?");
            sb.AppendLine();

            if (craftType == null || balanceType == null)
            {
                sb.AppendLine("ERROR CraftDefinition or GameBalance type missing.");
                Write(sb);
                return;
            }

            sb.AppendLine("=== CRAFTDEFINITION TYPE SHAPE ===");
            DumpTypeShape(sb, craftType);
            sb.AppendLine();

            sb.AppendLine("=== QUALITY-RELATED CRAFTDEFINITION METHODS / IL ===");
            MethodInfo[] methods = craftType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => IsQualityMethod(m.Name))
                .OrderBy(m => m.Name)
                .ThenBy(m => m.GetParameters().Length)
                .ToArray();
            foreach (MethodInfo method in methods)
            {
                DumpMethod(sb, method);
                sb.AppendLine();
            }

            object balance = GetStaticMember(balanceType, "me");
            if (balance == null)
            {
                sb.AppendLine("ERROR GameBalance.me unavailable.");
                Write(sb);
                return;
            }

            sb.AppendLine("=== RELEVANT CRAFT ROWS ===");
            int scanned = 0, matched = 0;
            const BindingFlags fields = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (FieldInfo collectionField in balanceType.GetFields(fields).OrderBy(f => f.Name))
            {
                IEnumerable enumerable;
                try { enumerable = collectionField.GetValue(balance) as IEnumerable; }
                catch { continue; }
                if (enumerable == null || enumerable is string) continue;

                foreach (object row in enumerable)
                {
                    if (row == null || !craftType.IsInstanceOfType(row)) continue;
                    scanned++;
                    string id = TryGetId(row);
                    if (!RelevantCraftId(id)) continue;
                    matched++;
                    sb.AppendLine("--- collection=" + collectionField.Name + " id=" + Quote(id) + " ---");
                    DumpObjectFields(sb, row, 0, new HashSet<object>(ReferenceComparer.Instance));
                    sb.AppendLine();
                }
            }
            sb.AppendLine("CRAFT_ROWS scanned=" + scanned + " matched=" + matched);
            sb.AppendLine();

            sb.AppendLine("=== QUALITY-RELATED METHODS ON NEARBY TYPES ===");
            foreach (Type t in all.Where(t => t != null && (t.Name == "CraftComponent" || t.Name == "Item" || t.Name == "PlayerComponent")))
            {
                foreach (MethodInfo m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .Where(m => IsQualityMethod(m.Name))
                    .OrderBy(m => m.Name)
                    .ThenBy(m => m.GetParameters().Length))
                {
                    DumpMethod(sb, m);
                    sb.AppendLine();
                }
            }

            Write(sb);
        }

        private static bool IsQualityMethod(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            return name.IndexOf("quality", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("multi", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("buffvalue", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool RelevantCraftId(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            string s = id.ToLowerInvariant();
            return s.Contains("note") ||
                   s.Contains("chapter") ||
                   s.Contains("book") ||
                   s.StartsWith("pray:b_faith:") ||
                   s.StartsWith("pray:b_money:") ||
                   s.StartsWith("pray:b_faith_money:");
        }

        private static void DumpTypeShape(StringBuilder sb, Type type)
        {
            sb.AppendLine("TYPE " + type.FullName);
            foreach (FieldInfo f in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).OrderBy(f => f.Name))
                sb.AppendLine("FIELD " + f.FieldType.FullName + " " + f.Name);
            foreach (PropertyInfo p in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).OrderBy(p => p.Name))
                sb.AppendLine("PROPERTY " + p.PropertyType.FullName + " " + p.Name);
        }

        private static void DumpMethod(StringBuilder sb, MethodInfo method)
        {
            sb.AppendLine("METHOD " + Signature(method));
            MethodBody body;
            try { body = method.GetMethodBody(); }
            catch (Exception ex)
            {
                sb.AppendLine("IL_UNAVAILABLE " + ex.GetType().Name);
                return;
            }
            if (body == null)
            {
                sb.AppendLine("IL=<none>");
                return;
            }

            byte[] il = body.GetILAsByteArray();
            if (il == null)
            {
                sb.AppendLine("IL=<null>");
                return;
            }

            Module module = method.Module;
            int pos = 0;
            while (pos < il.Length)
            {
                int offset = pos;
                OpCode op;
                byte first = il[pos++];
                if (first == 0xfe)
                {
                    if (pos >= il.Length || !TwoByte.TryGetValue(il[pos++], out op))
                    {
                        sb.AppendLine("IL_" + offset.ToString("X4") + " <unknown-fe>");
                        break;
                    }
                }
                else if (!OneByte.TryGetValue(first, out op))
                {
                    sb.AppendLine("IL_" + offset.ToString("X4") + " <unknown-" + first.ToString("X2") + ">");
                    break;
                }

                string operand;
                try { operand = ReadOperand(il, ref pos, op, module); }
                catch (Exception ex)
                {
                    operand = "<operand-error:" + ex.GetType().Name + ">";
                    pos = Math.Min(il.Length, pos);
                }
                sb.AppendLine("IL_" + offset.ToString("X4") + " " + op.Name + (string.IsNullOrEmpty(operand) ? "" : " " + operand));
            }
        }

        private static string ReadOperand(byte[] il, ref int pos, OpCode op, Module module)
        {
            switch (op.OperandType)
            {
                case OperandType.InlineNone: return "";
                case OperandType.ShortInlineI: return ((sbyte)il[pos++]).ToString(CultureInfo.InvariantCulture);
                case OperandType.InlineI:
                {
                    int v = BitConverter.ToInt32(il, pos); pos += 4; return v.ToString(CultureInfo.InvariantCulture);
                }
                case OperandType.InlineI8:
                {
                    long v = BitConverter.ToInt64(il, pos); pos += 8; return v.ToString(CultureInfo.InvariantCulture);
                }
                case OperandType.ShortInlineR:
                {
                    float v = BitConverter.ToSingle(il, pos); pos += 4; return v.ToString("R", CultureInfo.InvariantCulture);
                }
                case OperandType.InlineR:
                {
                    double v = BitConverter.ToDouble(il, pos); pos += 8; return v.ToString("R", CultureInfo.InvariantCulture);
                }
                case OperandType.ShortInlineBrTarget:
                {
                    sbyte delta = (sbyte)il[pos++]; return "IL_" + (pos + delta).ToString("X4");
                }
                case OperandType.InlineBrTarget:
                {
                    int delta = BitConverter.ToInt32(il, pos); pos += 4; return "IL_" + (pos + delta).ToString("X4");
                }
                case OperandType.ShortInlineVar:
                    return "V_" + il[pos++].ToString(CultureInfo.InvariantCulture);
                case OperandType.InlineVar:
                {
                    ushort v = BitConverter.ToUInt16(il, pos); pos += 2; return "V_" + v.ToString(CultureInfo.InvariantCulture);
                }
                case OperandType.InlineString:
                {
                    int token = BitConverter.ToInt32(il, pos); pos += 4;
                    try { return Quote(module.ResolveString(token)); } catch { return "string-token=0x" + token.ToString("X8"); }
                }
                case OperandType.InlineField:
                case OperandType.InlineMethod:
                case OperandType.InlineType:
                case OperandType.InlineTok:
                case OperandType.InlineSig:
                {
                    int token = BitConverter.ToInt32(il, pos); pos += 4;
                    try
                    {
                        MemberInfo member = module.ResolveMember(token);
                        return member == null ? "token=0x" + token.ToString("X8") : member.DeclaringType.FullName + "::" + member.Name;
                    }
                    catch { return "token=0x" + token.ToString("X8"); }
                }
                case OperandType.InlineSwitch:
                {
                    int n = BitConverter.ToInt32(il, pos); pos += 4;
                    int basePos = pos + 4 * n;
                    List<string> targets = new List<string>();
                    for (int i = 0; i < n; i++)
                    {
                        int delta = BitConverter.ToInt32(il, pos); pos += 4;
                        targets.Add("IL_" + (basePos + delta).ToString("X4"));
                    }
                    return string.Join(",", targets.ToArray());
                }
                default:
                    return "<operand:" + op.OperandType + ">";
            }
        }

        private static string Signature(MethodInfo m)
        {
            string pars = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.FullName + " " + p.Name).ToArray());
            return (m.IsStatic ? "static " : "") + m.ReturnType.FullName + " " + m.DeclaringType.FullName + "." + m.Name + "(" + pars + ")";
        }

        private static void DumpObjectFields(StringBuilder sb, object obj, int depth, HashSet<object> seen)
        {
            if (obj == null || depth > 3) return;
            Type type = obj.GetType();
            if (!type.IsValueType && !(obj is string))
            {
                if (!seen.Add(obj)) return;
            }

            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (FieldInfo field in type.GetFields(flags).OrderBy(f => f.Name))
            {
                object value;
                try { value = field.GetValue(obj); } catch { continue; }
                if (value == null) continue;
                string prefix = new string(' ', depth * 2) + field.Name + "=";

                if (value is string)
                {
                    sb.AppendLine(prefix + Quote((string)value));
                    continue;
                }

                Type vt = value.GetType();
                if (vt.IsPrimitive || vt.IsEnum || value is decimal)
                {
                    sb.AppendLine(prefix + Convert.ToString(value, CultureInfo.InvariantCulture));
                    continue;
                }

                IEnumerable en = value as IEnumerable;
                if (en != null && !(value is string))
                {
                    int i = 0;
                    foreach (object child in en)
                    {
                        if (++i > 80) { sb.AppendLine(prefix + "<truncated>"); break; }
                        if (child == null) continue;
                        if (child is string || child.GetType().IsPrimitive || child.GetType().IsEnum || child is decimal)
                            sb.AppendLine(prefix + "[" + i + "] " + Convert.ToString(child, CultureInfo.InvariantCulture));
                        else
                        {
                            sb.AppendLine(prefix + "[" + i + "] type=" + child.GetType().FullName + " id=" + Quote(TryGetId(child)));
                            if (depth < 2) DumpObjectFields(sb, child, depth + 1, seen);
                        }
                    }
                    continue;
                }

                sb.AppendLine(prefix + "type=" + vt.FullName + " id=" + Quote(TryGetId(value)));
                if (depth < 2) DumpObjectFields(sb, value, depth + 1, seen);
            }
        }

        private static object GetStaticMember(Type type, string name)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            try
            {
                PropertyInfo p = type.GetProperty(name, flags);
                if (p != null && p.GetIndexParameters().Length == 0) return p.GetValue(null, null);
                FieldInfo f = type.GetField(name, flags);
                if (f != null) return f.GetValue(null);
            }
            catch { }
            return null;
        }

        private static string TryGetId(object obj)
        {
            if (obj == null) return "";
            Type type = obj.GetType();
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            try
            {
                FieldInfo f = type.GetField("id", flags);
                if (f != null) return Convert.ToString(f.GetValue(obj), CultureInfo.InvariantCulture) ?? "";
                PropertyInfo p = type.GetProperty("id", flags);
                if (p != null && p.GetIndexParameters().Length == 0) return Convert.ToString(p.GetValue(obj, null), CultureInfo.InvariantCulture) ?? "";
            }
            catch { }
            return "";
        }

        private void Write(StringBuilder sb)
        {
            string path = Path.Combine(Paths.BepInExRootPath, "PrayerClarity-crafting-quality-0.1.0.txt");
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
            Logger.LogInfo("PrayerClarity crafting-quality audit complete: " + path);
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

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceComparer Instance = new ReferenceComparer();
            public new bool Equals(object x, object y) { return ReferenceEquals(x, y); }
            public int GetHashCode(object obj) { return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj); }
        }
    }
}
