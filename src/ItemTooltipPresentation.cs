using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;

namespace PrayerClarity
{
    internal static class ItemTooltipPresentation
    {
        private static readonly HashSet<string> PrayerItemFamilies = new HashSet<string>(StringComparer.Ordinal)
        {
            "b_empty",
            "b_faith",
            "b_money",
            "b_faith_money",
            "b_plant",
            "b_sins",
            "b_skull",
            "b_sword",
            "b_shield",
            "b_pen",
            "b_star",
            "b_village",
            "b_souls",
            "b_grat_points_incr",
            "b_sin_shard",
            "b_ghost",
            "b_energy",
            "b_random",
            "b_techpoint_blue",
            "b_techpoint_green",
            "b_techpoint_red",
            "b_circle",
            "b_cross"
        };

        private const int ItemTooltipMaxWidth = TechnologyTooltipContentWidth.OwnedMaxWidth;

        private static ManualLogSource _log;
        private static bool _errorLogged;
        private static Type _bubbleTextType;
        private static ConstructorInfo _bubbleTextConstructor;

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

            R.Patch(harmonyId + ".itemtooltip", typeof(ItemTooltipPresentation), getTooltipData, nameof(ItemTooltipPostfix));
        }

        private static void ItemTooltipPostfix(object __instance, object __result)
        {
            try
            {
                IList list = __result as IList;
                if (__instance == null || list == null) return;

                object craft = ResolvePrayerCraft(__instance);
                if (craft == null) return;

                Localization.UseCurrentGameLanguage();
                PrayerForecast.TierDetails tier = PrayerForecast.BuildTierDetails(craft);
                TooltipPresentationSections sections = TooltipDetailsRenderer.BuildSingleSections(tier);
                if (sections == null || !sections.HasContent) return;

                if (TryReplaceVanillaPrayerMechanics(list, sections)) return;

                // Unexpected vanilla shape: preserve everything already returned by the
                // item tooltip and append current-item Clarity rather than deleting data.
                AppendSections(list, sections);
            }
            catch (Exception ex)
            {
                if (_errorLogged) return;
                _errorLogged = true;
                _log?.LogError("PrayerClarity prayer-item tooltip presentation failed; vanilla item tooltip remains available. " + ex);
            }
        }

        private static object ResolvePrayerCraft(object itemDefinition)
        {
            if (itemDefinition == null) return null;

            string itemId = R.Id(itemDefinition);
            if (!IsKnownPrayerItemId(itemId)) return null;

            object craft = R.Get(itemDefinition, "linked_craft");
            if (craft == null) return null;

            string craftId = R.Id(craft) ?? string.Empty;
            return craftId.StartsWith("pray:", StringComparison.Ordinal) ? craft : null;
        }

        internal static bool IsKnownPrayerItemId(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return false;
            int separator = itemId.IndexOf(':');
            string family = separator < 0 ? itemId : itemId.Substring(0, separator);
            return PrayerItemFamilies.Contains(family);
        }

        private static bool TryReplaceVanillaPrayerMechanics(IList list, TooltipPresentationSections sections)
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

            R.Set(header, "text", "\n" + Localization.F("tech.base_result"));
            // Current-item details are structured scanning blocks, not centered lore.
            // A leading newline on PrayerClarity-owned title rows gives a reliable
            // item-only visual gap without custom pixel positioning or Technology changes.
            list[headerIndex + 1] = CreateTextData(sections.BaseResult, 4);

            int insertIndex = headerIndex + 2;
            if (!string.IsNullOrEmpty(sections.SuccessBonuses))
            {
                list.Insert(insertIndex++, CreateTextData("\n" + Localization.F("tech.success_reward_bonus"), 3));
                list.Insert(insertIndex++, CreateTextData(sections.SuccessBonuses, 4, ItemTooltipMaxWidth));
            }

            TooltipTextPolish.NormalizeFollowingCraftingRow(list, insertIndex, _bubbleTextType);
            return true;
        }

        private static void AppendSections(IList list, TooltipPresentationSections sections)
        {
            if (!string.IsNullOrEmpty(sections.BaseResult))
            {
                list.Add(CreateTextData("\n" + Localization.F("tech.base_result"), 3));
                list.Add(CreateTextData(sections.BaseResult, 4));
            }

            if (!string.IsNullOrEmpty(sections.SuccessBonuses))
            {
                list.Add(CreateTextData("\n" + Localization.F("tech.success_reward_bonus"), 3));
                list.Add(CreateTextData(sections.SuccessBonuses, 4, ItemTooltipMaxWidth));
            }
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

        private static object CreateTextData(string text, int styleValue, int maxWidth = -1)
        {
            if (_bubbleTextConstructor == null)
            {
                if (_bubbleTextType == null) _bubbleTextType = R.GameType("BubbleWidgetTextData");
                if (_bubbleTextType == null) throw new MissingMemberException("BubbleWidgetTextData");

                foreach (ConstructorInfo constructor in _bubbleTextType.GetConstructors(R.Inst))
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
            // NGUIText.Alignment: Automatic=0, Left=1.
            object alignment = Enum.ToObject(parameters[2].ParameterType, 1);
            return _bubbleTextConstructor.Invoke(new object[] { text, style, alignment, maxWidth });
        }
    }
}
