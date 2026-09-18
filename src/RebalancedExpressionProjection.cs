using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PrayerClarity
{
    internal static class RebalancedExpressionProjection
    {
        private const string StockConfessionExpression = "SetPpar(\"confession_probability\", 0.15)";
        private const string RebalancedConfessionExpression = "SetPpar(\"confession_probability\", 0.15 + Ppar(\"buff_sins\") * Ppar(\"" + RebalancedTierState.ConfessionBonusParam + "\"))";
        private const string RebalancedCombatTickExpression = "AddPpar(\"hp\", Ppar(\"" + RebalancedTierState.CombatRegenParam + "\"))";

        private sealed class ExpressionReplacement
        {
            internal readonly object Owner;
            internal readonly string Member;
            internal readonly string Expression;
            internal readonly IList ListOwner;
            internal readonly int ListIndex;

            internal ExpressionReplacement(object owner, string member, string expression)
            {
                Owner = owner;
                Member = member;
                Expression = expression;
                ListOwner = null;
                ListIndex = -1;
            }

            internal ExpressionReplacement(IList listOwner, int listIndex, string expression)
            {
                Owner = null;
                Member = null;
                Expression = expression;
                ListOwner = listOwner;
                ListIndex = listIndex;
            }

            internal void Apply()
            {
                object parsed = Parse(Expression);
                if (ListOwner != null)
                    ListOwner[ListIndex] = parsed;
                else
                    R.Set(Owner, Member, parsed);
            }
        }

        private static Type _smartExpressionType;
        private static MethodInfo _parseExpression;
        private static MethodInfo _getRawExpression;

        internal static void Apply()
        {
            ResolveSmartExpressionApi();

            var replacements = new List<ExpressionReplacement>();
            CollectRepentanceReplacement(replacements);

            object combatBuff;
            bool setCombatTickPeriod;
            CollectCombatBuffReplacement(replacements, out combatBuff, out setCombatTickPeriod);

            foreach (ExpressionReplacement replacement in replacements)
                replacement.Apply();

            if (setCombatTickPeriod)
                R.Set(combatBuff, "tick_period", 1f);
        }

        private static void CollectRepentanceReplacement(List<ExpressionReplacement> replacements)
        {
            object logic = R.BalanceData("church_budka_roll", "LogicDefinition", true);
            if (logic == null) throw new MissingMemberException("LogicDefinition church_budka_roll");

            IList expressions = R.Get(logic, "execute_expressions") as IList;
            if (expressions == null || expressions.Count != 1)
                throw new InvalidOperationException("church_budka_roll execute_expressions shape changed.");

            string raw = Raw(expressions[0]);
            if (string.Equals(raw, RebalancedConfessionExpression, StringComparison.Ordinal))
                return;
            if (!string.Equals(raw, StockConfessionExpression, StringComparison.Ordinal))
                throw new InvalidOperationException("church_budka_roll reset expression changed: " + raw);

            replacements.Add(new ExpressionReplacement(expressions, 0, RebalancedConfessionExpression));
        }

        private static void CollectCombatBuffReplacement(List<ExpressionReplacement> replacements, out object combatBuff, out bool setTickPeriod)
        {
            combatBuff = R.BalanceData("buff_sword", "BuffDefinition", true);
            if (combatBuff == null) throw new MissingMemberException("BuffDefinition buff_sword");

            float tickPeriod = R.Float(R.Get(combatBuff, "tick_period"));
            if (Math.Abs(tickPeriod) > 0.0001f && Math.Abs(tickPeriod - 1f) > 0.0001f)
                throw new InvalidOperationException("buff_sword tick_period changed unexpectedly: " + tickPeriod);
            setTickPeriod = Math.Abs(tickPeriod - 1f) > 0.0001f;

            string raw = Raw(R.Get(combatBuff, "se_tick"));
            if (string.Equals(raw, RebalancedCombatTickExpression, StringComparison.Ordinal))
                return;
            if (!string.IsNullOrEmpty(raw))
                throw new InvalidOperationException("buff_sword se_tick changed unexpectedly: " + raw);

            replacements.Add(new ExpressionReplacement(combatBuff, "se_tick", RebalancedCombatTickExpression));
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
