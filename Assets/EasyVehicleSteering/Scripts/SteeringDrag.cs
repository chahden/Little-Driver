using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace EasyVehicleSteering
{
	public class SteeringDrag : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{
		public Image targetImage;
		public Image limitAreaImage;
		public float dragSpeed = 1f;
		public float returnSpeed = 5f;
		public bool invertDirection = false;
		public float steerValue;
		private bool isTouched = false;
		private Vector2 lastPos;
		private float currentOffsetX = 0f;
		private Vector3 originalPosition;
		private float minX, maxX;

		void Start()
		{
			if (targetImage == null)
				targetImage = GetComponent<Image>();
			originalPosition = targetImage.rectTransform.localPosition;
			float halfLimitWidth = (limitAreaImage.rectTransform.rect.width - targetImage.rectTransform.rect.width) / 2f;
			minX = -halfLimitWidth;
			maxX = halfLimitWidth;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			isTouched = true;
			lastPos = eventData.position;
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (!isTouched || targetImage == null) return;
			Vector2 delta = eventData.position - lastPos;
			lastPos = eventData.position;
			float direction = invertDirection ? -1f : 1f;
			currentOffsetX += delta.x * direction * dragSpeed;
			currentOffsetX = Mathf.Clamp(currentOffsetX, minX, maxX);
			targetImage.rectTransform.localPosition = originalPosition + new Vector3(currentOffsetX, 0f, 0f);
			steerValue = Mathf.InverseLerp(minX, maxX, currentOffsetX) * 2f - 1f;
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			isTouched = false;
		}

		void Update()
		{
			if (!isTouched && targetImage != null)
			{
				currentOffsetX = Mathf.Lerp(currentOffsetX, 0f, Time.deltaTime * returnSpeed);
				targetImage.rectTransform.localPosition = originalPosition + new Vector3(currentOffsetX, 0f, 0f);
				steerValue = Mathf.InverseLerp(minX, maxX, currentOffsetX) * 2f - 1f;
			}

			if (steerValue != 0)
				InputHandler.SetSimulatedHorizontal(steerValue);
			else
				InputHandler.ClearSimulated();
		}
	}
}
