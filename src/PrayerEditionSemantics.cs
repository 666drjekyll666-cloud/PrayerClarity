namespace PrayerClarity
{
    internal delegate bool PrayerTierEffectResolver(string craftId, string buffId, out string text, out string semanticKey);
    internal delegate bool PrayerActiveEffectResolver(string buffId, out string text);

    // Shared presentation seam for sibling editions. Vanilla leaves it unconfigured;
    // Rebalanced installs its provider during plugin initialization. Shared Clarity
    // code therefore remains edition-agnostic and independently buildable.
    internal static class PrayerEditionSemantics
    {
        private static PrayerTierEffectResolver _tierEffect;
        private static PrayerActiveEffectResolver _activeEffect;

        internal static void Install(PrayerTierEffectResolver tierEffect, PrayerActiveEffectResolver activeEffect)
        {
            _tierEffect = tierEffect;
            _activeEffect = activeEffect;
        }

        internal static bool TryBuildTierEffect(string craftId, string buffId, out string text, out string semanticKey)
        {
            text = null;
            semanticKey = null;
            return _tierEffect != null && _tierEffect(craftId, buffId, out text, out semanticKey);
        }

        internal static bool TryBuildActiveEffect(string buffId, out string text)
        {
            text = null;
            return _activeEffect != null && _activeEffect(buffId, out text);
        }
    }
}
