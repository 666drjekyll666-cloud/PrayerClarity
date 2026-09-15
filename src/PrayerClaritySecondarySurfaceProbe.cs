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
    public sealed class PrayerClaritySecondarySurfaceProbe : BaseUnityPlugin
    {
        public const string PluginGuid = "prayerclarity.auditprobe.secondarysurfaces";
        public const string PluginName = "PrayerClarity Secondary Surface Probe";
        public const string PluginVersion = "0.1.7";

        private bool _completed;
        private float _readyAt = -1f;
        private static readonly OpCode[] OneByte = new OpCode[256];
        private static readonly OpCode[] TwoByte = new OpCode[256];

        static PrayerClaritySecondarySurfaceProbe()
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
            Logger.LogInfo("PrayerClarity Secondary Surface Probe 0.1.7 loaded: read-only reflection/IL/UI hierarchy audit; no Harmony and no game-state mutation.");
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
            catch (Exception ex) { Logger.LogError("PrayerClarity secondary-surface audit failed: " + ex); }
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
            sb.AppendLine("PRAYERCLARITY — SECONDARY CLARITY SURFACE AUDIT");
            sb.AppendLine("ProbeVersion=" + PluginVersion);
            sb.AppendLine("GeneratedUtc=" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            sb.AppendLine("GameAssembly=" + game.FullName);
            sb.AppendLine("ModuleVersionId=" + game.ManifestModule.ModuleVersionId);
            sb.AppendLine("Contract=READ_ONLY_REFLECTION_IL_UI_HIERARCHY_NO_EXECUTION_NO_MUTATION");
            sb.AppendLine("Questions=exact Character/Temporary Effects rendering seam; exact Technology prayer-tooltip seam; native tooltip mutation API");
            sb.AppendLine();

            DumpType(sb, game, "PerkBuffItemGUI", null);
            DumpType(sb, game, "InventoryGUI", new[] { "RedrawBuffsAndPerks" });
            DumpType(sb, game, "TechTreeGUIItem", new[] { "InitGamepadTooltip", "OnMouseOvered", "OnGamepadOver" });
            DumpType(sb, game, "TechTreeGUIUnlockItem", new[] { "Draw" });
            DumpType(sb, game, "TechUnlock", new[] { "GetTooltip", "GetData" });
            DumpType(sb, game, "Tooltip", new[] { "AddData", "SetData", "ClearData", "SetText", "Show", "Hide" });
            DumpType(sb, game, "BubbleWidgetDataContainer", null);
            DumpInventoryBuffPrefabHierarchy(sb, game);

            string path = Path.Combine(Paths.BepInExRootPath, "PrayerClarity-secondary-surfaces-0.1.7.txt");
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
            Logger.LogInfo("PrayerClarity secondary-surface audit complete: " + path);
        }

        private static void DumpType(StringBuilder sb, Assembly game, string name, string[] methodNames)
        {
            sb.AppendLine("=== TYPE " + name + " ===");
            Type type = FindType(game, name);
            if (type == null)
            {
                sb.AppendLine("TYPE_MISSING");
                sb.AppendLine();
                return;
            }

            sb.AppendLine("FullName=" + type.FullName);
            sb.AppendLine("Assembly=" + type.Assembly.FullName);
            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                sb.AppendLine("FIELD " + (field.IsStatic ? "static " : "instance ") + Friendly(field.FieldType) + " " + field.Name);
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                sb.AppendLine("PROP " + Friendly(property.PropertyType) + " " + property.Name);

            IEnumerable<MethodInfo> methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
            if (methodNames != null)
                methods = methods.Where(m => methodNames.Contains(m.Name));
            foreach (MethodInfo method in methods.OrderBy(m => m.MetadataToken))
                DumpMethod(sb, method);

            sb.AppendLine("=== END TYPE " + name + " ===");
            sb.AppendLine();
        }

        private static void DumpInventoryBuffPrefabHierarchy(StringBuilder sb, Assembly game)
        {
            sb.AppendLine("=== INVENTORY BUFF PREFAB HIERARCHY ===");
            Type inventoryType = FindType(game, "InventoryGUI");
            if (inventoryType == null) { sb.AppendLine("InventoryGUI missing"); return; }

            UnityEngine.Object[] instances;
            try { instances = Resources.FindObjectsOfTypeAll(inventoryType); }
            catch (Exception ex) { sb.AppendLine("InventoryGUI scan failed=" + ex.GetType().Name); return; }

            sb.AppendLine("InventoryGUIInstances=" + instances.Length);
            FieldInfo prefabField = inventoryType.GetField("perk_buff_item_prefab", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prefabField == null) { sb.AppendLine("perk_buff_item_prefab field missing"); return; }

            Component prefab = null;
            foreach (UnityEngine.Object instance in instances)
            {
                try
                {
                    object value = prefabField.GetValue(instance);
                    prefab = value as Component;
                    if (prefab != null) break;
                }
                catch { }
            }

            if (prefab == null) { sb.AppendLine("perk_buff_item_prefab value unavailable"); return; }
            sb.AppendLine("PrefabType=" + prefab.GetType().FullName);
            sb.AppendLine("PrefabName=" + prefab.gameObject.name);
            DumpGameObject(sb, prefab.gameObject, 0, 6);
            sb.AppendLine("=== END INVENTORY BUFF PREFAB HIERARCHY ===");
            sb.AppendLine();
        }

        private static void DumpGameObject(StringBuilder sb, GameObject go, int depth, int maxDepth)
        {
            if (go == null || depth > maxDepth) return;
            string indent = new string(' ', depth * 2);
            Component[] components;
            try { components = go.GetComponents<Component>(); }
            catch { components = new Component[0]; }
            sb.AppendLine(indent + "GO name=" + go.name + " activeSelf=" + go.activeSelf + " layer=" + go.layer + " components=" + string.Join(",", components.Where(c => c != null).Select(c => c.GetType().FullName).ToArray()));

            Transform transform = go.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child != null) DumpGameObject(sb, child.gameObject, depth + 1, maxDepth);
            }
        }

        private static Type FindType(Assembly game, string name)
        {
            try { return game.GetTypes().FirstOrDefault(t => t != null && (t.Name == name || t.FullName == name)); }
            catch (ReflectionTypeLoadException ex) { return ex.Types.FirstOrDefault(t => t != null && (t.Name == name || t.FullName == name)); }
        }

        private static string Friendly(Type type)
        {
            return type == null ? "<null>" : (type.FullName ?? type.Name);
        }

        private static void DumpMethod(StringBuilder sb, MethodBase method)
        {
            sb.AppendLine("METHOD " + Signature(method));
            MethodBody body;
            try { body = method.GetMethodBody(); }
            catch (Exception ex) { sb.AppendLine("  <body failed: " + ex.GetType().Name + ">"); return; }
            if (body == null) { sb.AppendLine("  <no IL>"); return; }

            List<Instruction> instructions;
            try { instructions = Read(method); }
            catch (Exception ex) { sb.AppendLine("  <IL read failed: " + ex.GetType().Name + ">"); return; }
            foreach (Instruction instruction in instructions)
                sb.Append("  IL_").Append(instruction.Offset.ToString("X4", CultureInfo.InvariantCulture)).Append(' ').Append(instruction.Op.Name).Append(' ').AppendLine(instruction.Text ?? "");
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
                string text = "";

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
