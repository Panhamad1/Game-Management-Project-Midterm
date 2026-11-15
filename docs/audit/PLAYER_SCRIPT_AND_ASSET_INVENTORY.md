# PLAYER SCRIPT & ASSET INVENTORY

**Document ID:** `AUDIT-PLY-003`  
**Classification:** Complete Codebase Asset Register & Dependency Graph  
**Project:** REBOUND | **Branch:** `monirath/game-rework`  
**Date:** 2026-11-15  

---

## 1. COMPREHENSIVE SCRIPT REGISTRY

Every script in the player subsystem and its immediate combat interfaces is cataloged below with exact verified properties [V]:

### 1. `PlayerMovement.cs`
- **Path:** `Assets/_Project/Scripts/Player/PlayerMovement.cs`
- **Lines / Size:** 1,137 lines | 46,299 bytes
- **Primary Responsibility:** Locomotion physics, ground anchoring, Quake-style air strafing, power sliding, bunny-hopping, and velocity clamping.
- **Key Methods:** `FixedUpdate()`, `UpdateGroundState()`, `HandleLocomotion()`, `HandleSliding()`, `HandleAirStrafing()`, `Jump()`, `ApplyGrapplePull()`, `ApplyEnemyKnockback()`, `RespawnAtCheckpoint()`.
- **Dependencies:** `Rigidbody`, `CapsuleCollider`, `MouseLook`, `PlayerAudioEffects`, `PullAbility`, `PlayerMovementTelemetry`.
- **Status:** Core active locomotion controller [V].

### 2. `PlayerAbilityInput.cs`
- **Path:** `Assets/_Project/Scripts/Player/PlayerAbilityInput.cs`
- **Lines / Size:** 198 lines | 6,932 bytes
- **Primary Responsibility:** Reads hardware states from the Unity Input System and dispatches them to movement, look, weapons, and abilities.
- **Key Methods:** `Update()`, `HandleLocomotionInput()`, `HandleLookInput()`, `HandleAbilityInput()`, `HandleCursorState()`.
- **Dependencies:** `UnityEngine.InputSystem`, `Keyboard`, `Mouse`, `PlayerMovement`, `MouseLook`, `KineticHammergun`, `PullAbility`, `SlowMotionAbility`.
- **Status:** Core active input dispatcher [V].

### 3. `MouseLook.cs`
- **Path:** `Assets/_Project/Scripts/Player/MouseLook.cs`
- **Lines / Size:** 210 lines | 7,590 bytes
- **Primary Responsibility:** First-person look controller, pitch clamping (-85° to +85°), body yaw rotation, dynamic speed/slow-mo FOV warping, sinusoidal head bobbing, landing compression dip, and screen shake trauma.
- **Key Methods:** `UpdateCameraMotion()`, `SetLookInput()`, `AddScreenShake()`, `TriggerLandingDip()`, `LockCursor()`, `UnlockCursor()`.
- **Dependencies:** `Camera`, `SlowMotionAbility`, `Rigidbody` (player body).
- **Status:** Core active camera controller [V].

### 4. `PlayerHealth.cs`
- **Path:** `Assets/_Project/Scripts/Player/PlayerHealth.cs`
- **Lines / Size:** 245 lines | 7,539 bytes
- **Primary Responsibility:** Authoritative damage receiver, attack ID deduplication dictionary, damage event dispatch, directional impact shake, vampiric kill healing, and death reset.
- **Key Methods:** `TakeDamage(amount, source, attackId)`, `Heal(amount)`, `HealOnKill(enemyType, enemyPos)`, `Die()`, `PruneOldAttackIds()`.
- **Dependencies:** `PlayerMovement`, `PullAbility`, `PushAbility`, `SlowMotionAbility`, `MouseLook`, `Rigidbody`, `FloatingDamageNumber`.
- **Status:** Core active health pipeline [V].

### 5. `KineticHammergun.cs`
- **Path:** `Assets/_Project/Scripts/Combat/KineticHammergun.cs`
- **Lines / Size:** 235 lines | 8,847 bytes
- **Primary Responsibility:** Primary weapon: hold Left-Click to pull back pneumatic hammer, release in 0.22s - 0.45s sweet-spot to fire a 70-damage piercing kinetic slug; forward cone projectile deflection.
- **Key Methods:** `StartHammerPull()`, `ReleaseHammer()`, `FireKineticSlug()`, `DeflectProjectilesInBlastCone()`, `ShowTracerRoutine()`.
- **Dependencies:** `Camera`, `MouseLook`, `PlayerMovement`, `LineRenderer`, `IDeflectableProjectile`, `IKineticTarget`.
- **Status:** Core active primary weapon [V].

