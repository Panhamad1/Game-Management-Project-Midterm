# REBOUND — First-Person Physics Action & Parkour Game

Welcome to the **REBOUND** repository. Built in Unity for our Game Development Midterm.

---

## Quick Setup (1 Click)

1. Open `Assets/monirath-game.unity` (or `Assets/Scenes/SampleScene.unity`).
2. In the top Unity menu, click: **`Tools > REBOUND > Clean Scene (Delete All Arenas & Test Rooms)`**.
3. Next, click: **`Tools > REBOUND > Build 6-Chamber Tutorial Academy (Complete)`**.
4. Press **Play (`Ctrl + P`)**.

---

## Controls Summary

| Action | Key / Input | What It Does |
|---|---|---|
| **Walk** | `WASD` | Normal ground movement at `3.2 m/s`. |
| **Run / Sprint** | `Left Shift + WASD` | Sprint forward at `5.4 m/s`. |
| **Crouch** | `Ctrl` or `C` | Crouch-walk under low obstacles at `2.0 m/s`. |
| **Power Slide** | `Left Shift + Ctrl` | Slide at `4.8 m/s` with full 360-degree steering and carving. |
| **Dash** | `Q` | Instant burst of directional speed. Resets when touching the ground or walls. |
| **Jump & Hop** | `Space` | Jump up with a `1.6m` arc. Hold Space to bunny-hop continuously. |
| **Wall-Run** | Jump along a wall | Run horizontally along walls with slow, floaty gravity. |
| **Zig-Zag Wall-Jump** | `Space` while wall-running | Kick off the wall with `10.5 m/s` outward push and `8.0 m/s` lift. Steer with `A` or `D`. |
| **Wall-Hang** | Hold `W` into a wall in mid-air | Cling to the wall for up to 1.5 seconds. |
| **Wall-Climb Vault** | `Space` while wall-hanging | Hoist yourself `+9.5m` up and forward over the ledge. |
| **Push (Air Cannon)** | `Left Mouse Button (LMB)` | Blasts objects away within 12m. Rocket jump off floors and walls. |
| **Deflect Bullets** | `LMB` when a bullet approaches | Deflects enemy laser bolts back at enemies with double speed (`35 m/s`) and double damage. |
| **Pull & Carry** | `Hold Right Mouse Button (RMB)` | Pulls objects into your bottom-right pocket so your view stays clear. Bullets cannot be pulled. |
| **Throw / Launch Prop** | `LMB while holding prop` | Fires the held object at `32 m/s` as a heavy kinetic projectile. |
| **Slow Motion (Focus)** | `Z` | Slows the world to 25% speed. The screen stays clear with an electric edge border. |

---

## Egyptian Props & Mass Glow

Throwable objects in the game have a soft ambient glow around them so you can see their mass from across the room:

- **Light Props (Scarab Urns)**: Mint Green aura (`1.5 kg`). Quick to pull and launch.
- **Medium Props (Sandstone Boulders & Relics)**: Amber Gold aura (`12 kg`). Great for smashing doors and barricades.
- **Heavy Props (Obsidian Slabs)**: Molten Ruby aura (`45 kg`). Acts as a heavy shield against enemy fire.

---

## Enemies

- **Swarmer (20 HP)**: Fast flying attacker. When you grab and throw a Swarmer, it has a 65% chance to explode into a high-yield plasma blast (`6.5m` blast radius, 75 damage). Struck by props, it flies backward like a ragdoll.
- **Rapid Laser Sentry (50 HP)**: Fires 3-round bursts of red laser bolts. Deflect the bolts back with Push (`LMB`) or block them with a prop.
- **Hulk Leaper (90 HP)**: Heavy armored brute. Slams the ground with a shockwave that shakes the screen if you are within 20m. Throwing blocks at him deals damage, flashes red, shows damage numbers, and staggers him backward.
- **Sniper Enemy (10 HP)**: Tracks you with a red laser sight and fires a lethal bolt. Deflect his bolt back at him to defeat him instantly.

---

## 6-Chamber Egyptian Academy Tutorial

The game features a linear 180-meter training academy:
- **Chamber 1**: Movement basics (Walking, Sprinting, Crouching, Sliding under gates).
- **Chamber 2**: Wall-running across open chasms.
- **Chamber 3**: Wall-hanging on cliffs and using Wall-Climb to scale terraces.
- **Chamber 4**: Kinetic props (pulling urns and boulders into your pocket, smashing breakable barricades and Ankh target buttons).
- **Chamber 5**: Slow Motion and Bullet Deflection against a laser sentry.
- **Chamber 6**: The Combat Sanctum (defeating Swarmers, Sentries, and the Hulk Leaper).

---

## Documentation Index

- **Gameplay & Player Manual**: [`docs/PLAYER_SYSTEM_MANUAL.md`](docs/PLAYER_SYSTEM_MANUAL.md)
- **Enemy Combat Guide**: [`docs/REBOUND_ENEMY_GAMEPLAY_GUIDE.md`](docs/REBOUND_ENEMY_GAMEPLAY_GUIDE.md)
- **Interactive Web Documentation**: [`docs/archdoc/index.html`](docs/archdoc/index.html) (open in any web browser)
- **Single-File Offline Export**: [`docs/rebound-docs.html`](docs/rebound-docs.html)
