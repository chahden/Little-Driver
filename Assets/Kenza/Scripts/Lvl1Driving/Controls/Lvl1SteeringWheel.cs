using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LittleDriver.Driving
{
    /// <summary>On-screen steering wheel: drag it round to steer, it springs back when released.</summary>
    public sealed class Lvl1SteeringWheel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] private Image targetImage;
        [SerializeField] private float rotationSpeed = 0.5f;
        [SerializeField] private float returnSpeed = 5f;
        [SerializeField] private float maxAngle = 45f;
        [SerializeField] private bool invertRotation;

        private bool isTouched;
        private float lastAngle;
        private float currentRotation;

        /// <summary>-1 = full left, +1 = full right.</summary>
        public float SteerValue => Mathf.InverseLerp(-maxAngle, maxAngle, currentRotation) * 2f - 1f;

        private void Awake()
        {
            if (targetImage == null) targetImage = GetComponent<Image>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isTouched = true;
            lastAngle = GetTouchAngle(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isTouched) return;
            float currentAngle = GetTouchAngle(eventData);
            float angleDelta = Mathf.DeltaAngle(lastAngle, currentAngle);
            lastAngle = currentAngle;
            if (invertRotation) angleDelta = -angleDelta;
            currentRotation = Mathf.Clamp(currentRotation + angleDelta * rotationSpeed, -maxAngle, maxAngle);
            ApplyRotation();
        }

        public void OnPointerUp(PointerEventData eventData) => isTouched = false;

        private void Update()
        {
            if (isTouched) return;
            currentRotation = Mathf.Lerp(currentRotation, 0f, Time.deltaTime * returnSpeed);
            ApplyRotation();
        }

        // The controls are hidden when the run ends; without this the wheel could stay turned,
        // because the finger-up event never arrives.
        private void OnDisable()
        {
            isTouched = false;
            currentRotation = 0f;
            ApplyRotation();
        }

        private void ApplyRotation() => targetImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -currentRotation);

        private float GetTouchAngle(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(targetImage.rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPos);
            return Mathf.Atan2(localPos.y, localPos.x) * Mathf.Rad2Deg;
        }
    }
}
