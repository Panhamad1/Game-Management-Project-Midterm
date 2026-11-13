# REBOUND — Complete Team Development Handbook & Technical Guide

Welcome to the **REBOUND** team documentation! This guide gives everyone on the team—gameplay programmers, enemy AI developers, and level designers—the exact rules, controls, physics settings, and code contracts for our game.

No complicated jargon is used here. Everything is explained in direct, simple English so you can build your part of the project without confusion.

---

## 1. Game Concept & Thematic Fantasy

**REBOUND** is a first-person, high-speed physics action and movement game built in Unity. It blends fast momentum-based parkour with pneumatic air-pressure abilities. There are no conventional guns; all offensive combat and traversal comes from your air pressure abilities, throwing environmental physics objects, and deflecting enemy attacks.

The player possesses pneumatic air-pressure abilities and sensory awareness:

- **Push (Left Mouse Button)**: A high-pressure **Wind Cannon**. Blasts physics objects away. In normal flight, it pushes your body backward with recoil. While sliding, it converts that recoil into an instant forward speed boost. Can also deflect incoming enemy sniper rounds and missiles!
- **Pull (Right Mouse Button)**: A dual-mode **Vacuum Tether**.
  - Aimed at static walls or anchors: Acts as an elastic grapple, pulling your body quickly toward the target to slingshot you into high-speed slides.
  - Aimed at highlighted crates or light enemies: Pulls the target into your hands so you can carry, shield with, or launch it.
- **Crate Cannon (LMB while holding RMB)**: Instantly fires the held object forward as a high-velocity kinetic projectile to smash doors, activate distant buttons, and crush enemies.
- **Slow Motion (Left Shift)**: Focus bullet-time. Slows world physics down to 25% speed while keeping your camera aim and movement speed at 100%.
- **Spider Sense (Threat Warning)**: Directional red warning arc that flashes on screen when high-threat attacks (Sniper laser lock, Hulk leap slam) are about to strike.

---

## 2. Controls & Core Mechanics

### Control Mapping

| Input                     | Action                         | Description                                                                 |
| ------------------------- | ------------------------------ | --------------------------------------------------------------------------- |
| **WASD**                  | Walk                           | Normal movement at 6.5 m/s; pressing Crouch while walking ducks into 3.5 m/s crouch-walk |
| **Left Shift + W + C**    | **Power Slide / Surf**         | Sprinting + Crouch powers into an explosive 14–22 m/s slide; slopes accelerate to 35 m/s! |
| **Mouse**                 | Look                           | First-person camera aim with dynamic banking (-2.5° to +2.5°) and speed FOV |
| **Space (Hold)**          | **Auto-Bunny-Hop**             | Holding Space chains frame-perfect hops on every floor contact (+2.5 m/s per hop!) |
| **A / D + Mouse (Air)**   | **Air-Strafing**               | Turning camera into air-strafe adds +1.5 to +3.5 m/s speed per turn up to 35 m/s |
| **Left Click (LMB)**      | **Push (Wind Blast / Boost)**  | Pushes objects forward; Rocket Jump against floor, Wall Kick against wall, Slide Boost in slide |
| **Right Click (RMB)**     | **Pull (Grapple / Grab)**      | 75m Grapple Hook to walls/towers OR pulls highlighted crate up to 45m to hands |
| **LMB while holding RMB** | **Crate Cannon**               | Launches the held object forward at 35.0 m/s as a high-speed kinetic projectile |
| **Q / F**                 | **Infinite Slow Motion**       | Slows world to 25% speed; zero cooldown and infinite duration, toggle on/off anytime |
| **F3**                    | **Telemetry HUD**              | Toggles real-time diagnostic physics overlay (speeds, angles, timers, forces) |
| **Automatic Visual**      | **Spider Sense**               | Directional HUD flash warning of incoming sniper rounds or leap slams       |
| **R**                     | **Quick Checkpoint Reset**     | Instantly resets player onto the safe elevated platform                    |
| **Escape**                | Pause                          | Opens pause menu / unlocks cursor                                           |

---

### Detailed Ability & Locomotion Rules

