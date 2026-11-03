using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace EasyVehicleSteering
{
	public class SteeringButtons : MonoBehaviour
	{
		public Button leftButton;
		public Button rightButton;
		public float steerValue;

		private bool isLeftHeld;
		private bool isRightHeld;

		void Start()
		{
			AddButtonEvents(leftButton, OnLeftDown, OnLeftUp);
			AddButtonEvents(rightButton, OnRightDown, OnRightUp);
		}

		void Update()
		{
			if (isLeftHeld && !isRightHeld)
				steerValue = -1f;
			else if (isRightHeld && !isLeftHeld)
				steerValue = 1f;
			else
				steerValue = 0f;

			if (steerValue != 0)
				InputHandler.SetSimulatedHorizontal(steerValue);
			else
				InputHandler.ClearSimulated();
		}

		private void AddButtonEvents(Button button, UnityEngine.Events.UnityAction onDown, UnityEngine.Events.UnityAction onUp)
		{
			EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();

			var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
			down.callback.AddListener((e) => onDown());
			trigger.triggers.Add(down);

			var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
			up.callback.AddListener((e) => onUp());
			trigger.triggers.Add(up);
		}

		private void OnLeftDown() => isLeftHeld = true;
		private void OnLeftUp() => isLeftHeld = false;
		private void OnRightDown() => isRightHeld = true;
		private void OnRightUp() => isRightHeld = false;
	}
}
