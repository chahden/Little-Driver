using UnityEngine;
using UnityEngine.UI;

namespace EasyVehicleSteering
{
	public class TEST : MonoBehaviour
	{
		[Header("Slider Reference")]
		[SerializeField] private Slider steeringSlider;
        
		[Header("Slider Settings")]
		[SerializeField] private float sensitivity = 1f;
		[SerializeField] private float returnSpeed = 2f;
        
		private void Start()
		{
			// Ensure slider is properly configured for left-right movement
			if (steeringSlider != null)
			{
				steeringSlider.minValue = -1f;
				steeringSlider.maxValue = 1f;
				steeringSlider.value = 0f; // Start from center
			}
		}
        
		private void Update()
		{
			// Get input from InputHandler
			float horizontalInput = InputHandler.Horizontal;
            
			// Update slider position based on input
			if (steeringSlider != null)
			{
				if (Mathf.Abs(horizontalInput) > 0.1f)
				{
					// Move slider based on input with sensitivity
					float targetValue = horizontalInput * sensitivity;
					steeringSlider.value = Mathf.Lerp(steeringSlider.value, targetValue, Time.deltaTime * returnSpeed);
				}
				else
				{
					// Return to center when no input
					steeringSlider.value = Mathf.Lerp(steeringSlider.value, 0f, Time.deltaTime * returnSpeed);
				}
			}
		}
        
		// Method to manually set slider value (optional)
		public void SetSliderValue(float value)
		{
			if (steeringSlider != null)
			{
				steeringSlider.value = Mathf.Clamp(value, -1f, 1f);
			}
		}
        
		// Method to reset slider to center
		public void ResetSlider()
		{
			if (steeringSlider != null)
			{
				steeringSlider.value = 0f;
			}
		}
	}
}