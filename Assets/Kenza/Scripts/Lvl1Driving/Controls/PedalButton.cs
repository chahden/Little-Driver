using UnityEngine;
using UnityEngine.EventSystems;

namespace LittleDriver.Driving
{
    /// <summary>On-screen GO or STOP pedal: held while a finger is on it.</summary>
    public sealed class PedalButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public enum Pedal { Accelerate, Brake }

        [SerializeField] private Lvl1CarInput input;
        [SerializeField] private Pedal pedal;

        private int? pointerId;

        public void OnPointerDown(PointerEventData data)
        {
            if (pointerId.HasValue) return;
            pointerId = data.pointerId;
            SetHeld(true);
        }

        public void OnPointerUp(PointerEventData data)
        {
            if (pointerId != data.pointerId) return;
            pointerId = null;
            SetHeld(false);
        }

        private void OnDisable()
        {
            pointerId = null;
            SetHeld(false);
        }

        private void SetHeld(bool held)
        {
            if (pedal == Pedal.Brake) input.SetBrakeHeld(held);
            else input.SetAccelerateHeld(held);
        }
    }
}
