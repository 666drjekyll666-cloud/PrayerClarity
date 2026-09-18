using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;

namespace PrayerClarity
{
    internal static class RebalancedStaticProjection
    {
        private static ManualLogSource _log;
        private static bool _projectedForCurrentLoad;
        private static bool _projectionFailed;

        internal static void Install(string harmonyId, ManualLogSource log)
        {
            _log = log;
            Type craftComponent = R.GameType("CraftComponent");
            if (craftComponent == null) throw new MissingMemberException("CraftComponent");

            MethodInfo clear = FindMethod(craftComponent, "ClearCraftsListOnGameStart", 0);
            MethodInfo fill = FindMethod(craftComponent, "FillCraftsList", 0);
            if (clear == null) throw new MissingMethodException("CraftComponent.ClearCraftsListOnGameStart()");
            if (fill == null) throw new MissingMethodException("CraftComponent.FillCraftsList()");

            R.Patch(harmonyId + ".projection.reset", typeof(RebalancedStaticProjection), clear, nameof(ClearCraftsListOnGameStartPostfix));
            R.Patch(harmonyId + ".projection.apply", typeof(RebalancedStaticProjection), fill, nameof(FillCraftsListPostfix));
        }

        private static void ClearCraftsListOnGameStartPostfix()
        {
            _projectedForCurrentLoad = false;
            _projectionFailed = false;
        }

        private static void FillCraftsListPostfix()
        {
            if (_projectedForCurrentLoad || _projectionFailed) return;

            try
            {
                ApplyOnce();
                _projectedForCurrentLoad = true;
                _log?.LogInfo("PrayerClarity: Rebalanced static prayer projection applied for this save load.");
            }
            catch (Exception ex)
            {
                _projectionFailed = true;
                _log?.LogError("PrayerClarity: Rebalanced static prayer projection failed closed for this save load. " + ex);
            }
        }

        private static void ApplyOnce()
        {
            RebalancedExpressionProjection.Apply();
            ApplyCombatAliasProjection();
            RetireProtectionCrafting();

            foreach (RebalancedPrayerRule rule in RebalancedRuleSet.All)
            {
                if (!HasStaticProjection(rule)) continue;

                for (int tier = 1; tier <= 3; tier++)
                {
                    string craftId = RebalancedRuleSet.CraftId(rule.PrayerId, tier);
                    object craft = R.BalanceData(craftId, "CraftDefinition", true);
                    if (craft == null)
                    {
                        if (rule.OptionalDlc) continue;
                        throw new MissingMemberException("Missing required prayer craft " + craftId);
                    }

                    ApplyStockOwnedFields(craft, rule, tier);
                }
            }
        }

        private static void ApplyCombatAliasProjection()
        {
            for (int tier = 1; tier <= 3; tier++)
            {
                string craftId = RebalancedRuleSet.CraftId("b_shield", tier);
                object craft = R.BalanceData(craftId, "CraftDefinition", true);
                if (craft == null) throw new MissingMemberException("Missing legacy Combat alias prayer craft " + craftId);

                string currentBuff = Convert.ToString(R.Get(craft, "buff"));
                if (!string.Equals(currentBuff, "buff_shield", StringComparison.Ordinal) &&
                    !string.Equals(currentBuff, "buff_sword", StringComparison.Ordinal))
                    throw new InvalidOperationException(craftId + " buff target changed unexpectedly: " + currentBuff);

                R.Set(craft, "buff", "buff_sword");
            }
        }

        private static void RetireProtectionCrafting()
        {
            object tech = R.BalanceData("Martial skills", "TechDefinition", true);
            if (tech == null) throw new MissingMemberException("TechDefinition Martial skills");

            IList crafts = R.Get(tech, "crafts") as IList;
            if (crafts == null) throw new MissingMemberException("Martial skills", "crafts");
            RemoveStringEntries(crafts, "b_shield", "@b_shield_2");

            IList unlocks = R.Get(tech, "_unlocks_list") as IList;
            if (unlocks != null)
            {
                for (int i = unlocks.Count - 1; i >= 0; i--)
                {
                    string id = Convert.ToString(R.Get(unlocks[i], "id"));
                    if (string.Equals(id, "b_shield", StringComparison.Ordinal) ||
                        string.Equals(id, "b_shield_2", StringComparison.Ordinal))
                        unlocks.RemoveAt(i);
                }
            }

            HideCraftDefinition("b_shield");
            HideCraftDefinition("b_shield_2");
        }

