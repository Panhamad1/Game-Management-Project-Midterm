# REBOUND — Complete Team Development Handbook & Technical Guide

Welcome to the **REBOUND** team documentation! This guide gives everyone on the team—gamepy programmers, enemy AI developers, and level designers—the exact rules, controls, physics settings, and code contracts for our game.

No complicated jargon is used here. Everything is explained in direct, simple English so you can build your part of the project without confusion.

---

## 1. Game Concept & Thematic Fantasy

**REBOUND** is a first-person physics action-puzzle game built in Unity.

The player has pneumatic air-pressure abilities:

- **Push (Left Mouse Button)**: A high-pressure **Wind Cannon**. It fires an explosive blast of air that knocks objects away. Because of Newton's third law (action and reaction), blasting the air pushes your own body backward with strong recoil.
- **Pull (Right Mouse Button)**: A **Vacuum Vortex**. It creates a low-pressure suction funnel that pulls crates, objects, and enemy projectiles toward your hand, letting you carry and aim them.
- **Slow Motion (Left Shift)**: Focus bullet-time. The world slows down to 25% speed, but you run, look, and aim at normal speed.

---

## 2. Controls & Core Mechanics

### Control Mapping

| Input                     | Action                   | Description                                                       |
| ------------------------- | ------------------------ | ----------------------------------------------------------------- |
| **WASD**                  | Move                     | Responsive ground walking and air steering                        |
| **Mouse**                 | Look                     | Standard first-person camera aim                                  |
| **Space**                 | Jump                     | Vertical ground jump                                              |
| **Left Click (LMB)**      | **Push (Wind Blast)**    | Blasts target block forward; kicks player backward with 3D recoil |
| **Right Click (RMB)**     | **Pull (Vacuum Vortex)** | Reels in a block and hovers it 2.5m in front of your face         |
| **LMB while holding RMB** | **Crate Cannon**         | Instantly launches the carried block forward like a projectile    |
| **Left Shift**            | **Slow Motion**          | Slows down world physics; gives you floaty air-time               |
| **Escape**                | Pause                    | Opens pause menu                                                  |

---

### Detailed Ability Rules

#### 1. Push & 3D Rocket Recoil

- **Targeting**: Single-target raycast up to **12 meters**. Only the exact block under your crosshair is pushed (no accidental disturbance of nearby puzzle blocks).
- **Constant Recoil**: The recoil force on the player is **always fixed**. It does not change with distance to the target.
- **Rocket Jumps**: Recoil pushes you directly opposite to your camera aim. If you look straight down and click Push, you launch high into the air.
- **Empty Air Push**: Pushing into thin air still gives you full recoil backward.
- **Cooldown**: **0.45 seconds** between pushes to prevent infinite flight while rewarding rhythmic platforming jumps.

#### 2. Pull & Physics Carry

- **Suction & Float**: Holding Right Click reels in a block. Once it reaches **2.5 meters** in front of you, it floats steadily and rotates with your camera.
- **Energy Tether**: A glowing beam (`LineRenderer`) connects your hand to the block while pulling.
- **Releasing**: Letting go of Right Click drops the block smoothly under normal gravity.
- **Leash Break**: If the block gets wedged behind a wall (line of sight broken) or is dragged farther than **15 meters**, the tether automatically snaps and releases the block.
- **Anti-Box-Riding Glitch**: If you are standing on top of a block, you **cannot** pull it. This prevents the flying box glitch.

#### 3. Crate Cannon (Combo Launch)

- If you hold Right Click (carrying a block) and tap Left Click (Push):
  - The pull lock instantly disengages.
  - The block is fired forward at high speed as a kinetic projectile.
  - You receive standard push recoil backward.

#### 4. Slow Motion (Bullet-Time)

- **World Speed**: World physics and movable objects slow down to **0.25x speed** (`Time.timeScale = 0.25`).
- **Player Ground Speed**: Player walk, run, and camera look speeds remain at **1.0x normal speed**.
- **Floaty Air-Time**: While in the air during SlowMo, your fall gravity is reduced so you float down gently, giving you plenty of time to aim, push, and pull mid-air.
- **Energy Bar**: Lasts for **4 seconds** of continuous use; recharges in **6 seconds**. Turns off automatically when empty.

