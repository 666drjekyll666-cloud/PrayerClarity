using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;

namespace PrayerClarity
{
    internal static class ItemTooltipPresentation
    {
        private static ManualLogSource _log;
        private static bool _errorLogged;
        private static Type _bubbleTextType;
        private static Type _blankSeparatorType;
        private static ConstructorInfo _bubbleTextConstructor;
        private static MethodInfo _nameWithoutQualitySuffix;

        internal static void Install(string harmonyId, ManualLogSource log)
        {
            _log = log;

            Type itemDefinition = R.GameType("ItemDefinition");
            Type item = R.GameType("Item");
            if (itemDefinition == null || item == null)
                throw new MissingMemberException("ItemDefinition/Item type is unavailable.");

            MethodInfo getTooltipData = R.Method(itemDefinition, "GetTooltipData", false, new[] { item, typeof(bool) });
            if (getTooltipData == null)
                throw new MissingMethodException("ItemDefinition.GetTooltipData(Item, bool)");

            _nameWithoutQualitySuffix = R.Method(itemDefinition, "GetNameWithoutQualitySuffix", false, 0);
            if (_nameWithoutQualitySuffix == null)
                throw new MissingMethodException("ItemDefinition.GetNameWithoutQualitySuffix()");

            R.Patch(harmonyId + ".itemtooltip", typeof(ItemTooltipPresentation), getTooltipData, nameof(ItemTooltipPostfix));
        }

        private static void ItemTooltipPostfix(object __instance, object __result)
        {
            try
            {
                IList list = __result as IList;
                if (__instance == null || list == null) return;

                List<object> crafts = ResolvePrayerFamilyCrafts(__instance);
                if (crafts.Count == 0) return;

                string summary = BuildPrayerDetailsSummary(crafts);
                if (string.IsNullOrEmpty(summary)) return;

                Localization.UseCurrentGameLanguage();
                if (TryReplaceVanillaPrayerMechanics(list, summary)) return;

                // Unexpected vanilla shape: preserve everything already returned by the
                // item tooltip and append the same Clarity block used by Technology.
                object blank = CreateBlankSeparator();
                if (blank != null) list.Add(blank);
                list.Add(CreateTextData(Localization.F("tech.prayer_details"), 3));
                list.Add(CreateTextData(summary, 4));
            }
            catch (Exception ex)
            {
                if (_errorLogged) return;
                _errorLogged = true;
                _log?.LogError("PrayerClarity prayer-item tooltip presentation failed; vanilla item tooltip remains available. " + ex);
            }
        }

        private static List<object> ResolvePrayerFamilyCrafts(object definition)
        {
            List<object> result = new List<object>();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            AddLinkedPrayerCraft(result, seen, definition);
            if (result.Count == 0) return result;

            string root = _nameWithoutQualitySuffix.Invoke(definition, null) as string;
            if (string.IsNullOrEmpty(root)) return result;

            // Quality prayers use the verified root:1/root:2/root:3 item family. Resolve
            // siblings only when they exist; ordinary prayer remains a single-item case.
            for (int quality = 1; quality <= 3; quality++)
            {
                object sibling = R.BalanceData(root + ":" + quality, "ItemDefinition", true);
                AddLinkedPrayerCraft(result, seen, sibling);
            }

            return result;
        }

        private static void AddLinkedPrayerCraft(List<object> result, HashSet<string> seen, object itemDefinition)
        {
            if (itemDefinition == null) return;
            object craft = R.Get(itemDefinition, "linked_craft");
            if (craft == null) return;

            string craftId = R.Id(craft) ?? string.Empty;
            if (!craftId.StartsWith("pray:", StringComparison.Ordinal) || !seen.Add(craftId)) return;
            result.Add(craft);
        }

        private static string BuildPrayerDetailsSummary(List<object> crafts)
        {
            List<PrayerForecast.TierDetails> tiers = new List<PrayerForecast.TierDetails>();
            foreach (object craft in crafts)
            {
                PrayerForecast.TierDetails tier = PrayerForecast.BuildTierDetails(craft);
                if (tier != null) tiers.Add(tier);
            }
            if (tiers.Count == 0) return null;

            tiers.Sort((a, b) =>
            {
                int aq = a.QualityTier <= 0 ? int.MaxValue : a.QualityTier;
                int bq = b.QualityTier <= 0 ? int.MaxValue : b.QualityTier;
                int q = aq.CompareTo(bq);
                return q != 0 ? q : string.CompareOrdinal(a.CraftId, b.CraftId);
            });

            List<string> lines = new List<string>();
            foreach (PrayerForecast.TierDetails tier in tiers)
            {
                string quality = QualityLabel(tier.QualityTier);
                string requirement = tier.Requirement > 0
                    ? Localization.F("tech.requires", tier.Requirement)
                    : null;

                if (!string.IsNullOrEmpty(quality) && !string.IsNullOrEmpty(requirement))
                    lines.Add(quality + ": " + requirement);
                else if (!string.IsNullOrEmpty(quality))
                    lines.Add(quality);
                else if (!string.IsNullOrEmpty(requirement))
                    lines.Add(requirement);

                string contribution = PresentationText.FormatPrayerContribution(tier);
                if (!string.Equals(contribution, "—", StringComparison.Ordinal))
                    lines.Add(Localization.F("tech.success_bonus") + ": " + contribution);

                if (!string.IsNullOrEmpty(tier.SpecialText))
                    lines.Add(Localization.F("forecast.effect_header") + ": " + tier.SpecialText);
            }

            return string.Join("\n", lines.ToArray());
        }

