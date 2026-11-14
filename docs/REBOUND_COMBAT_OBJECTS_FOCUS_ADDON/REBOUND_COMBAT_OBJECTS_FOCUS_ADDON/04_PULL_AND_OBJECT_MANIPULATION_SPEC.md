# REBOUND — Pull / Object Manipulation Technical Specification

## 1. Purpose and mode separation

RMB is a dual-mode ability:

- **Object mode:** pull an eligible movable object or configured lightweight enemy toward the player, hold it in front of the player, use it as temporary cover, and launch it with Crate Cannon.
- **Traversal mode:** attach to a valid environment anchor and use the existing grapple movement system.

The object manipulation system must not be implemented as a second grapple movement controller. It must integrate with the current `PullAbility` and current movement implementation.

## 2. Target acquisition and priority

Existing reference values:

- Object acquisition range: approximately 16 m.
- Environment grapple range: approximately 75 m.
- Normal hold distance: approximately 2.5 m in front of the player.

Verify the real project values and maintain one authoritative configuration.

Recommended selection process:

1. Read the current camera/ability ray.
2. Search the object/interactable mask first, using the current targeting rules.
3. Accept only objects whose capability data allows pulling/holding/dragging.
4. If an eligible portable object is selected, enter object mode.
5. Optionally accept an existing lightweight enemy only if its current receiver explicitly marks it pullable and the existing game design already supports this. Do not make all enemies portable.
6. If no valid object target is selected, check the environment grapple mask and delegate to the existing traversal grapple controller.
7. If neither is valid, do nothing and show a clear invalid-target/neutral state.

Do not select objects through walls. Avoid unstable target switching between adjacent colliders. Deduplicate child colliders to one logical prop.

## 3. Object states

A portable object should have a clear logical state, implemented using the project's current pattern:

- `Free`: normal world Rigidbody behavior.
- `Targeted`: highlighted/eligible, not yet held.
- `PullingToPlayer`: under spring-damper control toward hold point.
- `Held`: maintained at a target position in front of the player.
- `GroundDragged`: heavy object being pulled along the floor.
- `Launched`: released as a Crate Cannon projectile.
- `Broken/Disabled`: optional state for objects with a breakable trait.

Not every object needs every state. Heavy objects must not enter `Held` or `Launched` through normal Crate Cannon.

## 4. Light and medium object holding

- Pull should produce a smooth, controlled approach to the hold position.
- Do not teleport or parent a dynamic Rigidbody directly to the camera every frame.
- Use a spring-damper/PD force or an equivalent stable Rigidbody-aware method.
- Maintain collision with the world, enemies, and eligible projectiles.
- Prevent collision with the player body while held, using a safe layer/collision-pair approach or carefully restored `Physics.IgnoreCollision` pairs.
- Restore every altered collision pair when the object is released, destroyed, disabled, or the player respawns.
- Keep gravity/physical response consistent with the selected hold mode. If gravity is reduced or disabled while held, make that an explicit, configurable design choice and restore the previous setting on release.
- Angular damping should stabilize spinning without making the object look completely frozen.
- Avoid excessive oscillation when the player turns the camera quickly.

### Hold position

Normal hold-distance reference: approximately 2.5 m ahead of the hands/camera. Use a configurable local offset rather than hard-coding world coordinates.

When the normal hold position is obstructed:

1. SphereCast or equivalent from the hand/hold origin toward the desired hold position.
2. Move the target point inward to the nearest safe position.
3. Old handoff reference for tight-space retraction: approximately 1.2 m.
4. Smoothly restore the normal distance when clear.
5. Never allow the object to snap through walls or sit inside the player capsule.

The hold target should be positioned so the object can block projectiles when intentionally placed in front of the player, while avoiding excessive obstruction of the player's view.

## 5. Heavy object ground dragging

Heavy objects use a different mode:

- Do not lift or hover them.
- Keep the object in contact with the ground where possible.
- Apply a bounded pull force toward a ground-projected target point.
- Preserve collision with floors, walls, and other objects.
- Prevent the force from pulling the object upward due to camera height.
- If the object is blocked by geometry, it should stop or slide around only when the physical collision permits it; do not teleport it through obstacles.
- Show a different reticle label such as `DRAG` rather than `GRAB`.
- Heavy objects are not normal Crate Cannon ammunition and cannot be carried as portable shields.

The player should not be able to stand on a heavy object and use Pull to lift themselves. If the object is currently supporting the player's feet, block its pull/drag activation until the player steps off or the support condition safely ends.

## 6. Shield behavior

A held light/medium object may block eligible incoming projectile types if its collider is physically between the projectile and the player.

Requirements:

- The object's collider remains active.
- Projectile collision is resolved against the held object before the projectile reaches the player.
- The projectile's own collision/deflection/blocking logic recognizes the prop as a valid blocker.
- Shielding must not work through the object or when the object is not between the projectile and the player.
- Different object classes may have different durability/resistance. Do not assume every prop absorbs unlimited hits.
- On impact, provide a visual/audio response and optional durability damage.
- Heavy ground-drag objects can serve as physical cover when placed between the player and threat, but they are not carried.

If current project intentionally has unlimited shielding, preserve it initially and expose durability as a separately testable future tuning option rather than silently changing balance.

## 7. Pulling lightweight enemies

Only implement this if the current enemy receiver and current gameplay design support it.

- Respect `CanBePulled` and a dedicated carryable/weight capability if present.
- A light enemy may be pulled toward the player and launched only if that is already an intended behavior in REBOUND.
- Do not allow the Hulk or other heavy/anchored enemies to be carried by default.
- Do not rewrite the enemy AI. The enemy may temporarily enter its existing pulled/staggered response, but its current behavior must resume or recover through existing state logic.
- Do not let an enemy become a permanent shield or block the camera indefinitely.

If this behavior is not implemented in the current game, leave it behind a feature flag or report it as a deliberate follow-up rather than inventing a new enemy mechanic during this task.

## 8. Release and cancellation

Object Pull ends when:

- RMB is released, if hold behavior is press-and-hold in the current input design.
- The player fires Crate Cannon.
- The object is destroyed or becomes invalid.
- The player respawns or changes scene.
- The object cannot be kept in a valid state due to a defined failure condition.

On release:
- Remove the Pull spring/constraint.
- Restore gravity, damping, collision pairs, layer, and ownership settings.
- Preserve physically appropriate object velocity rather than zeroing it by default.
- Clear the player's held-object reference.
- Update HUD/reticle state.

If the player's current RMB behavior is a toggle rather than hold, preserve the actual input contract and implement equivalent state cleanup.

## 9. Target feedback

Recommended reticle states:

- Light portable object: `GRAB` with weight/class hint.
- Medium portable object: `GRAB / SHIELD`.
- Heavy object: `DRAG`.
- Eligible lightweight enemy: `PULL` only if supported.
- Static anchor: `GRAPPLE`.
- Invalid target: neutral reticle.
- Holding object: `FIRE` / Crate Cannon prompt.

Highlight must identify the actual target and disappear promptly when the target is no longer eligible.

## 10. Acceptance criteria

- RMB selects the correct mode consistently.
- Object mode does not break or replace traversal grapple.
- Light and medium props approach and hold stably without teleporting.
- Held objects retract in tight spaces without clipping through geometry.
- Held objects can block eligible projectiles physically.
- Heavy objects drag on the floor and never hover through normal Pull.
- Player cannot exploit a held/dragged object to self-levitate.
- Release, respawn, disable, and destruction restore all physics/collision settings.
