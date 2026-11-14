# REBOUND — Unity Architecture and Safe Migration Plan

## 1. Architecture goal

Integrate combat/object improvements into the existing Unity project without duplicating the movement work or creating multiple competing sources of truth.

The architecture should have clear ownership:
- Input system reads player actions.
- Ability controllers decide what an action does.
- A shared interaction/target system determines eligible targets.
- The player movement system owns player velocity changes.
- Object components own their object capabilities and tuning.
- Enemy receivers own enemy-side responses through existing damage/physics systems.
- Focus owns the global/local time-dilation state and cleanup.
- HUD/audio/VFX react to confirmed events rather than independently deciding gameplay.

## 2. Recommended responsibilities

| Responsibility | Existing system to extend or recommended owner |
|---|---|
| Input reading | Existing `PlayerAbilityInput` / Input Actions |
| Player velocity and recoil | Existing movement controller / movement coordinator |
| Push blast | Existing `PushAbility` |
| Projectile deflection | Existing projectile adapter plus Push ability |
| Object target selection | Existing `PullAbility` or a small shared targeting helper |
| Held object control | Existing `PullAbility` plus `InteractableProp` |
| Heavy ground drag | Object manipulation mode |
| Crate Cannon | Existing Pull/Push coordination or a focused launch component |
| Object impact | Shared physical impact handler / existing kinetic receivers |
| Enemy damage | Existing enemy damage receiver |
| Weak-point check | Existing enemy weak-point component/collider |
| Focus meter/time domain | Existing `SlowMotionAbility` or a focused Focus controller |
| Targeting HUD | Existing `AbilityHUD` |
| Audio | Existing procedural audio component |

These names are based on the supplied documentation. Locate actual equivalents before implementation.

## 3. One source of truth for player velocity

This is a hard requirement because movement changes are already in progress.

- Push recoil must not overwrite grapple velocity.
- Crate Cannon recoil must not overwrite slide/air velocity.
- Grapple release must not erase air-strafe momentum.
- Focus compensation must not fight the movement controller.
- Collision reactions must be coordinated.

Use the existing authoritative movement API. If none exists, add the smallest adapter to the current movement owner. Example conceptual operations:

- Add external impulse.
- Add velocity change.
- Modify horizontal component.
- Modify vertical component.
- Add slide-direction boost.
- Request controlled velocity redirect.

Avoid arbitrary `rb.velocity = newVector`/`rb.linearVelocity = newVector` calls in multiple abilities. A complete velocity assignment may still be appropriate inside the authoritative movement owner, but not scattered through ability scripts.

## 4. Suggested data model

If the existing `InteractableProp` already holds the required data, extend it rather than creating a competing component.

Conceptual enum:

```csharp
public enum PropWeightClass
{
    Light,
    Medium,
    Heavy
}
```

Conceptual capability fields:

```csharp
[System.Serializable]
public class PropCapabilities
{
    public bool canBePulled;
    public bool canBeHeld;
    public bool canBeGroundDragged;
    public bool canBeLaunched;
    public bool canBlockProjectiles;
    public bool canBreak;
}
```

The actual project may use ScriptableObjects, serialized fields, or another data pattern. Do not create two sources of truth for mass/capabilities.

## 5. Suggested object manipulation algorithm

For a light/medium held object, a stable controller can use a spring-damper/PD model:

`force = spring * (targetPosition - objectPosition) - damping * (objectVelocity - targetVelocity)`

Then clamp the resulting force/acceleration to the class profile's maximum. The exact gains must be tuned in Unity; do not blindly apply the old `10.0` multiplier to every mass class.

For heavy drag:
- Project desired target onto the ground plane.
- Keep target height aligned with the object's ground-contact height.
- Apply bounded horizontal pull.
- Let colliders determine whether it can move around obstacles.

Avoid direct Transform movement for dynamic Rigidbodies.

## 6. Suggested event flow

```text
Input action
    |
    v
PlayerAbilityInput
    |
    +-- LMB while valid object held --> Crate Cannon launch
    |
    +-- LMB otherwise -------------> PushAbility
    |                                  |
    |                                  +--> deflection candidate check
    |                                  +--> blast target check
    |                                  +--> movement recoil request
    |
    +-- RMB ------------------------> PullAbility target selection
                                       |
                                       +--> object pull/hold/drag
                                       |
                                       +--> existing traversal grapple

Crate Cannon projectile
    |
    v
Unity collision / impact resolver
    |
    +--> prop profile + relative impact energy
    +--> target receiver / weak-point check
    +--> damage/knockback once
    +--> Focus reward event if eligible

Focus input
    |
    v
Focus resource manager
    |
    +--> drain/recharge using unscaled time
    +--> world time dilation
    +--> player time-domain behavior
    +--> HUD/audio feedback
```

## 7. Unity update responsibilities

- `Update` / Input System callbacks: collect button states, UI, camera look, target presentation, unscaled Focus meter.
- `FixedUpdate`: physics forces, held-object spring forces, projectile impacts, player Rigidbody interaction, and time-domain physics work.
- `LateUpdate`: camera presentation and reticle placement if that matches current architecture.
- Collision callbacks: capture collision data and dispatch one logical impact event. Avoid doing large searches or allocating data unnecessarily.

Respect Unity's actual version. Verify whether `Rigidbody.linearVelocity` or `Rigidbody.velocity` is the correct API for the project version. Do not mix APIs from different Unity versions without a deliberate compatibility layer.

## 8. Input conflict audit

The handoff contains several conflicting Focus bindings:
- Q/F in the newer player spec.
- Left Shift in one older table.
- Shift also appears as sprint in older movement controls.
- The previous movement pack may map Shift to a new movement action.

The actual Input Actions asset and latest code decide. Do not rebind without permission. Ensure LMB routes to exactly one action when holding a launchable object.

## 9. Physics layer audit

The old handoff references:
- Player.
- Environment.
- Interactables.
- Hazards.
- Enemies.
- Projectiles.
- GrappleAnchors.

Do not assume the layer numbers are still correct. Inspect Project Settings and every relevant prefab. Use named serialized LayerMasks rather than hard-coded numeric layer indices where possible.

Verify:
- Player does not raycast itself.
- Held object still collides with world/projectiles but does not destabilize the player.
- Launched object collides with enemies and environment.
- Deflected projectile collides with intended targets and not its new owner.
- Environment anchor detection does not accidentally select ordinary movable props.

## 10. Staged migration and build discipline

For each implementation stage:
1. Modify a small, coherent set of files.
2. Run compile/import checks.
3. Fix all errors before moving on.
4. Test the feature in a small isolated scene or existing test area.
5. Run relevant movement regression tests.
6. Record any serialized prefab/scene changes.

Do not rewrite the entire player prefab or scene as a shortcut.

## 11. Failure and lifecycle cleanup

Handle:
- Player death/respawn.
- Scene unload/reload.
- Object destruction while held.
- Ability component disable.
- Application pause.
- Focus interrupted by menus.
- Projectile pool reuse.
- Enemy destruction while a target reference is held.
- Collision pair restoration.
- TimeScale and fixedDeltaTime restoration.

No object may remain stuck in a held layer, no projectile may retain old ownership after pooling, and the game must not remain slowed after Focus exits.
