# PLAYER SUBSYSTEM MASTER AUDIT & TECHNICAL HANDOVER

**Document ID:** `AUDIT-PLY-001`  
**Classification:** Evidence-Based Source-of-Truth Technical Extraction  
**Project:** REBOUND (Egyptian Themed Kinetic Physics Action Game)  
**Engine & Target:** Unity 6000.3.8f1 (URP 17.3.0, Input System 1.18.0)  
**Branch:** `monirath/game-rework` | **Commit:** `3da8931`  
**Date:** 2026-11-15  
**Evidence Standard:** [V] Verified from code/scene | [D] Documented in repo specs | [I] Inferred from implementation | [S] Suspected issue | [U] Unknown / Unverified

---

## 1. EXECUTIVE PROJECT OVERVIEW

REBOUND is a high-speed, physics-driven, first-person action game set within an ancient Egyptian basalt and sandstone aesthetic. The core game loop was originally prototyped around a Redliner-inspired kinetic player controller utilizing an elastic grapple winch, compressed air blasts, gravity-manipulation vacuum snatching, object projectile throwing ("Crate Cannon"), and time dilation ("Focus").

Despite several iterative balance phases, the player experience remains frustrating, disjointed, and unsatisfying. The movement feels either unresponsive or slippery, ability interactions create cognitive clutter, the player lacks a satisfying and intuitive primary combat loop, and dependencies between movement, physics, and external enemy entities are coupled through brittle scene configurations.

This audit establishes an exhaustive, 100% verified technical baseline of the player subsystem to enable a complete, ground-up rework of player mechanics, abilities, and game feel without violating team boundaries.

---

## 2. DEVELOPMENT SCOPE & OWNERSHIP BOUNDARIES

The project is developed collaboratively with strict domain boundaries:

```
+-----------------------------------------------------------------------------------+
|                            REBOUND ARCHITECTURE BOUNDARIES                        |
+-----------------------------------------------------------------------------------+
|  IN-SCOPE (USER RESPONSIBILITY: COMPLETE PLAYER SUBSYSTEM)                        |
|  - Locomotion, Physics, State Machine, Rigidbody Authority (PlayerMovement.cs)   |
|  - First-Person Camera, Look, FOV, Dynamic Punch/Shake (MouseLook.cs)             |
|  - Hardware Input Dispatching & Buffering (PlayerAbilityInput.cs)                 |
|  - Weapon & Ability Mechanics (Hammergun, Push, Pull, SlowMo)                     |
|  - Player Health, Damage Pipeline, Deduplication, Vampiric Heal (PlayerHealth.cs) |
|  - Procedural Audio Synthesis (PlayerAudioEffects.cs)                             |
|  - Player UI Canvas, Reticles, Vignettes, Status HUD (AbilityHUD.cs)              |
|  - Real-Time Diagnostic Instrumentation (PlayerMovementTelemetry.cs)             |
|  - Rig Construction Tooling (PlayerRigBuilder.cs)                                 |
+-----------------------------------------------------------------------------------+
                                         |
                            INTERFACES & COUPLING POINTS
       [IKineticTarget] [IDeflectableProjectile] [IKineticCarryTarget] [Layer Masks]
                                         |
+-----------------------------------------------------------------------------------+
|  OUT-OF-SCOPE (TEAMMATE RESPONSIBILITY: PRESERVE & ADAPT VIA CONTRACTS)           |
|  - Map & Level Design, Basalt Architecture, Room Layouts                          |
|  - Enemy AI State Machines & Behaviors (Swarmer, Sentry, Sniper, Hulk Leaper)     |
|  - Environmental Assets, Egyptian Dressing, Level Hazards                        |
+-----------------------------------------------------------------------------------+
```

---

## 3. PROJECT STRUCTURE & TECHNOLOGY STACK

- **Game Engine [V]:** Unity 6000.3.8f1 (Revision: `1c7db571dde0`).
- **Render Pipeline [V]:** Universal Render Pipeline (URP) `17.3.0`.
- **Input System [V]:** Unity Input System `1.18.0` (`InputSystem_Actions.inputactions` present in `Assets/`).
- **Physics Engine [V]:** NVIDIA PhysX integrated (`ProjectSettings/DynamicsManager.asset`). Standard gravity: `Y = -9.81 m/s²`.
- **UI Framework [V]:** Unity UI (UGUI `2.0.0`) backed by immediate-mode `OnGUI()` fallback pipelines.
- **Audio Pipeline [V]:** Unity Audio Module with 100% runtime procedural waveform synthesis (zero external audio file dependencies).
- **Compilation Tooling [V]:** Roslyn C# Compiler `4.3.1-3.22526.13`. Custom standalone verification script `Temp/build_check.ps1`.

### Directory Tree of Relevant Assets

