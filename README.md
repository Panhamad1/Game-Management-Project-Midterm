# REBOUND — First-Person High-Speed Physics Action & Movement Game

Welcome to the **REBOUND** game repository! Built in Unity for our Year 3 Game Development Midterm.

---

## Quick Navigation

* **Team Development Handbook & Code Contracts**: [`docs/TEAM_DEVELOPMENT_HANDBOOK.md`](docs/TEAM_DEVELOPMENT_HANDBOOK.md)
  * Read this first if you are building the Player, Enemy AI, or Level Puzzles.
  * Contains the exact C# interface (`IKineticTarget.cs`) and component (`EnemyPhysicsReceiver.cs`) for enemy developers.
* **Interactive Architecture Documentation (ArchDoc)**: [`docs/archdoc/index.html`](docs/archdoc/index.html)
  * Open in any browser to explore the full interactive architecture diagrams and physics flows.
* **Standalone Offline ArchDoc**: [`docs/rebound-docs.html`](docs/rebound-docs.html)
  * A single, self-contained HTML file containing the entire architecture site for offline reading.

---

## Core Mechanics Summary

| Action | Control | Physical Effect |
|---|---|---|
| **Slide & Bunny-Hop** | `C` or `Left Ctrl` | Crouch-slide down slopes to accelerate; jumping at slide end preserves 100% horizontal speed in air. |
| **Push (Wind Blast & Boost)** | `Left Mouse Button` | Blasts objects away; kicks player backward in air, boosts player forward at high speed while sliding. |
| **Pull (Grapple / Grab)** | `Right Mouse Button (Hold)` | Dual-mode: Pulls player toward walls/anchors (Grapple Slingshot) OR pulls highlighted objects into your hands. |
| **Crate Cannon** | `LMB while holding RMB` | Instantly launches the carried block forward as a high-speed kinetic projectile to smash doors and enemies. |
| **Slow Motion** | `Left Shift` | World slows to 25% speed; meter siphons recharge from high-speed movement and target impacts. |
| **Spider Sense** | *Automatic Visual* | Directional red warning arc flashes on HUD when lethal attacks (Sniper laser lock, Hulk leap slam) are imminent. |

---

## The 4 Enemy Archetypes

1. **Small Fast Swarmer**: 4-8 pack rushers destroyed by high-speed slide ramming (bowling strike).
2. **Heavy Hulk Leaper**: High leap slam with ground shockwave; vulnerable back core exposed during 2.5s stun.
3. **Slow Deadly Sniper**: 1.5s red laser tracking with lethal sonic round; deflectable back at sniper with Slow-Mo Push.
4. **Fast Rapid-Laser Sentry**: 3-second sweeping plasma spray; physically absorbed by holding a crate as a riot shield.

---

## Unity Project Guidelines

* **Unity Version**: Ensure all team members use the agreed Unity LTS version.
* **Physics Layers**:
  * Layer 8: `Player`
  * Layer 9: `Environment`
  * Layer 10: `Interactables`
  * Layer 11: `Hazards`
  * Layer 12: `Enemies`
  * Layer 13: `Projectiles`
  * Layer 14: `GrappleAnchors`
* **Version Control**: Do not commit `Library/`, `Temp/`, or `Logs/` folders (handled by `.gitignore`).