---

## 3. Interactable Puzzle Blocks & Weight Classes

Never hardcode object names or tags. Attach the `InteractableProp` script to any movable object.

| Block Type       | Mass  | Pull Behavior                                                       | Push Behavior             | Primary Purpose                                              |
| ---------------- | ----- | ------------------------------------------------------------------- | ------------------------- | ------------------------------------------------------------ |
| **Light Crate**  | ~1.0  | Easily lifted, hovers at 2.5m, can be thrown                        | Flies very far            | Fast projectile, simple stacking, quick puzzle triggers      |
| **Medium Crate** | ~5.0  | Lifted at normal speed, hovers at 2.5m                              | Moves a moderate distance | Standard puzzle block, counter-weights, combat projectile    |
| **Heavy Block**  | ~25.0 | **Ground Drag Only**. Drags across floor; cannot be lifted into air | Nudges slightly           | Heavy door weights, immovable barricades, ground positioning |

### Visual Feedback

- When the crosshair points at an interactable within 12 meters, the crosshair expands and the block shows a highlighted outline glow.
- Looking away immediately disables the glow.
- Safe collision is enabled: blocks gently bump and slide off the player's body without crushing or clipping.

---

## 4. Enemy System & Combat Balance (For Enemy Developers)

Because the player has strong movement and telekinetic abilities, enemies must not be mindless targets. We balance the game using **3 clear enemy archetypes** and a **weight/armor tier system**.

### The 3 Enemy Archetypes

#### 1. Swarm Rusher (Light Enemy)

- **Behavior**: Fast, fragile melee crawlers that run at the player in small packs.
- **Power Reaction**:
  - **Pull**: Can be sucked in and carried.
  - **Push**: Blown backward or knocked down.
- **How They Threaten the Player**: If they surround you while your Wind Blast is on its 0.45s cooldown, they will deal quick melee damage.

#### 2. Ranged Shooter / Turret (Medium Enemy)

- **Behavior**: Stationary turrets or wall snipers that fire visible, slow-to-medium speed rockets, grenades, or plasma orbs.
- **Power Reaction (Deflect & Catch)**:
  - **Push Deflection**: Tapping Push (Left Click) right before an incoming projectile hits you deflects it straight back at the enemy!
  - **Pull Catching**: Holding Pull (Right Click) catches slow grenades or rockets mid-air, allowing you to fire them back using Crate Cannon!
  - **Slow Motion Synergy**: Players use SlowMo to time projectile catches and deflections cleanly.

#### 3. Armored Brute (Heavy Enemy)

- **Behavior**: Slow-moving mechanical juggernauts with front shields or heavy armor plating.
- **Power Reaction (Immune to Direct Air)**:
  - **Direct Push**: Does nothing (they are too heavy).
  - **Direct Pull**: Does nothing.
- **How to Defeat**: Players must use **indirect physics combat**:
  - Grab a medium crate or explosive barrel using Pull.
  - Fire it with **Crate Cannon** at their unarmored back or head to crush them!

---

## 5. Teammate Code Contract: Connecting Enemies to Abilities

If you are building the Enemy AI, you do **not** need to touch the player's movement or ability code.

Simply implement the `IKineticTarget` interface or attach the `EnemyPhysicsReceiver` component to your enemy prefab.

### The C# Interface (`IKineticTarget.cs`)

```csharp
using UnityEngine;

/// <summary>
/// Attach this interface to any enemy or object that should react to
/// the player's Wind Blast (Push), Vacuum Vortex (Pull), or thrown crates.
/// </summary>
public interface IKineticTarget
{
    // Can this enemy be lifted and pulled toward the player?
    bool CanBePulled { get; }

    // Can this enemy be shoved backward by wind blasts?
    bool CanBePushed { get; }

    // Called when the player hits this enemy with a Wind Blast (Push)
    void OnPushed(Vector3 forceDirection, float forceMagnitude, Vector3 hitPoint);

    // Called while the player is pulling this enemy with Vacuum Vortex
    void OnPulled(Transform pullOrigin, float pullForce);

    // Called when a thrown crate or heavy physics prop smashes into this enemy
    void OnPhysicsCrush(float impactSpeed, GameObject flyingProp);
}
```

