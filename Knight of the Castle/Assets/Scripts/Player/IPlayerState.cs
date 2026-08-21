using System;

public interface IKillable
{
    bool IsDead { get; }
    event Action OnDied;
    event Action OnRespawned;
    void Die();
}