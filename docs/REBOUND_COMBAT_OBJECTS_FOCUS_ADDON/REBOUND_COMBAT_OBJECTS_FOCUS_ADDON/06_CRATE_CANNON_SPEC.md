# REBOUND — Crate Cannon Technical Specification

## 1. Purpose

Crate Cannon converts a currently held light or medium physical object into a high-speed projectile. It is REBOUND's main deliberate object-launch attack and should reward object selection, positioning, timing, and aiming.

It must work while the player is moving and must preserve appropriate player momentum.

## 2. Activation and input priority

- The player must currently hold a valid launchable object.
- Pressing the existing Push input (LMB in the current handoff) while holding the object fires Crate Cannon.
- Crate Cannon takes priority over normal Push for that input event.
- The object is detached from Pull before launch.
- Clear the held-object reference and restore the object's free-flight physics/collision state before applying launch velocity.
- Do not allow one input event to both launch the object and fire the ordinary Wind Cannon.
- Heavy ground-drag objects are not launchable by normal Crate Cannon.

## 3. Launch velocity and player momentum

Existing reference:
- Aim-directed launch speed component: approximately 35 m/s.
- Player recoil reference: approximately 6.5 m/s opposite the aim direction.

The implementation must decide and document the meaning of 35 m/s:

**Recommended definition:** 35 m/s is the base aim-directed launch component, with a configurable fraction of the player's current velocity inherited by the projectile:

`projectileVelocity = aimDirection * launchSpeed + playerVelocity * playerVelocityInheritance`

This means the resulting world-space speed may be greater or lower than 35 m/s depending on player motion and aim direction. If the current project instead treats 35 m/s as the projectile's total world speed, preserve that behavior and document it. Do not silently switch between these interpretations.

Start with a configurable inheritance factor and tune it. The goal is to make firing during movement feel connected to player momentum without creating uncontrolled speed escalation.

Player recoil must be submitted through the authoritative movement controller's external impulse/velocity API. Do not replace the player's full velocity vector. Preserve components not intentionally modified by recoil.

## 4. Object release sequence

Recommended sequence:

1. Validate held object and launch eligibility.
2. Capture its Rigidbody, mass, profile, current velocity, and player velocity.
3. Clear Pull ownership/hold constraints.
4. Restore collision with the player if it was ignored, while preventing an immediate self-collision if the launch starts inside the capsule (use a short safe launch grace or spawn offset, not permanent collision suppression).
5. Set the object to free projectile state and apply its launch velocity once.
6. Apply player recoil once.
7. Play launch sound, muzzle/air-pressure effect, camera response, and projectile trail if appropriate.
8. Show trajectory prediction until the projectile is launched or the player changes target/aim materially.

## 5. Trajectory prediction

Existing reference: 20 LineRenderer points/steps.

Prediction must use the same inputs as actual projectile motion:
- Launch origin.
- Launch velocity, including inherited player velocity.
- Gravity under current Focus/time configuration.
- Drag where applicable.
- Projectile collision radius/shape as closely as practical.
- Relevant collision mask.

A simple ballistic approximation is acceptable if the actual projectile has no meaningful drag:

`p(t) = p0 + v0 * t + 0.5 * g * t^2`

The line must not imply that a projectile will pass through a wall or target. Use ray/sphere casts between predicted segments if practical. Keep the number of segments configurable and avoid recalculating an expensive path every render frame when aim has not changed.

## 6. Impact and damage

- Use the shared object impact system described in the Object Physics specification.
- Calculate impact using relative velocity and contact normal.
- Apply object-class impact multiplier and a maximum damage cap.
- Weak-point multiplier applies only when the impact point intersects the configured weak-point collider/component.
- Medium objects should be the most reliable intended Crate Cannon tool for the Hulk's existing exposed back/core weak point.
- Light objects remain useful against swarmers and lower-resistance targets.
- Heavy objects are ground-drag only and should not be fired through the normal Crate Cannon.
- Preserve existing destructible barricade and TargetButton interactions.
- Do not destroy every object on contact; the object profile decides whether it bounces, stops, damages, or breaks.

## 7. Enemy interaction

Preserve current enemy AI and attack patterns.

- Swarmer: receive the existing physical impact response; a launched prop may hit multiple enemies only if ordinary collision allows it and each enemy is deduplicated.
- Hulk: ordinary body armor remains resistant; the existing exposed weak point receives the configured multiplier.
- Sniper: physical hit behavior follows current receiver rules; do not change aiming or firing AI.
- Rapid sentry: receive existing damage/knockback behavior if currently supported; do not redesign its firing pattern.

## 8. Edge cases

- Object destroyed while held: clear held state and cancel launch.
- Player respawns during hold: safely release/reset the object and clear reference.
- Object becomes non-launchable: show no Fire prompt and do not launch.
- Input repeats due to frame rate: one launch per press event.
- Projector/line prediction has no gravity parity: fix so prediction matches actual path.
- Focus toggled during flight: prediction updates to the correct simulation assumption or gracefully hides/recomputes.
- Projectile hits a compound enemy collider: one logical damage event.
- Projectile collides with the player immediately after launch: prevent self-hit without making it permanently non-collidable.

## 9. Feedback

- Reticle changes to an obvious Crate Cannon state while holding a launchable object.
- Display object class/weight label where useful.
- Use a distinct pneumatic launch sound and short visual effect.
- Impact feedback should reflect the object's mass and target response without covering the screen.
- A successful weak-point hit should have a distinct cue.

## 10. Acceptance criteria

- Only valid portable objects can be launched.
- Heavy drag-only objects cannot be launched normally.
- One LMB press fires exactly one object and does not also fire normal Push.
- Projectile velocity follows the documented launch and inheritance formula.
- Player recoil does not erase unrelated momentum.
- Prediction path matches actual motion within a defined tolerance.
- Impact damage and weak-point logic are consistent and deduplicated.
- Existing TargetButton and destructible interactions still work.
