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
    public sealed class PrayerClaritySmartExpressionProbe : BaseUnityPlugin
    {
        public const string PluginGuid = "prayerclarity.auditprobe.smartexpression";
        public const string PluginName = "PrayerClarity SmartExpression Lifecycle Probe";
        public const string PluginVersion = "0.1.8";

        private bool _completed;
        private float _readyAt = -1f;
        private static readonly OpCode[] OneByte = new OpCode[256];
        private static readonly OpCode[] TwoByte = new OpCode[256];

        static PrayerClaritySmartExpressionProbe()
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
            Logger.LogInfo("PrayerClarity SmartExpression Lifecycle Probe 0.1.8 loaded: read-only type/IL/runtime-state inspection; no Harmony, expression mutation, graph execution, save mutation, or world mutation.");
        }

        private void Update()
        {
            if (_completed) return;

            Assembly game = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Assembly-CSharp");
            if (game == null || !GameStarted(game)) return;

            if (_readyAt < 0f)
            {
                _readyAt = Time.realtimeSinceStartup;
                return;
            }
            if (Time.realtimeSinceStartup - _readyAt < 5f) return;

            _completed = true;
            try { Run(game); }
            catch (Exception ex) { Logger.LogError("PrayerClarity SmartExpression lifecycle audit failed: " + ex); }
        }

        private static bool GameStarted(Assembly game)
        {
            Type mainGame = SafeTypes(game).FirstOrDefault(t => t != null && t.Name == "MainGame");
            FieldInfo field = mainGame == null ? null : mainGame.GetField("game_started", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            try { return field != null && field.FieldType == typeof(bool) && (bool)field.GetValue(null); }
            catch { return false; }
        }

        private void Run(Assembly game)
        {
            Type[] all = AppDomain.CurrentDomain.GetAssemblies().SelectMany(SafeTypes).ToArray();
            Type smart = all.FirstOrDefault(t => t != null && t.Name == "SmartExpression");
            Type balance = all.FirstOrDefault(t => t != null && t.Name == "GameBalance");

            StringBuilder sb = new StringBuilder(256 * 1024);
            sb.AppendLine("PRAYERCLARITY — SMARTEXPRESSION LIFECYCLE / ROOTS FIX AUDIT");
            sb.AppendLine("ProbeVersion=" + PluginVersion);
            sb.AppendLine("GeneratedUtc=" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            sb.AppendLine("GameAssembly=" + game.FullName);
            sb.AppendLine("ModuleVersionId=" + game.ManifestModule.ModuleVersionId);
            sb.AppendLine("Contract=READ_ONLY_TYPE_IL_RUNTIME_STATE_NO_HARMONY_NO_EXPRESSION_MUTATION_NO_WORLD_MUTATION");
            sb.AppendLine("Question=Can affected loaded craft_time SmartExpression values be replaced/reinitialized narrowly and safely for the Shoots-and-Roots WGOpar-to-Ppar scope repair?");
            sb.AppendLine();

            if (smart == null)
            {
                sb.AppendLine("SmartExpression missing");
            }
            else
            {
                DumpSmartExpressionContract(sb, smart);
                DumpRelevantIl(sb, smart);
                DumpCraftTimeFieldContract(sb, all, smart);
                DumpAffectedInstances(sb, balance, smart);
            }

            string path = Path.Combine(Paths.BepInExRootPath, "PrayerClarity-smartexpression-0.1.8.txt");
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
            Logger.LogInfo("PrayerClarity SmartExpression lifecycle audit complete: " + path);
        }

        private static void DumpSmartExpressionContract(StringBuilder sb, Type smart)
        {
            const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

            sb.AppendLine("=== SMARTEXPRESSION TYPE CONTRACT ===");
            sb.AppendLine("TYPE " + smart.AssemblyQualifiedName);
            sb.AppendLine("IsValueType=" + smart.IsValueType + " IsClass=" + smart.IsClass + " IsSerializable=" + smart.IsSerializable);

            foreach (FieldInfo field in smart.GetFields(all).OrderBy(f => f.MetadataToken))
            {
                sb.AppendLine("FIELD " + Visibility(field) + (field.IsStatic ? " static" : "") + (field.IsInitOnly ? " readonly" : "") + " " + TypeName(field.FieldType) + " " + field.Name + " token=0x" + field.MetadataToken.ToString("X8", CultureInfo.InvariantCulture));
            }

            foreach (PropertyInfo property in smart.GetProperties(all).OrderBy(p => p.MetadataToken))
            {
                MethodInfo getter = property.GetGetMethod(true);
                MethodInfo setter = property.GetSetMethod(true);
                sb.AppendLine("PROPERTY " + TypeName(property.PropertyType) + " " + property.Name + " get=" + MethodAccess(getter) + " set=" + MethodAccess(setter));
            }

            foreach (ConstructorInfo ctor in smart.GetConstructors(all).OrderBy(c => c.MetadataToken))
                sb.AppendLine("CTOR " + Signature(ctor));

            foreach (MethodInfo method in smart.GetMethods(all).OrderBy(m => m.MetadataToken))
                sb.AppendLine("METHOD " + Signature(method));

            sb.AppendLine("=== END SMARTEXPRESSION TYPE CONTRACT ===");
            sb.AppendLine();
        }

        private static void DumpRelevantIl(StringBuilder sb, Type smart)
        {
            const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            List<MethodBase> methods = new List<MethodBase>();
            methods.AddRange(smart.GetConstructors(all).Cast<MethodBase>());
            methods.AddRange(smart.GetMethods(all)
                .Where(m => IsRelevantMethod(m, smart))
                .Cast<MethodBase>());

            sb.AppendLine("=== RELEVANT SMARTEXPRESSION IL ===");
            foreach (MethodBase method in methods.Distinct().OrderBy(m => m.MetadataToken))
                DumpMethod(sb, method);
            sb.AppendLine("=== END RELEVANT SMARTEXPRESSION IL ===");
            sb.AppendLine();
        }

        private static bool IsRelevantMethod(MethodInfo method, Type smart)
        {
            string n = method.Name ?? "";
            if (n.IndexOf("Expression", StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.IndexOf("Init", StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.IndexOf("Check", StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.IndexOf("Raw", StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.IndexOf("Set", StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.IndexOf("Compile", StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.IndexOf("Parse", StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.IndexOf("Evaluate", StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.IndexOf("op_", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            if (method.ReturnType == smart) return true;
            return method.GetParameters().Any(p => p.ParameterType == smart);
        }

        private static void DumpCraftTimeFieldContract(StringBuilder sb, Type[] all, Type smart)
        {
            sb.AppendLine("=== CRAFT_TIME FIELD CONTRACT ===");
            const BindingFlags fields = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            foreach (Type type in all.Where(t => t != null && t.Assembly.GetName().Name == "Assembly-CSharp"))
            {
                FieldInfo[] matches;
                try
                {
                    matches = type.GetFields(fields)
                        .Where(f => string.Equals(f.Name, "craft_time", StringComparison.OrdinalIgnoreCase) ||
                                    (f.FieldType == smart && f.Name.IndexOf("time", StringComparison.OrdinalIgnoreCase) >= 0))
                        .ToArray();
                }
                catch { continue; }

                foreach (FieldInfo field in matches)
                {
                    sb.AppendLine("CRAFT_TIME_FIELD owner=" + type.FullName + " " + Visibility(field) + (field.IsStatic ? " static" : "") + (field.IsInitOnly ? " readonly" : "") + " type=" + TypeName(field.FieldType) + " name=" + field.Name + " token=0x" + field.MetadataToken.ToString("X8", CultureInfo.InvariantCulture));
                }
            }
            sb.AppendLine("=== END CRAFT_TIME FIELD CONTRACT ===");
            sb.AppendLine();
        }

        private static void DumpAffectedInstances(StringBuilder sb, Type balanceType, Type smart)
        {
            sb.AppendLine("=== AFFECTED BUFF_PLANT SMARTEXPRESSION INSTANCES ===");
            if (balanceType == null)
            {
                sb.AppendLine("GameBalance missing");
                sb.AppendLine("=== END AFFECTED BUFF_PLANT SMARTEXPRESSION INSTANCES ===");
                return;
            }

            object balance = null;
            try
            {
                PropertyInfo me = balanceType.GetProperty("me", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (me != null) balance = me.GetValue(null, null);
            }
            catch { }

            if (balance == null)
            {
                sb.AppendLine("GameBalance.me unavailable");
                sb.AppendLine("=== END AFFECTED BUFF_PLANT SMARTEXPRESSION INSTANCES ===");
                return;
            }

            MethodInfo raw = smart.GetMethod("GetRawExpressionString", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            int hits = 0;
            int detailed = 0;
            const BindingFlags fields = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            foreach (FieldInfo collectionField in balanceType.GetFields(fields).OrderBy(f => f.Name))
            {
                object collection;
                try { collection = collectionField.GetValue(balance); }
                catch { continue; }
                IEnumerable enumerable = collection as IEnumerable;
                if (enumerable == null || collection is string) continue;

                int rowIndex = 0;
                foreach (object row in enumerable)
                {
                    rowIndex++;
                    if (row == null) continue;
                    ScanForPlantExpressions(sb, row, collectionField.Name + "[" + rowIndex + "] id=" + Quote(TryGetId(row)), smart, raw, ref hits, ref detailed, 0);
                }
            }

            sb.AppendLine("AFFECTED_INSTANCE_COUNT=" + hits + " DETAILED_INSTANCE_COUNT=" + detailed);
            sb.AppendLine("=== END AFFECTED BUFF_PLANT SMARTEXPRESSION INSTANCES ===");
            sb.AppendLine();
        }

        private static void ScanForPlantExpressions(StringBuilder sb, object obj, string path, Type smart, MethodInfo raw, ref int hits, ref int detailed, int depth)
        {
            if (obj == null || depth > 2) return;
            Type type = obj.GetType();
            const BindingFlags fields = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            foreach (FieldInfo field in type.GetFields(fields))
            {
                object value;
                try { value = field.GetValue(obj); }
                catch { continue; }
                if (value == null) continue;

                string fieldPath = path + "." + field.Name;
                if (smart.IsInstanceOfType(value))
                {
                    string expression = GetRaw(value, raw);
                    if (expression.IndexOf("buff_plant", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        hits++;
                        sb.AppendLine("AFFECTED path=" + fieldPath + " declaringField=" + type.FullName + "." + field.Name + " fieldReadonly=" + field.IsInitOnly + " raw=" + Quote(expression));
                        if (detailed < 4)
                        {
                            detailed++;
                            DumpSmartInstanceFields(sb, value, smart, detailed);
                        }
                    }
                    continue;
                }

                IEnumerable nested = value as IEnumerable;
                if (nested != null && !(value is string) && depth < 2)
                {
                    int index = 0;
                    foreach (object child in nested)
                    {
                        index++;
                        if (child == null) continue;
                        if (smart.IsInstanceOfType(child))
                        {
                            string expression = GetRaw(child, raw);
                            if (expression.IndexOf("buff_plant", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                hits++;
                                sb.AppendLine("AFFECTED path=" + fieldPath + "[" + index + "] raw=" + Quote(expression));
                                if (detailed < 4)
                                {
                                    detailed++;
                                    DumpSmartInstanceFields(sb, child, smart, detailed);
                                }
                            }
                        }
                        else if (child.GetType().Assembly == type.Assembly)
                        {
                            ScanForPlantExpressions(sb, child, fieldPath + "[" + index + "]", smart, raw, ref hits, ref detailed, depth + 1);
                        }
                    }
                }
                else if (value.GetType().Assembly == type.Assembly && depth < 2)
                {
                    ScanForPlantExpressions(sb, value, fieldPath, smart, raw, ref hits, ref detailed, depth + 1);
                }
            }
        }

        private static void DumpSmartInstanceFields(StringBuilder sb, object instance, Type smart, int ordinal)
        {
            sb.AppendLine("INSTANCE_STATE_BEGIN ordinal=" + ordinal);
            const BindingFlags fields = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (FieldInfo field in smart.GetFields(fields).OrderBy(f => f.MetadataToken))
            {
                object value = null;
                string read = "ok";
                try { value = field.GetValue(instance); }
                catch (Exception ex) { read = ex.GetType().Name; }
                sb.AppendLine("  " + field.Name + " type=" + TypeName(field.FieldType) + " readonly=" + field.IsInitOnly + " read=" + read + " value=" + SummarizeValue(value));
            }
            sb.AppendLine("INSTANCE_STATE_END ordinal=" + ordinal);
        }

        private static string GetRaw(object expression, MethodInfo raw)
        {
            if (expression == null) return "";
            try { return raw == null ? "" : Convert.ToString(raw.Invoke(expression, null), CultureInfo.InvariantCulture) ?? ""; }
            catch { return ""; }
        }

        private static string TryGetId(object row)
        {
            if (row == null) return "";
            Type type = row.GetType();
            FieldInfo field = type.GetField("id", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                try { return Convert.ToString(field.GetValue(row), CultureInfo.InvariantCulture) ?? ""; }
                catch { }
            }
            PropertyInfo prop = type.GetProperty("id", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop != null && prop.GetIndexParameters().Length == 0)
            {
                try { return Convert.ToString(prop.GetValue(row, null), CultureInfo.InvariantCulture) ?? ""; }
                catch { }
            }
            return "";
        }

        private static string SummarizeValue(object value)
        {
            if (value == null) return "<null>";
            string s = value as string;
            if (s != null) return Quote(s.Length > 500 ? s.Substring(0, 500) + "..." : s);
            Delegate d = value as Delegate;
            if (d != null) return "<delegate " + Signature(d.Method) + ">";
            Type t = value.GetType();
            if (t.IsPrimitive || value is decimal || value is Enum)
                return Convert.ToString(value, CultureInfo.InvariantCulture);
            return "<" + TypeName(t) + ">";
        }

        private static void DumpMethod(StringBuilder sb, MethodBase method)
        {
            sb.AppendLine("IL_METHOD " + Signature(method));
            MethodBody body;
            try { body = method.GetMethodBody(); }
            catch { return; }
            if (body == null)
            {
                sb.AppendLine("  <no IL>");
                return;
            }

            List<Instruction> instructions;
            try { instructions = Read(method); }
            catch (Exception ex)
            {
                sb.AppendLine("  <IL read failed: " + ex.GetType().Name + ">");
                return;
            }

            foreach (Instruction ins in instructions)
                sb.Append("  IL_").Append(ins.Offset.ToString("X4", CultureInfo.InvariantCulture)).Append(' ').Append(ins.Op.Name).Append(' ').AppendLine(ins.Text ?? "");
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
                    case OperandType.ShortInlineBrTarget:
                    {
                        sbyte delta = unchecked((sbyte)bytes[p++]);
                        text = "IL_" + (p + delta).ToString("X4", CultureInfo.InvariantCulture);
                        break;
                    }
                    case OperandType.InlineBrTarget:
                    {
                        int delta = BitConverter.ToInt32(bytes, p); p += 4;
                        text = "IL_" + (p + delta).ToString("X4", CultureInfo.InvariantCulture);
                        break;
                    }
                    case OperandType.InlineString:
                    {
                        int token = BitConverter.ToInt32(bytes, p); p += 4;
                        try { text = Quote(module.ResolveString(token)); }
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
                            MemberInfo member = op.OperandType == OperandType.InlineField ? module.ResolveField(token, typeArgs, methodArgs)
                                : op.OperandType == OperandType.InlineMethod ? module.ResolveMethod(token, typeArgs, methodArgs)
                                : op.OperandType == OperandType.InlineType ? module.ResolveType(token, typeArgs, methodArgs)
                                : op.OperandType == OperandType.InlineTok ? module.ResolveMember(token, typeArgs, methodArgs)
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
                        for (int i = 0; i < count; i++)
                        {
                            int delta = BitConverter.ToInt32(bytes, p); p += 4;
                            targets[i] = "IL_" + (basePos + delta).ToString("X4", CultureInfo.InvariantCulture);
                        }
                        text = "[" + string.Join(",", targets) + "]";
                        break;
                    }
                    default:
                        throw new NotSupportedException("operand " + op.OperandType);
                }

                result.Add(new Instruction { Offset = offset, Op = op, Text = text });
            }

            return result;
        }

        private static string Resolved(MemberInfo member)
        {
            FieldInfo field = member as FieldInfo;
            if (field != null) return field.DeclaringType.FullName + "." + field.Name + " : " + TypeName(field.FieldType);
            MethodBase method = member as MethodBase;
            if (method != null) return Signature(method);
            Type type = member as Type;
            return type != null ? type.FullName : Convert.ToString(member, CultureInfo.InvariantCulture);
        }

        private static string Signature(MethodBase method)
        {
            if (method == null) return "<null>";
            string owner = method.DeclaringType == null ? "<global>" : method.DeclaringType.FullName;
            string ret = method is MethodInfo ? TypeName(((MethodInfo)method).ReturnType) : "System.Void";
            string pars = string.Join(", ", method.GetParameters().Select(p => TypeName(p.ParameterType) + " " + p.Name).ToArray());
            string access = method.IsPublic ? "public" : method.IsFamily ? "protected" : method.IsAssembly ? "internal" : method.IsPrivate ? "private" : "nonpublic";
            return access + (method.IsStatic ? " static " : " ") + ret + " " + owner + "." + method.Name + "(" + pars + ") token=0x" + method.MetadataToken.ToString("X8", CultureInfo.InvariantCulture);
        }

        private static string Visibility(FieldInfo field)
        {
            return field.IsPublic ? "public" : field.IsFamily ? "protected" : field.IsAssembly ? "internal" : field.IsPrivate ? "private" : "nonpublic";
        }

        private static string MethodAccess(MethodInfo method)
        {
            if (method == null) return "none";
            return method.IsPublic ? "public" : method.IsFamily ? "protected" : method.IsAssembly ? "internal" : method.IsPrivate ? "private" : "nonpublic";
        }

        private static string TypeName(Type type)
        {
            return type == null ? "<null>" : (type.FullName ?? type.Name);
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
