using System;
using BepInEx.Configuration;

namespace PrayerClarity
{
    internal static class PulpitTuning
    {
        // v3 deliberately drops experimental window resizing. Runtime 0.1.7 proved
        // that changing the child sprites stretches artwork without enlarging the
        // actual pulpit window. Keep only the controls that manipulate stable local
        // positions/sizes inside the stock window.
        private const string Section = "Prototype pulpit layout tuning v3";

        internal static ConfigEntry<float> ContextX { get; private set; }
        internal static ConfigEntry<float> ContextY { get; private set; }
        internal static ConfigEntry<int> ContextFontSize { get; private set; }
        internal static ConfigEntry<float> ResultX { get; private set; }
        internal static ConfigEntry<float> ResultY { get; private set; }
        internal static ConfigEntry<int> ResultFontSize { get; private set; }
        internal static ConfigEntry<float> EffectX { get; private set; }
        internal static ConfigEntry<float> EffectY { get; private set; }
        internal static ConfigEntry<int> EffectFontSize { get; private set; }
        internal static ConfigEntry<int> EffectIconSize { get; private set; }
        internal static ConfigEntry<float> NoteX { get; private set; }
        internal static ConfigEntry<float> NoteY { get; private set; }
        internal static ConfigEntry<int> NoteFontSize { get; private set; }
        internal static ConfigEntry<float> PrayerSelectorX { get; private set; }
        internal static ConfigEntry<float> PrayerSelectorY { get; private set; }
        internal static ConfigEntry<float> PrayerButtonX { get; private set; }
        internal static ConfigEntry<float> PrayerButtonY { get; private set; }

        internal static void Bind(ConfigFile config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            // Accepted starting geometry from the user's 0.1.7 live calibration at
            // 2560x1440. These remain temporary tuning controls until a second
            // resolution confirms portability.
            ContextX = BindFloat(config, "01 Context X", 8f, -220f, 220f,
                "Horizontal position of church / sermon / graveyard context.");
            ContextY = BindFloat(config, "02 Context Y", 72f, -180f, 160f,
                "Vertical top position of church / sermon / graveyard context.");
            ContextFontSize = BindInt(config, "03 Context font size", 12, 8, 18,
                "Font size for church / sermon / graveyard context.");

            ResultX = BindFloat(config, "04 Result X", 8f, -220f, 220f,
                "Horizontal position of the result block.");
            ResultY = BindFloat(config, "05 Result Y", 20f, -200f, 100f,
                "Vertical top position of the result block.");
            ResultFontSize = BindInt(config, "06 Result font size", 13, 8, 18,
                "Font size for guaranteed and success-bonus rows.");

            EffectX = BindFloat(config, "07 Effect X", -122f, -220f, 220f,
                "Horizontal position of the special-effect row and its icon.");
            EffectY = BindFloat(config, "08 Effect Y", -35f, -220f, 100f,
                "Vertical top position of the special-effect row.");
            EffectFontSize = BindInt(config, "09 Effect font size", 10, 8, 18,
                "Font size for the special-effect row.");
            EffectIconSize = BindInt(config, "10 Effect icon size", 10, 8, 28,
                "Size of a verified native special-effect icon when one is available.");

            NoteX = BindFloat(config, "11 Note X", -6f, -220f, 220f,
                "Horizontal position of the dependency note under the prayer button.");
            NoteY = BindFloat(config, "12 Note Y", -87f, -260f, 20f,
                "Vertical top position of the dependency note.");
            NoteFontSize = BindInt(config, "13 Note font size", 9, 6, 14,
                "Font size for the dependency note.");

            PrayerSelectorX = BindFloat(config, "14 Prayer selector X", 90f, -180f, 180f,
                "Horizontal position of the selected-prayer slot. The Choose sermon label follows it.");
            PrayerSelectorY = BindFloat(config, "15 Prayer selector Y", 55f, -180f, 100f,
                "Vertical position of the selected-prayer slot. The Choose sermon label follows it.");

            PrayerButtonX = BindFloat(config, "16 Prayer button X", 0f, -180f, 180f,
                "Horizontal position of the stock Pray / Try prayer button while PrayerClarity forecast is active.");
            PrayerButtonY = BindFloat(config, "17 Prayer button Y", -120f, -240f, 40f,
                "Vertical position of the stock Pray / Try prayer button while PrayerClarity forecast is active.");

            Watch(ContextX);
            Watch(ContextY);
            Watch(ContextFontSize);
            Watch(ResultX);
            Watch(ResultY);
            Watch(ResultFontSize);
            Watch(EffectX);
            Watch(EffectY);
            Watch(EffectFontSize);
            Watch(EffectIconSize);
            Watch(NoteX);
            Watch(NoteY);
            Watch(NoteFontSize);
            Watch(PrayerSelectorX);
            Watch(PrayerSelectorY);
            Watch(PrayerButtonX);
            Watch(PrayerButtonY);
        }

        private static ConfigEntry<float> BindFloat(ConfigFile config, string key, float value, float min, float max, string description)
        {
            return config.Bind(Section, key, value,
                new ConfigDescription(description, new AcceptableValueRange<float>(min, max)));
        }

        private static ConfigEntry<int> BindInt(ConfigFile config, string key, int value, int min, int max, string description)
        {
            return config.Bind(Section, key, value,
                new ConfigDescription(description, new AcceptableValueRange<int>(min, max)));
        }

        private static void Watch<T>(ConfigEntry<T> entry)
        {
            entry.SettingChanged += OnSettingChanged;
        }

        private static void OnSettingChanged(object sender, EventArgs e)
        {
            PulpitPresentation.ApplyTuning();
        }
    }
}
