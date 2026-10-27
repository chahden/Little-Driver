using UnityEngine;
using BehaviorDesigner.Runtime;

[DefaultExecutionOrder(-100)]
public class NPCGameManager : MonoBehaviour
{
    public BehaviorTree npcTree;

    [Range(0f, 100f)] public float health = 100f;
    [Range(0f, 100f)] public float stamina = 100f;
    [Range(0f, 10f)] public float enemyNumber = 1f;

    private void Awake()
    {
        Update();
    }

    private void Update()
    {
        GlobalVariables.Instance.SetVariableValue("Health", health);
        GlobalVariables.Instance.SetVariableValue("Stamina", stamina);
        GlobalVariables.Instance.SetVariableValue("EnemyNumber", enemyNumber);
    }
}
