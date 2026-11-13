# REBOUND Movement Upgrade — Architecture and Implementation Plan

## 1. Architecture objective

Create a modular controller in which one coordinated system owns movement decisions and all abilities contribute explicit velocity/force changes. Keep the Unity physics solver responsible for collision response; do not run competing velocity overrides from several scripts.

## 2. Recommended logical layers

### 2.1 Input sampling

Responsibilities:
- Read actions in Update/input callbacks.
- Track pressed, released, held, and buffered states separately.
- Record timestamps in a deliberate time domain.
- Convert WASD into a normalized wish vector.
- Obtain camera forward/right vectors projected onto the relevant plane.
- Pass immutable input snapshots to the movement coordinator.

Do not read mouse delta in FixedUpdate and do not process one press in multiple components.

### 2.2 Movement coordinator

Responsibilities:
- Maintain locomotion/traversal/ability state dimensions.
- Evaluate eligibility and priority.
- Begin/end state transitions.
- Resolve simultaneous inputs.
- Call movement modules in a deterministic order.
- Record why velocity changed.

### 2.3 Velocity/momentum authority

Responsibilities:
- Expose current Rigidbody velocity.
- Provide named operations: add impulse, apply acceleration, project onto plane, modify radial component, apply drag, modify vertical component, request speed limit/soft limit.
- Prevent accidental full-vector replacement.
- Keep a reason/source history for diagnostics.
- Apply changes at one defined point in the physics update, without cancelling solver collisions afterward.

Possible conceptual API (adapt to existing code style):

```csharp
public interface IMovementVelocity
{
    Vector3 Velocity { get; }
    void AddImpulse(Vector3 deltaVelocity, MovementSource source);
    void AddAcceleration(Vector3 acceleration, float deltaTime, MovementSource source);
    void ApplyDrag(float coefficient, float deltaTime, MovementSource source);
    void SetVerticalVelocity(float value, MovementSource source);
    void ProjectVelocityOnPlane(Vector3 normal, MovementSource source);
}
```

This is an interface sketch, not a demand to add this exact type.

### 2.4 Movement modules

- Ground locomotion.
- Air locomotion.
- Jump/BHop.
- Slide.
- Grapple traversal.
- Push/recoil integration.
- Optional wallrun.
- Optional environment launch/zipline.
- Landing/mantle/collision.

Presentation modules (camera, animation, sound, HUD) subscribe to state/events and should not directly own physics.

## 3. Orthogonal state model

Do not create a single enum for every compound situation. Use independent state dimensions.

Example:

```text
Locomotion:
  GroundedNormal | Crouching | Sliding | AirborneRising |
  AirborneFalling | LandingTransition | Wallrunning? | Mantling?

Traversal:
  None | Targeting | Attached | Releasing | HoldingProp

Movement modifiers:
  FocusSlowMo | PushRecoilWindow | CrateLaunchEvent | SlamLikePending? 

Ground contact:
  grounded bool + normal + collider + timeSinceContact
```

An ability may be attached while the player is airborne; Focus can be active during most locomotion states. Model those as orthogonal properties.

## 4. Candidate physics equations

These equations are implementation candidates. They are not the verified original REDLINER equations.

### 4.1 Input wish direction

Let camera forward/right projected onto the movement plane be `f` and `r`:

`wish = normalize(f * input.y + r * input.x)`

Normalize input magnitude so diagonal input is not stronger than axial input.

### 4.2 Acceleration with directional projection

A useful candidate acceleration constraint:

```text
currentAlongWish = dot(vHorizontal, wish)
addable = max(0, maxWishSpeed - currentAlongWish)
accel = min(addable, acceleration * dt)
vHorizontal += wish * accel
```

This allows the player to accelerate along the new wish direction while retaining perpendicular momentum. Test against an alternative bounded acceleration controller; choose by feel and stability.

### 4.3 Ground friction

When no useful movement input is present, apply a state-dependent drag/braking model. For example:

`vHorizontalNext = vHorizontal * exp(-groundDrag * dt)`

For active reverse input, apply directional braking acceleration rather than relying only on drag.

Do not set horizontal velocity to zero when input is released unless the player is truly stopped.

### 4.4 Air steering

Use a smaller/different acceleration than on ground. Constrain acceleration along the wish direction and preserve the perpendicular vector component. Tune so smooth mouse arcs produce useful curvature without allowing instant 180° redirection at high speed.

### 4.5 Gravity and jump

Use a clear vertical integration model:

`vyNext = vy + gravityY * dt`

At jump:

`vy = jumpTakeoffSpeed`

If ascent and descent gravity differ, define the switch condition explicitly and calculate the resulting maximum height and airtime from the actual piecewise model. Do not mix a stated jump velocity with an incompatible claimed height.

### 4.6 Landing

On grounded contact:
- Resolve vertical velocity according to contact/solver behavior.
- Preserve horizontal velocity unless a deliberate landing penalty is designed.
- Set grounded contact data/time.
- Process buffered jump/slide transitions in one priority order.
- Prevent one physics tick of full ground friction from killing a buffered hop.

### 4.7 Slide

During slide:
- Keep incoming horizontal speed.
- Apply low state-specific drag/friction.
- Project movement onto the ground plane.
- Apply slope gravity consistently.
- Apply bounded steering acceleration.
- Exit by time, input release, speed threshold, jump, or another explicitly defined transition.

### 4.8 Slope projection and gravity

For ground normal `n`, project desired horizontal direction onto the plane:

`groundDirection = normalize(ProjectOnPlane(wish, n))`