### Ready-to-Use Enemy Component (`EnemyPhysicsReceiver.cs`)

```csharp
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyPhysicsReceiver : MonoBehaviour, IKineticTarget
{
    [Header("Enemy Armor Settings")]
    [SerializeField] private bool canBePulled = true;   // False for heavy brutes
    [SerializeField] private bool canBePushed = true;   // False for heavy brutes
    [SerializeField] private float mass = 2.0f;

    [Header("Health & Damage")]
    [SerializeField] private float health = 100f;
    [SerializeField] private float crushSpeedThreshold = 4.0f; // Minimum speed to hurt

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

        // Apply knockback impulse
        rb.AddForceAtPosition(forceDirection * forceMagnitude, hitPoint, ForceMode.Impulse);

        // Optional: Trigger stumble/knockdown animation here
    }

    public void OnPulled(Transform pullOrigin, float pullForce)
    {
        if (!canBePulled) return;

        // Pull enemy towards player suction point
        Vector3 pullDir = (pullOrigin.position - transform.position).normalized;
        rb.AddForce(pullDir * pullForce, ForceMode.Acceleration);
    }

    public void OnPhysicsCrush(float impactSpeed, GameObject flyingProp)
    {
        if (impactSpeed < crushSpeedThreshold) return;

        // Calculate damage based on how fast the crate was flying
        float damage = (impactSpeed - crushSpeedThreshold) * 15f;
        health -= damage;

        Debug.Log($"{gameObject.name} took {damage} crush damage from {flyingProp.name}! Remaining HP: {health}");

        if (health <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        // Play death effect / ragdoll / destroy
        Destroy(gameObject);
    }
}
```

---

## 6. Level Designer Guide (For Map Builders)

When creating puzzle rooms and combat arenas:

1. **Drop In the Player**:
   - Drag the `Player_Rig.prefab` into your scene. It has the camera, physics, UI crosshair, and energy meter built in.
2. **Placing Puzzle Blocks**:
   - Use `Block_Light.prefab` for tall stacking or items that must be carried across long gaps.
   - Use `Block_Medium.prefab` for pressure plates and standard obstacle clearing.
   - Use `Block_Heavy.prefab` for ground-drag puzzles, anchor switches, or barricades.
3. **Designing Rocket Jump Gaps**:
   - A standard ground jump clears **1.5 meters** vertically.
   - A downward Wind Blast (Rocket Jump) boosts the player **4.5 to 6 meters** into the air.
   - Use high platforms that can only be reached with a well-timed rocket jump or a crate elevator.
4. **Combat Encounters**:
   - Place 1 or 2 Turrets on elevated ledges with crates placed nearby so the player can either deflect their shots or use crates as portable shields.
   - Place Swarmers in narrow corridors to encourage the player to use Wind Blast like a crowd-clearing shotgun.

---

## 7. Physics Layers Setup in Unity

To ensure raycasts and physics interact cleanly without glitching, verify these layers under **Project Settings > Tags and Layers**:

- **Layer 8: `Player`** — PlayerRoot capsule. Ignored by player raycasts.
- **Layer 9: `Environment`** — Static walls, floors, ramps, and ceilings.
- **Layer 10: `Interactables`** — All crates, boxes, and movable physics objects.
- **Layer 11: `Hazards`** — Pit reset triggers and deadly traps.
- **Layer 12: `Enemies`** — Enemy bodies and hurtboxes.
- **Layer 13: `Projectiles`** — Incoming rockets, grenades, and plasma orbs.

---

## Summary Checklist for the Team

- [x] **Controls Confirmed**: Left Click = Push, Right Click = Pull, Left Shift = SlowMo, Space = Jump.
- [x] **Physics Rules Locked**: 3D Rocket Recoil, 2.5m Physics Carry, 0.45s push cooldown.
- [x] **Glitches Guarded**: No box-riding flight, 15m leash break, safe pushback collisions.
- [x] **Enemies Integrated**: 3 archetypes (Swarm, Turret, Brute) with deflection and crushing.
- [x] **Teammate Code Ready**: `IKineticTarget` interface and `EnemyPhysicsReceiver` component.
- [x] **Prefabs Packaged**: `Player_Rig`, `Block_Light`, `Block_Medium`, `Block_Heavy`.
