using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PrayerClarity
{
    internal static class PulpitPresentation
    {
        private static object _template;
        private static GameObject _root;
        private static object _resultLabel;
        private static object _noteLabel;

        private static object _originalAlignment;
        private static object _originalOverflow;
        private static object _originalPivot;
        private static int _originalWidth;
        private static int _originalHeight;
        private static int _originalFontSize;
        private static object _originalSpacingY;
        private static Vector3 _originalPosition;
        private static bool _styleCaptured;
        private static bool _presentationActive;

        private static GameObject _buttonObject;
        private static Vector3 _originalButtonPosition;
        private static bool _buttonCaptured;

        internal static void Render(object template, object gui, string vanillaContext, PrayerForecast.Result forecast)
        {
            if (template == null || forecast == null) return;
            Ensure(template, gui);

            ConfigureContext(template);
            R.Set(template, "text", BuildContext(vanillaContext, forecast));

            ConfigureResult(_resultLabel);
            R.Set(_resultLabel, "text", BuildResult(forecast));

            ConfigureNote(_noteLabel);
            string noteKey = forecast.UsesSoulGratitude
                ? "forecast.dependency_note_souls"
                : "forecast.dependency_note";
            R.Set(_noteLabel, "text", "[777777]" + Localization.F(noteKey) + "[-]");

            MoveCraftButton(gui);
            if (_root != null) _root.SetActive(true);
            _presentationActive = true;
        }

        internal static void Hide(object template, object gui)
        {
            if (!_presentationActive) return;
            if (_root != null) _root.SetActive(false);
            RestoreTemplate(template);
            RestoreCraftButton(gui);
            _presentationActive = false;
        }

        private static void Ensure(object template, object gui)
        {
            if (ReferenceEquals(_template, template) && _styleCaptured && _root != null)
            {
                CaptureCraftButton(gui);
                return;
            }

            if (_template != null && _presentationActive)
            {
                try { RestoreTemplate(_template); }
                catch { }
                try { RestoreCraftButton(gui); }
                catch { }
            }

            if (_root != null) UnityEngine.Object.Destroy(_root);

            _template = template;
            GameObject templateGo = R.Get(template, "gameObject") as GameObject;
            if (templateGo == null) throw new InvalidOperationException("Pulpit label GameObject is unavailable.");
            Transform parent = templateGo.transform.parent;
            if (parent == null) throw new InvalidOperationException("Pulpit label parent is unavailable.");

            _originalAlignment = R.Get(template, "alignment");
            _originalOverflow = R.Get(template, "overflowMethod");
            _originalPivot = R.Get(template, "pivot");
            _originalWidth = Math.Max(1, R.Int(R.Get(template, "width")));
            _originalHeight = Math.Max(1, R.Int(R.Get(template, "height")));
            _originalFontSize = Math.Max(1, R.Int(R.Get(template, "fontSize")));
            _originalSpacingY = R.Get(template, "spacingY");
            _originalPosition = templateGo.transform.localPosition;
            _styleCaptured = true;
            _presentationActive = false;

            _root = new GameObject("PrayerClarity.PulpitForecast");
            _root.layer = templateGo.layer;
            _root.transform.SetParent(parent, false);
            _root.transform.localPosition = Vector3.zero;
            _root.transform.localRotation = Quaternion.identity;
            _root.transform.localScale = Vector3.one;

            int depth = R.Int(R.Get(template, "depth"));
            _resultLabel = CreateLabel("Result", template, depth + 1);
            _noteLabel = CreateLabel("DependencyNote", template, depth + 1);
            _root.SetActive(false);

            CaptureCraftButton(gui);
        }

        private static string BuildContext(string vanillaContext, PrayerForecast.Result forecast)
        {
            List<string> lines = new List<string>();
            lines.Add(Localization.F("forecast.context_header"));

            string normalized = (vanillaContext ?? string.Empty).Replace("\r\n", "\n").TrimEnd('\n');
            if (!string.IsNullOrEmpty(normalized))
            {
                string[] contextLines = normalized.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in contextLines)
                    lines.Add("  " + line.Trim());
            }

            lines.Add("  " + Localization.F("forecast.graveyard_quality", forecast.GraveyardQuality));
            return string.Join("\n", lines.ToArray());
        }

        private static string BuildResult(PrayerForecast.Result forecast)
        {
            List<string> lines = new List<string>();
            lines.Add(Localization.F("forecast.result_header"));
            lines.Add("  " + Localization.F("forecast.guaranteed") + ": " +
                      FormatResources(forecast.BaseFaith, forecast.BaseMoney));
            lines.Add("  " + Localization.F("forecast.success_bonus", forecast.ChancePercent) + ": " +
                      FormatResources(forecast.BonusFaith, forecast.BonusMoney));

            if (!string.IsNullOrEmpty(forecast.SpecialText))
                lines.Add("  " + Localization.F("forecast.effect_header") + ": " + forecast.SpecialText);

            return string.Join("\n", lines.ToArray());
        }

        private static string FormatResources(int faith, float money)
        {
            List<string> parts = new List<string>();
            if (faith != 0) parts.Add("(faith) " + faith);
            if (Math.Abs(money) >= 0.0001f) parts.Add(R.FormatMoney(money));
            return parts.Count == 0 ? "—" : string.Join(", ", parts.ToArray());
        }

        private static void ConfigureContext(object label)
        {
            // Probe 0.1.0 proved the vanilla label is centered at (-3,49), 242x68.
            // Keep its original top edge (83) but switch to a fixed top pivot so repeated
            // redraws can never accumulate the ResizeHeight/center-pivot Y drift seen in 0.1.4.
            ConfigureLabel(label, -3f, 83f, 258, 52, "Top", "Left", "ShrinkContent", 14, -2);
        }

        private static void ConfigureResult(object label)
        {
            // The stock prayer item cell occupies the middle of the window. Results live
            // below it instead of growing a single label through that cell.
            ConfigureLabel(label, -3f, -38f, 258, 42, "Top", "Left", "ShrinkContent", 11, -1);
        }

        private static void ConfigureNote(object label)
        {
            // Secondary dependency reminder sits in the measured gap below the craft button
            // and above the stock controller tips. It deliberately uses a smaller font.
            ConfigureLabel(label, -3f, -102f, 266, 10, "Top", "Center", "ShrinkContent", 9, -1);
        }

        private static object CreateLabel(string suffix, object template, int depth)
        {
            GameObject templateGo = R.Get(template, "gameObject") as GameObject;
            if (templateGo == null) throw new InvalidOperationException("Pulpit label GameObject is unavailable.");

            GameObject go = new GameObject("PrayerClarity." + suffix);
            go.layer = templateGo.layer;
            go.transform.SetParent(_root.transform, false);
            go.transform.localScale = templateGo.transform.localScale;

            object label = go.AddComponent(template.GetType());
            CopyStyle(template, label);
            TrySet(label, "depth", depth);
            return label;
        }

        private static void CopyStyle(object source, object target)
        {
            object bitmapFont = R.Get(source, "bitmapFont");
            if (bitmapFont != null)
                TrySet(target, "bitmapFont", bitmapFont);
            else
                TrySet(target, "trueTypeFont", R.Get(source, "trueTypeFont"));

            string[] properties =
            {
                "fontStyle", "color", "effectStyle", "effectColor", "effectDistance",
                "supportEncoding", "symbolStyle", "spacingX", "useFloatSpacing",
                "floatSpacingX", "floatSpacingY", "applyGradient", "gradientTop", "gradientBottom"
            };

            foreach (string property in properties)
            {
                try
                {
                    object value = R.Get(source, property);
                    if (value != null) R.Set(target, property, value);
                }
                catch { }
            }
        }

        private static void ConfigureLabel(object label, float x, float y, int width, int height,
            string pivot, string alignment, string overflow, int fontSize, int spacingY)
        {
            if (label == null) return;
            GameObject go = R.Get(label, "gameObject") as GameObject;
            if (go == null) return;

            SetEnum(label, "pivot", pivot);
            TrySet(label, "width", width);
            TrySet(label, "height", height);
            TrySet(label, "fontSize", fontSize);
            TrySet(label, "spacingY", spacingY);
            SetEnum(label, "alignment", alignment);
            SetEnum(label, "overflowMethod", overflow);
            go.transform.localPosition = new Vector3(x, y, 0f);
        }

        private static void CaptureCraftButton(object gui)
        {
            if (gui == null) return;
            object button = R.Get(gui, "l_button");
            GameObject go = button == null ? null : R.Get(button, "gameObject") as GameObject;
            if (go == null) return;

            if (ReferenceEquals(_buttonObject, go) && _buttonCaptured) return;
            _buttonObject = go;
            _originalButtonPosition = go.transform.localPosition;
            _buttonCaptured = true;
        }

        private static void MoveCraftButton(object gui)
        {
            CaptureCraftButton(gui);
            if (!_buttonCaptured || _buttonObject == null) return;
            Vector3 p = _originalButtonPosition;
            _buttonObject.transform.localPosition = new Vector3(p.x, -100f, p.z);
        }

        private static void RestoreCraftButton(object gui)
        {
            CaptureCraftButton(gui);
            if (!_buttonCaptured || _buttonObject == null) return;
            _buttonObject.transform.localPosition = _originalButtonPosition;
        }

        private static void RestoreTemplate(object template)
        {
            if (!_styleCaptured || template == null || !ReferenceEquals(_template, template)) return;
            GameObject go = R.Get(template, "gameObject") as GameObject;

            if (_originalAlignment != null) TrySet(template, "alignment", _originalAlignment);
            if (_originalOverflow != null) TrySet(template, "overflowMethod", _originalOverflow);
            if (_originalPivot != null) TrySet(template, "pivot", _originalPivot);
            TrySet(template, "width", _originalWidth);
            TrySet(template, "height", _originalHeight);
            TrySet(template, "fontSize", _originalFontSize);
            if (_originalSpacingY != null) TrySet(template, "spacingY", _originalSpacingY);
            if (go != null) go.transform.localPosition = _originalPosition;
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