### 6. `PullAbility.cs`
- **Path:** `Assets/_Project/Scripts/Abilities/PullAbility.cs`
- **Lines / Size:** 794 lines | 32,872 bytes
- **Primary Responsibility:** Dual-mode Right-Click utility: elastic grapple slingshot to walls/anchors and gravity-gun object snatching / Crate Cannon launching.
- **Key Methods:** `StartPull()`, `StopPull()`, `UpdateGrappleTether()`, `UpdateHeldObjectPhysics()`, `TryLaunchHeldObject()`, `FlingGrappleJump()`, `ResetState()`.
- **Dependencies:** `PlayerMovement`, `MouseLook`, `PlayerAudioEffects`, `IKineticTarget`, `InteractableProp`, `LineRenderer`.
- **Status:** Core active utility & traversal ability [V].

### 7. `PushAbility.cs`
- **Path:** `Assets/_Project/Scripts/Abilities/PushAbility.cs`
- **Lines / Size:** 348 lines | 14,257 bytes
- **Primary Responsibility:** Compressed air blast, surface proximity kick (rocket jump & slide boost), projectile deflection parry window.
- **Key Methods:** `TryPush()`, `PerformPush()`, `CheckSurfaceProximity()`, `CheckProjectileParry()`, `AnimateShockwave()`.
- **Dependencies:** `PlayerMovement`, `MouseLook`, `PlayerAudioEffects`, `IDeflectableProjectile`, `IKineticTarget`.
- **Status:** Active legacy ability (partially superseded by Hammergun) [V].

### 8. `SlowMotionAbility.cs`
- **Path:** `Assets/_Project/Scripts/Abilities/SlowMotionAbility.cs`
- **Lines / Size:** 172 lines | 5,791 bytes
- **Primary Responsibility:** Global time dilation (timeScale = 0.40, fixedDeltaTime = 0.008s), Focus energy meter drain, kinetic siphon recharge.
- **Key Methods:** `ActivateSlowMotion()`, `DeactivateSlowMotion()`, `SiphonEnergy()`.
- **Dependencies:** `PlayerMovement`, `Camera`, `PlayerAudioEffects`.
- **Status:** Core active time-dilation ability [V].

### 9. `PlayerAudioEffects.cs`
- **Path:** `Assets/_Project/Scripts/Player/PlayerAudioEffects.cs`
- **Lines / Size:** 492 lines | 17,134 bytes
- **Primary Responsibility:** Procedural synthesis of 13 custom audio clips at runtime; zero external WAV/MP3 asset dependencies.
- **Key Methods:** `GenerateClips()`, `PlayFootstep()`, `PlayWindBlast()`, `PlayCrateLaunch()`, `PlaySlowMoEnter()`, `PlayImpact()`, `SetSlideScrapeVolume()`, `SetWindRushSpeed()`.
- **Dependencies:** `AudioSource`, `AudioClip.Create()`.
- **Status:** Core active audio synthesizer [V].

### 10. `AbilityHUD.cs`
- **Path:** `Assets/_Project/Scripts/Player/AbilityHUD.cs`
- **Lines / Size:** 576 lines | 24,085 bytes
- **Primary Responsibility:** Real-time tactical HUD: dynamic reticle brackets, target badge readout, speedometer, health bar, slow-mo bar, and fullscreen vignettes.
- **Key Methods:** `UpdateCrosshairTargeting()`, `UpdateFocusVision()`, `UpdateDamageFeedback()`, `OnGUI()`, `SetupProceduralTextures()`.
- **Dependencies:** `PlayerMovement`, `PlayerHealth`, `SlowMotionAbility`, `PullAbility`, `PushAbility`, `KineticHammergun`, UGUI Canvas.
- **Status:** Core active HUD controller [V].

### 11. `PlayerMovementTelemetry.cs`
- **Path:** `Assets/_Project/Scripts/Player/PlayerMovementTelemetry.cs`
- **Lines / Size:** 560 lines | 21,014 bytes
- **Primary Responsibility:** Diagnostic instrumentation: real-time speed vector decomposition, state transition logging, grapple metrics, impulse tracking.
- **Key Methods:** `Update()`, `RecordTransition()`, `RecordImpulse()`, `OnGUI()`.
- **Dependencies:** `PlayerMovement`, `PullAbility`, `PushAbility`, `SlowMotionAbility`, `PlayerHealth`.
- **Status:** Core active diagnostic tool [V].

