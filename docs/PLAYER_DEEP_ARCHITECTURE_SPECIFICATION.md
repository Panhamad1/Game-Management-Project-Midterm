# REBOUND — Complete Player Subsystem Architecture & Engineering Specification

This document is the master engineering reference for the **Player Subsystem** in **REBOUND**.

Every component, physical calculation, collision interaction, state machine, and design trade-off is explained in full detail. The document uses a **Two-Layer Communication Pattern**:
1. **Plain-English Mental Model First**: How the system works intuitively in simple, everyday language.
2. **Explicit Senior Architectural Terms**: The precise industry-standard terminology, math formulas, and code mechanics.

---

## Table of Contents

1. [High-Level Architecture & Core Philosophy](#1-high-level-architecture--core-philosophy)
2. [Physics Rig Anatomy & Component Hierarchy](#2-physics-rig-anatomy--component-hierarchy)
3. [Locomotion Subsystem (PlayerMovement.cs)](#3-locomotion-subsystem-playermovementcs)
   - [3.1 Calibrated Speed Hierarchy & Speed Cap](#31-calibrated-speed-hierarchy--speed-cap)
   - [3.2 Low-Profile Momentum Slide](#32-low-profile-momentum-slide)
   - [3.3 Wall-Running State Machine](#33-wall-running-state-machine)
   - [3.4 Wall-Hang Cling (1.5s Window)](#34-wall-hang-cling-15s-window)
   - [3.5 Wall-Climb Jump Vault (+9.5m Elevation)](#35-wall-climb-jump-vault-95m-elevation)
   - [3.6 Additive Zig-Zag Wall-Jump Impulse](#36-additive-zig-zag-wall-jump-impulse)
   - [3.7 Overhead Clearance & Dynamic Capsule Sizing](#37-overhead-clearance--dynamic-capsule-sizing)
   - [3.8 Vertical Jump & Gravity Scaling](#38-vertical-jump--gravity-scaling)
4. [First-Person Sensory Camera Engine (MouseLook.cs)](#4-first-person-sensory-camera-engine-mouselookcs)
   - [4.1 Dual-Axis Aiming & Pitch Clamping](#41-dual-axis-aiming--pitch-clamping)
   - [4.2 Dynamic Speed Field of View (FOV)](#42-dynamic-speed-field-of-view-fov)
   - [4.3 High-Speed Turn Banking (Roll Tilt)](#43-high-speed-turn-banking-roll-tilt)
   - [4.4 Natural Head-Bob & Landing Compression Dip](#44-natural-head-bob--landing-compression-dip)
5. [Input Dispatching Engine (PlayerAbilityInput.cs)](#5-input-dispatching-engine-playerabilityinputcs)
6. [Kinetic Air Cannon & Bullet Deflection (PushAbility.cs)](#6-kinetic-air-cannon--bullet-deflection-pushabilitycs)
   - [6.1 Surface Proximity & Recoil Boost](#61-surface-proximity--recoil-boost)
   - [6.2 3.8-Meter Parry Cone & Bullet Deflection](#62-38-meter-parry-cone--bullet-deflection)
   - [6.3 Focus Energy Refuel (+20 Focus)](#63-focus-energy-refuel-20-focus)
7. [Kinetic Pull & Pocket Carry Subsystem (PullAbility.cs)](#7-kinetic-pull--pocket-carry-subsystem-pullabilitycs)
   - [7.1 18-Meter Kinetic Tractor Beam](#71-18-meter-kinetic-tractor-beam)
   - [7.2 Bottom-Right Carry Pocket Offset](#72-bottom-right-carry-pocket-offset)
   - [7.3 Anti-Screen-Obstruction Guarantee](#73-anti-screen-obstruction-guarantee)
   - [7.4 Bullet Pull Exclusion (Parry Preservation)](#74-bullet-pull-exclusion-parry-preservation)
8. [Kinetic Prop Cannon Subsystem](#8-kinetic-prop-cannon-subsystem)
9. [Chrono-Dilation Slow Motion (SlowMotionAbility.cs)](#9-chrono-dilation-slow-motion-slowmotionabilitycs)
   - [9.1 25% Physics Time Scale Dilation](#91-25-physics-time-scale-dilation)
   - [9.2 Unscaled Player Aim & Responsiveness](#92-unscaled-player-aim--responsiveness)
   - [9.3 12px Edge Accent Framing (Crisp Center Screen)](#93-12px-edge-accent-framing-crisp-center-screen)
10. [Egyptian Prop Hierarchy & Ambient Mass Auras (InteractableProp.cs)](#10-egyptian-prop-hierarchy--ambient-mass-auras-interactablepropcs)
11. [Enemy Physical Combat Reactions](#11-enemy-physical-combat-reactions)
12. [The 180-Meter Egyptian Academy (EgyptianTutorialBuilder.cs)](#12-the-180-meter-egyptian-academy-egyptiantutorialbuildercs)
13. [Master Physical Constants & Parameters Reference](#13-master-physical-constants--parameters-reference)

---

## 1. High-Level Architecture & Core Philosophy

### Plain-English Mental Model
In **REBOUND**, the player is not an armed soldier with guns. The player is an agile parkour runner who manipulates air pressure and kinetic props:
- You walk, sprint, slide under low stone gates, wall-run across bottomless pits, latch onto sheer walls, and vault up high cliffs.
- If you push air forward, your body is kicked backward with rocket recoil.
- If an enemy fires plasma bolts at you, pushing at the right second parries the bolt straight back at them.
- If you pull a heavy rock, it tucks safely into your lower right pocket so your screen stays completely clear.
- If you press Z, the world slows down to 25% speed while your aim and movement stay 100% full speed.

### Senior Architectural Definition
The subsystem implements a **Deterministic State-Machine Parkour & Kinetic Impulse Architecture**.
Ground locomotion is governed by friction-damped directional forces; wall-running and wall-hangs operate on surface-normal projections; and combat abilities emit physical impulses and deflection parry volumes rather than hitscan gun traces.

```text
[Hardware Input] -> [PlayerAbilityInput]
                           │
       ┌───────────────────┼───────────────────┐
       ▼                   ▼                   ▼
[PlayerMovement]      [MouseLook]       [Ability Controllers]
  (Walk/Sprint/Slide/   (FOV/Bob/Dips)    ├─ PushAbility (Air Cannon & Deflection)
   WallHang/Vault)                        ├─ PullAbility (Pocket Carry)
       │                   │              └─ SlowMotionAbility (Z Key Focus)
       ▼                   ▼                   │
 [PhysX Engine] <─── [Tactical HUD] <──────────┘
```

---

## 2. Physics Rig Anatomy & Component Hierarchy

### The Separation of Concerns
Camera logic and physics collision are strictly decoupled across a 4-tier tree to prevent camera jitter:

```text
Player_Rig [Root GameObject]
 │  - Transform (Position in World Space)
 │  - Rigidbody (Mass = 75kg, Interpolation = Interpolate, Constraints = FreezeRotationX | FreezeRotationZ)
 │  - CapsuleCollider (Radius = 0.5m, Height = 2.0m standing / 1.15m sliding)
 │  - PlayerMovement.cs (Main parkour solver, runs in FixedUpdate)
 │  - PlayerAbilityInput.cs (Input reader, runs in Update)
 │  - PlayerHealth.cs (Player health pool and hit feedback)
 │
 ├── GroundCheck [Child Anchor]
 │    - Transform (Local Position: Y = -1.0m, bottom center of capsule)
 │    - Ground detection sensor (SphereOverlap & Raycast probe)
 │
 └── CameraPivot [Eye Level Anchor]
      │  - Transform (Local Position: Y = 1.65m standing / Y = 1.05m sliding)
      │  - MouseLook.cs (Smooth yaw/pitch rotation, head-bob, landing dip)
      │  - SpeedCameraController.cs (Dynamic FOV scaling)
      │
      ├── MainCamera [Rendering Camera]
      │    - Camera (Universal Additional Camera Data)
      │    - AudioListener
      │
      ├── AbilityRayOrigin [Aiming Sensor Point]
      │    - Transform (Local Position: 0.35m in front of camera lens)
      │    - Eliminates parallax raycast clipping with player capsule
      │
      ├── CarryPocketAnchor [Prop Anchor Point]
      │    - Transform (Local Position: X = 0.42m, Y = -0.32m, Z = 1.35m)
      │    - Seats carried props in lower-right corner without blocking crosshair
      │
      ├── LeftHandVisual [Push & Deflect Arm]
      │    - PushAbility.cs (Air blast & 3.8m deflection parry cone)
      │
      └── RightHandVisual [Pull & Pocket Arm]
           - PullAbility.cs (18m tractor beam into CarryPocketAnchor)
```

---

## 3. Locomotion Subsystem (PlayerMovement.cs)

### 3.1 Calibrated Speed Hierarchy & Speed Cap
Locomotion speeds are calibrated for responsive, deliberate platforming control:
- **Walk Speed**: `3.2 m/s` (Precise positioning and obstacle alignment).
- **Sprint Speed**: `5.4 m/s` (Activated by holding `Left Shift`).
- **Slide Speed**: `4.8 m/s` (Activated by pressing `Ctrl` while moving).
- **Absolute Speed Cap**: `9.0 m/s` (Prevents physics tunneling and keeps camera controls readable).

### 3.2 Low-Profile Momentum Slide
When the player presses `Ctrl` while moving:
- Capsule height drops from `2.0m` to `1.15m` (Center Y shifts to `-0.425m`).
- Camera smoothly transitions downward by `0.6m`.
- Allows sliding cleanly under `1.2m` low stone gates (Chamber 1).
- Pushing (`LMB`) while sliding redirects recoil forward into slide momentum.

### 3.3 Wall-Running State Machine
When airborne and moving forward along a vertical sandstone wall (`Vector3.Dot(wallNormal, Vector3.up) < 0.1f`):
- `isWallRunning` engages automatically.
- Gravity pull is reduced to a gentle slide, allowing smooth horizontal runs along walls across chasm gaps (Chamber 2).

### 3.4 Wall-Hang Cling (1.5s Window)
When leaping directly into a sheer wall face:
- `StartWallHang(wallNormal)` activates.
- Downward fall rate slows to a controlled `0.35 m/s` cling.
- Window lasts a strict `1.5 seconds`, giving the player time to assess the ledge above.

### 3.5 Wall-Climb Jump Vault (+9.5m Elevation)
While in a wall-hang cling, pressing `Space` executes `ExecuteWallClimbJump()`:
- Injects an explosive upward velocity impulse:
  $$\vec{v}_{\text{climb}} = \text{Vector3.up} \cdot 9.5 \text{ m/s} + \text{cameraForward} \cdot 3.8 \text{ m/s}$$
- Vaults the player vertically onto high upper ledges (Chamber 3).
- Consumes the wall-hang state so the player cannot climb infinitely.

### 3.6 Additive Zig-Zag Wall-Jump Impulse
While wall-running, pressing `Space` along with directional input (`A` or `D`) executes `ExecuteWallJump()`:
- Injects an additive impulse pushing away from the wall:
  $$\vec{v}_{\text{jump}} = \vec{n}_{\text{wall}} \cdot 10.5 \text{ m/s} + \text{Vector3.up} \cdot 8.0 \text{ m/s} + \vec{w}_{\text{forward}} \cdot 7.5 \text{ m/s}$$
- Enables rhythmic zig-zag jumping back and forth across wide chasms.

### 3.7 Overhead Clearance & Dynamic Capsule Sizing
Before allowing the player to stand up from a slide:
- `Physics.SphereCast(headPos, 0.4m, Vector3.up, 0.95m)` checks for ceilings.
- If blocked by a low gate or duct, the capsule remains crouched at `1.15m` until the obstacle is cleared.

### 3.8 Vertical Jump & Gravity Scaling
- Normal jump impulse: `8.7 m/s` (Yields 1.6m vertical ground jump).
- Downward gravity multiplier: `2.4x` during normal fall for snappy platforming.

---

## 4. First-Person Sensory Camera Engine (MouseLook.cs)

### 4.1 Dual-Axis Aiming & Pitch Clamping
- Mouse X rotates the player root transform (Yaw).
- Mouse Y rotates the `CameraPivot` (Pitch clamped strictly between `-85.0°` and `+85.0°`).

### 4.2 Dynamic Speed Field of View (FOV)
Camera FOV expands smoothly based on movement speed:
$$\text{FOV} = \text{Lerp}(85.0^\circ, 105.0^\circ, \text{Clamp01}(v / 9.0))$$

### 4.3 High-Speed Turn Banking (Roll Tilt)
Strafing applies a subtle `±2.5°` camera roll tilt, enhancing athletic feel without causing disorientation.

### 4.4 Natural Head-Bob & Landing Compression Dip
- **Head-Bob**: Subtle vertical oscillation timed with walk/sprint pace. Horizontal sway is zeroed to keep crosshair aim stable.
- **Landing Dip**: Hard landings produce a brief downward compression dip (up to 0.12m) that springs back smoothly over 0.2 seconds.

---

## 5. Input Dispatching Engine (PlayerAbilityInput.cs)

Mapped directly to PC keyboard and mouse:
- `W / A / S / D`: Directional movement.
- `Left Shift`: Sprint / Run (`5.4 m/s`).
- `Ctrl`: Crouch / Momentum Slide (`4.8 m/s`).
- `Space`: Jump / Wall-Climb Vault (when hanging) / Zig-Zag Wall-Jump (when wall-running).
- `Q`: Dash.
- `Z`: Chrono-Dilation Slow Motion toggle.
- `Left Click (LMB)`: Push air cannon, Prop Cannon fire, or Bullet Deflection parry.
- `Right Click (RMB)`: Pull & Pocket Carry (tractors props into lower-right pocket).
- `R`: Instant checkpoint respawn.

---

## 6. Kinetic Air Cannon & Bullet Deflection (PushAbility.cs)

### 6.1 Surface Proximity & Recoil Boost
- Emits a pneumatic blast with 12m forward reach.
- Firing within 3.8m of a solid floor launches the player into a rocket jump (4.5m to 6.0m vertical clearance).
- Cooldown: `0.45 seconds`.

### 6.2 3.8-Meter Parry Cone & Bullet Deflection
When Push is triggered, `TryDeflect()` sweeps an overlap sphere of radius `3.8m` plus a forward cone:
- Intercepts incoming enemy plasma bolts and sniper projectiles.
- Reverses projectile trajectory directly toward player aim:
  $$\vec{v}_{\text{reflected}} = \vec{d}_{\text{aim}} \cdot 35.0 \text{ m/s}$$
- Multiplies damage by `2.0x`, enlarges projectile collider, and adds an electric cyan trail.
- One-shots or heavily damages the attacking enemy.

### 6.3 Focus Energy Refuel (+20 Focus)
Every successfully deflected projectile instantly restores **+20 Focus Energy** to `SlowMotionAbility`, rewarding aggressive, timed parries.

---

## 7. Kinetic Pull & Pocket Carry Subsystem (PullAbility.cs)

### 7.1 18-Meter Kinetic Tractor Beam
Holding `RMB` activates a focused suction beam:
- Reaches up to 18.0 meters.
- Targets Egyptian props (Urns, Boulders, Slabs).
- Reels the target smoothly toward the player using spring-damper physics.

### 7.2 Bottom-Right Carry Pocket Offset
When an object is reeled in, it snaps to `CarryPocketAnchor`:
- Local offset: `(X = 0.42m, Y = -0.32m, Z = 1.35m)` relative to the camera.
- Seats the prop neatly in the lower-right corner of the screen.

### 7.3 Anti-Screen-Obstruction Guarantee
Because carried props sit in the bottom-right carry pocket rather than directly in front of the lens, the center reticle and forward crosshair view remain **100% unobstructed**.

### 7.4 Bullet Pull Exclusion (Parry Preservation)
`PullAbility.cs` explicitly ignores enemy bullets and projectiles. Projectiles cannot be vacuum-held; they must be parried using Push (`LMB`), preserving distinct ability identity.

---

## 8. Kinetic Prop Cannon Subsystem

While carrying any prop in the pocket, tapping `LMB` fires the **Kinetic Prop Cannon**:
- Disengages suction lock.
- Sets linear velocity directly along the camera aim vector:
  $$\vec{v}_{\text{prop}} = \vec{d}_{\text{aim}} \cdot 32.0 \text{ m/s}$$
- Smashes destructible stone gates and activates Ankh target switches.
- Knocks back enemies, flings Swarmers, and staggers Hulk Leapers.

---

## 9. Chrono-Dilation Slow Motion (SlowMotionAbility.cs)

### 9.1 25% Physics Time Scale Dilation
Pressing `Z` engages Slow Motion Focus:
- `Time.timeScale = 0.25f` (world physics runs at quarter-speed).
- Synchronizes `Time.fixedDeltaTime = 0.005f` to prevent physics tunneling.

### 9.2 Unscaled Player Aim & Responsiveness
Camera mouse look, input polling, and ability firing execute using `Time.unscaledDeltaTime`. Player aim remains 100% responsive and instantaneous.

### 9.3 12px Edge Accent Framing (Crisp Center Screen)
The center viewport is kept 100% clear, sharp, and uncolored. A subtle `12px` electric border accent and status readout frame the screen edges so tactical vision is never compromised.

---

## 10. Egyptian Prop Hierarchy & Ambient Mass Auras (InteractableProp.cs)

Props have distinct physical masses and emit subtle, constant ambient point-light auras (`intensity = 0.55`, `range = 2.6m`):

| Prop Name | Mass | Material / Appearance | Aura Color | Gameplay Function |
|---|---|---|---|---|
| **Scarab Urn** | `1.5 kg` | Carved Jade Urn | **Mint Jade** (`#5eead4`) | Agile rapid throw, button activator |
| **Sandstone Boulder** | `12.0 kg` | Ancient Relic / Sandstone Rock | **Amber Gold** (`#f59e0b`) | Standard kinetic missile, bowls Swarmers |
| **Obsidian Slab** | `45.0 kg` | Massive Volcanic Slab | **Molten Ruby** (`#f43f5e`) | Heavy kinetic battering ram, staggers Hulk |

---

## 11. Enemy Physical Combat Reactions

1. **Swarmer Enemy (`SwarmerEnemy.cs`)**:
   - Rushes in packs.
   - Being struck by thrown props flings the body through the air (`linearVelocity = knockbackDir * (speed * 1.6f) + Vector3.up * 4.2f`).
   - Has a **65% chance** of an electric plasma explosion on death, damaging nearby enemies.
2. **Hulk Leaper Enemy (`HulkLeaperEnemy.cs`)**:
   - High leap and ground smash.
   - Ground smash produces a camera screen shake for players within a **20-meter radius** (intensity attenuates with distance).
   - Taking prop hits applies red hit flashes, floating damage numbers, and physical stagger impulse.
3. **Sentry Turret (`SentryEnemy.cs`)**:
   - Fires rapid plasma bursts.
   - Bolts can be deflected with Push (`LMB`) within the 3.8m parry cone back to destroy the turret.
4. **Sniper Enemy (`SniperEnemy.cs`)**:
   - Emits a 1.5-second red telegraph laser before firing a heavy sonic bolt.
   - Deflecting the bolt returns it along the laser sightline to one-shot the sniper.

---

## 12. The 180-Meter Egyptian Academy (EgyptianTutorialBuilder.cs)

Procedurally built by `EgyptianTutorialBuilder.cs` (`Tools → REBOUND → Build 6-Chamber Tutorial Academy (Complete)`):

| Chamber | Range | Environment & Layout | Core Lesson Taught |
|---|---|---|---|
| **Chamber 1** | 0m - 30m | Sandstone corridor with 1.2m low stone gate | Locomotion (Walk 3.2, Sprint 5.4) & Low-gate slide (Ctrl 4.8) |
| **Chamber 2** | 30m - 60m | Void chasm with alternating vertical panels | Wall-running & directional zig-zag wall-jumping (Space + A/D) |
| **Chamber 3** | 60m - 90m | 7m sheer cliff with elevated upper terrace | Wall-hang cling (1.5s) & vertical climb vault (+9.5m on Space) |
| **Chamber 4** | 90m - 120m | Locked gate guarded by elevated Ankh switch | Pull props into pocket (RMB) & launch at Ankh switch (LMB) |
| **Chamber 5** | 120m - 150m | Defensive corridor guarded by plasma Sentry | Slow-motion focus (Z key) & bullet deflection parry (LMB) |
| **Chamber 6** | 150m - 185m | Grand arena housing Swarmers, Hulk, Sniper & Sentry | Full combat mastery gauntlet combining all parkour and powers |
| **Void Kill Plane** | y = -8m | Spans entire 180m length | Falling instantly resets player to latest chamber checkpoint in 0.05s |

---

## 13. Master Physical Constants & Parameters Reference

| Parameter | Script | Value | Plain-English Meaning |
|---|---|---|---|
| `walkSpeed` | PlayerMovement | `3.2 m/s` | Deliberate forward walk speed |
| `sprintSpeed` | PlayerMovement | `5.4 m/s` | Sprint speed holding Left Shift |
| `slideSpeed` | PlayerMovement | `4.8 m/s` | Momentum slide speed under obstacles |
| `maxSpeed` | PlayerMovement | `9.0 m/s` | Calibrated maximum speed cap |
| `wallHangDuration` | PlayerMovement | `1.5 s` | Maximum cling time on sheer walls |
| `wallHangSlideSpeed` | PlayerMovement | `0.35 m/s` | Gentle downward drift rate during wall-hang |
| `wallClimbJumpForce` | PlayerMovement | `9.5 m/s` | Vertical takeoff impulse for climb vault |
| `wallJumpForce` | PlayerMovement | `10.5 m/s` | Push force off wall during zig-zag wall-jump |
| `wallJumpUpForce` | PlayerMovement | `8.0 m/s` | Upward lift during zig-zag wall-jump |
| `pushCooldown` | PushAbility | `0.45 s` | Delay between kinetic air blasts |
| `deflectConeRadius` | PushAbility | `3.8 m` | Overlap sphere sweep radius for bullet parries |
| `deflectBoltSpeed` | PushAbility | `35.0 m/s` | Velocity of deflected plasma bolts |
| `pullRange` | PullAbility | `18.0 m` | Maximum reach to snatch Egyptian props |
| `pocketOffset` | PullAbility | `(0.42, -0.32, 1.35)` | Offset seating props in lower-right corner |
| `propLaunchSpeed` | PullAbility | `32.0 m/s` | Projectile speed of Kinetic Prop Cannon |
| `slowMoTimeScale` | SlowMotionAbility | `0.25x` | World speed during Focus (quarter-speed) |
| `hulkSmashShakeDist` | HulkLeaperEnemy | `20.0 m` | Maximum distance for ground smash camera shake |
| `swarmerExplodeChance` | SwarmerEnemy | `65%` | Probability of electric plasma death explosion |