        private static void RemoveStringEntries(IList list, params string[] ids)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                string value = Convert.ToString(list[i]);
                if (ids.Any(id => string.Equals(value, id, StringComparison.Ordinal)))
                    list.RemoveAt(i);
            }
        }

        private static void HideCraftDefinition(string id)
        {
            object craft = R.BalanceData(id, "CraftDefinition", true);
            if (craft == null) throw new MissingMemberException("CraftDefinition " + id);
            object hidden = R.Get(craft, "hidden");
            if (hidden == null) throw new MissingMemberException(id, "hidden");
            R.Set(craft, "hidden", true);
        }

        private static bool HasStaticProjection(RebalancedPrayerRule rule)
        {
            return rule.Requirements != null ||
                   rule.FaithBonusRates != null ||
                   rule.MoneyBonusRates != null ||
                   rule.RemoveFixedFaith ||
                   rule.RemoveFixedMoney ||
                   rule.FixedFaithBonuses != null ||
                   rule.FixedMoneyBonusesCents != null ||
                   !string.IsNullOrEmpty(rule.SuccessRewardBaseItemId);
        }

        private static void ApplyStockOwnedFields(object craft, RebalancedPrayerRule rule, int tier)
        {
            if (rule.Requirements != null)
                R.Set(craft, "needs_quality", rule.TierValue(rule.Requirements, tier));
            if (rule.FaithBonusRates != null)
                R.Set(craft, "k_faith", rule.TierValue(rule.FaithBonusRates, tier));
            if (rule.MoneyBonusRates != null)
                R.Set(craft, "k_money", rule.TierValue(rule.MoneyBonusRates, tier));

            bool replaceFaith = rule.RemoveFixedFaith || rule.FixedFaithBonuses != null;
            bool replaceMoney = rule.RemoveFixedMoney || rule.FixedMoneyBonusesCents != null;
            if (replaceFaith || replaceMoney)
                RemovePrayerOwnedFixedOutputs(craft, replaceFaith, replaceMoney);

            if (rule.FixedFaithBonuses != null)
                AddPrayerOwnedFixedOutput(craft, "faith", rule.TierValue(rule.FixedFaithBonuses, tier, 0));
            if (rule.FixedMoneyBonusesCents != null)
                AddPrayerOwnedFixedOutput(craft, "money", rule.TierValue(rule.FixedMoneyBonusesCents, tier, 0));

            ApplySuccessReward(craft, rule, tier);
        }

        private static void RemovePrayerOwnedFixedOutputs(object craft, bool removeFaith, bool removeMoney)
        {
            IList output = RequireOutput(craft);
            for (int i = output.Count - 1; i >= 0; i--)
            {
                object item = output[i];
                string id = R.Id(item) ?? string.Empty;
                if ((removeFaith && string.Equals(id, "faith", StringComparison.Ordinal)) ||
                    (removeMoney && string.Equals(id, "money", StringComparison.Ordinal)))
                    output.RemoveAt(i);
            }
        }

        private static void AddPrayerOwnedFixedOutput(object craft, string itemId, int count)
        {
            if (count <= 0) return;

            Type itemType = R.GameType("Item");
            ConstructorInfo ctor = itemType?.GetConstructor(new[] { typeof(string), typeof(int) });
            if (ctor == null) throw new MissingMethodException("Item(string,int)");

            RequireOutput(craft).Add(ctor.Invoke(new object[] { itemId, count }));
        }

        private static void ApplySuccessReward(object craft, RebalancedPrayerRule rule, int tier)
        {
            if (string.IsNullOrEmpty(rule.SuccessRewardBaseItemId)) return;

            int rewardTier = rule.TierValue(rule.SuccessRewardQualityTiers, tier, 0);
            int count = rule.TierValue(rule.SuccessRewardCounts, tier, 0);
            IList output = RequireOutput(craft);

            for (int quality = 1; quality <= 3; quality++)
            {
                string ownedId = rule.SuccessRewardBaseItemId + ":" + quality;
                for (int i = output.Count - 1; i >= 0; i--)
                    if (string.Equals(R.Id(output[i]), ownedId, StringComparison.Ordinal))
                        output.RemoveAt(i);
            }

            if (rewardTier <= 0 || count <= 0) return;

            string rewardId = rule.SuccessRewardBaseItemId + ":" + rewardTier;
            Type itemType = R.GameType("Item");
            ConstructorInfo ctor = itemType?.GetConstructor(new[] { typeof(string), typeof(int) });
            if (ctor == null) throw new MissingMethodException("Item(string,int)");
            output.Add(ctor.Invoke(new object[] { rewardId, count }));
        }

        private static IList RequireOutput(object craft)
        {
            IList output = R.Get(craft, "output") as IList;
            if (output == null) throw new MissingMemberException(R.Id(craft) ?? "CraftDefinition", "output");
            return output;
        }

        private static MethodInfo FindMethod(Type type, string name, int parameterCount)
        {
            if (type == null) return null;
            return type.GetMethods(R.Inst | R.Stat)
                .FirstOrDefault(method => method.Name == name && method.GetParameters().Length == parameterCount);
        }
    }
}
