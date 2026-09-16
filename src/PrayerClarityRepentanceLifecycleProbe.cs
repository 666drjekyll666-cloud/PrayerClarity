using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using UnityEngine;

namespace PrayerClarityResearch
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class PrayerClarityRepentanceLifecycleProbe : BaseUnityPlugin
    {
        public const string PluginGuid = "prayerclarity.research.repentance.lifecycle";
        public const string PluginName = "PrayerClarity Repentance Lifecycle Probe";
        public const string PluginVersion = "0.1.1";

        private static readonly string[] Needles =
        {
            "church_budka_roll",
            "confession_available",
            "confession_probability",
            "church_budka",
            "confession"
        };

        private bool _completed;
        private float _readyAt = -1f;

        private void Awake()
        {
            Logger.LogInfo("PrayerClarity Repentance Lifecycle Probe 0.1.1 loaded: read-only loaded-graph/GameBalance inspection; no Harmony, graph execution, or intentional game/save mutation.");
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
            catch (Exception ex) { Logger.LogError("PrayerClarity Repentance lifecycle probe failed: " + ex); }
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
            Type[] allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(SafeTypes).ToArray();
            StringBuilder sb = new StringBuilder(1024 * 1024);

            sb.AppendLine("PRAYERCLARITY — REPENTANCE LIFECYCLE / REWARD FOLLOW-UP AUDIT");
            sb.AppendLine("ProbeVersion=" + PluginVersion);
            sb.AppendLine("GeneratedUtc=" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            sb.AppendLine("GameAssembly=" + game.FullName);
            sb.AppendLine("ModuleVersionId=" + game.ManifestModule.ModuleVersionId);
            sb.AppendLine("Contract=READ_ONLY_LOADED_GRAPH_GAMEBALANCE_INSPECTION_NO_HARMONY_NO_EXECUTION_NO_MUTATION");
            sb.AppendLine("Question=Which loaded graph invokes church_budka_roll, what lifecycle/cadence does that imply, and what graph/data handles confession_available rewards?");
            sb.AppendLine();

            DumpLoadedGraphs(sb, allTypes);
            DumpGameBalance(sb, allTypes);
            DumpPlayerConfessionProbability(sb, allTypes);

            string path = Path.Combine(Paths.BepInExRootPath, "PrayerClarity-repentance-lifecycle-0.1.1.txt");
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
            Logger.LogInfo("PrayerClarity Repentance lifecycle audit complete: " + path);
        }

        private static void DumpLoadedGraphs(StringBuilder sb, Type[] allTypes)
        {
            sb.AppendLine("=== LOADED FLOW GRAPHS ===");
            Type graphType = allTypes.FirstOrDefault(t => t != null && t.FullName == "NodeCanvas.Framework.Graph");
            if (graphType == null || !typeof(UnityEngine.Object).IsAssignableFrom(graphType))
            {
                sb.AppendLine("Graph type unavailable");
                sb.AppendLine("=== END LOADED FLOW GRAPHS ===");
                return;
            }

            UnityEngine.Object[] graphs;
            try { graphs = Resources.FindObjectsOfTypeAll(graphType); }
            catch (Exception ex)
            {
                sb.AppendLine("Graph enumeration failed=" + ex.GetType().Name + ":" + ex.Message);
                sb.AppendLine("=== END LOADED FLOW GRAPHS ===");
                return;
            }

            int hitCount = 0;
            foreach (UnityEngine.Object graph in graphs.OrderBy(g => g == null ? "" : g.name, StringComparer.Ordinal))
            {
                if (graph == null) continue;
                string serialized = GetSerializedGraph(graph);
                if (string.IsNullOrEmpty(serialized)) continue;

                string[] hits = Needles.Where(n => serialized.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
                if (hits.Length == 0) continue;

                hitCount++;
                sb.AppendLine("GRAPH_HIT name=" + Quote(graph.name) + " type=" + graph.GetType().FullName + " length=" + serialized.Length + " needles=" + string.Join(" | ", hits.Select(Quote).ToArray()));
                sb.AppendLine("GRAPH_SERIALIZED_BEGIN name=" + Quote(graph.name));
                sb.AppendLine(serialized);
                sb.AppendLine("GRAPH_SERIALIZED_END name=" + Quote(graph.name));
                sb.AppendLine();
            }

            sb.AppendLine("GRAPH_SUMMARY loaded=" + graphs.Length + " hits=" + hitCount);
            sb.AppendLine("=== END LOADED FLOW GRAPHS ===");
            sb.AppendLine();
        }

        private static void DumpGameBalance(StringBuilder sb, Type[] allTypes)
        {
            sb.AppendLine("=== GAMEBALANCE KEYWORD HITS ===");
            Type gameBalanceType = allTypes.FirstOrDefault(t => t != null && t.Name == "GameBalance");
            Type smartType = allTypes.FirstOrDefault(t => t != null && t.Name == "SmartExpression");
            MethodInfo rawExpression = smartType == null ? null : smartType.GetMethod("GetRawExpressionString", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (gameBalanceType == null)
            {
                sb.AppendLine("GameBalance missing");
                sb.AppendLine("=== END GAMEBALANCE KEYWORD HITS ===");
                return;
            }

            object gameBalance = null;
            try
            {
                PropertyInfo me = gameBalanceType.GetProperty("me", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (me != null) gameBalance = me.GetValue(null, null);
            }
            catch { }

            if (gameBalance == null)
            {
                sb.AppendLine("GameBalance.me unavailable");
                sb.AppendLine("=== END GAMEBALANCE KEYWORD HITS ===");
                return;
            }

            int matchedRows = 0;
            const BindingFlags fields = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (FieldInfo collectionField in gameBalanceType.GetFields(fields).OrderBy(f => f.Name))
            {
                object collection;
                try { collection = collectionField.GetValue(gameBalance); }
                catch { continue; }

                IEnumerable enumerable = collection as IEnumerable;
                if (enumerable == null || collection is string) continue;

                int index = 0;
                foreach (object row in enumerable)
                {
                    index++;
                    if (row == null) continue;
                    List<string> hits = new List<string>();
                    FindHits(row, smartType, rawExpression, hits, 0, new HashSet<object>(ReferenceComparer.Instance));
                    if (hits.Count == 0) continue;

                    matchedRows++;
                    sb.AppendLine("BALANCE_ROW collection=" + collectionField.Name + " index=" + index + " id=" + Quote(TryGetId(row)) + " type=" + row.GetType().FullName);
                    foreach (string hit in hits.Distinct()) sb.AppendLine("  HIT " + hit);
                    DumpObject(sb, row, smartType, rawExpression, 1, new HashSet<object>(ReferenceComparer.Instance));
                    sb.AppendLine();
                }
            }

            sb.AppendLine("BALANCE_SUMMARY matched_rows=" + matchedRows);
            sb.AppendLine("=== END GAMEBALANCE KEYWORD HITS ===");
            sb.AppendLine();
        }

        private static void DumpPlayerConfessionProbability(StringBuilder sb, Type[] allTypes)
        {
            sb.AppendLine("=== CURRENT PLAYER PARAM ===");
            Type mainGameType = allTypes.FirstOrDefault(t => t != null && t.Name == "MainGame");
            if (mainGameType == null)
            {
                sb.AppendLine("MainGame missing");
                sb.AppendLine("=== END CURRENT PLAYER PARAM ===");
                return;
            }

            object main = GetStaticMember(mainGameType, "me");
            object player = GetMember(main, "player");
            object data = GetMember(player, "data");
            if (data == null)
            {
                sb.AppendLine("player.data unavailable");
                sb.AppendLine("=== END CURRENT PLAYER PARAM ===");
                return;
            }

            MethodInfo getParam = data.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "GetParam" && m.GetParameters().Length >= 1 && m.GetParameters()[0].ParameterType == typeof(string));
            if (getParam == null)
            {
                sb.AppendLine("GetParam unavailable on player.data type=" + data.GetType().FullName);
                sb.AppendLine("=== END CURRENT PLAYER PARAM ===");
                return;
            }

            try
            {
                ParameterInfo[] ps = getParam.GetParameters();
                object[] args = new object[ps.Length];
                args[0] = "confession_probability";
                for (int i = 1; i < ps.Length; i++)
                {
                    args[i] = ps[i].HasDefaultValue ? ps[i].DefaultValue : DefaultValue(ps[i].ParameterType);
                }
                object value = getParam.Invoke(data, args);
                sb.AppendLine("confession_probability=" + Convert.ToString(value, CultureInfo.InvariantCulture));
                sb.AppendLine("GetParamSignature=" + getParam);
            }
            catch (Exception ex)
            {
                sb.AppendLine("GetParam failed=" + ex.GetType().Name + ":" + ex.Message);
            }
            sb.AppendLine("=== END CURRENT PLAYER PARAM ===");
        }

        private static void FindHits(object obj, Type smartType, MethodInfo rawExpression, List<string> hits, int depth, HashSet<object> seen)
        {
            if (obj == null || depth > 4) return;
            Type type = obj.GetType();
            if (!type.IsValueType && !(obj is string))
            {
                if (seen.Contains(obj)) return;
                seen.Add(obj);
            }

            string s = obj as string;
            if (s != null)
            {
                foreach (string needle in Needles)
                    if (s.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                        hits.Add("string=" + Quote(s) + " needle=" + Quote(needle));
                return;
            }

            if (smartType != null && smartType.IsInstanceOfType(obj))
            {
                string raw = GetRaw(obj, rawExpression);
                foreach (string needle in Needles)
                    if (raw.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                        hits.Add("SmartExpression=" + Quote(raw) + " needle=" + Quote(needle));
                return;
            }

            IEnumerable enumerable = obj as IEnumerable;
            if (enumerable != null && !(obj is string))
            {
                int count = 0;
                foreach (object child in enumerable)
                {
                    if (++count > 500) break;
                    FindHits(child, smartType, rawExpression, hits, depth + 1, seen);
                }
                return;
            }

            if (type.IsPrimitive || type.IsEnum || type == typeof(decimal)) return;
            const BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (FieldInfo field in type.GetFields(f))
            {
                object value;
                try { value = field.GetValue(obj); }
                catch { continue; }
                FindHits(value, smartType, rawExpression, hits, depth + 1, seen);
            }
        }

        private static void DumpObject(StringBuilder sb, object obj, Type smartType, MethodInfo rawExpression, int depth, HashSet<object> seen)
        {
            if (obj == null || depth > 3) return;
            Type type = obj.GetType();
            if (!type.IsValueType && !(obj is string))
            {
                if (seen.Contains(obj)) return;
                seen.Add(obj);
            }

            const BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (FieldInfo field in type.GetFields(f).OrderBy(x => x.Name))
            {
                object value;
                try { value = field.GetValue(obj); }
                catch { continue; }
                if (value == null) continue;

                string prefix = new string(' ', depth * 2) + field.Name + "=";
                string s = value as string;
                if (s != null) { sb.AppendLine(prefix + Quote(s)); continue; }

                Type vt = value.GetType();
                if (vt.IsPrimitive || vt.IsEnum || value is decimal)
                {
                    sb.AppendLine(prefix + Convert.ToString(value, CultureInfo.InvariantCulture));
                    continue;
                }

                if (smartType != null && smartType.IsInstanceOfType(value))
                {
                    sb.AppendLine(prefix + "SmartExpression " + Quote(GetRaw(value, rawExpression)));
                    continue;
                }

                IEnumerable enumerable = value as IEnumerable;
                if (enumerable != null && !(value is string))
                {
                    int i = 0;
                    foreach (object child in enumerable)
                    {
                        if (++i > 100) { sb.AppendLine(prefix + "<truncated>"); break; }
                        if (child == null) continue;
                        string cs = child as string;
                        if (cs != null) sb.AppendLine(prefix + "[" + i + "] " + Quote(cs));
                        else if (child.GetType().IsPrimitive || child.GetType().IsEnum || child is decimal)
                            sb.AppendLine(prefix + "[" + i + "] " + Convert.ToString(child, CultureInfo.InvariantCulture));
                        else if (smartType != null && smartType.IsInstanceOfType(child))
                            sb.AppendLine(prefix + "[" + i + "] SmartExpression " + Quote(GetRaw(child, rawExpression)));
                        else if (depth < 3)
                        {
                            sb.AppendLine(prefix + "[" + i + "] type=" + child.GetType().FullName);
                            DumpObject(sb, child, smartType, rawExpression, depth + 1, seen);
                        }
                    }
                    continue;
                }

                if (depth < 3 && vt.Assembly == type.Assembly)
                {
                    sb.AppendLine(prefix + "type=" + vt.FullName);
                    DumpObject(sb, value, smartType, rawExpression, depth + 1, seen);
                }
            }
        }

        private static string GetSerializedGraph(object graph)
        {
            Type t = graph == null ? null : graph.GetType();
            while (t != null)
            {
                FieldInfo f = t.GetField("_serializedGraph", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (f != null && f.FieldType == typeof(string))
                {
                    try { return f.GetValue(graph) as string; }
                    catch { return null; }
                }
                t = t.BaseType;
            }
            return null;
        }

        private static string GetRaw(object smart, MethodInfo rawExpression)
        {
            try { return rawExpression == null ? "" : Convert.ToString(rawExpression.Invoke(smart, null), CultureInfo.InvariantCulture) ?? ""; }
            catch { return ""; }
        }

        private static string TryGetId(object row)
        {
            if (row == null) return "";
            Type t = row.GetType();
            FieldInfo f = t.GetField("id", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null) try { return Convert.ToString(f.GetValue(row), CultureInfo.InvariantCulture) ?? ""; } catch { }
            PropertyInfo p = t.GetProperty("id", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (p != null && p.GetIndexParameters().Length == 0) try { return Convert.ToString(p.GetValue(row, null), CultureInfo.InvariantCulture) ?? ""; } catch { }
            return "";
        }

        private static object GetStaticMember(Type type, string name)
        {
            if (type == null) return null;
            const BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            try
            {
                FieldInfo field = type.GetField(name, f);
                if (field != null) return field.GetValue(null);
                PropertyInfo prop = type.GetProperty(name, f);
                if (prop != null && prop.GetIndexParameters().Length == 0) return prop.GetValue(null, null);
            }
            catch { }
            return null;
        }

        private static object GetMember(object obj, string name)
        {
            if (obj == null) return null;
            const BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            try
            {
                FieldInfo field = obj.GetType().GetField(name, f);
                if (field != null) return field.GetValue(obj);
                PropertyInfo prop = obj.GetType().GetProperty(name, f);
                if (prop != null && prop.GetIndexParameters().Length == 0) return prop.GetValue(obj, null);
            }
            catch { }
            return null;
        }

        private static object DefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
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
