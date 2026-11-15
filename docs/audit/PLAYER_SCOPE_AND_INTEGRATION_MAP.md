# PLAYER SCOPE & INTEGRATION MAP

**Document ID:** `AUDIT-PLY-004`  
**Classification:** Team Boundary Verification & System Coupling Matrix  
**Project:** REBOUND | **Branch:** `monirath/game-rework`  
**Date:** 2026-11-15  

---

## 1. SCOPE TAXONOMY & TEAM BOUNDARIES

```
+----------------------------------------------------------------------------------------------------+
|                                    PROJECT OWNERSHIP TAXONOMY                                      |
+----------------------------------------------------------------------------------------------------+
| 1. PLAYER SUBSYSTEM (MY SCOPE - 100% UNRESTRICTED MODIFICATION)                                   |
|    - Locomotion, Physics & State Machine: PlayerMovement.cs                                       |
|    - Look, FOV & Camera Feel: MouseLook.cs                                                         |
|    - Input Mapping & Buffering: PlayerAbilityInput.cs                                             |
|    - Primary Weapon Systems: KineticHammergun.cs                                                   |
|    - Traversal & Utility Abilities: PullAbility.cs, PushAbility.cs, SlowMotionAbility.cs          |
|    - Player Health & Resource Pipeline: PlayerHealth.cs                                            |
|    - Sensory & Audio Synthesis: PlayerAudioEffects.cs                                              |
|    - HUD & UI Presentation: AbilityHUD.cs, PlayerMovementTelemetry.cs                              |
|    - Player Hierarchy Tooling: PlayerRigBuilder.cs                                                 |
|    - Dedicated Physics & Unit Tests: MovementPhysicsTests.cs, CombatPhysicsTests.cs                |
+----------------------------------------------------------------------------------------------------+
| 2. ENEMY SUBSYSTEM (TEAMMATE SCOPE - STRICTLY READ-ONLY / DO NOT MODIFY)                          |
|    - Swarmer Enemy: SwarmerEnemy.cs, Swarmer.prefab                                                |
|    - Hulk Leaper Enemy: HulkLeaperEnemy.cs, HulkShockwave.cs, HulkWeakPoint.cs, HulkLeaper.prefab  |
|    - Rapid-Laser Sentry: SentryEnemy.cs, RapidLaserSentry.prefab, SentryBolt.prefab                |
|    - Relic Sniper: SniperEnemy.cs, Sniper.prefab, SniperRound.prefab                               |
|    - Enemy Projectiles: EnemyProjectile.cs                                                         |
|    - Legacy Test Components: SwarmerTestHealth.cs                                                  |
+----------------------------------------------------------------------------------------------------+
| 3. MAP & ENVIRONMENT SUBSYSTEM (TEAMMATE SCOPE - STRICTLY READ-ONLY / DO NOT MODIFY)               |
|    - Egyptian Basalt / Sandstone Architecture & Level Geometry                                     |
|    - Scene Layouts: Assets/Scenes/SampleScene.unity                                                |
|    - Puzzle Mechanics: TargetButton.cs, Door/Gate Triggers                                         |
|    - Materials & Shaders: Assets/_Project/Prefabs/PyramidMaterials/                                |
+----------------------------------------------------------------------------------------------------+
```

---

## 2. SHARED INTERFACES & COUPLING CONTRACTS

To maintain clean architecture without breaking teammates' systems, all player interactions connect through four standardized contracts [V]:

### Contract 1: `IKineticTarget`
- **Definition Path:** `Assets/_Project/Scripts/Abilities/IKineticTarget.cs`
- **Owning Domain:** Shared Interface (Player consumed).
- **Interface Methods:**
  ```csharp
  public interface IKineticTarget
  {
      bool CanBePulled { get; }
      bool CanBePushed { get; }
      void OnPushed(Vector3 direction, float force, Vector3 point);
      void OnPulled(Transform origin, float force);
      void OnPhysicsCrush(float impactSpeed, GameObject prop);
      void OnPlayerSlideRam(float slideSpeed);
  }
  ```
- **Implementers [V]:**
  - Enemies: `SwarmerEnemy`, `HulkLeaperEnemy`, `SentryEnemy`, `SniperEnemy`.
  - Interactables: `InteractableProp`, `ExplosiveProp`, `TargetButton`.
