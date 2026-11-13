# MASTER TASK — Upgrade REBOUND's Player Movement to a REDLINER-Inspired Kinetic Movement Experience

You are the lead gameplay movement engineer, Unity physics programmer, technical animator, camera/UX designer, and test engineer responsible for upgrading the existing **REBOUND** project.

Your assignment is to inspect the real repository and then implement the best practical movement-system improvement supported by the attached resource pack. The goal is to make REBOUND's movement feel fast, fluid, expressive, skill-based, momentum-preserving, and chainable in the broad behavioral spirit of REDLINER—without copying REDLINER's combat identity or proprietary material.

Read every file in this pack before editing. In particular:

- `02_REBOUND_CURRENT_BASELINE.md`
- `03_REDLINER_MOVEMENT_RESEARCH.md`
- `04_REBOUND_TARGET_MOVEMENT_SPEC.md`
- `05_ARCHITECTURE_AND_IMPLEMENTATION_PLAN.md`
- `06_VALIDATION_AND_TEST_SUITE.md`
- `07_PARAMETER_LEDGER.md`
- `08_SOURCE_REGISTER.md`

The source repository is authoritative. This document describes the intended design and the known baseline, but the current code may differ.

---

## A. Non-negotiable design boundaries

### A1. REBOUND stays REBOUND

Preserve REBOUND's own identity, systems, and fantasy:

- Pneumatic Wind Cannon / Push ability.
- Pull ability with prop snatching, shielding, grapple winch, and vortex presentation.
- Crate Cannon and physical prop interaction.
- Focus Time Dilation.
- Existing target buttons, kinetic props, puzzle interactions, and level design.
- Existing REBOUND audiovisual direction and HUD identity unless a focused movement improvement is justified.

**Do not add or copy REDLINER's sword, guns, parry mechanics, weapons, weapon roster, combat loop, instability system, or combat HUD.** Movement-only research is the reference. REBOUND's existing Wind Cannon and Crate Cannon are not to be replaced with REDLINER weapons.

### A2. No blind cloning

Recreate observable principles and movement feel—not inaccessible source code. Do not claim to know REDLINER's hidden equations. The community values in `03_REDLINER_MOVEMENT_RESEARCH.md` are historical observations/reports, not values to paste directly into Unity.

### A3. Preserve the working project

Before changes:

1. Inspect the Unity version, packages, scenes, prefabs, Input System configuration, script assembly structure, and current Git status.
2. Find every player movement/ability script and identify all writers to Rigidbody velocity, force, angular velocity, collider dimensions, camera transform, and time scale.
3. Trace the current input flow and all serialized Inspector references.
4. Establish a clean baseline build/compile state and record existing issues.
5. Preserve the current behavior of non-movement systems unless a movement integration requires a narrowly scoped change.

Do not overwrite or delete scripts just because their names differ from the documentation. Do not fabricate files or assume the documented constants are currently serialized exactly as stated.

### A4. Implement and verify, not just plan

After the audit, execute the implementation in safe stages. Do not stop after writing a theoretical proposal unless the project cannot be accessed or built. If something is blocked, complete all safe work and state exactly what remains blocked.

Do not ask questions whose answers can be established by inspecting the repository. For genuinely unknown design choices, make a conservative, reversible decision, expose it as a configuration value, and document the assumption.

---

## B. Primary movement objective

The improved REBOUND controller must provide:

1. **Persistent velocity:** movement input and abilities operate on an authoritative velocity rather than repeatedly replacing the character's entire velocity with a target speed.
2. **Responsive directional control:** quick acceleration and controllable braking, while retaining meaningful inertia at speed.
3. **Momentum-aware ground movement:** movement direction, velocity direction, and camera direction may differ without snapping the player unnaturally.
4. **Expressive aerial control:** air steering/air strafing that allows skilled players to shape a trajectory while preserving momentum.
5. **Reliable bunny hopping:** holding or buffering Jump should produce repeatable landing-to-jump transitions without a grounded friction pause, while avoiding accidental infinite jump behavior if the character is not actually grounded.
6. **Purposeful sliding:** sliding changes friction/stance and supports landing transitions, speed retention, and directional redirection—not merely a crouch animation.
7. **Momentum-rich grappling:** the existing winch should pull toward anchors while preserving useful tangential/lateral velocity, allowing elastic swing arcs and intentional release/redirect behavior.
8. **Ability chaining:** movement state transitions should support combinations such as slide → hop, landing → slide, grapple → release → air steer, downward grapple → auto-slide, and pneumatic recoil → continued movement.
9. **Optional wall interaction:** investigate whether a wallrun mechanic is appropriate for REBOUND's current level geometry and input design. Do not force it into the project if the maps, camera, or control scheme cannot support it reliably. If added, it must be an intentional REBOUND feature, not a REDLINER keybind clone.
10. **Camera and feedback that communicate speed without hiding the route:** preserve precise first-person aiming and keep visual feedback subordinate to control.
11. **Stable physics and collisions:** no jitter, tunneling, unexpected velocity resets, ground snapping, ceiling stand-up clipping, grapple obstruction bugs, or time-scale inconsistencies.

