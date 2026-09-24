using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PrayerClarity
{
    internal static class PulpitPresentation
    {
        private static readonly Dictionary<string, Sprite> SpriteCache =
            new Dictionary<string, Sprite>(StringComparer.Ordinal);

        private static object _template;
        private static GameObject _root;
        private static object _resultLabel;
        private static object _effectLabel;
        private static object _effectIcon;
        private static object _noteLabel;
        private static MethodInfo _getSprite;

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

        private static Transform _windowTransform;
        private static Transform _selectorTable;
        private static Transform _pickLabelTransform;
        private static Vector3 _originalSelectorPosition;
        private static Vector3 _originalPickLabelPosition;
        private static bool _windowCaptured;

        internal static void Render(object template, object gui, string vanillaContext, PrayerForecast.Result forecast)
        {
            if (template == null || forecast == null) return;
            Ensure(template, gui);

            ConfigureContext(template);
            R.Set(template, "text", BuildContext(vanillaContext, forecast));

            ConfigureResult(_resultLabel);
            R.Set(_resultLabel, "text", BuildResult(forecast));

            SetEffect(
                forecast.SoulGratitudeFaithCap > 0 ? null : forecast.SpecialText,
                forecast.SpecialIconName);

            ConfigureNote(_noteLabel);
            string noteKey = forecast.UsesSoulGratitude
                ? "forecast.dependency_note_souls"
                : "forecast.dependency_note";
            R.Set(_noteLabel, "text", "[777777]" + Localization.F(noteKey) + "[-]");

            MovePrayerButton();
            MovePrayerSelector();
            if (_root != null) _root.SetActive(true);
            _presentationActive = true;
        }

        internal static void Hide(object template, object gui)
        {
            if (!_presentationActive) return;
            if (_root != null) _root.SetActive(false);
            RestoreTemplate(template);
            RestorePrayerButton();
            RestoreWindow();
            _presentationActive = false;
        }

        internal static void ApplyTuning()
        {
            if (!_presentationActive || _template == null) return;

            ConfigureContext(_template);
            ConfigureResult(_resultLabel);
            ConfigureNote(_noteLabel);

            bool showIcon = IsActive(_effectIcon);
            ConfigureEffectGeometry(showIcon);

            MovePrayerButton();
            MovePrayerSelector();
        }

        private static void Ensure(object template, object gui)
        {
            if (ReferenceEquals(_template, template) && _styleCaptured && _root != null)
            {
                CapturePrayerButton(gui);
                CaptureWindow(template);
                return;
            }

            if (_template != null && _presentationActive)
            {
                try { RestoreTemplate(_template); }
                catch { }
                try { RestorePrayerButton(); }
                catch { }
                try { RestoreWindow(); }
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
            _effectLabel = CreateLabel("Effect", template, depth + 1);
            _effectIcon = CreateSprite("Effect.Icon", template, depth + 2);
            _noteLabel = CreateLabel("DependencyNote", template, depth + 1);
            _root.SetActive(false);

            CapturePrayerButton(gui);
            CaptureWindow(template);
        }

        private static string BuildContext(string vanillaContext, PrayerForecast.Result forecast)
        {
            List<string> lines = new List<string>();

            string normalized = (vanillaContext ?? string.Empty).Replace("\r\n", "\n").TrimEnd('\n');
            if (!string.IsNullOrEmpty(normalized))
            {
                string[] contextLines = normalized.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in contextLines)
                    lines.Add("  " + line.Trim());
            }

            lines.Add("  " + Localization.F("forecast.graveyard_quality", forecast.GraveyardQuality));
            if (forecast.UsesSoulGratitude || forecast.SoulGratitudeFaithCap > 0)
                lines.Add("  " + Localization.F("forecast.soul_gratitude", forecast.SoulGratitude));
            return string.Join("\n", lines.ToArray());
        }

        private static string BuildResult(PrayerForecast.Result forecast)
        {
            List<string> lines = new List<string>();
            lines.Add(Localization.F("forecast.result_header"));
            lines.Add("  " + Localization.F("forecast.guaranteed") + ": " +
                      FormatResources(forecast.BaseFaith, forecast.BaseMoney, PrayerForecast.BonusHighlight.None));
            if (forecast.SoulGratitudeFaithCap > 0)
            {
                string conversion = forecast.SoulGratitudeConversion > 0
                    ? Localization.F(
                        "rebalanced.pulpit.souls_conversion",
                        forecast.SoulGratitudeConversion,
                        forecast.SoulGratitudeConversion)
                    : Localization.F("rebalanced.pulpit.souls_conversion_empty");
                lines.Add("  " + Localization.F("forecast.success_bonus", forecast.ChancePercent) + ": " + conversion);
            }
            else
            {
                lines.Add("  " + Localization.F("forecast.success_bonus", forecast.ChancePercent) + ": " +
                          FormatResources(forecast.BonusFaith, forecast.BonusMoney, forecast.Highlight));
            }
            return string.Join("\n", lines.ToArray());
        }

        private static string FormatResources(int faith, float money, PrayerForecast.BonusHighlight highlight)
        {
            List<string> parts = new List<string>();

            if (faith != 0)
            {
                string prefix = highlight == PrayerForecast.BonusHighlight.Faith ? "(up) " : string.Empty;
                parts.Add(prefix + "(faith) " + faith);
            }

            if (Math.Abs(money) >= 0.0001f)
            {
                string prefix = highlight == PrayerForecast.BonusHighlight.Money ? "(up) " : string.Empty;
                string value = highlight == PrayerForecast.BonusHighlight.Money
                    ? R.FormatSignedMoney(money)
                    : R.FormatMoney(money);
                parts.Add(prefix + value);
            }

            return parts.Count == 0 ? "—" : string.Join(", ", parts.ToArray());
        }

        private static void SetEffect(string text, string iconName)
        {
            bool hasText = !string.IsNullOrEmpty(text);
            GameObject labelGo = _effectLabel == null ? null : R.Get(_effectLabel, "gameObject") as GameObject;
            if (labelGo != null) labelGo.SetActive(hasText);

            Sprite sprite = hasText ? ResolveSprite(iconName) : null;
            GameObject iconGo = _effectIcon == null ? null : R.Get(_effectIcon, "gameObject") as GameObject;
            bool showIcon = sprite != null && iconGo != null;

            if (showIcon)
            {
                TrySet(_effectIcon, "sprite2D", sprite);
                iconGo.SetActive(true);
            }
            else if (iconGo != null)
            {
                iconGo.SetActive(false);
            }

            // Configure the fixed wrapping width before assigning text. ResizeHeight is
            // safe here because this is our own top-left label; unlike rejected 0.1.4
            // it cannot move the stock center-pivot l_total_values block on redraw.
            ConfigureEffectGeometry(showIcon);

            if (_effectLabel != null)
                R.Set(_effectLabel, "text", hasText ? Localization.F("forecast.effect_header") + ": " + text : string.Empty);
        }

        private static Sprite ResolveSprite(string iconName)
        {
            if (string.IsNullOrEmpty(iconName)) return null;

            Sprite cached;
            if (SpriteCache.TryGetValue(iconName, out cached)) return cached;

            try
            {
                if (_getSprite == null)
                {
                    Type type = R.GameType("EasySpritesCollection");
                    _getSprite = R.Method(type, "GetSprite", true,
                        new[] { typeof(string), typeof(bool), typeof(string) });
                }

                Sprite sprite = _getSprite == null
                    ? null
                    : _getSprite.Invoke(null, new object[] { iconName, false, string.Empty }) as Sprite;

                SpriteCache[iconName] = sprite;
                return sprite;
            }
            catch
            {
                SpriteCache[iconName] = null;
                return null;
            }
        }

        private static void ConfigureContext(object label)
        {
            ConfigureLabel(label,
                PulpitTuning.ContextX.Value,
                PulpitTuning.ContextY.Value,
                258,
                44,
                "Top",
                "Left",
                "ShrinkContent",
                PulpitTuning.ContextFontSize.Value,
                -2);
        }

        private static void ConfigureResult(object label)
        {
            ConfigureLabel(label,
                PulpitTuning.ResultX.Value,
                PulpitTuning.ResultY.Value,
                258,
                40,
                "Top",
                "Left",
                "ShrinkContent",
                PulpitTuning.ResultFontSize.Value,
                -1);
        }

        private static void ConfigureEffectGeometry(bool showIcon)
        {
            if (_effectLabel == null) return;

            float x = PulpitTuning.EffectX.Value;
            float y = PulpitTuning.EffectY.Value;
            int iconSize = PulpitTuning.EffectIconSize.Value;
            float iconSpace = showIcon ? iconSize + 4f : 0f;

            ConfigureLabel(_effectLabel,
                x + iconSpace,
                y,
                Math.Max(1, 258 - Mathf.RoundToInt(iconSpace)),
                20,
                "TopLeft",
                "Left",
                "ResizeHeight",
                PulpitTuning.EffectFontSize.Value,
                -1);

            ConfigureWidget(_effectIcon,
                x,
                y - 1f,
                iconSize,
                iconSize,
                "TopLeft");
        }

        private static void ConfigureNote(object label)
        {
            ConfigureLabel(label,
                PulpitTuning.NoteX.Value,
                PulpitTuning.NoteY.Value,
                266,
                20,
                "Top",
                "Center",
                "ShrinkContent",
                PulpitTuning.NoteFontSize.Value,
                -1);
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

        private static object CreateSprite(string suffix, object template, int depth)
        {
            Type type = R.AnyType("UI2DSprite");
            if (type == null) return null;

            GameObject templateGo = R.Get(template, "gameObject") as GameObject;
            GameObject go = new GameObject("PrayerClarity." + suffix);
            go.layer = templateGo == null ? _root.layer : templateGo.layer;
            go.transform.SetParent(_root.transform, false);
            go.transform.localScale = templateGo == null ? Vector3.one : templateGo.transform.localScale;

            object sprite = go.AddComponent(type);
            TrySet(sprite, "depth", depth);
            return sprite;
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

        private static void ConfigureWidget(object widget, float x, float y, int width, int height, string pivot)
        {
            if (widget == null) return;
            GameObject go = R.Get(widget, "gameObject") as GameObject;
            if (go == null) return;

            SetEnum(widget, "pivot", pivot);
            TrySet(widget, "width", width);
            TrySet(widget, "height", height);
            go.transform.localPosition = new Vector3(x, y, 0f);
        }

        private static void CapturePrayerButton(object gui)
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

        private static void MovePrayerButton()
        {
            if (!_buttonCaptured || _buttonObject == null) return;
            Vector3 p = _originalButtonPosition;
            _buttonObject.transform.localPosition = new Vector3(
                PulpitTuning.PrayerButtonX.Value,
                PulpitTuning.PrayerButtonY.Value,
                p.z);
        }

        private static void RestorePrayerButton()
        {
            if (!_buttonCaptured || _buttonObject == null) return;
            _buttonObject.transform.localPosition = _originalButtonPosition;
        }

        private static void CaptureWindow(object template)
        {
            GameObject templateGo = template == null ? null : R.Get(template, "gameObject") as GameObject;
            Transform container = templateGo == null ? null : templateGo.transform.parent;
            Transform window = container == null ? null : container.parent;
            if (window == null) return;

            if (ReferenceEquals(_windowTransform, window) && _windowCaptured) return;

            _windowTransform = window;
            _selectorTable = container.Find("table");
            _pickLabelTransform = container.Find("txt_pick_a_res");
            _originalSelectorPosition = _selectorTable == null ? Vector3.zero : _selectorTable.localPosition;
            _originalPickLabelPosition = _pickLabelTransform == null ? Vector3.zero : _pickLabelTransform.localPosition;
            _windowCaptured = true;
        }

        private static void MovePrayerSelector()
        {
            if (!_windowCaptured) return;

            float targetX = PulpitTuning.PrayerSelectorX.Value;
            float targetY = PulpitTuning.PrayerSelectorY.Value;
            float dx = targetX - _originalSelectorPosition.x;
            float dy = targetY - _originalSelectorPosition.y;

            if (_selectorTable != null)
            {
                Vector3 p = _originalSelectorPosition;
                _selectorTable.localPosition = new Vector3(targetX, targetY, p.z);
            }

            if (_pickLabelTransform != null)
            {
                Vector3 p = _originalPickLabelPosition;
                _pickLabelTransform.localPosition = new Vector3(p.x + dx, p.y + dy, p.z);
            }
        }

        private static void RestoreWindow()
        {
            if (!_windowCaptured) return;
            if (_selectorTable != null) _selectorTable.localPosition = _originalSelectorPosition;
            if (_pickLabelTransform != null) _pickLabelTransform.localPosition = _originalPickLabelPosition;
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

        private static bool IsActive(object widget)
        {
            GameObject go = widget == null ? null : R.Get(widget, "gameObject") as GameObject;
            return go != null && go.activeSelf;
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
