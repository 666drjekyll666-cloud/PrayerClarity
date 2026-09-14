using System;
using BepInEx.Configuration;

namespace PrayerClarity
{
    internal static class PulpitTuning
    {
        // v2 deliberately uses a new section so rejected 0.1.6 calibration values do not
        // silently carry into the revised frame-resize implementation.
        private const string Section = "Prototype pulpit layout tuning v2";

        internal static ConfigEntry<float> WindowExtraWidth { get; private set; }
        internal static ConfigEntry<float> WindowExtraHeight { get; private set; }
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

            WindowExtraWidth = BindFloat(config, "01 Window extra width", 0f, 0f, 220f,
                "Temporary calibration control. Extends the stock pulpit frame sideways in local NGUI units.");
            WindowExtraHeight = BindFloat(config, "02 Window extra height", 40f, 0f, 160f,
                "Temporary calibration control. Extends the stock pulpit frame downward in local NGUI units.");

            ContextX = BindFloat(config, "03 Context X", -3f, -220f, 220f,
                "Horizontal position of church / sermon / graveyard context.");
            ContextY = BindFloat(config, "04 Context Y", 78f, -180f, 160f,
                "Vertical top position of church / sermon / graveyard context.");
            ContextFontSize = BindInt(config, "05 Context font size", 14, 8, 18,
                "Font size for church / sermon / graveyard context.");

            ResultX = BindFloat(config, "06 Result X", -3f, -220f, 220f,
                "Horizontal position of the result block.");
            ResultY = BindFloat(config, "07 Result Y", -37f, -200f, 100f,
                "Vertical top position of the result block.");
            ResultFontSize = BindInt(config, "08 Result font size", 11, 8, 18,
                "Font size for guaranteed and success-bonus rows.");

            EffectX = BindFloat(config, "09 Effect X", -3f, -220f, 220f,
                "Horizontal position of the special-effect row and its icon.");
            EffectY = BindFloat(config, "10 Effect Y", -75f, -220f, 100f,
                "Vertical top position of the special-effect row.");
            EffectFontSize = BindInt(config, "11 Effect font size", 11, 8, 18,
                "Font size for the special-effect row.");
            EffectIconSize = BindInt(config, "12 Effect icon size", 14, 8, 28,
                "Size of a verified native special-effect icon when one is available.");

            NoteX = BindFloat(config, "13 Note X", -3f, -220f, 220f,
                "Horizontal position of the dependency note under the prayer button.");
            NoteY = BindFloat(config, "14 Note Y", -126f, -260f, 20f,
                "Vertical top position of the dependency note.");
            NoteFontSize = BindInt(config, "15 Note font size", 9, 6, 14,
                "Font size for the dependency note.");

            PrayerSelectorX = BindFloat(config, "16 Prayer selector X", 0f, -180f, 180f,
                "Horizontal position of the selected-prayer slot. The Choose sermon label follows it.");
            PrayerSelectorY = BindFloat(config, "17 Prayer selector Y", -15f, -180f, 100f,
                "Vertical position of the selected-prayer slot. The Choose sermon label follows it.");

            PrayerButtonX = BindFloat(config, "18 Prayer button X", 0f, -180f, 180f,
                "Horizontal position of the stock Pray / Try prayer button while PrayerClarity forecast is active.");
            PrayerButtonY = BindFloat(config, "19 Prayer button Y", -120f, -240f, 40f,
                "Vertical position of the stock Pray / Try prayer button while PrayerClarity forecast is active.");

            Watch(WindowExtraWidth);
            Watch(WindowExtraHeight);
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
