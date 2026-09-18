using System;
using System.Reflection;

namespace PrayerClarity
{
    internal static class RebalancedExpressionProjection
    {
        private const string RebalancedCombatTickExpression =
            "AddPpar(\"hp\", Ppar(\"" + RebalancedTierState.CombatRegenParam + "\"))";

        private static Type _smartExpressionType;
        private static MethodInfo _parseExpression;
        private static MethodInfo _getRawExpression;

        internal static void Apply()
        {
            ResolveSmartExpressionApi();

            object combatBuff = R.BalanceData("buff_sword", "BuffDefinition", true);
            if (combatBuff == null)
                throw new MissingMemberException("BuffDefinition buff_sword");

            float tickPeriod = R.Float(R.Get(combatBuff, "tick_period"));
            if (Math.Abs(tickPeriod) > 0.0001f && Math.Abs(tickPeriod - 1f) > 0.0001f)
                throw new InvalidOperationException("buff_sword tick_period changed unexpectedly: " + tickPeriod);

            string raw = Raw(R.Get(combatBuff, "se_tick"));
            if (!string.Equals(raw, RebalancedCombatTickExpression, StringComparison.Ordinal))
            {
                if (!string.IsNullOrEmpty(raw))
                    throw new InvalidOperationException("buff_sword se_tick changed unexpectedly: " + raw);

                R.Set(combatBuff, "se_tick", Parse(RebalancedCombatTickExpression));
            }

            if (Math.Abs(tickPeriod - 1f) > 0.0001f)
                R.Set(combatBuff, "tick_period", 1f);
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
            if (_smartExpressionType == null)
                throw new MissingMemberException("SmartExpression");

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

            if (_parseExpression == null)
                throw new MissingMethodException("SmartExpression.ParseExpression(string)");
            if (_getRawExpression == null)
                throw new MissingMethodException("SmartExpression.GetRawExpressionString()");
        }
    }
}
