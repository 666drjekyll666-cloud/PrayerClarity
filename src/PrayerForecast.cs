using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace PrayerClarity
{
    internal static class PrayerForecast
    {
        internal static string Build(object prayGui)
        {
            object craft = R.Get(prayGui, "pray_craft");
            if (craft == null) return null;

            string craftId = R.Id(craft) ?? string.Empty;
            if (!craftId.StartsWith("pray:", StringComparison.Ordinal)) return null;

            string eventId = R.Get(craft, "linked_sub_id") as string;
            if (string.IsNullOrEmpty(eventId)) return null;

            Localization.UseCurrentGameLanguage();

            object prayEvent = R.BalanceData(eventId, false);
            int baseFaith = Mathf.Max(0, Mathf.RoundToInt(R.SmartFloat(R.Get(prayEvent, "faith"))));
            float baseMoney = Mathf.Max(0f, R.SmartFloat(R.Get(prayEvent, "money")));

            int fixedFaith = 0;
            float fixedMoney = 0f;
            List<RewardItem> rewards = new List<RewardItem>();
            CollectOutputs(craft, ref fixedFaith, ref fixedMoney, rewards);

            float kFaith = R.Float(R.Get(craft, "k_faith"));
            float kMoney = R.Float(R.Get(craft, "k_money"));

            int successFaith = baseFaith + fixedFaith + Mathf.RoundToInt(baseFaith * kFaith);
            float successMoney = baseMoney + fixedMoney + Mathf.Round(baseMoney * kMoney * 100f) / 100f;

            string outcomes = Localization.F("forecast.outcomes",
                successFaith,
                R.FormatMoney(successMoney),
                baseFaith,
                R.FormatMoney(baseMoney));

            string special = BuildSpecial(craft, rewards);
            return string.IsNullOrEmpty(special) ? outcomes : outcomes + "\n" + special;
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

        private static string BuildSpecial(object craft, List<RewardItem> rewards)
        {
            List<string> parts = new List<string>();

            string buffId = R.Get(craft, "buff") as string;
            float duration = R.Float(R.Get(craft, "dur_parameter"));
            string buffEffect = BuildBuffEffect(buffId, duration);
            if (!string.IsNullOrEmpty(buffEffect)) parts.Add(Localization.F("forecast.effect", buffEffect));

            if (rewards.Count > 0)
            {
                List<string> rewardParts = new List<string>();
                foreach (RewardItem reward in rewards)
                {
                    string name = R.VanillaLocalize(reward.Id);
                    rewardParts.Add(name + " ×" + reward.Value.ToString(CultureInfo.InvariantCulture));
                }
                parts.Add(Localization.F("forecast.reward", string.Join(", ", rewardParts.ToArray())));
            }

            return string.Join(" | ", parts.ToArray());
        }

        private static string BuildBuffEffect(string buffId, float duration)
        {
            if (string.IsNullOrEmpty(buffId)) return null;

            object buff = R.BalanceData(buffId, true);
            object res = buff == null ? null : R.Get(buff, "res");

            switch (buffId)
            {
                case "buff_sword":
                    return NumberedResEffect("buff.sword", res, "add_damage", duration);
                case "buff_shield":
                    return NumberedResEffect("buff.shield", res, "add_armor", duration);
                case "buff_skull":
                    return NumberedResEffect("buff.skull", res, "body_max", duration);
                case "buff_pen":
                    return buff == null ? null : Localization.F("buff.pen", R.Float(R.Get(buff, "craft_q")), duration);
                case "buff_star":
                    return buff == null ? null : Localization.F("buff.star", R.Float(R.Get(buff, "craft_q")), duration);
                case "buff_plant":
                    return Localization.F("buff.plant_inactive", duration);
                case "buff_sins":
                    return Localization.F("buff.sins_unverified", duration);
                case "buff_gp_increase":
                    return Localization.F("buff.gratitude", duration);
                case "buff_sin_shard":
                    return Localization.F("buff.sin_shard", duration);
                default:
                    return null;
            }
        }

        private static string NumberedResEffect(string key, object res, string resKey, float duration)
        {
            if (res == null) return null;
            float value = R.GameResGet(res, resKey);
            if (Math.Abs(value) < 0.0001f) return null;
            return Localization.F(key, value, duration);
        }

        private sealed class RewardItem
        {
            internal readonly string Id;
            internal readonly int Value;
            internal RewardItem(string id, int value) { Id = id; Value = value; }
        }
    }
}
