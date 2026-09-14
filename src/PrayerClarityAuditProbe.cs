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
    public sealed class PrayerClarityAuditProbe : BaseUnityPlugin
    {
        public const string PluginGuid = "prayerclarity.auditprobe";
        public const string PluginName = "PrayerClarity Audit Probe";
        public const string PluginVersion = "0.1.0";

        private bool _completed;
        private float _startedAt;

        private static readonly OpCode[] OneByteOpCodes = new OpCode[0x100];
        private static readonly OpCode[] TwoByteOpCodes = new OpCode[0x100];

        static PrayerClarityAuditProbe()
        {
            foreach (FieldInfo field in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType != typeof(OpCode))
                    continue;

                OpCode op = (OpCode)field.GetValue(null);
                ushort value = unchecked((ushort)op.Value);
                if (value < 0x100)
                    OneByteOpCodes[value] = op;
                else if ((value & 0xFF00) == 0xFE00)
                    TwoByteOpCodes[value & 0xFF] = op;
            }
        }

        private void Awake()
        {
            _startedAt = Time.realtimeSinceStartup;
            Logger.LogInfo("PrayerClarity Audit Probe 0.1.0 loaded. Read-only reflection/IL audit; no Harmony patches or game-state mutation.");
        }

        private void Update()
        {
            if (_completed || Time.realtimeSinceStartup - _startedAt < 8f)
                return;

            Assembly gameAssembly = FindGameAssembly();
            if (gameAssembly == null)
                return;

            object gameBalance = TryGetGameBalance(gameAssembly);
            if (gameBalance == null && Time.realtimeSinceStartup - _startedAt < 60f)
                return;

            _completed = true;
            try
            {
                RunAudit(gameAssembly, gameBalance);
            }
            catch (Exception ex)
            {
                Logger.LogError("PrayerClarity audit failed: " + ex);
            }
        }

        private void RunAudit(Assembly gameAssembly, object gameBalance)
        {
            StringBuilder sb = new StringBuilder(256 * 1024);
            sb.AppendLine("PRAYERCLARITY — TARGETED READ-ONLY PRAYER AUDIT");
            sb.AppendLine("ProbeVersion=" + PluginVersion);
            sb.AppendLine("GeneratedUtc=" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            sb.AppendLine("GameAssembly=" + gameAssembly.FullName);
            sb.AppendLine("ModuleVersionId=" + gameAssembly.ManifestModule.ModuleVersionId);
            sb.AppendLine("Contract=READ_ONLY_REFLECTION_IL_NO_MUTATION");
            sb.AppendLine("Questions=final PrayResult consumer; prayer buff application/duration; passive buff consumers; exact selection/report UI; current localized prayer descriptions");
            sb.AppendLine();

            Type[] allTypes = GetTypesSafe(gameAssembly);

            sb.AppendLine("=== TARGET TYPE IL ===");
            DumpTypeMethods(sb, FindType(allTypes, "PrayCraftGUI"), new[] { "RedrawTextValues", "OnResourcePickerClosed", "OnPrayPressed", "Open", "Redraw" });
            DumpTypeMethods(sb, FindType(allTypes, "PrayReportGUI"), null);
            DumpTypeMethods(sb, FindType(allTypes, "PrayLogics"), null);
            DumpTypeMethods(sb, FindType(allTypes, "BuffsLogics"), new[] { "AddBuff", "RemoveBuff", "GiveBuffIfNotExists" });
            DumpTypeMethods(sb, FindType(allTypes, "PlayerBuff"), new[] { "GetTimerText" });
            DumpTypeMethods(sb, FindType(allTypes, "ItemDefinition"), new[] { "GetDescription" });
            sb.AppendLine("=== END TARGET TYPE IL ===");
            sb.AppendLine();

            DumpCrossReferences(sb, gameAssembly, allTypes);
            DumpRuntimeDefinitions(sb, allTypes, gameBalance);
            DumpLocalization(sb, allTypes);

            string outputPath = Path.Combine(Paths.BepInExRootPath, "PrayerClarity-audit-0.1.0.txt");
            File.WriteAllText(outputPath, sb.ToString(), new UTF8Encoding(false));
            Logger.LogInfo("PrayerClarity audit complete: " + outputPath);
        }

        private static Assembly FindGameAssembly()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => string.Equals(a.GetName().Name, "Assembly-CSharp", StringComparison.Ordinal));
        }

        private static Type[] GetTypesSafe(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null).ToArray();
            }
        }

        private static Type FindType(IEnumerable<Type> types, string name)
        {
            return types.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.Ordinal) || string.Equals(t.FullName, name, StringComparison.Ordinal));
        }

        private static object TryGetGameBalance(Assembly gameAssembly)
        {
            Type type = gameAssembly.GetType("GameBalance", false);
            if (type == null)
                return null;

            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            try
            {
                PropertyInfo prop = type.GetProperty("me", flags);
                if (prop != null)
                    return prop.GetValue(null, null);

                FieldInfo field = type.GetField("me", flags);
                return field == null ? null : field.GetValue(null);
            }
            catch
            {
                return null;
            }
        }

        private static void DumpTypeMethods(StringBuilder sb, Type type, string[] names)
        {
            if (type == null)
            {
                sb.AppendLine("MISSING TARGET TYPE");
                return;
            }

            sb.AppendLine("TYPE " + type.FullName);
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            IEnumerable<MethodBase> methods = type.GetMethods(flags).Cast<MethodBase>().Concat(type.GetConstructors(flags).Cast<MethodBase>());
            if (names != null)
                methods = methods.Where(m => names.Contains(m.Name));

            foreach (MethodBase method in methods.OrderBy(m => m.MetadataToken))
                DumpMethod(sb, method, null);
        }

        private static void DumpCrossReferences(StringBuilder sb, Assembly gameAssembly, Type[] allTypes)
        {
            sb.AppendLine("=== TARGETED CROSS-REFERENCE SCAN ===");
            List<FieldInfo> fields = new List<FieldInfo>();
            AddField(fields, allTypes, "PrayLogics", "last_pray_result");
            AddField(fields, allTypes, "PrayLogics", "_sermon_drops");
            AddField(fields, allTypes, "CraftDefinition", "dur_parameter");
            AddField(fields, allTypes, "CraftDefinition", "buff");
            AddField(fields, allTypes, "BuffDefinition", "craft_q");
            AddField(fields, allTypes, "PrayLogics+PrayResult", "faith");
            AddField(fields, allTypes, "PrayLogics+PrayResult", "faith_bonus");
            AddField(fields, allTypes, "PrayLogics+PrayResult", "money");
            AddField(fields, allTypes, "PrayLogics+PrayResult", "money_bonus");
            AddField(fields, allTypes, "PrayLogics+PrayResult", "success");

            string[] targetStrings =
            {
                "buff_sins", "buff_pen", "buff_star", "increase_gp_gain", "preach_params",
                "btn_try_pray", "btn_pray", "faith_bonus", "money_bonus"
            };

            Dictionary<string, MethodHit> hits = new Dictionary<string, MethodHit>();
            foreach (Type type in allTypes)
            {
                const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
                IEnumerable<MethodBase> methods;
                try
                {
                    methods = type.GetMethods(flags).Cast<MethodBase>().Concat(type.GetConstructors(flags).Cast<MethodBase>()).ToArray();
                }
                catch
                {
                    continue;
                }

                foreach (MethodBase method in methods)
                {
                    List<Instruction> instructions;
                    try
                    {
                        instructions = ReadInstructions(method);
                    }
                    catch
                    {
                        continue;
                    }

                    List<string> reasons = new List<string>();
                    foreach (Instruction instruction in instructions)
                    {
                        FieldInfo referencedField = instruction.ResolvedMember as FieldInfo;
                        if (referencedField != null)
                        {
                            foreach (FieldInfo target in fields)
                            {
                                if (SameMember(referencedField, target))
                                    reasons.Add("field:" + target.DeclaringType.FullName + "." + target.Name);
                            }
                        }

                        if (instruction.StringValue != null && targetStrings.Contains(instruction.StringValue))
                            reasons.Add("string:\"" + instruction.StringValue + "\"");
                    }

                    if (reasons.Count == 0)
                        continue;

                    string key = MethodIdentity(method);
                    MethodHit hit;
                    if (!hits.TryGetValue(key, out hit))
                    {
                        hit = new MethodHit { Method = method };
                        hits.Add(key, hit);
                    }
                    hit.Reasons.AddRange(reasons.Where(r => !hit.Reasons.Contains(r)));
                }
            }

            foreach (MethodHit hit in hits.Values.OrderBy(h => h.Method.DeclaringType.FullName).ThenBy(h => h.Method.MetadataToken))
                DumpMethod(sb, hit.Method, "HITS=" + string.Join(" | ", hit.Reasons.ToArray()));

            sb.AppendLine("CrossReferenceMethodCount=" + hits.Count);
            sb.AppendLine("=== END TARGETED CROSS-REFERENCE SCAN ===");
            sb.AppendLine();
        }

        private static void AddField(List<FieldInfo> fields, Type[] allTypes, string typeName, string fieldName)
        {
            Type type = FindType(allTypes, typeName);
            if (type == null)
                return;

            FieldInfo field = GetFieldRecursive(type, fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (field != null)
                fields.Add(field);
        }

        private static bool SameMember(MemberInfo a, MemberInfo b)
        {
            return a != null && b != null && a.Module == b.Module && a.MetadataToken == b.MetadataToken;
        }

        private static string MethodIdentity(MethodBase method)
        {
            return method.Module.ModuleVersionId + ":" + method.MetadataToken.ToString("X8", CultureInfo.InvariantCulture);
        }

        private static void DumpRuntimeDefinitions(StringBuilder sb, Type[] allTypes, object gameBalance)
        {
            sb.AppendLine("=== LIVE RUNTIME PRAYER DEFINITIONS ===");
            if (gameBalance == null)
            {
                sb.AppendLine("GameBalance singleton unavailable; registry dump skipped.");
                sb.AppendLine("=== END LIVE RUNTIME PRAYER DEFINITIONS ===");
                sb.AppendLine();
                return;
            }

            List<object> crafts = CollectObjectsOfType(gameBalance, "CraftDefinition");
            List<object> events = CollectObjectsOfType(gameBalance, "PrayEventDefinition");
            List<object> buffs = CollectObjectsOfType(gameBalance, "BuffDefinition");
            List<object> items = CollectObjectsOfType(gameBalance, "ItemDefinition");

            List<object> prayCrafts = crafts.Where(c => string.Equals(Convert.ToString(GetMemberValue(c, "craft_type"), CultureInfo.InvariantCulture), "PrayCraft", StringComparison.Ordinal)).ToList();
            HashSet<string> prayerBuffIds = new HashSet<string>(prayCrafts.Select(c => Convert.ToString(GetMemberValue(c, "buff"), CultureInfo.InvariantCulture)).Where(s => !string.IsNullOrEmpty(s)));

            sb.AppendLine("-- PrayCraft rows --");
            foreach (object craft in prayCrafts.OrderBy(c => Convert.ToString(GetMemberValue(c, "id"), CultureInfo.InvariantCulture)))
            {
                sb.Append("CRAFT id=").Append(FormatMember(c, "id"));
                sb.Append(" linked_sub_id=").Append(FormatMember(c, "linked_sub_id"));
                sb.Append(" needs_quality=").Append(FormatMember(c, "needs_quality"));
                sb.Append(" k_faith=").Append(FormatMember(c, "k_faith"));
                sb.Append(" k_money=").Append(FormatMember(c, "k_money"));
                sb.Append(" buff=").Append(FormatMember(c, "buff"));
                sb.Append(" dur_parameter=").Append(FormatMember(c, "dur_parameter"));
                sb.Append(" output=").Append(FormatItemList(GetMemberValue(c, "output")));
                sb.AppendLine();
            }

            sb.AppendLine("-- PrayEventDefinition rows --");
            foreach (object evt in events.OrderBy(e => Convert.ToString(GetMemberValue(e, "id"), CultureInfo.InvariantCulture)))
            {
                sb.Append("EVENT id=").Append(FormatMember(evt, "id"));
                sb.Append(" people=").Append(FormatExpression(GetMemberValue(evt, "people")));
                sb.Append(" faith=").Append(FormatExpression(GetMemberValue(evt, "faith")));
                sb.Append(" money=").Append(FormatExpression(GetMemberValue(evt, "money")));
                sb.AppendLine();
            }

            sb.AppendLine("-- Buff definitions referenced by prayers --");
            foreach (object buff in buffs.Where(b => prayerBuffIds.Contains(Convert.ToString(GetMemberValue(b, "id"), CultureInfo.InvariantCulture))).OrderBy(b => Convert.ToString(GetMemberValue(b, "id"), CultureInfo.InvariantCulture)))
            {
                sb.Append("BUFF id=").Append(FormatMember(buff, "id"));
                sb.Append(" craft_q=").Append(FormatMember(buff, "craft_q"));
                sb.Append(" length=").Append(FormatExpression(GetMemberValue(buff, "length")));
                sb.Append(" overlay_type=").Append(FormatMember(buff, "overlay_type"));
                sb.Append(" res=").Append(FormatGameRes(GetMemberValue(buff, "res")));
                sb.AppendLine();
            }

            sb.AppendLine("-- Prayer item mapping / current localized descriptions --");
            foreach (object item in items.OrderBy(i => Convert.ToString(GetMemberValue(i, "id"), CultureInfo.InvariantCulture)))
            {
                object linkedCraft = null;
                try { linkedCraft = GetMemberValue(item, "linked_craft"); } catch { }
                string linkedCraftType = linkedCraft == null ? "" : Convert.ToString(GetMemberValue(linkedCraft, "craft_type"), CultureInfo.InvariantCulture);
                string id = Convert.ToString(GetMemberValue(item, "id"), CultureInfo.InvariantCulture);
                if (!string.Equals(linkedCraftType, "PrayCraft", StringComparison.Ordinal) && (id == null || !id.StartsWith("b_", StringComparison.Ordinal)))
                    continue;

                sb.Append("ITEM id=").Append(id ?? "<null>");
                sb.Append(" linked_craft=").Append(linkedCraft == null ? "<null>" : FormatMember(linkedCraft, "id"));
                sb.Append(" name=").Append(QuoteAndFlatten(TryInvokeZeroArg(item, "GetName")));
                sb.Append(" description=").Append(QuoteAndFlatten(TryInvokeZeroArg(item, "GetDescription")));
                sb.AppendLine();
            }

            sb.AppendLine("=== END LIVE RUNTIME PRAYER DEFINITIONS ===");
            sb.AppendLine();
        }

        private static List<object> CollectObjectsOfType(object root, string elementTypeName)
        {
            List<object> result = new List<object>();
            HashSet<object> seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
            if (root == null)
                return result;

            foreach (FieldInfo field in GetFieldsRecursive(root.GetType(), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object value;
                try { value = field.GetValue(root); }
                catch { continue; }

                IEnumerable enumerable = value as IEnumerable;
                if (enumerable == null || value is string)
                    continue;

                int inspected = 0;
                try
                {
                    foreach (object element in enumerable)
                    {
                        if (++inspected > 10000)
                            break;
                        if (element == null || !string.Equals(element.GetType().Name, elementTypeName, StringComparison.Ordinal))
                            continue;
                        if (seen.Add(element))
                            result.Add(element);
                    }
                }
                catch { }
            }
            return result;
        }

        private static string FormatMember(object obj, string name)
        {
            object value = GetMemberValue(obj, name);
            return value == null ? "<null>" : Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        private static object GetMemberValue(object obj, string name)
        {
            if (obj == null)
                return null;

            Type type = obj.GetType();
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            PropertyInfo prop = GetPropertyRecursive(type, name, flags);
            if (prop != null && prop.GetIndexParameters().Length == 0)
            {
                try { return prop.GetValue(obj, null); } catch { }
            }

            FieldInfo field = GetFieldRecursive(type, name, flags);
            if (field != null)
            {
                try { return field.GetValue(obj); } catch { }
            }
            return null;
        }

        private static string TryInvokeZeroArg(object obj, string methodName)
        {
            if (obj == null)
                return null;
            try
            {
                MethodInfo method = obj.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == 0);
                if (method == null)
                    return null;
                object value = method.Invoke(obj, null);
                return value == null ? null : Convert.ToString(value, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                return "<error:" + ex.GetType().Name + ">";
            }
        }

        private static string FormatExpression(object expression)
        {
            if (expression == null)
                return "<null>";
            object raw = GetMemberValue(expression, "_expression");
            if (raw != null)
                return QuoteAndFlatten(Convert.ToString(raw, CultureInfo.InvariantCulture));
            try { return QuoteAndFlatten(Convert.ToString(expression, CultureInfo.InvariantCulture)); }
            catch { return "<unprintable>"; }
        }

        private static string FormatItemList(object value)
        {
            IEnumerable enumerable = value as IEnumerable;
            if (enumerable == null || value is string)
                return "[]";
            List<string> parts = new List<string>();
            try
            {
                foreach (object item in enumerable)
                {
                    if (item == null)
                        continue;
                    string id = Convert.ToString(GetMemberValue(item, "id"), CultureInfo.InvariantCulture);
                    object rawValue = GetMemberValue(item, "value");
                    parts.Add((id ?? "?") + ":" + (rawValue == null ? "?" : Convert.ToString(rawValue, CultureInfo.InvariantCulture)));
                }
            }
            catch { }
            return "[" + string.Join(",", parts.ToArray()) + "]";
        }

        private static string FormatGameRes(object value)
        {
            if (value == null)
                return "<null>";
            object typesValue = GetMemberValue(value, "_res_type");
            object valuesValue = GetMemberValue(value, "_res_v");
            IEnumerable types = typesValue as IEnumerable;
            IEnumerable values = valuesValue as IEnumerable;
            if (types == null || values == null)
                return QuoteAndFlatten(Convert.ToString(value, CultureInfo.InvariantCulture));

            List<object> typeList = types.Cast<object>().ToList();
            List<object> valueList = values.Cast<object>().ToList();
            List<string> parts = new List<string>();
            for (int i = 0; i < Math.Min(typeList.Count, valueList.Count); i++)
                parts.Add(Convert.ToString(typeList[i], CultureInfo.InvariantCulture) + "=" + Convert.ToString(valueList[i], CultureInfo.InvariantCulture));
            return "{" + string.Join(",", parts.ToArray()) + "}";
        }

        private static void DumpLocalization(StringBuilder sb, Type[] allTypes)
        {
            sb.AppendLine("=== CURRENT LOCALIZATION KEYS ===");
            Type gjl = FindType(allTypes, "GJL");
            if (gjl == null)
            {
                sb.AppendLine("GJL type not found.");
                sb.AppendLine("=== END CURRENT LOCALIZATION KEYS ===");
                return;
            }

            foreach (MethodInfo method in gjl.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Where(m => m.Name == "L"))
                sb.AppendLine("GJL_SIGNATURE " + FormatMethodSignature(method));

            string[] keys = { "preach_params", "btn_try_pray", "btn_pray" };
            foreach (string key in keys)
                sb.AppendLine("LOC key=" + key + " value=" + QuoteAndFlatten(TryLocalize(gjl, key)));

            sb.AppendLine("=== END CURRENT LOCALIZATION KEYS ===");
            sb.AppendLine();
        }

        private static string TryLocalize(Type gjl, string key)
        {
            foreach (MethodInfo method in gjl.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Where(m => m.Name == "L"))
            {
                ParameterInfo[] pars = method.GetParameters();
                if (pars.Length == 0 || pars[0].ParameterType != typeof(string))
                    continue;
                object[] args = new object[pars.Length];
                args[0] = key;
                bool supported = true;
                for (int i = 1; i < pars.Length; i++)
                {
                    if (pars[i].IsOptional)
                        args[i] = Type.Missing;
                    else if (pars[i].ParameterType == typeof(string))
                        args[i] = "";
                    else if (pars[i].ParameterType == typeof(object[]))
                        args[i] = new object[0];
                    else
                    {
                        supported = false;
                        break;
                    }
                }
                if (!supported)
                    continue;
                try
                {
                    object value = method.Invoke(null, args);
                    if (value != null)
                        return Convert.ToString(value, CultureInfo.InvariantCulture);
                }
                catch { }
            }
            return "<unresolved>";
        }

        private static string QuoteAndFlatten(string value)
        {
            if (value == null)
                return "<null>";
            return "\"" + value.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\"") + "\"";
        }

        private static FieldInfo GetFieldRecursive(Type type, string name, BindingFlags flags)
        {
            for (Type cur = type; cur != null; cur = cur.BaseType)
            {
                FieldInfo field = cur.GetField(name, flags | BindingFlags.DeclaredOnly);
                if (field != null)
                    return field;
            }
            return null;
        }

        private static IEnumerable<FieldInfo> GetFieldsRecursive(Type type, BindingFlags flags)
        {
            for (Type cur = type; cur != null; cur = cur.BaseType)
                foreach (FieldInfo field in cur.GetFields(flags | BindingFlags.DeclaredOnly))
                    yield return field;
        }

        private static PropertyInfo GetPropertyRecursive(Type type, string name, BindingFlags flags)
        {
            for (Type cur = type; cur != null; cur = cur.BaseType)
            {
                PropertyInfo prop = cur.GetProperty(name, flags | BindingFlags.DeclaredOnly);
                if (prop != null)
                    return prop;
            }
            return null;
        }

        private static void DumpMethod(StringBuilder sb, MethodBase method, string reason)
        {
            sb.AppendLine("METHOD " + FormatMethodSignature(method));
            if (!string.IsNullOrEmpty(reason))
                sb.AppendLine(reason);
            MethodBody body;
            try { body = method.GetMethodBody(); }
            catch (Exception ex)
            {
                sb.AppendLine("  <GetMethodBody failed: " + ex.GetType().Name + ">");
                return;
            }
            if (body == null)
            {
                sb.AppendLine("  <no IL body>");
                return;
            }

            foreach (Instruction instruction in ReadInstructions(method))
                sb.Append("  IL_").Append(instruction.Offset.ToString("X4", CultureInfo.InvariantCulture)).Append(' ').Append(instruction.OpCode.Name).Append(' ').AppendLine(instruction.OperandText ?? "");
        }

        private static string FormatMethodSignature(MethodBase method)
        {
            string declaring = method.DeclaringType == null ? "<global>" : method.DeclaringType.FullName;
            string returnType = method is MethodInfo ? ((MethodInfo)method).ReturnType.FullName : "System.Void";
            string pars = string.Join(", ", method.GetParameters().Select(p => (p.ParameterType.FullName ?? p.ParameterType.Name) + " " + p.Name).ToArray());
            return returnType + " " + declaring + "." + method.Name + "(" + pars + ") token=0x" + method.MetadataToken.ToString("X8", CultureInfo.InvariantCulture);
        }

        private static List<Instruction> ReadInstructions(MethodBase method)
        {
            List<Instruction> result = new List<Instruction>();
            MethodBody body = method.GetMethodBody();
            if (body == null)
                return result;
            byte[] il = body.GetILAsByteArray();
            Module module = method.Module;
            Type[] typeArgs = method.DeclaringType != null && method.DeclaringType.IsGenericType ? method.DeclaringType.GetGenericArguments() : null;
            Type[] methodArgs = method.IsGenericMethod ? method.GetGenericArguments() : null;
            int pos = 0;
            while (pos < il.Length)
            {
                int offset = pos;
                OpCode op;
                byte first = il[pos++];
                if (first == 0xFE)
                    op = TwoByteOpCodes[il[pos++]];
                else
                    op = OneByteOpCodes[first];

                string operandText = "";
                MemberInfo resolvedMember = null;
                string stringValue = null;
                switch (op.OperandType)
                {
                    case OperandType.InlineNone:
                        break;
                    case OperandType.ShortInlineI:
                        operandText = ((sbyte)il[pos]).ToString(CultureInfo.InvariantCulture);
                        pos += 1;
                        break;
                    case OperandType.InlineI:
                        operandText = BitConverter.ToInt32(il, pos).ToString(CultureInfo.InvariantCulture);
                        pos += 4;
                        break;
                    case OperandType.InlineI8:
                        operandText = BitConverter.ToInt64(il, pos).ToString(CultureInfo.InvariantCulture);
                        pos += 8;
                        break;
                    case OperandType.ShortInlineR:
                        operandText = BitConverter.ToSingle(il, pos).ToString("R", CultureInfo.InvariantCulture);
                        pos += 4;
                        break;
                    case OperandType.InlineR:
                        operandText = BitConverter.ToDouble(il, pos).ToString("R", CultureInfo.InvariantCulture);
                        pos += 8;
                        break;
                    case OperandType.ShortInlineBrTarget:
                    {
                        sbyte delta = unchecked((sbyte)il[pos++]);
                        operandText = "IL_" + (pos + delta).ToString("X4", CultureInfo.InvariantCulture);
                        break;
                    }
                    case OperandType.InlineBrTarget:
                    {
                        int delta = BitConverter.ToInt32(il, pos);
                        pos += 4;
                        operandText = "IL_" + (pos + delta).ToString("X4", CultureInfo.InvariantCulture);
                        break;
                    }
                    case OperandType.ShortInlineVar:
                        operandText = il[pos++].ToString(CultureInfo.InvariantCulture);
                        break;
                    case OperandType.InlineVar:
                        operandText = BitConverter.ToUInt16(il, pos).ToString(CultureInfo.InvariantCulture);
                        pos += 2;
                        break;
                    case OperandType.InlineString:
                    {
                        int token = BitConverter.ToInt32(il, pos);
                        pos += 4;
                        try
                        {
                            stringValue = module.ResolveString(token);
                            operandText = "\"" + stringValue.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\"") + "\"";
                        }
                        catch { operandText = "string-token:0x" + token.ToString("X8", CultureInfo.InvariantCulture); }
                        break;
                    }
                    case OperandType.InlineField:
                    case OperandType.InlineMethod:
                    case OperandType.InlineType:
                    case OperandType.InlineTok:
                    case OperandType.InlineSig:
                    {
                        int token = BitConverter.ToInt32(il, pos);
                        pos += 4;
                        try
                        {
                            if (op.OperandType == OperandType.InlineField)
                                resolvedMember = module.ResolveField(token, typeArgs, methodArgs);
                            else if (op.OperandType == OperandType.InlineMethod)
                                resolvedMember = module.ResolveMethod(token, typeArgs, methodArgs);
                            else if (op.OperandType == OperandType.InlineType)
                                resolvedMember = module.ResolveType(token, typeArgs, methodArgs);
                            else if (op.OperandType == OperandType.InlineTok)
                                resolvedMember = module.ResolveMember(token, typeArgs, methodArgs);
                            operandText = resolvedMember == null ? "token:0x" + token.ToString("X8", CultureInfo.InvariantCulture) : FormatResolvedMember(resolvedMember);
                        }
                        catch { operandText = "token:0x" + token.ToString("X8", CultureInfo.InvariantCulture); }
                        break;
                    }
                    case OperandType.InlineSwitch:
                    {
                        int count = BitConverter.ToInt32(il, pos);
                        pos += 4;
                        int basePos = pos + count * 4;
                        string[] targets = new string[count];
                        for (int i = 0; i < count; i++)
                        {
                            int delta = BitConverter.ToInt32(il, pos);
                            pos += 4;
                            targets[i] = "IL_" + (basePos + delta).ToString("X4", CultureInfo.InvariantCulture);
                        }
                        operandText = "[" + string.Join(",", targets) + "]";
                        break;
                    }
                    default:
                        throw new NotSupportedException("Unsupported operand type " + op.OperandType);
                }
                result.Add(new Instruction { Offset = offset, OpCode = op, OperandText = operandText, ResolvedMember = resolvedMember, StringValue = stringValue });
            }
            return result;
        }

        private static string FormatResolvedMember(MemberInfo member)
        {
            FieldInfo field = member as FieldInfo;
            if (field != null)
                return (field.DeclaringType == null ? "" : field.DeclaringType.FullName + ".") + field.Name + " : " + (field.FieldType.FullName ?? field.FieldType.Name);
            MethodBase method = member as MethodBase;
            if (method != null)
                return FormatMethodSignature(method);
            Type type = member as Type;
            if (type != null)
                return type.FullName;
            return member.ToString();
        }

        private sealed class Instruction
        {
            public int Offset;
            public OpCode OpCode;
            public string OperandText;
            public MemberInfo ResolvedMember;
            public string StringValue;
        }

        private sealed class MethodHit
        {
            public MethodBase Method;
            public readonly List<string> Reasons = new List<string>();
        }

        private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();
            public new bool Equals(object x, object y) { return ReferenceEquals(x, y); }
            public int GetHashCode(object obj) { return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj); }
        }
    }
}