World gravity naturally contributes along a slope when using Rigidbody physics. If adding custom slope acceleration, avoid double-counting gravity. For a slope angle `theta`, the ideal gravity component along the plane is approximately `g * sin(theta)` for a simple rigid-body incline, before friction. Any 28 m/s² multiplier should be treated as tuned/gameplay acceleration, not natural gravity, and must be calibrated.

### 4.9 Grapple radial/tangential decomposition

Let `u = normalize(anchorPosition - playerPosition)`.

`vRadial = dot(v, u) * u`

`vTangential = v - vRadial`

Candidate behavior:
- Apply pull acceleration toward anchor.
- Optionally limit inward radial velocity without touching tangential velocity.
- If using a rope constraint, enforce maximum rope length and remove only outward radial velocity when taut.
- Keep gravity unless the chosen state intentionally modifies it.
- On release, preserve actual current velocity and then apply only a designed, tested release adjustment.

Do not use a large spring force without damping/constraint checks; it can explode near the anchor or oscillate.

### 4.10 Wallrun candidate

For a valid wall normal `n`:

`vOnWall = ProjectOnPlane(v, n)`

Then apply:
- Adhesion toward the wall only while eligible.
- Movement acceleration along the wall plane.
- A tuned gravity multiplier/vertical rule.
- Detach conditions (lost wall, angle, timeout, user jump, ability transition).

Must not attach to floor/ceiling or small decorative geometry.

## 5. Time handling and Focus Time Dilation

REBOUND documents a timeScale of 0.25 and a fixedDeltaTime of 0.005 s. Audit how this is applied.

Rules:
- Save the original fixed timestep once; do not multiply an already scaled fixed timestep repeatedly.
- Physics that should slow with the world should use scaled fixed time naturally.
- Input collection and mouse look should remain responsive using unscaled time or input callbacks.
- UI/reticle animation should use the intended time domain.
- Audio pitch/time behavior should be intentional.
- Coyote/buffer windows should be defined as gameplay-time or real-time; do not mix timer domains accidentally.
- Ensure coroutines and animation transitions use the desired scaled/unscaled clock.

## 6. Collision and geometry

- Confirm capsule dimensions and center offsets at every stance.
- Test ceilings, ramps, corners, wall edges, slopes, and high-speed collision.
- Use Continuous/Continuous Dynamic collision where justified by Rigidbody setup and project version.
- Use raycasts/capsulecasts with correct layer masks and trigger settings.
- Prevent grapple self-hits; ignore anchor collider in LOS check.
- Prevent wallrun on non-traversable surfaces using normal threshold, tag/layer, and optional surface component.
- Use NonAlloc queries in hot paths if profiling warrants it.

## 7. Implementation sequence

### Stage 0: Audit

No broad edits. Establish project truth and velocity ownership.

### Stage 1: Instrument

Add developer-only telemetry and a movement test scene/arena.

### Stage 2: Core velocity model

Refactor only necessary paths so ground, air, slide, grapple, and recoil contribute through one authority. Preserve collision solver behavior.

### Stage 3: Locomotion

Tune ground acceleration, friction, braking, slope projection, and air control.

### Stage 4: Jump/slide/landing

Reconcile thresholds, jump height/gravity, coyote/buffer, hold-to-hop, slide entry, slide exit, landing slide, slide jump, and redirect.

### Stage 5: Grapple

Improve radial/tangential control, swing arcs, release, and movement state interactions. Keep object pulling/shielding independent.

### Stage 6: Push/recoil and crate launch integration

Ensure ability impulses stack correctly and preserve existing restrictions.

### Stage 7: Optional wallrun and environment interfaces

Only if justified by project geometry and game design.

### Stage 8: Presentation

Camera, FOV, bank, landing, sound, animation, speed/state HUD.

### Stage 9: Test/tune/regression

Run `06_VALIDATION_AND_TEST_SUITE.md`, record metrics, fix defects.

## 8. Debug instrumentation specification

Minimum live telemetry:

- Player world position.
- Total velocity vector and speed.
- Horizontal velocity vector/magnitude.
- Vertical speed.
- Wish direction and wish speed.
- Grounded flag, collider, ground normal, slope angle.
- Locomotion state, traversal state, active modifiers.
- Current friction/drag and acceleration values.
- Jump-held, jump-buffer timer, coyote timer, last grounded time.
- Slide timer, slide entry speed, slide exit reason.
- Grapple target, anchor position, distance, radial speed, tangential speed, LOS status.
- Push recoil eligibility, surface distance, applied delta-v.
- Current speed limit/soft limit and whether it engaged.
- Last velocity modification source and amount.

Provide a log/CSV capture for repeatable tests if feasible. Add gizmos for ground probe, wall probe, grapple ray/sphere cast, anchor, slide direction, and velocity vector.

## 9. Tuning process

- Establish a known test route with flat ground, 10–30 m ramps, a 26° ramp, wall faces, ceilings, and anchors at forward/lateral/upward/downward angles.
- Change one parameter family at a time.
- Use at least five repeatable trials for key movement tests.
- Compare vectors, not just speed magnitudes.
- Keep default tuning values in one configuration asset or grouped serializable component.
- Record old value, new value, reason, observed effect, and regression status.
- Never tune based on uncalibrated REDLINER `u/s` values.

## 10. Completion criteria

The final controller should be modular, predictable, stable, inspectable, and maintainable. A skilled player should be able to chain REBOUND's own locomotion and abilities without momentum disappearing because a different script overwrote the Rigidbody velocity.