### 12. `PlayerRigBuilder.cs`
- **Path:** `Assets/_Project/Scripts/Editor/PlayerRigBuilder.cs`
- **Lines / Size:** 946 lines | 52,251 bytes
- **Primary Responsibility:** Editor procedural generator constructing the complete `Player_Rig` hierarchy, camera, hand primitives, and UGUI canvas in one click.
- **Key Methods:** `SetupCompleteGame()`, `CreatePlayerRig()`, `BuildTacticalHUD()`, `RunMovementUnitTests()`, `RunCombatAndHealthUnitTests()`.
- **Dependencies:** `UnityEditor`, `PlayerMovement`, `MouseLook`, `PushAbility`, `PullAbility`, `SlowMotionAbility`, `PlayerHealth`, `AbilityHUD`.
- **Status:** Core editor construction utility [V].

---

## 2. PLAYER DEPENDENCY GRAPH

```
                          +------------------------+
                          |   PlayerAbilityInput   |
                          +------------------------+
                             |       |        |
        +--------------------+       |        +--------------------+
        |                            |                             |
        v                            v                             v
+----------------+          +-----------------+          +--------------------+
| PlayerMovement | <------- | KineticHammergun|          |    PullAbility     |
+----------------+          +-----------------+          +--------------------+
   |   ^     ^                                              |       |       ^
   |   |     +--------------------+                         |       |       |
   |   +--------------------+     |                         |       |       |
   v                        |     |                         |       |       |
+-----------+               |     |                         v       v       |
| MouseLook |               |     |                 +--------------------+  |
+-----------+               |     |                 |  InteractableProp  |  |
                            |     |                 +--------------------+  |
                            |     |                            |            |
+-------------------+       |     |                            v            |
| PlayerAudioEffects|<------+     |                 +--------------------+  |
+-------------------+             |                 |   Crate Cannon     |--+
                                  |                 +--------------------+
+-------------------+             |
| SlowMotionAbility |<------------+
+-------------------+
   ^
   |
+-------------------+       +-----------------------+
|    PlayerHealth   | ----> |  FloatingDamageNumber |
+-------------------+       +-----------------------+
   |
   v
+-------------------+
|    AbilityHUD     |
+-------------------+
```

---

## 3. ASSET INVENTORY & INTEGRITY AUDIT

### 3.1 3D Models & Meshes
- `Player_Rig` Mesh: **None [V]**. Pure procedural primitives (`CapsuleCollider`, Unity Cylinders).
- Hands: Left & Right cylinder primitives parented under `CameraPivot`.

### 3.2 Textures & Materials
- All HUD textures (Noir vignette, red damage vignette, white bar texture) are generated at runtime via `Texture2D.SetPixel()` [V].
- `PlayerFrictionless.physicMaterial`: Generated in memory via `new PhysicsMaterial("PlayerFrictionless")` with $0.0$ static/dynamic friction [V].

### 3.3 Audio Assets
- External Audio Files (`.wav`, `.mp3`, `.ogg`): **0 files present in project [V]**.
- All clips generated via `AudioClip.Create()` inside `PlayerAudioEffects.cs`.

### 3.4 Animation Assets
- Player Animator Controller: **None [V]**.
- Skinned Mesh Rigs: **None [V]**.
- All player animations (hand kickback, camera bobbing, camera dip, reticle pulsing) are procedural mathematical interpolations calculated in `Update()` / `LateUpdate()`.

---

## 4. QUESTIONABLE, REDUNDANT OR OBSOLETE ELEMENTS

1. **`PushAbility.cs` (Redundancy):**
   `KineticHammergun.cs` now covers both ranged damage and projectile deflection parry. `PushAbility.cs` remains on the left hand cylinder, but its direct activation is overridden when the hammergun is armed.
2. **`vortexAngle` in `PullAbility.cs`:**
   Assigned in code but unused (triggers compiler warning CS0414). Blue vortex lines were deactivated per user feedback.
3. **`abilityRayOrigin` in `PullAbility.cs`:**
   Declared as private field but unused (triggers compiler warning CS0169).
