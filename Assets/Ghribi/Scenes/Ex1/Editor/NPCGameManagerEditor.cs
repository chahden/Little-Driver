using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NPCGameManager))]
public class NPCGameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var manager = (NPCGameManager)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Live Utility Scores (0–1)", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.FloatField("Heal", HealAction.Evaluate(manager.health));
            EditorGUILayout.FloatField("Rest", RestAction.Evaluate(manager.health, manager.stamina));
            EditorGUILayout.FloatField("Attack", AttackAction.Evaluate(manager.health, manager.stamina, manager.enemyNumber));
        }
    }

    public override bool RequiresConstantRepaint()
    {
        return Application.isPlaying;
    }
}
