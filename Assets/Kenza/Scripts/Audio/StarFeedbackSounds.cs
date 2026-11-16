using LittleDriver.Game;
using UnityEngine;

namespace LittleDriver.Audio
{
    /// <summary>
    /// Sounds for the star system: a sound effect for gaining or losing a star, plus a voice line
    /// explaining why (replaces the old on-screen messages). Empty slots stay silent.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class StarFeedbackSounds : MonoBehaviour
    {
        [SerializeField] private StarScore score;
        [SerializeField] private VoiceLines voice;

        [Header("Sound effects")]
        [SerializeField] private AudioClip starGained;
        [SerializeField] private AudioClip starLost;

        [Header("Voice lines: star lost")]
        [Tooltip("e.g. \"Careful, look before moving forward!\"")]
        [SerializeField] private AudioClip bumpVoice;
        [Tooltip("e.g. \"Red light! Stop and wait for green.\"")]
        [SerializeField] private AudioClip redLightVoice;
        [Tooltip("e.g. \"Stop! Let people cross first.\"")]
        [SerializeField] private AudioClip busyCrossingVoice;

        [Header("Voice lines: star earned")]
        [Tooltip("e.g. \"Well done, you let the people cross!\"")]
        [SerializeField] private AudioClip waitedAtCrossingVoice;
        [Tooltip("e.g. \"Well done, you waited for them!\"")]
        [SerializeField] private AudioClip waitedForCrosserVoice;
        [Tooltip("e.g. \"Well done, you stopped at the red light and went on green!\"")]
        [SerializeField] private AudioClip stoppedAtRedLightVoice;
        [Tooltip("Played for any star earned whose own voice line above is empty, e.g. \"Bravo, you earned a star!\"")]
        [SerializeField] private AudioClip starEarnedVoice;

        private AudioSource effects;

        private void Awake()
        {
            effects = GetComponent<AudioSource>();
            effects.playOnAwake = false;
        }

        private void OnEnable()
        {
            score.Penalized += OnPenalized;
            score.Rewarded += OnRewarded;
        }

        private void OnDisable()
        {
            score.Penalized -= OnPenalized;
            score.Rewarded -= OnRewarded;
        }

        private void OnPenalized(StarReason reason)
        {
            Play(starLost);
            voice.Say(VoiceFor(reason));
        }

        private void OnRewarded(StarReason reason)
        {
            Play(starGained);
            AudioClip line = VoiceFor(reason);
            voice.Say(line != null ? line : starEarnedVoice);
        }

        private AudioClip VoiceFor(StarReason reason)
        {
            switch (reason)
            {
                case StarReason.Bump: return bumpVoice;
                case StarReason.RedLight: return redLightVoice;
                case StarReason.BusyCrossing: return busyCrossingVoice;
                case StarReason.WaitedAtCrossing: return waitedAtCrossingVoice;
                case StarReason.WaitedForCrosser: return waitedForCrosserVoice;
                case StarReason.StoppedAtRedLight: return stoppedAtRedLightVoice;
                default: return null;
            }
        }

        private void Play(AudioClip clip)
        {
            if (clip != null) effects.PlayOneShot(clip);
        }
    }
}