#### 1. Momentum Slide, Slope Surfing & Auto-Bunny-Hopping
- **Walking vs Crouching**: Walking at 6.5 m/s and pressing `C` or `Ctrl` ducks down into a quiet crouch-walk at 3.5 m/s. It will **not** slide by accident.
- **Power Sliding**: Sprinting forward (`Shift + W`) and pressing `C` launches you into an explosive slide at **14.0 to 22.0 m/s**.
- **Slope Acceleration**: Sliding down angled slopes converts gravity into direct forward acceleration (`28 m/s²`). You can reach the **35 m/s maximum speed cap**!
- **Hold-Space Auto-Bunny-Hop**: Players can simply hold down `Space` to continuously chain frame-perfect hops without friction loss, gaining `+2.5 m/s` boost per hop.
- **Air-Strafing Acceleration**: Turning the camera into an air-strafe with `A` or `D` adds `+1.5 to +3.5 m/s` per curve, allowing skilled players to maintain top speed across the entire map.
- **Slide Bowling Ram**: When sliding faster than 7.5 m/s, your body physically smashes through wooden barricades and sends physics crates flying like bowling pins.
- **Recoil-Convert Boost**: Blasting Push (`LMB`) while sliding redirects the explosion force into forward linear slide acceleration (+10 m/s).

#### 2. Dual-Mode Pull (75m Grapple Hook vs 45m Object Snatching)
- **Target Priority**:
  - Aiming at a movable crate, barrel, or debris within **45.0m**: Pull reels that target to hover 2.5m in front of your hands (Gravity Gun style). Uses magnetic 0.85m SphereCast fallback and weight-class reel dynamics (Light 32 m/s, Medium 24 m/s, Heavy 12 m/s).
  - Aiming at static walls, floors, towers, spires, or ceilings: Pull shoots an electric cyan cable up to **75.0 meters away**!
- **Elastic Winch Pull**: Pulls the player body toward the anchor with strong 55 m/s² spring tension while preserving tangential swinging velocity.
- **Ground Grapple Auto-Slide**: Hooking the ground or downward ramps ahead pulls you straight into a 24–34 m/s power slide.
- **Slingshot Fling**: Releasing `RMB` or pressing Jump during a grapple snaps the cable with an extra **+6.5 m/s forward boost**, catapulting you into aerial leaps or slides.
- **Crate Shielding**: Holding a crate in your hands physically blocks incoming fast-laser fire, functioning as a portable riot shield.

#### 3. Crate Cannon (Shooting Without Guns)
- Holding `RMB` to carry an object and tapping `LMB` (Push):
  - Fires the object forward at **35.0 m/s** as a high-velocity projectile following a visible aiming arc.
  - Smashes destructible wooden barricades, triggers remote Target Buttons, and crushes enemies upon impact.

#### 4. Infinite Slow Motion (Focus)
- **World Speed**: World physics and movable objects slow down to **0.25x speed** (`Time.timeScale = 0.25`).
- **Player Responsiveness**: Player look, aim, and camera remain at **1.0x normal speed**.
- **Zero Cooldown / Infinite Duration**: Players can toggle Slow-Mo on and off freely with `Q` or `F` without an energy depletion penalty or cooldown timer.

#### 5. Smart Crosshair HUD Feedback
- **Amber / Yellow Square (`[ · ]`)**: Locks onto movable crates, blocks, and buttons within 45m (`[RMB] GRAB`).
- **Electric Cyan Diamond (`+`)**: Locks onto static walls, towers, and floors within 75m (`[RMB] GRAPPLE`).
- **Neon Green Reticle**: Activates while holding a crate (`[LMB] FIRE CRATE CANNON`).
- **Neutral White**: Displayed when aiming at empty air beyond 75m.

#### 6. Spider Sense (Threat Warning Indicator)
- **Visual Alert**: When an enemy sniper finishes charging or a Hulk begins its downward slam, a sharp curved directional chevron flashes red on the player's HUD pointing toward the incoming attack.
- **Reflex Slow-Mo Window**: Pressing **Q / F** during the flash grants an instant Slow-Motion focus window, giving the player reaction time to slide under the shot, grapple out of the slam zone, or time a Push deflection.

---

## 3. Interactable Props, Weight Classes & Tight Spaces

Never hardcode object names. Attach the `InteractableProp` script to any movable object.

