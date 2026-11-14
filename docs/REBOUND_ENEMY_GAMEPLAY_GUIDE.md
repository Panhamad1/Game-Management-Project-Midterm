# REBOUND Enemy Gameplay Guide

This guide describes the four playable enemy encounters and the default values in the current Unity project. You can tune the values on each enemy prefab in the Inspector. See [REBOUND Enemy Design](REBOUND_ENEMY_DESIGN.md) for the original design plan.

## Player controls during combat

| Input | What it does |
| --- | --- |
| Left click with empty hands | **Push** hits what you aim at within 12 m. A Push aimed at a nearby surface can also move you away from it. |
| Hold right click on a crate or Swarmer | **Pull** grabs eligible targets within 45 m and brings them toward you. Release right click to drop what you're holding. |
| Left click while holding an item | **Crate Cannon** launches the held item at 35 m/s. Aim at the enemy before firing. |
| Hold right click on an anchor or wall | **Grapple** pulls you toward the surface. Use it to get across a room or over a ground attack. |
| Q, F, or middle click | **Focus** toggles quarter-speed gameplay so you have more real time to aim, dodge, or return a Sniper round. |
| Shift, then C while sprinting | **Power Slide** lets you approach quickly. A fast slide can defeat a Swarmer it reaches. |
| Space | Jump over ground attacks. |

## Small Fast Swarmer

**20 HP.** It is a flying enemy from the moment it spawns. It rises about 1 m above its spawn point when there is room. With no visible player, it flies between random points within roughly 5 m of its home at up to 2 m/s. It chooses a new point about every 1.5–3 seconds.

### How it attacks

1. It spots you within **25 m** if it can see you, then flies toward you at up to **10 m/s**. It can follow you upward, so staying above the ground alone won't keep you safe.
2. Inside **5 m**, it turns **yellow** and pauses for about **0.35 seconds**. Move sideways during this warning. The Swarmer cancels the wind-up if it loses sight of you or you leave attack range before the launch.
3. It turns red-orange and lunges in the direction it aimed at launch, at **11 m/s**. Contact during the lunge deals **15 damage** once. It recovers for about **1 second** before starting another attack.

### How to fight it

- **Push** knocks it away and deals **10 damage**. Two Push hits defeat a full-health Swarmer.
- **Grab or drop** it with right click to interrupt its attack. Picking it up and dropping it deal zero damage. It resumes flying after a short recovery.
- **Grab, then fire** to use it as ammo. A launched Swarmer takes **10 damage** when it hits something or its **5-second** projectile timer ends. A full-health Swarmer survives its first launch. You can Push it once and then launch it to take its remaining health. An enemy hit by the launched Swarmer can also take impact damage according to that enemy's rules.
- **Fire a crate** at it. The impact damage depends on how fast the crate and Swarmer collide. A clean high-speed hit can defeat it outright.
- **Slide into it** at **7.5 m/s or faster** to defeat it if your paths meet. Against a pack, a slide can clear several at once when they dive low enough. Use Push or Pull on the ones that stay above the slide.

## Rapid-Laser Sentry

**30 HP.** The Sentry stays in place and rotates its turret to track you. It starts firing within **30 m** when it can see you and its turret is aimed at you.

### How it attacks

1. It fires **three bolts**, **0.25 seconds apart**. The burst sweeps slightly across its aim, so the bolts travel on different paths.
2. Each bolt travels at **25 m/s** and deals **5 damage** if it reaches you. A full burst deals **15 damage** if all three bolts hit.
3. It waits **1.5 seconds after the last shot** before firing again. Breaking line of sight behind a wall stops the current burst.

### How to fight it

- **Hold a crate in front of you.** A bolt stops at the first solid object it hits. Advance with the crate between you and the turret. A bolt that passes around the crate can still hit you. Walls and other solid cover can stop bolts too.
- **Use the pause.** Count the three shots, then cross the gap, slide past the firing lane, or grapple around to another angle.
- **Push or launch a crate.** Each successful Push or qualifying crate impact deals **10 damage**. Three hits in any combination defeat it. If you're using the crate as a shield, firing it gives up your cover.
- The Sentry itself cannot be grabbed. Sliding straight into the turret deals no damage, and its bolts cannot be returned with Focus and Push.

## Hulk Leaper

**90 HP.** The Hulk is an armored melee enemy. It detects you within **24 m** when it can see you and walks toward you at about **3 m/s**.

### How it attacks

