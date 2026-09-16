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
    public sealed class PrayerClarityQualityScopeProbe : BaseUnityPlugin
    {
        public const string PluginGuid = "prayerclarity.research.qualityscope";
        public const string PluginName = "PrayerClarity Quality Prayer Scope Probe";
        public const string PluginVersion = "0.1.0";

        private bool _completed;
        private float _readyAt = -1f;

        private void Awake()
        {
            Logger.LogInfo("PrayerClarity Quality Prayer Scope Probe 0.1.0 loaded: read-only GameBalance inspection; no Harmony and no intentional game/save mutation.");
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
            catch (Exception ex) { Logger.LogError("PrayerClarity quality-scope probe failed: " + ex); }
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
            Type gameBalanceType = all.FirstOrDefault(t => t != null && t.Name == "GameBalance");
            Type smartType = all.FirstOrDefault(t => t != null && t.Name == "SmartExpression");
            MethodInfo rawExpression = smartType == null ? null : smartType.GetMethod("GetRawExpressionString", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            StringBuilder sb = new StringBuilder(512 * 1024);
            sb.AppendLine("PRAYERCLARITY — IMAGINATION / EXCELLENCE LINKED-CRAFT SCOPE AUDIT");
            sb.AppendLine("ProbeVersion=" + PluginVersion);
            sb.AppendLine("GeneratedUtc=" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            sb.AppendLine("GameAssembly=" + game.FullName);
            sb.AppendLine("ModuleVersionId=" + game.ManifestModule.ModuleVersionId);
            sb.AppendLine("Contract=READ_ONLY_GAMEBALANCE_INSPECTION_NO_HARMONY_NO_MUTATION");
            sb.AppendLine("Question=Which stock 1.407 craft definitions actually link buff_pen or buff_star, and what quality-related row data accompanies those links?");
            sb.AppendLine();

            if (gameBalanceType == null)
            {
                sb.AppendLine("GameBalance type missing");
                Write(sb);
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
                Write(sb);
                return;
            }

            int rows = 0;
            int hits = 0;
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
                    rows++;

                    List<string> rowHits = new List<string>();
                    FindNeedles(row, smartType, rawExpression, rowHits, 0, new HashSet<object>(ReferenceComparer.Instance));
                    if (rowHits.Count == 0) continue;

                    hits++;
                    sb.AppendLine("=== MATCH collection=" + collectionField.Name + " index=" + index + " id=" + Quote(TryGetId(row)) + " type=" + row.GetType().FullName + " ===");
                    foreach (string hit in rowHits.Distinct()) sb.AppendLine("HIT " + hit);
                    DumpObject(sb, row, smartType, rawExpression, 0, new HashSet<object>(ReferenceComparer.Instance));
                    sb.AppendLine("=== END MATCH ===");
                    sb.AppendLine();
                }
            }

            sb.AppendLine("SUMMARY scanned_rows=" + rows + " matched_rows=" + hits);
            Write(sb);
        }

        private static void FindNeedles(object obj, Type smartType, MethodInfo rawExpression, List<string> hits, int depth, HashSet<object> seen)
        {
            if (obj == null || depth > 3) return;
            if (!obj.GetType().IsValueType && !(obj is string))
            {
                if (seen.Contains(obj)) return;
                seen.Add(obj);
            }

            string s = obj as string;
            if (s != null)
            {
                if (ContainsNeedle(s)) hits.Add("string=" + Quote(s));
                return;
            }

            if (smartType != null && smartType.IsInstanceOfType(obj))
            {
                string raw = GetRaw(obj, rawExpression);
                if (ContainsNeedle(raw)) hits.Add("SmartExpression=" + Quote(raw));
                return;
            }

            IEnumerable enumerable = obj as IEnumerable;
            if (enumerable != null && !(obj is string))
            {
                int count = 0;
                foreach (object child in enumerable)
                {
                    if (++count > 500) break;
                    FindNeedles(child, smartType, rawExpression, hits, depth + 1, seen);
                }
                return;
            }

            Type type = obj.GetType();
            if (type.IsPrimitive || type.IsEnum || type == typeof(decimal)) return;

            const BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (FieldInfo field in type.GetFields(f))
            {
                object value;
                try { value = field.GetValue(obj); }
                catch { continue; }
                FindNeedles(value, smartType, rawExpression, hits, depth + 1, seen);
            }
        }

        private static bool ContainsNeedle(string value)
        {
            return !string.IsNullOrEmpty(value) &&
                   (value.IndexOf("buff_pen", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    value.IndexOf("buff_star", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static void DumpObject(StringBuilder sb, object obj, Type smartType, MethodInfo rawExpression, int depth, HashSet<object> seen)
        {
            if (obj == null || depth > 2) return;
            if (!obj.GetType().IsValueType && !(obj is string))
            {
                if (seen.Contains(obj)) return;
                seen.Add(obj);
            }

            Type type = obj.GetType();
            const BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (FieldInfo field in type.GetFields(f).OrderBy(x => x.Name))
            {
                object value;
                try { value = field.GetValue(obj); }
                catch { continue; }
                if (value == null) continue;

                string prefix = new string(' ', depth * 2) + field.Name + "=";
                string s = value as string;
                if (s != null)
                {
                    sb.AppendLine(prefix + Quote(s));
                    continue;
                }

                Type valueType = value.GetType();
                if (valueType.IsPrimitive || valueType.IsEnum || value is decimal)
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
                        string childString = child as string;
                        if (childString != null) sb.AppendLine(prefix + "[" + i + "] " + Quote(childString));
                        else if (child.GetType().IsPrimitive || child.GetType().IsEnum || child is decimal)
                            sb.AppendLine(prefix + "[" + i + "] " + Convert.ToString(child, CultureInfo.InvariantCulture));
                        else if (smartType != null && smartType.IsInstanceOfType(child))
                            sb.AppendLine(prefix + "[" + i + "] SmartExpression " + Quote(GetRaw(child, rawExpression)));
                        else if (depth < 2)
                        {
                            sb.AppendLine(prefix + "[" + i + "] type=" + child.GetType().FullName);
                            DumpObject(sb, child, smartType, rawExpression, depth + 1, seen);
                        }
                    }
                    continue;
                }

                if (depth < 2 && valueType.Assembly == type.Assembly)
                {
                    sb.AppendLine(prefix + "type=" + valueType.FullName);
                    DumpObject(sb, value, smartType, rawExpression, depth + 1, seen);
                }
            }
        }

        private static string GetRaw(object smartExpression, MethodInfo rawExpression)
        {
            try { return rawExpression == null ? "" : Convert.ToString(rawExpression.Invoke(smartExpression, null), CultureInfo.InvariantCulture) ?? ""; }
            catch { return ""; }
        }

        private void Write(StringBuilder sb)
        {
            string path = Path.Combine(Paths.BepInExRootPath, "PrayerClarity-quality-scope-0.1.0.txt");
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
            Logger.LogInfo("PrayerClarity quality-prayer scope audit complete: " + path);
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
