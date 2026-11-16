using LittleDriver.Driving;
using LittleDriver.Game;
using LittleDriver.UI;
using UnityEngine;

namespace LittleDriver.Audio
{
    /// <summary>
    /// Spoken guidance that replaces the old on-screen hints: says what to do whenever the situation changes,
    /// and repeats the "please do this" hints while the child hasn't done it yet.
    /// </summary>
    public sealed class GuidancePrompts : MonoBehaviour
    {
        private enum Guidance { None, TapDriver, BuckleUp, Drive, OutOfStars, Finished }

        [SerializeField] private Lvl1Car car;
        [SerializeField] private StarScore score;
        [SerializeField] private Lvl1HUD hud;
        [SerializeField] private VoiceLines voice;
        [Tooltip("Seconds between repeats of a hint the child still has to follow (0 = never repeat).")]
        [SerializeField, Min(0f)] private float repeatHintSeconds = 10f;

        [Header("Voice lines")]
        [Tooltip("e.g. \"Tap your driver to put on the seat belt!\"")]
        [SerializeField] private AudioClip tapDriverVoice;
        [Tooltip("e.g. \"Buckle up before driving.\"")]
        [SerializeField] private AudioClip buckleUpVoice;
        [Tooltip("e.g. \"Hold GO to drive, turn the wheel to steer, STOP to brake.\"")]
        [SerializeField] private AudioClip driveVoice;
        [Tooltip("e.g. \"No stars left, press RESTART to try again.\"")]
        [SerializeField] private AudioClip outOfStarsVoice;
        [Tooltip("e.g. \"Course complete!\"")]
        [SerializeField] private AudioClip finishedVoice;

        private Guidance current;
        private float repeatAt;

        private void Update()
        {
            Guidance now = CurrentGuidance();
            if (now == current)
            {
                if (IsReminder(now) && repeatHintSeconds > 0f && Time.unscaledTime >= repeatAt) Say(now);
                return;
            }
            current = now;
            Say(now);
        }

        private Guidance CurrentGuidance()
        {
            if (car.IsFinished) return Guidance.Finished;
            if (!car.IsBuckled) return hud.IsSeatBeltOpen ? Guidance.BuckleUp : Guidance.TapDriver;
            if (score.IsOutOfStars) return Guidance.OutOfStars;
            return Guidance.Drive;
        }

        private static bool IsReminder(Guidance guidance) =>
            guidance == Guidance.TapDriver || guidance == Guidance.BuckleUp || guidance == Guidance.OutOfStars;

        private void Say(Guidance guidance)
        {
            repeatAt = Time.unscaledTime + repeatHintSeconds;
            switch (guidance)
            {
                case Guidance.TapDriver: voice.Say(tapDriverVoice); break;
                case Guidance.BuckleUp: voice.Say(buckleUpVoice); break;
                case Guidance.Drive: voice.Say(driveVoice); break;
                case Guidance.OutOfStars: voice.Say(outOfStarsVoice); break;
                case Guidance.Finished: voice.Say(finishedVoice); break;
            }
        }
    }
}
