using UnityEngine;

public class ArcherTower : DefenseBuilding
{
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private LayerMask enemyLayer;

    private float attackTimer;

    private void Update()
    {
        if (data == null) return;

        attackTimer += Time.deltaTime;
        if (attackTimer >= data.attackRate)
        {
            Transform target = GetNearestEnemy();
            if (target != null)
            {
                Shoot(target);
                attackTimer = 0f;
            }
        }
    }

    private Transform GetNearestEnemy()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, data.attackRange, enemyLayer);
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = enemy.transform;
            }
        }
        return nearest;
    }

    private void Shoot(Transform target)
    {
        if (arrowPrefab != null && firePoint != null)
        {
            GameObject projectile = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);
        }
    }
}