```
Assets/
├── Scenes/
│   ├── CombatIntegrationScene.unity    [V] (Primary encounter & colosseum scene)
│   └── SampleScene.unity               [V] (Legacy base environment scene)
├── _Project/
│   ├── Prefabs/
│   │   ├── HulkLeaper.prefab           [V] (Heavy boss brawler)
│   │   ├── RapidLaserSentry.prefab     [V] (Burst fire turret)
│   │   ├── SentryBolt.prefab           [V] (Enemy projectile)
│   │   ├── Sniper.prefab               [V] (Relic long-range enemy)
│   │   ├── SniperRound.prefab          [V] (High-velocity sniper lance)
│   │   └── Swarmer.prefab              [V] (Melee insectoid flyer)
│   └── Scripts/
│       ├── Abilities/
│       │   ├── IDeflectableProjectile.cs
│       │   ├── IKineticCarryTarget.cs
│       │   ├── IKineticTarget.cs
│       │   ├── KineticImpactContext.cs
│       │   ├── PullAbility.cs
│       │   ├── PushAbility.cs
│       │   └── SlowMotionAbility.cs
│       ├── Combat/
│       │   ├── CombatWaveManager.cs
│       │   └── KineticHammergun.cs
│       ├── Editor/
│       │   ├── CombatIntegrationSetup.cs
│       │   ├── CombatPhysicsTests.cs
│       │   ├── MovementPhysicsTests.cs
│       │   └── PlayerRigBuilder.cs
│       ├── Enemies/
│       │   ├── EnemyHealthBar.cs
│       │   ├── EnemyHitFlash.cs
│       │   ├── EnemyProjectile.cs
│       │   ├── EnemyVisualPart.cs
│       │   ├── FloatingDamageNumber.cs
│       │   ├── HulkLeaperEnemy.cs
│       │   ├── HulkShockwave.cs
│       │   ├── HulkWeakPoint.cs
│       │   ├── SentryEnemy.cs
│       │   ├── SniperEnemy.cs
│       │   ├── SwarmerEnemy.cs
│       │   └── SwarmerTestHealth.cs
│       ├── Interactables/
│       │   ├── ExplosiveProp.cs
│       │   ├── InteractableProp.cs
│       │   └── TargetButton.cs
│       └── Player/
│           ├── AbilityHUD.cs
│           ├── MouseLook.cs
│           ├── PlayerAbilityInput.cs
│           ├── PlayerAudioEffects.cs
│           ├── PlayerHealth.cs
│           ├── PlayerMovement.cs
│           └── PlayerMovementTelemetry.cs
```

---

## 4. PLAYER HIERARCHY & CONFIGURATION

There is **no serialized player prefab asset** in `Assets/_Project/Prefabs/` [V]. The player is procedurally built and injected into scenes via `PlayerRigBuilder.cs` [V].

### Hierarchy Map of `Player_Rig`

```
Player_Rig [Layer: Player (8), Tag: Player]
├── Rigidbody (Mass: 75kg, Drag: 0.05, FreezeRotation X/Y/Z, ContinuousDynamic, Interpolate)
├── CapsuleCollider (Height: 2.0m, Radius: 0.45m, Center: (0,0,0), Material: Frictionless)
├── PlayerMovement.cs
├── PlayerAbilityInput.cs
├── PlayerHealth.cs
├── PlayerAudioEffects.cs
├── PlayerMovementTelemetry.cs
├── SlowMotionAbility.cs
├── KineticHammergun.cs
│
├── GroundCheck (LocalPos: 0, -0.95, 0)
│
├── CameraPivot (LocalPos: 0, 0.65, 0)
│   ├── MouseLook.cs
│   ├── MainCamera [Tag: MainCamera]
│   │   ├── Camera (NearClip: 0.03m, BaseFOV: 85)
│   │   └── AudioListener
│   ├── AbilityRayOrigin (LocalPos: 0, 0, 0.35)
│   ├── LeftHandVisual (LocalPos: -0.35, -0.25, 0.5; Rot: 90, 0, 0; Scale: 0.08, 0.2, 0.08)
│   │   └── PushAbility.cs
│   └── RightHandVisual (LocalPos: 0.35, -0.25, 0.5; Rot: 90, 0, 0; Scale: 0.08, 0.2, 0.08)
│       └── PullAbility.cs
│
└── AbilityHUD_Canvas (Canvas: ScreenSpaceOverlay)
    ├── AbilityHUD.cs
    ├── ElectricCyanFocusOverlay (Image, Fullscreen Rect)
    ├── CrosshairRoot (40x40 Rect)
    │   ├── CenterDot (6x6 Image)
    │   ├── Top (10x2 Bracket)
    │   ├── Bottom (10x2 Bracket)
    │   ├── Left (2x10 Bracket)
    │   └── Right (2x10 Bracket)
    ├── TargetBadge (Text)
    ├── SlowMoEnergySlider (Slider)
    ├── ControlsGuide (Text)
    └── SpeedometerHUD (Text)
```

