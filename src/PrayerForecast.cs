using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;

namespace PrayerClarity
{
    internal static class PrayerForecast
    {
        internal enum BonusHighlight
        {
            None,
            Faith,
            Money
        }

        internal sealed class Result
        {
            // Exact resolved values stay available for correctness/tests/balance work.
            // Default pulpit rendering intentionally does not expose these totals.
            internal int BaseFaith;
            internal float BaseMoney;
            internal int BonusFaith;
            internal float BonusMoney;

            // Prayer-owned success contribution, kept separate from resolved payout so
            // presentation can explain the mechanic without spoiling sermon rewards.
            internal float FaithBonusRate;
            internal float MoneyBonusRate;
            internal int FixedFaithBonus;
            internal float FixedMoneyBonus;

            internal int ChancePercent;
            internal float GraveyardQuality;
            internal bool UsesSoulGratitude;
            internal BonusHighlight Highlight;
            internal string SpecialText;
            // Leading effect icon only. For timed effects this is the exact
            // BuffDefinition.GetIconName() used by vanilla BuffIcon.Draw.
            internal string SpecialIconName;
        }

        internal sealed class TierDetails
        {
            internal string CraftId;
            internal string EventId;
            internal int QualityTier;
            internal int Requirement;
            internal float FaithBonusRate;
            internal float MoneyBonusRate;
            internal int FixedFaithBonus;
            internal float FixedMoneyBonus;
            internal bool UsesSoulGratitude;
            internal BonusHighlight Highlight;
            internal string SpecialText;
            internal string SpecialIconName;
        }

        private sealed class SpecialInfo
        {
            internal readonly string Text;
            internal readonly string IconName;

            internal SpecialInfo(string text, string iconName)
            {
                Text = text;
                IconName = iconName;
            }
        }

        private static MethodInfo _fromTimeKToSeconds;

        internal static Result Build(object prayGui, float chance)
        {
            object craft = R.Get(prayGui, "pray_craft");
            TierDetails tier = BuildTierDetails(craft);
            if (tier == null) return null;

            object prayEvent = R.BalanceData(tier.EventId, "PrayEventDefinition", false);
            int baseFaith = Mathf.Max(0, Mathf.RoundToInt(R.SmartFloat(R.Get(prayEvent, "faith"))));
            float baseMoney = Mathf.Max(0f, R.SmartFloat(R.Get(prayEvent, "money")));

            // Preserve the verified exact side-effect-free calculator unchanged.
            int bonusFaith = tier.FixedFaithBonus + Mathf.RoundToInt(baseFaith * tier.FaithBonusRate);
            float bonusMoney = tier.FixedMoneyBonus + Mathf.Round(baseMoney * tier.MoneyBonusRate * 100f) / 100f;

            return new Result
            {
                BaseFaith = baseFaith,
                BaseMoney = baseMoney,
                BonusFaith = bonusFaith,
                BonusMoney = bonusMoney,
                FaithBonusRate = tier.FaithBonusRate,
                MoneyBonusRate = tier.MoneyBonusRate,
                FixedFaithBonus = tier.FixedFaithBonus,
                FixedMoneyBonus = tier.FixedMoneyBonus,
                ChancePercent = Mathf.RoundToInt(Mathf.Clamp01(chance) * 100f),
                GraveyardQuality = R.ZoneQuality("graveyard"),
                UsesSoulGratitude = tier.UsesSoulGratitude,
                Highlight = tier.Highlight,
                SpecialText = tier.SpecialText,
                SpecialIconName = tier.SpecialIconName
            };
        }

