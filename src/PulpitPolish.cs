using System;
using System.Collections;
using UnityEngine;

namespace PrayerClarity
{
    // Small final presentation pass for the live-tuned pulpit prototype.
    // It runs only on pulpit redraw/config changes, after PulpitLayoutV4 has applied
    // the real window/frame geometry.
    internal static class PulpitPolish
    {
        private static object _template;
        private static object _gui;
        private static PrayerForecast.Result _forecast;

        private static Transform _window;
        private static Transform _container;
        private static object _containerWidget;
        private static Transform _craftButton;
        private static Vector3 _craftButtonOriginalPosition;
        private static bool _craftButtonCaptured;

        private static object _effectLabel;
        private static GameObject _effectLabelObject;
        private static GameObject _effectIconObject;

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

                _effectLabel = null;
                _effectLabelObject = null;
                _effectIconObject = null;
            }

            Transform root = _container == null ? null : _container.Find("PrayerClarity.PulpitForecast");
            if (root == null) return;

            Transform effect = root.Find("PrayerClarity.Effect");
            if (effect != null)
            {
                _effectLabelObject = effect.gameObject;
                _effectLabel = effect.gameObject.GetComponent(_template.GetType());
            }

            Transform icon = root.Find("PrayerClarity.Effect.Icon");
            _effectIconObject = icon == null ? null : icon.gameObject;
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
            string specialText = null;
            bool overrideText = false;

            if (craftId.StartsWith("pray:b_village:", StringComparison.Ordinal))
            {
                int count = CountOutput(craft, "blessing_commerce");
                string description = R.VanillaLocalize("blessing_commerce_d");
                bool hasDescription = !string.IsNullOrEmpty(description) &&
                                      !string.Equals(description, "blessing_commerce_d", StringComparison.Ordinal);
                specialText = (count > 0 ? "×" + count : string.Empty) +
                              (hasDescription ? (count > 0 ? " — " : string.Empty) + description : string.Empty);
                if (string.IsNullOrEmpty(specialText)) specialText = "—";
                overrideText = true;
            }
            else if (craftId.StartsWith("pray:b_sin_shard:", StringComparison.Ordinal))
            {
                float duration = R.Float(R.Get(craft, "dur_parameter"));
                specialText = Localization.F("buff.sin_shard", duration);
                overrideText = true;
            }
            else if (string.IsNullOrEmpty(_forecast.SpecialText))
            {
                // Resource-only prayers still get an explicit row so the common early-
                // game layout does not look like it accidentally reserved empty space.
                specialText = "—";
                overrideText = true;
                if (_effectIconObject != null) _effectIconObject.SetActive(false);
            }

            if (overrideText)
            {
                _effectLabelObject.SetActive(true);
                R.Set(_effectLabel, "text", Localization.F("forecast.effect_header") + ": " + specialText);
            }

            // Give long/localized effect text a larger right safety inset than 0.1.10.
            // The label remains ResizeHeight, so languages wrap vertically instead of
            // colliding with the sliced frame.
            int containerWidth = _containerWidget == null ? 274 : Math.Max(1, R.Int(R.Get(_containerWidget, "width")));
            float safeRight = containerWidth * 0.5f - 16f;
            float labelX = _effectLabelObject.transform.localPosition.x;
            int width = Math.Max(48, Mathf.FloorToInt(safeRight - labelX));
            try { R.Set(_effectLabel, "width", width); }
            catch { }
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
    }
}