---

## 5. COMPLETE SCRIPT & DEPENDENCY INVENTORY

| Script Path | Class Name | Responsibility | Dependencies | Active State |
|---|---|---|---|---|
| `Assets/_Project/Scripts/Player/PlayerMovement.cs` | `PlayerMovement` | Locomotion, physics velocity, air strafing, slide math, jump buffers | `Rigidbody`, `CapsuleCollider`, `MouseLook`, `PlayerAudioEffects`, `PullAbility` | Active [V] |
| `Assets/_Project/Scripts/Player/PlayerAbilityInput.cs` | `PlayerAbilityInput` | Polling Input System hardware states and routing to mechanics | `Keyboard`, `Mouse`, `PlayerMovement`, `MouseLook`, `KineticHammergun`, `PullAbility`, `SlowMotionAbility` | Active [V] |
| `Assets/_Project/Scripts/Player/MouseLook.cs` | `MouseLook` | First-person look, yaw/pitch clamp, head bob, landing dip, FOV warp, trauma shake | `Camera`, `SlowMotionAbility`, `Rigidbody` | Active [V] |
| `Assets/_Project/Scripts/Player/PlayerHealth.cs` | `PlayerHealth` | Authoritative health, damage deduplication, vampiric heal, death reset | `PlayerMovement`, `PullAbility`, `SlowMotionAbility`, `MouseLook`, `Rigidbody` | Active [V] |
| `Assets/_Project/Scripts/Player/PlayerAudioEffects.cs` | `PlayerAudioEffects` | Procedural synthesis and playback of 13 sound effects | `AudioSource` | Active [V] |
| `Assets/_Project/Scripts/Player/AbilityHUD.cs` | `AbilityHUD` | UI canvas reticle, badge readout, speedometer, health bar, vignettes | `PlayerMovement`, `PlayerHealth`, `SlowMotionAbility`, `PullAbility`, `PushAbility`, `KineticHammergun` | Active [V] |
| `Assets/_Project/Scripts/Player/PlayerMovementTelemetry.cs` | `PlayerMovementTelemetry` | Diagnostic overlay, state transition timers, vector decomposition | `PlayerMovement`, `PullAbility`, `PushAbility`, `SlowMotionAbility`, `PlayerHealth` | Active [V] |
| `Assets/_Project/Scripts/Combat/KineticHammergun.cs` | `KineticHammergun` | Primary weapon: hold LMB to draw hammer, release on timing to shoot | `Camera`, `MouseLook`, `PlayerMovement`, `IDeflectableProjectile`, `IKineticTarget` | Active [V] |
| `Assets/_Project/Scripts/Abilities/PullAbility.cs` | `PullAbility` | Grapple tether slingshot, object snatch, Crate Cannon aiming & release | `PlayerMovement`, `MouseLook`, `PlayerAudioEffects`, `IKineticTarget`, `InteractableProp` | Active [V] |
| `Assets/_Project/Scripts/Abilities/PushAbility.cs` | `PushAbility` | Wind cannon blast, surface proximity kick, parry deflection window | `PlayerMovement`, `MouseLook`, `PlayerAudioEffects`, `IDeflectableProjectile` | Active [V] |
| `Assets/_Project/Scripts/Abilities/SlowMotionAbility.cs` | `SlowMotionAbility` | Time dilation (timeScale 0.40), energy meter drain and kinetic siphon | `PlayerMovement`, `Camera`, `PlayerAudioEffects` | Active [V] |
| `Assets/_Project/Scripts/Abilities/IKineticTarget.cs` | `IKineticTarget` | Interface for entities responding to push, pull, crush, and slide ram | Unity Object | Interface [V] |
| `Assets/_Project/Scripts/Abilities/IDeflectableProjectile.cs` | `IDeflectableProjectile` | Interface for deflectable enemy projectiles | Unity Object | Interface [V] |
| `Assets/_Project/Scripts/Abilities/IKineticCarryTarget.cs` | `IKineticCarryTarget` | Interface for lightweight carryable entities | Unity Object | Interface [V] |
| `Assets/_Project/Scripts/Editor/PlayerRigBuilder.cs` | `PlayerRigBuilder` | Editor builder creating the complete player hierarchy and HUD | UnityEditor, SceneManager, SerializedObject | Editor [V] |

---

## 6. COMPLETE MOVEMENT SYSTEM ANALYSIS

### Movement Parameters Table

