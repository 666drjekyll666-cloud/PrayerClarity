using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using BepInEx.Logging;
using UnityEngine;

namespace PrayerClarity
{
    internal static class SecondarySurfacePresentation
    {
        private static ManualLogSource _log;
        private static bool _buffErrorLogged;
        private static bool _techErrorLogged;
        private static ConstructorInfo _bubbleTextConstructor;
        private static MethodInfo _tooltipAddDataMethod;
        private static Type _blankSeparatorType;

        internal static void Install(string harmonyId, ManualLogSource log)
        {
            _log = log;

            Type perkBuffItemGui = R.GameType("PerkBuffItemGUI");
            Type playerBuff = R.GameType("PlayerBuff");
            Type techUnlock = R.GameType("TechUnlock");
            Type tooltip = R.GameType("Tooltip");

            MethodInfo drawBuff = R.Method(perkBuffItemGui, "Draw", false, new[] { playerBuff });
            MethodInfo getTooltip = R.Method(techUnlock, "GetTooltip", false, new[] { tooltip });
            if (drawBuff == null) throw new MissingMethodException("PerkBuffItemGUI.Draw(PlayerBuff)");
            if (getTooltip == null) throw new MissingMethodException("TechUnlock.GetTooltip(Tooltip)");

            R.Patch(harmonyId + ".activeeffects", typeof(SecondarySurfacePresentation), drawBuff, nameof(PerkBuffDrawPostfix));
            R.Patch(harmonyId + ".technology", typeof(SecondarySurfacePresentation), getTooltip, nameof(TechUnlockTooltipPostfix));
        }

        private static void PerkBuffDrawPostfix(object __instance, object __0)
        {
            try
            {
                string text = PrayerForecast.BuildActiveBuffText(__0);
                if (string.IsNullOrEmpty(text)) return;

                object description = R.Get(__instance, "txt_descr");
                if (description == null) return;
                R.Set(description, "text", text);

                MethodInfo recalculate = R.Method(__instance.GetType(), "RecalculateDescriptionHeight", false, 0);
                recalculate?.Invoke(__instance, null);
                RepositionTextTable(__instance);
            }
            catch (Exception ex)
            {
                if (_buffErrorLogged) return;
                _buffErrorLogged = true;
                _log?.LogError("PrayerClarity active-effect presentation failed; vanilla Temporary Effects text remains available. " + ex);
            }
        }

        private static void TechUnlockTooltipPostfix(object __instance, object __0)
        {
            try
            {
                string summary = BuildTechnologySummary(__instance);
                if (string.IsNullOrEmpty(summary) || __0 == null) return;

                Localization.UseCurrentGameLanguage();
                object blank = CreateBlankSeparator();
                if (blank != null) AddTooltipData(__0, blank);
                AddTooltipData(__0, CreateTextData(Localization.F("tech.prayer_details"), 3));
                AddTooltipData(__0, CreateTextData(summary, 4));
            }
            catch (Exception ex)
            {
                if (_techErrorLogged) return;
                _techErrorLogged = true;
                _log?.LogError("PrayerClarity technology-tooltip presentation failed; vanilla technology tooltip remains available. " + ex);
            }
        }

        private static string BuildTechnologySummary(object techUnlock)
        {
            List<object> crafts = ResolvePrayerCrafts(techUnlock);
            if (crafts.Count == 0) return null;

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
                List<string> parts = new List<string>();
                if (tier.Requirement > 0)
                    parts.Add(Localization.F("tech.requires", tier.Requirement));

                string contribution = FormatPrayerContribution(tier);
                if (!string.IsNullOrEmpty(contribution)) parts.Add(contribution);
                if (!string.IsNullOrEmpty(tier.SpecialText)) parts.Add(tier.SpecialText);

                string body = parts.Count == 0 ? "—" : string.Join(" · ", parts.ToArray());
                lines.Add(QualityMarker(tier.QualityTier) + " " + body);
            }

            return string.Join("\n", lines.ToArray());
        }

        private static List<object> ResolvePrayerCrafts(object techUnlock)
        {
            List<object> result = new List<object>();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            string unlockId = R.Get(techUnlock, "id") as string;
            if (string.IsNullOrEmpty(unlockId)) return result;

            object recipe = R.BalanceData(unlockId, "CraftDefinition", true);
            AddPrayerCraft(result, seen, recipe);
            if (recipe == null) return result;

            IEnumerable output = R.Get(recipe, "output") as IEnumerable;
            if (output == null) return result;

            foreach (object item in output)
            {
                if (item == null) continue;

                IEnumerable multi = R.Get(item, "multiquality_items") as IEnumerable;
                if (multi != null)
                {
                    foreach (object idValue in multi)
                    {
                        string itemId = idValue as string;
                        if (string.IsNullOrEmpty(itemId)) continue;
                        object definition = R.BalanceData(itemId, "ItemDefinition", true);
                        AddLinkedPrayerCraft(result, seen, definition);
                    }
                }

                object directDefinition = R.Get(item, "definition");
                if (directDefinition == null)
                {
                    MethodInfo getter = R.Method(item.GetType(), "get_definition", false, 0);
                    directDefinition = getter == null ? null : getter.Invoke(item, null);
                }
                AddLinkedPrayerCraft(result, seen, directDefinition);
            }

            return result;
        }

