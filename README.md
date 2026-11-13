# REBOUND — First-Person High-Speed Physics Action & Movement Game

Welcome to the **REBOUND** game repository! Built in Unity for our Year 3 Game Development Midterm.

---

## Quick Setup (1 Click)

1. Open `Assets/Scenes/SampleScene.unity`.
2. In the top Unity menu, click: **`Tools > REBOUND > 1. Setup Complete Game (Arena + Player)`**.
3. Press **Play (`Ctrl + P`)**.
4. To verify movement physics, click: **`Tools > REBOUND > 2. Run Movement Unit Tests`**.

---

## Quick Navigation

* **Master Player System & Gameplay Manual**: [`docs/PLAYER_SYSTEM_MANUAL.md`](docs/PLAYER_SYSTEM_MANUAL.md)
  * Complete plain-English manual explaining every script, calculation, control, and mechanic.
* **Master Deep Architecture Specification**: [`docs/PLAYER_DEEP_ARCHITECTURE_SPECIFICATION.md`](docs/PLAYER_DEEP_ARCHITECTURE_SPECIFICATION.md)
  * Exhaustive reference with mathematical formulas, state machine tables, and technical defense Q&A.
* **Audit & Redliner Upgrade Roadmap**: [`docs/REBOUND_REDLINE_AUDIT_AND_ROADMAP.md`](docs/REBOUND_REDLINE_AUDIT_AND_ROADMAP.md)
  * Audit findings, reconciled parameters, and test verification suite.
* **Team Development Handbook & Code Contracts**: [`docs/TEAM_DEVELOPMENT_HANDBOOK.md`](docs/TEAM_DEVELOPMENT_HANDBOOK.md)
  * Read this if you are building Enemy AI or Level Puzzles (`IKineticTarget.cs` and `TargetButton.cs`).

---

## Core Mechanics Summary

| Action | Control | Physical Effect |
|---|---|---|
| **Walk & Crouch** | `WASD` / `C` | Walk at 6.5 m/s; pressing `C` while walking crouch-walks at 3.5 m/s without accidental slides. |
| **Power Slide & Carving** | `Shift + W + C` | Sprint + Crouch slides at 14–22 m/s; WASD allows full 360-degree slide steering and carving! Downhill slopes accelerate up to 35 m/s. |
| **Hold-Space Auto-Hop** | `Space (Hold)` | Automatically chains frame-perfect bunny hops upon touching the ground; adds +2.5 m/s boost per hop! |
| **Air-Strafing** | `A / D + Mouse` | Turning camera into an air-strafe adds +1.5 to +3.5 m/s speed per turn up to 35 m/s soft cap. |
| **Ledge Mantling** | `W into ledge` | Touching a wall or platform edge at chest height automatically climbs you up onto the top surface. |
| **Push (Wind Cannon)** | `Left Mouse Button` | Blasts objects away within 12m; Rocket Jump against floors, Wall Kick against walls, Slide Boost (+10 m/s) during slide. |
| **Pull (Grapple Hook)** | `Right Mouse Button (Hold)` | 75m elastic grapple hook to swing around towers or slingshot into slides (+6.5 m/s fling boost on release). |
| **Crate Snatch & Shield** | `RMB on Crate` | 45m range with magnetic aim; Light crates snap at 32 m/s, Medium at 24 m/s, Heavy drag at 12 m/s. Acts as riot shield. |
| **Crate Cannon** | `LMB while holding Crate` | Launches carried block at 35 m/s projectile speed to smash doors, hit buttons, and crush enemies. |
| **Infinite Slow Motion** | `Q` or `F` | World slows to 25% speed; zero cooldown and infinite duration, toggle on or off anytime. |
| **Target Buttons** | *Shoot or Slide* | Glowing wall/tower switches that turn Neon Green, chime, and reward Focus when hit by crates or slide rams. |
| **Telemetry Overlay** | `F3 Key` | Real-time diagnostic on-screen overlay showing velocity vectors, ground normal, slope angle, and peak speeds. |
| **Instant Safe Reset** | `R Key` | Instantly respawns on top of the safe elevated arena platform. |

---

## The 4 Enemy Archetypes

1. **Small Fast Swarmer**: 4-8 pack rushers destroyed by high-speed slide ramming (bowling strike).
2. **Heavy Hulk Leaper**: High leap slam with ground shockwave; vulnerable back core exposed during 2.5s stun.
3. **Slow Deadly Sniper**: 1.5s red laser tracking with lethal sonic round; deflectable back at sniper with Slow-Mo Push.
4. **Fast Rapid-Laser Sentry**: 3-second sweeping plasma spray; physically absorbed by holding a crate as a riot shield.

---

## Unity Project Guidelines

* **Unity Version**: Unity 6000.3.8f1 (or agreed team LTS).
* **Physics Layers**:
  * Layer 8: `Player`
  * Layer 9: `Environment`
  * Layer 10: `Interactables`
  * Layer 11: `Hazards`
  * Layer 12: `Enemies`
  * Layer 13: `Projectiles`
  * Layer 14: `GrappleAnchors`
* **Version Control**: Do not commit `Library/`, `Temp/`, or `Logs/` folders (handled by `.gitignore`).
