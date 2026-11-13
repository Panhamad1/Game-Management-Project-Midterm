# REBOUND — Target Movement Experience Specification

This document translates the movement research into a REBOUND-specific target. It intentionally preserves REBOUND's mechanics and identity.

## 1. Experience goal

The player should feel like a high-momentum kinetic specialist who can fluidly combine:

- Running and steering.
- Sliding and downhill surfing.
- Repeated hops and air steering.
- Grapple winch movement around anchors.
- Slingshot-style release and aerial continuation.
- Pneumatic recoil movement from the Wind Cannon.
- Physical object interactions and Crate Cannon recoil.
- Focus Time Dilation for controlled precision.

The player should be able to build speed, retain useful momentum, redirect that momentum intentionally, and understand why a movement transition succeeded or failed.

**North star:** fast but legible; momentum-rich but controllable; physically expressive but deterministic enough to learn.

## 2. Keep REBOUND's identity

### Preserve

- Wind Cannon = pneumatic pressure blast and recoil, with its surface-proximity rule.
- Pull = physical prop snatching/shielding and traversal winch, with vortex presentation.
- Crate Cannon = physical prop projectile and recoil.
- Focus = slow-motion time dilation.
- Existing puzzle props, kinetic targets, buttons, arena, and first-person identity.

### Explicitly exclude

- REDLINER sword/melee system.
- REDLINER guns or weapon roster.
- REDLINER parry and combat-clash system.
- REDLINER Instability/status mechanics.
- REDLINER maps, assets, characters, UI copy, names, or proprietary implementation.

## 3. Desired control feel

### 3.1 Ground movement

- Immediate response from input, without feeling like instant teleportation to a fixed target speed.
- Ground acceleration must be tunable independently from top controllable speed.
- At high inherited momentum, turning should change the movement trajectory progressively rather than instantly replacing velocity.
- Opposing input must provide usable braking.
- Releasing movement should cause predictable state-dependent deceleration, not an arbitrary velocity reset.
- Diagonal movement should be normalized for input magnitude while retaining the existing momentum vector.
- Uphill/downhill behavior should be coherent and easy to predict.

### 3.2 Aerial movement

- Takeoff preserves horizontal velocity.
- Air input can accelerate/shape trajectory without destroying the existing perpendicular velocity.
- Camera rotation and movement wish direction should be coordinated, but high-speed turns must remain smooth and aimable.
- Holding Jump should support reliable repeated hops on landing, with configurable buffer and coyote windows.
- No extra hop boost should be stacked indefinitely without a deliberate and tested rule.

### 3.3 Slide

- Slide is distinct from low-speed crouch-walk.
- Entering slide should preserve or add a controlled amount of momentum; never set speed to an arbitrary slide speed if that erases a faster incoming velocity.
- Sliding should reduce ground drag and alter the character capsule/profile.
- Steering should be possible but should not instantly rotate the velocity vector.
- The player should be able to jump out of a slide and preserve useful horizontal speed.
- Landing-to-slide should be a deliberate input/automatic-state path with clear eligibility rules.
- A REBOUND-specific slide redirect should allow a controlled backward/sideways slide request while preserving inherited momentum for a short, configurable window, inspired by community-described REDLINER Slide Redirect.

### 3.4 Grapple/winch

- Keep target selection and object-snatching logic intact.
- When traversing, pulling toward an anchor should not erase sideways/tangential momentum.
- The player should be able to swing around a pillar or tower rather than being pulled in a perfectly straight line in every case.
- Release should preserve the actual velocity at detachment.
- Any additional release boost should be a separate, explicit parameter and should be calibrated through testing.
- Camera orientation can influence targeting and possibly a bounded redirect component, but should not silently replace all momentum.
- Downward grapple transitions into the existing REBOUND slide behavior where intended.

### 3.5 Pneumatic propulsion

- Wind Cannon recoil must integrate with existing velocity, grapple, slide, and air steering.
- Keep zero recoil beyond the current proximity threshold.
- Keep wall-normal deflection logic.
- Keep slide-forward boost as REBOUND's own mechanic.
- Make sure the impulse is applied once per activation, not once per frame.
- The player should see/feel a clear cue that recoil occurred.

### 3.6 Optional wall traversal

Wallrunning can be considered if the actual arena has large clean wall surfaces, towers, and safe wall exits. It is not mandatory to ship if it would destabilize movement or require a new map design pass.

If implemented, it should be a REBOUND traversal feature integrated with jump, grapple, and slide—not a combat wallrun cloned from REDLINER. It should support reliable entry, controlled travel along the surface, deliberate detach, and preservation of tangential momentum.

### 3.7 Optional environmental movement

Consider future launch pads/zipline-like interactions only if they fit the current arena. If implemented, they should apply an explicit velocity change while preserving selected incoming velocity components. Do not add content solely to imitate another game.

## 4. Input behavior and state priority

Use the existing REBOUND inputs as a starting point (verify actual action assets):

