using UnityEngine;

/// <summary>
/// Attach this script to your Enemy Spawner GameObject.
/// It provides utility methods to call SetupDetection on spawned enemy prefabs.
/// </summary>
public class EnemySpawnerSetup : MonoBehaviour
{
    [Header("Target Configuration")]
    [Tooltip("Drag your Castle transform here from the Hierarchy.")]
    [SerializeField] private Transform castleTransform;

    [Header("Optional Tag Lookup")]
    [Tooltip("If castleTransform is left unassigned, the script will search for an object with this tag.")]
    [SerializeField] private string castleTag = "Castle";

    private void Awake()
    {
        // Auto-assign castle by Tag if not set in Inspector
        if (castleTransform == null)
        {
            GameObject castleObj = GameObject.FindGameObjectWithTag(castleTag);
            if (castleObj != null)
            {
                castleTransform = castleObj.transform;
            }
            else
            {
                Debug.LogWarning($"[EnemySpawnerSetup] Castle reference is missing and no object found with tag '{castleTag}'.", this);
            }
        }
    }

    /// <summary>
    /// Call this method right after instantiating an enemy to initialize its EnemyDetection script.
    /// </summary>
    /// <param name="enemyObject">The newly instantiated enemy GameObject.</param>
    public void InitializeEnemy(GameObject enemyObject)
    {
        if (enemyObject == null) return;

        if (castleTransform == null)
        {
            Debug.LogError($"[EnemySpawnerSetup] Cannot setup {enemyObject.name} because Castle transform is null!", this);
            return;
        }

        if (enemyObject.TryGetComponent<EnemyDetection>(out EnemyDetection detection))
        {
            detection.SetupDetection(castleTransform);
        }
        else
        {
            Debug.LogWarning($"[EnemySpawnerSetup] {enemyObject.name} does not have an EnemyDetection component attached.", enemyObject);
        }
    }

    /// <summary>
    /// Instantiates an enemy prefab at a given position/rotation and automatically initializes its detection.
    /// </summary>
    public GameObject SpawnAndSetupEnemy(GameObject enemyPrefab, Vector3 position, Quaternion rotation)
    {
        GameObject spawnedEnemy = Instantiate(enemyPrefab, position, rotation);
        InitializeEnemy(spawnedEnemy);
        return spawnedEnemy;
    }
}