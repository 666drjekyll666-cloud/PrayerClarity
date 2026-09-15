using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PrayerClarity
{
    // Final presentation pass for the live-tuned pulpit prototype. It runs only on
    // pulpit redraw/config changes, after PulpitLayoutV4 has applied window geometry.
    // Leading effect icons come from the same BuffDefinition.GetIconName() seam used
    // by vanilla BuffIcon.Draw. Concrete reward/resource icons remain separate inline
    // nouns inside the effect sentence.
    internal static class PulpitPolish
    {
        private static readonly Dictionary<string, Sprite> SpriteCache =
            new Dictionary<string, Sprite>(StringComparer.Ordinal);
        private static readonly Dictionary<string, string> ItemIconNameCache =
            new Dictionary<string, string>(StringComparer.Ordinal);

        private static object _template;
        private static object _gui;
        private static PrayerForecast.Result _forecast;

        private static Transform _window;
        private static Transform _container;
        private static Transform _root;
        private static object _containerWidget;
        private static Transform _craftButton;
        private static Vector3 _craftButtonOriginalPosition;
        private static bool _craftButtonCaptured;

        private static object _effectLabel;
        private static GameObject _effectLabelObject;
        private static object _effectIcon;
        private static object _inlineIcon;
        private static MethodInfo _getSpriteMethod;
        private static MethodInfo _labelSizeCalcMethod;
        private static ConstructorInfo _itemConstructor;
        private static MethodInfo _itemGetIconMethod;
        private static bool _itemIconPathResolved;

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
            MovePrayerButton();
            PolishEffectRow();
        }

        internal static void Restore()
        {
            SetSprite(_inlineIcon, null);
            if (_craftButtonCaptured && _craftButton != null)
                _craftButton.localPosition = _craftButtonOriginalPosition;
        }

        private static void Capture()
        {
            GameObject templateGo = R.Get(_template, "gameObject") as GameObject;
            if (templateGo == null) return;

            Transform container = templateGo.transform.parent;
            Transform window = container == null ? null : container.parent;
            if (window == null) return;

            if (!ReferenceEquals(_window, window))
            {
                _window = window;
                _container = container;
                Type uiWidgetType = R.AnyType("UIWidget");
                _containerWidget = uiWidgetType == null || container == null
                    ? null
                    : container.gameObject.GetComponent(uiWidgetType);

                _craftButton = window.Find("craft button");
                _craftButtonOriginalPosition = _craftButton == null ? Vector3.zero : _craftButton.localPosition;
                _craftButtonCaptured = _craftButton != null;

                _root = null;
                _effectLabel = null;
                _effectLabelObject = null;
                _effectIcon = null;
                _inlineIcon = null;
            }

            Transform root = _container == null ? null : _container.Find("PrayerClarity.PulpitForecast");
            if (root == null) return;
            _root = root;

            Transform effect = root.Find("PrayerClarity.Effect");
            if (effect != null)
            {
                _effectLabelObject = effect.gameObject;
                _effectLabel = effect.gameObject.GetComponent(_template.GetType());
            }

            _effectIcon = GetComponent(root.Find("PrayerClarity.Effect.Icon"), "UI2DSprite");

            Transform existingInline = root.Find("PrayerClarity.Effect.InlineIcon");
            _inlineIcon = GetComponent(existingInline, "UI2DSprite");
            if (_inlineIcon == null)
                _inlineIcon = CreateInlineIcon(root);
        }

        private static void MovePrayerButton()
        {
            if (!_craftButtonCaptured || _craftButton == null) return;
            _craftButton.localPosition = new Vector3(
                PulpitTuning.PrayerButtonX.Value,
                PulpitTuning.PrayerButtonY.Value,
                _craftButtonOriginalPosition.z);
        }

        private static void PolishEffectRow()
        {
            if (_effectLabel == null || _effectLabelObject == null) return;

            object craft = R.Get(_gui, "pray_craft");
            string craftId = R.Id(craft) ?? string.Empty;
            string body = _forecast.SpecialText;
            if (string.IsNullOrEmpty(body)) body = "—";

            string leadingIconName = body == "—" ? null : GetBuffIconName(craft);
            _forecast.SpecialIconName = leadingIconName;
            bool leadingVisible = SetSprite(_effectIcon, ResolveSprite(leadingIconName));

            int iconSize = PulpitTuning.EffectIconSize.Value;
            float labelX = PulpitTuning.EffectX.Value + (leadingVisible ? iconSize + 4f : 0f);
            ConfigureWidget(_effectIcon,
                PulpitTuning.EffectX.Value,
                PulpitTuning.EffectY.Value - 1f,
                iconSize,
                iconSize,
                "TopLeft");
            ConfigureLabelGeometry(labelX);

            string inlineSpriteName = null;
            string marker = null;
            string fallbackBody = body;

            if (craftId.StartsWith("pray:b_village:", StringComparison.Ordinal))
            {
                int count = CountOutput(craft, "blessing_commerce");
                if (count > 0)
                {
                    marker = "×" + count;
                    string description = R.VanillaLocalize("blessing_commerce_d");
                    bool hasDescription = !string.IsNullOrEmpty(description) &&
                                          !string.Equals(description, "blessing_commerce_d", StringComparison.Ordinal);
                    body = marker + (hasDescription ? ". " + description : string.Empty);

                    string itemName = R.VanillaLocalize("blessing_commerce");
                    fallbackBody = marker +
                                   (string.IsNullOrEmpty(itemName) || string.Equals(itemName, "blessing_commerce", StringComparison.Ordinal)
                                       ? string.Empty
                                       : " " + itemName) +
                                   (hasDescription ? ". " + description : string.Empty);
                    inlineSpriteName = GetItemIconName("blessing_commerce");
                }
            }
            else if (craftId.StartsWith("pray:b_sin_shard:", StringComparison.Ordinal))
            {
                float duration = R.Float(R.Get(craft, "dur_parameter"));
                body = Localization.F("buff.sin_shard", duration);
                marker = FindMultiplierMarker(body);
                if (!string.IsNullOrEmpty(marker))
                {
                    string resourceName = R.VanillaLocalize("sin_shard");
                    fallbackBody = InsertAfterMarker(body, marker,
                        string.IsNullOrEmpty(resourceName) || string.Equals(resourceName, "sin_shard", StringComparison.Ordinal)
                            ? string.Empty
                            : " " + resourceName);
                    inlineSpriteName = GetItemIconName("sin_shard");
                }
            }

            _effectLabelObject.SetActive(true);

            Sprite inlineSprite = ResolveSprite(inlineSpriteName);
            if (inlineSprite != null && !string.IsNullOrEmpty(marker))
            {
                float prefixWidth;
                string prefix = Localization.F("forecast.effect_header") + ": " +
                                body.Substring(0, body.IndexOf(marker, StringComparison.Ordinal) + marker.Length);
                string gap = TryMeasure(prefix, out prefixWidth)
                    ? BuildGap(prefix, iconSize + 3f)
                    : null;

                if (!string.IsNullOrEmpty(gap) && SetSprite(_inlineIcon, inlineSprite))
                {
                    int markerIndex = body.IndexOf(marker, StringComparison.Ordinal);
                    body = body.Insert(markerIndex + marker.Length, gap);
                    ConfigureWidget(_inlineIcon,
                        labelX + prefixWidth + 1f,
                        PulpitTuning.EffectY.Value - 1f,
                        iconSize,
                        iconSize,
                        "TopLeft");
                }
                else
                {
                    SetSprite(_inlineIcon, null);
                    body = fallbackBody;
                }
            }
            else
            {
                SetSprite(_inlineIcon, null);
                body = fallbackBody;
            }

            R.Set(_effectLabel, "text", Localization.F("forecast.effect_header") + ": " + body);
        }

        private static string GetBuffIconName(object craft)
        {
            string buffId = craft == null ? null : R.Get(craft, "buff") as string;
            if (string.IsNullOrEmpty(buffId)) return null;

            try
            {
                object buff = R.BalanceData(buffId, "BuffDefinition", true);
                if (buff == null) return null;
                MethodInfo method = R.Method(buff.GetType(), "GetIconName", false, 0);
                object value = method == null ? null : method.Invoke(buff, null);
                return value == null ? null : value.ToString();
            }
            catch
            {
                return null;
            }
        }

        private static string GetItemIconName(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return null;

            string cached;
            if (ItemIconNameCache.TryGetValue(itemId, out cached)) return cached;

            try
            {
                if (!_itemIconPathResolved)
                {
                    Type itemType = R.GameType("Item");
                    _itemConstructor = itemType == null
                        ? null
                        : itemType.GetConstructor(R.Inst, null, new[] { typeof(string), typeof(int) }, null);
                    _itemGetIconMethod = R.Method(itemType, "GetIcon", false, 0);
                    _itemIconPathResolved = true;
                }

                if (_itemConstructor == null || _itemGetIconMethod == null) return null;
                object item = _itemConstructor.Invoke(new object[] { itemId, 1 });
                object value = _itemGetIconMethod.Invoke(item, null);
                string iconName = value == null ? null : value.ToString();
                if (!string.IsNullOrEmpty(iconName)) ItemIconNameCache[itemId] = iconName;
                return iconName;
            }
            catch
            {
                return null;
            }
        }

        private static object CreateInlineIcon(Transform root)
        {
            Type type = R.AnyType("UI2DSprite");
            if (type == null || root == null) return null;

            GameObject go = new GameObject("PrayerClarity.Effect.InlineIcon");
            go.layer = _effectLabelObject == null ? root.gameObject.layer : _effectLabelObject.layer;
            go.transform.SetParent(root, false);
            go.transform.localScale = _effectLabelObject == null ? Vector3.one : _effectLabelObject.transform.localScale;

            object sprite = go.AddComponent(type);
            int depth = _effectLabel == null ? 0 : R.Int(R.Get(_effectLabel, "depth"));
            TrySet(sprite, "depth", depth + 2);
            go.SetActive(false);
            return sprite;
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

        private static bool TryMeasure(string text, out float width)
        {
            width = 0f;
            if (_effectLabel == null) return false;

            try
            {
                if (_labelSizeCalcMethod == null)
                {
                    Type type = R.GameType("LabelSizeCalculator") ?? R.AnyType("LabelSizeCalculator");
                    _labelSizeCalcMethod = R.Method(type, "Calc", true,
                        new[] { _effectLabel.GetType(), typeof(string) });
                }
                if (_labelSizeCalcMethod == null) return false;

                object measured = _labelSizeCalcMethod.Invoke(null, new object[] { _effectLabel, text });
                if (!(measured is Vector2)) return false;
                width = ((Vector2)measured).x;
                return width >= 0f;
            }
            catch
            {
                return false;
            }
        }

        private static string BuildGap(string prefix, float requiredWidth)
        {
            float baseWidth;
            if (!TryMeasure(prefix, out baseWidth)) return null;

            for (int n = 1; n <= 12; n++)
            {
                string spaces = new string(' ', n);
                float width;
                if (!TryMeasure(prefix + spaces, out width)) return null;
                if (width - baseWidth >= requiredWidth) return spaces;
            }
            return null;
        }

        private static void ConfigureLabelGeometry(float labelX)
        {
            int containerWidth = _containerWidget == null ? 274 : Math.Max(1, R.Int(R.Get(_containerWidget, "width")));
            float safeRight = containerWidth * 0.5f - 16f;
            int width = Math.Max(48, Mathf.FloorToInt(safeRight - labelX));
            TrySet(_effectLabel, "width", width);
            _effectLabelObject.transform.localPosition = new Vector3(
                labelX,
                PulpitTuning.EffectY.Value,
                _effectLabelObject.transform.localPosition.z);
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
            int start = text.IndexOf('×');
            if (start < 0 || start + 1 >= text.Length) return null;
            int end = start + 1;
            while (end < text.Length && char.IsDigit(text[end])) end++;
            return end == start + 1 ? null : text.Substring(start, end - start);
        }

        private static string InsertAfterMarker(string text, string marker, string insertion)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(marker) || string.IsNullOrEmpty(insertion)) return text;
            int index = text.IndexOf(marker, StringComparison.Ordinal);
            return index < 0 ? text : text.Insert(index + marker.Length, insertion);
        }

        private static object GetComponent(Transform transform, string typeName)
        {
            if (transform == null) return null;
            Type type = R.AnyType(typeName);
            return type == null ? null : transform.gameObject.GetComponent(type);
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
