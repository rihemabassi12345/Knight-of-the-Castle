using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private EnemyController enemyPrefab;
    [SerializeField] private int poolCapacity = 100; 
    
    [Header("Spawn Settings")]
    [SerializeField] private Transform castleTarget;     
    [SerializeField] private Transform[] spawnPoints;   
    [SerializeField] private float spawnInterval = 2f;  

    private Queue<EnemyController> enemyPool;
    private List<EnemyController> activeEnemies;
    private float spawnTimer;

    private void OnEnable()
    {
        EnemyController.OnEnemyDeath += HandleEnemyDeath;
    }

    private void OnDisable()
    {
        EnemyController.OnEnemyDeath -= HandleEnemyDeath;
    }

    private void Start()
    {
        // Optimization: تحديد السعة مسبقاً لمنع إعادة الحجز في الذاكرة (Memory Allocation)
        enemyPool = new Queue<EnemyController>(poolCapacity);
        activeEnemies = new List<EnemyController>(poolCapacity);

        InitializePool();
    }

    private void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnEnemyFromPool();
        }
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolCapacity; i++)
        {
            EnemyController enemyInstance = Instantiate(enemyPrefab, transform);
            enemyInstance.gameObject.SetActive(false); 
            enemyPool.Enqueue(enemyInstance);          
        }
    }

    public void SpawnEnemyFromPool()
    {
        if (enemyPool.Count > 0 && spawnPoints.Length > 0 && castleTarget != null)
        {
            Transform randomSpawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];

            EnemyController enemy = enemyPool.Dequeue();
            enemy.InitializeEnemy(randomSpawnPoint.position, castleTarget);
            
            activeEnemies.Add(enemy);
        }
    }

    private void HandleEnemyDeath(EnemyController deadEnemy)
    {
        // Optimization: إرجاع سريع بدون تكلفة تفكيك الكائن
        if (activeEnemies.Remove(deadEnemy))
        {
            enemyPool.Enqueue(deadEnemy);
        }
    }
}