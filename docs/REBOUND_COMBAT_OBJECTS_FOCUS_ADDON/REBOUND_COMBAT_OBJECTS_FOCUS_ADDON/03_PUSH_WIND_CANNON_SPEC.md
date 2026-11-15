# REBOUND — Push / Wind Cannon Technical Specification

## 1. Purpose

Push is the player's immediate pneumatic blast. It should serve four related functions:

1. Push eligible enemies and physical objects.
2. Interrupt or displace eligible attacks/targets.
3. Deflect eligible incoming projectiles with correct timing.
4. Apply the existing player recoil/propulsion behavior when surface and movement conditions permit.

Push must remain fast, readable, and reliable. It should not become a slow lock-on ability.

## 2. Input and dispatch

- Default action remains the project's current Push input (the handoff uses LMB).
- Input is collected in `Update`/Input System callbacks and dispatched once per press or configured action phase.
- Physics effects execute in the physics update or through a queued command consumed by the movement/physics coordinator.
- A single LMB event must not activate both normal Push and Crate Cannon. If the player is holding a valid launchable object, Crate Cannon has priority and normal Push is suppressed for that same event.
- If the player is not holding a launchable object, normal Push proceeds.
- If the current design supports charge/hold behavior, preserve it; do not silently convert it.

## 3. Blast targeting

Starting reference from the existing player specification:

- Range: 12 m.
- Target impulse: approximately 18 N·s for ordinary Rigidbody targets.

These are tunable starting values. Inspect the actual project before changing them.

Target behavior:

- Use a clear camera/ability origin and an explicit layer mask.
- Ignore the player and the currently held object unless a specific interaction requires it.
- Use line of sight so a blast does not affect targets through solid walls.
- Respect `CanBePushed` or the equivalent target capability.
- Apply force at a hit point when the target's shape and current behavior support it; this may create rotation as well as translation.
- Avoid applying the same blast multiple times to the same collider hierarchy.
- Use falloff by distance only if the current design already supports it or playtests show it is required. If added, make the curve configurable and test it; do not invent an arbitrary sharp cutoff.

For physics props, apply an impulse in Newton-seconds (`ForceMode.Impulse` in Unity). Do not pass an impulse value to an API that expects force per second without conversion.

For enemies implementing a target receiver, call the receiver once with direction, magnitude/impulse, and impact point. Do not separately apply Rigidbody force and receiver damage unless the receiver contract explicitly expects both; otherwise the target will be double-pushed.

## 4. Player recoil / self-propulsion

Preserve the existing REBOUND recoil design where it exists:

- A nearby surface is required for ordinary surface recoil.
- Reference proximity boundary: 3.8 m.
- Reference kick speed: 13.5 m/s.
- Existing formula reference: `kickSpeed * (0.8 + (1 - distance / 3.8) * 0.4)` for a valid nearby surface.
- Open air beyond the configured surface-proximity boundary must not accidentally generate ordinary surface recoil.
- A downward Push can provide the existing single airborne rocket-jump opportunity if current code supports it.
- Respect the existing `hasAirRocketJumped`/equivalent limiter; do not allow unlimited vertical launches by spamming Push.
- Subsequent airborne blasts may redirect/cancel vertical movement according to current design but should not erase horizontal momentum.
- On a vertical wall, use the existing wall-kick redirection behavior if present.
- While power-sliding, preserve the existing forward slide-boost behavior (reference +10 m/s), rather than applying a contradictory backward recoil.

All player movement effects must be submitted to the authoritative movement system, e.g. `ApplyExternalImpulse`, `AddVelocityChange`, or the current equivalent. Do not directly replace `Rigidbody.linearVelocity` in Push if that would overwrite grapple, slide, or air-strafe velocity. The movement owner must decide which vector components are modified.

## 5. Projectile deflection

### Eligibility

Not every projectile should be deflectable. Each projectile type must explicitly declare whether it can be deflected by Push.

At minimum, investigate and support the existing sniper projectile and rapid-sentry bolts where their design allows it. Preserve any current restrictions.

### Activation logic

When Push is pressed:

1. Query for eligible incoming projectiles in a configurable forward/near-player detection region.
2. Reject projectiles outside the configured deflection window, too far away, blocked by geometry, or explicitly non-deflectable.
3. Select the best candidate deterministically (for example: shortest time-to-impact, then nearest eligible projectile).
4. Deflect only the selected projectile or a small explicitly configured group. Do not reflect every projectile in the scene.
5. If no projectile qualifies, normal Push still works.

Use the existing Spider Sense telegraph as feedback where available. Do not require Focus to deflect unless the current design explicitly requires it; Focus should make the timing window easier, not mandatory.

### Reflected projectile behavior

- Reverse/redirect the projectile toward the source direction or the direction defined by current design.
- For the sniper round, the intended behavior is a return along the incoming firing path toward the sniper.
- Update ownership/team/faction so the original shooter can be damaged by its reflected shot.
- Prevent the reflected projectile from immediately hitting the player or the held shield object.
- Prevent an infinite reflection loop. A reflected projectile should normally become non-reflectable or have a reflection-count limit.
- Preserve speed or apply a configurable deflection speed multiplier; tune this by testing, not assumption.
- Use continuous collision detection where appropriate.

### Timing/feel

The deflection window, radius, and cone are unknown and must be tuned. Begin with conservative values exposed in the Inspector. The player should receive a clear success cue: distinct sound, short visual flash, and projectile color/ownership change if appropriate.

## 6. Priority when several interactions overlap

Recommended action priority for one Push input:

1. Fire held Crate Cannon if a valid portable object is held.
2. Otherwise attempt an eligible projectile deflection.
3. Apply normal Wind Cannon blast to eligible targets.
4. Apply player recoil/slide boost according to movement and surface conditions.

The agent must check current intended behavior before enforcing this exact order. If projectile deflection and normal blast are designed to happen in the same activation, implement that deliberately and prevent duplicate impulses.

## 7. Presentation

- Reticle state should clearly distinguish normal Push, a valid deflection opportunity, and Crate Cannon while holding an object.
- Add a clear deflection success cue, separate from ordinary Push audio.
- Keep the existing procedural Wind Cannon audio where it is already implemented.
- Use concise feedback; avoid large effects that obscure aim.

## 8. Edge cases

- Player is holding an object and presses LMB: one Crate Cannon launch only.
- Projectile is behind the player: no forward deflection unless the existing mechanic supports a wider detection region.
- Projectile is already reflected: no repeated reflection loop.
- Projectile is blocked by a wall: do not deflect through the wall.
- Push hits multiple colliders belonging to one enemy: one logical target response.
- Target is immune: no physical/damage response unless its design explicitly allows one.
- Player is sliding or grappling: use movement API; preserve unrelated velocity components.
- Focus is active: projectile timing and Push input must remain consistent in real time.
- Player respawns or ability is disabled: clear pending target/deflection references.

## 9. Acceptance criteria

- One press produces one logical Push/Crate Cannon action.
- Eligible props and enemies receive one correctly directed impact.
- Ineligible targets remain unaffected.
- Surface recoil occurs only under the configured conditions.
- Slide Push produces the intended forward boost without a second backward kick.
- A valid projectile can be deflected and can hit its original shooter.
- Deflection does not become an automatic reflection of all projectiles.
- Movement momentum is not destroyed by direct velocity replacement.