- **Melee.** Within **2.3 m**, it can hit for **20 damage**. The hit cancels a slide and knocks you away with a small lift. Its melee cooldown is **1.35 seconds**.
- **Leap warning.** Within **12 m**, when its slam is ready, the Hulk flashes **orange** for **0.8 seconds**. It aims its leap toward where it expects you to be based on your current movement. Change direction after the warning to get away from the landing spot.
- **Shockwave.** On landing, an orange ring spreads outward at **12 m/s**, up to **14 m** from the impact. If the ring reaches you while your feet are near ground level, it deals **30 damage**, cancels your slide, and launches you up and away. The knockback is stronger near the center. Each wave can hit you once. Jump or grapple high enough for the ring to pass under you, or get outside its radius.
- **Recovery.** The Hulk is vulnerable for **2.5 seconds** after landing. Its armor darkens and the glowing core on its back opens. The slam cooldown is **6 seconds** from the start of recovery.

### How to fight it

1. **Pick up a nearby crate or another grabbable physics item.** The Hulk Test Crate is a regular medium crate. It exists so you always have ammo for the test encounter.
2. **Dodge the slam.** Grapple over the ring, jump as the ring reaches you, or move away from the landing point.
3. **Launch the item during recovery.** A fast item can damage the Hulk anywhere on its body during this window. A qualifying hit needs at least **4 m/s** of impact speed. A regular body hit deals damage based on impact speed. The glowing back core gives **3×** damage, so a clean full-speed crate hit to the core can defeat a full-health Hulk in one shot.
4. **Back away when the core closes.** Wait for the next opening if you need another hit. The Hulk can't be grabbed, and regular Push and slide rams deal no damage.

## Slow Deadly Sniper

**30 HP.** The Sniper stays in one position and looks for a clear shot within **55 m**. It works well from a ledge or across an open room.

### How it attacks

1. A **red laser** tracks you during a **1.5-second lock**. The visible line runs slightly below your eye level, so you can see where it comes from.
2. In the final **0.3 seconds**, the line widens and brightens toward orange, and the camera gives a small shake. This is the cue that the shot is about to fire.
3. It fires **one round** at **42 m/s**, along the aim direction at the time of firing. The round deals **60 damage** if it hits you. The Sniper then reloads for **3 seconds**.
4. Breaking line of sight before it shoots cancels the lock. The Sniper starts a fresh lock when it sees you again. A wall or solid crate can block a round already in flight.

### How to fight it

- **Dodge across the firing line.** The Sniper keeps tracking until it fires. Use the bright orange warning to get behind cover, or time a sideways slide or grapple so you move clear of the shot as it comes toward you.
- **Return the round.** Keep your hands free. Toggle **Focus**, face the incoming round, and **left-click Push** when you can aim at the round within **12 m**. A successful Push sends it back toward the Sniper. It defeats the Sniper in one hit if the path is clear. Cover can also intercept the returned round.
- **Use the reload window.** Direct Push hits and qualifying item impacts deal **10 damage** each. Three hits in any combination defeat the Sniper. You can shoot a crate from farther away than Push reaches. The Sniper itself cannot be grabbed or damaged by a slide ram.

## Testing the encounters in Unity

1. Open a scene with `Player_Rig` and point the rig toward open floor. For the built-in arena, open `Assets/Scenes/SampleScene.unity`. If you need to create the arena and player, use **Tools → REBOUND → 1. Setup Complete Game (Arena + Player)**.
2. In Edit Mode, open **Tools → REBOUND → Enemies**. Choose **Spawn Swarmer Test Pack**, **Spawn Sentry Test Encounter**, **Spawn Hulk Test Encounter**, or **Spawn Sniper Test Encounter**. The Swarmer pack places four enemies. The Sentry and Hulk encounters each provide a test crate.
3. Enter Play Mode. The test menus add a temporary health component with **100 HP** to the player. The Console shows remaining HP after each hit. At zero HP, the player respawns at the checkpoint with full test health.
4. Stop and restart Play Mode to restore the original enemy positions and health. **R** respawns the player, so defeated enemies stay defeated during the current run.

### Quick checks

| Encounter | What to try |
| --- | --- |
| Swarmer | Grab and drop it without firing. Then Push once and launch it to finish it. |
| Sentry | Count three shots. Hold a crate through one burst, then fire it during the pause. |
| Hulk | Watch for orange, jump over the ring, and fire into its body during recovery. Try the back core on the next run to compare. |
| Sniper | Break one lock with cover. On the next lock, activate Focus and Push the incoming round back. |
