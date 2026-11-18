using BehaviorDesigner.Runtime.Tasks;
using LittleDriver.AI.Pedestrians;

namespace LittleDriver.AI.Tasks
{
    // Conditions

    [TaskCategory("Little Driver/Pedestrian")]
    [TaskDescription("Has the pedestrian been given a place to walk to?")]
    public sealed class HasRoute : NpcCondition<Pedestrian>
    {
        protected override bool Check() => Body.HasRoute;
    }

    [TaskCategory("Little Driver/Pedestrian")]
    [TaskDescription("Is the pedestrian on the zebra crossing (on the road)?")]
    public sealed class IsOnRoad : NpcCondition<Pedestrian>
    {
        protected override bool Check() => Body.IsOnRoad;
    }

    [TaskCategory("Little Driver/Pedestrian")]
    [TaskDescription("Is the pedestrian standing at a zebra crossing, wanting to cross?")]
    public sealed class WantsToCross : NpcCondition<Pedestrian>
    {
        protected override bool Check() => Body.WantsToCross;
    }

    [TaskCategory("Little Driver/Pedestrian")]
    [TaskDescription("Is a car on the crossing or too close to stop before it?")]
    public sealed class CarTooClose : NpcCondition<Pedestrian>
    {
        protected override bool Check() => Body.CarTooClose;
    }

    // Actions

    [TaskCategory("Little Driver/Pedestrian")]
    [TaskDescription("Walk along the current route (sidewalk or crossing). Stays Running.")]
    public sealed class WalkRoute : NpcAction<Pedestrian>
    {
        protected override TaskStatus Perform()
        {
            Body.Walk();
            return TaskStatus.Running;
        }
    }

    [TaskCategory("Little Driver/Pedestrian")]
    [TaskDescription("Stand at the kerb facing the road until it is safe. Stays Running.")]
    public sealed class WaitForCars : NpcAction<Pedestrian>
    {
        protected override TaskStatus Perform()
        {
            Body.Wait();
            return TaskStatus.Running;
        }
    }

    [TaskCategory("Little Driver/Pedestrian")]
    [TaskDescription("Step onto the zebra crossing (it now counts as busy for cars).")]
    public sealed class StepOut : NpcAction<Pedestrian>
    {
        protected override TaskStatus Perform()
        {
            Body.StepOut();
            return TaskStatus.Success;
        }
    }
}
