using System;
using System.Collections.Generic;
using System.Reflection;

namespace PrayerClarity
{
    internal static class PulpitPresentation
    {
        private static object _template;
        private static object _originalAlignment;
        private static object _originalOverflow;
        private static int _originalHeight;
        private static bool _styleCaptured;
        private static bool _presentationActive;

        internal static void Render(object template, string vanillaContext, PrayerForecast.Result forecast)
        {
            if (template == null || forecast == null) return;
            Ensure(template);

            // Reuse the vanilla label itself. This preserves its panel, layer, bitmap font,
            // symbol table and anchoring instead of creating a parallel UI hierarchy.
            TrySet(template, "height", _originalHeight);
            SetEnum(template, "alignment", "Left");
            SetEnum(template, "overflowMethod", "ResizeHeight");

            List<string> lines = new List<string>();
            lines.Add(Localization.F("forecast.context_header"));
            if (!string.IsNullOrEmpty(vanillaContext))
            {
                string normalized = vanillaContext.Replace("\r\n", "\n").TrimEnd('\n');
                if (!string.IsNullOrEmpty(normalized)) lines.Add(normalized);
            }
            lines.Add(Localization.F("forecast.graveyard_quality", forecast.GraveyardQuality));

            lines.Add(string.Empty);
            lines.Add(Localization.F("forecast.result_header"));
            lines.Add("(faith) " + Localization.F("forecast.guaranteed") + ": " + forecast.BaseFaith);
            lines.Add(Localization.F("forecast.guaranteed") + ": " + R.FormatMoney(forecast.BaseMoney));

            if (forecast.BonusFaith != 0)
                lines.Add("(faith) " + Localization.F("forecast.success_bonus", forecast.ChancePercent) + ": +" + forecast.BonusFaith);

            if (Math.Abs(forecast.BonusMoney) >= 0.0001f)
                lines.Add(Localization.F("forecast.success_bonus", forecast.ChancePercent) + ": +" + R.FormatMoney(forecast.BonusMoney));

            if (!string.IsNullOrEmpty(forecast.SpecialText))
            {
                lines.Add(string.Empty);
                lines.Add(Localization.F("forecast.effect_header"));
                lines.Add(forecast.SpecialText);
            }

            lines.Add(string.Empty);
            string noteKey = forecast.UsesSoulGratitude
                ? "forecast.dependency_note_souls"
                : "forecast.dependency_note";
            lines.Add("[777777]* " + Localization.F(noteKey) + "[-]");

            R.Set(template, "text", string.Join("\n", lines.ToArray()));
            _presentationActive = true;
        }

        internal static void Hide(object template)
        {
            if (!_presentationActive || template == null || !ReferenceEquals(_template, template)) return;
            Restore(template);
        }

        private static void Ensure(object template)
        {
            if (ReferenceEquals(_template, template) && _styleCaptured) return;

            if (_template != null && _presentationActive)
            {
                try { Restore(_template); }
                catch { }
            }

            _template = template;
            _originalAlignment = R.Get(template, "alignment");
            _originalOverflow = R.Get(template, "overflowMethod");
            _originalHeight = Math.Max(1, R.Int(R.Get(template, "height")));
            _styleCaptured = true;
            _presentationActive = false;
        }

        private static void Restore(object template)
        {
            if (_originalAlignment != null) TrySet(template, "alignment", _originalAlignment);
            if (_originalOverflow != null) TrySet(template, "overflowMethod", _originalOverflow);
            TrySet(template, "height", _originalHeight);
            _presentationActive = false;
        }

        private static void TrySet(object obj, string name, object value)
        {
            if (obj == null || value == null) return;
            try { R.Set(obj, name, value); }
            catch { }
        }

        private static void SetEnum(object obj, string propertyName, string value)
        {
            if (obj == null) return;
            try
            {
                PropertyInfo property = obj.GetType().GetProperty(propertyName, R.Inst);
                if (property == null || !property.CanWrite || !property.PropertyType.IsEnum) return;
                property.SetValue(obj, Enum.Parse(property.PropertyType, value), null);
            }
            catch { }
        }
    }
}