| Parameter | Code Field | Value | Unit | Verification Source |
|---|---|---|---|---|
| Walking Speed | `walkSpeed` | 5.5 | m/s | `PlayerMovement.cs:22` [V] |
| Sprint Speed | `sprintSpeed` | 8.5 | m/s | `PlayerMovement.cs:25` [V] |
| Crouch Speed | `crouchSpeed` | 3.5 | m/s | `PlayerMovement.cs:28` [V] |
| Ground Acceleration | `groundAcceleration` | 35.0 | m/s² | `PlayerMovement.cs:31` [V] |
| Ground Braking Deceleration | `groundBraking` | 35.0 | m/s² | `PlayerMovement.cs:34` [V] |
| Ground Momentum Drag | `groundMomentumDrag` | 3.0 | 1/s | `PlayerMovement.cs:37` [V] |
| Air Wish Speed | `maxAirWishSpeed` | 11.0 | m/s | `PlayerMovement.cs:40` [V] |
| Air Acceleration | `airAcceleration` | 10.0 | m/s² | `PlayerMovement.cs:43` [V] |
| Air Speed Cap | `airSpeedCap` | 12.0 | m/s | `PlayerMovement.cs:46` [V] |
| Jump Impulse | `jumpImpulse` | 8.7 | m/s | `PlayerMovement.cs:49` [V] |
| Bunny-Hop Boost | `bHopBoost` | 0.0 | m/s | `PlayerMovement.cs:52` [V] |
| Slide Run Threshold | `runThreshold` | 7.0 | m/s | `PlayerMovement.cs:56` [V] |
| Slide Entry Boost | `slideBoost` | 0.0 | m/s | `PlayerMovement.cs:59` [V] |
| Slide Speed Cap | `slideSpeedCap` | 14.0 | m/s | `PlayerMovement.cs:65` [V] |
| Slide Drop Speed | `minSlideSpeed` | 3.5 | m/s | `PlayerMovement.cs:68` [V] |
| Slide Friction Deceleration | `slideFriction` | 2.0 | m/s² | `PlayerMovement.cs:71` [V] |
| Slide Aerodynamic Drag | `slideDrag` | 0.025 | 1/m | `PlayerMovement.cs:74` [V] |
| Slope Downhill Acceleration | `slopeAcceleration` | 22.0 | m/s² | `PlayerMovement.cs:77` [V] |
| Slide Steering Rate | `slideSteerRate` | 110.0 | deg/s | `PlayerMovement.cs:80` [V] |
| Emergency Speed Ceiling | `emergencySpeedCeiling`| 19.0 | m/s | `PlayerMovement.cs:86` [V] |
| Grapple Inward Target Speed | `grappleInwardSpeed` | 14.0 | m/s | `PlayerMovement.cs:96` [V] |
| Grapple Pull Acceleration | `grapplePullAcceleration`| 22.0 | m/s² | `PlayerMovement.cs:99` [V] |
| Grapple Transient Cap | `transientGrappleCap` | 17.0 | m/s | `PlayerMovement.cs:102` [V] |
| Coyote Time | `coyoteTime` | 0.15 | s | `PlayerMovement.cs:114` [V] |
| Jump Buffer Duration | `jumpBufferTime` | 0.15 | s | `PlayerMovement.cs:115` [V] |
| Standard Gravity Scale | `standardGravityScale` | 2.4 | x (23.54 m/s²) | `PlayerMovement.cs:123` [V] |
| Slow-Mo Gravity Scale | `slowMoGravityScale` | 0.5 | x (4.90 m/s²) | `PlayerMovement.cs:124` [V] |

### Movement State Machine

```
      +-----------------------------------------------------------+
      |                           SPAWN                           |
      +-----------------------------------------------------------+
                                    |
                                    v
+-------------------> +----------------------------+ <-------------------+
|                     |        GROUND_IDLE         |                     |
|                     +----------------------------+                     |
|                       | (WASD input)     ^                             |
|                       v                  | (No input, speed < 0.2)     |
|                     +----------------------------+                     |
|                     |        GROUND_WALK         |                     |
|                     +----------------------------+                     |
|                       | (Shift held)     ^                             |
|                       v                  | (Shift released)            |
|                     +----------------------------+                     |
|                     |       GROUND_SPRINT        |                     |
|                     +----------------------------+                     |
|                       |                  |                             |
|        (C or Ctrl)    |                  | (Space)                     |
|        Speed >= 7 m/s |                  v                             |
|                       |          +---------------+                     |
|                       v          |     JUMP      |                     |
|     +-------------------------+  +---------------+                     |
|     |         SLIDING         |          |                             |
|     +-------------------------+          |                             |
|       |                     |            v                             |
|       | (Speed < 3.5 m/s    | (Space)  +-------------------+           |
|       |  or release C)      v          |     AIRBORNE      |           |
|       |               +-----------+    +-------------------+           |
|       |               | SLIDE_HOP |      |       | (RMB)               |
|       v               +-----------+      |       v                     |
|     +---------------+       |            |    +-------------------+    |
|     |  CROUCH_WALK  |       +------------+    |      GRAPPLE      |    |
|     +---------------+                         +-------------------+    |
|       |                                          | (Release / Jump)    |
|       v                                          v                     |
+-------+------------------------------------------+---------------------+
                                 (Touch Ground)
```

