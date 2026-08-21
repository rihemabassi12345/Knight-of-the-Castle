using System.Collections;
using UnityEngine;

public class GameManagerSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private DeathUIController deathUI;

    [Header("Rules Configuration")]
    [SerializeField] private float respawnWaitTime = 20.0f;

    private PlayerController activePlayer;

    private void Start()
    {
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion spawnRot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        if (activePlayer == null)
        {
            if (playerPrefab == null)
            {
                Debug.LogError("GameManagerSpawner: Player Prefab is not assigned in the Inspector!", this);
                return;
            }

            GameObject playerObj = Instantiate(playerPrefab, spawnPos, spawnRot);
            activePlayer = playerObj.GetComponentInChildren<PlayerController>();
            if (activePlayer == null)
            {
                Debug.LogError("GameManagerSpawner: PlayerPrefab does not have a PlayerController component attached!", this);
                return;
            }
        }
        else
        {
            // Unbind event before reviving to prevent duplicate listener registrations
            activePlayer.OnDied -= HandlePlayerDeath;
            activePlayer.ReviveAt(spawnPos, spawnRot);
        }

        // Bind death event listener
        activePlayer.OnDied += HandlePlayerDeath;
    }

    private void HandlePlayerDeath()
    {
        if (activePlayer != null)
        {
            activePlayer.OnDied -= HandlePlayerDeath; // Unbind to avoid double triggering
        }

        if (deathUI != null)
        {
            deathUI.ShowDeathUI(respawnWaitTime, OnRespawnTimerFinished);
        }
        else
        {
            Invoke(nameof(OnRespawnTimerFinished), respawnWaitTime);
        }
    }

    private void OnRespawnTimerFinished()
    {
        SpawnPlayer();
    }

    // ==========================================
    // EDITOR & TESTING HELPER METHODS
    // ==========================================

    [ContextMenu("Test/Kill Active Player")]
    public void TestKillPlayer()
    {
        if (activePlayer != null && !activePlayer.IsDead)
        {
            Debug.Log("<color=red>[Debug Tool]:</color> Force killing active player.", this);
            activePlayer.Die();
        }
        else
        {
            Debug.LogWarning("GameManagerSpawner: Cannot kill player — Active player is either null or already dead!", this);
        }
    }

    [ContextMenu("Test/Cancel Respawn UI")]
    public void TestCancelRespawn()
    {
        if (deathUI != null)
        {
            deathUI.HideDeathUI();
            CancelInvoke(nameof(OnRespawnTimerFinished));
            Debug.Log("<color=yellow>[Debug Tool]:</color> Cancelled Death UI sequence.", this);
        }
    }
}