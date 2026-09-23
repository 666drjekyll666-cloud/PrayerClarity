namespace PrayerClarity
{
    internal delegate bool PrayerTierEffectResolver(string craftId, string buffId, out string text, out string semanticKey);
    internal delegate bool PrayerActiveEffectResolver(string buffId, out string text);
    internal delegate bool PrayerTechnologyEffectResolver(string craftId, out string sharedText, out string tierText);
    internal delegate bool PrayerSoulConversionResolver(string craftId, out int cap, out int conversion);

    // Shared presentation seam for sibling editions. Vanilla leaves it unconfigured;
    // Rebalanced installs its provider during plugin initialization. Shared Clarity
    // code therefore remains edition-agnostic and independently buildable.
    internal static class PrayerEditionSemantics
    {
        private static PrayerTierEffectResolver _tierEffect;
        private static PrayerActiveEffectResolver _activeEffect;
        private static PrayerTechnologyEffectResolver _technologyEffect;
        private static PrayerSoulConversionResolver _soulConversion;

        internal static void Install(
            PrayerTierEffectResolver tierEffect,
            PrayerActiveEffectResolver activeEffect,
            PrayerTechnologyEffectResolver technologyEffect = null,
            PrayerSoulConversionResolver soulConversion = null)
        {
            _tierEffect = tierEffect;
            _activeEffect = activeEffect;
            _technologyEffect = technologyEffect;
            _soulConversion = soulConversion;
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

        internal static bool HasTechnologyProvider
        {
            get { return _technologyEffect != null; }
        }

        internal static bool TryBuildTechnologyEffect(string craftId, out string sharedText, out string tierText)
        {
            sharedText = null;
            tierText = null;
            return _technologyEffect != null && _technologyEffect(craftId, out sharedText, out tierText);
        }

        internal static bool TryGetSoulConversion(string craftId, out int cap, out int conversion)
        {
            cap = 0;
            conversion = 0;
            return _soulConversion != null && _soulConversion(craftId, out cap, out conversion);
        }
    }
}