| Prop Type        | Mass  | Pull Behavior                                                       | Push Behavior             | Combat / Puzzle Role                                         |
| ---------------- | ----- | ------------------------------------------------------------------- | ------------------------- | ------------------------------------------------------------ |
| **Light Crate**  | ~1.0  | Fast hover at 2.5m, can be launched as projectile                   | Flies very far            | Fast projectile, rapid barricade clearing, easy mid-air grab |
| **Medium Crate** | ~5.0  | Lifted at normal speed, hovers at 2.5m                              | Moves a moderate distance | Heavy projectile, riot shield vs lasers, Hulk weak-point hit |
| **Heavy Block**  | ~25.0 | **Ground Drag Only**. Drags across floor; cannot be lifted into air | Nudges slightly           | Heavy door weights, immovable barricades, cover from snipers |

### Elastic Retraction in Narrow Spaces
- When carrying an object through tight corridors or doorways, the object smoothly retracts closer to your chest (down from 2.5m to 1.2m).
- This prevents props from snagging on doorframes, catching on walls, or blocking your forward slide.

### TargetButton.cs (Interactive Puzzle & Arena Switches)
Attach the `TargetButton` component (`Assets/_Project/Scripts/Interactables/TargetButton.cs`) to any wall switch, high tower target, or puzzle trigger:
- **Implements `IKineticTarget`**: Works automatically with all player abilities.
- **How It Activates**:
  1. Hit by flying crate fired from Crate Cannon (`LMB` while carrying).
  2. Rammed by player during high-speed slide (`> 5.5 m/s`).
  3. Blasted directly with Push (`LMB`).
- **Tactile Feedback**:
  - Depresses physically into its casing.
  - Glowing core switches from Bright Red to glowing Neon Green.
  - Plays a crisp high-energy synthesized confirmation chime.
  - Shakes the camera and rewards the player with instant Focus energy.
- **Level Design Hooks**:
  - Connect the `onActivated` UnityEvent in the Inspector to open gates, activate launch kickers, trigger elevators, or complete room objectives.

---

## 4. Enemy System & Combat Balance (The 4 Enemy Archetypes)

Because the player has extreme speed and kinetic powers, enemies are built around clear behavioral niches, telegraphed attacks, and weight tiers rather than simple hitscan attacks.

### The 4 Enemy Archetypes

#### 1. Small Fast Swarmer (Light Pack Enemy)
- **Spawns**: Packs of 4 to 8.
- **Behavior**: Fast, low-profile crawlers that scuttle along floors and walls, leaping at the player when within 5 meters.
- **How to Defeat**:
  - **High-Speed Slide (Bowling Strike)**: Ramming into a pack while sliding at high speed plows straight through them and destroys them without reducing your velocity.
  - **Crate Cannon**: Throwing a single crate in a straight line crushes multiple swarmers.
  - **Vacuum Grab**: Can be pulled directly into your hands with `RMB` and launched with `LMB` into other enemies or switches.

#### 2. Heavy Hulk Leaper (Juggernaut)
- **Spawns**: 1 or 2 at a time in large chambers or arena chokepoints.
- **Behavior**: Giant, armored mechanical brute. Crouches and leaps high into the air, targeting the player's predicted landing area.
- **Leap Slam & Shockwave**: Upon landing, creates a radial ground shockwave that forces the player to jump or grapple over it. Ramming into the Hulk stops the player's slide immediately and deals heavy collision damage.
- **Vulnerable Stun Recovery**: After landing the leap slam, the Hulk remains stunned on hands and knees for 2.5 seconds, exposing an unarmored glowing core on its back.
- **How to Defeat**: Pull a medium crate and launch it directly into its exposed back core with Crate Cannon.

#### 3. Slow Deadly Sniper (Precision High-Ground Enemy)
- **Spawns**: Perched on elevated pillars, catwalks, or distant ledges.
- **Behavior**: Tracks the player with a visible red laser targeting line for 1.5 seconds.
- **The Shot**: Right before firing, the player's Spider Sense flashes red. The sniper fires a high-velocity sonic round that deals massive damage.
- **How to Defeat / Counterplay**:
  - **Slide / Grapple Dodge**: Breaking line-of-sight or executing a slide/grapple right as Spider Sense flashes avoids the shot.
  - **Push Deflection (One-Shot Return)**: Activating Slow-Mo and tapping Push (`LMB`) right as the bullet reaches you deflects the round straight back along the laser path, instantly destroying the sniper!