---

## 7. COMPLETE ABILITY SYSTEM ANALYSIS

### Ability Comparison Table

| Ability | Input Trigger | Primary Purpose | Range / Area | Impulse / Output | Resource / Cost | State Duration |
|---|---|---|---|---|---|---|
| **Kinetic Hammergun** | Left Mouse Button (Hold & Release) | Direct offensive ranged weapon; piercing kinetic slug | 60.0m ray | 35 base / 70 crit damage; 16 N impulse | None (Cooldown: 0.35s) | Pullback: 0.22 - 0.45s sweet spot [V] |
| **Pull Ability (Grapple)** | Right Mouse Button (Hold) | High-speed elastic tether traversal to world geometry | 22.0m ray/spherecast | 22 m/s² inward pull, 14 m/s target speed | None | Continuous while held; auto-detaches at 2.5m [V] |
| **Pull Ability (Snatch)** | Right Mouse Button (Hold) | Gravity gun manipulation of crates and light enemies | 14.0m ray/spherecast | 14-24 m/s reel speed; front pocket lock | None | Continuous until released [V] |
| **Crate Cannon** | Left Mouse Button (while carrying prop) | Explosive projectile launch of held prop | Forward ballistic vector | 21 m/s base + 0.25 player vel (max 28 m/s) | Consumes held prop | Instantaneous launch [V] |
| **Push Ability** | Left Mouse Button (Legacy fallback) | Compressed air blast; projectile deflection & surface kick | 12.0m ray; 1.1m parry radius | 18 N target impulse; 4.5-6.0 m/s recoil kick | Cooldown: 0.65s | Parry window: 0.22s [V] |
| **Slow Motion (Focus)** | Q / F / Middle Mouse Button | Time dilation for precision maneuvering and aiming | Global physics | timeScale = 0.40; fixedDeltaTime = 0.008s | Energy pool (100 units, drains in 4.0s) | Continuous while toggled on [V] |

---

## 8. PHYSICS & INTERACTION ARCHITECTURE

1. **Velocity Modification Authority [V]:**
   The player uses a single authoritative physics controller in `PlayerMovement.FixedUpdate()`. Direct assignments to `rb.linearVelocity` occur sequentially:
   - Base locomotion (acceleration/braking).
   - Friction & drag application.
   - Ground slope projection.
   - Gravity acceleration.
   - Impulses from abilities (Hammergun recoil, Push kick, Grapple pull).
   - Hard clamp at `emergencySpeedCeiling = 19.0 m/s`.

2. **Ground & Clearance Probes [V]:**
   - Ground detection is performed using `Physics.CheckSphere` at `GroundCheck` (radius: 0.35m) and `Physics.Raycast` downwards (distance: 1.15m) using `groundLayerMask`.
   - Overhead clearance probe: `Physics.Raycast(transform.position, Vector3.up, standingHeight - crouchHeight, overheadClearanceMask)` prevents standing up inside crouch tunnels.

3. **Collision Matrix & Layers [V]:**
   - `Layer 8: Player`
   - `Layer 9: Environment`
   - `Layer 10: Interactables`
   - `Layer 11: Hazards`
   - `Layer 12: Enemies`
   - `Layer 13: Projectiles`

---

## 9. CAMERA & SENSORY FEEL

- **Camera Hierarchy [V]:** `Player_Rig` -> `CameraPivot` (Y: 0.65m standing, 0.05m crouching) -> `MainCamera`.
- **Mouse Sensitivity [V]:** `mouseSensitivity = 0.12f`.
- **Clamping [V]:** Pitch clamped between `-85°` and `+85°`. Yaw rotates player body directly (`bodyRb.rotation`).
- **Dynamic FOV [V]:** Base FOV: `85°`. Sliding: `+4°`. Slow-Mo: `+12°`. Speed interpolation up to `104°` maximum.
- **Screen Shake [V]:** Implemented via `MouseLook.AddScreenShake(intensity, direction)` using decaying quadratic trauma:
  $$\text{Power} = \text{Trauma}^2 \times 0.08$$
  Decays at rate $4.0 \times \text{unscaledDeltaTime}$.
- **Head Bob & Landing Dip [V]:** Vertical-only sinusoidal bobbing ($10\text{ Hz}$, $0.03\text{m}$ amplitude) disabled while sliding. Landing compression dip proportional to fall speed (max $0.15\text{m}$, recovers at $6.0\text{ m/s}$).

---

## 10. INPUT & CONTROL MAPPING

