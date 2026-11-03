using UnityEngine;

namespace EasyVehicleSteering
{
	public static class InputHandler
	{
		private static bool useSimulated = false;
		private static float simulatedHorizontal = 0f;

		public static void SetSimulatedHorizontal(float value)
		{
			simulatedHorizontal = Mathf.Clamp(value, -1f, 1f);
			useSimulated = true;
		}

		public static void ClearSimulated()
		{
			simulatedHorizontal = 0f;
			useSimulated = false;
		}

		public static float Horizontal
		{
			get
			{
#if UNITY_EDITOR || UNITY_STANDALONE
				if (!useSimulated)
					return Input.GetAxisRaw("Horizontal");
				else
					return simulatedHorizontal;
#else
				if (useSimulated)
				return simulatedHorizontal;
				else
				return Input.GetAxisRaw("Horizontal");
#endif
			}
		}
	}
}
