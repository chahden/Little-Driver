using UnityEngine;

namespace LittleDriver.Driving
{
    /// <summary>Comic puff and soft "boing" when a bump counts.</summary>
    public sealed class CarBumpEffects : MonoBehaviour
    {
        [SerializeField] private CarCollisionHandler collisions;
        [SerializeField] private ParticleSystem puff;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip bumpClip;

        private void OnEnable() => collisions.Bumped += Play;
        private void OnDisable() => collisions.Bumped -= Play;

        private void Play(Vector3 point)
        {
            puff.transform.position = point + Vector3.up * 0.5f;
            puff.Play();
            audioSource.PlayOneShot(bumpClip);
        }
    }
}
