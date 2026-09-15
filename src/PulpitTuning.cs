namespace PrayerClarity
{
    // Accepted pulpit geometry from the 0.1.x runtime calibration. These values are
    // intentionally fixed for the public Clarity build: layout sliders were a
    // development instrument, not a player-facing feature.
    internal static class PulpitTuning
    {
        internal struct FixedValue<T>
        {
            internal readonly T Value;

            internal FixedValue(T value)
            {
                Value = value;
            }
        }

        internal static readonly FixedValue<float> WindowExtraWidth = new FixedValue<float>(10f);
        internal static readonly FixedValue<float> WindowExtraHeight = new FixedValue<float>(100f);

        internal static readonly FixedValue<float> ContextX = new FixedValue<float>(60f);
        internal static readonly FixedValue<float> ContextY = new FixedValue<float>(137f);
        internal static readonly FixedValue<int> ContextFontSize = new FixedValue<int>(15);

        internal static readonly FixedValue<float> ResultHeaderX = new FixedValue<float>(6f);
        internal static readonly FixedValue<float> ResultHeaderY = new FixedValue<float>(7f);
        internal static readonly FixedValue<int> ResultHeaderFontSize = new FixedValue<int>(15);

        internal static readonly FixedValue<float> ResultX = new FixedValue<float>(-5f);
        internal static readonly FixedValue<float> ResultY = new FixedValue<float>(-8f);
        internal static readonly FixedValue<int> ResultFontSize = new FixedValue<int>(14);

        internal static readonly FixedValue<float> EffectX = new FixedValue<float>(-128f);
        internal static readonly FixedValue<float> EffectY = new FixedValue<float>(-70f);
        internal static readonly FixedValue<int> EffectFontSize = new FixedValue<int>(12);
        internal static readonly FixedValue<int> EffectIconSize = new FixedValue<int>(10);

        internal static readonly FixedValue<float> NoteX = new FixedValue<float>(-6f);
        internal static readonly FixedValue<float> NoteY = new FixedValue<float>(-137f);
        internal static readonly FixedValue<int> NoteFontSize = new FixedValue<int>(10);

        internal static readonly FixedValue<float> PrayerSelectorX = new FixedValue<float>(0f);
        internal static readonly FixedValue<float> PrayerSelectorY = new FixedValue<float>(40f);
        internal static readonly FixedValue<float> PrayerButtonX = new FixedValue<float>(0f);
        internal static readonly FixedValue<float> PrayerButtonY = new FixedValue<float>(-120f);
    }
}
