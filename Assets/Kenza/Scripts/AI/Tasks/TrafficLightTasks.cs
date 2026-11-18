using BehaviorDesigner.Runtime.Tasks;
using LittleDriver.Traffic;
using UnityEngine;

namespace LittleDriver.AI.Tasks
{
    [TaskCategory("Little Driver/Traffic Light")]
    [TaskDescription("One state of the traffic light FSM: switches the light to this colour and stays Running for its duration " +
                     "(set on the Traffic Light component), then succeeds so the sequence moves to the next state.")]
    public sealed class TrafficLightPhase : NpcAction<TrafficLight>
    {
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The colour this state shows.")]
        public TrafficLightState state = TrafficLightState.Green;

        private bool skip;

        // Before the light's start state is reached, the earlier phases are skipped at once.
        public override void OnStart() => skip = Body == null || !Body.BeginPhase(state);

        protected override TaskStatus Perform()
        {
            if (skip) return TaskStatus.Success;
            return Body.TickPhase(Time.deltaTime) ? TaskStatus.Success : TaskStatus.Running;
        }
    }
}
