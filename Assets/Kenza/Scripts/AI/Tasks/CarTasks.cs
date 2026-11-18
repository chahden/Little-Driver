using BehaviorDesigner.Runtime.Tasks;
using LittleDriver.AI.Traffic;

namespace LittleDriver.AI.Tasks
{
    // Conditions

    [TaskCategory("Little Driver/AI Car")]
    [TaskDescription("Is the light at the next stop line red, or yellow with room to stop comfortably?")]
    public sealed class LightSaysStop : NpcCondition<AITrafficCar>
    {
        protected override bool Check() => Body.LightSaysStop();
    }

    [TaskCategory("Little Driver/AI Car")]
    [TaskDescription("Is someone on the zebra crossing just ahead?")]
    public sealed class CrossingBusy : NpcCondition<AITrafficCar>
    {
        protected override bool Check() => Body.CrossingBusy();
    }

    [TaskCategory("Little Driver/AI Car")]
    [TaskDescription("Is there a car or obstacle in the lane ahead?")]
    public sealed class ObstacleAhead : NpcCondition<AITrafficCar>
    {
        protected override bool Check() => Body.ObstacleAhead();
    }

    [TaskCategory("Little Driver/AI Car")]
    [TaskDescription("Is the junction ahead taken by another car?")]
    public sealed class JunctionBusy : NpcCondition<AITrafficCar>
    {
        protected override bool Check() => Body.JunctionBusy();
    }

    // Actions (stay Running; a conditional abort switches branch)

    [TaskCategory("Little Driver/AI Car")]
    [TaskDescription("Brake to stop just before the next stop line.")]
    public sealed class StopAtLine : NpcAction<AITrafficCar>
    {
        protected override TaskStatus Perform()
        {
            Body.StopAtLine();
            return TaskStatus.Running;
        }
    }

    [TaskCategory("Little Driver/AI Car")]
    [TaskDescription("Brake to keep a safe gap to whatever is ahead.")]
    public sealed class BrakeForObstacle : NpcAction<AITrafficCar>
    {
        protected override TaskStatus Perform()
        {
            Body.BrakeForObstacle();
            return TaskStatus.Running;
        }
    }

    [TaskCategory("Little Driver/AI Car")]
    [TaskDescription("Drive along the lane waypoints at the allowed speed, reserving the junction ahead.")]
    public sealed class FollowLane : NpcAction<AITrafficCar>
    {
        protected override TaskStatus Perform()
        {
            Body.FollowLane();
            return TaskStatus.Running;
        }
    }
}
