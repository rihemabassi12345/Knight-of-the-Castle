Here is your complete game development design document converted into a structured, professional **Markdown (.md)** file written in clear, concise English.

---

# Technical Specification: Modular Enemy AI System Architecture

## 1. System Overview & Architecture

To ensure high scalability, performance, and clean code separation, the enemy unit responsibility is broken down into independent, single-responsibility modules. The core logic is decoupled from a monolithic controller into modular components attached to the main `Enemy` GameObject.

```
ENEMY GAMEOBJECT
├── EnemyController (Coordinator / State Engine)
├── EnemyData (ScriptableObject Configuration)
├── EnemyMovement (NavMesh Navigation)
├── EnemyDetection (Vision, Line of Sight & Spatial Queries)
├── EnemyTargeting (Priority & Locking Logic)
├── EnemyCombat (Range Check, Cooldown & Damage Output)
└── EnemyAnimation (Animator Driver & Event Handlers)

```

---

## 2. Data-Driven Configuration (`EnemyData`)

The `EnemyData` class inherits from `ScriptableObject`. It houses pure data attributes without executing logic, enabling seamless creation of distinct enemy archetypes (e.g., Goblin, Knight, Orc, Berserker) directly from the Unity Editor.

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy System/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Health")]
    public float health = 100f;

    [Header("Combat")]
    public float damage = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;

    [Header("Movement")]
    public float speed = 3.5f;
    public float stoppingDistance = 1.5f;
    public float rotationSpeed = 10f;

    [Header("Detection")]
    public float detectionRange = 10f;
    public float targetSwitchDelay = 2f;

    [Header("Behavior Flags")]
    public bool canChasePlayer = true;
    public bool canAttackCastle = true;
    public bool canDestroyObstacles = true;
}

```

---

## 3. Finite State Machine (FSM)

Instead of running unorganized condition checks inside `Update()`, the behavior flow is driven by an explicit Finite State Machine:

```
                  [ SPAWN ]
                      │
                      ▼
                   [ IDLE ]
                      │
               Target Acquired
                      │
                      ▼
               [ TARGET LOCK ]
                      │
          ┌───────────┴───────────┐
          ▼                       ▼
     [ MOVING ]              [ ATTACKING ]
          │                       │
          │                  Target Dead
          │                       │
          └───────────┬───────────┘
                      ▼
              [ SEARCH TARGET ]
                      │
                      ▼
                   [ DEAD ]

```

### Core States Enum

```csharp
public enum EnemyState
{
    Idle,
    Moving,
    Attacking,
    Searching,
    Stunned,      // Reserved for CC mechanics
    Returning,    // Reserved for leash behavior
    Dead
}

```

---

## 4. Target Acquisition & Priority Matrix

The main goal of the enemy is to reach the Castle, but active threats and physical blockades override this primary objective based on strict priority ordering:

```
[ Priority 1 ] ──> Obstacles (Blocking direct path)
[ Priority 2 ] ──> King / Player Character
[ Priority 3 ] ──> Defensive Structures (Towers, Walls)
[ Priority 4 ] ──> Main Castle (Ultimate Target)

```

### Target Evaluation Scenarios

* **Scenario A:** Enemy path blocked by wall $\rightarrow$ Target and attack **Obstacle**.
* **Scenario B:** Clear path + Player enters detection radius $\rightarrow$ Target and chase **King**.
* **Scenario C:** Clear path + Player absent + Defense active $\rightarrow$ Target and attack **Defense**.
* **Scenario D:** No dynamic threats detected $\rightarrow$ Pathing towards **Castle**.

---

## 5. Target Lock Mechanics

To prevent **target flickering** (rapidly switching targets every frame due to distance micro-changes), the system enforces a target evaluation delay (`targetSwitchDelay`).

```
Target Acquired ──> Lock Target Timer (e.g., 2.0s) ──> Continue Tracking ──> Timer Expired ──> Re-evaluate Best Target

```

---

## 6. Spatial Detection & Line of Sight

The detection system evaluates spatial awareness using a multi-phase query pipeline:

```
Physics.OverlapSphere
        │
        ▼
Filter by IDamageable & IsAlive
        │
        ▼
Execute Line of Sight (Raycast Check)
        │
        ▼
Calculate Weighted Score (Priority + Distance)
        │
        ▼
Return Optimal Target

```

### Line of Sight Logic

```
[Enemy] ─── Raycast ───> [Obstacle Wall] ───X───> [King]  (Target Invalid)
[Enemy] ──────────────── Raycast ───────────────> [King]  (Target Valid)