| Input Hardware | Bound Action | Trigger Type | Implementation Method | Execution Conditions |
|---|---|---|---|---|
| `W / A / S / D` | Horizontal Locomotion | Continuous Polling | `PlayerMovement.SetMoveInput()` | Always active [V] |
| `Left Shift` | Sprint Running | Continuous Polling | `PlayerMovement.SetSprintInput()` | Grounded or preserved in air [V] |
| `C / Left Ctrl` | Crouch / Slide | Continuous Polling | `PlayerMovement.SetCrouchInput()` | Speed $\ge 7\text{ m/s}$ triggers slide [V] |
| `Space` | Jump / B-Hop | Down Event & Held | `PlayerMovement.Jump()` | Grounded, Coyote ($0.15\text{s}$), or Buffer ($0.15\text{s}$) [V] |
| `Left Mouse Button` | Hammergun Prime / Release | Press & Release | `KineticHammergun.Start/Release` | Armed if not carrying prop [V] |
| `Left Mouse Button` | Crate Cannon Launch | Down Event | `PullAbility.TryLaunchHeldObject` | Only when prop locked in front pocket [V] |
| `Right Mouse Button`| Grapple Tether / Prop Reel | Press & Release | `PullAbility.StartPull/StopPull` | Raycast hit on geometry or prop [V] |
| `Q / F / MMB` | Slow-Mo Toggle | Down Event | `SlowMotionAbility.Toggle` | Requires energy $> 5$ units [V] |
| `R` | Quick Respawn | Down Event | `PlayerMovement.RespawnAtCheckpoint`| Always active [V] |
| `F3` | Diagnostic Telemetry | Down Event | `PlayerMovementTelemetry.Toggle` | Debug HUD toggle [V] |
| `Escape` | Unlock Mouse Cursor | Down Event | `MouseLook.UnlockCursor` | Returns cursor to OS [V] |

---

## 11. ANIMATIONS & VISUAL PRESENTATION

- **Player Model [V]:** There is **no skinned mesh humanoid or rigged character model** on the player. The player is rendered from a pure first-person perspective using primitive geometric placeholders:
  - Left Hand: Cylinder (`LeftHandVisual`, scale: $0.08, 0.2, 0.08$) representing the wind/recoil conduit.
  - Right Hand: Cylinder (`RightHandVisual`, scale: $0.08, 0.2, 0.08$) representing the winch/tether conduit.
- **Weapon Visuals [V]:** `KineticHammergun` renders an immediate-mode 2-point dynamic `LineRenderer` tracer ribbon.
- **Screen Shaders [V]:** Immediate-mode procedural textures created at runtime:
  - Noir speed-tunnel vignette (`cyanVignetteTex` with neutral dark alpha).
  - Red damage screen-flash vignette (`redDamageVignetteTex`).

---

## 12. AUDIO & FEEDBACK

All 13 audio clips are **synthesized procedurally at runtime** inside `PlayerAudioEffects.cs` via `AudioClip.Create()` [V]:
1. `Footstep`: Soft low-frequency damped noise click.
2. `WindBlast`: Exponential low-pass noise pop.
3. `SuctionHum`: Continuous $65\text{ Hz}$ sine wave hum with slight frequency modulation.
4. `CrateLaunch`: Punchy transient impulse pop with pitched decay.
5. `SlowMoEnter`: Downward pitch frequency drop from $180\text{ Hz}$ to $45\text{ Hz}$.
6. `SlowMoExit`: Upward pitch sweep from $45\text{ Hz}$ to $180\text{ Hz}$.
7. `ImpactCrunch`: High-frequency noise crack followed by low-frequency ringdown.
8. `SlideStart`: Friction whoosh.
9. `SlideBoost`: High-velocity metallic air rush.
10. `SlideScrapeLoop`: Continuous ground sandpaper scrape noise.
11. `WindRushLoop`: Aerodynamic wind rush loop scaled dynamically by speed.
12. `GrappleAttach`: High-frequency mechanical wire impact click.
13. `GrappleFling`: Spring-loaded tension release snap.

---

## 13. UI & HUD

- **Engine Canvas [V]:** `AbilityHUD_Canvas` (Screen Space Overlay) containing UGUI components and immediate-mode `OnGUI()` rendering pipelines.
- **Crosshair Reticle [V]:**
  - Center Dot: $6\times 6\text{ px}$ square.
  - 4 Dynamic Brackets: Expand and contract based on target priority and hammergun drawback.
  - Dynamic Colors: White (Default), Solar Gold (Critical Ready), Electric Cyan (Parry/Deflect), Emerald Green (Interactable).
- **Player Health Bar [V]:** Rendered bottom-left via `OnGUI()` displaying real-time numerical health (`HP / MaxHP`) and color-coded status bar.
- **Focus Energy Bar [V]:** Rendered bottom-left directly above health bar showing real-time slow-mo energy percentage.
- **Floating Numbers [V]:** Billboarded 3D text meshes in world space:
  - Critical hits: Amber/Gold with exclamation mark.
  - Standard damage: White/Red.
  - Vampiric healing: Emerald Green (`+XX HP`).

