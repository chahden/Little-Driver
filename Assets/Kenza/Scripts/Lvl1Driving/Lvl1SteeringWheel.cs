using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LittleDriver.Kenza
{
	// Kenza's copy of EasyVehicleSteering.SteeringWheel.
	// Changes: no InputHandler calls (the car reads steerValue directly; InputHandler uses the
	// old input system, which this project has turned off), and the wheel resets when hidden.
	public class Lvl1SteeringWheel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{
		public Image targetImage;
		public float rotationSpeed = 0.5f;
		public float returnSpeed = 5f;
		public float maxAngle = 45f;
		public bool invertRotation = false;
		public float steerValue;
		private bool isTouched = false;
		private float lastAngle = 0f;
		private float currentRotation = 0f;
		private Quaternion originalRotation;

		void Start()
		{
			if (targetImage == null)
				targetImage = GetComponent<Image>();
			originalRotation = targetImage.rectTransform.rotation;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			isTouched = true;
			lastAngle = GetTouchAngle(eventData);
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (!isTouched || targetImage == null) return;
			float currentAngle = GetTouchAngle(eventData);
			float angleDelta = Mathf.DeltaAngle(lastAngle, currentAngle);
			lastAngle = currentAngle;
			if (invertRotation) angleDelta = -angleDelta;
			currentRotation += angleDelta * rotationSpeed;
			currentRotation = Mathf.Clamp(currentRotation, -maxAngle, maxAngle);
			targetImage.rectTransform.localRotation = Quaternion.Euler(0, 0, -currentRotation);
			steerValue = Mathf.InverseLerp(-maxAngle, maxAngle, currentRotation) * 2f - 1f;
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			isTouched = false;
		}

		void Update()
		{
			if (!isTouched && targetImage != null)
			{
				currentRotation = Mathf.Lerp(currentRotation, 0f, Time.deltaTime * returnSpeed);
				targetImage.rectTransform.localRotation = Quaternion.Euler(0, 0, -currentRotation);
				steerValue = Mathf.InverseLerp(-maxAngle, maxAngle, currentRotation) * 2f - 1f;
			}
		}

		// The HUD hides the driving controls when the course ends. Without this reset the wheel
		// could stay "stuck" turned after a restart, because the finger-up event never arrives.
		void OnDisable()
		{
			isTouched = false;
			currentRotation = 0f;
			steerValue = 0f;
			if (targetImage != null) targetImage.rectTransform.localRotation = Quaternion.identity;
		}

		float GetTouchAngle(PointerEventData eventData)
		{
			RectTransform rect = targetImage.rectTransform;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out Vector2 localPos);
			return Mathf.Atan2(localPos.y, localPos.x) * Mathf.Rad2Deg;
		}
	}
}