        private static string QualityLabel(int qualityTier)
        {
            switch (qualityTier)
            {
                case 1: return Localization.F("quality.bronze");
                case 2: return Localization.F("quality.silver");
                case 3: return Localization.F("quality.gold");
                default: return null;
            }
        }

        private static bool TryReplaceVanillaPrayerMechanics(IList list, string summary)
        {
            if (list == null || list.Count < 2) return false;
            if (_bubbleTextType == null) _bubbleTextType = R.GameType("BubbleWidgetTextData");
            if (_bubbleTextType == null) return false;

            string vanillaHeader = R.VanillaLocalize("preach_params_2");
            int headerIndex = -1;
            for (int i = list.Count - 2; i >= 0; i--)
            {
                object row = list[i];
                if (row == null || !_bubbleTextType.IsInstanceOfType(row)) continue;
                string text = R.Get(row, "text") as string;
                if (string.Equals(text, vanillaHeader, StringComparison.Ordinal))
                {
                    headerIndex = i;
                    break;
                }
            }

            if (headerIndex < 0 || headerIndex + 1 >= list.Count) return false;
            object header = list[headerIndex];
            object body = list[headerIndex + 1];
            if (body == null || !_bubbleTextType.IsInstanceOfType(body)) return false;

            R.Set(header, "text", Localization.F("tech.prayer_details"));
            R.Set(body, "text", summary);

            if (headerIndex > 0)
            {
                object previous = list[headerIndex - 1];
                if (previous != null && _bubbleTextType.IsInstanceOfType(previous))
                {
                    string text = R.Get(previous, "text") as string;
                    string trimmed = StripStockRequirementLine(text);
                    if (!string.Equals(text, trimmed, StringComparison.Ordinal))
                        R.Set(previous, "text", trimmed);
                }
            }

            return true;
        }

        private static string StripStockRequirementLine(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            string normalized = text.Replace("\r\n", "\n");

            int newline = normalized.IndexOf('\n');
            if (newline > 0)
            {
                string firstLine = normalized.Substring(0, newline);
                if (firstLine.IndexOf("(cross)", StringComparison.Ordinal) >= 0)
                    return normalized.Substring(newline + 1).TrimStart();
            }

            int cross = normalized.IndexOf("(cross)", StringComparison.Ordinal);
            if (cross < 0 || cross > 64) return text;

            int end = FindSentenceTerminator(normalized, cross);
            if (end < 0 || end + 1 >= normalized.Length) return text;
            return normalized.Substring(end + 1).TrimStart();
        }

        private static int FindSentenceTerminator(string text, int start)
        {
            for (int i = Math.Max(0, start); i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '.':
                    case '!':
                    case '?':
                    case '。':
                    case '！':
                    case '？':
                        return i;
                }
            }
            return -1;
        }

        private static object CreateBlankSeparator()
        {
            if (_blankSeparatorType == null)
                _blankSeparatorType = R.GameType("BubbleWidgetBlankSeparatorData");
            return _blankSeparatorType == null ? null : Activator.CreateInstance(_blankSeparatorType);
        }

        private static object CreateTextData(string text, int styleValue)
        {
            if (_bubbleTextConstructor == null)
            {
                if (_bubbleTextType == null) _bubbleTextType = R.GameType("BubbleWidgetTextData");
                if (_bubbleTextType == null) throw new MissingMemberException("BubbleWidgetTextData");

                ConstructorInfo[] constructors = _bubbleTextType.GetConstructors(R.Inst);
                foreach (ConstructorInfo constructor in constructors)
                {
                    ParameterInfo[] p = constructor.GetParameters();
                    if (p.Length == 2 && p[0].ParameterType == typeof(string) && p[1].ParameterType.IsEnum)
                    {
                        _bubbleTextConstructor = constructor;
                        break;
                    }
                }
                if (_bubbleTextConstructor == null)
                    throw new MissingMethodException("BubbleWidgetTextData(string, style)");
            }

            Type styleType = _bubbleTextConstructor.GetParameters()[1].ParameterType;
            object style = Enum.ToObject(styleType, styleValue);
            return _bubbleTextConstructor.Invoke(new[] { (object)text, style });
        }
    }
}