---

## 14. PLAYER RESOURCES & GAMEPLAY ECONOMY

1. **Player Health [V]:**
   - Base: 100 HP. Authoritative component: `PlayerHealth.cs`.
   - Micro-invulnerability window: $0.05\text{s}$ to debounce physics frame overlap.
   - Deduplication dictionary: Tracks `attackId -> timestamp` with a $0.35\text{s}$ debounce window.
   - Vampiric Kill Healing:
     - Swarmer: $+10\text{ HP}$
     - Sentry: $+18\text{ HP}$
     - Relic Sniper: $+30\text{ HP}$
     - Hulk Leaper: $+45\text{ HP}$
2. **Focus Energy Meter [V]:**
   - Pool: 100 units. Drain rate: $25\text{ units/s}$ (4.0s total duration).
   - Recovery delay: $0.75\text{s}$. Base recovery: $15\text{ units/s}$ (recharges full in ~6.6s).
   - Kinetic Siphon: Moving $> 6\text{ m/s}$ accelerates recharge up to $+12\text{ units/s}$ additional.
   - Cooldown: $0\text{s}$ lockout (fluid toggle on/off).

---

## 15. PLAYER-RELATED ASSET INVENTORY

```
Assets/
├── _Project/
│   ├── Scripts/Player/PlayerMovement.cs          [V] (Locomotion source)
│   ├── Scripts/Player/PlayerAbilityInput.cs      [V] (Input source)
│   ├── Scripts/Player/MouseLook.cs               [V] (Camera source)
│   ├── Scripts/Player/PlayerHealth.cs            [V] (Health source)
│   ├── Scripts/Player/PlayerAudioEffects.cs       [V] (Audio synthesizer)
│   ├── Scripts/Player/AbilityHUD.cs              [V] (HUD source)
│   ├── Scripts/Player/PlayerMovementTelemetry.cs [V] (Telemetry source)
│   ├── Scripts/Combat/KineticHammergun.cs        [V] (Primary weapon source)
│   ├── Scripts/Abilities/PullAbility.cs          [V] (Grapple/Pull source)
│   ├── Scripts/Abilities/PushAbility.cs          [V] (Push source)
│   └── Scripts/Abilities/SlowMotionAbility.cs    [V] (Focus source)
```
- **External Asset Status [V]:** The player subsystem contains **0 external 3D FBX models, 0 external texture files, and 0 external audio WAV/MP3 files**. All geometry is constructed from Unity primitives and all audio is procedurally synthesized.

---

## 16. ENEMY & ENVIRONMENT INTEGRATION CONTRACTS

### Integration Protocols

```
+--------------------------------------------------------------------------------+
|                             INTERACTION CONTRACTS                              |
+--------------------------------------------------------------------------------+
| 1. IKineticTarget Interface:                                                   |
|    - void OnPushed(Vector3 direction, float force, Vector3 point);             |
|    - void OnPulled(Transform origin, float force);                             |
|    - void OnPhysicsCrush(float impactSpeed, GameObject prop);                  |
|    - void OnPlayerSlideRam(float slideSpeed);                                  |
|    Implemented by: SwarmerEnemy, HulkLeaperEnemy, SentryEnemy, SniperEnemy,   |
|                    InteractableProp, ExplosiveProp                             |
+--------------------------------------------------------------------------------+
| 2. IDeflectableProjectile Interface:                                           |
|    - bool CanBeDeflected { get; }                                              |
|    - bool TryDeflect(Vector3 newDirection, GameObject newOwner);               |
|    Implemented by: EnemyProjectile                                             |
+--------------------------------------------------------------------------------+
| 3. IDamageReceiver Interface:                                                  |
|    - void TakeDamage(float amount, string source, int attackId);               |
|    Implemented by: PlayerHealth                                                |
+--------------------------------------------------------------------------------+
| 4. Physics Layer Mask Conventions:                                             |
|    - Layer 8: Player                                                           |
|    - Layer 9: Environment (Static walls, floor, pillars)                       |
|    - Layer 10: Interactables (Crates, Barrels, Tables, Buttons)                |
|    - Layer 12: Enemies (Hitbox capsules and spheres)                           |
|    - Layer 13: Projectiles (Sentry bolts, Sniper rounds)                       |
+--------------------------------------------------------------------------------+
```

---

## 17. PLAYER GAMEPLAY LOOP & STATE MANAGEMENT

