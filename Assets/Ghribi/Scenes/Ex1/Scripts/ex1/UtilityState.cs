using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("NPC")]
public class UtilityState : Action
{
    public enum NPCState { Heal, Rest, Attack }

    public NPCState state;
    public SharedFloat health;
    public SharedFloat stamina;
    public SharedFloat enemyNumber;

    public override float GetUtility()
    {
        float h = Mathf.Clamp01(health.Value / 100f);
        float s = Mathf.Clamp01(stamina.Value / 100f);
        float enemies = Mathf.Clamp01(enemyNumber.Value / 5f);

        switch (state)
        {
            case NPCState.Heal:
                return 1f - h;

            case NPCState.Rest:
                return h * (1f - s);

            case NPCState.Attack:
                return h * s * enemies;
        }

        return 0f;
    }

    public override void OnStart()
    {
        Debug.Log($"NPC state: {state}");
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Running;
    }
}