#### 4. Fast Rapid-Laser Sentry (Continuous Suppression)
- **Spawns**: Corridor bottlenecks and mid-range vantage points.
- **Behavior**: Fires 3-second rapid bursts of sweeping plasma laser bolts that chip away at player health and degrade momentum.
- **How to Defeat / Counterplay**:
  - **Crate Shielding**: Holding a crate in front of you with Pull (`RMB`) absorbs all incoming laser bolts like a portable riot shield while you advance.
  - **Flanking**: Sliding under the spread or grappling behind the sentry to crush it with a point-blank crate launch.

#### 5. Laser Tripwires (Area Denial Hazard)
- Static or sweeping horizontal laser grids. Touching a beam triggers an electric shock that halts your slide and deals damage.
- Players must slide underneath low beams (0.8m) or grapple over high beams.

---

## 5. Technical Safeguards & Edge Cases

To maintain stability at high speeds (up to 35 m/s):

1. **Continuous Collision Detection (CCD)**:
   - The player capsule Rigidbody and all interactable props must use `CollisionDetectionMode.ContinuousDynamic`.
   - Level walls and obstacles use standard static mesh colliders with thickness of at least 0.5m to prevent collision tunneling.
2. **Overhead Clearance Sensor**:
   - Releasing the Crouch key while under low ceilings or pipes uses a vertical raycast (`Physics.Raycast(headOrigin, Vector3.up, 1.0m)`).
   - If blocked, the player remains crouched until clearing the obstacle, preventing clipping into geometry.
3. **Soft Velocity Cap (~35 m/s)**:
   - When horizontal velocity exceeds 35 m/s, exponential aerodynamic drag is applied to gently stabilize speed without jarring clamps.
4. **Target-Priority Raycast Layering**:
   - Raycast tests Layer 10 (`Interactables`) and Layer 12 (`Enemies`) first. If an object is hit, Pull acts as object snatch.
   - If no interactable is hit, the raycast tests Layer 9 (`Environment`) for grapple surface attachment.

---

## 6. Teammate Code Contracts

Teammates implementing enemy AI or props do not need to alter player locomotion scripts. Use these clean contracts:

### The C# Target Interface (`IKineticTarget.cs`)

```csharp
using UnityEngine;

/// <summary>
/// Interface for any enemy or object reacting to wind blasts,
/// grapple pulls, thrown props, bullet deflections, or high-speed player collisions.
/// </summary>
public interface IKineticTarget
{
    bool CanBePulled { get; }
    bool CanBePushed { get; }

    // Called when hit by wind blast or deflected bullet
    void OnPushed(Vector3 forceDirection, float forceMagnitude, Vector3 hitPoint);

    // Called while being pulled by vacuum
    void OnPulled(Transform pullOrigin, float pullForce);

    // Called when smashed by a thrown crate or prop
    void OnPhysicsCrush(float impactSpeed, GameObject flyingProp, bool hitWeakPoint);

    // Called when the player physically rams into this target at high slide speed
    void OnPlayerRam(float playerSpeed, Vector3 ramDirection);
}
```

### Ready-to-Use Enemy Component (`EnemyPhysicsReceiver.cs`)

