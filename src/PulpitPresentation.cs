using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PrayerClarity
{
    internal static class PulpitPresentation
    {
        private sealed class StretchPart
        {
            internal Transform Transform;
            internal object Widget;
            internal int OriginalHeight;
            internal Vector3 OriginalPosition;
            internal float DownFactor;
        }

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
        private static StretchPart _windowBack;
        private static StretchPart _decoreBack;
        private static StretchPart _decore;
        private static StretchPart _inactiveBack;
        private static Transform _tipsTransform;
        private static Vector3 _originalTipsPosition;
        private static bool _windowCaptured;

        internal static void Render(object template, object gui, string vanillaContext, PrayerForecast.Result forecast)
        {
            if (template == null || forecast == null) return;
            Ensure(template, gui);

            ConfigureContext(template);
            R.Set(template, "text", BuildContext(vanillaContext, forecast));

            ConfigureResult(_resultLabel);
            R.Set(_resultLabel, "text", BuildResult(forecast));

            SetEffect(forecast.SpecialText, forecast.SpecialIconName);

            ConfigureNote(_noteLabel);
            string noteKey = forecast.UsesSoulGratitude
                ? "forecast.dependency_note_souls"
                : "forecast.dependency_note";
            R.Set(_noteLabel, "text", "[777777]" + Localization.F(noteKey) + "[-]");

            MoveCraftButton();
            ApplyWindowExtension();
            if (_root != null) _root.SetActive(true);
            _presentationActive = true;
        }

        internal static void Hide(object template, object gui)
        {
            if (!_presentationActive) return;
            if (_root != null) _root.SetActive(false);
            RestoreTemplate(template);
            RestoreCraftButton();
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

            MoveCraftButton();
            ApplyWindowExtension();
        }

        private static void Ensure(object template, object gui)
        {
            if (ReferenceEquals(_template, template) && _styleCaptured && _root != null)
            {
                CaptureCraftButton(gui);
                CaptureWindow(template);
                return;
            }

            if (_template != null && _presentationActive)
            {
                try { RestoreTemplate(_template); }
                catch { }
                try { RestoreCraftButton(); }
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

            CaptureCraftButton(gui);
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
            return string.Join("\n", lines.ToArray());
        }

        private static string BuildResult(PrayerForecast.Result forecast)
        {
            List<string> lines = new List<string>();
            lines.Add(Localization.F("forecast.result_header"));
            lines.Add("  " + Localization.F("forecast.guaranteed") + ": " +
                      FormatResources(forecast.BaseFaith, forecast.BaseMoney));

            bool positiveBonus = forecast.BonusFaith > 0 || forecast.BonusMoney > 0.0001f;
            string bonusMarker = positiveBonus ? "(up) " : string.Empty;
            lines.Add("  " + Localization.F("forecast.success_bonus", forecast.ChancePercent) + ": " +
                      bonusMarker + FormatResources(forecast.BonusFaith, forecast.BonusMoney));

            return string.Join("\n", lines.ToArray());
        }

        private static string FormatResources(int faith, float money)
        {
            List<string> parts = new List<string>();
            if (faith != 0) parts.Add("(faith) " + faith);
            if (Math.Abs(money) >= 0.0001f) parts.Add(R.FormatMoney(money));
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

            if (_effectLabel != null)
                R.Set(_effectLabel, "text", hasText ? Localization.F("forecast.effect_header") + ": " + text : string.Empty);

            ConfigureEffectGeometry(showIcon);
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
                Math.Max(1, Mathf.RoundToInt(258f - iconSpace)),
                24,
                "TopLeft",
                "Left",
                "ShrinkContent",
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

        private static void MoveCraftButton()
        {
            if (!_buttonCaptured || _buttonObject == null) return;
            Vector3 p = _originalButtonPosition;
            _buttonObject.transform.localPosition = new Vector3(p.x, PulpitTuning.CraftButtonY.Value, p.z);
        }

        private static void RestoreCraftButton()
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
            _windowBack = CaptureStretchPart(window, "back", 0f);
            _decoreBack = CaptureStretchPart(window, "decore_back", 1f);
            _decore = CaptureStretchPart(window, "decore", 0.5f);
            _inactiveBack = CaptureStretchPart(window, "back for inactive stuff", 0.5f);

            _tipsTransform = window.Find("buttons tips");
            _originalTipsPosition = _tipsTransform == null ? Vector3.zero : _tipsTransform.localPosition;
            _windowCaptured = true;
        }

        private static StretchPart CaptureStretchPart(Transform window, string childName, float downFactor)
        {
            Transform transform = window == null ? null : window.Find(childName);
            if (transform == null) return null;

            Type spriteType = R.AnyType("UI2DSprite");
            object widget = spriteType == null ? null : transform.gameObject.GetComponent(spriteType);
            return new StretchPart
            {
                Transform = transform,
                Widget = widget,
                OriginalHeight = widget == null ? 0 : Math.Max(1, R.Int(R.Get(widget, "height"))),
                OriginalPosition = transform.localPosition,
                DownFactor = downFactor
            };
        }

        private static void ApplyWindowExtension()
        {
            if (!_windowCaptured) return;
            float extra = Mathf.Max(0f, PulpitTuning.WindowExtraHeight.Value);

            Stretch(_windowBack, extra);
            Stretch(_decoreBack, extra);
            Stretch(_decore, extra);
            Stretch(_inactiveBack, extra);

            if (_tipsTransform != null)
            {
                Vector3 p = _originalTipsPosition;
                _tipsTransform.localPosition = new Vector3(p.x, p.y - extra, p.z);
            }
        }

        private static void Stretch(StretchPart part, float extra)
        {
            if (part == null || part.Transform == null) return;

            if (part.Widget != null && part.OriginalHeight > 0)
                TrySet(part.Widget, "height", Math.Max(1, Mathf.RoundToInt(part.OriginalHeight + extra)));

            Vector3 p = part.OriginalPosition;
            part.Transform.localPosition = new Vector3(p.x, p.y - extra * part.DownFactor, p.z);
        }

        private static void RestoreWindow()
        {
            if (!_windowCaptured) return;

            RestoreStretchPart(_windowBack);
            RestoreStretchPart(_decoreBack);
            RestoreStretchPart(_decore);
            RestoreStretchPart(_inactiveBack);

            if (_tipsTransform != null)
                _tipsTransform.localPosition = _originalTipsPosition;
        }

        private static void RestoreStretchPart(StretchPart part)
        {
            if (part == null || part.Transform == null) return;
            if (part.Widget != null && part.OriginalHeight > 0)
                TrySet(part.Widget, "height", part.OriginalHeight);
            part.Transform.localPosition = part.OriginalPosition;
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
