using System;
using LittleDriver.Core;
using UnityEngine;

namespace LittleDriver.Traffic
{
    public enum TrafficLightState { Red, Yellow, Green }

    /// <summary>
    /// Traffic light driven by a finite state machine: Green -> Yellow -> Red -> Green.
    /// Vehicles read <see cref="CurrentState"/>; views listen to <see cref="StateChanged"/>.
    /// Lights at the same junction stay in sync by sharing the same cycle length
    /// (green + yellow + red) and using <see cref="startState"/> / <see cref="startOffset"/>.
    /// </summary>
    public sealed class TrafficLight : MonoBehaviour
    {
        [SerializeField, Min(0.5f)] private float greenSeconds = 8f;
        [SerializeField, Min(0.5f)] private float yellowSeconds = 2f;
        [SerializeField, Min(0.5f)] private float redSeconds = 12f;
        [SerializeField] private TrafficLightState startState = TrafficLightState.Green;
        [Tooltip("Seconds already spent in the start state when the scene begins.")]
        [SerializeField, Min(0f)] private float startOffset;

        private readonly StateMachine<TrafficLightState> machine = new StateMachine<TrafficLightState>();

        public TrafficLightState CurrentState => machine.CurrentId;
        public event Action<TrafficLightState> StateChanged;

        private void Awake()
        {
            machine.Add(new TimedState<TrafficLightState>(TrafficLightState.Green, greenSeconds, TrafficLightState.Yellow));
            machine.Add(new TimedState<TrafficLightState>(TrafficLightState.Yellow, yellowSeconds, TrafficLightState.Red));
            machine.Add(new TimedState<TrafficLightState>(TrafficLightState.Red, redSeconds, TrafficLightState.Green));
            machine.StateChanged += OnStateChanged;
        }

        private void Start() => machine.Start(startState, startOffset);

        private void Update() => machine.Tick(Time.deltaTime);

        private void OnDestroy() => machine.StateChanged -= OnStateChanged;

        private void OnStateChanged(TrafficLightState previous, TrafficLightState next)
        {
            Debug.Log(previous == next ? $"[TrafficLight] {name} starts {next}" : $"[TrafficLight] {name}: {previous} -> {next}", this);
            StateChanged?.Invoke(next);
        }
    }
}
