# REBOUND: Parkour Locomotion, Kinetic Combat & Egyptian Arena Specification

- **Date**: 15 November 2026
- **Status**: Approved
- **Target Engine**: Unity 6000.3.8f1 (URP 17.3.0, Input System 1.18.0)
- **Author**: Antigravity Assistant & Senior Gameplay Systems Architect

---

## 1. Executive Summary & Goals

Transform REBOUND from an overly floaty, high-speed grapple sandbox into a tight, visceral, first-person parkour combat game set in a compact Egyptian arena:
1. **Locomotion Rebalance**: Reduce base player speeds by ~35-40% to bring the game into a grounded, readable range.
2. **Parkour Mechanics**: Implement Wall-Running, Wall-Jumping, Omnidirectional Dashing, Dash-to-Slide chaining, and procedural camera bobbing/tilting.
3. **Ability Specialization**: Remove grappling hook traversal entirely; focus the Tether/Pull ability exclusively on ripping, holding, and launching physics props.
4. **Authoritative Health & Visceral Feedback**: Implement `PlayerHealth` (HUD bar, damage vignette, camera shake) and `EnemyHealth` (emission hit flash, floating damage numbers, impact audio).
5. **Enemy Polish**: Harmonize enemy velocity with player speed and upgrade projectile visuals with bright plasma glows and readable bullet trails.
6. **Compact Egyptian Arena**: Replace oversized geometry with a 45m x 45m Egyptian courtyard packed with vertical wall-run surfaces and 30+ interactive physics props.

---

## 2. Subsystem Specifications

### 2.1 Movement & Parkour (`PlayerMovement.cs` & `MouseLook.cs`)
- **Tuned Speed Baseline**:
  - Walk: 5.0 m/s (down from 6.5 m/s)
  - Sprint: 8.5 m/s (down from 11.5 m/s)
  - Crouch Walk: 3.0 m/s
  - Slide Speed: 10.5 m/s max entry, decaying to crouch walk
  - Max Hard Speed Cap: 16.0 m/s (down from 26.0 m/s)
- **Omnidirectional Dash**:
  - Keybind: `Left Shift` tap or dedicated Dash key (`Q` / `C`)
  - Impulse: 9.0 m/s horizontal impulse in the current WASD movement direction
  - Cooldown: 1.2 seconds with HUD cooldown indicator
- **Dash-to-Slide Combo**:
  - Pressing or holding `Ctrl` while dashing immediately converts forward dash momentum into a ground slide with reduced friction (`slideFriction = 1.4`).
- **Wall-Running & Wall-Jumping**:
  - Detection: Left and right horizontal raycasts (length 1.1m) querying wall geometry.
  - Activation: In-air + moving forward + against a runnable wall surface (>75 deg incline).
  - Behavior: Gravity reduced to 15%, player glides forward along wall tangent for up to 1.8 seconds.
  - Wall Jump: Pressing `Space` while wall running executes an outward + upward impulse (6.5 m/s up, 7.5 m/s away from wall normal).
- **First-Person Camera Juice (`MouseLook.cs`)**:
  - Head Bobbing: Sinusoidal translation when moving on ground (frequency 8Hz on walk, 12Hz on sprint; amplitude 0.04m).
  - Dynamic Tilt / Roll: Camera tilts 8-10 degrees into the wall during wall-runs, and 4 degrees during slides.

### 2.2 Kinetic Ability Refinement (`PullAbility.cs` & `PushAbility.cs`)
- **Disable Traversal Grapple**:
  - Disable world surface grapple raycasting in `PullAbility.cs`.
  - Player cannot latch cables onto walls, ceilings, or towers to fling themselves across the map.
- **Weaponized Prop Manipulation**:
  - Targeting spherecast exclusively grabs objects implementing `InteractableProp` or `IKineticCarryTarget`.
  - Tether pulls prop smoothly toward player hold position (1.5m in front of camera).
  - Pressing `Left Click` (Push) while carrying launches the held prop at 28 m/s with full physics crush damage on impact.

### 2.3 Health & Combat Feedback Pipeline
- **Player Health (`PlayerHealth.cs`)**:
  - Max Health: 100 HP.
  - Damage Receiver: Implements `IDamageReceiver` with attack ID deduplication (prevents multiple frame damage ticks from the same projectile/attack).
  - Damage FX: Full-screen red flash overlay via UGUI Canvas, directional camera punch shake.
- **Enemy Health & Visuals**:
  - Universal health tracking (`currentHealth`, `maxHealth`).
  - `EnemyHitFlash`: Changes material emission to glowing red/white for 0.1s upon taking damage.
  - `FloatingDamageNumber`: Spawns 3D world-space text mesh popping upward with damage value (white for standard, gold for critical prop impacts, emerald for heals).
  - SFX: Impact thump audio on prop crush and projectile hit.

### 2.4 Enemy Visuals & Tuning
- **Enemy Projectiles (`EnemyProjectile.cs`)**:
  - Visuals: Bright emissive material with line renderer trail.
  - Velocity: Calibrated to 18-22 m/s so player can dodge with parkour or deflect with Push.
- **Enemy Speeds**:
  - Swarmer: 6.5 m/s pursuit (tuned to match new player sprint).
  - Hulk Leaper: 12 m/s leap with clear 0.75s anticipation glow.

### 2.5 Egyptian Arena Architecture (`EgyptianArenaSetup.cs`)
- **Footprint**: 45m x 45m enclosed courtyard bounded by 8m-tall sandstone walls.
- **Features**:
  - Central 6m stepped pyramid with flat ledges.
  - 4 perimeter Sandstone obelisks (3m x 3m x 10m) aligned for continuous wall-running loops.
  - 2 wall-run gantries along opposing walls.
  - 30+ interactive physics props (Sandstone blocks, wooden crates, explosive oil jars) distributed across cover points.
- **Materials**: Reuses existing pyramid materials (`Sandstone.mat`, `ScarabJade.mat`, `RoyalLapis.mat`, `Obsidian.mat`, `Brass.mat`).

---

## 3. Success Criteria & Verification
1. `Assembly-CSharp.dll` and `Assembly-CSharp-Editor.dll` compile with 0 errors.
2. Player speeds match spec: Walk 5.0 m/s, Sprint 8.5 m/s, Max 16.0 m/s.
3. Wall-running attaches cleanly to walls and wall-jumping launches outward.
4. Dash and Dash+Ctrl slide function reliably with cooldown.
5. Grapple cable no longer latches to static environment; prop pull and launch remain 100% functional.
6. Player and enemies take damage; red hit flashes and floating damage numbers appear on hit.
7. Egyptian arena instantiates in Scene with dense props and parkour geometry.
