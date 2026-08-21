using System;
using UnityEngine;

public class PlayerController : MonoBehaviour, IKillable
{
    public bool IsDead { get; private set; }

    public event Action OnDied;
    public event Action OnRespawned;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Die()
    {
        if (IsDead) return;

        IsDead = true;
        rb.isKinematic = true;
        OnDied?.Invoke();
    }

    public void ReviveAt(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
        IsDead = false;
        rb.isKinematic = false;
        OnRespawned?.Invoke();
    }
}