using UnityEngine;

namespace LittleDriver.Driving
{
    /// <summary>Plays one short, friendly beep per press. Purely cosmetic: no effect on traffic or score.</summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class CarHorn : MonoBehaviour
    {
        [SerializeField] private AudioClip hornClip;
        [SerializeField, Range(0f, 1f)] private float volume = 0.7f;
        [SerializeField] private Lvl1CarInput input;

        private AudioSource source;

        public bool IsHonking => source.isPlaying;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.clip = hornClip;
        }

        private void OnEnable() { if (input != null) input.HornPressed += Honk; }
        private void OnDisable() { if (input != null) input.HornPressed -= Honk; }

        public void Honk()
        {
            source.volume = volume;
            source.Play();
        }
    }
}
