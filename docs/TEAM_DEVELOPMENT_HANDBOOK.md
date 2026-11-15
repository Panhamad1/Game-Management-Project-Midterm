# REBOUND — Complete Team Development Handbook & Technical Guide

Welcome to the **REBOUND** team documentation! This guide gives everyone on the team—gameplay programmers, enemy AI developers, and level designers—the exact rules, controls, physics settings, and code contracts for our game.

No complicated jargon is used here. Everything is explained in direct, simple English so you can build your part of the project without confusion.

---

## 1. Game Concept & Thematic Fantasy

**REBOUND** is a single-player, first-person physics parkour and kinetic combat game built in Unity. It blends responsive parkour movement (sprinting, sliding under gates, wall-running, sheer wall clings, and ledge climbing) with pneumatic air-pressure abilities and kinetic prop weaponry. 

There are no conventional guns or ammo boxes. All offensive combat and traversal comes from kinetic abilities, throwing environmental Egyptian props, and deflecting enemy plasma bolts:

- **Push & Deflect (Left Mouse Button - LMB)**: Shoots a pneumatic **Wind Cannon**. Blasts physics objects away, launches carried props at 32 m/s, and kicks your body backward with rocket recoil. Blasting within 3.8m of incoming plasma bolts parries the projectile back toward the attacker at 35 m/s for 2.0x damage and restores +20 Focus energy.
- **Pull & Carry (Right Mouse Button - RMB)**: An 18-meter **Kinetic Tractor Beam**. Snaps Egyptian props (Urns, Boulders, Slabs) into a dedicated bottom-right carry pocket (0.42m, -0.32m, 1.35m) so your forward crosshair view remains 100% clear. (Grapple traversal is cut; parkour handles all movement).
- **Kinetic Prop Cannon (LMB while carrying a prop)**: Instantly fires the carried object forward at 32.0 m/s as a heavy kinetic missile to smash stone gates, activate Ankh targets, and crush or stagger enemies.
- **Chrono-Dilation Slow Motion (Z Key)**: Slows world physics down to 25% speed (`Time.timeScale = 0.25`) while keeping your camera aim and movement speed at 100% full speed. Center screen stays crisp and uncolored, framed by subtle 12px edge border accents.

---

## 2. Controls & Core Mechanics

### Control Mapping

| Input | Action | Description |
|---|---|---|
| **WASD** | Walk | Deliberate movement at `3.2 m/s` for precise platforming |
| **Left Shift** | **Sprint / Run** | Accelerates player to `5.4 m/s` |
| **Ctrl** | **Crouch / Slide** | Drops capsule height to 1.15m to slide under low 1.2m gates at `4.8 m/s` |
| **Space** | **Jump / Wall-Climb / Wall-Jump** | Normal 1.6m vertical jump; when wall-hanging, executes +9.5m climb vault; when wall-running, executes zig-zag wall-jump |
| **Q** | **Dash** | Rapid horizontal dash burst |
| **Z** | **Slow Motion (Focus)** | Dilates world to 25% speed; unscaled player aim and movement |
| **Left Click (LMB)** | **Push / Deflect / Fire Prop** | Pushes objects, launches carried prop at 32 m/s, or deflects incoming bolts within 3.8m cone |
| **Right Click (RMB)** | **Pull & Carry** | 18m tractor beam that seats props in the lower-right carry pocket |
| **Mouse** | Look | Smooth pitch (-85° to +85°) and yaw look with natural head-bob and landing compression dip |
| **R** | **Quick Checkpoint Reset** | Instantly relocates player to the latest chamber checkpoint anchor |
| **Escape** | Pause | Opens pause menu / unlocks cursor |

---

## 3. Parkour Locomotion System

1. **Sprint & Low-Gate Slide (Chamber 1)**:
   - Holding `Left Shift` sprints at `5.4 m/s`.
   - Tapping `Ctrl` drops capsule height to `1.15m` and glides forward at `4.8 m/s`, cleanly sliding under low `1.2m` sandstone gates.
   - Overhead clearance raycast ensures the player remains crouched until clearing overhead obstacles.
2. **Wall-Running & Zig-Zag Wall-Jumping (Chamber 2)**:
   - Sprinting and jumping into a vertical sandstone wall engages an automatic horizontal wall-run.
   - Pressing `Space + A/D` executes an additive impulse:
     $$\vec{v}_{\text{jump}} = \vec{n}_{\text{wall}} \cdot 10.5 \text{ m/s} + \text{Vector3.up} \cdot 8.0 \text{ m/s} + \vec{w}_{\text{forward}} \cdot 7.5 \text{ m/s}$$
   - Enables rhythmic zig-zag traversal between opposite walls across wide chasms.
3. **Wall-Hang Cling & Vertical Climb Vault (Chamber 3)**:
   - Leaping into a sheer wall face latches into a 1.5-second wall-hang cling with gentle `0.35 m/s` downward slide.
   - Pressing `Space` while hanging executes `ExecuteWallClimbJump()`:
     $$\vec{v}_{\text{climb}} = \text{Vector3.up} \cdot 9.5 \text{ m/s} + \text{cameraForward} \cdot 3.8 \text{ m/s}$$
   - Vaults the player +9.5 meters vertically over high cliff ledges.
4. **Calibrated Speed Cap**:
   - Maximum velocity is calibrated to `9.0 m/s`, eliminating physics tunneling and preserving human reaction times.

---

