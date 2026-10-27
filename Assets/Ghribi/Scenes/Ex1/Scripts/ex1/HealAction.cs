using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("NPC/Ex1")]
public class HealAction : Action
{
    public SharedFloat health = new SharedFloat { Name = "Health", IsShared = true, IsGlobal = true };

    public override float GetUtility()
    {
        return Evaluate(health.Value);
    }

    public static float Evaluate(float health)
    {
        return 1f - Mathf.Clamp01(health / 100f);
    }

    public override void OnStart()
    {
        Debug.Log("NPC state: Heal");
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Running;
    }
}

