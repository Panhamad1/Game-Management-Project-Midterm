# REBOUND

**Mid-Term Group Project · Playable Obstacle Course**

REBOUND is a first-person parkour and physics combat game built in Unity. You are sealed inside an Egyptian pyramid. Use movement and objects in the room to survive five increasingly difficult enemy waves, then walk through the exit to finish the run.

## How the game works

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
| Q | Dash. This input also toggles Focus in the current build. |
| Left mouse | Push, deflect a shot, or throw the held item |
| Hold right mouse | Pull and carry a prop. Release to drop it. |
| F or middle mouse | Toggle Focus to slow the world |
| R | Return to the checkpoint |
| Esc | Pause. Choose Resume, Restart, or Exit. |

## Run the project

1. Clone the repository and use the default branch, `master`.
2. Open the project in **Unity 6000.3.8f1** and let Unity import the project.
3. Open [pyramid-map.unity](Assets/Scenes/pyramid-map.unity) and enter Play Mode. Choose **Play** on the in-game menu.

For the Windows 64-bit build, extract the entire `REBOUND-Windows.zip` archive, open the `REBOUND-Windows` folder, and run `REBOUND.exe`.

## Team and credits

| Contributor | Work |
| --- | --- |
| Panha (`panha`) | Enemy AI and models, five-wave encounter, pyramid gameplay integration, and combat fixes |
| Svay Monirath | Player movement, physics-based abilities, combat integration, and tutorial tools |
| `vanchhoeurngchao-cmyk` (`chhoeurng` branch) | Pyramid map and environment layout |

The environment uses the imported [Dungeon Modular Pack](Assets/DungeonModularPack) and [Low Poly Dungeon FREE](Assets/LowPolyDungeonFree) assets. The latter includes its own [setup guide](Assets/LowPolyDungeonFree/Documentation.pdf). OpenAI Codex assisted with implementation, debugging, and documentation.

## Submission links

- [Unity project repository](https://github.com/Panhamad1/Game-Management-Project-Midterm)
- **Gameplay recording** — Add the public YouTube or Google Drive link.
- **Standalone build ZIP** — Add the Google Drive or GitHub Releases download link.
