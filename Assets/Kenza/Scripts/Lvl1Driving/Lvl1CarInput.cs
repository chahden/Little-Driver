using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LittleDriver.Driving
{
    /// <summary>
    /// Merges the on-screen controls (GO/STOP pedals, steering wheel) with a keyboard fallback for testing in the Editor:
    /// W/Up = go, Space/Down = brake, A/D or arrows = steer, H = horn, R = restart.
    /// </summary>
    public sealed class Lvl1CarInput : MonoBehaviour
    {
        [SerializeField] private Lvl1SteeringWheel steeringWheel;
        [Tooltip("Tiny wheel movements smaller than this are ignored.")]
        [SerializeField, Range(0f, 0.3f)] private float steeringDeadZone = 0.05f;

        private bool accelerateHeld;
        private bool brakeHeld;

        public event Action HornPressed;
        public event Action RestartPressed;

        public bool Accelerate => accelerateHeld || IsKeyHeld(Key.W) || IsKeyHeld(Key.UpArrow);
        public bool Brake => brakeHeld || IsKeyHeld(Key.Space) || IsKeyHeld(Key.DownArrow);

        /// <summary>-1 = full left, +1 = full right.</summary>
        public float Steer
        {
            get
            {
                float steer = steeringWheel != null ? steeringWheel.SteerValue : 0f;
                if (IsKeyHeld(Key.A) || IsKeyHeld(Key.LeftArrow)) steer = -1f;
                if (IsKeyHeld(Key.D) || IsKeyHeld(Key.RightArrow)) steer = 1f;
                return Mathf.Abs(steer) < steeringDeadZone ? 0f : steer;
            }
        }

        public void SetAccelerateHeld(bool held) => accelerateHeld = held;
        public void SetBrakeHeld(bool held) => brakeHeld = held;

        public void ReleaseAll()
        {
            accelerateHeld = false;
            brakeHeld = false;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.hKey.wasPressedThisFrame) HornPressed?.Invoke();
            if (keyboard.rKey.wasPressedThisFrame) RestartPressed?.Invoke();
        }

        private void OnApplicationFocus(bool focused) { if (!focused) ReleaseAll(); }
        private void OnApplicationPause(bool paused) { if (paused) ReleaseAll(); }
        private void OnDisable() => ReleaseAll();

        private static bool IsKeyHeld(Key key) => Keyboard.current != null && Keyboard.current[key].isPressed;
    }
}
