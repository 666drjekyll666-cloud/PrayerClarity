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
    public sealed class PrayerClarityVisualSchedulerProbe : BaseUnityPlugin
    {
        public const string PluginGuid = "prayerclarity.research.visualscheduler";
        public const string PluginName = "PrayerClarity Visual FX and Scheduler Research Probe";
        public const string PluginVersion = "0.1.0";

        private bool _completed;
        private float _readyAt = -1f;
        private static readonly OpCode[] OneByte = new OpCode[256];
        private static readonly OpCode[] TwoByte = new OpCode[256];
        private static readonly string[] VisualNeedles =
        {
            "effect", "particle", "aura", "glow", "light", "puff", "flying", "pray", "bless", "holy", "fire"
        };

        static PrayerClarityVisualSchedulerProbe()
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
            Logger.LogInfo("PrayerClarity Visual FX/Scheduler Probe 0.1.0 loaded: read-only reflection/IL/loaded-object inspection; no Harmony, no effect activation, no mutation.");
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

            if (Time.realtimeSinceStartup - _readyAt < 6f) return;

            _completed = true;
            try { Run(game); }
            catch (Exception ex) { Logger.LogError("PrayerClarity visual/scheduler audit failed: " + ex); }
        }

        private static bool GameStarted(Assembly game)
        {
            Type t = SafeTypes(game).FirstOrDefault(x => x != null && x.Name == "MainGame");
            if (t == null) return false;
            FieldInfo f = t.GetField("game_started", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            try { return f != null && f.FieldType == typeof(bool) && (bool)f.GetValue(null); }
            catch { return false; }
        }

        private void Run(Assembly game)
        {
            Type[] gameTypes = SafeTypes(game);
            Type[] allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(SafeTypes).ToArray();
            StringBuilder sb = new StringBuilder(1024 * 1024);

            sb.AppendLine("PRAYERCLARITY — VISUAL FX / LOGIC SCHEDULER NARROW RESEARCH AUDIT");
            sb.AppendLine("ProbeVersion=" + PluginVersion);
            sb.AppendLine("GeneratedUtc=" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            sb.AppendLine("GameAssembly=" + game.FullName);
            sb.AppendLine("ModuleVersionId=" + game.ManifestModule.ModuleVersionId);
            sb.AppendLine("Contract=READ_ONLY_REFLECTION_IL_LOADED_OBJECT_INSPECTION_NO_HARMONY_NO_ACTIVATION_NO_MUTATION");
            sb.AppendLine("Questions=What are the exact LogicDefinition start_time/period_time scheduler semantics; which native visual FX/player anchors/flying-object seams can be reused cheaply for prayer polish?");
            sb.AppendLine();

            DumpLogicSchedulerConsumers(sb, gameTypes);
            DumpPrayBuffVisualMethods(sb, gameTypes);
            DumpVisualTypeCandidates(sb, gameTypes);
            DumpPlayerHierarchy(sb, gameTypes);
            DumpLoadedParticleSystems(sb, allTypes);
            DumpLoadedVisualNamedObjects(sb);

            string path = Path.Combine(Paths.BepInExRootPath, "PrayerClarity-visual-scheduler-audit-0.1.0.txt");
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
            Logger.LogInfo("PrayerClarity visual/scheduler research audit complete: " + path);
        }

        private static void DumpLogicSchedulerConsumers(StringBuilder sb, Type[] gameTypes)
        {
            sb.AppendLine("=== LOGICDEFINITION SCHEDULER CONSUMERS ===");
            Type logic = gameTypes.FirstOrDefault(t => t != null && t.Name == "LogicDefinition");
            if (logic == null)
            {
                sb.AppendLine("LogicDefinition missing");
                sb.AppendLine("=== END LOGICDEFINITION SCHEDULER CONSUMERS ===");
                return;
            }

            const BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            FieldInfo start = logic.GetField("start_time", f);
            FieldInfo period = logic.GetField("period_time", f);
            sb.AppendLine("LogicDefinition=" + logic.FullName);
            sb.AppendLine("start_time=" + FieldSig(start));
            sb.AppendLine("period_time=" + FieldSig(period));

            int hits = 0;
            foreach (Type type in gameTypes)
            {
                if (type == null) continue;
                foreach (MethodBase method in GetDeclaredMethods(type))
                {
                    List<Instruction> il;
                    try { il = Read(method); }
                    catch { continue; }

                    bool hit = il.Any(i => i.Member is FieldInfo &&
                        ((start != null && SameField((FieldInfo)i.Member, start)) ||
                         (period != null && SameField((FieldInfo)i.Member, period))));
                    if (!hit) continue;

                    hits++;
                    DumpMethod(sb, method, il);
                }
            }
            sb.AppendLine("METHOD_HIT_COUNT=" + hits);
            sb.AppendLine("=== END LOGICDEFINITION SCHEDULER CONSUMERS ===");
            sb.AppendLine();
        }

        private static void DumpPrayBuffVisualMethods(StringBuilder sb, Type[] gameTypes)
        {
            sb.AppendLine("=== PRAYER / BUFF / FLYING VISUAL METHOD CANDIDATES ===");
            int hits = 0;
            foreach (Type type in gameTypes)
            {
                if (type == null) continue;
                foreach (MethodBase method in GetDeclaredMethods(type))
                {
                    string hay = ((type.FullName ?? type.Name) + "." + method.Name).ToLowerInvariant();
                    if (!(hay.Contains("pray") || hay.Contains("buff") || hay.Contains("flying"))) continue;
                    if (!(hay.Contains("create") || hay.Contains("effect") || hay.Contains("flying") || hay.Contains("buff"))) continue;

                    List<Instruction> il;
                    try { il = Read(method); }
                    catch { il = new List<Instruction>(); }

                    bool especiallyRelevant = method.Name.IndexOf("CreatePrayBuffFlyingObject", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                              method.Name.IndexOf("Flying", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                              method.Name.IndexOf("Buff", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (!especiallyRelevant) continue;

                    hits++;
                    DumpMethod(sb, method, il);
                }
            }
            sb.AppendLine("METHOD_HIT_COUNT=" + hits);
            sb.AppendLine("=== END PRAYER / BUFF / FLYING VISUAL METHOD CANDIDATES ===");
            sb.AppendLine();
        }

        private static void DumpVisualTypeCandidates(StringBuilder sb, Type[] gameTypes)
        {
            sb.AppendLine("=== GAME VISUAL TYPE CANDIDATES ===");
            int count = 0;
            foreach (Type type in gameTypes.OrderBy(t => t == null ? "" : t.FullName))
            {
                if (type == null) continue;
                string name = (type.FullName ?? type.Name).ToLowerInvariant();
                if (!VisualNeedles.Any(n => name.Contains(n))) continue;
                if (!(name.Contains("effect") || name.Contains("particle") || name.Contains("aura") || name.Contains("glow") || name.Contains("light") || name.Contains("puff") || name.Contains("flying"))) continue;

                count++;
                sb.AppendLine("TYPE " + (type.FullName ?? type.Name) + " base=" + (type.BaseType == null ? "<null>" : type.BaseType.FullName));
                const BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
                foreach (FieldInfo field in type.GetFields(f).Take(40)) sb.AppendLine("  FIELD " + FieldSig(field));
                foreach (MethodInfo method in type.GetMethods(f).Where(m => VisualNeedles.Any(n => m.Name.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0)).Take(40))
                    sb.AppendLine("  METHOD " + Signature(method));
            }
            sb.AppendLine("TYPE_COUNT=" + count);
            sb.AppendLine("=== END GAME VISUAL TYPE CANDIDATES ===");
            sb.AppendLine();
        }

        private static void DumpPlayerHierarchy(StringBuilder sb, Type[] gameTypes)
        {
            sb.AppendLine("=== PLAYER HIERARCHY / COMPONENTS ===");
            Type main = gameTypes.FirstOrDefault(t => t != null && t.Name == "MainGame");
            object me = GetStaticMember(main, "me");
            object player = GetMember(me, "player");
            sb.AppendLine("MainGame.me=" + DescribeObject(me));
            sb.AppendLine("player=" + DescribeObject(player));

            GameObject go = ToGameObject(player);
            if (go == null)
            {
                sb.AppendLine("player GameObject unavailable");
            }
            else
            {
                DumpTransform(sb, go.transform, 0, 7);
            }
            sb.AppendLine("=== END PLAYER HIERARCHY / COMPONENTS ===");
            sb.AppendLine();
        }

        private static void DumpLoadedParticleSystems(StringBuilder sb, Type[] allTypes)
        {
            sb.AppendLine("=== LOADED PARTICLE SYSTEMS ===");
            Type particleType = allTypes.FirstOrDefault(t => t != null && t.FullName == "UnityEngine.ParticleSystem");
            if (particleType == null || !typeof(UnityEngine.Object).IsAssignableFrom(particleType))
            {
                sb.AppendLine("UnityEngine.ParticleSystem type unavailable");
                sb.AppendLine("=== END LOADED PARTICLE SYSTEMS ===");
                return;
            }

            UnityEngine.Object[] objects;
            try { objects = Resources.FindObjectsOfTypeAll(particleType); }
            catch (Exception ex)
            {
                sb.AppendLine("scan failed=" + ex.GetType().Name + ":" + ex.Message);
                sb.AppendLine("=== END LOADED PARTICLE SYSTEMS ===");
                return;
            }

            sb.AppendLine("COUNT=" + objects.Length);
            int i = 0;
            foreach (UnityEngine.Object obj in objects.Take(500))
            {
                i++;
                Component c = obj as Component;
                sb.AppendLine("PARTICLE #" + i + " name=" + Quote(obj == null ? "<null>" : obj.name) +
                              " path=" + Quote(c == null ? "" : TransformPath(c.transform)) +
                              " active=" + (c == null || c.gameObject == null ? "?" : c.gameObject.activeInHierarchy.ToString()));
                if (c != null)
                {
                    Component[] comps;
                    try { comps = c.gameObject.GetComponents<Component>(); }
                    catch { comps = new Component[0]; }
                    sb.AppendLine("  sibling_components=" + string.Join(" | ", comps.Where(x => x != null).Select(x => x.GetType().FullName).ToArray()));
                }
                DumpSimpleVisualMembers(sb, obj, "  ");
            }
            sb.AppendLine("=== END LOADED PARTICLE SYSTEMS ===");
            sb.AppendLine();
        }

        private static void DumpLoadedVisualNamedObjects(StringBuilder sb)
        {
            sb.AppendLine("=== LOADED VISUAL-NAMED GAMEOBJECT CANDIDATES ===");
            GameObject[] objects;
            try { objects = Resources.FindObjectsOfTypeAll<GameObject>(); }
            catch (Exception ex)
            {
                sb.AppendLine("scan failed=" + ex.GetType().Name + ":" + ex.Message);
                sb.AppendLine("=== END LOADED VISUAL-NAMED GAMEOBJECT CANDIDATES ===");
                return;
            }

            List<GameObject> matches = objects.Where(go => go != null && VisualName(go.name)).Take(750).ToList();
            sb.AppendLine("LOADED_GAMEOBJECTS=" + objects.Length + " MATCHES=" + matches.Count);
            int i = 0;
            foreach (GameObject go in matches)
            {
                i++;
                Component[] comps;
                try { comps = go.GetComponents<Component>(); }
                catch { comps = new Component[0]; }
                sb.AppendLine("GO #" + i + " name=" + Quote(go.name) + " path=" + Quote(TransformPath(go.transform)) +
                              " activeSelf=" + go.activeSelf + " activeInHierarchy=" + go.activeInHierarchy);
                sb.AppendLine("  components=" + string.Join(" | ", comps.Where(c => c != null).Select(c => c.GetType().FullName).ToArray()));
            }
            sb.AppendLine("=== END LOADED VISUAL-NAMED GAMEOBJECT CANDIDATES ===");
        }

        private static bool VisualName(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            string lower = name.ToLowerInvariant();
            return VisualNeedles.Any(n => lower.Contains(n));
        }

        private static void DumpTransform(StringBuilder sb, Transform t, int depth, int maxDepth)
        {
            if (t == null || depth > maxDepth) return;
            string indent = new string(' ', depth * 2);
            Component[] comps;
            try { comps = t.gameObject.GetComponents<Component>(); }
            catch { comps = new Component[0]; }
            sb.AppendLine(indent + "GO name=" + Quote(t.gameObject.name) + " path=" + Quote(TransformPath(t)) +
                          " active=" + t.gameObject.activeInHierarchy +
                          " components=" + string.Join(" | ", comps.Where(c => c != null).Select(c => c.GetType().FullName).ToArray()));
            for (int i = 0; i < t.childCount; i++) DumpTransform(sb, t.GetChild(i), depth + 1, maxDepth);
        }

        private static void DumpSimpleVisualMembers(StringBuilder sb, object obj, string indent)
        {
            if (obj == null) return;
            Type type = obj.GetType();
            const BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            string[] needles = { "loop", "duration", "playonawake", "material", "sorting", "emission", "startcolor", "startsize" };
            foreach (PropertyInfo p in type.GetProperties(f))
            {
                if (p.GetIndexParameters().Length != 0 || !p.CanRead) continue;
                if (!needles.Any(n => p.Name.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0)) continue;
                try
                {
                    object value = p.GetValue(obj, null);
                    sb.AppendLine(indent + "PROP " + p.Name + "=" + DescribeSimple(value));
                }
                catch { }
            }
        }

        private static object GetStaticMember(Type type, string name)
        {
            if (type == null) return null;
            const BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            try
            {
                PropertyInfo p = type.GetProperty(name, f);
                if (p != null && p.GetIndexParameters().Length == 0) return p.GetValue(null, null);
                FieldInfo field = type.GetField(name, f);
                if (field != null) return field.GetValue(null);
            }
            catch { }
            return null;
        }

        private static object GetMember(object obj, string name)
        {
            if (obj == null) return null;
            Type type = obj.GetType();
            const BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            try
            {
                PropertyInfo p = type.GetProperty(name, f);
                if (p != null && p.GetIndexParameters().Length == 0) return p.GetValue(obj, null);
                FieldInfo field = type.GetField(name, f);
                if (field != null) return field.GetValue(obj);
            }
            catch { }
            return null;
        }

        private static GameObject ToGameObject(object obj)
        {
            GameObject go = obj as GameObject;
            if (go != null) return go;
            Component c = obj as Component;
            if (c != null) return c.gameObject;

            object nested = GetMember(obj, "gameObject");
            go = nested as GameObject;
            if (go != null) return go;
            c = nested as Component;
            return c == null ? null : c.gameObject;
        }

        private static string TransformPath(Transform t)
        {
            if (t == null) return "";
            List<string> parts = new List<string>();
            int guard = 0;
            while (t != null && guard++ < 64)
            {
                parts.Add(t.gameObject == null ? "<null>" : t.gameObject.name);
                t = t.parent;
            }
            parts.Reverse();
            return string.Join("/", parts.ToArray());
        }

        private static string DescribeObject(object obj)
        {
            if (obj == null) return "<null>";
            UnityEngine.Object u = obj as UnityEngine.Object;
            return obj.GetType().FullName + (u == null ? "" : " name=" + Quote(u.name));
        }

        private static string DescribeSimple(object value)
        {
            if (value == null) return "<null>";
            UnityEngine.Object u = value as UnityEngine.Object;
            if (u != null) return value.GetType().FullName + " name=" + Quote(u.name);
            if (value is string) return Quote((string)value);
            try { return Convert.ToString(value, CultureInfo.InvariantCulture); }
            catch { return value.GetType().FullName; }
        }

        private static bool SameField(FieldInfo a, FieldInfo b)
        {
            if (a == null || b == null) return false;
            return a.Module == b.Module && a.MetadataToken == b.MetadataToken;
        }

        private static string FieldSig(FieldInfo field)
        {
            if (field == null) return "<missing>";
            return (field.FieldType.FullName ?? field.FieldType.Name) + " " +
                   (field.DeclaringType == null ? "<global>" : field.DeclaringType.FullName) + "." + field.Name +
                   " token=0x" + field.MetadataToken.ToString("X8", CultureInfo.InvariantCulture);
        }

        private static void DumpMethod(StringBuilder sb, MethodBase method, List<Instruction> il)
        {
            sb.AppendLine("METHOD " + Signature(method));
            foreach (Instruction ins in il)
            {
                sb.Append("  IL_").Append(ins.Offset.ToString("X4", CultureInfo.InvariantCulture)).Append(' ')
                  .Append(ins.Op.Name).Append(' ').AppendLine(ins.Text ?? "");
            }
        }

        private static string Signature(MethodBase method)
        {
            string owner = method.DeclaringType == null ? "<global>" : method.DeclaringType.FullName;
            string ret = method is MethodInfo ? (((MethodInfo)method).ReturnType.FullName ?? ((MethodInfo)method).ReturnType.Name) : "System.Void";
            string pars = string.Join(", ", method.GetParameters().Select(p => (p.ParameterType.FullName ?? p.ParameterType.Name) + " " + p.Name).ToArray());
            return ret + " " + owner + "." + method.Name + "(" + pars + ") token=0x" + method.MetadataToken.ToString("X8", CultureInfo.InvariantCulture);
        }

        private static MethodBase[] GetDeclaredMethods(Type type)
        {
            if (type == null) return new MethodBase[0];
            const BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            try { return type.GetMethods(f).Cast<MethodBase>().Concat(type.GetConstructors(f).Cast<MethodBase>()).ToArray(); }
            catch { return new MethodBase[0]; }
        }

        private static Type[] SafeTypes(Assembly assembly)
        {
            if (assembly == null) return new Type[0];
            try { return assembly.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null).ToArray(); }
            catch { return new Type[0]; }
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
                            member = op.OperandType == OperandType.InlineField ? module.ResolveField(token, ta, ma)
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

                result.Add(new Instruction { Offset = offset, Op = op, Text = text, Member = member });
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
            public MemberInfo Member;
        }
    }
}
