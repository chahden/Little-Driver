using UnityEngine;

namespace LittleDriver.Prototype
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class TabletSafeArea : MonoBehaviour
    {
        private Rect lastArea;
        private Vector2Int lastSize;

        private void OnEnable() => Apply();

        private void Update()
        {
            if (Screen.safeArea != lastArea || Screen.width != lastSize.x || Screen.height != lastSize.y)
                Apply();
        }

        private void Apply()
        {
            if (Screen.width <= 0 || Screen.height <= 0) return;
            lastArea = Screen.safeArea;
            lastSize = new Vector2Int(Screen.width, Screen.height);
            var rect = (RectTransform)transform;
            // Editor previews can report a safe area larger than their render resolution.
            rect.anchorMin = new Vector2(Mathf.Clamp01(lastArea.xMin / Screen.width), Mathf.Clamp01(lastArea.yMin / Screen.height));
            rect.anchorMax = new Vector2(Mathf.Clamp01(lastArea.xMax / Screen.width), Mathf.Clamp01(lastArea.yMax / Screen.height));
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
