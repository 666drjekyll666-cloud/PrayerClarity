using System;
using System.Collections.Generic;

namespace PrayerClarity
{
    internal static class PrayerLoreResolver
    {
        internal static string ResolveForCraftId(string craftId)
        {
            string family = PrayerFamilyFromCraftId(craftId);
            if (string.IsNullOrEmpty(family)) return null;

            string key = family + "_d";
            string lore = R.VanillaLocalize(key);
            if (string.IsNullOrEmpty(lore) || string.Equals(lore, key, StringComparison.Ordinal))
                return null;

            string editionLore;
            if (PrayerEditionSemantics.TryResolveLoreOverride(craftId, lore, out editionLore))
                lore = editionLore;

            if (string.Equals(family, "b_village", StringComparison.Ordinal))
            {
                lore = TechnologyTooltipTextStyle.AccentLoreEntity(
                    lore,
                    "blessing_commerce",
                    R.VanillaLocalize("blessing_commerce"));
            }

            return lore;
        }

        internal static string ResolveForCrafts(List<object> crafts)
        {
            if (crafts == null || crafts.Count == 0) return null;

            string family = null;
            string firstCraftId = null;
            foreach (object craft in crafts)
            {
                string craftId = R.Id(craft);
                string currentFamily = PrayerFamilyFromCraftId(craftId);
                if (string.IsNullOrEmpty(currentFamily)) return null;

                if (family == null)
                {
                    family = currentFamily;
                    firstCraftId = craftId;
                }
                else if (!string.Equals(family, currentFamily, StringComparison.Ordinal))
                {
                    return null;
                }
            }

            return ResolveForCraftId(firstCraftId);
        }

        private static string PrayerFamilyFromCraftId(string craftId)
        {
            const string prefix = "pray:";
            if (string.IsNullOrEmpty(craftId) ||
                !craftId.StartsWith(prefix, StringComparison.Ordinal))
                return null;

            int last = craftId.LastIndexOf(':');
            if (last <= prefix.Length || last + 1 >= craftId.Length)
                return craftId.Substring(prefix.Length);

            return craftId.Substring(prefix.Length, last - prefix.Length);
        }
    }
}