| Input | Existing REBOUND action | Desired rule |
|---|---|---|
| WASD | Move | Camera-relative wish direction; preserve inertia |
| Mouse | Look | Immediate aim response; independent of movement velocity |
| Space | Jump/hold-to-bhop | Press buffer; held auto-hop only on valid ground contact; slide-jump allowed if designed |
| C/Ctrl | Crouch/slide | Low-speed crouch; high-speed slide when eligible |
| Shift | Sprint | Existing sprint mode; do not repurpose as REDLINER dash unless owner explicitly changes the design |
| LMB | Wind Cannon / Crate Cannon | Preserve existing context-dependent behavior |
| RMB | Pull/Grab/Grapple | Preserve target arbitration and held-object behavior |
| Q/F | Focus toggle (as documented) | Preserve whichever actual mapping is in the action asset |
| R | Safe respawn | Preserve |

The current REBOUND design does not document a standalone dash ability. Do not add Shift dash automatically just because REDLINER has it. First evaluate whether the existing Wind Cannon recoil, grapple release, slide, and bunny-hop chains already supply the desired burst movement. If a dedicated dash is genuinely needed, present it as a REBOUND-specific design addition with a clear reason, input mapping, and compatibility analysis—not as a copied REDLINER feature.

### Input priority principles

1. Safety/respawn and pause/cursor actions remain highest priority.
2. Existing held-object interaction must take precedence for LMB (Crate Cannon) over ordinary Push.
3. RMB target arbitration must distinguish nearby interactable pickup from environmental traversal.
4. Ground Jump should fire only when eligibility/buffer rules are satisfied.
5. If Space is also used for optional wallrun or grapple release, define exact context rules so it does not double-fire jump and release in the same tick.
6. Slide entry and slide-jump must have deterministic priority when simultaneous inputs occur.
7. A single action press must never be consumed independently by two controllers.

## 5. Movement state model

Use orthogonal state dimensions rather than a giant enum containing every combination.

Suggested locomotion state:

- GroundedNormal
- GroundedCrouch
- Sliding
- AirborneRising
- AirborneFalling
- LandingTransition
- Optional Wallrunning
- Optional Mantling

Traversal state:

- None
- GrappleTargeting
- GrappleAttached
- GrappleReleasing
- HoldingProp

Ability activity:
- PushRecoilActive (brief event, not necessarily a persistent locomotion state)
- CrateLaunchEvent
- FocusTimeScaleActive

Ground contact, grapple attachment, held object, and Focus should not all be encoded into one mutually exclusive movement enum.

## 6. REBOUND-specific movement chains to support

These are target behaviors to implement or validate. They are not all new abilities.

1. Run → power slide → jump → air steer → land → hop again.
2. Downhill slide → gain speed → jump while retaining horizontal velocity.
3. Grapple to side anchor → build/sustain swing arc → release → air steer.
4. Grapple to overhead anchor → swing/redirect → release into forward movement.
5. Grapple downward → auto-slide → slide hop.
6. Run/slide → Wind Cannon close-surface recoil → preserved horizontal motion.
7. Slide → Wind Cannon forward slide boost → hop.
8. Grapple movement → Push recoil → redirected escape/launch.
9. Hold crate → move with crate shield → Crate Cannon launch → player recoil → air steer/land.
10. High-speed landing → buffered jump or slide, with no unexplained full stop.
11. Slide redirect near landing → retain inherited velocity while choosing a new slide input direction.
12. Optional wallrun → detach/jump → grapple or air steer.

## 7. Movement feedback goals

### Camera

- Preserve responsive mouse look.
- Retain current dynamic FOV only if it helps speed perception without harming aim. Calibrate rather than assume 105° at 30 m/s is ideal.
- Keep camera roll subtle and independent of physics yaw.
- Avoid sustained camera lag that makes the player feel unresponsive.
- Use landing dip and trauma shake sparingly; high-speed control has priority.
- Ensure Focus uses unscaled input/look so time dilation does not make the camera sluggish.

### HUD

- Keep existing target-reticle modes and speedometer.
- Clearly distinguish current movement state and total speed.
- Consider displaying horizontal speed separately from vertical speed in the debug view; the player-facing HUD can remain concise.
- Show grapple target validity and distance accurately.
- If new resources/cooldowns are not actually present, do not fabricate meters or bars.

### Audio

- Preserve the procedural identity.
- Make transitions audible: slide begins/ends, grapple attaches/releases, slingshot-like release, landing/hop, recoil event, optional wall contact.
- Do not use an audio cue as the only indicator of a critical state.

## 8. Performance and readability

- Physics must remain stable at the intended frame rate and under time dilation.
- Avoid excessive per-frame raycasts; batch/NonAlloc where appropriate.
- Avoid camera effects that reduce spatial awareness.
- Retain continuous collision handling for fast movement.
- Make movement parameters accessible in a centralized tuning asset or clearly grouped serialized fields.
- Include a safe test arena with ramps, flat ground, vertical walls, ceilings, grapple anchors at varied angles, and known distances.

## 9. Completion standard

REBOUND should not be considered improved simply because the player can go faster. It should be possible to perform repeatable movement chains while preserving control and the game's own pneumatic/physical identity. Use the test suite to compare behavior before and after.