```csharp
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyPhysicsReceiver : MonoBehaviour, IKineticTarget
{
    [Header("Enemy Armor Settings")]
    [SerializeField] private bool canBePulled = true;   // False for heavy Hulk
    [SerializeField] private bool canBePushed = true;   // False for heavy Hulk
    [SerializeField] private bool isHeavy = false;      // True for heavy Hulk
    [SerializeField] private float mass = 2.0f;

    [Header("Health & Damage")]
    [SerializeField] private float health = 100f;
    [SerializeField] private float crushSpeedThreshold = 4.0f;
    [SerializeField] private float ramSpeedThreshold = 12.0f;

    [Header("Weak Points")]
    [SerializeField] private Collider weakPointCollider;
    [SerializeField] private float weakPointDamageMultiplier = 3.0f;

    private Rigidbody rb;

    public bool CanBePulled => canBePulled;
    public bool CanBePushed => canBePushed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = mass;
    }

    public void OnPushed(Vector3 forceDirection, float forceMagnitude, Vector3 hitPoint)
    {
        if (!canBePushed) return;
        rb.AddForceAtPosition(forceDirection * forceMagnitude, hitPoint, ForceMode.Impulse);
    }

    public void OnPulled(Transform pullOrigin, float pullForce)
    {
        if (!canBePulled) return;
        Vector3 pullDir = (pullOrigin.position - transform.position).normalized;
        rb.AddForce(pullDir * pullForce, ForceMode.Acceleration);
    }

    public void OnPhysicsCrush(float impactSpeed, GameObject flyingProp, bool hitWeakPoint)
    {
        if (impactSpeed < crushSpeedThreshold) return;
        float damage = (impactSpeed - crushSpeedThreshold) * 15f;
        if (hitWeakPoint) damage *= weakPointDamageMultiplier;
        TakeDamage(damage);
    }

    public void OnPlayerRam(float playerSpeed, Vector3 ramDirection)
    {
        if (isHeavy) return; // Heavy enemies halt player and deal collision damage to player

        if (playerSpeed >= ramSpeedThreshold)
        {
            rb.AddForce(ramDirection * playerSpeed * 1.5f, ForceMode.Impulse);
            TakeDamage(playerSpeed * 6f);
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0f) Destroy(gameObject);
    }
}
```

---

## 7. Level Designer Guide (Speedrun & Combat Encounters)

When designing test courses:

1. **Swarmer Chokepoints**:
   - Place Swarmer packs at the base of long downhill ramps to reward players with high-speed bowling smashes.
2. **Hulk Arena Platforms**:
   - Place the Hulk in open arena chambers with plenty of vertical grapple anchors so the player can vault over ground shockwaves and attack from behind.
3. **Sniper High-Ground Placement**:
   - Position Snipers on elevated ledges with long sightlines. Ensure players have cover blocks or clear timing windows to deflect bullets with Slow-Mo Push.
4. **Rapid-Laser Chokepoints**:
   - Place crates 10 meters before Rapid-Laser Sentries to allow players to snatch a crate and use it as a forward riot shield.
5. **Speedrun Medal Targets**:
   - Each course displays target clear times (Gold, Silver, Bronze) with split checkpoints at major chamber transitions.

---

## 8. Physics Layers Setup in Unity

Verify these layers in **Project Settings > Tags and Layers**:

- **Layer 8: `Player`** — PlayerRoot capsule. Ignored by player raycasts.
- **Layer 9: `Environment`** — Static walls, floors, ramps, and grapple surfaces.
- **Layer 10: `Interactables`** — All crates, boxes, and movable physics objects.
- **Layer 11: `Hazards`** — Pit reset triggers, kill planes, and laser tripwires.
- **Layer 12: `Enemies`** — Enemy bodies and hurtboxes.
- **Layer 13: `Projectiles`** — Incoming sniper rounds, rockets, and rapid laser bolts.
- **Layer 14: `GrappleAnchors`** — Dedicated high-priority aerial swing points.

---

## Summary Checklist for the Team

- [x] **Locomotion Verified**: Slide, Slope Acceleration, Bunny-Hop preservation, and Recoil-Convert boost.
- [x] **Dual-Mode Pull Implemented**: Wall Grapple Slingshot vs Highlighted Prop Snatch.
- [x] **Combat Balanced Without Guns**: Crate Cannon throws, bullet deflection, crate riot shielding, and Swarmer bowling ram.
- [x] **4 Enemy Archetypes Integrated**: Swarm Rusher (pack), Hulk Leaper (ground slam), Sniper (deflectable round), Rapid Sentry (suppression).
- [x] **Player Spider Sense Added**: Directional HUD threat flash and Slow-Mo reflex window.
- [x] **Technical Edge Cases Guarded**: Continuous Collision Detection (CCD), overhead clearance sensor, elastic prop retraction, 35 m/s soft cap.
- [x] **Feedback & Presentation**: Dynamic FOV (85-105), camera banking, wind audio crescendo, speedrun medal timer.
