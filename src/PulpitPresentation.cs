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
        private static object _context;
        private static object _guaranteedLabel;
        private static object _guaranteedFaith;
        private static object _guaranteedMoney;
        private static object _successLabel;
        private static object _successFaith;
        private static object _successMoney;
        private static object _specialLabel;
        private static object _specialIcon;
        private static MethodInfo _getSprite;

        internal static void Render(object template, string vanillaContext, PrayerForecast.Result forecast)
        {
            if (template == null || forecast == null) return;
            Ensure(template);

            string faithBase = "(faith) " + forecast.BaseFaith;
            string faithBonus = forecast.BonusFaith == 0 ? "—" : "(faith) +" + forecast.BonusFaith;
            string moneyBase = R.FormatMoney(forecast.BaseMoney);
            string moneyBonus = Math.Abs(forecast.BonusMoney) < 0.0001f
                ? "—"
                : "+" + R.FormatMoney(forecast.BonusMoney);

            SetText(_context, vanillaContext);
            SetText(_guaranteedLabel, Localization.F("forecast.guaranteed"));
            SetText(_guaranteedFaith, faithBase);
            SetText(_guaranteedMoney, moneyBase);
            SetText(_successLabel, Localization.F("forecast.success_bonus", forecast.ChancePercent));
            SetText(_successFaith, faithBonus);
            SetText(_successMoney, moneyBonus);
            SetSpecial(forecast.SpecialText, forecast.SpecialIconName);

            _root.SetActive(true);
            R.Set(template, "text", string.Empty);
        }

        internal static void Hide(object template)
        {
            if (_root != null) _root.SetActive(false);
        }

        private static void Ensure(object template)
        {
            if (ReferenceEquals(_template, template) && _root != null) return;

            if (_root != null) UnityEngine.Object.Destroy(_root);
            _template = template;

            GameObject templateGo = R.Get(template, "gameObject") as GameObject;
            if (templateGo == null) throw new InvalidOperationException("Pulpit label GameObject is unavailable.");

            Transform parent = templateGo.transform.parent;
            if (parent == null) throw new InvalidOperationException("Pulpit label parent is unavailable.");

            _root = new GameObject("PrayerClarity.PulpitForecast");
            _root.layer = templateGo.layer;
            _root.transform.SetParent(parent, false);
            _root.transform.localPosition = Vector3.zero;
            _root.transform.localRotation = Quaternion.identity;
            _root.transform.localScale = Vector3.one;

            int width = Math.Max(1, R.Int(R.Get(template, "width")));
            int height = Math.Max(1, R.Int(R.Get(template, "height")));
            int fontSize = Math.Max(1, R.Int(R.Get(template, "fontSize")));
            int depth = R.Int(R.Get(template, "depth"));
            Vector3 pos = templateGo.transform.localPosition;
            string pivot = Convert.ToString(R.Get(template, "pivot"));

            float left = pos.x - width * PivotX(pivot);
            float top = pos.y + height * PivotTop(pivot);
            float line = Math.Max(fontSize + 2f, 14f);
            float contextHeight = line * 2.05f;
            float rowHeight = line * 1.25f;
            float sectionGap = Math.Max(6f, line * 0.42f);
            float rowGap = Math.Max(2f, line * 0.16f);

            float labelWidth = width * 0.56f;
            float faithWidth = width * 0.17f;
            float moneyWidth = width - labelWidth - faithWidth;
            float row1Y = top - contextHeight - sectionGap;
            float row2Y = row1Y - rowHeight - rowGap;
            float specialY = row2Y - rowHeight - Math.Max(5f, line * 0.34f);

            _context = CreateLabel("Context", template, depth + 1);
            ConfigureLabel(_context, left, top, width, contextHeight, "TopLeft", "Center", "ClampContent");

            _guaranteedLabel = CreateLabel("Guaranteed.Label", template, depth + 1);
            ConfigureLabel(_guaranteedLabel, left, row1Y, labelWidth, rowHeight, "TopLeft", "Right", "ClampContent");

            _guaranteedFaith = CreateLabel("Guaranteed.Faith", template, depth + 1);
            ConfigureLabel(_guaranteedFaith, left + labelWidth, row1Y, faithWidth, rowHeight, "TopLeft", "Left", "ClampContent");

            _guaranteedMoney = CreateLabel("Guaranteed.Money", template, depth + 1);
            ConfigureLabel(_guaranteedMoney, left + labelWidth + faithWidth, row1Y, moneyWidth, rowHeight, "TopLeft", "Left", "ClampContent");

            _successLabel = CreateLabel("Success.Label", template, depth + 1);
            ConfigureLabel(_successLabel, left, row2Y, labelWidth, rowHeight, "TopLeft", "Right", "ClampContent");

            _successFaith = CreateLabel("Success.Faith", template, depth + 1);
            ConfigureLabel(_successFaith, left + labelWidth, row2Y, faithWidth, rowHeight, "TopLeft", "Left", "ClampContent");

            _successMoney = CreateLabel("Success.Money", template, depth + 1);
            ConfigureLabel(_successMoney, left + labelWidth + faithWidth, row2Y, moneyWidth, rowHeight, "TopLeft", "Left", "ClampContent");

            float iconSize = Math.Max(16f, fontSize + 2f);
            _specialIcon = CreateSprite("Special.Icon", depth + 2);
            ConfigureWidget(_specialIcon, left, specialY - rowHeight * 0.5f, iconSize, iconSize, "Left");

            _specialLabel = CreateLabel("Special.Text", template, depth + 1);
            ConfigureLabel(_specialLabel, left + iconSize + 5f, specialY, width - iconSize - 5f, rowHeight,
                "TopLeft", "Left", "ResizeHeight");

            _root.SetActive(false);
        }

        private static object CreateLabel(string suffix, object template, int depth)
        {
            GameObject templateGo = R.Get(template, "gameObject") as GameObject;
            GameObject go = new GameObject("PrayerClarity." + suffix);
            go.layer = templateGo == null ? _root.layer : templateGo.layer;
            go.transform.SetParent(_root.transform, false);
            go.transform.localScale = templateGo == null ? Vector3.one : templateGo.transform.localScale;

            object label = go.AddComponent(template.GetType());
            CopyStyle(template, label);
            TrySet(label, "depth", depth);
            return label;
        }

        private static object CreateSprite(string suffix, int depth)
        {
            Type type = R.AnyType("UI2DSprite");
            if (type == null) return null;

            GameObject go = new GameObject("PrayerClarity." + suffix);
            go.layer = _root.layer;
            go.transform.SetParent(_root.transform, false);
            object sprite = go.AddComponent(type);
            TrySet(sprite, "depth", depth);
            return sprite;
        }

        private static void CopyStyle(object source, object target)
        {
            // Preserve the bitmap font when the vanilla label uses one. Its symbol table is
            // what makes inline tokens such as (faith) render as native icons. Setting the
            // derived trueTypeFont afterwards would clear that bitmap font in NGUI.
            object bitmapFont = R.Get(source, "bitmapFont");
            if (bitmapFont != null)
                TrySet(target, "bitmapFont", bitmapFont);
            else
                TrySet(target, "trueTypeFont", R.Get(source, "trueTypeFont"));

            string[] properties =
            {
                "fontSize", "fontStyle", "color", "effectStyle", "effectColor",
                "effectDistance", "supportEncoding", "symbolStyle", "spacingX",
                "spacingY", "useFloatSpacing", "floatSpacingX", "floatSpacingY",
                "applyGradient", "gradientTop", "gradientBottom"
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

        private static void ConfigureLabel(object label, float x, float y, float width, float height,
            string pivot, string alignment, string overflow)
        {
            ConfigureWidget(label, x, y, width, height, pivot);
            SetEnum(label, "alignment", alignment);
            SetEnum(label, "overflowMethod", overflow);
        }

        private static void ConfigureWidget(object widget, float x, float y, float width, float height, string pivot)
        {
            if (widget == null) return;

            GameObject go = R.Get(widget, "gameObject") as GameObject;
            if (go == null) return;

            go.transform.localPosition = new Vector3(x, y, 0f);
            TrySet(widget, "width", Math.Max(1, Mathf.RoundToInt(width)));
            TrySet(widget, "height", Math.Max(1, Mathf.RoundToInt(height)));
            SetEnum(widget, "pivot", pivot);
        }

        private static void SetSpecial(string text, string iconName)
        {
            bool hasText = !string.IsNullOrEmpty(text);
            GameObject labelGo = _specialLabel == null ? null : R.Get(_specialLabel, "gameObject") as GameObject;
            if (labelGo != null) labelGo.SetActive(hasText);

            Sprite sprite = hasText ? ResolveSprite(iconName) : null;
            GameObject iconGo = _specialIcon == null ? null : R.Get(_specialIcon, "gameObject") as GameObject;
            bool showIcon = sprite != null && iconGo != null;

            if (showIcon)
            {
                TrySet(_specialIcon, "sprite2D", sprite);
                iconGo.SetActive(true);
            }
            else if (iconGo != null)
            {
                iconGo.SetActive(false);
            }

            if (_specialLabel != null)
            {
                SetText(_specialLabel, text ?? string.Empty);

                int width = Math.Max(1, R.Int(R.Get(_context, "width")));
                GameObject contextGo = R.Get(_context, "gameObject") as GameObject;
                GameObject specialGo = R.Get(_specialLabel, "gameObject") as GameObject;
                GameObject specialIconGo = _specialIcon == null ? null : R.Get(_specialIcon, "gameObject") as GameObject;

                if (contextGo != null && specialGo != null)
                {
                    float left = contextGo.transform.localPosition.x;
                    float iconWidth = showIcon ? Math.Max(16, R.Int(R.Get(_specialIcon, "width"))) + 5f : 0f;
                    Vector3 p = specialGo.transform.localPosition;
                    specialGo.transform.localPosition = new Vector3(left + iconWidth, p.y, p.z);
                    TrySet(_specialLabel, "width", Math.Max(1, Mathf.RoundToInt(width - iconWidth)));
                }

                if (specialIconGo != null && contextGo != null)
                {
                    Vector3 p = specialIconGo.transform.localPosition;
                    specialIconGo.transform.localPosition = new Vector3(contextGo.transform.localPosition.x, p.y, p.z);
                }
            }
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

        private static void SetText(object label, string text)
        {
            if (label != null) R.Set(label, "text", text ?? string.Empty);
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

        private static float PivotX(string pivot)
        {
            if (string.IsNullOrEmpty(pivot)) return 0.5f;
            if (pivot.IndexOf("Left", StringComparison.OrdinalIgnoreCase) >= 0) return 0f;
            if (pivot.IndexOf("Right", StringComparison.OrdinalIgnoreCase) >= 0) return 1f;
            return 0.5f;
        }

        private static float PivotTop(string pivot)
        {
            if (string.IsNullOrEmpty(pivot)) return 0.5f;
            if (pivot.IndexOf("Top", StringComparison.OrdinalIgnoreCase) >= 0) return 0f;
            if (pivot.IndexOf("Bottom", StringComparison.OrdinalIgnoreCase) >= 0) return 1f;
            return 0.5f;
        }
    }
}