Success is not just a high speed number. Success means the player can maintain, build, redirect, and understand velocity while retaining precise control.

---

## C. Required repository audit (do this first)

Produce an audit note before changing code containing:

- Unity editor version and relevant packages.
- Exact player prefab/rig and scene(s) in which it is used.
- Rigidbody settings, collider dimensions, collision mode, interpolation, constraints, mass, and gravity settings.
- All scripts that read movement input.
- All scripts that modify player velocity or call `AddForce`/`AddForceAtPosition`.
- All scripts that change collider dimensions/center or camera position/FOV/roll.
- All scripts that set `Time.timeScale` or `Time.fixedDeltaTime`.
- Existing movement state flags and transition logic.
- Existing tuning values versus the resource pack.
- Compile/runtime errors and missing references.
- Which tests can be run automatically and which require Play Mode/manual validation.

Create a dependency map and identify duplicate/conflicting authorities before refactoring.

---

## D. Target implementation architecture

Use a single, explicit movement coordination model. The exact class names may follow the project conventions, but responsibilities must be separated.

### D1. Input layer

- Read input in the correct Unity update phase.
- Store edge-triggered press/release events separately from continuously held input.
- Buffer Jump and other timing-sensitive actions for configurable windows.
- Read camera-relative direction at the appropriate moment.
- Do not have ability scripts independently poll the same action in a way that causes duplicate activation.

### D2. Movement coordinator

- Own movement-state transitions and input priority.
- Resolve conflicts (e.g. slide/jump/grapple/wall interaction) deterministically.
- Avoid one monolithic script that also owns audio, camera, HUD, and prop interaction.
- Ensure that ability modules request changes through a shared velocity/movement interface.

### D3. Authoritative velocity model

- Maintain explicit horizontal and vertical velocity components or an equivalent well-defined velocity state.
- Distinguish acceleration, impulse, friction/drag, velocity projection, gravity, and collision response.
- Preserve Unity physics collision response. Do not write a new velocity every frame after the physics solver in a way that cancels collisions.
- Define a single point/order at which movement code applies its velocity changes.
- Track which system produced a velocity change for debugging.
- Use `Rigidbody.linearVelocity` only if the project's actual Unity version/API supports it; otherwise use the API appropriate to the installed version. Do not blindly assume Unity 6.

### D4. Ground movement

Implement acceleration-based movement with tunable parameters:

- Ground acceleration.
- Maximum control speed (if any), distinct from accumulated total momentum.
- Ground friction when idle.
- Opposing-input braking.
- Direction-change response.
- Forward/sideways/backward input scaling.
- Slope projection and gravity/slope contribution.

Do not automatically clamp the full 3D speed to the ordinary run speed. Separate ordinary input control from inherited momentum and ability-generated momentum.

### D5. Air movement and jump

- Preserve horizontal velocity through takeoff and most airborne movement.
- Add controllable air acceleration with a projection/acceleration limit model or another tested model.
- Implement coyote time and jump buffering if compatible with the actual design.
- Support repeatable hold-to-bhop behavior, but validate it against real grounded contact and prevent repeated jumps while falling.
- Apply vertical jump impulse independently from horizontal velocity.
- Keep gravity/extra-fall behavior configurable and mathematically consistent.
- If a jump is requested while sliding, decide and document whether it ends slide, preserves slide momentum, and adds any jump boost.

### D6. Slide

- Slide should be a locomotion state with explicit entry and exit rules.
- Preserve horizontal velocity at entry; do not reduce it to a fixed slide speed.
- Use state-dependent friction/drag.
- Allow controlled steering, but do not snap the velocity vector to camera direction.
- Support landing-to-slide and slide-to-hop transitions.
- Investigate a backward-input landing redirect inspired by REDLINER's Slide Redirect, but implement the REBOUND version with a clear input window and no hidden/unreliable timing.
- Preserve REBOUND's distinction between low-speed crouch-walk and high-speed power slide unless the actual playtest shows it should change.

### D7. Grapple/winch