## 4. Egyptian Props & Ambient Mass Auras

Never hardcode object names. Movable props use the `InteractableProp` script and emit subtle, constant ambient point-light auras (`intensity = 0.55`, `range = 2.6m`):

| Prop Name | Mass | Material / Appearance | Aura Color | Physical & Combat Role |
|---|---|---|---|---|
| **Scarab Urn** | `1.5 kg` | Carved Jade Urn | **Mint Jade** (`#5eead4`) | Agile rapid throw, button activator |
| **Sandstone Boulder** | `12.0 kg` | Ancient Relic / Boulder | **Amber Gold** (`#f59e0b`) | Heavy kinetic missile, bowls Swarmers |
| **Obsidian Slab** | `45.0 kg` | Volcanic Heavy Slab | **Molten Ruby** (`#f43f5e`) | Massive kinetic battering ram, staggers Hulk |

### Bottom-Right Carry Pocket Anchor
- Pulled props seat at `(X = 0.42m, Y = -0.32m, Z = 1.35m)` relative to the camera.
- Guarantees the center crosshair and forward sightlines stay **100% unobstructed**.
- Tapping `LMB` while carrying fires the prop forward at `32.0 m/s`.

---

## 5. Enemy System & Combat Balance (The 4 Enemy Archetypes)

Enemies are built around clear mechanical profiles and telegraphed attacks:

1. **Swarmer Enemy (`SwarmerEnemy.cs`)**:
   - Rushes in packs of 4 to 8.
   - When struck by thrown props or high-speed slides, flings through the air (`OnPhysicsCrush`).
   - Has a **65% chance** of an electric plasma explosion upon death, damaging nearby foes.
2. **Hulk Leaper Enemy (`HulkLeaperEnemy.cs`)**:
   - Giant armored mechanical brute.
   - Leaps high and executes a ground smash that shakes the camera for players within **20 meters** (intensity attenuates with distance).
   - Struck by heavy props: receives a red hit flash, floating damage numbers, and physical stagger impulse.
3. **Sentry Turret (`SentryEnemy.cs`)**:
   - Fires rapid plasma bursts.
   - Incoming plasma bolts can be parried with Push (`LMB`) within the 3.8m parry cone back to destroy the turret.
4. **Sniper Enemy (`SniperEnemy.cs`)**:
   - Perched on high pillars. Tracks with a visible 1.5-second red laser sightline before firing.
   - Pushing right before impact deflects the round back along the laser path to one-shot the sniper.

---

## 6. The 180-Meter 6-Chamber Egyptian Academy

Built procedurally by `EgyptianTutorialBuilder.cs` (`Tools → REBOUND → Build 6-Chamber Tutorial Academy (Complete)`):

- **Chamber 1 (0m - 30m)**: Locomotion & Gate Slide (WASD walk 3.2 m/s, Left Shift sprint 5.4 m/s, Ctrl slide 4.8 m/s under low 1.2m gate).
- **Chamber 2 (30m - 60m)**: Wall-Run Chasm (Horizontal wall-running and zig-zag wall-jumping across void).
- **Chamber 3 (60m - 90m)**: Wall-Hang & Climb Vault (7m sheer wall; 1.5s cling and +9.5m vertical climb vault on Space).
- **Chamber 4 (90m - 120m)**: Egyptian Kinetic Prop Weaponry (RMB pocket pull, LMB 32 m/s launch at Ankh target switch).
- **Chamber 5 (120m - 150m)**: Chrono-Dilation & Bullet Deflection (Z key slow-mo, LMB 3.8m parry deflection of Sentry bolts).
- **Chamber 6 (150m - 185m)**: Combat Mastery Sanctum (Full combat gauntlet combining all skills against Swarmers, Hulk, Sniper, and Sentry).
- **Void Kill Plane (y = -8m)**: Falling below -8m instantly resets the player to the latest chamber checkpoint in 0.05 seconds.

---

## 7. Physics Layers Setup in Unity

Verify these layers in **Project Settings > Tags and Layers**:

- **Layer 8: `Player`** — PlayerRoot capsule and sensors. Ignored by own ability rays.
- **Layer 9: `Environment`** — Static sandstone walls, floors, and slide gates.
- **Layer 10: `Interactables`** — Egyptian props (Urns, Boulders, Slabs).
- **Layer 11: `Hazards`** — Void kill plane at y = -8m.
- **Layer 12: `Enemies`** — Enemy bodies and hurtboxes.
- **Layer 13: `Projectiles`** — Deflectable sniper rounds and sentry plasma bolts.

---

## Summary Checklist for the Team

- [x] **Parkour Locomotion Verified**: Sprint, low-gate slide, 1.5s wall-hang, +9.5m climb vault, and zig-zag wall-jump.
- [x] **Unobstructed Pocket Carry**: Props seat in bottom-right pocket; forward crosshair view stays 100% clear.
- [x] **Bullet Deflection Parry Cone**: 3.8m overlap sweep reverses bolts at 35 m/s and restores +20 Focus energy.
- [x] **Chrono-Dilation Clarity**: Z key slow motion with 100% crisp center screen and 12px edge border accents.
- [x] **4 Enemy Archetypes Integrated**: Swarmers (65% death explosion, fling), Hulk (leap smash, <20m shake, prop stagger), Sniper & Sentry.
- [x] **Egyptian Academy**: 180m procedural 6-chamber academy with checkpoint triggers and void kill plane.
