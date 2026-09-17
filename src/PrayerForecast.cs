using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace PrayerClarity
{
    internal static class PrayerForecast
    {
        internal sealed class TierDetails
        {
            internal string CraftId;
            internal int QualityTier;
            internal int Requirement;
            internal float FaithBonusRate;
            internal float MoneyBonusRate;
            internal float FixedFaithBonus;
            internal float FixedMoneyBonus;
            internal bool UsesSoulGratitude;
            internal string SpecialText;
            internal string SpecialCoreText;
            internal string SpecialSemanticKey;
            internal string SpecialIconName;
            internal float SpecialDurationDays;
            internal bool HasSpecialDuration;
        }

        internal sealed class Result
        {
            internal float ChurchQuality;
            internal float GraveyardQuality;
            internal int SuccessPercent;
            internal float BaseFaith;
            internal float SuccessFaith;
            internal float BaseMoney;
            internal float SuccessMoney;
            internal float FaithBonusRate;
            internal float MoneyBonusRate;
            internal float FixedFaithBonus;
            internal float FixedMoneyBonus;
            internal bool UsesSoulGratitude;
            internal string PrayerCraftId;
            internal int QualityTier;
            internal string SpecialText;
            internal string SpecialIconName;
        }

        private sealed class SpecialInfo
        {
            internal readonly string DisplayText;
            internal readonly string CoreText;
            internal readonly string SemanticKey;
            internal readonly string IconName;
            internal readonly float DurationDays;
            internal readonly bool HasDuration;

            internal SpecialInfo(
                string displayText,
                string coreText,
                string semanticKey,
                string iconName,
                float durationDays = 0f,
                bool hasDuration = false)
            {
                DisplayText = displayText;
                CoreText = coreText;
                SemanticKey = semanticKey;
                IconName = iconName;
                DurationDays = durationDays;
                HasDuration = hasDuration;
            }
        }

        private sealed class RewardItem
        {
            internal readonly string Id;
            internal readonly int Value;

            internal RewardItem(string id, int value)
            {
                Id = id;
                Value = value;
            }
        }

        internal static Result Build(object prayGui, float chance)
        {
            Localization.UseCurrentGameLanguage();

            object craft = R.Get(prayGui, "selected_craft");
            if (craft == null) return null;

            string craftId = R.Id(craft);
            object eventDef = ResolvePrayerEvent(craft);
            if (eventDef == null) return null;

            float church = EvaluateEventExpression(eventDef, "people");
            float graveyard = ResolveGraveyardQuality();

            float baseFaith = EvaluateEventExpression(eventDef, "faith");
            float baseMoney = EvaluateEventExpression(eventDef, "money");

            float fixedFaith = 0f;
            float fixedMoney = 0f;
            List<RewardItem> rewards = new List<RewardItem>();
            ReadOutputs(craft, ref fixedFaith, ref fixedMoney, rewards);

            float faithRate = R.Float(R.Get(craft, "k_faith"));
            float moneyRate = R.Float(R.Get(craft, "k_money"));
            int qualityTier = PrayerQualityTier(craftId);
            bool usesSoulGratitude = UsesSoulGratitudeEvent(eventDef);

            float successFaith = fixedFaith + UnityEngine.Mathf.RoundToInt(baseFaith * faithRate);
            float successMoney = fixedMoney + RoundMoney(baseMoney * moneyRate);

            SpecialInfo special = BuildSpecialInfo(craftId, craft, rewards);

            return new Result
            {
                ChurchQuality = church,
                GraveyardQuality = graveyard,
                SuccessPercent = UnityEngine.Mathf.Clamp(UnityEngine.Mathf.RoundToInt(chance * 100f), 0, 100),
                BaseFaith = baseFaith,
                SuccessFaith = successFaith,
                BaseMoney = baseMoney,
                SuccessMoney = successMoney,
                FaithBonusRate = faithRate,
                MoneyBonusRate = moneyRate,
                FixedFaithBonus = fixedFaith,
                FixedMoneyBonus = fixedMoney,
                UsesSoulGratitude = usesSoulGratitude,
                PrayerCraftId = craftId,
                QualityTier = qualityTier,
                SpecialText = special == null ? null : special.DisplayText,
                SpecialIconName = special == null ? null : special.IconName
            };
        }

        internal static TierDetails BuildTierDetails(object craft)
        {
            Localization.UseCurrentGameLanguage();
            if (craft == null) return null;

            string craftId = R.Id(craft);
            if (string.IsNullOrEmpty(craftId) || !craftId.StartsWith("pray:", StringComparison.Ordinal)) return null;

            object eventDef = ResolvePrayerEvent(craft);
            float fixedFaith = 0f;
            float fixedMoney = 0f;
            List<RewardItem> rewards = new List<RewardItem>();
            ReadOutputs(craft, ref fixedFaith, ref fixedMoney, rewards);

            SpecialInfo special = BuildSpecialInfo(craftId, craft, rewards);
            return new TierDetails
            {
                CraftId = craftId,
                QualityTier = PrayerQualityTier(craftId),
                Requirement = UnityEngine.Mathf.RoundToInt(R.Float(R.Get(craft, "needs_quality"))),
                FaithBonusRate = R.Float(R.Get(craft, "k_faith")),
                MoneyBonusRate = R.Float(R.Get(craft, "k_money")),
                FixedFaithBonus = fixedFaith,
                FixedMoneyBonus = fixedMoney,
                UsesSoulGratitude = eventDef != null && UsesSoulGratitudeEvent(eventDef),
                SpecialText = special == null ? null : special.DisplayText,
                SpecialCoreText = special == null ? null : special.CoreText,
                SpecialSemanticKey = special == null ? null : special.SemanticKey,
                SpecialIconName = special == null ? null : special.IconName,
                SpecialDurationDays = special == null ? 0f : special.DurationDays,
                HasSpecialDuration = special != null && special.HasDuration
            };
        }

        internal static string BuildActiveBuffText(object playerBuff)
        {
            Localization.UseCurrentGameLanguage();
            if (playerBuff == null) return null;

            string buffId = R.Get(playerBuff, "buff_id") as string;
            if (string.IsNullOrEmpty(buffId)) return null;

            string editionText;
            if (PrayerEditionSemantics.TryBuildActiveEffect(buffId, out editionText))
                return editionText;

            object buff = R.BalanceData(buffId, "BuffDefinition", true);
            object res = buff == null ? null : R.Get(buff, "res");

            switch (buffId)
            {
                case "buff_sword":
                    return NumberedActiveResEffect("active.sword", res, "add_damage");
                case "buff_shield":
                    return NumberedActiveResEffect("active.shield", res, "add_armor");
                case "buff_skull":
                    return NumberedActiveResEffect("active.skull", res, "body_max");
                case "buff_pen":
                    return buff == null ? null : Localization.F("active.pen", R.Float(R.Get(buff, "craft_q")));
                case "buff_star":
                    return buff == null ? null : Localization.F("active.star", R.Float(R.Get(buff, "craft_q")));
                case "buff_plant":
                    return Localization.F("active.plant_inactive");
                case "buff_sins":
                    return Localization.F("active.sins_inactive");
                case "buff_gp_increase":
                    return Localization.F("active.gratitude");
                case "buff_sin_shard":
                    return Localization.F("active.sin_shard");
                default:
                    return null;
            }
        }

        private static object ResolvePrayerEvent(object craft)
        {
            object linked = R.Get(craft, "linked_sub_definition");
            if (linked != null) return linked;

            string linkedId = R.Get(craft, "linked_sub_id") as string;
            if (string.IsNullOrEmpty(linkedId)) return null;
            return R.BalanceData(linkedId, "PrayEventDefinition", true);
        }

        private static float EvaluateEventExpression(object eventDef, string member)
        {
            object expression = R.Get(eventDef, member);
            if (expression == null) return 0f;
            return R.EvaluateExpression(expression);
        }

        private static float ResolveGraveyardQuality()
        {
            return R.ZoneQuality("graveyard");
        }

        private static bool UsesSoulGratitudeEvent(object eventDef)
        {
            string id = R.Id(eventDef);
            return !string.IsNullOrEmpty(id) && id.StartsWith("pray_for_souls_", StringComparison.Ordinal);
        }

        private static int PrayerQualityTier(string craftId)
        {
            if (string.IsNullOrEmpty(craftId)) return 0;
            int lastColon = craftId.LastIndexOf(':');
            if (lastColon < 0 || lastColon + 1 >= craftId.Length) return 0;
            int tier;
            return int.TryParse(craftId.Substring(lastColon + 1), out tier) ? tier : 0;
        }

        private static void ReadOutputs(
            object craft,
            ref float fixedFaith,
            ref float fixedMoney,
            List<RewardItem> rewards)
        {
            IEnumerable output = R.Get(craft, "output") as IEnumerable;
            if (output == null) return;

            foreach (object item in output)
            {
                if (item == null) continue;
                string id = R.Id(item) ?? string.Empty;
                int value = R.Int(R.Get(item, "value"));
                if (value <= 0) continue;

                if (string.Equals(id, "faith", StringComparison.Ordinal))
                    fixedFaith += value;
                else if (string.Equals(id, "money", StringComparison.Ordinal))
                    fixedMoney += value / 100f;
                else
                    rewards.Add(new RewardItem(id, value));
            }
        }

        private static SpecialInfo BuildSpecialInfo(string craftId, object craft, List<RewardItem> rewards)
        {
            List<string> displayParts = new List<string>();
            List<string> coreParts = new List<string>();
            List<string> semanticParts = new List<string>();
            string iconName = null;
            float durationDays = 0f;
            bool hasDuration = false;

            string buffId = R.Get(craft, "buff") as string;
            float duration = R.Float(R.Get(craft, "dur_parameter"));
            SpecialInfo buff = BuildBuffEffect(craftId, buffId, duration);
            if (buff != null)
            {
                displayParts.Add(buff.DisplayText);
                coreParts.Add(buff.CoreText);
                semanticParts.Add(buff.SemanticKey);
                iconName = buff.IconName;
                if (buff.HasDuration)
                {
                    hasDuration = true;
                    durationDays = buff.DurationDays;
                }
            }

            if (rewards.Count > 0)
            {
                List<string> rewardParts = new List<string>();
                List<string> rewardKeys = new List<string>();
                foreach (RewardItem reward in rewards)
                {
                    rewardParts.Add(BuildRewardText(reward));
                    rewardKeys.Add(reward.Id + "=" + reward.Value.ToString(CultureInfo.InvariantCulture));
                }
                rewardKeys.Sort(StringComparer.Ordinal);

                string rewardText = Localization.F("forecast.reward", string.Join(", ", rewardParts.ToArray()));
                displayParts.Add(rewardText);
                coreParts.Add(rewardText);
                semanticParts.Add("rewards:" + string.Join(",", rewardKeys.ToArray()));
            }

            if (displayParts.Count == 0) return null;
            return new SpecialInfo(
                string.Join(" · ", displayParts.ToArray()),
                string.Join(" · ", coreParts.ToArray()),
                string.Join("|", semanticParts.ToArray()),
                iconName,
                durationDays,
                hasDuration);
        }

        private static string BuildRewardText(RewardItem reward)
        {
            string name = R.VanillaLocalize(reward.Id);
            string amount = " ×" + reward.Value.ToString(CultureInfo.InvariantCulture);

            if (string.Equals(reward.Id, "blessing_commerce", StringComparison.Ordinal))
            {
                string description = R.VanillaLocalize("blessing_commerce_d");
                if (!string.IsNullOrEmpty(description) &&
                    !string.Equals(description, "blessing_commerce_d", StringComparison.Ordinal))
                    return name + amount + " — " + description;
            }

            return name + amount;
        }

        private static SpecialInfo BuildBuffEffect(string craftId, string buffId, float duration)
        {
            if (string.IsNullOrEmpty(buffId)) return null;

            object buff = R.BalanceData(buffId, "BuffDefinition", true);
            object res = buff == null ? null : R.Get(buff, "res");
            string iconName = GetBuffIconName(buff);

            string editionText;
            string editionSemanticKey;
            if (PrayerEditionSemantics.TryBuildTierEffect(craftId, buffId, out editionText, out editionSemanticKey))
            {
                float editionDurationDays = duration > 0.0001f ? DurationParameterToGameDays(duration) : 0f;
                bool editionHasDuration = duration > 0.0001f;
                string editionDisplay = editionText;
                if (editionHasDuration)
                    editionDisplay += " · " + Localization.F("active.timer_days", editionDurationDays);

                return new SpecialInfo(
                    editionDisplay,
                    editionText,
                    editionSemanticKey,
                    iconName,
                    editionDurationDays,
                    editionHasDuration);
            }

            bool showDuration = true;
            string semanticKey = "buff:" + buffId;
            string text;
            switch (buffId)
            {
                case "buff_sword":
                {
                    float value = res == null ? 0f : R.GameResGet(res, "add_damage");
                    text = NumberedActiveResEffect("active.sword", res, "add_damage");
                    semanticKey += ":add_damage=" + value.ToString("R", CultureInfo.InvariantCulture);
                    break;
                }
                case "buff_shield":
                {
                    float value = res == null ? 0f : R.GameResGet(res, "add_armor");
                    text = NumberedActiveResEffect("active.shield", res, "add_armor");
                    semanticKey += ":add_armor=" + value.ToString("R", CultureInfo.InvariantCulture);
                    break;
                }
                case "buff_skull":
                {
                    float value = res == null ? 0f : R.GameResGet(res, "body_max");
                    text = NumberedActiveResEffect("active.skull", res, "body_max");
                    semanticKey += ":body_max=" + value.ToString("R", CultureInfo.InvariantCulture);
                    break;
                }
                case "buff_pen":
                {
                    float value = buff == null ? 0f : R.Float(R.Get(buff, "craft_q"));
                    text = buff == null ? null : Localization.F("active.pen", value);
                    semanticKey += ":craft_q=" + value.ToString("R", CultureInfo.InvariantCulture);
                    break;
                }
                case "buff_star":
                {
                    float value = buff == null ? 0f : R.Float(R.Get(buff, "craft_q"));
                    text = buff == null ? null : Localization.F("active.star", value);
                    semanticKey += ":craft_q=" + value.ToString("R", CultureInfo.InvariantCulture);
                    break;
                }
                case "buff_plant":
                    text = Localization.F("active.plant_inactive");
                    showDuration = false;
                    break;
                case "buff_sins":
                    text = Localization.F("active.sins_inactive");
                    showDuration = false;
                    break;
                case "buff_gp_increase":
                    text = Localization.F("active.gratitude");
                    break;
                case "buff_sin_shard":
                    text = Localization.F("active.sin_shard");
                    break;
                default:
                    text = null;
                    break;
            }

            if (string.IsNullOrEmpty(text)) return null;

            float durationDays = 0f;
            bool hasDuration = showDuration && duration > 0.0001f;
            string displayText = text;
            if (hasDuration)
            {
                durationDays = DurationParameterToGameDays(duration);
                displayText += " · " + Localization.F("active.timer_days", durationDays);
            }

            return new SpecialInfo(
                displayText,
                text,
                semanticKey,
                iconName,
                durationDays,
                hasDuration);
        }

        private static string NumberedActiveResEffect(string key, object res, string resource)
        {
            if (res == null) return null;
            float value = R.GameResGet(res, resource);
            return Localization.F(key, value);
        }

        private static string GetBuffIconName(object buff)
        {
            if (buff == null) return null;
            string sprite = R.Get(buff, "icon") as string;
            if (!string.IsNullOrEmpty(sprite)) return sprite;
            return R.Get(buff, "sprite") as string;
        }

        private static float RoundMoney(float value)
        {
            return (float)Math.Round(value * 100f, MidpointRounding.AwayFromZero) / 100f;
        }

        private static float DurationParameterToGameDays(float minutes)
        {
            return minutes / 11.25f;
        }
    }
}