- Preserve existing target arbitration: nearby Interactables first (currently 16 m), then Environment anchors (currently up to 75 m), using current code as authority.
- Preserve crate snatching, crate shielding, anti-box-riding logic, and Crate Cannon behavior.
- For player traversal, investigate radial/tangential velocity decomposition or an equivalent physically coherent method.
- The inward pull may change radial velocity strongly while preserving tangential velocity.
- Do not overwrite all player velocity with a direct anchor direction every frame.
- Keep line-of-sight obstruction handling correct; the anchor collider must not falsely obstruct its own tether.
- On release, retain the current useful velocity. Any extra release boost should be explicit, tunable, and tested—not added blindly.
- Support lateral, upward, and downward anchors.
- Keep the existing downward grapple → slide behavior unless the audit proves it is broken; document the chosen angle threshold and test edge cases.

### D8. Wind Cannon movement interaction

- Preserve 12 m push range and current target impulse semantics unless testing shows a bug.
- Preserve the close-surface recoil condition: no player recoil when no surface is within the configured proximity threshold.
- Preserve the wall-kick direction behavior.
- Preserve slide recoil redirection/boost as REBOUND's own identity.
- Integrate recoil through the shared velocity authority so it stacks consistently with existing momentum and grapple movement.
- Do not turn the Wind Cannon into a gun or hitscan weapon.

### D9. Optional wallrun

First inspect REBOUND levels for suitable walls and test collision geometry.

If implemented:

- Enter only when airborne/eligible and a valid wall is detected within a configurable distance and normal-angle range.
- Keep movement constrained/projected along the wall plane.
- Preserve useful tangential velocity.
- Apply configurable adhesion and gravity behavior.
- Allow deterministic detach and jump-off behavior.
- Define camera roll/tilt and reset behavior carefully.
- Prevent wallrun attachment to floors, ceilings, tiny trim, props, or invalid moving surfaces unless deliberately allowed.
- Add cooldown/anti-regrab rules only if required to prevent flicker.
- Do not reuse REDLINER's exact keybinds blindly. The existing REBOUND Space action is Jump/BHop; define a context-aware interaction that does not break those behaviors.

If wallrun is not appropriate yet, deliver an interface/technical proposal but do not add a half-working feature.

### D10. Environment movement extension points

Consider reusable interfaces for future zipline/bounce-pad/launch-surface interactions, but do not build an entire environmental traversal system unless the existing REBOUND levels require it. These systems should modify the same authoritative velocity model and preserve incoming momentum where appropriate.

---

## E. Preserve and reconcile the known REBOUND baseline

Use `02_REBOUND_CURRENT_BASELINE.md` as the intended starting context. Important existing features to preserve include:

- Rigidbody mass documented as 75 kg, interpolation enabled, X/Z rotation frozen.
- Walk 6.5 m/s, sprint 11.5 m/s, low-speed crouch 3.5 m/s.
- Power slide entry from moving forward at sufficient speed, initial speed boost, slope surfing, friction and drag.
- Hold-Space bunny hopping and jump buffering/coyote time.
- Air acceleration/strafe.
- 35 m/s documented ability speed cap (must audit whether it should remain hard or become configurable/soft for the target feel).
- Dynamic capsule height and overhead clearance test.
- Mantling and safe respawn.
- Camera FOV/banking/bob/landing dip/trauma effects.
- Push recoil and wall kicks.
- Pull/grapple, object snatching, crate shield, Crate Cannon, and Focus Time Dilation.
- HUD, procedural audio, kinetic target interface, target buttons, arena builder.

The baseline has known internal inconsistencies. Resolve them with code inspection and measured tests—not assumptions:

1. Slide entry threshold is listed as 6 m/s in one detailed section and 7.5 m/s in the master constants.
2. Slide threshold/runThreshold and `minSlideEntrySpeed` may represent different concepts; separate them if appropriate.
3. Jump vertical speed 6.6 m/s, 2.4× gravity during descent, and stated ~1.6 m height do not automatically agree. Calculate using the actual upward and downward gravity model.
4. `g_slope = 28 m/s²` requires a clear interpretation and unit-consistent slope acceleration formula.
5. Grapple pull acceleration described as `55 m/s² * deltaTime` must be implemented as acceleration integration, not multiply acceleration by time twice.
6. Crouch capsule height/center/feet relationship must be verified in the actual prefab and against ground penetration.
7. Spawn feet offset +1.1 m versus a nominal -1.0 m feet location suggests 0.1 m clearance; validate rather than assume.
8. Focus is described as infinite, while target buttons refill it; determine whether this is legacy or meaningful.
9. The 35 m/s cap may limit the very movement chains this project is trying to improve. Do not remove it without designing a safe configurable policy.

---

## F. Required phases and deliverables

### Phase 0 — Audit and baseline

Deliver an audit summary and list of velocity writers, input dependencies, scene/prefab wiring, and baseline errors. Do not edit until the authority conflicts are understood.

### Phase 1 — Instrumentation

Add or improve a developer-only movement diagnostic view/log that can show:

