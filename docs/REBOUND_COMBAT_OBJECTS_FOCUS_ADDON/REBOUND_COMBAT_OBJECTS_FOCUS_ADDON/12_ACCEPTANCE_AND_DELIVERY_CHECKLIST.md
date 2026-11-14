# REBOUND — Acceptance and Agent Delivery Checklist

## 1. Functional acceptance

### Push
- [ ] Push input is read once and dispatched once.
- [ ] Push affects eligible props/enemies through one interaction path.
- [ ] Line-of-sight and target eligibility are respected.
- [ ] Player recoil is coordinated with the movement controller.
- [ ] Open-air/no-surface recoil restriction remains correct.
- [ ] Slide-forward Push boost remains correct.
- [ ] Air rocket-jump limiter remains correct.
- [ ] Projectile deflection works for explicitly eligible projectile types.
- [ ] Reflected projectile can hit its original source and cannot reflection-loop.
- [ ] Push does not also fire when Crate Cannon is launched.

### Pull and objects
- [ ] Object targeting and grapple traversal have predictable priority.
- [ ] Light/medium objects pull and hold smoothly.
- [ ] Heavy objects are ground-drag only.
- [ ] Obstruction retraction works.
- [ ] Held object collider blocks only eligible projectiles that physically hit it.
- [ ] Collision-ignore settings are restored on release/respawn/destruction.
- [ ] Anti-box-riding rule works.
- [ ] Optional enemy Pull is limited to existing configured eligible targets.
- [ ] Object release preserves appropriate physics velocity.

### Object physics
- [ ] Light, medium, heavy classes have distinct useful roles.
- [ ] Mass and relative impact speed influence impact result.
- [ ] Damage is thresholded and capped.
- [ ] Weak-point detection uses actual collider/component configuration.
- [ ] Damage and TargetButton events are deduplicated.
- [ ] Heavy objects cannot be launched by normal Crate Cannon.
- [ ] Object breakage/debris, if present, is pooled/bounded.

### Crate Cannon
- [ ] One input launches one object.
- [ ] Launch velocity definition is documented.
- [ ] Player velocity inheritance is configurable and tested.
- [ ] Player recoil goes through movement owner.
- [ ] Prediction line matches actual projectile trajectory.
- [ ] High-speed collisions do not tunnel in tested conditions.
- [ ] Enemy and weak-point impact behavior works once per collision.
- [ ] Existing TargetButton/destructible interactions still work.

### Focus
- [ ] Rechargeable meter works (unless a justified project decision keeps the old infinite design).
- [ ] Drain/recovery use unscaled time.
- [ ] Enemy/projectile/prop slowdown is consistent.
- [ ] Player input/aim remains responsive.
- [ ] Player real-time movement and collision under Focus are actually measured.
- [ ] Player impulses/grapple/landing remain stable under Focus.
- [ ] TimeScale/fixedDeltaTime restore correctly on every exit path.
- [ ] Rewards cannot be farmed.

### Enemy preservation
- [ ] No enemy archetype added/removed/replaced.
- [ ] No enemy AI/attack redesign.
- [ ] Existing enemy timings/telegraphs remain unchanged.
- [ ] Physical interactions are limited by each enemy's configured capabilities.
- [ ] Hulk armor/weak-point behavior remains intact.
- [ ] Sniper projectile ownership/deflection works.
- [ ] Sentry shield blocking works physically.
- [ ] Tripwire behavior remains unchanged.

## 2. Non-regression acceptance

- [ ] Previous movement pack features still work.
- [ ] No duplicate input readers or duplicate ability controllers.
- [ ] No movement ability directly overwrites velocity outside the authoritative movement owner.
- [ ] No player stuck holding/dangling object after death or scene change.
- [ ] No global timeScale remains altered after Focus ends.
- [ ] No stale projectile ownership after pooling.
- [ ] No stale collision-ignore pairs.
- [ ] No compile errors or missing serialized references.
- [ ] No unexplained scene/prefab changes outside scope.

## 3. Agent final handoff format

The coding agent must provide:

### A. Summary
A concise explanation of the improvements implemented.

### B. Project audit
Unity version, actual relevant scripts, movement work found, and existing conflicts.

### C. Files changed
List added/modified/deleted files and why. Include prefab/scene/ScriptableObject changes separately.

### D. Controls
Show actual current bindings after the work. Explain how LMB dispatches Push versus Crate Cannon and how RMB chooses object Pull versus grapple.

### E. Physics/tuning table
Record actual final values for object masses, pull tuning, launch speed, velocity inheritance, impact thresholds, damage caps, shield durability, Focus drain/recovery, and projectile deflection window.

### F. Focus implementation
Explain exactly how player and world simulation are separated (or what compromise is used), plus measured movement under Focus.

### G. Enemy integration
Confirm each existing enemy's behavior was preserved and describe only the receiver-level changes.

### H. Tests
List every test run with pass/fail. Explicitly list tests not run. Do not claim untested behavior works.

### I. Known issues
List remaining bugs, tuning issues, and risks.

### J. Playtest instructions
Give the user an exact short sequence to verify Push, Pull, object shielding, Crate Cannon, Focus, and enemy interactions in Unity.

## 4. Final definition of done

The task is complete when the new systems work together with the movement upgrade, the existing enemies retain their behavior, no critical lifecycle/physics bugs remain, the project compiles, and the validation results are documented. Map or theme work is not part of completion.
