using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("NPC/Ex1")]
public class RestAction : Action
{
    public SharedFloat health = new SharedFloat { Name = "Health", IsShared = true, IsGlobal = true };
    public SharedFloat stamina = new SharedFloat { Name = "Stamina", IsShared = true, IsGlobal = true };

    public override float GetUtility()
    {
        return Evaluate(health.Value, stamina.Value);
    }

    public static float Evaluate(float health, float stamina)
    {
        return Mathf.Clamp01(health / 100f) * (1f - Mathf.Clamp01(stamina / 100f));
    }

    public override void OnStart()
    {
        Debug.Log("NPC state: Rest");
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Running;
    }
}