- **Player Call Sites [V]:**
  - `KineticHammergun.cs:175`: Calls `target.OnPhysicsCrush(damage, gameObject)`.
  - `PullAbility.cs:250`: Checks `target.CanBePulled` and calls `target.OnPulled(...)`.
  - `PushAbility.cs:180`: Checks `target.CanBePushed` and calls `target.OnPushed(...)`.
  - `PlayerMovement.cs:815`: Calls `target.OnPlayerSlideRam(currentSpeed)` upon high-speed slide collision.

### Contract 2: `IDeflectableProjectile`
- **Definition Path:** `Assets/_Project/Scripts/Abilities/IDeflectableProjectile.cs`
- **Owning Domain:** Shared Interface.
- **Interface Methods:**
  ```csharp
  public interface IDeflectableProjectile
  {
      bool CanBeDeflected { get; }
      bool TryDeflect(Vector3 newDirection, GameObject newOwner);
  }
  ```
- **Implementers [V]:** `EnemyProjectile.cs` (attached to Sentry bolts and Sniper rounds).
- **Player Call Sites [V]:**
  - `KineticHammergun.cs:208`: Deflects projectiles inside forward blast cone on fire.
  - `PushAbility.cs:145`: Deflects projectiles inside parry sphere window ($0.22\text{s}$).

### Contract 3: `IDamageReceiver`
- **Definition Path:** `Assets/_Project/Scripts/Player/PlayerHealth.cs`
- **Owning Domain:** Player Subsystem.
- **Interface Methods:**
  ```csharp
  public interface IDamageReceiver
  {
      void TakeDamage(float amount, string source = "Unknown", int attackId = 0);
  }
  ```
- **Implementer [V]:** `PlayerHealth.cs`.
- **Enemy Call Sites [V]:**
  - `SwarmerEnemy.cs:266`: `playerHealth.TakeDamage(15f, "Swarmer_Bite", GetInstanceID())`.
  - `HulkLeaperEnemy.cs:187`: `playerHealth.TakeDamage(20f, "Hulk_Melee_Punch", GetInstanceID())`.
  - `HulkShockwave.cs:68`: `playerHealth.TakeDamage(30f, "Hulk_Slam_Shockwave", GetInstanceID())`.
  - `EnemyProjectile.cs:149`: `playerHealth.TakeDamage(damage, "Sentry_Bolt" / "Sniper_Round", GetInstanceID())`.

### Contract 4: Physics Layers & Collision Matrix
- **Config Path:** `ProjectSettings/TagManager.asset` [V]
- **Required Convention:**
  - `Layer 8: Player` (Never targeted by player raycasts).
  - `Layer 9: Environment` (Targeted by grapple raycasts, ground check, mantling).
  - `Layer 10: Interactables` (Targeted by vacuum snatch, Crate Cannon).
  - `Layer 12: Enemies` (Targeted by Hammergun, Crate Cannon, slide rams).
  - `Layer 13: Projectiles` (Targeted by parry deflection cones).

---

## 3. INTEGRATION FRICTION & ISOLATION STRATEGY

### Potential Coupling Hazards & Solutions

1. **Hazard:** Teammate enemies directly calling legacy `SwarmerTestHealth` on the player.
   - **Status:** Resolved [V]. `PlayerHealth.cs` implements backward-compatible `TakeDamage(float amount)` and enemy scripts fall back to `SwarmerTestHealth` only if `PlayerHealth` is absent.
   - **Isolation Strategy:** Keep `PlayerHealth` on `Player_Rig` so all enemy attacks route cleanly through the authoritative damage pipeline.

2. **Hazard:** Teammates modifying enemy speeds or health to compensate for player movement feel.
   - **Status:** Guarded. All enemy parameters in `Prefabs` are calibrated to documented baselines.
   - **Isolation Strategy:** Complete player rework must achieve responsive locomotion and engaging combat within existing enemy health budgets ($20\text{ HP}$ Swarmer, $30\text{ HP}$ Sentry, $30\text{ HP}$ Sniper, $90\text{ HP}$ Hulk).

3. **Hazard:** Environmental level geometry lacking designated grapple anchor components.
   - **Status:** Resolved [V]. `PullAbility.cs` probes any geometry on the `Environment` layer, allowing grapples to attach to any solid wall, ceiling, or pillar without requiring teammates to manually place custom anchor scripts.
   - **Isolation Strategy:** Maintain broad layer-based collision queries rather than relying on brittle component tags.

---

## 4. BOUNDARY SUMMARY

Every required player subsystem rework—locomotion, firearms, grappling, camera behavior, and HUD presentation—can be achieved **100% within player-owned files** while respecting teammate interfaces.
