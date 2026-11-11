# REBOUND — First-Person Physics Action & Puzzle Game

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
| **Push (Wind Cannon)** | `Left Mouse Button` | Blasts objects forward with compressed air; kicks player backward with 3D rocket recoil. |
| **Pull (Vacuum Vortex)** | `Right Mouse Button (Hold)` | Suction funnel pulls objects to a 2.5m hover anchor in front of the camera. |
| **Crate Cannon** | `LMB while holding RMB` | Instantly launches the carried block forward as a high-speed kinetic missile. |
| **Slow Motion** | `Left Shift` | World slows to 25% speed; player moves, aims, and falls with floaty air-time at full control. |
| **Rocket Jump** | `Look Down + LMB` | Recoil launches the player high over pits and obstacles. |

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
* **Version Control**: Do not commit `Library/`, `Temp/`, or `Logs/` folders (handled by `.gitignore`).
