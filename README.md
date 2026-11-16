# REBOUND

**Mid-Term Group Project · Playable Obstacle Course**

REBOUND is a first-person parkour and physics combat game set inside an Egyptian pyramid.

## Objective

Survive five enemy waves and walk through the opened exit to finish the run.

## Rules

Select **Play** to start the timer and the first wave countdown. The room’s pillars provide cover, and the stairway leads to a raised platform. Early waves start with flying Swarmers. Later waves add Sentries that fire bursts, Hulks that knock players back with ground slams, and Snipers that warn you with a red laser before shooting.

Pull a crate toward you to use it as cover, then throw it to damage an enemy. Push can knock targets back or send incoming shots back toward enemies. The player and movable props use Unity Rigidbodies. Solid colliders block movement, crate impacts deal damage on contact, and an exit trigger checks whether the player has cleared the waves.

The HUD shows player and enemy health, remaining lives, wave progress, and elapsed time. You have **two lives**. The first time your HP reaches zero, it refills on the spot and the same wave continues. The second time ends the run with **Game Over**. Clear Wave 5 and cross the opened exit to see your finish time. The fastest successful run is saved locally. **Play Again** starts a fresh run.

## Controls

| Input | Action |
| --- | --- |
| WASD and mouse | Move and look around |
| Left Shift | Sprint |
| C or Left Ctrl | Crouch, or slide while sprinting |
| Space | Jump or kick off a wall |
| Q | Dash and toggle Focus |
| F or middle mouse | Toggle Focus to slow the world |
| Left mouse | Push, deflect a shot, or throw the held item |
| Hold right mouse | Pull and carry a prop. Release to drop it. |
| Esc | Pause. Choose Resume, Restart, or Exit. |

## Play on Windows

1. [Download REBOUND-Windows.zip](https://github.com/Panhamad1/Game-Management-Project-Midterm/raw/master/Builds/REBOUND-Windows.zip).
2. Extract the entire ZIP, open the `REBOUND-Windows` folder, and run `REBOUND.exe`. Unity is optional for the Windows build.
3. Select **Play** on the main menu.

## Open the Unity project

1. Clone the repository and use the default branch, `master`.
2. Open the project in **Unity 6000.3.8f1** and let Unity import the project.
3. Open [pyramid-map.unity](Assets/Scenes/pyramid-map.unity) and enter Play Mode. Choose **Play** on the in-game menu.

See [System Explanation](SYSTEM_EXPLANATION.md) for the technical project overview.

## Team and credits

| Contributor | Work |
| --- | --- |
| Panha (`panha`) | Enemy AI and models, five-wave encounter, pyramid gameplay integration, and combat fixes |
| Svay Monirath | Player movement, physics-based abilities, combat integration, and tutorial tools |
| `vanchhoeurngchao-cmyk` (`chhoeurng` branch) | Pyramid map and environment layout |

## Asset and tutorial references

- [Dungeon Modular Pack](Assets/DungeonModularPack) and [Low Poly Dungeon FREE](Assets/LowPolyDungeonFree) provide the dungeon environment assets. The latter includes an [asset setup guide](Assets/LowPolyDungeonFree/Documentation.pdf).
- The [REDLINER movement source register](docs/REBOUND_REDLINE_AGENT_PACK/REBOUND_REDLINE_AGENT_PACK/08_SOURCE_REGISTER.md) lists the movement tutorials and demonstrations used as design references.
- The [REBOUND Player System Manual](docs/PLAYER_SYSTEM_MANUAL.md) covers movement and abilities. The [Enemy Gameplay Guide](docs/REBOUND_ENEMY_GAMEPLAY_GUIDE.md) explains the enemies and how to fight them.
- OpenAI Codex assisted with implementation, debugging, and documentation.

## Gameplay video

Public gameplay video link pending.
