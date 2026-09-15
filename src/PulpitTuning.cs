using System;
using BepInEx.Configuration;

namespace PrayerClarity
{
    internal static class PulpitTuning
    {
        // v4 keys are retained so an existing calibration remains readable, but the
        // defaults now reflect the accepted 0.1.10/0.1.11 2560x1440 working composition.
        private const string Section = "Prototype pulpit layout tuning v4";

        internal static ConfigEntry<float> WindowExtraWidth { get; private set; }
        internal static ConfigEntry<float> WindowExtraHeight { get; private set; }
        internal static ConfigEntry<float> ContextX { get; private set; }
        internal static ConfigEntry<float> ContextY { get; private set; }
        internal static ConfigEntry<int> ContextFontSize { get; private set; }
        internal static ConfigEntry<float> ResultHeaderX { get; private set; }
        internal static ConfigEntry<float> ResultHeaderY { get; private set; }
        internal static ConfigEntry<int> ResultHeaderFontSize { get; private set; }
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

            WindowExtraWidth = BindFloat(config, "01 Window extra width", 10f, 0f, 360f,
                "Extra width applied to the real Pray GUI window and sliced frame.");
            WindowExtraHeight = BindFloat(config, "02 Window extra height", 100f, 0f, 360f,
                "Extra height applied to the real Pray GUI window and sliced frame.");

            ContextX = BindFloat(config, "03 Context X", 60f, -260f, 260f,
                "Horizontal position of church / sermon / graveyard context.");
            ContextY = BindFloat(config, "04 Context Y", 137f, -240f, 220f,
                "Vertical top position of church / sermon / graveyard context.");
            ContextFontSize = BindInt(config, "05 Context font size", 15, 8, 20,
                "Font size for church / sermon / graveyard context.");

            ResultHeaderX = BindFloat(config, "06 Result header X", 6f, -260f, 260f,
                "Horizontal position of the Result heading.");
            ResultHeaderY = BindFloat(config, "07 Result header Y", 5f, -240f, 180f,
                "Vertical position of the Result heading.");
            ResultHeaderFontSize = BindInt(config, "08 Result header font size", 16, 8, 20,
                "Font size of the Result heading.");

            ResultX = BindFloat(config, "09 Result rows X", -5f, -260f, 260f,
                "Horizontal position of Guaranteed / Success bonus rows.");
            ResultY = BindFloat(config, "10 Result rows Y", -20f, -260f, 160f,
                "Vertical top position of Guaranteed / Success bonus rows.");
            ResultFontSize = BindInt(config, "11 Result rows font size", 14, 8, 20,
                "Font size for Guaranteed / Success bonus rows.");

            EffectX = BindFloat(config, "12 Effect X", -128f, -300f, 300f,
                "Horizontal position of the special-effect row and its icon.");
            EffectY = BindFloat(config, "13 Effect Y", -70f, -300f, 160f,
                "Vertical top position of the special-effect row.");
            EffectFontSize = BindInt(config, "14 Effect font size", 12, 8, 20,
                "Font size for the special-effect row.");
            EffectIconSize = BindInt(config, "15 Effect icon size", 10, 8, 32,
                "Size of verified native effect/resource icons.");

            NoteX = BindFloat(config, "16 Note X", -6f, -300f, 300f,
                "Horizontal position of the dependency note.");
            NoteY = BindFloat(config, "17 Note Y", -137f, -340f, 80f,
                "Vertical top position of the dependency note.");
            NoteFontSize = BindInt(config, "18 Note font size", 9, 6, 14,
                "Font size for the dependency note.");

            PrayerSelectorX = BindFloat(config, "19 Prayer selector X", 0f, -240f, 240f,
                "Horizontal position of the selected-prayer slot. The Choose sermon label follows it.");
            PrayerSelectorY = BindFloat(config, "20 Prayer selector Y", 40f, -240f, 180f,
                "Vertical position of the selected-prayer slot. The Choose sermon label follows it.");

            PrayerButtonX = BindFloat(config, "21 Prayer button X", 0f, -240f, 240f,
                "Experimental horizontal position of the stock Pray / Try prayer button.");
            PrayerButtonY = BindFloat(config, "22 Prayer button Y", -120f, -320f, 100f,
                "Experimental vertical position of the stock Pray / Try prayer button.");

            Watch(WindowExtraWidth);
            Watch(WindowExtraHeight);
            Watch(ContextX);
            Watch(ContextY);
            Watch(ContextFontSize);
            Watch(ResultHeaderX);
            Watch(ResultHeaderY);
            Watch(ResultHeaderFontSize);
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
            PulpitLayoutV4.ApplyTuning();
            PulpitPolish.ApplyTuning();
            PulpitEffectIconPolicy.ApplyTuning();
        }
    }
}
