# REBOUND — Complete Player System & Gameplay Manual

Welcome to the official, complete manual for the **Player Subsystem** in **REBOUND**.

This document explains every single script, control, calculation, and game mechanic in direct, simple English. There are no confusing buzzwords. Everything is described clearly so that you, your teammates, and your professors can understand exactly how every part of the player works and how to answer any question about the project.

---

## Table of Contents

1. [Game Overview & Core Idea](#1-game-overview--core-idea)
2. [How the Player is Built in Unity (Hierarchy)](#2-how-the-player-is-built-in-unity-hierarchy)
3. [Complete Script-by-Script Breakdown](#3-complete-script-by-script-breakdown)
   - [3.1 PlayerMovement.cs — Running, Sliding, Air-Strafing & Bunny-Hopping](#31-playermovementcs--running-sliding-air-strafing--bunny-hopping)
   - [3.2 MouseLook.cs — Smooth Aiming, Camera Banking & Landing Dips](#32-mouselookcs--smooth-aiming-camera-banking--landing-dips)
   - [3.3 PlayerAbilityInput.cs — Keyboard & Mouse Control Listener](#33-playerabilityinputcs--keyboard--mouse-control-listener)
   - [3.4 PushAbility.cs — Wind Cannon, Rocket Jumps & Wall Kicks](#34-pushabilitycs--wind-cannon-rocket-jumps--wall-kicks)
   - [3.5 PullAbility.cs — Dual-Mode Grapple Hook & Crate Snatching](#35-pullabilitycs--dual-mode-grapple-hook--crate-snatching)
   - [3.6 SlowMotionAbility.cs — Zero-Cooldown Focus Time Dilation](#36-slowmotionabilitycs--zero-cooldown-focus-time-dilation)
   - [3.7 AbilityHUD.cs — 4-State Smart Crosshair, Target Badges & Speedometer](#37-abilityhudcs--4-state-smart-crosshair-target-badges--speedometer)
   - [3.8 PlayerAudioEffects.cs — Zero-File Procedural Sound Generator](#38-playeraudioeffectscs--zero-file-procedural-sound-generator)
   - [3.9 InteractableProp.cs — Physics Crates & Weight Classes](#39-interactablepropcs--physics-crates--weight-classes)
   - [3.10 TargetButton.cs — Interactive Wall & Tower Impact Switches](#310-targetbuttoncs--interactive-wall--tower-impact-switches)
   - [3.11 IKineticTarget.cs — Code Contract for Enemies and Targets](#311-ikinetic-targetcs--code-contract-for-enemies-and-targets)
   - [3.12 PlayerRigBuilder.cs — 1-Click Unity Setup & Arena Generator](#312-playerrigbuildercs--1-click-unity-setup--arena-generator)
4. [Master Movement & Physics Guide](#4-master-movement--physics-guide)
   - [Walking vs Crouching vs Power Sliding](#41-walking-vs-crouching-vs-power-sliding)
   - [Downhill Slope Surfing](#42-downhill-slope-surfing)
   - [Holding Space for Auto-Bunny-Hop](#43-holding-space-for-auto-bunny-hop)
   - [Source / Redliner Air-Strafing](#44-source--redliner-air-strafing)
   - [Slide Bowling Ram](#45-slide-bowling-ram)
   - [Dual-Mode Grapple & Slingshot Fling](#46-dual-mode-grapple--slingshot-fling)
   - [Crate Cannon Ballistics](#47-crate-cannon-ballistics)
   - [Infinite Slow Motion (Focus)](#48-infinite-slow-motion-focus)
5. [Smart Crosshair Visual States](#5-smart-crosshair-visual-states)
6. [The 200m Redliner Grapple Arena Guide](#6-the-200m-redliner-grapple-arena-guide)
7. [How to Test in Unity (Step-by-Step)](#7-how-to-test-in-unity-step-by-step)
8. [Inspector Tuning Guide (Every Variable Explained)](#8-inspector-tuning-guide-every-variable-explained)
9. [Complete Changelog & Version History](#9-complete-changelog--version-history)

---

## 1. Game Overview & Core Idea

**REBOUND** is a high-speed first-person physics action and movement game built in Unity.

Instead of traditional firearms with bullets, your player uses pneumatic air-pressure abilities and momentum parkour:
- **Push (Wind Cannon / Left Click)**: Fires a blast of compressed air forward. It knocks crates and enemies away. Blasting near the floor launches you into the air (**Rocket Jump**). Blasting near a wall kicks you sideways (**Wall Kick**). Blasting while sliding boosts your forward speed.
- **Pull (Dual-Mode Tether / Right Click Hold)**:
  - When aimed at a crate or barrel: Sucks the object through the air to float 2.5 meters in front of your hands like a gravity gun.
  - When aimed at a wall, floor, tower, or ceiling: Shoots a 75-meter electric grapple cable that pulls your body forward with an elastic winch, allowing you to swing through the air like Roblox Redliner or Spider-Man.
- **Crate Cannon (Left Click while holding a crate)**: Fires the carried crate forward at 35 meters per second like a high-speed cannonball to smash doors, hit buttons, and crush enemies.
- **Slow Motion (Focus / Q or F)**: Slows down game world physics to quarter speed (25%), while keeping your camera aiming 100% fast and responsive. It has zero cooldown and can be turned on or off whenever you want.

---

## 2. How the Player is Built in Unity (Hierarchy)

The player GameObject is named `Player_Rig`. It is structured in a clear parent-child tree so that camera aiming, physics forces, and visual hands never glitch or fight each other:

```text
Player_Rig (Layer 8: Player, Rigidbody, CapsuleCollider, PlayerMovement, AudioEffects, Input)
 │
 ├── GroundCheck (Sphere sensor at feet level Y = -1.0 to detect floor contact)
 │
 └── CameraPivot (Eye level Y = 1.65m, MouseLook script)
      │
      ├── MainCamera (First-person camera view, AudioListener)
      │
      ├── AbilityRayOrigin (Sensor point 0.35m in front of camera lens)
      │
      ├── LeftHandVisual (Left hand cylinder, PushAbility, Shockwave Ring VFX)
      │
      ├── RightHandVisual (Right hand cylinder, PullAbility, Vortex VFX, Cable Renderer)
      │
      └── AbilityHUD_Canvas (UI: Smart Crosshair, Target Badge, Speedometer, Guide)
```

### Why It Is Set Up This Way:
1. **Capsule Collider on Root**: Only the capsule touches the floor and walls. We freeze its X and Z rotations (`FreezeRotationX | FreezeRotationZ`) so the player capsule always stands perfectly upright and never tumbles over.
2. **Camera Pivot Separated from Body**: When your body gets pushed backward by recoil, the camera smoothly glides with you without violently jerking your mouse aim.
3. **Ray Origin in Front of Face**: By casting ability rays from 0.35m in front of the camera, your own player collider will never block your own abilities.

---

## 3. Complete Script-by-Script Breakdown

### 3.1 PlayerMovement.cs — Running, Sliding, Air-Strafing & Bunny-Hopping
- **File Location**: `Assets/_Project/Scripts/Player/PlayerMovement.cs`
- **What it does**: Controls physical movement, walking, sprinting, crouching, power sliding, slope surfing, auto-bunny-hopping, air-strafing, jumping, rocket recoil, and safe respawning.
- **Three Movement Speeds**:
  - **Walk Speed**: `6.5 m/s` (normal WASD movement).
  - **Sprint Speed**: `11.5 m/s` (holding `Left Shift`).
  - **Crouch Speed**: `3.5 m/s` (holding `C` or `Ctrl` while walking).
- **Strict Slide vs. Crouch Rule**:
  - Walking forward + pressing Crouch does **NOT** slide. It smoothly ducks into a quiet crouch-walk at 3.5 m/s.
  - To enter a **Power Slide**, you must be sprinting forward (`Shift + W`) with high speed (`>= 6.0 m/s`). Pressing `C` then launches you into an explosive slide at **14.0 to 22.0 m/s**!
- **Downhill Slope Surfing**:
  - Sliding down an angled hill or ramp converts gravity into forward acceleration (`slopeAcceleration = 28 m/s²`). You can reach speeds up to the **35 m/s maximum speed cap**!
- **Hold-Space Auto-Bunny-Hop**:
  - You do not need to time your jump keys manually. Simply **hold down Space**!
  - Every single time your feet touch the ground, the script immediately executes a frame-perfect bunny-hop jump with zero friction loss.
  - Each hop out of a sprint or slide injects a bonus speed kick (`bHopBoost = +2.5 m/s`).
- **Redliner / Source Air-Strafing**:
  - While in mid-air, hold `A` or `D` and smoothly turn your mouse in that direction.
  - The script uses vector projection math (`airAcceleration = 26f`) to steer your velocity vector while adding +1.5 to +3.5 m/s of speed per turn!
- **Slide Bowling Ram**:
  - When sliding faster than `7.5 m/s`, your body acts as a battering ram. Smashing into crates, barricades, or enemies knocks them away like bowling pins.
- **Safe Platform Raycast Spawning**:
  - At the start of the game or upon respawning (`R`), the script raycasts down from the sky to find the highest platform surface and safely places your feet on top of it. You will never fall through the floor.

---

### 3.2 MouseLook.cs — Smooth Aiming, Camera Banking & Landing Dips
- **File Location**: `Assets/_Project/Scripts/Player/MouseLook.cs`
- **What it does**: Turns your mouse movements into first-person camera rotation and adds tactile camera motion.
- **Vertical Pitch Clamping**: Clamped strictly between **-85° and +85°** so your view never flips upside down.
- **Dynamic Speed FOV**: Camera Field of View widens from **85° to 105°** as your speed climbs from 0 to 30 m/s, creating a sensation of high velocity.
- **Speed Banking (Roll Tilt)**: Tilts the camera slightly (-2.5° to +2.5°) when turning sharply at high speeds.
- **Landing Dip**: When falling from high platforms, the camera compresses down slightly on impact and springs back up, making landings feel weighty.
- **Screen Shake System**: Adds directional screen shakes during rocket jumps, grapple flings, and crate explosions.

---

### 3.3 PlayerAbilityInput.cs — Keyboard & Mouse Control Listener
- **File Location**: `Assets/_Project/Scripts/Player/PlayerAbilityInput.cs`
- **What it does**: Reads raw player inputs using Unity's modern Input System and calls the appropriate functions on player scripts.
- **Full Control List**:
  - `W, A, S, D`: Walk forward, left, backward, right.
  - `Left Shift (Hold)`: Sprint forward at 11.5 m/s.
  - `C` or `Left Ctrl`: Crouch (when walking) or Power Slide (when sprinting).
  - `Space (Hold)`: Jump and continuous Auto-Bunny-Hop.
  - `Left Click (LMB)`: Fire Push (Wind Cannon) OR fire Crate Cannon (if carrying a crate).
  - `Right Click (RMB Hold)`: Pull crate to hands OR grapple static geometry.
  - `Q` or `F`: Toggle Slow Motion (Focus) on and off.
  - `R`: Instant reset back to the safe platform checkpoint.
  - `Escape`: Unlocks the mouse cursor.

---

### 3.4 PushAbility.cs — Wind Cannon, Rocket Jumps & Wall Kicks
- **File Location**: `Assets/_Project/Scripts/Abilities/PushAbility.cs`
- **What it does**: Fires compressed air bursts forward to push objects, launch rocket jumps, and kick off walls.
- **Surface Proximity Rule**:
  - Firing in **open air (> 3.8m away from walls/floors)** causes **zero recoil** to the player. You will not accidentally drift backward when pushing distant objects.
  - Firing within **3.8m of the floor** launches you into a **Rocket Jump** (4.5m to 6.0m high).
  - Firing within **3.8m of a wall** launches you into an explosive **Wall Kick / Redirection Dash** away from the wall.
  - Firing while **sliding** converts the recoil into an instant forward **Slide Boost (+10 m/s)**.
- **Airborne Rocket Jump Limit**: Maximum of 1 upward rocket jump while airborne to prevent infinite vertical climbing.

---

### 3.5 PullAbility.cs — Dual-Mode Grapple Hook & Crate Snatching
- **File Location**: `Assets/_Project/Scripts/Abilities/PullAbility.cs`
- **What it does**: Dual-mode ability that either pulls physics crates to your hands or shoots a long-range grapple hook cable to pull your body across the arena.
- **Priority 1: Crate Snatch (Within 16 Meters)**:
  - If aiming at a movable crate, barrel, or debris, it reels the object to hover 2.5 meters in front of your hands.
  - The held crate physically blocks incoming laser fire like a riot shield.
  - Left-clicking while holding the crate launches it forward at **35.0 m/s** (`launchSpeed`).
- **Priority 2: Redliner Grapple Hook (Up to 75 Meters)**:
  - If aiming at static geometry (walls, towers, ceiling, floor), it shoots an electric cyan cable up to **75.0 meters away**.
  - Uses camera-center raycasting with a generous **0.55m SphereCast fallback**, so you can easily hook thin pillars, cable beams, and distant ledges.
  - **Elastic Winch Pull**: Pulls your body toward the anchor with strong 55 m/s² spring acceleration while preserving your sideways swing momentum.
  - **Ground Grapple Auto-Slide**: Hooking the ground or a downward ramp ahead pulls you directly into an explosive **24 to 34 m/s power slide**.
  - **Slingshot Fling**: Releasing `RMB` or tapping `Space` mid-grapple snaps the cable and flings you forward with an extra **+6.5 m/s boost** directly into bunny hops or slides.

---

### 3.6 SlowMotionAbility.cs — Zero-Cooldown Focus Time Dilation
- **File Location**: `Assets/_Project/Scripts/Abilities/SlowMotionAbility.cs`
- **What it does**: Slows down the entire game world to quarter-speed (25%) so you can aim mid-air, line up crate shots, and make precision swings.
- **Zero Cooldown / Infinite Duration**:
  - `infiniteSlowMo = true`: The energy meter does not drain over time.
  - There is no waiting penalty or cooldown. You can toggle Slow-Mo on with `Q` or `F`, stay in Slow-Mo as long as you want, and turn it off whenever you want.
- **Crystal Clear Visibility**: Zero screen haze or fog. The image remains completely sharp.

---

### 3.7 AbilityHUD.cs — 4-State Smart Crosshair, Target Badges & Speedometer
- **File Location**: `Assets/_Project/Scripts/Player/AbilityHUD.cs`
- **What it does**: Renders your tactical first-person HUD, dynamic crosshair reticle, target readouts, and speedometer.
- **Dynamic 4-State Crosshair**:
  1. **Amber / Yellow Square (`[ · ]`)**: Activates when aiming at movable crates, blocks, or buttons within 16m. Crosshair expands into a box and badge displays `[RMB] GRAB · {OBJECT NAME} · {DISTANCE}m`.
  2. **Electric Cyan Diamond (`+`)**: Activates when aiming at static walls, floors, towers, and spires within 75m. Crosshair contracts into a sharp reticle and badge displays `[RMB] GRAPPLE · {DISTANCE}m`.
  3. **Neon Green Missile Reticle**: Activates while holding a crate in your hands. Badge displays `[LMB] FIRE CRATE CANNON (35 m/s)`.
  4. **Neutral White Crosshair**: Displayed when aiming at empty air beyond 75m.
- **Speedometer**: Shows your current horizontal speed in real time (e.g. `SLIDE · 24.5 m/s` or `B-HOP · 18.2 m/s`).

---

### 3.8 PlayerAudioEffects.cs — Zero-File Procedural Sound Generator
- **File Location**: `Assets/_Project/Scripts/Player/PlayerAudioEffects.cs`
- **What it does**: Generates all audio clips mathematically in code so the project has zero missing file errors.
- **Audio Catalog**:
  - Footstep thuds timed with head-bob.
  - Wind blast pneumatic punch.
  - Vacuum suction turbine hum.
  - Grapple attach ping and slingshot fling whoosh.
  - Sub-bass Slow-Mo enter and exit drops.
  - Slide surface scraping and wind rush crescendo up to 35 m/s.
  - Heavy impact crunches when crates hit walls.

---

### 3.9 InteractableProp.cs — Physics Crates & Weight Classes
- **File Location**: `Assets/_Project/Scripts/Interactables/InteractableProp.cs`
- **What it does**: Attached to every movable crate, block, and barricade.
- **3 Weight Classes**:
  1. **Light (~1.0kg, Cyan)**: Easily lifted, high projectile speed, ideal for quick shots.
  2. **Medium (~5.0kg, Yellow)**: Standard puzzle block, balance counterweight, heavy combat missile.
  3. **Heavy (~25.0kg, Red)**: Cannot be lifted into mid-air; drags along the floor. Used for heavy barricades and bowling pins.
- **Crush Damage**: Flying crates moving faster than 4.0 m/s deal high crush damage to any target they hit.

---

### 3.10 TargetButton.cs — Interactive Wall & Tower Impact Switches
- **File Location**: `Assets/_Project/Scripts/Interactables/TargetButton.cs`
- **What it does**: Large circular target switches with dark metal casings mounted across the arena.
- **How to Activate**:
  1. Hit with a crate fired by Crate Cannon (35 m/s projectile).
  2. Slide into it at high speed with Slide Bowling Ram (> 5.5 m/s).
  3. Blast it directly with Push (Wind Cannon).
- **Reaction Feedback**:
  - Core switches from glowing Bright Red to glowing Neon Green.
  - Core physically depresses inward into the casing.
  - Plays a crisp high-energy synthesized confirmation chime.
  - Shakes the camera slightly and rewards the player with instant Focus energy.

---

### 3.11 IKineticTarget.cs — Code Contract for Enemies and Targets
- **File Location**: `Assets/_Project/Scripts/Abilities/IKineticTarget.cs`
- **What it does**: The C# interface that all destructible objects, buttons, and enemy AI scripts implement so the player's abilities interact with them.
- **Interface Methods**:
  - `CanBePulled`: Can this object be lifted to the player's hands?
  - `CanBePushed`: Can this object be pushed backward?
  - `OnPushed(dir, force, hitPoint)`: Called when blasted by Wind Cannon.
  - `OnPulled(origin, force)`: Called continuously while being reeled in.
  - `OnPhysicsCrush(speed, prop)`: Called when a flying crate smashes into it.
  - `OnPlayerSlideRam(slideSpeed)`: Called when the player slides into it at high speed.

---

### 3.12 PlayerRigBuilder.cs — 1-Click Unity Setup & Arena Generator
- **File Location**: `Assets/_Project/Scripts/Editor/PlayerRigBuilder.cs`
- **What it does**: An editor tool that sets up the player and builds the testing arena with one click from Unity's top menu bar:
  - **`Tools > REBOUND > Setup Complete Player Rig`**: Spawns and configures the player, hands, HUD, and audio.
  - **`Tools > REBOUND > Create Redliner Grapple Arena (200m)`**: Spawns the massive 200m x 200m arena with the 36m Spire, 4 Towers, Gantries, Mega Ramps, and 6 Target Buttons.
  - **`Tools > REBOUND > Snap Player to Safe Spawn`**: Instantly snaps the player to the safe central platform deck.

---

## 4. Master Movement & Physics Guide

### 4.1 Walking vs Crouching vs Power Sliding
- **Walking**: Press `W, A, S, D` to move at `6.5 m/s`.
- **Crouching**: Press `C` or `Ctrl` while walking or standing still. The player ducks down to 1.15m height and crouch-walks quietly at `3.5 m/s`. It will **never** slide by accident.
- **Power Sliding**: Sprint forward (`Shift + W`) to reach at least 6.0 m/s, then press `C`. Your player drops into an explosive power slide at **14.0 to 22.0 m/s**!
- **Overhead Clearance**: If sliding under a low duct or platform (e.g. 1.3m clearance), releasing crouch will keep you safely ducked until you reach open space.

### 4.2 Downhill Slope Surfing
- When sliding down an inclined ramp or hill, gravity adds continuous forward acceleration (`28 m/s²`).
- On flat ground, slight friction slows you down gradually. On downward slopes, you accelerate continuously all the way to **35.0 m/s**!

### 4.3 Holding Space for Auto-Bunny-Hop
- In traditional movement games, bunny-hopping requires precise keyboard timing. In REBOUND, simply **hold down Space**!
- Every time your feet touch the floor, the game automatically executes a frame-perfect hop.
- Each hop preserves 100% of your horizontal speed and adds `+2.5 m/s` of bonus velocity!

### 4.4 Source / Redliner Air-Strafing
- While airborne, hold `A` (to turn left) or `D` (to turn right) and smoothly swing your mouse in that same direction.
- The game accelerates your horizontal velocity into the turn, adding **+1.5 to +3.5 m/s** on every curve. Skilled players can chain turns to maintain maximum speed across the entire arena!

### 4.5 Slide Bowling Ram
- Sliding faster than `7.5 m/s` turns your body into a wrecking ball.
- Ramming into wooden barricades shatters them instantly. Ramming into physics crates sends them scattering like bowling pins.

### 4.6 Dual-Mode Grapple & Slingshot Fling
- Hold `RMB` while aiming at any wall, tower, or ceiling up to 75 meters away.
- An electric cyan cable connects and pulls you toward the point with 55 m/s² spring tension.
- **Swing Physics**: You can swing in wide circular arcs around pillars without losing speed.
- **Ground Slingshot**: Hooking the floor in front of you pulls you straight into a 24–34 m/s power slide.
- **Slingshot Fling**: Releasing `RMB` or pressing `Space` while grappling releases the cable with a **+6.5 m/s forward launch**, flinging you into high-altitude bunny hops.

### 4.7 Crate Cannon Ballistics
- Hold `RMB` on a crate to lift it into your hands.
- Aim at a distant wall, enemy, or button. A cyan dotted line shows the flight trajectory.
- Click `LMB` to fire the crate at **35.0 m/s**. Upon impact, the crate deals heavy crush damage and shatters targets.

### 4.8 Infinite Slow Motion (Focus)
- Press `Q` or `F` at any time to enter Slow Motion.
- World physics slows down to 25% speed, giving you time to plan swings and line up shots.
- It has **zero cooldown and infinite duration**. Press `Q` or `F` again to turn it off whenever you choose.

---

## 5. Smart Crosshair Visual States

The crosshair automatically changes color and shape so you always know what your abilities will do before you click:

| State | Crosshair Look | Meaning | Action When Clicking |
|---|---|---|---|
| **Grab Object** | **Amber / Yellow Box `[ · ]`** | Aiming at a movable crate, block, or button within 16m. | `RMB`: Snatch crate to your hands. |
| **Grapple Anchor** | **Electric Cyan Diamond `+`** | Aiming at a wall, tower, ceiling, or floor within 75m. | `RMB`: Shoot grapple cable to swing. |
| **Armed Missile** | **Neon Green Wide Reticle** | You are currently holding a crate in your hands. | `LMB`: Fire Crate Cannon (35 m/s). |
| **Neutral** | **Translucent White Cross** | Aiming at open air beyond 75m. | No target in range. |

---

## 6. The 200m Redliner Grapple Arena Guide

The arena is built specifically for testing high-speed parkour, long-range grapple swinging, and crate shooting:

1. **Massive Scale**: 200 meters wide by 200 meters long, enclosed by 35-meter high walls.
2. **Central Grapple Spire**: 36 meters tall with 4 grapple anchor brackets at the summit.
3. **4 Quadrant Towers**: 28 meters tall each, positioned at the four corners of the arena.
4. **Aerial Swing Gantries**: High suspended beams at Y = 25m connecting all towers across the sky for continuous sky-swinging without touching the ground.
5. **Mega Downhill Surf Ramp**: 38 meters long at a 26-degree slope, dropping from 16m down to floor level for reaching 35 m/s max slide speed.
6. **Ski-Jump Kicker**: An 18-degree upward launch ramp for launching into aerial bunny hops.
7. **6 Interactive Target Buttons**: Glowing button switches mounted on the 36m Spire, Quadrant Towers, Sky-Deck, and launch ramps that turn green and chime when hit by flying crates.
8. **Bowling Alley**: A destructible wooden barricade wall and 10 heavy physics crates arranged in a bowling pin formation.
9. **Safe Spawn Hub**: Elevated platform at Y = 9.6m overlooking the arena.
10. **Hazard Pit Reset**: Invisible trigger plane at Y = -12m that smoothly resets you to the spawn hub if you fall off platforms.

---

## 7. How to Test in Unity (Step-by-Step)

1. Open your Unity project.
2. In the top menu bar, click:
   - **`Tools > REBOUND > Create Redliner Grapple Arena (200m)`** (or simply press Play; legacy test rooms auto-upgrade automatically).
3. Press **Play** (`Ctrl + P`).
4. **Test Movements**:
   - Walk with `W`: verify you walk at 6.5 m/s.
   - Walk + `C`: verify you crouch-walk at 3.5 m/s without sliding.
   - Sprint (`Shift + W`) + `C`: verify you enter an explosive power slide (14–22 m/s).
   - Hold `Space`: verify your player automatically chains continuous bunny-hops upon touching the ground.
   - Turn with mouse while holding `A` or `D` in air: verify your speed increases on turns.
5. **Test Grapple & Crate Cannon**:
   - Point at the 36m Central Spire: verify crosshair turns **Electric Cyan diamond** and badge reads `[RMB] GRAPPLE`.
   - Hold `RMB`: verify you get pulled toward the spire with elastic swing momentum.
   - Release `RMB` or press `Space` mid-grapple: verify you get flung forward with +6.5 m/s boost.
   - Point at a crate: verify crosshair turns **Amber/Yellow box** and badge reads `[RMB] GRAB`.
   - Hold `RMB` to lift the crate: verify crosshair turns **Neon Green**. Aim at a red Target Button on a tower and click `LMB`: verify the crate launches at 35 m/s and activates the button into glowing Neon Green with an audio chime!
6. **Test Infinite Slow-Mo**:
   - Press `Q` or `F`: verify world slows to 25% speed with zero cooldown. Press again to exit.

---

## 8. Inspector Tuning Guide (Every Variable Explained)

If you want to tune numbers in the Unity Inspector, select `Player_Rig` in your Hierarchy and inspect these fields:

| Script | Variable Name | Default Value | Plain-English Explanation |
|---|---|---|---|
| `PlayerMovement` | `walkSpeed` | `6.5` | Normal forward walking speed in meters per second. |
| `PlayerMovement` | `sprintSpeed` | `11.5` | Sprinting speed when holding Left Shift. |
| `PlayerMovement` | `crouchSpeed` | `3.5` | Stealth walking speed when crouching without sprinting. |
| `PlayerMovement` | `airAcceleration` | `26.0` | How strongly air-strafing accelerates your speed when turning. |
| `PlayerMovement` | `jumpImpulse` | `6.6` | Upward vertical launch speed on jump (gives 1.6m height). |
| `PlayerMovement` | `bHopBoost` | `2.5` | Bonus speed added every time you bunny-hop out of a sprint or slide. |
| `PlayerMovement` | `runThreshold` | `7.5` | Minimum speed needed to trigger a power slide (prevents slide on walk). |
| `PlayerMovement` | `slideBoost` | `4.5` | Initial speed punch added when entering a power slide. |
| `PlayerMovement` | `maxSpeed` | `35.0` | Maximum speed cap for physics and parkour momentum. |
| `PullAbility` | `maxGrappleRange` | `75.0` | Maximum distance in meters to attach a grapple hook cable. |
| `PullAbility` | `grappleFlingBoost`| `6.5` | Forward launch speed added when releasing a grapple tether. |
| `PullAbility` | `launchSpeed` | `35.0` | Speed in meters per second of crates fired with Crate Cannon. |
| `PushAbility` | `cooldownTime` | `0.45` | Minimum seconds between Push air blasts. |
| `SlowMotionAbility`| `infiniteSlowMo` | `true` | If true, Slow-Mo has zero cooldown and infinite duration. |
| `SlowMotionAbility`| `slowMoTimeScale`| `0.25` | Speed of the world during Slow-Mo (0.25 = quarter speed). |

---

## 9. Complete Changelog & Version History

- **v1.0 (Initial Midterm Setup)**: Basic movement, raycasts, standard jump.
- **v2.0 (Tactical Polish Upgrade)**: Fixed jump impulse with `VelocityChange` for reliable 1.6m jump; added Ledge Mantling; added visible air blast shockwaves; added procedural synthesized audio suite.
- **v3.0 (Parkour Flow & 6-Chamber Course)**: Added Apex/Titanfall run-jump-slide queue; added slope surfing; added Slide Bowling Ram; built 6-chamber linear flow test course; added Global Hazard Reset Trigger.
- **v4.0 (Roblox Redliner Movement & Grapple Overhaul)**:
  - Separated walking-crouch (3.5 m/s) from power-sliding (14–22 m/s).
  - Implemented Source/Redliner air-strafing with vector projection math, rewarding camera turns with speed gains up to 35 m/s.
  - Added 75m long-range grapple hook with SphereCast assisted aiming and elastic winch pull.
  - Built the expansive 200m x 200m x 35m Redliner Grapple Arena with the 36m Spire, 4 Towers, and Aerial Gantries.
- **v4.5 (Dynamic Smart Crosshair, Target Buttons & Infinite Focus)**:
  - **Zero-Cooldown Infinite Focus**: Removed meter depletion and cooldown timers; toggle on/off with `Q`/`F` at any time.
  - **Hold-Space Auto-Bunny-Hop**: Players can hold Space to continuously chain frame-perfect bunny hops upon every ground contact.
  - **Smart 4-State Crosshair**: Amber/Yellow square for grabbing crates, Electric Cyan diamond for grappling walls, Neon Green for armed Crate Cannon, White for open air.
  - **Interactive Target Buttons (`TargetButton.cs`)**: Mounted 6 switches across the arena that depress, turn Neon Green, chime, and reward Focus when hit by crates or slide rams.
