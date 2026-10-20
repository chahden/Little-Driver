using UnityEngine;
using UnityEngine.EventSystems;

namespace LittleDriver.Prototype
{
    public sealed class SprintHoldControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private SprintPrototypeCar car;
        [SerializeField] private bool isBrake;
        [SerializeField] private bool isHorn;
        private int? pointerId;
        private UnityEngine.UI.Button button;

        private void Awake() => button = GetComponent<UnityEngine.UI.Button>();

        private void Update()
        {
            if (isHorn && button != null)
                button.interactable = car != null && car.IsBuckled && !car.IsFinished && !car.IsRecovering && !car.IsHornBlocked;
        }

        public void OnPointerDown(PointerEventData data)
        {
            if (pointerId.HasValue || (isHorn && (car == null || !car.CanHorn))) return;
            pointerId = data.pointerId;
            SetHeld(true);
        }

        public void OnPointerUp(PointerEventData data)
        {
            if (pointerId != data.pointerId) return;
            pointerId = null;
            SetHeld(false);
        }

        private void SetHeld(bool held)
        {
            if (car == null) return;
            if (isHorn) car.SetHornHeld(held);
            else if (isBrake) car.SetBrakeHeld(held);
            else car.SetGoHeld(held);
        }

        private void OnDisable()
        {
            pointerId = null;
            SetHeld(false);
        }
    }
}
