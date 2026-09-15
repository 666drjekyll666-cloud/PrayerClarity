using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PrayerClarity
{
    // Final pulpit effect-row pass for 0.1.12.
    //
    // Two icon roles are deliberately separate:
    // 1) the leading icon is the selected prayer's actual BuffDefinition.GetIconName(),
    //    exactly the same source used by vanilla BuffIcon.Draw for the active-buff HUD;
    // 2) an inline icon may carry a concrete reward/resource noun inside the sentence.
    //
    // All work is pulpit-redraw/config-change driven. There is no polling or scene scan.
    internal static class PulpitEffectIconPolicy
    {
        private static readonly Dictionary<string, Sprite> SpriteCache =
            new Dictionary<string, Sprite>(StringComparer.Ordinal);

        private static object _template;
        private static object _gui;
        private static PrayerForecast.Result _forecast;
        private static Transform _root;
        private static object _effectLabel;
        private static object _leadingIcon;
        private static object _inlineIcon;
        private static object _containerWidget;
        private static MethodInfo _getSpriteMethod;
        private static MethodInfo _labelSizeCalcMethod;

        internal static void Apply(object template, object gui, PrayerForecast.Result forecast)
        {
            if (template == null || gui == null || forecast == null) return;
            _template = template;
            _gui = gui;
            _forecast = forecast;
            Capture();
            ApplyTuning();
        }

        internal static void ApplyTuning()
        {
            if (_template == null || _gui == null || _forecast == null) return;
            Capture();
            if (_effectLabel == null) return;

            object craft = R.Get(_gui, "pray_craft");
            string craftId = R.Id(craft) ?? string.Empty;
            bool hasSemanticEffect = !string.IsNullOrEmpty(_forecast.SpecialText);

            string buffIconName = hasSemanticEffect ? GetSelectedBuffIconName(craft) : null;
            Sprite buffSprite = ResolveSprite(buffIconName);
            bool leadingVisible = SetSprite(_leadingIcon, buffSprite);

            // Correct the semantic result after older presentation layers have run:
            // SpecialIconName now means the actual active-buff HUD icon only.
            _forecast.SpecialIconName = buffIconName;

            string inlineSpriteName = null;
            string effectText = null;
            string fallbackText = null;
            string marker = null;

            if (craftId.StartsWith("pray:b_village:", StringComparison.Ordinal))
            {
                int count = CountOutput(craft, "blessing_commerce");
                if (count > 0)
                {
                    marker = "×" + count;
                    string description = R.VanillaLocalize("blessing_commerce_d");
                    bool hasDescription = !string.IsNullOrEmpty(description) &&
                                          !string.Equals(description, "blessing_commerce_d", StringComparison.Ordinal);
                    effectText = marker + (hasDescription ? ". " + description : string.Empty);

                    string itemName = R.VanillaLocalize("blessing_commerce");
                    fallbackText = marker +
                                   (string.IsNullOrEmpty(itemName) || string.Equals(itemName, "blessing_commerce", StringComparison.Ordinal)
                                       ? string.Empty
                                       : " " + itemName) +
                                   (hasDescription ? ". " + description : string.Empty);
                    inlineSpriteName = "i_scroll_3";
                }
            }
            else if (craftId.StartsWith("pray:b_sin_shard:", StringComparison.Ordinal))
            {
                float duration = R.Float(R.Get(craft, "dur_parameter"));
                effectText = Localization.F("buff.sin_shard", duration);
                marker = FindMultiplierMarker(effectText);
                if (!string.IsNullOrEmpty(marker))
                {
                    string resourceName = R.VanillaLocalize("sin_shard");
                    fallbackText = InsertAfterMarker(effectText, marker,
                        string.IsNullOrEmpty(resourceName) || string.Equals(resourceName, "sin_shard", StringComparison.Ordinal)
                            ? string.Empty
                            : " " + resourceName);
                    inlineSpriteName = "i_sin_shard";
                }
            }

            int iconSize = PulpitTuning.EffectIconSize.Value;
            float labelX = PulpitTuning.EffectX.Value + (leadingVisible ? iconSize + 4f : 0f);
            ConfigureLeadingGeometry(iconSize);
            ConfigureLabelGeometry(labelX);

            if (string.IsNullOrEmpty(inlineSpriteName) || string.IsNullOrEmpty(effectText) || string.IsNullOrEmpty(marker))
            {
                SetSprite(_inlineIcon, null);
                return;
            }

            Sprite inlineSprite = ResolveSprite(inlineSpriteName);
            if (inlineSprite == null || !TryBuildInlineText(effectText, marker, labelX, iconSize, inlineSprite, out string rendered))
            {
                SetSprite(_inlineIcon, null);
                SetEffectText(string.IsNullOrEmpty(fallbackText) ? effectText : fallbackText);
                return;
            }

            SetEffectText(rendered);
        }

        internal static void Restore()
        {
            SetSprite(_inlineIcon, null);
        }

        private static void Capture()
        {
            GameObject templateGo = R.Get(_template, "gameObject") as GameObject;
            Transform container = templateGo == null ? null : templateGo.transform.parent;
            Transform root = container == null ? null : container.Find("PrayerClarity.PulpitForecast");
            if (root == null) return;

            if (!ReferenceEquals(_root, root))
            {
                _root = root;
                _effectLabel = null;
                _leadingIcon = null;
                _inlineIcon = null;
                _containerWidget = null;
            }

            Transform effect = root.Find("PrayerClarity.Effect");
            _effectLabel = effect == null ? null : effect.gameObject.GetComponent(_template.GetType());
            _leadingIcon = GetComponent(root.Find("PrayerClarity.Effect.Icon"), "UI2DSprite");

            Type uiWidgetType = R.AnyType("UIWidget");
            _containerWidget = uiWidgetType == null || container == null
                ? null
                : container.gameObject.GetComponent(uiWidgetType);

            if (_inlineIcon == null)
            {
                Transform existing = root.Find("PrayerClarity.Effect.InlineIcon");
                _inlineIcon = GetComponent(existing, "UI2DSprite");
                if (_inlineIcon == null)
                    _inlineIcon = CreateInlineIcon(root);
            }
        }

        private static object CreateInlineIcon(Transform root)
        {
            Type type = R.AnyType("UI2DSprite");
            if (type == null || root == null) return null;

            GameObject labelGo = _effectLabel == null ? null : R.Get(_effectLabel, "gameObject") as GameObject;
            GameObject go = new GameObject("PrayerClarity.Effect.InlineIcon");
            go.layer = labelGo == null ? root.gameObject.layer : labelGo.layer;
            go.transform.SetParent(root, false);
            go.transform.localScale = labelGo == null ? Vector3.one : labelGo.transform.localScale;

            object sprite = go.AddComponent(type);
            int depth = _effectLabel == null ? 0 : R.Int(R.Get(_effectLabel, "depth"));
            TrySet(sprite, "depth", depth + 2);
            go.SetActive(false);
            return sprite;
        }

        private static string GetSelectedBuffIconName(object craft)
        {
            string buffId = craft == null ? null : R.Get(craft, "buff") as string;
            if (string.IsNullOrEmpty(buffId)) return null;

            try
            {
                object buff = R.BalanceData(buffId, "BuffDefinition", true);
                if (buff == null) return null;
                MethodInfo getIcon = R.Method(buff.GetType(), "GetIconName", false, 0);
                object value = getIcon == null ? null : getIcon.Invoke(buff, null);
                return value == null ? null : value.ToString();
            }
            catch
            {
                return null;
            }
        }

        private static bool TryBuildInlineText(string effectText, string marker, float labelX, int iconSize,
            Sprite sprite, out string rendered)
        {
            rendered = effectText;
            int markerIndex = effectText.IndexOf(marker, StringComparison.Ordinal);
            if (markerIndex < 0) return false;

            string header = Localization.F("forecast.effect_header") + ": ";
            string prefixEffect = effectText.Substring(0, markerIndex + marker.Length);
            string prefix = header + prefixEffect;

            float prefixWidth;
            if (!TryMeasure(prefix, out prefixWidth)) return false;

            string gap = BuildGap(prefix, iconSize + 3f);
            if (string.IsNullOrEmpty(gap)) return false;

            rendered = effectText.Insert(markerIndex + marker.Length, gap);
            SetEffectText(rendered);

            if (!SetSprite(_inlineIcon, sprite)) return false;
            ConfigureWidget(_inlineIcon,
                labelX + prefixWidth + 1f,
                PulpitTuning.EffectY.Value - 1f,
                iconSize,
                iconSize,
                "TopLeft");
            return true;
        }

        private static string BuildGap(string prefix, float requiredWidth)
        {
            float baseWidth;
            if (!TryMeasure(prefix, out baseWidth)) return null;

            for (int n = 1; n <= 12; n++)
            {
                string spaces = new string(' ', n);
                float withGap;
                if (!TryMeasure(prefix + spaces, out withGap)) return null;
                if (withGap - baseWidth >= requiredWidth) return spaces;
            }
            return null;
        }

        private static bool TryMeasure(string text, out float width)
        {
            width = 0f;
            if (_effectLabel == null) return false;

            try
            {
                if (_labelSizeCalcMethod == null)
                {
                    Type calcType = R.GameType("LabelSizeCalculator") ?? R.AnyType("LabelSizeCalculator");
                    _labelSizeCalcMethod = R.Method(calcType, "Calc", true,
                        new[] { _effectLabel.GetType(), typeof(string) });
                }
                if (_labelSizeCalcMethod == null) return false;

                object result = _labelSizeCalcMethod.Invoke(null, new object[] { _effectLabel, text });
                if (!(result is Vector2)) return false;
                width = ((Vector2)result).x;
                return width >= 0f;
            }
            catch
            {
                return false;
            }
        }

        private static void SetEffectText(string effectText)
        {
            if (_effectLabel == null) return;
            R.Set(_effectLabel, "text", Localization.F("forecast.effect_header") + ": " + (effectText ?? string.Empty));
        }

        private static void ConfigureLeadingGeometry(int iconSize)
        {
            ConfigureWidget(_leadingIcon,
                PulpitTuning.EffectX.Value,
                PulpitTuning.EffectY.Value - 1f,
                iconSize,
                iconSize,
                "TopLeft");
        }

        private static void ConfigureLabelGeometry(float labelX)
        {
            if (_effectLabel == null) return;
            GameObject go = R.Get(_effectLabel, "gameObject") as GameObject;
            if (go == null) return;

            int containerWidth = _containerWidget == null ? 274 : Math.Max(1, R.Int(R.Get(_containerWidget, "width")));
            float safeRight = containerWidth * 0.5f - 16f;
            int width = Math.Max(48, Mathf.FloorToInt(safeRight - labelX));

            TrySet(_effectLabel, "width", width);
            go.transform.localPosition = new Vector3(labelX, PulpitTuning.EffectY.Value, go.transform.localPosition.z);
        }

        private static bool SetSprite(object widget, Sprite sprite)
        {
            if (widget == null) return false;
            GameObject go = R.Get(widget, "gameObject") as GameObject;
            if (go == null) return false;

            if (sprite == null)
            {
                go.SetActive(false);
                return false;
            }

            TrySet(widget, "sprite2D", sprite);
            go.SetActive(true);
            return true;
        }

        private static Sprite ResolveSprite(string iconName)
        {
            if (string.IsNullOrEmpty(iconName)) return null;

            Sprite cached;
            if (SpriteCache.TryGetValue(iconName, out cached)) return cached;

            try
            {
                if (_getSpriteMethod == null)
                {
                    Type type = R.GameType("EasySpritesCollection") ?? R.AnyType("EasySpritesCollection");
                    _getSpriteMethod = R.Method(type, "GetSprite", true,
                        new[] { typeof(string), typeof(bool), typeof(string) });
                }

                Sprite sprite = _getSpriteMethod == null
                    ? null
                    : _getSpriteMethod.Invoke(null, new object[] { iconName, false, string.Empty }) as Sprite;
                if (sprite != null) SpriteCache[iconName] = sprite;
                return sprite;
            }
            catch
            {
                return null;
            }
        }

        private static int CountOutput(object craft, string id)
        {
            IEnumerable output = craft == null ? null : R.Get(craft, "output") as IEnumerable;
            if (output == null) return 0;

            int total = 0;
            foreach (object item in output)
            {
                if (item == null || !string.Equals(R.Id(item), id, StringComparison.Ordinal)) continue;
                total += Math.Max(0, R.Int(R.Get(item, "value")));
            }
            return total;
        }

        private static string FindMultiplierMarker(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            int i = text.IndexOf('×');
            if (i < 0 || i + 1 >= text.Length) return null;
            int j = i + 1;
            while (j < text.Length && char.IsDigit(text[j])) j++;
            return j == i + 1 ? null : text.Substring(i, j - i);
        }

        private static string InsertAfterMarker(string text, string marker, string insertion)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(marker) || string.IsNullOrEmpty(insertion)) return text;
            int i = text.IndexOf(marker, StringComparison.Ordinal);
            return i < 0 ? text : text.Insert(i + marker.Length, insertion);
        }

        private static object GetComponent(Transform transform, string typeName)
        {
            if (transform == null) return null;
            Type type = R.AnyType(typeName);
            return type == null ? null : transform.gameObject.GetComponent(type);
        }

        private static void ConfigureWidget(object widget, float x, float y, int width, int height, string pivot)
        {
            if (widget == null) return;
            GameObject go = R.Get(widget, "gameObject") as GameObject;
            if (go == null) return;

            SetEnum(widget, "pivot", pivot);
            TrySet(widget, "width", width);
            TrySet(widget, "height", height);
            go.transform.localPosition = new Vector3(x, y, go.transform.localPosition.z);
        }

        private static void SetEnum(object obj, string property, string name)
        {
            if (obj == null) return;
            object current = R.Get(obj, property);
            if (current == null) return;
            try { R.Set(obj, property, Enum.Parse(current.GetType(), name)); }
            catch { }
        }

        private static void TrySet(object obj, string property, object value)
        {
            if (obj == null) return;
            try { R.Set(obj, property, value); }
            catch { }
        }
    }
}
