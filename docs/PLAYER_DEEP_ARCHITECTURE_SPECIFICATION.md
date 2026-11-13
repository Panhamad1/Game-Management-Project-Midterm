# REBOUND — Complete Player Subsystem Architecture & Engineering Specification

This document is the exhaustive, master-level engineering reference for the **Player Subsystem** in **REBOUND**.

Every single component, mathematical calculation, physics interaction, state machine, and design trade-off is explained in full detail. The document uses a **Two-Layer Communication Pattern**:
1. **Plain-English Mental Model First**: How the system works intuitively in simple, everyday language.
2. **Explicit Senior Architectural Terms**: The precise industry-standard terminology, math formulas, and code mechanics.

---

## Table of Contents

1. [High-Level Architecture & Core Philosophy](#1-high-level-architecture--core-philosophy)
2. [Physics Rig Anatomy & Component Hierarchy](#2-physics-rig-anatomy--component-hierarchy)
3. [Locomotion Subsystem (PlayerMovement.cs)](#3-locomotion-subsystem-playermovementcs)
   - [3.1 Speed Hierarchy & Decoupled Crouch](#31-speed-hierarchy--decoupled-crouch)
   - [3.2 Power Slide Dynamics & Aerodynamic Drag](#32-power-slide-dynamics--aerodynamic-drag)
   - [3.3 Downhill Slope Surfing Physics](#33-downhill-slope-surfing-physics)
   - [3.4 Continuous Hold-Space Auto-Bunny-Hop](#34-continuous-hold-space-auto-bunny-hop)
   - [3.5 Vector-Projection Air-Strafing (Quake/Source Engine)](#35-vector-projection-air-strafing-quakesource-engine)
   - [3.6 Slide Bowling Ramming](#36-slide-bowling-ramming)
   - [3.7 Vertical Jump & Gravity Scaling](#37-vertical-jump--gravity-scaling)
   - [3.8 Coyote Time & Input Buffering](#38-coyote-time--input-buffering)
   - [3.9 Overhead Clearance & Dynamic Capsule Sizing](#39-overhead-clearance--dynamic-capsule-sizing)
   - [3.10 Ledge Mantling & Ground Snapping](#310-ledge-mantling--ground-snapping)
4. [First-Person Sensory Camera Engine (MouseLook.cs)](#4-first-person-sensory-camera-engine-mouselookcs)
   - [4.1 Dual-Axis Aiming & Pitch Clamping](#41-dual-axis-aiming--pitch-clamping)
   - [4.2 Dynamic Speed Field of View (FOV)](#42-dynamic-speed-field-of-view-fov)
   - [4.3 High-Speed Turn Banking (Roll Tilt)](#43-high-speed-turn-banking-roll-tilt)
   - [4.4 Natural Head-Bob & Landing Compression Dip](#44-natural-head-bob--landing-compression-dip)
   - [4.5 Directional Screen Shake Matrix](#45-directional-screen-shake-matrix)
5. [Input Dispatching Engine (PlayerAbilityInput.cs)](#5-input-dispatching-engine-playerabilityinputcs)
6. [Pneumatic Wind Cannon Subsystem (PushAbility.cs)](#6-pneumatic-wind-cannon-subsystem-pushabilitycs)
   - [6.1 Surface Proximity & Inverse-Square Recoil](#61-surface-proximity--inverse-square-recoil)
   - [6.2 3D Rocket Jump Physics & Vertical Safety Cap](#62-3d-rocket-jump-physics--vertical-safety-cap)
   - [6.3 Wall Kicks & Directional Deflection](#63-wall-kicks--directional-deflection)
   - [6.4 Slide-Recoil Forward Velocity Conversion](#64-slide-recoil-forward-velocity-conversion)
7. [Dual-Mode Grapple Winch & Vortex Subsystem (PullAbility.cs)](#7-dual-mode-grapple-winch--vortex-subsystem-pullabilitycs)
   - [7.1 Dual-Mode Target Arbitration Algorithm](#71-dual-mode-target-arbitration-algorithm)
   - [7.2 Gravity Gun Prop Snatching & Riot Shielding](#72-gravity-gun-prop-snatching--riot-shielding)
   - [7.3 75-Meter Long-Range Winch & SphereCast Aim](#73-75-meter-long-range-winch--spherecast-aim)
   - [7.4 Elastic Winch Pull & Centrifugal Swing Physics](#74-elastic-winch-pull--centrifugal-swing-physics)
   - [7.5 Downward Grapple Auto-Slide](#75-downward-grapple-auto-slide)
   - [7.6 Slingshot Tension Fling](#76-slingshot-tension-fling)
   - [7.7 Line of Sight & Obstruction Filtering](#77-line-of-sight--obstruction-filtering)
8. [Crate Cannon Projectile Subsystem](#8-crate-cannon-projectile-subsystem)
9. [Zero-Cooldown Focus Time Dilation (SlowMotionAbility.cs)](#9-zero-cooldown-focus-time-dilation-slowmotionabilitycs)
10. [Dynamic 4-State Tactical HUD (AbilityHUD.cs)](#10-dynamic-4-state-tactical-hud-abilityhudcs)
11. [Zero-File Procedural Audio Synthesis Engine (PlayerAudioEffects.cs)](#11-zero-file-procedural-audio-synthesis-engine-playeraudioeffectscs)
12. [Interactive Props & Target Switches (InteractableProp.cs & TargetButton.cs)](#12-interactive-props--target-switches-interactablepropcs--targetbuttoncs)
13. [The IKineticTarget Code Contract](#13-the-ikinetic-target-code-contract)
14. [Automated Editor Setup & The 200m Arena (PlayerRigBuilder.cs)](#14-automated-editor-setup--the-200m-arena-playerrigbuildercs)
15. [Master Physical Constants & Parameters Reference](#15-master-physical-constants--parameters-reference)
16. [Defense Q&A: Architectural Rationales](#16-defense-qa-architectural-rationales)

---

## 1. High-Level Architecture & Core Philosophy

### Plain-English Mental Model
In **REBOUND**, the player is not an armed soldier with a rifle. The player is a **kinetic air-pressure engine**. 
Every action follows Newton's laws of motion:
- If you push air forward, your body is pushed backward.
- If you slide down a hill, gravity makes you go faster.
- If you jump out of a slide, your speed does not vanish into the ground; it stays in your body while you fly through the air.
- If you grapple a wall, an elastic cable pulls you like a slingshot.

### Senior Architectural Definition
The subsystem implements a **Deterministic Momentum Preservation Locomotion Architecture**. 
Unlike standard character controllers that enforce hard velocity clamping (e.g., standard `CharacterController.Move` which snaps velocity to a constant walk speed every frame), REBOUND implements a hybrid **kinematic-impulse physics model**:
- Ground movement is governed by friction-damped directional forces.
- Aerial and slide states are governed by momentum conservation, vector projection on normal planes, and energy stacking.
- Weapons and abilities are physical force emitters rather than hitscan bullet raycasts.

```text
[Hardware Input] -> [PlayerAbilityInput]
                           │
       ┌───────────────────┼───────────────────┐
       ▼                   ▼                   ▼
[PlayerMovement]      [MouseLook]       [Ability Controllers]
  (Forces/Velocities)   (FOV/Banking)     ├─ PushAbility (Wind Cannon)
       │                   │              ├─ PullAbility (Winch & Vortex)
       │                   │              └─ SlowMotionAbility (Time Scale)
       ▼                   ▼                   │
 [PhysX Engine] <─── [Tactical HUD] <──────────┘
```

---

## 2. Physics Rig Anatomy & Component Hierarchy

### The Separation of Concerns
In first-person games, placing camera logic and physics collision on the same GameObject causes camera tumbling, jitter, and physics solver fighting. REBOUND strictly decouples them across a 4-tier tree:

```text
Player_Rig [Root GameObject]
 │  - Transform (Position in World Space)
 │  - Rigidbody (Mass = 75kg, Interpolation = Interpolate, Constraints = FreezeRotationX | FreezeRotationZ)
 │  - CapsuleCollider (Radius = 0.5m, Height = 2.0m standing / 1.15m crouched)
 │  - PlayerMovement.cs (Main physics solver, runs in FixedUpdate)
 │  - PlayerAbilityInput.cs (Input reader, runs in Update)
 │  - PlayerAudioEffects.cs (Audio synthesizer, runs on AudioSource)
 │
 ├── GroundCheck [Child Anchor]
 │    - Transform (Local Position: Y = -1.0m, bottom center of capsule)
 │    - Ground detection sensor (SphereOverlap & Raycast probe)
 │
 └── CameraPivot [Eye Level Anchor]
      │  - Transform (Local Position: Y = 1.65m standing / Y = 1.05m crouched)
      │  - MouseLook.cs (Smooth yaw/pitch rotation, FOV widening, camera tilt)
      │
      ├── MainCamera [Rendering Camera]
      │    - Camera (Universal Additional Camera Data, Dynamic FOV 85° - 105°)
      │    - AudioListener (Global 3D audio listener)
      │
      ├── AbilityRayOrigin [Aiming Sensor Point]
      │    - Transform (Local Position: 0.35m in front of camera lens)
      │    - Eliminates parallax and prevents raycasts from colliding with player's own head
      │
      ├── LeftHandVisual [Push Arm]
      │    - Cylinder Mesh (Local Position: Left side)
      │    - PushAbility.cs (Wind Cannon & Shockwave line renderer)
      │
      ├── RightHandVisual [Pull Arm]
      │    - Cylinder Mesh (Local Position: Right side)
      │    - PullAbility.cs (Grapple cable LineRenderer & Vortex rings)
      │
      └── AbilityHUD_Canvas [Screen Space UI]
           - Canvas (Screen Space - Overlay)
           - AbilityHUD.cs (Smart Reticle, Target Badge, Speedometer)
```

### Critical Physics Solver Settings:
1. **Rigidbody Constraints (`FreezeRotationX | FreezeRotationZ`)**: The X and Z rotational axes are locked. The physics engine is strictly prohibited from tilting the player like a domino when colliding with edges. The Y axis (yaw) is controlled programmatically.
2. **Rigidbody Interpolation (`RigidbodyInterpolation.Interpolate`)**: Eliminates micro-stutter by smoothly interpolating the visual transform between physics `FixedUpdate` ticks (50 Hz) and rendering `Update` frames (144+ Hz).
3. **Collision Detection (`CollisionDetectionMode.Continuous`)**: Essential for high-speed parkour. Prevents the player from tunneling through walls or floors when traveling at maximum speed (35 m/s).

---

## 3. Locomotion Subsystem (PlayerMovement.cs)

### 3.1 Speed Hierarchy & Decoupled Crouch

#### The Problem:
Many FPS controllers trigger a slide whenever the crouch key is pressed while moving forward, causing players to accidentally slide when trying to make careful, quiet movements.

#### The Solution:
REBOUND enforces a strict **Decoupled Kinetic State Machine**:

```text
[Movement Input Detected]
       │
       ├─ If NOT sprinting OR Speed < 6.0 m/s ──> [Crouch-Walk State]
       │                                           Speed: 3.5 m/s
       │                                           Height: 1.15m
       │                                           Slide: False
       │
       └─ If SPRINTING (Shift) AND Speed >= 6.0 m/s ──> [Power Slide State]
                                                         Speed: 14.0 - 22.0 m/s
                                                         Height: 1.15m
                                                         Slide: True
```

- **Walk (`walkSpeed = 6.5 m/s`)**: Base tactical movement.
- **Sprint (`sprintSpeed = 11.5 m/s`)**: Activated by holding `Left Shift`.
- **Crouch-Walk (`crouchSpeed = 3.5 m/s`)**: Activated by pressing `C` or `Ctrl` while walking or stationary.
- **Power Slide**: Requires forward sprint momentum (`moveInput.y > 0.1f` AND `CurrentSpeed >= 6.0 m/s`). Enters at `Mathf.Max(CurrentSpeed + 4.5f, 14.0f)`.

---

### 3.2 Power Slide Dynamics & Aerodynamic Drag

When the player enters a slide, ground friction is substantially reduced, and a custom **Quadratic Aerodynamic Drag Model** is simulated in `FixedUpdate`:

$$\text{Total Deceleration} = \mu_{\text{kinetic}} + (v \cdot C_{\text{drag}})$$

Where:
- $\mu_{\text{kinetic}} = 2.2 \text{ m/s}^2$ (Flat ground contact friction).
- $C_{\text{drag}} = 0.035$ (Aerodynamic drag coefficient proportional to current velocity).
- $v = \text{Current Slide Speed}$.

#### Minimum Tactile Window:
To prevent jittery on-off slide cancels when rapidly tapping keys, `minSlideDuration = 0.35s` guarantees that once initiated, a slide maintains momentum for at least 350 milliseconds before transitioning back to standing.

---

### 3.3 Downhill Slope Surfing Physics

When sliding on angled surfaces, the player's slide velocity is dynamically projected onto the ground slope plane:

$$\vec{v}_{\text{slide}} = \text{ProjectOnPlane}(\vec{d}_{\text{slide}}, \vec{n}_{\text{ground}})$$

The acceleration due to gravity along the slope is calculated using the sine of the slope angle:

$$a_{\text{slope}} = (\vec{d}_{\text{slide}} \cdot \vec{d}_{\text{downhill}}) \cdot g_{\text{slope}} \cdot \sin(\theta)$$

Where:
- $\theta = \text{Angle between Ground Normal and World Up Vector}$.
- $g_{\text{slope}} = 28.0 \text{ m/s}^2$ (Slope gravity acceleration multiplier).
- Sliding downhill adds massive forward speed up to the soft cap of **35.0 m/s**.
- Sliding uphill naturally decelerates the player based on gravity resistance.

---

### 3.4 Continuous Hold-Space Auto-Bunny-Hop

#### Mental Model:
In classic movement games (Quake, Counter-Strike, Titanfall), chaining bunny hops requires clicking Space within a 1-frame window upon touching the ground. If you miss, ground friction instantly halts your speed. In REBOUND, you simply **hold down Space**.

#### Architecture:
`PlayerMovement.cs` evaluates ground status every physics tick:
```csharp
bool canJump = (Time.time - lastGroundedTime <= coyoteTime);
bool wantsJump = (Time.time - lastJumpPressedTime <= jumpBufferTime) || isJumpHeld;
```
If `isJumpHeld` is true:
1. The exact physics frame `isGrounded` becomes true, `canJump && wantsJump` executes immediately.
2. Ground friction is completely bypassed (0 frames spent on ground at rest).
3. The player's existing horizontal momentum is preserved 100%.
4. A bunny-hop speed injection (`bHopBoost = +2.5 m/s`) is added directly to current horizontal speed.

---

### 3.5 Vector-Projection Air-Strafing (Quake/Source Engine)

#### Mental Model:
When flying through the air, turning your mouse left while holding `A` doesn't just turn your view—it actually bends your trajectory and pushes you faster through the air.

#### Senior Mathematical Mechanics:
The system implements the classic Quake/Source **Wish Direction Projection**:

```text
       Velocity Vector (Current Momentum)
              ─────────────►
                   \     │
                    \    │  Dot Product Projection
                     \   │
                      ▼  ▼
                 Wish Direction (Input Vector)
```

1. Let $\vec{v}_h$ be the current horizontal velocity.
2. Let $\vec{w}$ be the normalized wish direction from `WASD` input.
3. Calculate current projection: $P = \vec{v}_h \cdot \vec{w}$.
4. Let $V_{\text{maxWish}} = 16.0 \text{ m/s}$ be the maximum allowed strafe wish speed.
5. Calculate add speed: $A_{\text{add}} = \max(0, V_{\text{maxWish}} - P)$.
6. If $A_{\text{add}} > 0$:
   $$\vec{v}_h = \vec{v}_h + \vec{w} \cdot \min(A_{\text{add}}, a_{\text{air}} \cdot \Delta t)$$
7. The velocity vector is then rotated toward the wish direction proportionally to turning rate:
   $$\vec{v}_h = \text{RotateTowards}\left(\vec{v}_h, \vec{w}, \left(\frac{a_{\text{air}} \cdot 1.6}{\max(\|\vec{v}_h\|, 6.0)}\right) \cdot \Delta t\right)$$

**Result**: Every smooth air curve adds **+1.5 to +3.5 m/s** to the player's speed, rewarding rhythmic parkour mastery.

---

### 3.6 Slide Bowling Ramming

When `isSliding == true` and `CurrentSpeed >= 7.5 m/s`:
- In `OnCollisionEnter(Collision collision)`, the script probes for the `IKineticTarget` interface on the hit object.
- If present, calls `target.OnPlayerSlideRam(CurrentSpeed)`.
- Destructible wooden barricades shatter instantly.
- Physics crates and enemy swarmers are dealt collision impulse damage and launched backward without arresting the player's slide momentum.

---

### 3.7 Vertical Jump & Gravity Scaling

#### Jump Calculation:
Jumping applies an instantaneous vertical velocity:
$$\vec{v}_y = \text{jumpImpulse} = 6.6 \text{ m/s}$$
Using the kinematic formula $h = \frac{v_y^2}{2g}$, this produces a clean **1.6-meter jump** (matching standard curb and platform clearances).

#### Asymmetric Gravity Scaling:
To eliminate floaty, unnatural physics, REBOUND uses a dual gravity scale:
- **Standard Gravity (`standardGravityScale = 2.4x`)**: World gravity is multiplied by 2.4 during normal descent, giving a snappy, tactile fall.
- **Slow Motion Gravity (`slowMoGravityScale = 0.5x`)**: World gravity is multiplied by 0.5 during Focus mode, granting extended mid-air hang time to aim abilities and line up crate cannon shots.

---

### 3.8 Coyote Time & Input Buffering

1. **Coyote Time (`coyoteTime = 0.15s`)**: When running off a ledge or platform, the player has a 150-millisecond grace period where pressing Jump still executes a valid jump, preventing frustrating falls.
2. **Jump Buffering (`jumpBufferTime = 0.15s`)**: If the player presses Jump up to 150 milliseconds before touching the ground, the input is queued and fires the exact millisecond the feet make contact.

---

### 3.9 Overhead Clearance & Dynamic Capsule Sizing

- **Capsule Dimensions**:
  - Standing: Height = 2.0m, Center Y = 0.0m (Feet anchored at local Y = -1.0m).
  - Crouched / Sliding: Height = 1.15m, Center Y = -0.425m.
- **Overhead Clearance Sensor**:
  Before allowing the player to stand up from a slide or crouch, an upward `Physics.SphereCast` (radius = 0.4m, distance = 0.95m) probes the ceiling. If a low duct or ceiling is detected, the player is forced to remain crouched until clearing the obstacle, preventing camera clipping through geometry.

---

### 3.10 Ledge Mantling & Ground Snapping

#### Dual-Raycast Ledge Mantling:
When airborne and falling (`rb.linearVelocity.y < 1.0 m/s`), two forward raycasts execute:
1. **Chest Raycast**: Cast forward at chest level (Y = +0.2m). Detects if a wall is in front of the player.
2. **Ledge Top Downward Raycast**: If a wall is hit, a raycast shoots downward from 0.8m above the wall to find the exact flat top surface.
3. If a flat ledge is detected, the player's vertical velocity is arrested and smoothly pulled up onto the ledge (`Mathf.MoveTowards`), allowing effortless vaulting over obstacles.

#### Safe Platform Spawning (`EnsureSafeSpawnPosition`):
To prevent players from ever spawning in mid-air or falling through elevated map geometry:
1. Shoots a raycast down from Y = +50.0m to locate the highest solid platform.
2. Snaps the capsule so its feet (`feetLocalY = -1.0m`) rest exactly on the surface (`hit.point.y + 1.1m`).
3. Updates the active checkpoint position idempotently.

---

## 4. First-Person Sensory Camera Engine (MouseLook.cs)

### 4.1 Dual-Axis Aiming & Pitch Clamping
- Mouse delta X rotates the entire `Player_Rig` transform around the Y axis (Yaw).
- Mouse delta Y rotates only the `CameraPivot` around the X axis (Pitch).
- Pitch angle is strictly clamped:
  $$\theta_{\text{pitch}} = \text{Clamp}(\theta_{\text{pitch}} - \Delta y \cdot S, -85.0^\circ, +85.0^\circ)$$
- Eliminates gimbal lock and stops the camera from tumbling upside down.

### 4.2 Dynamic Speed Field of View (FOV)
As the player accelerates, camera FOV dynamically expands:
$$\text{FOV} = \text{Lerp}(\text{baseFOV}, \text{maxFOV}, \text{Clamp01}(v / 30.0))$$
- Base FOV: **85.0°** (at rest).
- Max FOV: **105.0°** (at 30.0 m/s).
- Gives intense visual speed sensation without motion sickness.

### 4.3 High-Speed Turn Banking (Roll Tilt)
When strafing or turning at high velocity, the camera applies a subtle roll tilt:
$$\text{Target Roll} = -\text{moveInput.x} \cdot 2.5^\circ$$
The camera rolls up to **±2.5 degrees**, interpolating smoothly with `bankSpeed = 8.0`.

### 4.4 Natural Head-Bob & Landing Compression Dip
- **Head-Bob**: Vertical sinusoidal oscillation timed with movement speed. Horizontal sway is strictly zeroed to avoid throwing off crosshair aiming.
- **Landing Dip**: When falling with high downward velocity, `OnLand` calculates an impact compression dip (up to 0.15m downward) that smoothly springs back over 0.2 seconds, conveying physical mass.

### 4.5 Directional Screen Shake Matrix
Implements a trauma-based directional screen shake:
- Trauma value decays linearly over time: $\text{trauma} = \max(0, \text{trauma} - \Delta t \cdot 1.5)$.
- Shake offset: $\vec{O} = \text{trauma}^2 \cdot \vec{d}_{\text{shake}} \cdot \text{Random}(-1, 1)$.
- Used for rocket jumps, crate launches, grapple snaps, and target impacts.

---

## 5. Input Dispatching Engine (PlayerAbilityInput.cs)

Built using Unity's new Input System package (`UnityEngine.InputSystem`), polling hardware at the start of `Update`:
- Dispatches continuous vectors (`WASD`) to `PlayerMovement.SetMoveInput`.
- Streams mouse delta to `MouseLook.SetLookInput`.
- Dispatches instantaneous button triggers:
  - `Left Click`: Push or Crate Cannon launch.
  - `Right Click`: Pull or Grapple Winch start/stop.
  - `Space`: Jump press and continuous `SetJumpHoldInput(isPressed)`.
  - `C` / `Ctrl`: Crouch / Slide input.
  - `Q` / `F`: Slow Motion toggle.
  - `R`: Instant safe platform respawn.
  - `Escape`: Cursor lock toggle.

---

## 6. Pneumatic Wind Cannon Subsystem (PushAbility.cs)

### 6.1 Surface Proximity & Inverse-Square Recoil
The Wind Cannon does not fire physical bullets; it emits a pneumatic pressure blast:
- **Blast Range**: 12.0 meters.
- **Target Impulse**: 18.0 N·s applied to hit rigidbodies or `IKineticTarget` objects.

#### The Proximity Rule (Newton's Third Law):
To prevent players from flying infinitely through open air:
- If fired in **open air (> 3.8m from any surface)**, player recoil is **strictly 0.0 m/s**.
- If fired within **surfaceProximityDistance = 3.8m** of a solid floor or wall, player recoil is calculated:
  $$\text{Recoil Speed} = v_{\text{kick}} \cdot \left(0.8 + \left(1.0 - \frac{d}{3.8}\right) \cdot 0.4\right)$$
  Where $v_{\text{kick}} = 13.5 \text{ m/s}$. Blasting closer to a surface delivers a stronger launch.

### 6.2 3D Rocket Jump Physics & Vertical Safety Cap
- Aiming down at the floor and firing launches the player **4.5 to 6.0 meters high**.
- **Airborne Safety Cap**: A boolean `hasAirRocketJumped` permits a maximum of **1 upward rocket jump while airborne**. Subsequent airborne blasts cancel the vertical component while preserving horizontal redirection, stopping players from hovering infinitely like a helicopter.

### 6.3 Wall Kicks & Directional Deflection
When blasting a vertical wall (`hit.normal.y < 0.6f`), the recoil vector blends the reverse aim direction with the wall surface normal:
$$\vec{d}_{\text{recoil}} = \text{Normalize}(\text{Lerp}(-\vec{d}_{\text{aim}}, \vec{n}_{\text{wall}}, 0.5))$$
This deflects the player away from the wall, allowing wall-kick redirection across gaps.

### 6.4 Slide-Recoil Forward Velocity Conversion
If the player fires Push while actively sliding (`isSliding == true`):
- Instead of kicking backward, the recoil impulse is redirected forward along the player's slide trajectory:
  $$\vec{v}_{\text{slide}} = \vec{v}_{\text{slide}} + (\text{transform.forward} \cdot 10.0 \text{ m/s})$$
- Adds an immediate **+10.0 m/s speed boost**, launching slides up to the 35 m/s speed cap!

---

## 7. Dual-Mode Grapple Winch & Vortex Subsystem (PullAbility.cs)

### 7.1 Dual-Mode Target Arbitration Algorithm

When Right Click (`RMB`) is pressed, `AcquireTarget()` determines whether to snatch an object or grapple a wall:

```text
[RMB Pressed]
      │
      ├─ Priority 1: Raycast within 16.0m against LayerMask "Interactables"
      │    │
      │    ├─ Hit Movable Prop / Crate? ──> [START CRATE SNATCH]
      │    │                                Reeled to hover 2.5m in front of hands
      │    │                                Acts as laser riot shield
      │    │
      │    └─ No Prop Found? ──> Proceed to Priority 2
      │
      └─ Priority 2: Raycast + SphereCast (0.55m) within 75.0m against "Environment"
           │
           ├─ Hit Wall, Tower, Floor, or Spire? ──> [START GRAPPLE WINCH]
           │                                         55 m/s² elastic tether pull
           │                                         Centrifugal swing physics
           │
           └─ No Geometry Found? ──> Idle
```

### 7.2 Gravity Gun Prop Snatching & Riot Shielding
- Reeled crates smoothly hover at `hoverDistance = 2.5m` in front of the camera using a spring-damper tracking force:
  $$\vec{F}_{\text{pull}} = (\vec{P}_{\text{target}} - \vec{P}_{\text{crate}}) \cdot 10.0$$
- Angular damping is set to 6.0 to stop objects from spinning wildly.
- **Riot Shielding**: The carried crate physically occupies space and has an active collider, absorbing enemy sniper rounds and rapid-laser fire.
- **Anti-Box-Riding Safeguard**: If the player's feet are touching the crate (`groundedObject == prop.transform`), pulling is prohibited, preventing infinite self-levitation exploits.

### 7.3 75-Meter Long-Range Winch & SphereCast Aim
- **Range**: **75.0 meters** (calibrated for high towers, open chasms, and 36m spires).
- **Parallax-Free Raycasting**: Ray origin is placed at the exact camera optical center (`playerCamera.transform.position`).
- **SphereCast Fallback**: If a razor-thin raycast misses a distant metal beam or pillar edge, a secondary `Physics.SphereCast` (radius = 0.55m) sweeps the view cone, effortlessly locking onto thin grapple anchors without tedious pixel-hunting.

### 7.4 Elastic Winch Pull & Centrifugal Swing Physics

#### The Problem:
Linear velocity overrides (`rb.linearVelocity = toAnchor * speed`) destroy sideways momentum, making swinging feel like a robotic elevator instead of an athletic swing.

#### The Elastic Winch Solver:
REBOUND implements an inward acceleration vector that preserves perpendicular velocity:
1. Calculate inward direction: $\vec{u} = \text{Normalize}(\vec{P}_{\text{anchor}} - \vec{P}_{\text{player}})$.
2. Calculate inward pull acceleration: $\vec{a}_{\text{pull}} = \vec{u} \cdot (55.0 \text{ m/s}^2 \cdot \Delta t)$.
3. Update velocity: $\vec{v}_{\text{new}} = \vec{v}_{\text{current}} + \vec{a}_{\text{pull}}$.
4. Calculate inward velocity component: $v_{\text{inward}} = \vec{v}_{\text{new}} \cdot \vec{u}$.
5. Cap inward speed to target speed ($24.0 - 35.0 \text{ m/s}$) while leaving tangential velocity untouched:
   $$\text{If } v_{\text{inward}} > v_{\text{target}} \implies \vec{v}_{\text{new}} = \vec{v}_{\text{new}} - \vec{u} \cdot (v_{\text{inward}} - v_{\text{target}})$$

**Result**: The player swings around pillars and towers in wide, thrilling centrifugal arcs like Spider-Man or Pathfinder!

### 7.5 Downward Grapple Auto-Slide
If the grapple anchor is on the ground or a downward ramp in front of the player (`pullDir.y <= 0.25f`), the system immediately engages `StartSlide(Mathf.Max(CurrentSpeed + 4.0f, targetSpeed))`, pulling the player directly into an explosive **24.0 to 34.0 m/s power slide**.

### 7.6 Slingshot Tension Fling
Releasing `RMB` or tapping `Space` mid-grapple executes `FlingGrapple`:
1. Disconnects cable.
2. Injects a spring tension boost:
   $$v_{\text{boosted}} = \min(\max(v_{\text{current}}, 16.0) + 6.5 \text{ m/s}, 35.0 \text{ m/s})$$
3. Blends current momentum direction with camera forward direction:
   $$\vec{d}_{\text{fling}} = \text{Normalize}(\text{Slerp}(\vec{d}_{\text{momentum}}, \vec{d}_{\text{aim}}, 0.70))$$
4. Launches the player forward and upward into high aerial bunny hops.

### 7.7 Line of Sight & Obstruction Filtering
To prevent cables from snapping on the very surface they are attached to:
- A probe shoots from `camPos + 0.8m` to `grapplePoint + normal * 0.35m`.
- If an obstruction collider is hit, the script verifies:
  $$\text{If } (\text{obsHit.collider} \neq \text{grappleCollider}) \implies \text{Break Tether}$$
- Eliminates false-positive disconnects while preserving realistic wall breaks when swinging behind buildings.

---

## 8. Crate Cannon Projectile Subsystem

While holding a crate with Pull (`RMB`), pressing Left Click (`LMB`) fires the **Crate Cannon**:
1. Releases the object from the suction tether.
2. Sets rigidbody linear velocity directly to:
   $$\vec{v}_{\text{projectile}} = \vec{d}_{\text{aim}} \cdot 35.0 \text{ m/s}$$
3. Spawns procedural audio launch punch and air hiss.
4. Applies directional recoil kick to the player (`-aimDir * 6.5 m/s`).
5. Trajectory Arc: While holding the crate, a 20-step `LineRenderer` simulates gravity-influenced parabolic projectile flight, showing the player exactly where the block will land.
6. Target Impact: Upon impact with enemies or barricades, calls `target.OnPhysicsCrush(impactSpeed, prop)` dealing heavy kinetic shatter damage.

---

## 9. Zero-Cooldown Focus Time Dilation (SlowMotionAbility.cs)

### Time Scale Dilation:
- World physics slowed to quarter-speed: `Time.timeScale = 0.25f`.
- Physics tick rate synchronized to prevent tunneling:
  $$\text{Time.fixedDeltaTime} = 0.02 \cdot 0.25 = 0.005\text{s}$$

### Unscaled Architecture:
Mouse aiming, camera look, input polling, and audio synthesis run on `Time.unscaledDeltaTime`, ensuring the player retains 100% fast, responsive control while enemies and obstacles crawl in slow motion.

### Zero Cooldown & Infinite Meter:
- `infiniteSlowMo = true`: The Focus meter does not drain over time.
- Players can toggle Slow Motion on or off at any time using `Q` or `F` with zero cooldown and zero wait penalty.
- Perspective is crystal-clear: Camera FOV expands by +8 degrees, and all screen-obscuring haze is disabled for optimal tactical visibility.

---

## 10. Dynamic 4-State Tactical HUD (AbilityHUD.cs)

The crosshair reticle automatically morphs its shape, color, and readouts based on what is in front of the player:

```text
State 1: GRAB OBJECT (Crates, Barricades, Buttons)
   Look: Amber/Yellow Square [ · ]
   Distance: <= 16.0m
   Readout: [RMB] GRAB · BLOCK_MEDIUM_5KG · 4.2m

State 2: GRAPPLE ANCHOR (Walls, Floors, Towers, Spire)
   Look: Electric Cyan Diamond +
   Distance: <= 75.0m
   Readout: [RMB] GRAPPLE · 38.5m

State 3: ARMED CRATE CANNON (Carrying Crate)
   Look: Neon Green Wide Missile Reticle
   Readout: [LMB] FIRE CRATE CANNON (35 m/s)

State 4: NEUTRAL OPEN AIR (Beyond 75m)
   Look: Translucent White Crosshair
   Readout: Hidden
```

### Dynamic Speedometer:
Renders current player velocity and movement state in real-time above the bottom gauge:
- `GRAPPLE · 28.4 m/s` (Electric Cyan)
- `SLIDE · 22.1 m/s` (Bright Green)
- `B-HOP · 18.5 m/s` (Cyan)
- `SPRINT · 11.5 m/s` (Yellow)
- `6.5 m/s` (White)

---

## 11. Zero-File Procedural Audio Synthesis Engine (PlayerAudioEffects.cs)

All sound effects are synthesized mathematically in code using runtime float buffers (`AudioClip.Create`) and frequency modulation, guaranteeing zero missing asset errors:

| Sound Effect | Math Formula & Frequency Profile | Duration |
|---|---|---|
| **Footstep Thud** | 75 Hz sine wave with rapid exponential decay ($e^{-18t}$) | 0.12s |
| **Wind Cannon Blast** | White noise burst mixed with 90 Hz sub-bass punch | 0.35s |
| **Vortex Suction** | Modulated 110 Hz turbine hum with frequency oscillation | Looping |
| **Crate Cannon Fire** | 120 Hz pneumatic impulse + high-frequency 2400 Hz hiss | 0.28s |
| **Grapple Attach** | Clean 1200 Hz metallic ping with fast decay | 0.15s |
| **Slingshot Fling** | Wide whoosh noise sweep from 400 Hz to 1800 Hz | 0.40s |
| **Focus Enter Drop** | Sub-bass pitch drop sweeping from 160 Hz down to 40 Hz | 0.45s |
| **Slide Scrape** | Filtered white noise with pitch mapped to current slide speed | Looping |
| **Target Switch Chime** | Harmonious dual-tone chime (A5 880 Hz + A6 1760 Hz) | 0.45s |

---

## 12. Interactive Props & Target Switches (InteractableProp.cs & TargetButton.cs)

### 12.1 InteractableProp.cs (Physics Crates)
Attached to movable puzzle blocks:
- **Light Crate (~1.0 kg, Cyan)**: Highly agile projectile, easy aerial handling.
- **Medium Crate (~5.0 kg, Yellow)**: Standard combat missile and riot shield.
- **Heavy Block (~25.0 kg, Red)**: Ground drag only (`hoverTarget.y = rb.position.y`), immovable cover.

### 12.2 TargetButton.cs (Interactive Arena Switches)
Interactive wall and tower buttons mounted across the arena:
- **Triggers**:
  1. Crate Cannon hit (`impactSpeed >= 4.0 m/s`).
  2. Slide Bowling Ram (`slideSpeed >= 5.5 m/s`).
  3. Direct Push blast.
- **Feedback**:
  - Core depresses physically inward by 0.18m.
  - Material turns from glowing Bright Red to glowing Neon Green.
  - Plays procedural dual-tone A5/A6 chime.
  - Triggers camera screen shake and refills Focus energy.

---

## 13. The IKineticTarget Code Contract

This interface (`Assets/_Project/Scripts/Abilities/IKineticTarget.cs`) is the shared contract between the Player Subsystem and all other team systems (Enemy AI, level puzzles):

```csharp
public interface IKineticTarget
{
    // Can this target be lifted by the vacuum vortex?
    bool CanBePulled { get; }

    // Can this target be pushed backward by wind cannon?
    bool CanBePushed { get; }

    // Called when blasted by Push (Wind Cannon)
    void OnPushed(Vector3 forceDirection, float forceMagnitude, Vector3 hitPoint);

    // Called continuously while being reeled in by Pull
    void OnPulled(Transform pullOrigin, float pullForce);

    // Called when a high-speed flying crate smashes into it
    void OnPhysicsCrush(float impactSpeed, GameObject flyingProp);

    // Called when the player slides into it at high speed (> 7.5 m/s)
    void OnPlayerSlideRam(float slideSpeed);
}
```

---

## 14. Automated Editor Setup & The 200m Arena (PlayerRigBuilder.cs)

### 1-Click Unity Setup:
- **`Tools > REBOUND > Create Redliner Grapple Arena (200m)`**: Spawns the entire 200m arena.
- **`Tools > REBOUND > Setup Complete Player Rig`**: Wires all player components, HUD, and audio.
- **`Tools > REBOUND > Snap Player to Safe Spawn`**: Relocates player to the top deck at Y = 9.6m.

### Arena Geometry Blueprint:
- **Arena Floor**: 200m x 200m, centered at (0, -0.5, 60).
- **Perimeter Walls**: 35 meters tall.
- **Central Grapple Spire**: 36m tall at (0, 18, 60) with 4 anchor brackets.
- **4 Quadrant Towers**: 28m tall at (±55, 14, 25) and (±55, 14, 95).
- **Aerial Swing Gantries**: High suspended beams at Y = 25m connecting towers across the sky.
- **Mega Surf Ramp**: 38m long at 26° incline dropping from Y = 16m.
- **Ski-Jump Kicker**: 18° upward kicker launching into bunny hops.
- **6 Target Buttons**: Mounted at Spire Apex, Towers, Sky-Deck, and launch ramps.
- **Hazard Pit Trigger**: Invisible floor at Y = -12m that resets the player instantly without scene reload.

---

## 15. Master Physical Constants & Parameters Reference

| Parameter | Script | Value | Plain-English Meaning |
|---|---|---|---|
| `walkSpeed` | PlayerMovement | `6.5 m/s` | Normal forward walking speed |
| `sprintSpeed` | PlayerMovement | `11.5 m/s` | Sprint speed holding Shift |
| `crouchSpeed` | PlayerMovement | `3.5 m/s` | Quiet stealth crouch-walk speed |
| `airAcceleration` | PlayerMovement | `26.0` | Air-strafe turn acceleration factor |
| `jumpImpulse` | PlayerMovement | `6.6 m/s` | Vertical jump launch speed (1.6m height) |
| `bHopBoost` | PlayerMovement | `+2.5 m/s` | Speed injected on every bunny-hop |
| `runThreshold` | PlayerMovement | `7.5 m/s` | Minimum speed needed to trigger a slide |
| `slideBoost` | PlayerMovement | `+4.5 m/s` | Initial speed punch added on slide entry |
| `minSlideEntrySpeed`| PlayerMovement | `14.0 m/s` | Minimum speed when slide begins |
| `minSlideSpeed` | PlayerMovement | `3.5 m/s` | Speed where slide drops to crouch-walk |
| `slideFriction` | PlayerMovement | `2.2 m/s²` | Flat ground friction deceleration |
| `slideDrag` | PlayerMovement | `0.035` | Quadratic aerodynamic drag coefficient |
| `slopeAcceleration` | PlayerMovement | `28.0 m/s²`| Downhill gravity slide acceleration |
| `maxSpeed` | PlayerMovement | `35.0 m/s` | Hard soft-cap for all momentum chaining |
| `surfaceProximityDistance` | PushAbility | `3.8 m` | Max surface distance for rocket recoil |
| `surfaceKickVelocity` | PushAbility | `13.5 m/s` | Base rocket jump velocity |
| `maxPullRange` | PullAbility | `16.0 m` | Max range to snatch crates |
| `maxGrappleRange`| PullAbility | `75.0 m` | Max reach of grapple hook cable |
| `grappleFlingBoost` | PullAbility | `+6.5 m/s` | Launch speed added on grapple release |
| `launchSpeed` | PullAbility | `35.0 m/s` | Projectile speed of Crate Cannon |
| `slowMoTimeScale`| SlowMotionAbility | `0.25x` | World speed during Focus (quarter-speed) |
| `infiniteSlowMo` | SlowMotionAbility | `true` | Zero-cooldown infinite Focus toggle |

---

## 16. Defense Q&A: Architectural Rationales

### Q1: "Why did you separate crouching from sliding?"
**Answer**: In fast movement games, accidentally sliding when trying to make precise micro-adjustments causes players to slide off ledges. By requiring sprint momentum (`CurrentSpeed >= 6.0 m/s` and `isSprinting`), walking forward and pressing `C` cleanly crouch-walks at 3.5 m/s, giving the player total control.

### Q2: "How does air-strafing make the player go faster?"
**Answer**: It implements the Quake/Source engine vector projection. In air, if the player's wish direction is perpendicular to their current velocity (strafing into a camera turn), the dot product projection is zero. This allows the air acceleration formula to continuously add speed without violating the wish speed limit, adding +1.5 to +3.5 m/s per turn up to 35 m/s.

### Q3: "Why doesn't the grapple break when aiming far away?"
**Answer**: Previously, a linecast probed from camera to anchor. Because the anchor point rests on the surface of the wall collider itself, floating-point precision caused the linecast to register the wall as an obstruction whenever distance exceeded 3m. We fixed this by filtering the linecast to ignore the hooked collider and added a 0.55m SphereCast fallback to effortlessly catch distant thin pillars.

### Q4: "Why use procedural audio synthesis instead of imported WAV files?"
**Answer**: Procedural audio generates sound directly from math waveforms in memory. This guarantees that team members pulling the git repository will never suffer from missing audio file GUIDs, broken asset paths, or silence.

### Q5: "How does combat work without guns?"
**Answer**: Combat is purely kinetic. Players snatch physics crates with Pull to use as portable riot shields against laser fire, fire carried crates at 35 m/s with Crate Cannon to crush enemies, slide-ram swarmers like bowling pins at 7.5+ m/s, and deflect incoming sniper rounds using the Wind Cannon.
