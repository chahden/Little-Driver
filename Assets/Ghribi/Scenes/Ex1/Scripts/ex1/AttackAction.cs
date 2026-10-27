using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("NPC/Ex1")]
public class AttackAction : Action
{
    public SharedFloat health = new SharedFloat { Name = "Health", IsShared = true, IsGlobal = true };
    public SharedFloat stamina = new SharedFloat { Name = "Stamina", IsShared = true, IsGlobal = true };
    public SharedFloat enemyNumber = new SharedFloat { Name = "EnemyNumber", IsShared = true, IsGlobal = true };

    public override float GetUtility()
    {
        return Evaluate(health.Value, stamina.Value, enemyNumber.Value);
    }

    public static float Evaluate(float health, float stamina, float enemyNumber)
    {
        return Mathf.Clamp01(health / 100f) * Mathf.Clamp01(stamina / 100f) * Mathf.Clamp01(enemyNumber / 5f);
    }

    public override void OnStart()
    {
        Debug.Log("NPC state: Attack");
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Running;
    }
}

