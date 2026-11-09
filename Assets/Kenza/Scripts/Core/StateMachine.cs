using System;
using System.Collections.Generic;

namespace LittleDriver.Core
{
    /// <summary>One state of a <see cref="StateMachine{TId}"/>.</summary>
    public interface IState<TId> where TId : struct, Enum
    {
        TId Id { get; }

        /// <param name="carriedTime">Time already spent in this state (overflow of the previous one), so timed cycles never drift.</param>
        void Enter(float carriedTime);

        /// <summary>Returns true when the machine must switch to <paramref name="next"/>.</summary>
        bool Tick(float deltaTime, out TId next, out float carriedTime);

        void Exit();
    }

    /// <summary>Small reusable finite state machine keyed by an enum.</summary>
    public sealed class StateMachine<TId> where TId : struct, Enum
    {
        private readonly Dictionary<TId, IState<TId>> states = new Dictionary<TId, IState<TId>>();
        private IState<TId> current;

        public TId CurrentId => current != null ? current.Id : default;

        /// <summary>(previous, next). Both are equal for the initial state.</summary>
        public event Action<TId, TId> StateChanged;

        public void Add(IState<TId> state) => states.Add(state.Id, state);

        public void Start(TId id, float timeAlreadyElapsed = 0f)
        {
            current?.Exit();
            current = states[id];
            current.Enter(timeAlreadyElapsed);
            StateChanged?.Invoke(id, id);
        }

        public void Tick(float deltaTime)
        {
            if (current == null || !current.Tick(deltaTime, out TId next, out float carriedTime)) return;
            TId previous = current.Id;
            current.Exit();
            current = states[next];
            current.Enter(carriedTime);
            StateChanged?.Invoke(previous, next);
        }
    }

    /// <summary>A state that lasts a fixed time, then hands over to a fixed next state.</summary>
    public sealed class TimedState<TId> : IState<TId> where TId : struct, Enum
    {
        private readonly float duration;
        private readonly TId next;
        private float elapsed;

        public TimedState(TId id, float duration, TId next)
        {
            Id = id;
            this.duration = duration;
            this.next = next;
        }

        public TId Id { get; }

        public void Enter(float carriedTime) => elapsed = carriedTime;

        public bool Tick(float deltaTime, out TId nextState, out float carriedTime)
        {
            elapsed += deltaTime;
            bool finished = elapsed >= duration;
            nextState = finished ? next : Id;
            carriedTime = finished ? elapsed - duration : 0f;
            return finished;
        }

        public void Exit() { }
    }
}
