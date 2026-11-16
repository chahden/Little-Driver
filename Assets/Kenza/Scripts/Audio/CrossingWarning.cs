using LittleDriver.AI.Pedestrians;
using UnityEngine;

namespace LittleDriver.Audio
{
    /// <summary>Speaks a crosser's warning (child, cat, dog) when it goes onto the road.</summary>
    public sealed class CrossingWarning : MonoBehaviour
    {
        [SerializeField] private RoadCrosser[] crossers = new RoadCrosser[0];
        [SerializeField] private VoiceLines voice;

        private void OnEnable()
        {
            foreach (RoadCrosser crosser in crossers) crosser.CrossingStarted += Warn;
        }

        private void OnDisable()
        {
            foreach (RoadCrosser crosser in crossers) crosser.CrossingStarted -= Warn;
        }

        private void Warn(RoadCrosser crosser) => voice.Say(crosser.WarningVoice);
    }
}
