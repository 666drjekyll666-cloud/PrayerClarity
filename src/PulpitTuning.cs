using System;
using BepInEx.Configuration;

namespace PrayerClarity
{
    internal static class PulpitTuning
    {
        private const string Section = "Prototype pulpit layout tuning";

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
        internal static ConfigEntry<float> CraftButtonY { get; private set; }

        internal static void Bind(ConfigFile config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            WindowExtraHeight = BindFloat(config, "01 Window extra height", 40f, 0f, 100f,
                "Temporary calibration control. Extends the stock pulpit frame downward in local NGUI units.");

            ContextX = BindFloat(config, "02 Context X", -3f, -50f, 50f,
                "Horizontal position of church / sermon / graveyard context.");
            ContextY = BindFloat(config, "03 Context Y", 78f, 30f, 110f,
                "Vertical top position of church / sermon / graveyard context.");
            ContextFontSize = BindInt(config, "04 Context font size", 14, 8, 18,
                "Font size for church / sermon / graveyard context.");

            ResultX = BindFloat(config, "05 Result X", -3f, -50f, 50f,
                "Horizontal position of the result block.");
            ResultY = BindFloat(config, "06 Result Y", -37f, -90f, 20f,
                "Vertical top position of the result block.");
            ResultFontSize = BindInt(config, "07 Result font size", 11, 8, 18,
                "Font size for guaranteed and success-bonus rows.");

            EffectX = BindFloat(config, "08 Effect X", -3f, -50f, 50f,
                "Horizontal position of the special-effect row.");
            EffectY = BindFloat(config, "09 Effect Y", -75f, -130f, 20f,
                "Vertical top position of the special-effect row.");
            EffectFontSize = BindInt(config, "10 Effect font size", 11, 8, 18,
                "Font size for the special-effect row.");
            EffectIconSize = BindInt(config, "11 Effect icon size", 14, 8, 24,
                "Size of a verified native special-effect icon when one is available.");

            NoteX = BindFloat(config, "12 Note X", -3f, -50f, 50f,
                "Horizontal position of the dependency note under the action button.");
            NoteY = BindFloat(config, "13 Note Y", -126f, -180f, -60f,
                "Vertical top position of the dependency note.");
            NoteFontSize = BindInt(config, "14 Note font size", 9, 6, 14,
                "Font size for the dependency note.");

            CraftButtonY = BindFloat(config, "15 Craft button Y", -120f, -170f, -70f,
                "Vertical position of the stock Create button while PrayerClarity forecast is active.");

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
            Watch(CraftButtonY);
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
