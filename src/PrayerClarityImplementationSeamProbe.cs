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
    public sealed class PrayerClarityImplementationSeamProbe : BaseUnityPlugin
    {
        public const string PluginGuid = "nikich.graveyardkeeper.prayerclarity.implementationseamprobe";
        public const string PluginName = "PrayerClarity Implementation Seam Probe";
        public const string PluginVersion = "0.1.0";

        private static readonly Guid SupportedGameMvid = new Guid("6f50b8e7-156b-49ac-bbe8-7505894b2364");
        private static readonly OpCode[] OneByte = new OpCode[256];
        private static readonly OpCode[] TwoByte = new OpCode[256];

        private bool _completed;
        private float _readyAt = -1f;

        static PrayerClarityImplementationSeamProbe()
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
            Logger.LogInfo(PluginName + " " + PluginVersion + " loaded. Read-only reflection/IL only; no Harmony, graph execution, balance mutation, save writes, or gameplay changes.");
        }

        private void Update()
        {
            if (_completed) return;

            Assembly game = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => string.Equals(a.GetName().Name, "Assembly-CSharp", StringComparison.Ordinal));
            if (game == null || !IsGameStarted(game)) return;

            if (_readyAt < 0f)
            {
                _readyAt = Time.realtimeSinceStartup;
                return;
            }
            if (Time.realtimeSinceStartup - _readyAt < 3f) return;

            _completed = true;
            try { Run(game); }
            catch (Exception ex) { Logger.LogError(PluginName + " failed: " + ex); }
        }

        private void Run(Assembly game)
        {
            if (game.ManifestModule.ModuleVersionId != SupportedGameMvid)
                throw new InvalidOperationException("Unsupported Assembly-CSharp MVID " + game.ManifestModule.ModuleVersionId);

            Type[] allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(SafeTypes).Where(t => t != null).ToArray();
            StringBuilder sb = new StringBuilder(1024 * 1024);
            sb.AppendLine("PRAYERCLARITY — IMPLEMENTATION SEAM PROBE");
            sb.AppendLine("ProbeVersion=" + PluginVersion);
            sb.AppendLine("GeneratedUtc=" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            sb.AppendLine("GameAssembly=" + game.FullName);
            sb.AppendLine("ModuleVersionId=" + game.ManifestModule.ModuleVersionId);
            sb.AppendLine("Contract=READ_ONLY_REFLECTION_IL_NO_HARMONY_NO_GRAPH_EXECUTION_NO_MUTATION_NO_SAVE_WRITE");
            sb.AppendLine("Questions=Combat exact outgoing player-damage seam; Repose Flow_DropBody executable node identity/owner seam");
            sb.AppendLine();

            DumpCombat(sb, allTypes);
            DumpRepose(sb, allTypes);

            string path = Path.Combine(Paths.BepInExRootPath, "PrayerClarity-implementation-seam-probe-0.1.0.txt");
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
            Logger.LogInfo(PluginName + " complete: " + path);
        }

        private static void DumpCombat(StringBuilder sb, Type[] allTypes)
        {
            sb.AppendLine("=== COMBAT: BASE CHARACTER ATTACK ===");
            Type attack = FindType(allTypes, "BaseCharacterAttack");
            DumpTypeChain(sb, attack);
            DumpDeclaredMethods(sb, attack);
            if (attack != null)
            {
                foreach (Type nested in attack.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
                {
                    sb.AppendLine("--- NESTED ATTACK TYPE ---");
                    DumpTypeChain(sb, nested);
                    DumpDeclaredMethods(sb, nested);
                }
            }
            sb.AppendLine();

            sb.AppendLine("=== COMBAT: STRING/MEMBER USERS ===");
            DumpMethodsMatching(sb, allTypes, method =>
            {
                List<Instruction> il;
                try { il = Read(method); } catch { return false; }
                return il.Any(i => string.Equals(i.StringValue, "ATTACK!!!", StringComparison.Ordinal) ||
                                   string.Equals(i.StringValue, "ATTACK STATE!!!!", StringComparison.Ordinal) ||
                                   string.Equals(i.StringValue, "damage", StringComparison.Ordinal) ||
                                   (i.Member != null && i.Member.DeclaringType != null &&
                                    string.Equals(i.Member.DeclaringType.Name, "BaseCharacterAttack", StringComparison.Ordinal)));
            }, 120);
            sb.AppendLine();

            Type baseCharacter = FindType(allTypes, "BaseCharacterComponent");
            sb.AppendLine("=== COMBAT: BASE CHARACTER METHODS BY NAME ===");
            if (baseCharacter != null)
            {
                foreach (MethodInfo method in baseCharacter.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .Where(m => ContainsAny(m.Name, "attack", "damage", "hit", "weapon", "sword"))
                    .OrderBy(m => m.MetadataToken))
                    DumpMethod(sb, method, "NAME_MATCH");
            }
            sb.AppendLine();
        }

        private static void DumpRepose(StringBuilder sb, Type[] allTypes)
        {
            sb.AppendLine("=== REPOSE: FLOW_DROPBODY TYPE/EXECUTION ===");
            Type drop = FindType(allTypes, "Flow_DropBody");
            DumpTypeChain(sb, drop);
            DumpDeclaredMethods(sb, drop);
            if (drop != null)
            {
                foreach (Type nested in drop.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
                {
                    sb.AppendLine("--- NESTED DROP TYPE ---");
                    DumpTypeChain(sb, nested);
                    DumpDeclaredMethods(sb, nested);
                }
            }
            sb.AppendLine();

            foreach (string typeName in new[] { "MyFlowNode", "FlowNode", "Node" })
            {
                Type t = FindPreferredType(allTypes, typeName);
                sb.AppendLine("=== REPOSE: BASE SCHEMA " + typeName + " ===");
                DumpTypeChain(sb, t);
                sb.AppendLine();
            }

            sb.AppendLine("=== REPOSE: LOADED NODE INSTANCES ===");
            if (drop != null && typeof(UnityEngine.Object).IsAssignableFrom(drop))
            {
                try
                {
                    UnityEngine.Object[] objects = Resources.FindObjectsOfTypeAll(drop);
                    sb.AppendLine("Resources.FindObjectsOfTypeAll count=" + objects.Length);
                    foreach (UnityEngine.Object obj in objects) DumpObjectIdentity(sb, obj);
                }
                catch (Exception ex) { sb.AppendLine("Resources scan failed=" + ex.GetType().Name + ":" + ex.Message); }
            }
            else sb.AppendLine("Flow_DropBody is not a UnityEngine.Object; Resources scan skipped.");

            DumpNpcDonkeyGraphNodes(sb, allTypes, drop);
            sb.AppendLine();
        }

        private static void DumpNpcDonkeyGraphNodes(StringBuilder sb, Type[] allTypes, Type dropType)
        {
            Type customFlow = FindType(allTypes, "CustomFlowScript");
            MethodInfo getGraph = customFlow == null ? null : customFlow.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == "GetGraph" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(string));
            if (getGraph == null)
            {
                sb.AppendLine("npc_donkey GetGraph(string) unavailable");
                return;
            }

            object graph = null;
            try { graph = getGraph.Invoke(null, new object[] { "npc_donkey" }); }
            catch (Exception ex) { sb.AppendLine("npc_donkey GetGraph failed=" + ex.GetType().Name + ":" + ex.Message); }
            if (graph == null)
            {
                sb.AppendLine("npc_donkey graph=null");
                return;
            }

            sb.AppendLine("npc_donkey graphType=" + graph.GetType().FullName + " unityName=" + UnityName(graph));
            DumpObjectIdentity(sb, graph);

            HashSet<object> seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
            int candidates = 0;
            foreach (object value in EnumerateMembers(graph))
            {
                IEnumerable enumerable = value as IEnumerable;
                if (enumerable == null || value is string) continue;
                foreach (object node in SafeEnumerate(enumerable))
                {
                    if (node == null || !seen.Add(node)) continue;
                    Type nt = node.GetType();
                    if (dropType != null && dropType.IsAssignableFrom(nt))
                    {
                        candidates++;
                        sb.AppendLine("NPC_DONKEY_DROP_NODE #" + candidates);
                        DumpObjectIdentity(sb, node);
                    }
                }
            }
            sb.AppendLine("npc_donkey dropNodeCandidates=" + candidates);
        }

        private static IEnumerable<object> EnumerateMembers(object obj)
        {
            if (obj == null) yield break;
            const BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            for (Type t = obj.GetType(); t != null; t = t.BaseType)
            {
                foreach (FieldInfo field in t.GetFields(f | BindingFlags.DeclaredOnly))
                {
                    object value = null;
                    try { value = field.GetValue(obj); } catch { }
                    if (value != null) yield return value;
                }
                foreach (PropertyInfo prop in t.GetProperties(f | BindingFlags.DeclaredOnly))
                {
                    if (!prop.CanRead || prop.GetIndexParameters().Length != 0) continue;
                    object value = null;
                    try { value = prop.GetValue(obj, null); } catch { }
                    if (value != null) yield return value;
                }
            }
        }

        private static IEnumerable<object> SafeEnumerate(IEnumerable enumerable)
        {
            IEnumerator e = null;
            try { e = enumerable.GetEnumerator(); } catch { yield break; }
            if (e == null) yield break;
            while (true)
            {
                bool moved;
                try { moved = e.MoveNext(); } catch { yield break; }
                if (!moved) yield break;
                object current = null;
                try { current = e.Current; } catch { }
                if (current != null) yield return current;
            }
        }

        private static void DumpObjectIdentity(StringBuilder sb, object obj)
        {
            if (obj == null) { sb.AppendLine("  <null>"); return; }
            sb.AppendLine("  objectType=" + obj.GetType().FullName + " unityName=" + UnityName(obj));
            const BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            for (Type t = obj.GetType(); t != null; t = t.BaseType)
            {
                foreach (FieldInfo field in t.GetFields(f | BindingFlags.DeclaredOnly)
                    .Where(x => ContainsAny(x.Name, "id", "graph", "owner", "agent", "name")))
                {
                    string value;
                    try { value = FormatValue(field.GetValue(obj)); } catch (Exception ex) { value = "<" + ex.GetType().Name + ">"; }
                    sb.AppendLine("  FIELD " + t.FullName + "." + field.Name + " : " + field.FieldType.FullName + " = " + value);
                }
                foreach (PropertyInfo prop in t.GetProperties(f | BindingFlags.DeclaredOnly)
                    .Where(x => x.CanRead && x.GetIndexParameters().Length == 0 && ContainsAny(x.Name, "id", "graph", "owner", "agent", "name")))
                {
                    string value;
                    try { value = FormatValue(prop.GetValue(obj, null)); } catch (Exception ex) { value = "<" + ex.GetType().Name + ">"; }
                    sb.AppendLine("  PROP  " + t.FullName + "." + prop.Name + " : " + prop.PropertyType.FullName + " = " + value);
                }
            }
        }

        private static string FormatValue(object value)
        {
            if (value == null) return "<null>";
            if (value is string) return Quote((string)value);
            if (value is UnityEngine.Object) return value.GetType().FullName + "#" + ((UnityEngine.Object)value).GetInstanceID() + " name=" + Quote(((UnityEngine.Object)value).name);
            Type t = value.GetType();
            if (t.IsPrimitive || t.IsEnum || value is decimal) return Convert.ToString(value, CultureInfo.InvariantCulture);
            return t.FullName;
        }

        private static void DumpTypeChain(StringBuilder sb, Type type)
        {
            if (type == null) { sb.AppendLine("TYPE <missing>"); return; }
            for (Type t = type; t != null; t = t.BaseType)
            {
                sb.AppendLine("TYPE " + t.FullName + " assembly=" + t.Assembly.GetName().Name);
                const BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
                foreach (FieldInfo field in t.GetFields(f).OrderBy(x => x.Name))
                    sb.AppendLine("  FIELD " + (field.IsStatic ? "static " : "instance ") + TypeName(field.FieldType) + " " + field.Name);
                foreach (PropertyInfo prop in t.GetProperties(f).OrderBy(x => x.Name))
                    sb.AppendLine("  PROPERTY " + TypeName(prop.PropertyType) + " " + prop.Name + " read=" + prop.CanRead + " write=" + prop.CanWrite);
                foreach (MethodInfo method in t.GetMethods(f).OrderBy(x => x.MetadataToken))
                    sb.AppendLine("  METHOD " + Signature(method));
            }
        }

        private static void DumpDeclaredMethods(StringBuilder sb, Type type)
        {
            if (type == null) return;
            const BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            foreach (MethodInfo method in type.GetMethods(f).OrderBy(m => m.MetadataToken))
                DumpMethod(sb, method, "DECLARED");
        }

        private static void DumpMethodsMatching(StringBuilder sb, Type[] types, Func<MethodBase, bool> predicate, int max)
        {
            int count = 0;
            foreach (Type type in types)
            {
                foreach (MethodBase method in GetDeclaredMethods(type))
                {
                    bool match;
                    try { match = predicate(method); } catch { continue; }
                    if (!match) continue;
                    DumpMethod(sb, method, "MATCH");
                    count++;
                    if (count >= max)
                    {
                        sb.AppendLine("MATCH_LIMIT_REACHED=" + max);
                        return;
                    }
                }
            }
            sb.AppendLine("MATCH_COUNT=" + count);
        }

        private static IEnumerable<MethodBase> GetDeclaredMethods(Type type)
        {
            if (type == null) yield break;
            const BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            foreach (ConstructorInfo c in type.GetConstructors(f)) yield return c;
            foreach (MethodInfo m in type.GetMethods(f)) yield return m;
        }

        private static void DumpMethod(StringBuilder sb, MethodBase method, string label)
        {
            if (method == null) return;
            sb.AppendLine(label + " METHOD " + Signature(method));
            List<Instruction> il;
            try { il = Read(method); }
            catch (Exception ex)
            {
                sb.AppendLine("  IL_READ_ERROR " + ex.GetType().Name + ":" + ex.Message);
                return;
            }
            foreach (Instruction i in il)
                sb.AppendLine("  IL_" + i.Offset.ToString("X4", CultureInfo.InvariantCulture) + " " + i.OpCode.Name + (string.IsNullOrEmpty(i.Text) ? "" : " " + i.Text));
        }

        private static string Signature(MethodBase method)
        {
            string owner = method.DeclaringType == null ? "<null>" : method.DeclaringType.FullName;
            string ret = method is MethodInfo ? TypeName(((MethodInfo)method).ReturnType) : "System.Void";
            string pars = string.Join(", ", method.GetParameters().Select(p => TypeName(p.ParameterType) + " " + p.Name).ToArray());
            return ret + " " + owner + "." + method.Name + "(" + pars + ") token=0x" + method.MetadataToken.ToString("X8", CultureInfo.InvariantCulture);
        }

        private static string TypeName(Type type)
        {
            return type == null ? "<null>" : (type.FullName ?? type.Name);
        }

        private static List<Instruction> Read(MethodBase method)
        {
            List<Instruction> result = new List<Instruction>();
            MethodBody body = method.GetMethodBody();
            if (body == null) return result;
            byte[] bytes = body.GetILAsByteArray();
            if (bytes == null) return result;

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
                    case OperandType.ShortInlineI: text = ((sbyte)bytes[p++]).ToString(CultureInfo.InvariantCulture); break;
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
                    case OperandType.InlineSwitch:
                    {
                        int n = BitConverter.ToInt32(bytes, p); p += 4;
                        int basePos = p + n * 4;
                        string[] labels = new string[n];
                        for (int j = 0; j < n; j++) labels[j] = "IL_" + (basePos + BitConverter.ToInt32(bytes, p + j * 4)).ToString("X4", CultureInfo.InvariantCulture);
                        p += n * 4;
                        text = string.Join(",", labels);
                        break;
                    }
                    case OperandType.InlineString:
                    {
                        int token = BitConverter.ToInt32(bytes, p); p += 4;
                        try { str = method.Module.ResolveString(token); text = Quote(str); }
                        catch { text = "stringToken=0x" + token.ToString("X8", CultureInfo.InvariantCulture); }
                        break;
                    }
                    case OperandType.InlineField:
                    case OperandType.InlineMethod:
                    case OperandType.InlineType:
                    case OperandType.InlineTok:
                    {
                        int token = BitConverter.ToInt32(bytes, p); p += 4;
                        try
                        {
                            Type[] gt = method.DeclaringType == null || !method.DeclaringType.IsGenericType ? null : method.DeclaringType.GetGenericArguments();
                            Type[] gm = method is MethodInfo && ((MethodInfo)method).IsGenericMethod ? ((MethodInfo)method).GetGenericArguments() : null;
                            member = method.Module.ResolveMember(token, gt, gm);
                            text = MemberText(member);
                        }
                        catch { text = "memberToken=0x" + token.ToString("X8", CultureInfo.InvariantCulture); }
                        break;
                    }
                    case OperandType.InlineSig:
                    {
                        int token = BitConverter.ToInt32(bytes, p); p += 4;
                        text = "sigToken=0x" + token.ToString("X8", CultureInfo.InvariantCulture);
                        break;
                    }
                    default:
                        throw new NotSupportedException("OperandType " + op.OperandType);
                }

                result.Add(new Instruction(offset, op, text, member, str));
            }
            return result;
        }

        private static string MemberText(MemberInfo member)
        {
            if (member == null) return "<null-member>";
            MethodBase mb = member as MethodBase;
            if (mb != null) return Signature(mb);
            FieldInfo fi = member as FieldInfo;
            if (fi != null) return TypeName(fi.FieldType) + " " + TypeName(fi.DeclaringType) + "." + fi.Name;
            Type t = member as Type;
            if (t != null) return TypeName(t);
            return member.ToString();
        }

        private static bool ContainsAny(string value, params string[] needles)
        {
            if (string.IsNullOrEmpty(value)) return false;
            foreach (string needle in needles)
                if (value.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        private static string UnityName(object obj)
        {
            UnityEngine.Object uo = obj as UnityEngine.Object;
            return uo == null ? "<not-unity-object>" : uo.name;
        }

        private static string Quote(string value)
        {
            if (value == null) return "<null>";
            return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n") + "\"";
        }

        private static Type FindType(Type[] types, string name)
        {
            return types.FirstOrDefault(t => t != null && (string.Equals(t.Name, name, StringComparison.Ordinal) || string.Equals(t.FullName, name, StringComparison.Ordinal)));
        }

        private static Type FindPreferredType(Type[] types, string name)
        {
            Type[] matches = types.Where(t => t != null && string.Equals(t.Name, name, StringComparison.Ordinal)).ToArray();
            return matches.OrderByDescending(t => t.Namespace != null && (t.Namespace.StartsWith("NodeCanvas", StringComparison.Ordinal) || t.Namespace.StartsWith("FlowCanvas", StringComparison.Ordinal)))
                .ThenBy(t => t.FullName)
                .FirstOrDefault();
        }

        private static Type[] SafeTypes(Assembly assembly)
        {
            try { return assembly.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null).ToArray(); }
            catch { return new Type[0]; }
        }

        private static bool IsGameStarted(Assembly game)
        {
            Type mainGame = SafeTypes(game).FirstOrDefault(t => t != null && t.Name == "MainGame");
            if (mainGame == null) return false;
            FieldInfo field = mainGame.GetField("game_started", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (field == null || field.FieldType != typeof(bool)) return false;
            try { return (bool)field.GetValue(null); } catch { return false; }
        }

        private sealed class Instruction
        {
            internal readonly int Offset;
            internal readonly OpCode OpCode;
            internal readonly string Text;
            internal readonly MemberInfo Member;
            internal readonly string StringValue;

            internal Instruction(int offset, OpCode opCode, string text, MemberInfo member, string stringValue)
            {
                Offset = offset;
                OpCode = opCode;
                Text = text;
                Member = member;
                StringValue = stringValue;
            }
        }

        private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            internal static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();
            public new bool Equals(object x, object y) { return ReferenceEquals(x, y); }
            public int GetHashCode(object obj) { return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj); }
        }
    }
}