- Total velocity magnitude and vector.
- Horizontal and vertical velocity.
- Input wish direction.
- Camera forward/right vectors.
- Grounded state, ground normal, slope angle.
- Current locomotion state and active ability flags.
- Current friction/drag and acceleration contributions.
- Grapple anchor, distance, radial/tangential velocity.
- Jump buffer, coyote timer, time since landing/takeoff.
- Speed cap/soft-cap state if any.
- Last velocity writer/reason.

Provide a way to capture repeatable test output (CSV or equivalent) where practical.

### Phase 2 — Core locomotion

Tune ground acceleration, directional braking, momentum retention, slope behavior, and air steering. Ensure the controller is stable and responsive before adding advanced transitions.

### Phase 3 — Jump, bhop, landing, slide

Implement/test hold jump, buffered jump, coyote time, takeoff preservation, slide entry/exit, slide hop, landing-to-slide, and slide redirect. Fix the threshold and jump-physics inconsistencies.

### Phase 4 — Grapple momentum

Improve winch radial/tangential behavior, release momentum, swing arcs, lateral/up/down anchor behavior, and integration with slide/air movement. Preserve crate/object functionality.

### Phase 5 — Ability chaining and REBOUND-specific propulsion

Integrate Wind Cannon recoil, wall kicks, slide recoil, and crate projectile recoil with the shared velocity model. Test all important pairs and sequences.

### Phase 6 — Optional wall traversal / environmental extensions

Implement wallrun only if it is feasible and adds clear value. Add zipline/bounce-style interfaces only where relevant to REBOUND's levels.

### Phase 7 — Feel, presentation, and tuning

Tune camera feedback, FOV, camera roll, landing response, animation states, audio transitions, HUD/speed display, and accessibility. Keep the camera readable at speed.

### Phase 8 — Validation and regression

Run the test suite in `06_VALIDATION_AND_TEST_SUITE.md`. Record pass/fail, measured values, setup, and unresolved issues. Confirm all existing abilities still work.

---

## G. Engineering and code quality requirements

- Keep code modular, readable, and testable.
- Use serialized tuning assets/configuration where appropriate; avoid hard-coded magic values scattered across scripts.
- Include comments for non-obvious physics and state transitions, not comments that merely restate code.
- Avoid per-frame allocations in hot movement loops.
- Use non-allocating physics queries where appropriate and verify layer masks.
- Respect Unity's FixedUpdate/Update timing and unscaled time where necessary.
- Handle `Time.timeScale = 0.25` correctly: physics delta, input, camera, UI, and audio must use intentional time domains.
- Guard null references and missing components, but do not silently conceal setup errors.
- Keep collision detection appropriate for high speed; validate continuous collision detection and collider shapes.
- Do not let camera shake/roll alter physics orientation.
- Do not allow the player Rigidbody to rotate or tip unexpectedly.
- Maintain deterministic input transition order within a physics step.
- Add unit/edit-mode tests for pure calculations and play-mode tests for state transitions where feasible.
- If a change cannot be verified in the current environment, label it unverified.

## H. Acceptance criteria

The work is not complete unless:

1. The project compiles without new errors.
2. The player prefab and relevant scenes have valid references.
3. Ground movement is responsive from rest and at speed.
4. Opposing input brakes/redirects in a controlled, tunable way.
5. Jump and bunny-hop transitions are repeatable and do not randomly fail.
6. Slide retains velocity and supports the selected landing transition behavior.
7. Grapple preserves useful tangential momentum and does not overwrite all velocity.
8. Releasing the grapple does not unexpectedly zero the player's momentum.
9. Wind Cannon recoil stacks with existing velocity and respects REBOUND's close-surface rule.
10. Crate snatch, shielding, Crate Cannon, target buttons, and Focus Time Dilation remain functional.
11. High-speed collision, wall contact, ceiling clearance, and respawn are stable.
12. Camera remains aimable and understandable at high speed.
13. Instrumentation can explain why velocity changed.
14. Tests and results are documented.
15. No REDLINER sword/gun/parry combat system or copyrighted assets have been added.

## I. Final report format

When finished, provide:

1. Executive summary of what changed and what feel improvements were achieved.
2. Repository audit findings.
3. Architecture before/after.
4. Exact list of modified/created files and scenes/prefabs.
5. Parameter table with old/new values, units, reason, and confidence.
6. State-transition and input-priority documentation.
7. Test matrix with pass/fail/not run and evidence.
8. Known limitations and decisions intentionally deferred.
9. Manual Unity Editor setup steps, only if genuinely required.
10. Rollback instructions or Git commit/branch reference if available.

Do not claim something was tested when it was not. Do not report performance or movement values without actual measurement. Prioritize a clean, reliable integrated result over adding every speculative technique.
