using UnityEngine;

namespace LittleDriver.UI
{
    /// <summary>Keeps this RectTransform inside the device safe area (notches, rounded corners).</summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform rect;
        private Rect applied;
        private Vector2Int appliedScreen;

        private void Awake() => rect = (RectTransform)transform;

        private void Update()
        {
            var screen = new Vector2Int(Screen.width, Screen.height);
            if (Screen.safeArea == applied && screen == appliedScreen) return;
            applied = Screen.safeArea;
            appliedScreen = screen;
            rect.anchorMin = new Vector2(applied.xMin / screen.x, applied.yMin / screen.y);
            rect.anchorMax = new Vector2(applied.xMax / screen.x, applied.yMax / screen.y);
        }
    }
}
