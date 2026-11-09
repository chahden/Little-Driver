using UnityEngine;

namespace LittleDriver.Driving
{
    /// <summary>Keeps the camera behind the car, turning with it.</summary>
    public sealed class CarFollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [Tooltip("Camera position seen from the car: x = right, y = up, z = forward (negative = behind).")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 4.5f, -8.5f);
        [SerializeField] private float lookAhead = 5f;
        [SerializeField, Min(0.1f)] private float smoothing = 5f;

        private void LateUpdate()
        {
            Vector3 forward = target.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f) return;
            forward.Normalize();
            Vector3 wanted = target.position + Quaternion.LookRotation(forward) * offset;
            transform.position = Vector3.Lerp(transform.position, wanted, 1f - Mathf.Exp(-smoothing * Time.deltaTime));
            transform.LookAt(target.position + forward * lookAhead);
        }
    }
}