1. **Spawn Phase [V]:** `Player_Rig` spawns at `currentCheckpointPos` (Y: 7.6m on `Chamber1_StartHub` or Colosseum floor).
2. **Control Engagement [V]:** `PlayerAbilityInput` locks the mouse cursor on first click. `MouseLook` takes exclusive control of camera pitch/yaw.
3. **Locomotion Phase [V]:** Input drives horizontal velocity. Releasing input applies immediate $35\text{ m/s}²$ braking. Holding C at $> 7\text{ m/s}$ enters sliding state.
4. **Combat & Weapons Phase [V]:**
   - LMB holds drawback on `KineticHammergun`, releasing on sweet spot fires 70-damage piercing slug.
   - RMB targets geometry to launch elastic grapple tether, or targets props to reel them in.
   - If prop is reeled into front pocket, LMB converts to Crate Cannon launch.
5. **Death & Reset Loop [V]:** When HP reaches 0, `PlayerHealth.Die()` triggers:
   - Resets grapple tether and drops held objects.
   - Restores real-time timeScale (1.0).
   - Zeroes physics linear and angular velocity.
   - Relocates player to `currentCheckpointPos`.
   - Restores HP to 100.

---

## 18. TECHNICAL QUALITY & IDENTIFIED PROBLEMS

| Finding ID | Severity | Class | Description | Evidence | Impact | Owner |
|---|---|---|---|---|---|---|
| `FIND-PLY-01` | High | Confirmed [V] | No Player Prefab exists in the project assets; player must be procedurally generated via Editor script. | Absence of `Player_Rig.prefab` in `Assets/_Project/Prefabs/` | Risk of scene drift and broken references across multiple developer scenes. | Player |
| `FIND-PLY-02` | High | Confirmed [V] | Dual UI Architecture: `AbilityHUD.cs` runs UGUI Canvas components AND immediate-mode `OnGUI()` simultaneously. | `AbilityHUD.cs:385` contains full `OnGUI` fallback drawing routines. | Potential layout inconsistencies across different screen resolutions and performance overhead. | Player |
| `FIND-PLY-03` | Medium | Confirmed [V] | Redundant/Overlapping Abilities: `PushAbility` and `KineticHammergun` compete for Left-Click functionality. | `PlayerAbilityInput.cs:101-118` branches between Hammergun, Crate Cannon, and Push. | Push ability is rendered partially obsolete by the new primary weapon. | Player |
| `FIND-PLY-04` | Medium | Confirmed [V] | Direct Component Coupling: `PlayerMovement` directly accesses `PullAbility`, `MouseLook`, and `PlayerAudioEffects` rather than using decoupled events. | `PlayerMovement.cs:127-129` serialized references. | High coupling makes modular refactoring difficult. | Player |
| `FIND-PLY-05` | Low | Confirmed [V] | Visual Primitive Placeholders: Hands and arms are primitive cylinders without animation rigs. | `PlayerRigBuilder.cs:265-283` creates primitives. | Limits visual immersion and weapon feel. | Player |

---

## 19. RUNTIME & TESTING STATUS

- **Automated Test Suites [V]:**
  - `Assets/_Project/Scripts/Editor/MovementPhysicsTests.cs` (13 unit tests covering jump impulse, Quake air strafing, slide drag, slope surfing, grapple velocity decomposition, and emergency speed caps).
  - `Assets/_Project/Scripts/Editor/CombatPhysicsTests.cs` (16 unit tests covering Crate Cannon velocity inheritance, kinetic impact damage, focus time dilation coupling, hammergun timing, and vampiric healing).
- **Compiler Health [V]:** Roslyn compiler passes with **0 errors** (`Temp/build_check.ps1`).
- **Telemetry Infrastructure [V]:** Real-time on-screen telemetry overlay exists (`PlayerMovementTelemetry.cs`, toggled via F3) measuring speed, state, tether distance, and impulse delta-V.

---

## 20. RELEVANT FILE INDEX & SUMMARY

- Locomotion Engine: `Assets/_Project/Scripts/Player/PlayerMovement.cs`
- Hardware Input: `Assets/_Project/Scripts/Player/PlayerAbilityInput.cs`
- First-Person Camera: `Assets/_Project/Scripts/Player/MouseLook.cs`
- Authoritative Health: `Assets/_Project/Scripts/Player/PlayerHealth.cs`
- Primary Firearm: `Assets/_Project/Scripts/Combat/KineticHammergun.cs`
- Grapple & Manipulator: `Assets/_Project/Scripts/Abilities/PullAbility.cs`
- Wind Blast: `Assets/_Project/Scripts/Abilities/PushAbility.cs`
- Time Dilation: `Assets/_Project/Scripts/Abilities/SlowMotionAbility.cs`
- Audio Engine: `Assets/_Project/Scripts/Player/PlayerAudioEffects.cs`
- UI & HUD: `Assets/_Project/Scripts/Player/AbilityHUD.cs`
- Builder Tool: `Assets/_Project/Scripts/Editor/PlayerRigBuilder.cs`
- Wave Manager: `Assets/_Project/Scripts/Combat/CombatWaveManager.cs`