        internal static TierDetails BuildTierDetails(object craft)
        {
            if (craft == null) return null;

            string craftId = R.Id(craft) ?? string.Empty;
            if (!craftId.StartsWith("pray:", StringComparison.Ordinal)) return null;

            string eventId = R.Get(craft, "linked_sub_id") as string;
            if (string.IsNullOrEmpty(eventId)) return null;

            Localization.UseCurrentGameLanguage();

            int fixedFaith = 0;
            float fixedMoney = 0f;
            List<RewardItem> rewards = new List<RewardItem>();
            CollectOutputs(craft, ref fixedFaith, ref fixedMoney, rewards);

            SpecialInfo special = BuildSpecial(craft, eventId, rewards);
            return new TierDetails
            {
                CraftId = craftId,
                EventId = eventId,
                QualityTier = ParseQualityTier(craftId),
                Requirement = Mathf.Max(0, Mathf.RoundToInt(R.Float(R.Get(craft, "needs_quality")))),
                FaithBonusRate = R.Float(R.Get(craft, "k_faith")),
                MoneyBonusRate = R.Float(R.Get(craft, "k_money")),
                FixedFaithBonus = fixedFaith,
                FixedMoneyBonus = fixedMoney,
                UsesSoulGratitude = eventId.StartsWith("pray_for_souls_", StringComparison.Ordinal),
                Highlight = GetBonusHighlight(craftId),
                SpecialText = special == null ? null : special.Text,
                SpecialIconName = special == null ? null : special.IconName
            };
        }

        internal static string BuildActiveBuffText(object playerBuff)
        {
            if (playerBuff == null) return null;
            Localization.UseCurrentGameLanguage();

            object buff = R.Get(playerBuff, "definition");
            if (buff == null) return null;
            string buffId = R.Id(buff) ?? string.Empty;
            object res = R.Get(buff, "res");

            switch (buffId)
            {
                case "buff_sword":
                    return NumberedActiveResEffect("active.sword", res, "add_damage");
                case "buff_shield":
                    return NumberedActiveResEffect("active.shield", res, "add_armor");
                case "buff_skull":
                    return NumberedActiveResEffect("active.skull", res, "body_max");
                case "buff_pen":
                    return Localization.F("active.pen", R.Float(R.Get(buff, "craft_q")));
                case "buff_star":
                    return Localization.F("active.star", R.Float(R.Get(buff, "craft_q")));
                case "buff_plant":
                    return Localization.F("active.plant_inactive");
                case "buff_sins":
                    return Localization.F("active.sins_unverified");
                case "buff_gp_increase":
                    return Localization.F("active.gratitude");
                case "buff_sin_shard":
                    return Localization.F("active.sin_shard");
                default:
                    return null;
            }
        }

