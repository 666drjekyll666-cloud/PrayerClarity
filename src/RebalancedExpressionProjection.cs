using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace PrayerClarity
{
    internal static class RebalancedExpressionProjection
    {
        private const string StockPlantTerm = "0.2*WGOpar(\"buff_plant\")";
        private const string RebalancedPlantTerm = "Ppar(\"buff_plant\")*Ppar(\"" + RebalancedTierState.PlantReductionParam + "\")";
        private const string StockConfessionExpression = "SetPpar(\"confession_probability\", 0.15)";
        private const string RebalancedConfessionExpression = "SetPpar(\"confession_probability\", 0.15 + Ppar(\"buff_sins\") * Ppar(\"" + RebalancedTierState.ConfessionBonusParam + "\"))";

        private static readonly string[] PlantCraftIds =
        {
            "garden_wheat_growing",
            "garden_cannabis_growing",
            "garden_cabbage_growing",
            "garden_carrot_growing",
            "garden_beet_growing",
            "garden_grapes_growing",
            "garden_lentils_growing",
            "garden_pumpkin_growing",
            "garden_onion_growing",
            "garden_hop_growing",
            "garden_wheat_grow_desk_planting",
            "garden_cabbage_grow_desk_planting",
            "garden_carrot_grow_desk_planting",
            "garden_beet_grow_desk_planting",
            "garden_lentils_grow_desk_planting_1",
            "garden_lentils_grow_desk_planting_2",
            "garden_lentils_grow_desk_planting_3",
            "garden_pumpkin_grow_desk_planting_1",
            "garden_pumpkin_grow_desk_planting_2",
            "garden_pumpkin_grow_desk_planting_3",
            "garden_onion_grow_desk_planting_1",
            "garden_onion_grow_desk_planting_2",
            "garden_onion_grow_desk_planting_3",
            "garden_grapes_grow_vineyard_planting_1",
            "garden_grapes_grow_vineyard_planting_2",
            "garden_grapes_grow_vineyard_planting_3",
            "garden_hops_grow_vineyard_planting_1",
            "garden_hops_grow_vineyard_planting_2",
            "garden_hops_grow_vineyard_planting_3",
            "refugee_garden_wheat_grow_desk_planting",
            "refugee_garden_cabbage_grow_desk_planting",
            "refugee_garden_carrot_grow_desk_planting",
            "refugee_garden_beet_grow_desk_planting",
            "refugee_garden_lentils_grow_desk_planting_1",
            "refugee_garden_lentils_grow_desk_planting_2",
            "refugee_garden_lentils_grow_desk_planting_3",
            "refugee_garden_pumpkin_grow_desk_planting_1",
            "refugee_garden_pumpkin_grow_desk_planting_2",
            "refugee_garden_pumpkin_grow_desk_planting_3",
            "refugee_garden_onion_grow_desk_planting_1",
            "refugee_garden_onion_grow_desk_planting_2",
            "refugee_garden_onion_grow_desk_planting_3",
            "bush_berry_garden",
            "bush_berry_garden_growing",
            "bush_1_berry_respawn",
            "bush_2_berry_respawn",
            "bush_3_berry_respawn",
            "tree_apple_garden_growing",
            "tree_apple_garden_crops_growing",
            "spawn_tree",
            "tree_growing",
            "flower_spawn",
            "hiccup_grass_spawn",
            "mushroom_spawn",
            "spawn_tree_apple",
            "tree_apple_growing_1",
            "tree_apple_growing_2",
            "tree_apple_growing_3"
        };

        private static Type _smartExpressionType;
        private static MethodInfo _parseExpression;
        private static MethodInfo _getRawExpression;

        internal static void Apply()
        {
            ResolveSmartExpressionApi();
            ApplyRoots();
            ApplyRepentance();
        }

        private static void ApplyRoots()
        {
            foreach (string craftId in PlantCraftIds)
            {
                object craft = R.BalanceData(craftId, "CraftDefinition", true);
                if (craft == null)
                    throw new MissingMemberException("Missing verified Roots consumer craft " + craftId);

                object craftTime = R.Get(craft, "craft_time");
                string raw = Raw(craftTime);
                if (string.IsNullOrEmpty(raw))
                    throw new InvalidOperationException("Roots consumer " + craftId + " has no craft_time expression.");

                string transformed;
                if (raw.Contains(RebalancedPlantTerm))
                    transformed = raw;
                else
                {
                    int first = raw.IndexOf(StockPlantTerm, StringComparison.Ordinal);
                    if (first < 0 || raw.IndexOf(StockPlantTerm, first + StockPlantTerm.Length, StringComparison.Ordinal) >= 0)
                        throw new InvalidOperationException("Roots consumer " + craftId + " no longer contains exactly one verified stock prayer term: " + raw);
                    transformed = raw.Replace(StockPlantTerm, RebalancedPlantTerm);
                }

                R.Set(craft, "craft_time", Parse(transformed));
            }
        }

        private static void ApplyRepentance()
        {
            object logic = R.BalanceData("church_budka_roll", "LogicDefinition", true);
            if (logic == null) throw new MissingMemberException("LogicDefinition church_budka_roll");

            IList expressions = R.Get(logic, "execute_expressions") as IList;
            if (expressions == null || expressions.Count != 1)
                throw new InvalidOperationException("church_budka_roll execute_expressions shape changed.");

            string raw = Raw(expressions[0]);
            if (!string.Equals(raw, StockConfessionExpression, StringComparison.Ordinal) &&
                !string.Equals(raw, RebalancedConfessionExpression, StringComparison.Ordinal))
                throw new InvalidOperationException("church_budka_roll reset expression changed: " + raw);

            expressions[0] = Parse(RebalancedConfessionExpression);
        }

        private static object Parse(string expression)
        {
            return _parseExpression.Invoke(null, new object[] { expression });
        }

        private static string Raw(object expression)
        {
            return expression == null ? null : _getRawExpression.Invoke(expression, null) as string;
        }

        private static void ResolveSmartExpressionApi()
        {
            if (_smartExpressionType != null) return;

            _smartExpressionType = R.GameType("SmartExpression");
            if (_smartExpressionType == null) throw new MissingMemberException("SmartExpression");

            _parseExpression = _smartExpressionType.GetMethod(
                "ParseExpression",
                R.Stat,
                null,
                new[] { typeof(string) },
                null);
            _getRawExpression = _smartExpressionType.GetMethod(
                "GetRawExpressionString",
                R.Inst,
                null,
                Type.EmptyTypes,
                null);

            if (_parseExpression == null) throw new MissingMethodException("SmartExpression.ParseExpression(string)");
            if (_getRawExpression == null) throw new MissingMethodException("SmartExpression.GetRawExpressionString()");
        }
    }
}
