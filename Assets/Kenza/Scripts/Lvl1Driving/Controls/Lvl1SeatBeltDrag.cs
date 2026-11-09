using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LittleDriver.Driving
{
    /// <summary>Drag the belt handle onto the buckle to buckle up the car.</summary>
    public sealed class Lvl1SeatBeltDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private static readonly Color BuckledColour = new Color(0.75f, 0.82f, 0.75f);
        private static readonly Color LooseColour = new Color(0.78f, 0.78f, 0.78f);

        [SerializeField] private Lvl1Car car;
        [SerializeField] private RectTransform buckle;
        [SerializeField] private RectTransform strap;
        [SerializeField] private Image handleImage;
        [SerializeField] private float snapDistance = 85f;

        private RectTransform handle;
        private RectTransform panel;
        private Vector2 home;
        private Vector2 dragOffset;
        private int? pointerId;

        private void Awake()
        {
            handle = (RectTransform)transform;
            panel = (RectTransform)transform.parent;
            home = handle.anchoredPosition;
        }

        // A restart can happen while this panel is hidden (reset when it shows again) or still visible (Restarted event).
        private void OnEnable()
        {
            car.Restarted += ResetBelt;
            if (!car.IsBuckled) ResetBelt();
        }

        private void OnDisable()
        {
            car.Restarted -= ResetBelt;
            pointerId = null;
        }

        public void OnBeginDrag(PointerEventData data)
        {
            if (car.IsBuckled || pointerId.HasValue) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(panel, data.pressPosition, data.pressEventCamera, out Vector2 local)) return;
            pointerId = data.pointerId;
            dragOffset = handle.anchoredPosition - local;
        }

        public void OnDrag(PointerEventData data)
        {
            if (pointerId != data.pointerId || car.IsBuckled) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(panel, data.position, data.pressEventCamera, out Vector2 local)) return;
            Vector2 half = handle.rect.size * 0.5f;
            handle.anchoredPosition = new Vector2(
                Mathf.Clamp(local.x + dragOffset.x, panel.rect.xMin + half.x, panel.rect.xMax - half.x),
                Mathf.Clamp(local.y + dragOffset.y, panel.rect.yMin + half.y, panel.rect.yMax - half.y));
            UpdateStrap();
        }

        public void OnEndDrag(PointerEventData data)
        {
            if (pointerId != data.pointerId || car.IsBuckled) return;
            pointerId = null;
            if (Vector2.Distance(handle.anchoredPosition, buckle.anchoredPosition) <= snapDistance)
            {
                handle.anchoredPosition = buckle.anchoredPosition;
                handleImage.color = BuckledColour;
                car.Buckle();
            }
            else handle.anchoredPosition = home;
            UpdateStrap();
        }

        private void ResetBelt()
        {
            pointerId = null;
            handle.anchoredPosition = home;
            handleImage.color = LooseColour;
            UpdateStrap();
        }

        private void UpdateStrap()
        {
            Vector2 delta = handle.anchoredPosition - home;
            strap.anchoredPosition = home + delta * 0.5f;
            strap.sizeDelta = new Vector2(Mathf.Max(25f, delta.magnitude), 24f);
            strap.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        }
    }
}
