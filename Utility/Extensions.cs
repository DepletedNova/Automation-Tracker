using Kitchen;
using KitchenData;
using KitchenLib.Utils;
using TMPro;
using UnityEngine;

namespace KitchenTracker.Utility
{
    public static class Extensions
    {
        public static GameObject CreateLabel(this Transform parent, string name, Vector3 position, Quaternion rotation, Material material, TMP_FontAsset font, float spacing, float size, string text)
        {
            var label = new GameObject(name, new[] { typeof(RectTransform), typeof(MeshRenderer), typeof(TextMeshPro) });

            var labelTransform = label.TryAddComponent<RectTransform>();
            labelTransform.SetParent(parent, false);
            labelTransform.localPosition = position;
            labelTransform.localRotation = rotation;
            label.layer = parent.gameObject.layer;

            label.TryAddComponent<MeshRenderer>().material = material;

            var TMP = label.TryAddComponent<TextMeshPro>();
            TMP.font = font;
            TMP.lineSpacing = spacing;
            TMP.fontSize = size;
            TMP.alignment = TextAlignmentOptions.Center;
            TMP.text = text;

            return label;
        }
    }
}