```

---

## 7. Decoupled Subsystems Implementation

### Movement Subsystem (`EnemyMovement`)

Responsible purely for NavMesh pathing operations:

```csharp
public class EnemyMovement : MonoBehaviour
{
    private NavMeshAgent agent;

    public void MoveTo(Vector3 destination)
    {
        if (agent.isStopped) agent.isStopped = false;
        agent.SetDestination(destination);
    }

    public void Stop()
    {
        agent.isStopped = true;
    }
}

```

### Combat Subsystem (`EnemyCombat`)

Handles range validation, attack timing, and damage delivery.

> [!TIP]
> **Component Hierarchy Robustness:** Always query interfaces using `GetComponentInParent<IDamageable>()` to ensure detection works when colliders are assigned to child node transforms.

```csharp
// Robust damage resolution logic
IDamageable damageableTarget = target.GetComponentInParent<IDamageable>();
if (damageableTarget != null && canAttack)
{
    // Execute Attack Logic
}

```

---

## 8. Animation & Damage Synchronization

Damage application must sync cleanly with combat animations using **Animation Events** rather than instant calculations at state entry.

```
0.0s: Play Attack Animation
 ├── 0.4s: [Animation Event Trigger] ──> Execute DealDamage()
 └── 0.8s: Animation Complete ──> Reset Attack Cooldown

```

---

## 9. Death Cycle & Object Pooling Integration

To eliminate allocation overhead during runtime, units recycled back into the pool must go through a structured reset pipeline.

```
Take Damage ──> HP <= 0 ──> Die Trigger
                                │
                                ▼
                   Disable Agent & Colliders
                                │
                                ▼
                       Play Death Animation
                                │
                                ▼
                      Wait Delay (1.0 - 2.0s)
                                │
                                ▼
                     Return Unit to Pool

```

### Object Reset Matrix on Reuse (`OnSpawnFromPool`)

| Component | Required Reset Action |
| --- | --- |
| **Health** | Restore to `EnemyData.health` |
| **FSM State** | Set to `EnemyState.Idle` |
| **Targeting** | Clear reference buffers & unlock timers |
| **Combat** | Zero out cooldown timers |
| **Animator** | Trigger reset parameters & restore speeds |
| **Physics / NavMesh** | Re-enable `Collider` and `NavMeshAgent` |

---

## 10. Archetype Parameter Configurations

| Archetype | HP | Damage | Speed | Range | Special Behavior |
| --- | --- | --- | --- | --- | --- |
| **Melee Grunt** | 100 | 10 | 3.5 | 2.0m | Standard pathing |
| **Tank** | 500 | 30 | 1.5 | 2.5m | Ignores player, targets defenses |
| **Fast Flanker** | 70 | 8 | 6.0 | 1.5m | High player-chase priority |
| **Ranged Shooter** | 80 | 15 | 2.5 | 10.0m | Maintains distance from target |

---

## 11. Wave & Spawner Data Pipeline

Transitioning from time-interval spawning to data-driven wave structures:

```
Wave Sequence Configuration
 ├── Enemy Archetype Reference
 ├── Spawn Count
 ├── Spawn Interval Rate
 └── Target Spawn Point Node

```

---

## 12. Advanced AI Extensions (Future Roadmap)

* **Squad Behaviors:** Leader-follower dynamics.
* **Tactical Pathing:** Dynamic hazard avoidance and flank routes.
* **Aggro & Threat System:** Target shifting based on highest damage dealt.
* **Status Effects:** Stun, knockback, and freeze processing modules.

---

## 13. Step-by-Step Refactoring Strategy

To refactor your existing codebase cleanly without breaking existing logic, follow this sequence:

1. **Step 1:** Extract `EnemyMovement` logic into its dedicated script.
2. **Step 2:** Upgrade `EnemyDetection` to support priority filtering (*Obstacle* $\rightarrow$ *King* $\rightarrow$ *Defense* $\rightarrow$ *Castle*).
3. **Step 3:** Connect `EnemyDetection` output directly to `EnemyMovement`.
4. **Step 4:** Refactor `EnemyCombat` to use parent component lookups and animation triggers.
5. **Step 5:** Implement the core `EnemyStateMachine` in `EnemyController`.
6. **Step 6:** Standardize the **Death & Object Pooling** reset routines.
7. **Step 7:** Create `EnemyData` ScriptableObject presets for distinct enemy types.
8. **Step 8:** Build the data-driven **Wave Spawner System**.