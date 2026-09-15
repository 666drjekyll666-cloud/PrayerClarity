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
    public sealed class PrayerClarityQualityIconProbe : BaseUnityPlugin
    {
        public const string PluginGuid = "prayerclarity.auditprobe.qualityicons";
        public const string PluginName = "PrayerClarity Quality Icon Probe";
        public const string PluginVersion = "0.1.9";

        private bool _completed;
        private float _readyAt = -1f;
        private static readonly OpCode[] OneByte = new OpCode[256];
        private static readonly OpCode[] TwoByte = new OpCode[256];

        static PrayerClarityQualityIconProbe()
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
            Logger.LogInfo("PrayerClarity Quality Icon Probe 0.1.9 loaded: one-shot read-only quality icon / tooltip capability audit; no Harmony and no game-state mutation.");
        }

        private void Update()
        {
            if (_completed) return;
            Assembly game = FindAssembly("Assembly-CSharp");
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
                Logger.LogError("PrayerClarity quality-icon audit failed: " + ex);
            }
        }

        private static Assembly FindAssembly(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == name);
        }

        private static bool GameStarted(Assembly game)
        {
            Type mainGame = FindType(game, "MainGame");
            FieldInfo field = mainGame == null ? null : mainGame.GetField("game_started", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            try { return field != null && field.FieldType == typeof(bool) && (bool)field.GetValue(null); }
            catch { return false; }
        }

        private void Run(Assembly game)
        {
            StringBuilder sb = new StringBuilder(256 * 1024);
            sb.AppendLine("PRAYERCLARITY — QUALITY ICON AUDIT");
            sb.AppendLine("ProbeVersion=" + PluginVersion);
            sb.AppendLine("GeneratedUtc=" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            sb.AppendLine("GameAssembly=" + game.FullName);
            sb.AppendLine("ModuleVersionId=" + game.ManifestModule.ModuleVersionId);
            sb.AppendLine("Contract=READ_ONLY_REFLECTION_IL_UI_RESOURCE_INSPECTION_NO_EXECUTION_NO_MUTATION");
            sb.AppendLine("Questions=native Bronze/Silver/Gold quality icon mapping; existing inline font-symbol support; native BubbleWidget icon-capable data; cheapest reliable tooltip integration seam");
            sb.AppendLine();

            DumpBaseItemCellQualityPath(sb, game);
            DumpUIFontSymbols(sb);
            DumpBubbleWidgetDataTypes(sb);
            DumpLiveQualityIcons(sb, game);
            DumpQualityNamedUiSprites(sb);

            string path = Path.Combine(Paths.BepInExRootPath, "PrayerClarity-quality-icons-0.1.9.txt");
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
            Logger.LogInfo("PrayerClarity quality-icon audit complete: " + path);
        }

        private static void DumpBaseItemCellQualityPath(StringBuilder sb, Assembly game)
        {
            sb.AppendLine("=== BASE ITEM CELL QUALITY PATH ===");
            Type cell = FindType(game, "BaseItemCellGUI");
            if (cell == null)
            {
                sb.AppendLine("BaseItemCellGUI missing");
                sb.AppendLine();
                return;
            }

            sb.AppendLine("Type=" + cell.FullName);
            foreach (FieldInfo field in cell.GetFields(AllDeclared))
            {
                if (ContainsQuality(field.Name))
                    sb.AppendLine("FIELD " + Friendly(field.FieldType) + " " + field.Name + " token=0x" + field.MetadataToken.ToString("X8", CultureInfo.InvariantCulture));
            }
            foreach (PropertyInfo property in cell.GetProperties(AllDeclared))
            {
                if (ContainsQuality(property.Name))
                    sb.AppendLine("PROP " + Friendly(property.PropertyType) + " " + property.Name);
            }

            foreach (MethodInfo method in cell.GetMethods(AllDeclared).OrderBy(m => m.MetadataToken))
            {
                List<Instruction> il;
                try { il = Read(method); }
                catch { continue; }
                bool relevant = ContainsQuality(method.Name) || il.Any(i => ContainsQuality(i.Text));
                if (!relevant) continue;
                sb.AppendLine("METHOD " + Signature(method));
                foreach (Instruction instruction in il)
                    sb.Append("  IL_").Append(instruction.Offset.ToString("X4", CultureInfo.InvariantCulture)).Append(' ').Append(instruction.Op.Name).Append(' ').AppendLine(instruction.Text ?? "");
            }

            Type elements = FindType(game, "BaseItemCellElements");
            if (elements != null)
            {
                sb.AppendLine("-- BaseItemCellElements quality members --");
                foreach (FieldInfo field in elements.GetFields(AllDeclared))
                    if (ContainsQuality(field.Name)) sb.AppendLine("FIELD " + Friendly(field.FieldType) + " " + field.Name);
                foreach (PropertyInfo property in elements.GetProperties(AllDeclared))
                    if (ContainsQuality(property.Name)) sb.AppendLine("PROP " + Friendly(property.PropertyType) + " " + property.Name);
            }
            sb.AppendLine("=== END BASE ITEM CELL QUALITY PATH ===");
            sb.AppendLine();
        }

        private static void DumpUIFontSymbols(StringBuilder sb)
        {
            sb.AppendLine("=== NGUI FONT SYMBOLS ===");
            Type fontType = FindTypeAcrossLoaded("UIFont");
            if (fontType == null)
            {
                sb.AppendLine("UIFont missing");
                sb.AppendLine();
                return;
            }

            UnityEngine.Object[] fonts;
            try { fonts = Resources.FindObjectsOfTypeAll(fontType); }
            catch (Exception ex)
            {
                sb.AppendLine("UIFont scan failed=" + ex.GetType().Name + ": " + ex.Message);
                sb.AppendLine();
                return;
            }

            sb.AppendLine("UIFontInstances=" + fonts.Length);
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (UnityEngine.Object font in fonts)
            {
                if (font == null) continue;
                object symbols = ReadMember(font, "symbols") ?? ReadMember(font, "mSymbols");
                IEnumerable enumerable = symbols as IEnumerable;
                if (enumerable == null) continue;

                string fontName = font.name ?? "<unnamed>";
                foreach (object symbol in enumerable)
                {
                    if (symbol == null) continue;
                    string sequence = Convert.ToString(ReadMember(symbol, "sequence"), CultureInfo.InvariantCulture) ?? string.Empty;
                    string sprite = Convert.ToString(ReadMember(symbol, "spriteName"), CultureInfo.InvariantCulture) ?? string.Empty;
                    string key = sequence + "|" + sprite;
                    if (!seen.Add(key)) continue;
                    sb.AppendLine("FONT " + fontName + " SYMBOL sequence=" + Quote(sequence) + " sprite=" + Quote(sprite) + " type=" + symbol.GetType().FullName);
                }
            }
            sb.AppendLine("UniqueSymbols=" + seen.Count);
            sb.AppendLine("=== END NGUI FONT SYMBOLS ===");
            sb.AppendLine();
        }

        private static void DumpBubbleWidgetDataTypes(StringBuilder sb)
        {
            sb.AppendLine("=== BUBBLE WIDGET DATA TYPES ===");
            Type baseType = FindTypeAcrossLoaded("BubbleWidgetData");
            if (baseType == null)
            {
                sb.AppendLine("BubbleWidgetData missing");
                sb.AppendLine();
                return;
            }

            List<Type> types = new List<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in SafeTypes(assembly))
                {
                    if (type != null && type != baseType && baseType.IsAssignableFrom(type))
                        types.Add(type);
                }
            }

            foreach (Type type in types.OrderBy(t => t.FullName, StringComparer.Ordinal))
            {
                sb.AppendLine("TYPE " + type.FullName);
                foreach (ConstructorInfo ctor in type.GetConstructors(AllInstance).OrderBy(c => c.MetadataToken))
                    sb.AppendLine("  CTOR " + Signature(ctor));
                foreach (FieldInfo field in type.GetFields(AllDeclared))
                    sb.AppendLine("  FIELD " + Friendly(field.FieldType) + " " + field.Name);
                foreach (PropertyInfo property in type.GetProperties(AllDeclared))
                    sb.AppendLine("  PROP " + Friendly(property.PropertyType) + " " + property.Name);
            }
            sb.AppendLine("DerivedTypeCount=" + types.Count);
            sb.AppendLine("=== END BUBBLE WIDGET DATA TYPES ===");
            sb.AppendLine();
        }

        private static void DumpLiveQualityIcons(StringBuilder sb, Assembly game)
        {
            sb.AppendLine("=== LIVE / PREFAB QUALITY ICONS ===");
            Type cellType = FindType(game, "BaseItemCellGUI");
            if (cellType == null)
            {
                sb.AppendLine("BaseItemCellGUI missing");
                sb.AppendLine();
                return;
            }

            FieldInfo qualityField = cellType.GetField("quality_icon", AllInstance);
            if (qualityField == null)
            {
                sb.AppendLine("quality_icon field missing");
                sb.AppendLine();
                return;
            }
            sb.AppendLine("quality_icon field type=" + Friendly(qualityField.FieldType));

            UnityEngine.Object[] cells;
            try { cells = Resources.FindObjectsOfTypeAll(cellType); }
            catch (Exception ex)
            {
                sb.AppendLine("BaseItemCellGUI scan failed=" + ex.GetType().Name + ": " + ex.Message);
                sb.AppendLine();
                return;
            }

            sb.AppendLine("BaseItemCellGUIInstances=" + cells.Length);
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            int emitted = 0;
            foreach (UnityEngine.Object cellObject in cells)
            {
                if (cellObject == null) continue;
                object icon;
                try { icon = qualityField.GetValue(cellObject); }
                catch { continue; }
                if (icon == null) continue;

                string signature = DescribeUiObject(icon);
                if (!seen.Add(signature)) continue;
                emitted++;
                sb.AppendLine("ICON_VARIANT " + emitted);
                sb.AppendLine("  " + signature.Replace("\n", "\n  "));
                sb.AppendLine("  CELL " + DescribeCellQualityContext(cellObject, cellType));
            }
            sb.AppendLine("UniqueQualityIconVariants=" + emitted);
            sb.AppendLine("=== END LIVE / PREFAB QUALITY ICONS ===");
            sb.AppendLine();
        }

        private static void DumpQualityNamedUiSprites(StringBuilder sb)
        {
            sb.AppendLine("=== QUALITY / STAR NAMED UI SPRITES ===");
            string[] typeNames = { "UI2DSprite", "UISprite" };
            HashSet<string> emitted = new HashSet<string>(StringComparer.Ordinal);
            foreach (string typeName in typeNames)
            {
                Type type = FindTypeAcrossLoaded(typeName);
                if (type == null) continue;
                UnityEngine.Object[] objects;
                try { objects = Resources.FindObjectsOfTypeAll(type); }
                catch { continue; }
                foreach (UnityEngine.Object obj in objects)
                {
                    if (obj == null) continue;
                    string description = DescribeUiObject(obj);
                    if (!ContainsQualityOrStar(description)) continue;
                    if (emitted.Add(description)) sb.AppendLine(description);
                }
            }
            sb.AppendLine("MatchingUniqueSprites=" + emitted.Count);
            sb.AppendLine("=== END QUALITY / STAR NAMED UI SPRITES ===");
            sb.AppendLine();
        }

        private static string DescribeCellQualityContext(object cell, Type cellType)
        {
            List<string> parts = new List<string>();
            foreach (FieldInfo field in cellType.GetFields(AllInstance))
            {
                string lower = field.Name.ToLowerInvariant();
                if (field.Name == "quality_icon") continue;
                if (!lower.Contains("quality") && !lower.Contains("item")) continue;
                object value;
                try { value = field.GetValue(cell); }
                catch { continue; }
                parts.Add(field.Name + "=" + DescribeValue(value));
                if (parts.Count >= 12) break;
            }
            return parts.Count == 0 ? "<no quality/item context fields>" : string.Join("; ", parts.ToArray());
        }

        private static string DescribeUiObject(object value)
        {
            if (value == null) return "<null>";
            List<string> parts = new List<string>();
            Type type = value.GetType();
            parts.Add("type=" + type.FullName);

            UnityEngine.Object unity = value as UnityEngine.Object;
            if (unity != null) parts.Add("name=" + Quote(unity.name));

            Component component = value as Component;
            GameObject go = component == null ? value as GameObject : component.gameObject;
            if (go != null)
            {
                parts.Add("go=" + Quote(go.name));
                parts.Add("activeSelf=" + go.activeSelf);
                parts.Add("layer=" + go.layer);
            }

            string[] memberNames =
            {
                "spriteName", "sprite2D", "atlas", "mainTexture", "material",
                "width", "height", "color", "enabled", "depth"
            };
            foreach (string memberName in memberNames)
            {
                object member = ReadMember(value, memberName);
                if (member != null) parts.Add(memberName + "=" + DescribeValue(member));
            }

            if (go != null)
            {
                Component[] components;
                try { components = go.GetComponents<Component>(); }
                catch { components = new Component[0]; }
                parts.Add("components=[" + string.Join(",", components.Where(c => c != null).Select(c => c.GetType().FullName).ToArray()) + "]");
            }

            return string.Join("; ", parts.ToArray());
        }

        private static object ReadMember(object instance, string name)
        {
            if (instance == null) return null;
            Type type = instance.GetType();
            try
            {
                PropertyInfo property = type.GetProperty(name, AllInstance);
                if (property != null && property.GetIndexParameters().Length == 0)
                    return property.GetValue(instance, null);
            }
            catch { }
            try
            {
                FieldInfo field = type.GetField(name, AllInstance);
                if (field != null) return field.GetValue(instance);
            }
            catch { }
            return null;
        }

        private static string DescribeValue(object value)
        {
            if (value == null) return "<null>";
            string text = value as string;
            if (text != null) return Quote(text);
            UnityEngine.Object unity = value as UnityEngine.Object;
            if (unity != null) return Friendly(value.GetType()) + "(" + Quote(unity.name) + ")";
            Type type = value.GetType();
            if (type.IsPrimitive || value is decimal || value is Enum)
                return Convert.ToString(value, CultureInfo.InvariantCulture);
            return Friendly(type) + "(" + Convert.ToString(value, CultureInfo.InvariantCulture) + ")";
        }

        private static bool ContainsQuality(string text)
        {
            return !string.IsNullOrEmpty(text) && text.IndexOf("quality", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool ContainsQualityOrStar(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            string lower = text.ToLowerInvariant();
            return lower.Contains("quality") || lower.Contains("star") || lower.Contains("bronze") || lower.Contains("silver") || lower.Contains("gold");
        }

        private static string Quote(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\"") + "\"";
        }

        private static Type FindType(Assembly assembly, string name)
        {
            return SafeTypes(assembly).FirstOrDefault(t => t != null && (t.Name == name || t.FullName == name));
        }

        private static Type FindTypeAcrossLoaded(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = FindType(assembly, name);
                if (type != null) return type;
            }
            return null;
        }

        private static IEnumerable<Type> SafeTypes(Assembly assembly)
        {
            try { return assembly.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null); }
            catch { return Enumerable.Empty<Type>(); }
        }

        private static string Friendly(Type type)
        {
            return type == null ? "<null>" : (type.FullName ?? type.Name);
        }

        private static string Signature(MethodBase method)
        {
            string owner = method.DeclaringType == null ? "<global>" : method.DeclaringType.FullName;
            string ret = method is MethodInfo ? Friendly(((MethodInfo)method).ReturnType) : "System.Void";
            string pars = string.Join(", ", method.GetParameters().Select(p => Friendly(p.ParameterType) + " " + p.Name).ToArray());
            return ret + " " + owner + "." + method.Name + "(" + pars + ") token=0x" + method.MetadataToken.ToString("X8", CultureInfo.InvariantCulture);
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
                string text = string.Empty;

                switch (op.OperandType)
                {
                    case OperandType.InlineNone:
                        break;
                    case OperandType.ShortInlineI:
                        text = ((sbyte)bytes[pos++]).ToString(CultureInfo.InvariantCulture);
                        break;
                    case OperandType.InlineI:
                        text = BitConverter.ToInt32(bytes, pos).ToString(CultureInfo.InvariantCulture); pos += 4;
                        break;
                    case OperandType.InlineI8:
                        text = BitConverter.ToInt64(bytes, pos).ToString(CultureInfo.InvariantCulture); pos += 8;
                        break;
                    case OperandType.ShortInlineR:
                        text = BitConverter.ToSingle(bytes, pos).ToString("R", CultureInfo.InvariantCulture); pos += 4;
                        break;
                    case OperandType.InlineR:
                        text = BitConverter.ToDouble(bytes, pos).ToString("R", CultureInfo.InvariantCulture); pos += 8;
                        break;
                    case OperandType.ShortInlineVar:
                        text = bytes[pos++].ToString(CultureInfo.InvariantCulture);
                        break;
                    case OperandType.InlineVar:
                        text = BitConverter.ToUInt16(bytes, pos).ToString(CultureInfo.InvariantCulture); pos += 2;
                        break;
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
                        int basePos = pos + (count * 4);
                        string[] targets = new string[count];
                        for (int i = 0; i < count; i++)
                        {
                            int delta = BitConverter.ToInt32(bytes, pos); pos += 4;
                            targets[i] = "IL_" + (basePos + delta).ToString("X4", CultureInfo.InvariantCulture);
                        }
                        text = "[" + string.Join(",", targets) + "]";
                        break;
                    }
                    case OperandType.InlineString:
                    {
                        int token = BitConverter.ToInt32(bytes, pos); pos += 4;
                        try { text = Quote(module.ResolveString(token)); }
                        catch { text = "string-token:0x" + token.ToString("X8", CultureInfo.InvariantCulture); }
                        break;
                    }
                    case OperandType.InlineField:
                    case OperandType.InlineMethod:
                    case OperandType.InlineType:
                    case OperandType.InlineTok:
                    {
                        int token = BitConverter.ToInt32(bytes, pos); pos += 4;
                        try
                        {
                            MemberInfo member = module.ResolveMember(token, typeArgs, methodArgs);
                            text = FormatMember(member) + " token=0x" + token.ToString("X8", CultureInfo.InvariantCulture);
                        }
                        catch { text = "member-token:0x" + token.ToString("X8", CultureInfo.InvariantCulture); }
                        break;
                    }
                    case OperandType.InlineSig:
                    {
                        int token = BitConverter.ToInt32(bytes, pos); pos += 4;
                        text = "sig-token:0x" + token.ToString("X8", CultureInfo.InvariantCulture);
                        break;
                    }
                    default:
                        throw new NotSupportedException("Unsupported operand type " + op.OperandType);
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

        private const BindingFlags AllInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private const BindingFlags AllDeclared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        private sealed class Instruction
        {
            public readonly int Offset;
            public readonly OpCode Op;
            public readonly string Text;

            public Instruction(int offset, OpCode op, string text)
            {
                Offset = offset;
                Op = op;
                Text = text;
            }
        }
    }
}
