using UnityEngine;

namespace LittleDriver.Audio
{
    /// <summary>Plays one spoken line at a time to the driver; a new line replaces the one playing.</summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class VoiceLines : MonoBehaviour
    {
        private AudioSource source;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
        }

        /// <summary>Does nothing when no clip is assigned yet.</summary>
        public void Say(AudioClip clip)
        {
            if (clip == null) return;
            source.Stop();
            source.clip = clip;
            source.Play();
        }
    }
}
