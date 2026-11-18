using System;
using LittleDriver.AI.Tasks;
using UnityEngine;

namespace LittleDriver.Traffic
{
    public enum TrafficLightState { Red, Yellow, Green }

    /// <summary>
    /// Traffic light. Its finite state machine (Green -> Yellow -> Red -> Green) is the Behavior Designer
    /// tree "Traffic Light FSM": each phase node is one state, and the sequence order is the transitions.
    /// This component holds the timings and the current state; vehicles read <see cref="CurrentState"/>
    /// and views listen to <see cref="StateChanged"/>.
    /// Lights at the same junction stay in sync by sharing the same cycle length
    /// (green + yellow + red) and using <see cref="startState"/> / <see cref="startOffset"/>.
    /// </summary>
    public sealed class TrafficLight : MonoBehaviour, INpcBody
    {
        [SerializeField, Min(0.5f)] private float greenSeconds = 8f;
        [SerializeField, Min(0.5f)] private float yellowSeconds = 2f;
        [SerializeField, Min(0.5f)] private float redSeconds = 12f;
        [SerializeField] private TrafficLightState startState = TrafficLightState.Green;
        [Tooltip("Seconds already spent in the start state when the scene begins.")]
        [SerializeField, Min(0f)] private float startOffset;
        [Tooltip("Read-only: the phase the behaviour tree is running.")]
        [SerializeField] private string currentAction;

        private bool started;
        private float phaseElapsed;
        private float carriedTime;

        public TrafficLightState CurrentState { get; private set; }
        public event Action<TrafficLightState> StateChanged;

        string INpcBody.CurrentAction { set => currentAction = value; }

        public float DurationOf(TrafficLightState state) =>
            state == TrafficLightState.Green ? greenSeconds : state == TrafficLightState.Yellow ? yellowSeconds : redSeconds;

        /// <summary>
        /// Called by a phase node when the tree reaches it. Returns false (skip this phase) until the
        /// cycle has reached the light's start state, so each light starts where it was set up to.
        /// </summary>
        public bool BeginPhase(TrafficLightState state)
        {
            if (!started)
            {
                if (state != startState) return false;
                started = true;
                phaseElapsed = Mathf.Min(startOffset, DurationOf(state));
                SetState(state, true);
                return true;
            }
            // Time left over from the previous phase is carried in, so linked lights never drift apart.
            phaseElapsed = carriedTime;
            carriedTime = 0f;
            SetState(state, false);
            return true;
        }

        /// <summary>Advances the running phase; true when it is over.</summary>
        public bool TickPhase(float deltaTime)
        {
            phaseElapsed += deltaTime;
            float duration = DurationOf(CurrentState);
            if (phaseElapsed < duration) return false;
            carriedTime = phaseElapsed - duration;
            return true;
        }

        private void SetState(TrafficLightState next, bool first)
        {
            TrafficLightState previous = CurrentState;
            CurrentState = next;
            Debug.Log(first ? $"[TrafficLight] {name} starts {next}" : $"[TrafficLight] {name}: {previous} -> {next}", this);
            StateChanged?.Invoke(next);
        }
    }
}