        private static void AddLinkedPrayerCraft(List<object> result, HashSet<string> seen, object itemDefinition)
        {
            if (itemDefinition == null) return;
            object linked = R.Get(itemDefinition, "linked_craft");
            if (linked == null)
            {
                MethodInfo getter = R.Method(itemDefinition.GetType(), "get_linked_craft", false, 0);
                linked = getter == null ? null : getter.Invoke(itemDefinition, null);
            }
            AddPrayerCraft(result, seen, linked);
        }

        private static void AddPrayerCraft(List<object> result, HashSet<string> seen, object craft)
        {
            if (craft == null) return;
            string id = R.Id(craft) ?? string.Empty;
            if (!id.StartsWith("pray:", StringComparison.Ordinal) || !seen.Add(id)) return;
            result.Add(craft);
        }

        private static string FormatPrayerContribution(PrayerForecast.TierDetails tier)
        {
            List<string> parts = new List<string>();

            if (Math.Abs(tier.FaithBonusRate) >= 0.0001f || tier.FixedFaithBonus != 0)
            {
                string value = "(faith)";
                if (Math.Abs(tier.FaithBonusRate) >= 0.0001f)
                    value += " " + FormatPercent(tier.FaithBonusRate);
                if (tier.FixedFaithBonus != 0)
                    value += " " + FormatSignedInt(tier.FixedFaithBonus);
                parts.Add(value);
            }

            if (Math.Abs(tier.MoneyBonusRate) >= 0.0001f || Math.Abs(tier.FixedMoneyBonus) >= 0.0001f)
            {
                string value = "(slv)";
                if (Math.Abs(tier.MoneyBonusRate) >= 0.0001f)
                    value += " " + FormatPercent(tier.MoneyBonusRate);
                if (Math.Abs(tier.FixedMoneyBonus) >= 0.0001f)
                    value += " " + FormatSignedMoney(tier.FixedMoneyBonus);
                parts.Add(value);
            }

            return parts.Count == 0 ? null : string.Join(", ", parts.ToArray());
        }

        private static string FormatPercent(float rate)
        {
            float percent = rate * 100f;
            string sign = percent > 0.0001f ? "+" : percent < -0.0001f ? "−" : string.Empty;
            return sign + Math.Abs(percent).ToString("0.##", CultureInfo.InvariantCulture) + "%";
        }

        private static string FormatSignedInt(int value)
        {
            return value > 0 ? "+" + value.ToString(CultureInfo.InvariantCulture)
                : value < 0 ? "−" + Math.Abs(value).ToString(CultureInfo.InvariantCulture)
                : "0";
        }

        private static string FormatSignedMoney(float value)
        {
            if (Math.Abs(value) < 0.0001f) return string.Empty;
            return (value > 0f ? "+" : "−") + R.FormatMoney(Math.Abs(value));
        }

        private static string QualityMarker(int qualityTier)
        {
            switch (qualityTier)
            {
                case 1: return "(brz)";
                case 2: return "(slv)";
                case 3: return "(gld)";
                default: return "•";
            }
        }

        private static void RepositionTextTable(object itemGui)
        {
            GameObject go = R.Get(itemGui, "gameObject") as GameObject;
            Transform container = go == null ? null : go.transform.Find("text container");
            Type tableType = R.AnyType("SimpleUITable");
            object table = container == null || tableType == null ? null : container.gameObject.GetComponent(tableType);
            if (table == null) return;
            MethodInfo reposition = R.Method(table.GetType(), "Reposition", false, 0);
            reposition?.Invoke(table, null);
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
                Type type = R.GameType("BubbleWidgetTextData");
                if (type == null) throw new MissingMemberException("BubbleWidgetTextData");
                foreach (ConstructorInfo constructor in type.GetConstructors(R.Inst))
                {
                    ParameterInfo[] p = constructor.GetParameters();
                    if (p.Length == 4 && p[0].ParameterType == typeof(string) && p[3].ParameterType == typeof(int))
                    {
                        _bubbleTextConstructor = constructor;
                        break;
                    }
                }
                if (_bubbleTextConstructor == null)
                    throw new MissingMethodException("BubbleWidgetTextData(string, TextStyle, Alignment, int)");
            }

            ParameterInfo[] parameters = _bubbleTextConstructor.GetParameters();
            object style = Enum.ToObject(parameters[1].ParameterType, styleValue);
            object alignment = Enum.ToObject(parameters[2].ParameterType, 1);
            return _bubbleTextConstructor.Invoke(new[] { text, style, alignment, (object)(-1) });
        }

        private static void AddTooltipData(object tooltip, object data)
        {
            if (tooltip == null || data == null) return;
            if (_tooltipAddDataMethod == null)
                _tooltipAddDataMethod = R.Method(tooltip.GetType(), "AddData", false, 1);
            if (_tooltipAddDataMethod == null) throw new MissingMethodException("Tooltip.AddData(BubbleWidgetData)");
            _tooltipAddDataMethod.Invoke(tooltip, new[] { data });
        }
    }
}
