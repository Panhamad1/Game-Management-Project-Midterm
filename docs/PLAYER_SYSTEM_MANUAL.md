# REBOUND — Complete Player System & Gameplay Manual

Welcome to the official manual for **REBOUND**.

This document explains every control, calculation, script, and game mechanic in direct, simple English. There are no confusing buzzwords. Everything matches the actual code in the project.

---

## Table of Contents

1. [Game Overview](#1-game-overview)
2. [Player Hierarchy in Unity](#2-player-hierarchy-in-unity)
3. [Movement & Parkour System](#3-movement--parkour-system)
   - [Walking, Running & Crouching](#31-walking-running--crouching)
   - [Power Sliding & Carving](#32-power-sliding--carving)
   - [Dashing](#33-dashing)
   - [Jumping & Bunny-Hopping](#34-jumping--bunny-hopping)
   - [Wall-Running & Zig-Zag Wall-Jumping](#35-wall-running--zig-zag-wall-jumping)
   - [Wall-Hanging & Wall-Climb Vault](#36-wall-hanging--wall-climb-vault)
   - [Ledge Mantling](#37-ledge-mantling)
4. [Abilities & Combat System](#4-abilities--combat-system)
   - [Push & Rocket Recoil](#41-push--rocket-recoil)
   - [Bullet Deflection (Parry)](#42-bullet-deflection-parry)
   - [Pull & Pocket Carry](#43-pull--pocket-carry)
   - [Crate Cannon (Launching Props)](#44-crate-cannon-launching-props)
   - [Slow Motion (Focus)](#45-slow-motion-focus)
   - [Health, Damage Numbers & Hit Flashes](#46-health-damage-numbers--hit-flashes)
5. [Egyptian Props & Ambient Mass Glow](#5-egyptian-props--ambient-mass-glow)
6. [Enemies & Combat Tactics](#6-enemies--combat-tactics)
7. [The 6-Chamber Egyptian Academy](#7-the-6-chamber-egyptian-academy)
8. [Unity Editor Tools & Quick Setup](#8-unity-editor-tools--quick-setup)

---

## 1. Game Overview

**REBOUND** is a first-person physics action and parkour game built in Unity.

Instead of traditional guns, you use physical air-pressure abilities, momentum parkour, and throwable Egyptian relics:
- **Push (`LMB`)**: A compressed air blast that shoves objects, launches you off surfaces, and deflects incoming enemy bullets.
- **Pull (`Hold RMB`)**: Reels throwable rocks and urns into your bottom-right pocket so your view remains clear. Bullets cannot be pulled.
- **Crate Cannon (`LMB while holding an object`)**: Fires the carried prop forward as a heavy kinetic projectile to smash doors and crush enemies.
- **Slow Motion (`Z`)**: Slows world physics down to 25% speed while keeping your aim and look fast and responsive.

---

## 2. Player Hierarchy in Unity

The player character in Unity is called `Player_Rig`. It uses a clean parent-child structure so physics and camera movements never fight each other:

```text
Player_Rig (Layer 8: Player, Rigidbody, CapsuleCollider, PlayerMovement, PlayerHealth, AudioEffects)
 │
 ├── GroundCheck (Sphere sensor at feet level Y = -1.0 to check floor contact)
 │
 └── CameraPivot (Eye level Y = 1.65m, MouseLook script)
      │
      ├── MainCamera (First-person camera view, AudioListener)
      │
      ├── AbilityRayOrigin (Sensor point 0.35m in front of camera lens)
      │
      ├── LeftHandVisual (Left hand cylinder, PushAbility)
      │
      ├── RightHandVisual (Right hand cylinder, PullAbility)
      │
      └── AbilityHUD_Canvas (UI: Smart Crosshair, Target Badge, Speedometer, Guide)
```

### Why It Is Built This Way:
1. **Capsule Collider on Root**: Only the outer capsule touches floors and walls. X and Z rotations are frozen so the player never falls over.
2. **Camera Pivot Separated from Body**: When recoil pushes your character, the camera smoothly moves along without snapping or shaking your mouse aim.
3. **Ray Origin in Front of Lens**: Ability rays cast from 0.35m ahead of the camera so your own body collider never blocks your own abilities.

---

## 3. Movement & Parkour System

### 3.1 Walking, Running & Crouching
- **Walk Speed**: `3.2 m/s` using `WASD`.
- **Run / Sprint**: `5.4 m/s` by holding `Left Shift`.
- **Crouch**: `2.0 m/s` by pressing `Ctrl` or `C`. Lets you duck under low gates.
- **Max Speed Cap**: `9.0 m/s`. Prevents erratic, uncontrollable flying speeds.

### 3.2 Power Sliding & Carving
- **How to Slide**: Sprint forward (`Left Shift + W`) and press `Ctrl`. You enter a slide at `4.8 m/s`.
- **360° Carving**: While sliding, press `A`, `S`, or `D` to steer and carve your slide path around corners.
- **Slide Battering Ram**: Sliding into small enemies (Swarmers) at high speed defeats them instantly.

### 3.3 Dashing
- **Control**: Press `Q`.
- **Effect**: Delivers an instant directional burst of speed in your current move direction.
- **Cooldown**: 1.0 second. You get 1 air-dash per aerial jump, which resets as soon as you touch the ground or cling to a wall.

### 3.4 Jumping & Bunny-Hopping
- **Single Jump**: Tap `Space`. Produces a clean `1.6-meter` jump arc under 2.4x world gravity.
- **Auto Bunny-Hop**: Simply hold down `Space`! When your feet touch the ground, you immediately jump again with zero speed loss.

### 3.5 Wall-Running & Zig-Zag Wall-Jumping
- **Wall-Run**: Jump diagonally toward any vertical wall. Your character sticks to the wall horizontally with very slow gravity drift for up to 1.8 seconds.
- **Zig-Zag Wall-Jump**: Press `Space` while wall-running.
  - Pushes you outward from the wall at `10.5 m/s`.
  - Lifts you upward at `8.0 m/s`.
  - Preserves forward running speed.
  - Steer with `A` or `D` to aim for the opposite wall, letting you zig-zag up narrow shafts and chasms.

### 3.6 Wall-Hanging & Wall-Climb Vault
- **Wall-Hang**: Jump face-first into a tall wall while holding `W` in mid-air. You cling to the wall for up to `1.5 seconds` with a gentle physical slide.
- **Wall-Climb Vault**: While hanging on the wall, tap `Space`. You push off with your hands and vault `+9.5 meters` upward and `3.8 meters` forward over the cliff ledge.
- **Drop Off**: Press `S` while hanging to let go and drop down.

### 3.7 Ledge Mantling
- Moving forward near a chest-high platform automatically hoists your character up onto the platform floor.

---

## 4. Abilities & Combat System

### 4.1 Push & Rocket Recoil
- **Control**: `Left Mouse Button (LMB)`.
- **Range**: 12 meters.
- **Effect**: Blasts objects and enemies away.
- **Recoil Movement**:
  - Aiming at the floor pushes you upward (clamped to `4.2 m/s` so you do not fly into outer space).
  - Aiming at a wall kicks you backward.
  - Blasting while sliding boosts your forward slide speed.

### 4.2 Bullet Deflection (Parry)
- When an enemy (Sentry or Sniper) shoots red laser bolts at you, press `LMB` (Push) as the bolt nears you.
- The bolt **deflects back** along your crosshair aim.
- Reflected bolts fly at **35 m/s** (double speed) and deal **double damage**, turning bright glowing cyan.
- Deflecting a bolt instantly restores **+20 Focus** energy.

### 4.3 Pull & Pocket Carry
- **Control**: Hold `Right Mouse Button (RMB)`.
- **How it works**: Targets an eligible prop within 45m and pulls it smoothly into your **bottom-right pocket** (`1.15m` right, `-0.55m` down from camera).
- **Clear View**: The object stays in your bottom-right pocket, keeping the center of your screen and crosshair 100% visible.
- **Safety**: Carried objects are kinematic while held. Walking past destructible walls or props while holding an object will **never** accidentally break or fling them.
- **No Bullet Pull**: Projectiles cannot be pulled by RMB.

### 4.4 Crate Cannon (Launching Props)
- **Control**: Click `LMB` while carrying an object with `RMB`.
- **Effect**: Fires the carried object forward at `32 m/s`.
- Adds player forward momentum to the throw. Deals heavy damage to breakable barricades, target switches, and enemies.

### 4.5 Slow Motion (Focus)
- **Control**: Press `Z` to toggle on or off.
- **Effect**: Slows the game world to 25% speed.
- **Visuals**: Zero arm-stretching distortion. The center of the screen stays crystal clear, framed by a subtle electric border rim and a bottom status label.
- **Energy**: Focus meter drains over 10 seconds. Recharges automatically when resting, or faster when moving fast, deflecting bullets, or defeating enemies.

### 4.6 Health, Damage Numbers & Hit Flashes
- **Player Health**: 100 HP. When hit, the screen flashes red and an audio cue plays. Defeating enemies drops immediate heal rewards.
- **Enemy Health**: Enemies have real health bars. Striking them spawns floating 3D damage numbers and flashes their model bright red.

---

## 5. Egyptian Props & Ambient Mass Glow

Every throwable prop in the game has a soft ambient glow around it so you can identify its mass and speed at a glance:

| Prop Type | Shape | Mass | Glow Aura | Tactical Role |
|---|---|---|---|---|
| **Scarab Jade Urn** | Cylinder | `1.5 kg` (Light) | **Mint Green** | Snaps into pocket almost instantly. Rapid-fire projectile. |
| **Sandstone Boulder / Relic** | Sphere / Cube | `12 kg` (Medium) | **Amber Gold** | Balanced ammo. Smashes barricades and hits target buttons. |
| **Obsidian Slab** | Flat Slab | `45 kg` (Heavy) | **Molten Ruby** | Heavy shield. Absorbs incoming enemy laser fire. |

---

## 6. Enemies & Combat Tactics

### 1. Swarmer (20 HP)
- Fast flying creature that lunges at you when within 5m.
- **Tactic**: Slide into it to crush it, or pull it with `RMB` and throw it with `LMB`.
- **Special**: A thrown Swarmer has a **65% chance** to explode into a massive plasma detonation (`6.5m` radius, 75 damage). Struck by props, it flies backward through the air.

### 2. Rapid Laser Sentry (30 HP)
- Stationary turret that tracks you and fires 3-round bursts of red laser bolts (`22 m/s`).
- **Tactic**: Deflect its bolts back at it using `LMB` Push, or advance with an Obsidian Slab as a shield. Struck by a boulder, it gets knocked back and takes damage.

### 3. Hulk Leaper (90 HP)
- Armored brute with heavy melee attacks and a massive leaping ground slam.
- **Tactic**: When he slams the ground, jump over the shockwave. The slam shakes your screen if you are within 20m.
- Throw boulders at him to stagger him backward, flash him red, and chip away his armor. Hit him when stunned for critical damage.

### 4. Sniper Enemy (10 HP)
- Stationed at long range. Tracks you with a visible red laser beam for 1.5 seconds before firing a sonic shot.
- **Tactic**: Activate Slow-Mo with `Z`, aim directly at him, and click `LMB` Push as his shot arrives to reflect it straight back into him.

---

## 7. The 6-Chamber Egyptian Academy

The main level is a 180-meter linear tutorial built into an ancient Egyptian stone sanctum:

1. **Chamber 1 — Locomotion**: Practice walking (`3.2 m/s`), sprinting (`5.4 m/s`), and sliding (`Left Shift + Ctrl`) under a low stone gate.
2. **Chamber 2 — Wall-Running**: Leap onto sandstone walls to run across a chasm and zig-zag jump (`Space`) to the other side.
3. **Chamber 3 — Wall-Hanging & Vertical Climb**: Jump face-first into cliffs while holding `W` to hang, then tap `Space` to vault over high terraces and scale the chimney shaft.
4. **Chamber 4 — Kinetic Prop Weaponry**: Pull Scarab Urns and Sandstone Boulders into your pocket. Fire them to destroy the wooden barricade and hit the elevated Ankh button.
5. **Chamber 5 — Chrono-Dilation & Bullet Deflection**: Use Slow-Mo (`Z`) and Push (`LMB`) to deflect incoming Sentry laser bolts back at the turret.
6. **Chamber 6 — Combat Sanctum**: Defeat Swarmers, a high Sentry turret, and the Hulk Leaper in a final arena battle to unlock the exit portal.

---

## 8. Unity Editor Tools & Quick Setup

To clear old scenes and test the game:
1. In Unity's top menu bar, click: **`Tools > REBOUND > Clean Scene (Delete All Arenas & Test Rooms)`**. This completely removes any legacy floors, bowling pins, or towers.
2. Click: **`Tools > REBOUND > Build 6-Chamber Tutorial Academy (Complete)`**. This spawns the full 180-meter Egyptian academy, positions `Player_Rig`, and prepares all checkpoints.
3. Press **Play (`Ctrl + P`)**.
