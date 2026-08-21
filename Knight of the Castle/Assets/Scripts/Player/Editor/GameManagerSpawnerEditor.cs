using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameManagerSpawner))]
public class GameManagerSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw standard inspector fields
        DrawDefaultInspector();

        GameManagerSpawner spawner = (GameManagerSpawner)target;

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("Testing & Debugging Tools", EditorStyles.boldLabel);

        // --- PLAY MODE BUTTONS ---
        EditorGUI.BeginDisabledGroup(!Application.isPlaying);

        if (GUILayout.Button("Test Kill Active Player", GUILayout.Height(30)))
        {
            spawner.TestKillPlayer();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Force Spawn / Respawn"))
        {
            spawner.SpawnPlayer();
        }

        if (GUILayout.Button("Cancel Respawn UI"))
        {
            spawner.TestCancelRespawn();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUI.EndDisabledGroup();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to test player spawn and death triggers.", MessageType.Info);
        }
    }

    // ==========================================
    // RIGHT-CLICK CONTEXT MENU COMMANDS
    // ==========================================
    [MenuItem("CONTEXT/GameManagerSpawner/Test Kill Player")]
    private static void ContextKillPlayer(MenuCommand command)
    {
        GameManagerSpawner spawner = (GameManagerSpawner)command.context;
        if (Application.isPlaying)
        {
            spawner.TestKillPlayer();
        }
        else
        {
            Debug.LogWarning("Enter Play Mode first to test player death!");
        }
    }

    [MenuItem("CONTEXT/GameManagerSpawner/Test Force Spawn")]
    private static void ContextForceSpawn(MenuCommand command)
    {
        GameManagerSpawner spawner = (GameManagerSpawner)command.context;
        if (Application.isPlaying)
        {
            spawner.SpawnPlayer();
        }
        else
        {
            Debug.LogWarning("Enter Play Mode first to test spawning!");
        }
    }
}