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
    public sealed class PrayerClarityLookupIdentityProbe : BaseUnityPlugin
    {
        public const string PluginGuid = "nikich.graveyardkeeper.prayerclarity.lookupidentityprobe";
        public const string PluginName = "PrayerClarity Lookup Identity Probe";
        public const string PluginVersion = "0.1.0";

        private static readonly Guid SupportedGameMvid = new Guid("6f50b8e7-156b-49ac-bbe8-7505894b2364");
        private bool _completed;
        private float _gameReadyAt = -1f;

        private void Awake()
        {
            Logger.LogInfo(PluginName + " " + PluginVersion + " loaded. Read-only reflection only; no Harmony, balance mutation, save writes, or gameplay changes.");
        }

        private void Update()
        {
            if (_completed) return;

            Assembly gameAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => string.Equals(a.GetName().Name, "Assembly-CSharp", StringComparison.Ordinal));
            if (gameAssembly == null || !IsGameStarted(gameAssembly)) return;

            if (_gameReadyAt < 0f)
            {
                _gameReadyAt = Time.realtimeSinceStartup;
                return;
            }

            if (Time.realtimeSinceStartup - _gameReadyAt < 3f) return;
            _completed = true;

            try
            {
                RunProbe(gameAssembly);
            }
            catch (Exception ex)
            {
                Logger.LogError(PluginName + " failed: " + ex);
            }
        }

        private void RunProbe(Assembly gameAssembly)
        {
            if (gameAssembly.ManifestModule.ModuleVersionId != SupportedGameMvid)
                throw new InvalidOperationException("Unsupported Assembly-CSharp MVID " + gameAssembly.ManifestModule.ModuleVersionId);

            Type gameBalanceType = FindType(gameAssembly, "GameBalance");
            Type craftType = FindType(gameAssembly, "CraftDefinition");
            if (gameBalanceType == null || craftType == null)
                throw new MissingMemberException("GameBalance/CraftDefinition unavailable");

            object balance = GetStatic(gameBalanceType, "me");
            if (balance == null) throw new InvalidOperationException("GameBalance.me unavailable");

            IEnumerable craftData = Get(balance, "craft_data") as IEnumerable;
            if (craftData == null) throw new MissingMemberException("GameBalance.craft_data unavailable");

            MethodInfo getDataOrNull = ResolveBalanceGetter(balance.GetType(), "GetDataOrNull", craftType);
            if (getDataOrNull == null)
                throw new MissingMethodException("GameBalanceBase.GetDataOrNull<CraftDefinition>(string)");

            StringBuilder report = new StringBuilder(32768);
            report.AppendLine("PRAYERCLARITY — LOOKUP IDENTITY PROBE");
            report.AppendLine("ProbeVersion=" + PluginVersion);
            report.AppendLine("GeneratedUtc=" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            report.AppendLine("GameAssembly=" + gameAssembly.FullName);
            report.AppendLine("ModuleVersionId=" + gameAssembly.ManifestModule.ModuleVersionId);
            report.AppendLine("Contract=READ_ONLY_REFLECTION_NO_HARMONY_NO_BALANCE_MUTATION_NO_SAVE_WRITE");
            report.AppendLine("Question=Does GameBalance.GetDataOrNull return the same live CraftDefinition object stored in GameBalance.craft_data for pray:* definitions?");
            report.AppendLine();

            int prayerRows = 0;
            int lookupNull = 0;
            int sameCraft = 0;
            int differentCraft = 0;
            int sameOutput = 0;
            int differentOutput = 0;
            HashSet<string> seenIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (object source in craftData)
            {
                if (source == null || !craftType.IsInstanceOfType(source)) continue;
                string id = Get(source, "id") as string;
                if (string.IsNullOrEmpty(id) || !id.StartsWith("pray:", StringComparison.Ordinal)) continue;

                prayerRows++;
                object lookup = getDataOrNull.Invoke(balance, new object[] { id });
                bool craftSame = ReferenceEquals(source, lookup);
                object sourceOutput = Get(source, "output");
                object lookupOutput = lookup == null ? null : Get(lookup, "output");
                bool outputSame = lookup != null && ReferenceEquals(sourceOutput, lookupOutput);
                bool duplicateId = !seenIds.Add(id);

                if (lookup == null) lookupNull++;
                if (craftSame) sameCraft++; else differentCraft++;
                if (outputSame) sameOutput++; else differentOutput++;

                report.Append("ID=").Append(id)
                    .Append(" | lookupNull=").Append(lookup == null ? "true" : "false")
                    .Append(" | sameCraft=").Append(craftSame ? "true" : "false")
                    .Append(" | sameOutput=").Append(outputSame ? "true" : "false")
                    .Append(" | duplicateId=").Append(duplicateId ? "true" : "false")
                    .AppendLine();
            }

            report.AppendLine();
            report.AppendLine("SUMMARY prayerRows=" + prayerRows
                + " lookupNull=" + lookupNull
                + " sameCraft=" + sameCraft
                + " differentCraft=" + differentCraft
                + " sameOutput=" + sameOutput
                + " differentOutput=" + differentOutput);
            report.AppendLine("PASS=" + (prayerRows > 0 && lookupNull == 0 && differentCraft == 0 ? "true" : "false"));

            string path = Path.Combine(Paths.BepInExRootPath, "PrayerClarity-lookup-identity-probe-0.1.0.txt");
            File.WriteAllText(path, report.ToString(), new UTF8Encoding(false));
            Logger.LogInfo(PluginName + " complete: " + path);
            Logger.LogInfo("Identity summary: prayerRows=" + prayerRows + ", lookupNull=" + lookupNull + ", differentCraft=" + differentCraft + ", PASS=" + (prayerRows > 0 && lookupNull == 0 && differentCraft == 0));
        }

        private static bool IsGameStarted(Assembly gameAssembly)
        {
            Type mainGame = FindType(gameAssembly, "MainGame");
            if (mainGame == null) return false;
            FieldInfo field = mainGame.GetField("game_started", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (field == null || field.FieldType != typeof(bool)) return false;
            try { return (bool)field.GetValue(null); }
            catch { return false; }
        }

        private static MethodInfo ResolveBalanceGetter(Type runtimeType, string name, Type expectedType)
        {
            MethodInfo[] candidates = runtimeType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => string.Equals(m.Name, name, StringComparison.Ordinal))
                .Where(m =>
                {
                    ParameterInfo[] p = m.GetParameters();
                    return p.Length == 1 && p[0].ParameterType == typeof(string);
                })
                .ToArray();

            MethodInfo closed = candidates.FirstOrDefault(m =>
                !m.ContainsGenericParameters && expectedType.IsAssignableFrom(m.ReturnType));
            if (closed != null) return closed;

            MethodInfo generic = candidates.FirstOrDefault(m =>
                m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1);
            if (generic == null) return null;

            MethodInfo bound = generic.MakeGenericMethod(expectedType);
            return bound.ContainsGenericParameters ? null : bound;
        }

        private static Type FindType(Assembly assembly, string name)
        {
            Type exact = assembly.GetType(name, false);
            if (exact != null) return exact;
            try { return assembly.GetTypes().FirstOrDefault(t => t != null && t.Name == name); }
            catch (ReflectionTypeLoadException ex) { return ex.Types.FirstOrDefault(t => t != null && t.Name == name); }
        }

        private static object Get(object instance, string name)
        {
            if (instance == null) return null;
            for (Type type = instance.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null) return field.GetValue(instance);
                PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (property != null && property.CanRead) return property.GetValue(instance, null);
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
                if (property != null && property.CanRead) return property.GetValue(null, null);
            }
            return null;
        }
    }
}
