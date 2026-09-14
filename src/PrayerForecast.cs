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
        internal sealed class Result
        {
            internal int BaseFaith;
            internal float BaseMoney;
            internal int BonusFaith;
            internal float BonusMoney;
            internal int ChancePercent;
            internal float GraveyardQuality;
            internal bool UsesSoulGratitude;
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

        internal static Result Build(object prayGui, float chance)
        {
            object craft = R.Get(prayGui, "pray_craft");
            if (craft == null) return null;

            string craftId = R.Id(craft) ?? string.Empty;
            if (!craftId.StartsWith("pray:", StringComparison.Ordinal)) return null;

            string eventId = R.Get(craft, "linked_sub_id") as string;
            if (string.IsNullOrEmpty(eventId)) return null;

            Localization.UseCurrentGameLanguage();

            object prayEvent = R.BalanceData(eventId, "PrayEventDefinition", false);
            int baseFaith = Mathf.Max(0, Mathf.RoundToInt(R.SmartFloat(R.Get(prayEvent, "faith"))));
            float baseMoney = Mathf.Max(0f, R.SmartFloat(R.Get(prayEvent, "money")));

            int fixedFaith = 0;
            float fixedMoney = 0f;
            List<RewardItem> rewards = new List<RewardItem>();
            CollectOutputs(craft, ref fixedFaith, ref fixedMoney, rewards);

            float kFaith = R.Float(R.Get(craft, "k_faith"));
            float kMoney = R.Float(R.Get(craft, "k_money"));

            int bonusFaith = fixedFaith + Mathf.RoundToInt(baseFaith * kFaith);
            float bonusMoney = fixedMoney + Mathf.Round(baseMoney * kMoney * 100f) / 100f;
            SpecialInfo special = BuildSpecial(craft, rewards);

            return new Result
            {
                BaseFaith = baseFaith,
                BaseMoney = baseMoney,
                BonusFaith = bonusFaith,
                BonusMoney = bonusMoney,
                ChancePercent = Mathf.RoundToInt(Mathf.Clamp01(chance) * 100f),
                GraveyardQuality = R.ZoneQuality("graveyard"),
                UsesSoulGratitude = eventId.StartsWith("pray_for_souls_", StringComparison.Ordinal),
                SpecialText = special == null ? null : special.Text,
                SpecialIconName = special == null ? null : special.IconName
            };
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

        private static SpecialInfo BuildSpecial(object craft, List<RewardItem> rewards)
        {
            List<string> parts = new List<string>();
            string iconName = null;

            string buffId = R.Get(craft, "buff") as string;
            float duration = R.Float(R.Get(craft, "dur_parameter"));
            SpecialInfo buff = BuildBuffEffect(buffId, duration);
            if (buff != null)
            {
                if (!string.IsNullOrEmpty(buff.Text)) parts.Add(buff.Text);
                iconName = buff.IconName;
            }

            if (rewards.Count > 0)
            {
                List<string> rewardParts = new List<string>();
                foreach (RewardItem reward in rewards)
                {
                    string name = R.VanillaLocalize(reward.Id);
                    rewardParts.Add(name + " ×" + reward.Value.ToString(CultureInfo.InvariantCulture));
                    if (string.IsNullOrEmpty(iconName)) iconName = GetItemIconName(reward.Id);
                }
                parts.Add(Localization.F("forecast.reward", string.Join(", ", rewardParts.ToArray())));
            }

            return parts.Count == 0 ? null : new SpecialInfo(string.Join(" · ", parts.ToArray()), iconName);
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
                    text = NumberedResEffect("buff.sword", res, "add_damage", duration);
                    break;
                case "buff_shield":
                    text = NumberedResEffect("buff.shield", res, "add_armor", duration);
                    break;
                case "buff_skull":
                    text = NumberedResEffect("buff.skull", res, "body_max", duration);
                    break;
                case "buff_pen":
                    text = buff == null ? null : Localization.F("buff.pen", R.Float(R.Get(buff, "craft_q")), duration);
                    break;
                case "buff_star":
                    text = buff == null ? null : Localization.F("buff.star", R.Float(R.Get(buff, "craft_q")), duration);
                    break;
                case "buff_plant":
                    text = Localization.F("buff.plant_inactive", duration);
                    break;
                case "buff_sins":
                    text = Localization.F("buff.sins_unverified", duration);
                    break;
                case "buff_gp_increase":
                    text = Localization.F("buff.gratitude", duration);
                    break;
                case "buff_sin_shard":
                    text = Localization.F("buff.sin_shard", duration);
                    if (string.IsNullOrEmpty(iconName)) iconName = GetItemIconName("sin_shard");
                    break;
                default:
                    text = null;
                    break;
            }

            return string.IsNullOrEmpty(text) ? null : new SpecialInfo(text, iconName);
        }

        private static string NumberedResEffect(string key, object res, string resKey, float duration)
        {
            if (res == null) return null;
            float value = R.GameResGet(res, resKey);
            if (Math.Abs(value) < 0.0001f) return null;
            return Localization.F(key, value, duration);
        }

        private static string GetBuffIconName(object buff)
        {
            if (buff == null) return null;
            MethodInfo method = R.Method(buff.GetType(), "GetIconName", false, 0);
            object value = method == null ? null : method.Invoke(buff, null);
            return value == null ? null : value.ToString();
        }

        private static string GetItemIconName(string itemId)
        {
            object item = R.BalanceData(itemId, "ItemDefinition", true);
            if (item == null) return null;

            string icon = R.Get(item, "icon") as string;
            if (!string.IsNullOrEmpty(icon)) return icon;

            icon = R.Get(item, "custom_ovr_icon") as string;
            return string.IsNullOrEmpty(icon) ? null : icon;
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
