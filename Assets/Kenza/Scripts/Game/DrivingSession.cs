using LittleDriver.Driving;
using UnityEngine;
using UnityEngine.Serialization;

namespace LittleDriver.Game
{
    /// <summary>
    /// Referee of one drive: turns bumps, red lights and ignored zebra crossings into lost stars,
    /// stops the car when the stars run out, and restarts the run.
    /// </summary>
    public sealed class DrivingSession : MonoBehaviour
    {
        [SerializeField] private Lvl1Car car;
        [SerializeField] private Lvl1CarInput input;
        [SerializeField] private CarCollisionHandler collisions;
        [FormerlySerializedAs("redLights")]
        [SerializeField] private StopLineMonitor stopLines;
        [SerializeField] private StarScore score;
        [SerializeField] private string bumpMessage = "Careful, look before moving forward!";
        [SerializeField] private string redLightMessage = "Red light! Stop and wait for green.";
        [SerializeField] private string crossingMessage = "Stop! Let people cross first.";

        private void OnEnable()
        {
            collisions.Bumped += OnBumped;
            stopLines.RanRedLight += OnRanRedLight;
            stopLines.DroveOntoBusyCrossing += OnDroveOntoBusyCrossing;
            score.OutOfStars += OnOutOfStars;
            input.RestartPressed += Restart;
        }

        private void OnDisable()
        {
            collisions.Bumped -= OnBumped;
            stopLines.RanRedLight -= OnRanRedLight;
            stopLines.DroveOntoBusyCrossing -= OnDroveOntoBusyCrossing;
            score.OutOfStars -= OnOutOfStars;
            input.RestartPressed -= Restart;
        }

        public void Restart()
        {
            score.ResetStars();
            car.Restart();
        }

        private void OnBumped(Vector3 point) => score.Penalize(bumpMessage);
        private void OnRanRedLight() => score.Penalize(redLightMessage);
        private void OnDroveOntoBusyCrossing() => score.Penalize(crossingMessage);
        private void OnOutOfStars() => car.SetLocked(true);
    }
}
