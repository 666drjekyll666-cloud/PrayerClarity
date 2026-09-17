using System;
using System.Reflection;
using BepInEx.Logging;

namespace PrayerClarity
{
    internal static class RebalancedCombat
    {
        [ThreadStatic]
        private static bool _armorLookupContext;
        [ThreadStatic]
        private static float _armorBonus;

        private static ManualLogSource _log;
        private static MethodInfo _findBuffById;
        private static bool _runtimeErrorLogged;

        internal static void Install(string harmonyId, ManualLogSource log)
        {
            _log = log;
            ResolveBuffApi();

            Type worldGameObject = R.GameType("WorldGameObject");
            Type damageType = R.GameType("ObjectDefinition+DamageType");
            Type hpAction = R.GameType("HPActionComponent");
            if (worldGameObject == null || damageType == null || hpAction == null)
                throw new MissingMemberException("Combat runtime types are unavailable.");

            MethodInfo getDamage = R.Method(worldGameObject, "GetDamage", false, new[] { damageType });
            MethodInfo getParam = R.Method(worldGameObject, "GetParam", false, new[] { typeof(string), typeof(float) });
            MethodInfo decHp = R.Method(hpAction, "DecHP", false, new[] { typeof(float) });
            if (getDamage == null) throw new MissingMethodException("WorldGameObject.GetDamage(DamageType)");
            if (getParam == null) throw new MissingMethodException("WorldGameObject.GetParam(string,float)");
            if (decHp == null) throw new MissingMethodException("HPActionComponent.DecHP(float)");

            R.Patch(harmonyId + ".combat.damage", typeof(RebalancedCombat), getDamage, nameof(GetDamagePostfix));
            R.PatchHooks(
                harmonyId + ".combat.armor.context",
                typeof(RebalancedCombat),
                decHp,
                nameof(DecHpPrefix),
                null,
                nameof(DecHpFinalizer));
            R.Patch(harmonyId + ".combat.armor.value", typeof(RebalancedCombat), getParam, nameof(GetParamPostfix));
        }

        private static void GetDamagePostfix(object __instance, ref float __result)
        {
            try
            {
                int tier;
                if (!TryGetActiveCombatTier(__instance, out tier)) return;

                float extra = RebalancedTierState.GetPlayerParam(RebalancedTierState.CombatExtraDamageParam, 0f);
                if (extra > 0f)
                    __result += extra;
            }
            catch (Exception ex)
            {
                LogRuntimeError("Combat outgoing-damage extension failed closed", ex);
            }
        }

        private static void DecHpPrefix(object __instance)
        {
            _armorLookupContext = false;
            _armorBonus = 0f;

            try
            {
                object wgo = R.Get(__instance, "wgo");
                int tier;
                if (!TryGetActiveCombatTier(wgo, out tier)) return;

                RebalancedPrayerRule rule;
                if (!RebalancedRuleSet.TryGet("b_sword", out rule)) return;
                float armor = rule.TierValue(rule.CombatArmor, tier, 0f);
                if (armor <= 0f) return;

                _armorBonus = armor;
                _armorLookupContext = true;
            }
            catch (Exception ex)
            {
                _armorLookupContext = false;
                _armorBonus = 0f;
                LogRuntimeError("Combat armor context failed closed", ex);
            }
        }

        private static void GetParamPostfix(object __instance, string param_name, ref float __result)
        {
            if (!_armorLookupContext || _armorBonus <= 0f) return;
            if (!string.Equals(param_name, "add_armor", StringComparison.Ordinal)) return;

            try
            {
                if (!IsPlayer(__instance)) return;
                __result += _armorBonus;
            }
            catch (Exception ex)
            {
                LogRuntimeError("Combat armor value extension failed closed", ex);
            }
        }

        private static void DecHpFinalizer()
        {
            _armorLookupContext = false;
            _armorBonus = 0f;
        }

        private static bool TryGetActiveCombatTier(object wgo, out int tier)
        {
            tier = 0;
            if (wgo == null || !IsPlayer(wgo) || !HasLiveBuff("buff_sword")) return false;

            tier = RebalancedTierState.GetCapturedTier(RebalancedTierState.CombatTierParam);
            return tier >= 1 && tier <= 3;
        }

        private static bool IsPlayer(object wgo)
        {
            object value = R.Get(wgo, "is_player");
            return value != null && Convert.ToBoolean(value);
        }

        private static void ResolveBuffApi()
        {
            if (_findBuffById != null) return;
            Type buffsLogics = R.GameType("BuffsLogics");
            _findBuffById = R.Method(buffsLogics, "FindBuffByID", true, new[] { typeof(string) });
            if (_findBuffById == null)
                throw new MissingMethodException("BuffsLogics.FindBuffByID(string)");
        }

        private static bool HasLiveBuff(string buffId)
        {
            ResolveBuffApi();
            return _findBuffById.Invoke(null, new object[] { buffId }) != null;
        }

        private static void LogRuntimeError(string context, Exception ex)
        {
            if (_runtimeErrorLogged) return;
            _runtimeErrorLogged = true;
            _log?.LogError("PrayerClarity: Rebalanced " + context + ". Stock combat behavior remains active for the affected call. " + ex);
        }
    }
}