        private static int ParseQualityTier(string craftId)
        {
            if (string.IsNullOrEmpty(craftId)) return 0;
            int index = craftId.LastIndexOf(':');
            if (index < 0 || index + 1 >= craftId.Length) return 0;
            int tier;
            return int.TryParse(craftId.Substring(index + 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out tier)
                ? tier
                : 0;
        }

        private static BonusHighlight GetBonusHighlight(string craftId)
        {
            if (craftId.StartsWith("pray:b_faith:", StringComparison.Ordinal))
                return BonusHighlight.Faith;
            if (craftId.StartsWith("pray:b_money:", StringComparison.Ordinal))
                return BonusHighlight.Money;
            return BonusHighlight.None;
        }

        private static void CollectOutputs(object craft, ref int fixedFaith, ref float fixedMoney, List<RewardItem> rewards)
        {
            IEnumerable output = R.Get(craft, "output") as IEnumerable;
            if (output == null) return;

            foreach (object item in output)
            {
                if (item == null) continue;
                string id = R.Id(item) ?? string.Empty;
                int value = R.Int(R.Get(item, "value"));
                if (string.Equals(id, "faith", StringComparison.Ordinal)) fixedFaith += value;
                else if (string.Equals(id, "money", StringComparison.Ordinal)) fixedMoney += value / 100f;
                else if (!string.IsNullOrEmpty(id) && value > 0) rewards.Add(new RewardItem(id, value));
            }
        }

        private static SpecialInfo BuildSpecial(object craft, string eventId, List<RewardItem> rewards)
        {
            List<string> parts = new List<string>();
            string iconName = null;

            // The event family is the verified discriminator for the BSS prayer. Keep
            // the player-facing mechanic concise instead of copying the flavor sentence.
            if (eventId.StartsWith("pray_for_souls_", StringComparison.Ordinal))
                parts.Add(Localization.F("buff.souls_repose"));

            string buffId = R.Get(craft, "buff") as string;
            float duration = R.Float(R.Get(craft, "dur_parameter"));
            SpecialInfo buff = BuildBuffEffect(buffId, duration);
            if (buff != null)
            {
                if (!string.IsNullOrEmpty(buff.Text)) parts.Add(buff.Text);
                if (!string.IsNullOrEmpty(buff.IconName)) iconName = buff.IconName;
            }

            if (rewards.Count > 0)
            {
                List<string> rewardParts = new List<string>();
                foreach (RewardItem reward in rewards)
                    rewardParts.Add(BuildRewardText(reward));
                parts.Add(Localization.F("forecast.reward", string.Join(", ", rewardParts.ToArray())));
            }

            return parts.Count == 0 ? null : new SpecialInfo(string.Join(" · ", parts.ToArray()), iconName);
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

        private static SpecialInfo BuildBuffEffect(string buffId, float duration)
        {
            if (string.IsNullOrEmpty(buffId)) return null;

            object buff = R.BalanceData(buffId, "BuffDefinition", true);
            object res = buff == null ? null : R.Get(buff, "res");
            string iconName = GetBuffIconName(buff);

            string text;
            switch (buffId)
            {
                case "buff_sword":
                    text = NumberedActiveResEffect("active.sword", res, "add_damage");
                    break;
                case "buff_shield":
                    text = NumberedActiveResEffect("active.shield", res, "add_armor");
                    break;
                case "buff_skull":
                    text = NumberedActiveResEffect("active.skull", res, "body_max");
                    break;
                case "buff_pen":
                    text = buff == null ? null : Localization.F("active.pen", R.Float(R.Get(buff, "craft_q")));
                    break;
                case "buff_star":
                    text = buff == null ? null : Localization.F("active.star", R.Float(R.Get(buff, "craft_q")));
                    break;
                case "buff_plant":
                    text = Localization.F("active.plant_inactive");
                    break;
                case "buff_sins":
                    text = Localization.F("active.sins_unverified");
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

            if (!string.IsNullOrEmpty(text) && duration > 0.0001f)
                text += " · " + Localization.F("active.timer_days", DurationParameterToGameDays(duration));

            return string.IsNullOrEmpty(text) ? null : new SpecialInfo(text, iconName);
        }

        private static float DurationParameterToGameDays(float durationMinutes)
        {
            if (durationMinutes <= 0.0001f) return 0f;

            if (_fromTimeKToSeconds == null)
            {
                Type timeOfDay = R.GameType("TimeOfDay");
                _fromTimeKToSeconds = R.Method(timeOfDay, "FromTimeKToSeconds", true, new[] { typeof(float) });
            }
            if (_fromTimeKToSeconds == null)
                throw new MissingMethodException("TimeOfDay.FromTimeKToSeconds(float)");

            float secondsPerDay = R.Float(_fromTimeKToSeconds.Invoke(null, new object[] { 1f }));
            if (secondsPerDay <= 0.0001f)
                throw new InvalidOperationException("TimeOfDay.FromTimeKToSeconds(1) returned an invalid day length.");

            // dur_parameter is the verified prayer-buff duration in real-time minutes.
            // Convert it through the game's effective day length. Longer Days patches
            // the same TimeOfDay method, so this automatically reflects that mod with
            // no dependency or per-mod branch.
            return durationMinutes * 60f / secondsPerDay;
        }

        private static string NumberedActiveResEffect(string key, object res, string resKey)
        {
            if (res == null) return null;
            float value = R.GameResGet(res, resKey);
            if (Math.Abs(value) < 0.0001f) return null;
            return Localization.F(key, value);
        }

        private static string GetBuffIconName(object buff)
        {
            if (buff == null) return null;
            MethodInfo method = R.Method(buff.GetType(), "GetIconName", false, 0);
            object value = method == null ? null : method.Invoke(buff, null);
            return value == null ? null : value.ToString();
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
    }
}
