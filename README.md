# REBOUND — First-Person High-Speed Physics Action & Movement Game

Welcome to the **REBOUND** game repository! Built in Unity for our Year 3 Game Development Midterm.

---

## Quick Navigation

* **Master Player System & Gameplay Manual**: [`docs/PLAYER_SYSTEM_MANUAL.md`](docs/PLAYER_SYSTEM_MANUAL.md)
  * Complete plain-English manual explaining every script, calculation, control, and mechanic.
* **Team Development Handbook & Code Contracts**: [`docs/TEAM_DEVELOPMENT_HANDBOOK.md`](docs/TEAM_DEVELOPMENT_HANDBOOK.md)
  * Read this if you are building Enemy AI or Level Puzzles.
  * Contains the exact C# interface (`IKineticTarget.cs`) and component (`TargetButton.cs`).
* **Interactive Architecture Documentation (ArchDoc)**: [`docs/archdoc/index.html`](docs/archdoc/index.html)
  * Open in any browser to explore interactive architecture diagrams.
* **Standalone Offline ArchDoc**: [`docs/rebound-docs.html`](docs/rebound-docs.html)
  * A single, self-contained HTML file containing the entire architecture site for offline reading.

---

## Core Mechanics Summary

| Action | Control | Physical Effect |
|---|---|---|
| **Walk & Crouch** | `WASD` / `C` | Walk at 6.5 m/s; pressing `C` while walking ducks into a stealth crouch-walk at 3.5 m/s without sliding. |
| **Power Slide & Slope Surf** | `Shift + W + C` | Sprinting + Crouch triggers explosive slide at 14–22 m/s; downward slopes accelerate up to 35 m/s! |
| **Hold-Space Auto-Hop** | `Space (Hold)` | Automatically chains frame-perfect bunny hops upon touching the ground; adds +2.5 m/s boost per hop! |
| **Air-Strafing** | `A / D + Mouse` | Turning camera into an air-strafe adds +1.5 to +3.5 m/s speed per turn up to 35 m/s soft cap. |
| **Push (Wind Cannon)** | `Left Mouse Button` | Blasts objects away; Rocket Jump against floors, Wall Kick against walls, Slide Boost (+10 m/s) during slide. |
| **Pull (Grapple Hook)** | `Right Mouse Button (Hold)` | 75m elastic grapple hook to swing around towers or slingshot into slides (+6.5 m/s fling boost on release). |
| **Crate Snatch & Shield** | `RMB on Crate` | Pulls highlighted crate to hover 2.5m in front of hands; acts as physical riot shield against bullets. |
| **Crate Cannon** | `LMB while holding Crate` | Launches carried block at 35 m/s projectile speed to smash doors, hit buttons, and crush enemies. |
| **Infinite Slow Motion** | `Q` or `F` | World slows to 25% speed; zero cooldown and infinite duration, toggle on or off anytime. |
| **Target Buttons** | *Shoot or Slide* | Glowing wall/tower switches that turn Neon Green, chime, and reward Focus when hit by crates or slide rams. |
| **Spider Sense** | *Automatic Visual* | Directional red warning arc flashes on HUD when lethal attacks are imminent. |
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
