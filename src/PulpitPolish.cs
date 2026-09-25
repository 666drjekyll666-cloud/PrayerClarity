using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PrayerClarity
{
    // Final presentation pass for the accepted pulpit layout. It runs only on pulpit
    // redraw after PulpitLayoutV4 has applied the fixed window geometry.
    // Leading effect icons come from the same BuffDefinition.GetIconName() seam used
    // by vanilla BuffIcon.Draw. Item/resource nouns stay as localized text because the
    // attempted inline-item sprite paths did not resolve in the verified 1.407 runtime.
    internal static class PulpitPolish
    {
        private static readonly Dictionary<string, Sprite> SpriteCache =
            new Dictionary<string, Sprite>(StringComparer.Ordinal);

        private static object _template;
        private static object _gui;
        private static PrayerForecast.Result _forecast;

        private static Transform _window;
        private static Transform _container;
        private static Transform _root;
        private static object _containerWidget;
        private static object _windowWidget;
        private static Transform _craftButton;
        private static object _craftButtonWidget;
        private static Vector3 _craftButtonOriginalPosition;
        private static bool _craftButtonCaptured;

        private static object _resultRowsLabel;
        private static GameObject _dependencyNoteObject;
        private static object _effectLabel;
        private static GameObject _effectLabelObject;
        private static object _effectIcon;
        private static MethodInfo _getSpriteMethod;

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
            PolishResultRows();
            PolishEffectRow();
            MovePrayerButton();
        }

        internal static void Restore()
        {
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
                _windowWidget = uiWidgetType == null
                    ? null
                    : window.gameObject.GetComponent(uiWidgetType);

                _craftButton = window.Find("craft button");
                _craftButtonWidget = FindFirstWidget(_craftButton, uiWidgetType);
                _craftButtonOriginalPosition = _craftButton == null ? Vector3.zero : _craftButton.localPosition;
                _craftButtonCaptured = _craftButton != null;

                _root = null;
                _resultRowsLabel = null;
                _dependencyNoteObject = null;
                _effectLabel = null;
                _effectLabelObject = null;
                _effectIcon = null;
            }

            Transform root = _container == null ? null : _container.Find("PrayerClarity.PulpitForecast");
            if (root == null) return;
            _root = root;

            Transform result = root.Find("PrayerClarity.Result");
            _resultRowsLabel = result == null ? null : result.gameObject.GetComponent(_template.GetType());

            Transform note = root.Find("PrayerClarity.DependencyNote");
            _dependencyNoteObject = note == null ? null : note.gameObject;

            Transform effect = root.Find("PrayerClarity.Effect");
            if (effect != null)
            {
                _effectLabelObject = effect.gameObject;
                _effectLabel = effect.gameObject.GetComponent(_template.GetType());
            }

            _effectIcon = GetComponent(root.Find("PrayerClarity.Effect.Icon"), "UI2DSprite");
        }

        private static void MovePrayerButton()
        {
            if (!_craftButtonCaptured || _craftButton == null) return;

            Vector3 target = new Vector3(
                PulpitTuning.PrayerButtonX.Value,
                PulpitTuning.PrayerButtonY.Value,
                _craftButtonOriginalPosition.z);
            _craftButton.localPosition = target;

            if (_effectLabel == null ||
                _effectLabelObject == null ||
                !_effectLabelObject.activeSelf ||
                _craftButtonWidget == null ||
                _window == null)
                return;

            // ResizeHeight has to process the current localized text before its live
            // widget height is authoritative. Measure both widgets in the common
            // window coordinate space so the calculation is independent of their
            // different parents/pivots.
            R.Get(_effectLabel, "processedText");

            float effectBottom;
            float effectTop;
            float buttonBottom;
            float buttonTop;
            if (!TryGetVerticalBounds(_effectLabel, _window, out effectBottom, out effectTop) ||
                !TryGetVerticalBounds(_craftButtonWidget, _window, out buttonBottom, out buttonTop))
                return;

            const float clearance = 8f;
            float allowedButtonTop = effectBottom - clearance;
            if (buttonTop <= allowedButtonTop) return;

            float downward = buttonTop - allowedButtonTop;
            target.y -= downward;

            // The accepted pulpit window already carries 100 UI units of extra height.
            // Keep the adaptive move inside that live window; if a future locale ever
            // exceeds this budget, the visual acceptance test will expose it instead
            // of silently moving the action button outside the parchment.
            float windowBottom;
            float windowTop;
            if (_windowWidget != null &&
                TryGetVerticalBounds(_windowWidget, _window, out windowBottom, out windowTop))
            {
                _craftButton.localPosition = target;
                if (TryGetVerticalBounds(_craftButtonWidget, _window, out buttonBottom, out buttonTop))
                {
                    const float bottomMargin = 10f;
                    float minBottom = windowBottom + bottomMargin;
                    if (buttonBottom < minBottom)
                        target.y += minBottom - buttonBottom;
                }
            }

            _craftButton.localPosition = target;
        }

        private static void PolishResultRows()
        {
            if (_resultRowsLabel == null || _forecast == null) return;

            SetEnum(_resultRowsLabel, "overflowMethod", "ResizeHeight");
            TrySet(_resultRowsLabel, "height", 40);

            if (_dependencyNoteObject != null) _dependencyNoteObject.SetActive(false);
            R.Set(_resultRowsLabel, "text", PresentationText.BuildPulpitResultRows(_forecast));
        }

        private static void PolishEffectRow()
        {
            if (_effectLabel == null || _effectLabelObject == null) return;

            object craft = R.Get(_gui, "pray_craft");
            string craftId = R.Id(craft) ?? string.Empty;
            string body = _forecast.SpecialText;

            if (_forecast.SoulGratitudeFaithCap > 0 && string.IsNullOrEmpty(body))
            {
                _forecast.SpecialIconName = null;
                SetSprite(_effectIcon, null);
                _effectLabelObject.SetActive(false);
                return;
            }

            if (string.IsNullOrEmpty(body)) body = "—";

            // Prosperity is a physical sermon output rather than a timed buff, so use
            // the game's localized item name and description as the readable fallback.
            if (craftId.StartsWith("pray:b_village:", StringComparison.Ordinal))
            {
                int count = CountOutput(craft, "blessing_commerce");
                if (count > 0)
                {
                    string itemName = R.VanillaLocalize("blessing_commerce");
                    string description = R.VanillaLocalize("blessing_commerce_d");
                    bool hasName = !string.IsNullOrEmpty(itemName) &&
                                   !string.Equals(itemName, "blessing_commerce", StringComparison.Ordinal);
                    bool hasDescription = !string.IsNullOrEmpty(description) &&
                                          !string.Equals(description, "blessing_commerce_d", StringComparison.Ordinal);
                    body = "×" + count +
                           (hasName ? " " + itemName : string.Empty) +
                           (hasDescription ? ". " + description : string.Empty);
                }
            }

            string leadingIconName = body == "—" ? null : GetBuffIconName(craft);
            _forecast.SpecialIconName = leadingIconName;
            bool leadingVisible = SetSprite(_effectIcon, ResolveSprite(leadingIconName));

            int iconSize = PulpitTuning.EffectIconSize.Value;
            float labelX = PulpitTuning.EffectX.Value + (leadingVisible ? iconSize + 4f : 0f);
            float effectY = ResolveEffectY();
            ConfigureWidget(_effectIcon,
                PulpitTuning.EffectX.Value,
                effectY - 1f,
                iconSize,
                iconSize,
                "TopLeft");
            ConfigureLabelGeometry(labelX, effectY);

            _effectLabelObject.SetActive(true);
            R.Set(_effectLabel, "text", Localization.F("forecast.effect_header") + ": " + body);
        }

        private static float ResolveEffectY()
        {
            float desired = PulpitTuning.EffectY.Value;
            if (_resultRowsLabel == null) return desired;

            GameObject resultGo = R.Get(_resultRowsLabel, "gameObject") as GameObject;
            if (resultGo == null) return desired;

            R.Get(_resultRowsLabel, "processedText");
            int height = Math.Max(0, R.Int(R.Get(_resultRowsLabel, "height")));
            if (height <= 0) return desired;

            float belowResult = resultGo.transform.localPosition.y - height - 8f;
            return Mathf.Min(desired, belowResult);
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

        private static void ConfigureLabelGeometry(float labelX, float effectY)
        {
            int containerWidth = _containerWidget == null ? 274 : Math.Max(1, R.Int(R.Get(_containerWidget, "width")));
            float safeRight = containerWidth * 0.5f - 16f;
            int width = Math.Max(48, Mathf.FloorToInt(safeRight - labelX));
            TrySet(_effectLabel, "width", width);
            _effectLabelObject.transform.localPosition = new Vector3(
                labelX,
                effectY,
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

        private static object GetComponent(Transform transform, string typeName)
        {
            if (transform == null) return null;
            Type type = R.AnyType(typeName);
            return type == null ? null : transform.gameObject.GetComponent(type);
        }

        private static object FindFirstWidget(Transform root, Type uiWidgetType)
        {
            if (root == null || uiWidgetType == null) return null;

            object direct = root.gameObject.GetComponent(uiWidgetType);
            if (direct != null) return direct;

            Component[] children = root.gameObject.GetComponentsInChildren(uiWidgetType, true);
            return children != null && children.Length > 0 ? children[0] : null;
        }

        private static bool TryGetVerticalBounds(
            object widget,
            Transform frame,
            out float minY,
            out float maxY)
        {
            minY = 0f;
            maxY = 0f;
            if (widget == null || frame == null) return false;

            Vector3[] corners = R.Get(widget, "worldCorners") as Vector3[];
            if (corners == null || corners.Length == 0) return false;

            minY = float.PositiveInfinity;
            maxY = float.NegativeInfinity;
            foreach (Vector3 corner in corners)
            {
                float y = frame.InverseTransformPoint(corner).y;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }

            return !float.IsInfinity(minY) && !float.IsInfinity(maxY);
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
