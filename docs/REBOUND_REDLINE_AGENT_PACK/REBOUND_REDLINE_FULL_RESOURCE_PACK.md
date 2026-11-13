# REBOUND × REDLINER — FULL AI AGENT RESOURCE PACK


---

<!-- SOURCE FILE: 00_READ_ME_FIRST.md -->

# REBOUND Movement Upgrade — AI Agent Resource Pack

**Purpose:** Give an AI coding agent enough context to inspect, improve, implement, tune, and validate REBOUND's player movement so that it captures the strongest *movement feel and movement-system behaviors* documented in REDLINER, while remaining unmistakably REBOUND.

**Target:** REBOUND (Unity project; current player subsystem uses a Rigidbody-oriented architecture, subject to verification against the actual repository).

**Research reference:** REDLINER, Roblox experience ID `94987506187454`.

**Important boundary:** Borrow movement principles and observable movement behaviors—not REDLINER's combat identity. Do **not** add REDLINER's swords, guns, parry system, weapon roster, combat loop, UI identity, maps, assets, names, or proprietary material. REBOUND retains its pneumatic Wind Cannon, Pull/Winch/Vortex, crate interactions and Crate Cannon, Focus Time Dilation, target puzzles, and existing world identity.

## Package contents and recommended order

1. `01_MASTER_AGENT_PROMPT.md` — the actual instruction to give the coding agent. Start here.
2. `02_REBOUND_CURRENT_BASELINE.md` — the existing player subsystem and its documented values. This is the preservation baseline, not proof that the repository currently matches it.
3. `03_REDLINER_MOVEMENT_RESEARCH.md` — the movement reference: official controls, advanced techniques, evidence labels, historical values, and uncertainty.
4. `04_REBOUND_TARGET_MOVEMENT_SPEC.md` — what the improved REBOUND controller should feel and do; how Redliner-inspired principles map onto REBOUND's own mechanics.
5. `05_ARCHITECTURE_AND_IMPLEMENTATION_PLAN.md` — architecture, state model, mathematical candidates, sequencing, and engineering rules.
6. `06_VALIDATION_AND_TEST_SUITE.md` — repeatable tests, instrumentation, acceptance gates, and regression checks.
7. `07_PARAMETER_LEDGER.md` — all known REBOUND values, Redliner-reported values, unknowns, conflicts, and tuning instructions.
8. `08_SOURCE_REGISTER.md` — stable source URLs and what each source can and cannot establish.

## How to use this pack

- Attach the entire folder/ZIP to the coding agent together with the **actual Unity repository**.
- Paste `01_MASTER_AGENT_PROMPT.md` as the task instruction.
- Give the agent permission to inspect the project, edit code, create tests/debug tooling, and report changed files. The agent must not assume this document is the actual codebase.
- The agent should use the current project files, scenes, prefabs, Input System assets, package manifest, and Unity version as the implementation source of truth.
- The agent should work in small, buildable stages and preserve a rollback path.

## Truth hierarchy

When information conflicts, use this order:

1. Actual current Unity project and reproducible in-project tests.
2. User's explicit design decisions in the task prompt.
3. The REBOUND baseline in `02_REBOUND_CURRENT_BASELINE.md`.
4. REDLINER official listing for game identity and officially listed control bindings.
5. REDLINER community guides and demonstrations for reported movement behaviors.
6. Engineering hypotheses in this pack.

The REDLINER sources are not access to its original code. All reconstructed formulas are candidates, not claims about the original implementation.

## Critical warnings

- Do not convert community `u/s` values into Unity `m/s` without establishing the unit scale. Treat them as uncalibrated community notation.
- Do not set a hard maximum speed just because a controller normally has one. REBOUND currently documents a soft/explicit 35 m/s cap for its own movement abilities; determine whether this cap conflicts with the desired movement feel before changing it. Any new cap must be justified and configurable.
- Do not assume all historical REDLINER techniques still work in the November 2026 version.
- Do not replace a working controller wholesale before auditing it.
- Do not let multiple scripts fight over `Rigidbody.linearVelocity` or repeatedly overwrite velocity without a defined authority/ordering model.
- Do not sacrifice reliable aiming, camera readability, collision stability, or predictable input to achieve speed alone.
- Do not remove Focus Time Dilation or change REBOUND's ability concepts as a shortcut.

## Expected final outcome

The agent should deliver working project changes, not merely advice: an improved and integrated movement system, inspector/configuration setup, clear documentation, debug instrumentation, test results, and a list of remaining limitations. All important changes must be traceable to an explicit requirement or a documented design decision.


---

<!-- SOURCE FILE: 01_MASTER_AGENT_PROMPT.md -->

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


---

<!-- SOURCE FILE: 02_REBOUND_CURRENT_BASELINE.md -->

# REBOUND — Current Player Subsystem Baseline

This document captures the player-system specification supplied by the project owner. It is the design/baseline reference for the coding agent. The actual repository must be inspected to confirm that the implementation still matches it.

## 1. Design identity

REBOUND's player is a kinetic air-pressure movement engine, not an armed soldier with a conventional gun/sword combat kit.

Core fantasy:

- Push air forward and the player is pushed backward.
- Slide downhill and gravity accelerates the player.
- Jump out of a slide and preserve horizontal momentum into the air.
- Grapple a surface and use the cable/winch to move toward it, swing, and slingshot away.
- Snatch physical objects and use them as shields or projectiles.
- Use Focus Time Dilation as a player-controlled slow-motion ability.

Architecture philosophy:

- Ground movement: directional acceleration and friction-damped control.
- Air and slide: momentum preservation, velocity projection, and state-dependent acceleration/drag.
- Abilities: physical force/impulse systems, not hitscan weapon substitutes.

## 2. Player rig and physics

### Root: `Player_Rig`

- Rigidbody mass: 75 kg.
- Interpolation: Interpolate.
- Constraints: FreezeRotationX and FreezeRotationZ; yaw is controlled by the player/camera system.
- CapsuleCollider: radius 0.5 m, standing height 2.0 m, crouched/slide height 1.15 m.
- `PlayerMovement.cs`: physics solver in FixedUpdate.
- `PlayerAbilityInput.cs`: Input System polling in Update.
- `PlayerAudioEffects.cs`: procedural runtime sound generation.
- Collision mode: specification defense notes recommend Continuous to prevent tunneling at 35 m/s; confirm actual serialized setting.

### Children

- `GroundCheck`: local Y = -1.0 m; bottom center of capsule; sphere overlap plus raycast.
- `CameraPivot`: local Y = 1.65 m standing / 1.05 m crouched; `MouseLook.cs`.
- `MainCamera`: Universal Additional Camera Data, dynamic FOV 85°–105°, AudioListener.
- `AbilityRayOrigin`: 0.35 m in front of lens to avoid self-hit/parallax.
- `LeftHandVisual`: PushAbility and shockwave LineRenderer.
- `RightHandVisual`: PullAbility, grapple LineRenderer, vortex rings.
- `AbilityHUD_Canvas`: Screen Space Overlay, `AbilityHUD.cs`.

## 3. Locomotion — `PlayerMovement.cs`

### 3.1 Speed hierarchy

- Walk: 6.5 m/s.
- Sprint: 11.5 m/s (Left Shift).
- Low-speed crouch-walk: 3.5 m/s.
- Power slide: reported 14–22 m/s entry/typical range, depending on current speed and boost.
- Crouch does not automatically trigger slide from a walk or stationary state.

Detailed state logic in the supplied spec:

- If not sprinting OR current speed is below 6 m/s: crouch-walk at 3.5 m/s.
- If sprinting and current speed is at/above 6 m/s: power slide.
- Slide additionally requires forward input `moveInput.y > 0.1`.
- Slide entry velocity uses `Mathf.Max(CurrentSpeed + 4.5f, 14.0f)`.

**Conflict:** the master constants list gives `runThreshold = 7.5 m/s`, and slide-ramming also uses 7.5 m/s. Determine whether 6 m/s is the slide/crouch mode threshold and 7.5 m/s is a distinct run/ram threshold. Do not silently choose one.

### 3.2 Slide dynamics

- Kinetic friction/deceleration term: 2.2 m/s².
- Quadratic aerodynamic drag approximation described as `deceleration = μ_kinetic + (v * C_drag)`.
- Drag coefficient: 0.035.
- Minimum slide duration: 0.35 s to prevent jittery cancel.
- Slide can be redirected/boosted by firing Wind Cannon while sliding.

The drag equation's units and exact use must be checked. A term `v * C_drag` has acceleration units only if `C_drag` has units of 1/s; do not treat 0.035 as dimensionless without deciding the equation.

### 3.3 Slope surfing

- Slide velocity is projected onto the ground plane:
  `v_slide = ProjectOnPlane(d_slide, n_ground)`.
- Slope contribution described as:
  `a_slope = (d_slide · d_downhill) * g_slope * sin(theta)`.
- `theta` is the angle between ground normal and world up.
- `g_slope = 28 m/s²`.
- Downhill accelerates up to a soft cap of 35 m/s; uphill decelerates.

Validate the direction vectors and dimensional consistency. `g_slope` may be an authored acceleration multiplier rather than physical gravity.

### 3.4 Bunny hopping

- Holding Space automatically jumps on the physics tick of landing.
- Coyote time: 0.15 s.
- Jump buffer: 0.15 s.
- Held-jump is accepted as an ongoing request.
- The spec says held jump bypasses grounded friction, preserves 100% horizontal momentum, and adds `bHopBoost = +2.5 m/s`.

Candidate input logic from the baseline:

```csharp
bool canJump = (Time.time - lastGroundedTime <= coyoteTime);
bool wantsJump = (Time.time - lastJumpPressedTime <= jumpBufferTime) || isJumpHeld;
```

Verify exact timer domain and contact order. For slow motion, decide whether timers use scaled or unscaled time intentionally.

### 3.5 Air strafing

- Horizontal velocity `v_h`; normalized wish direction `w`.
- Projection `P = v_h · w`.
- Wish speed `V_maxWish = 16 m/s`.
- Additional speed request `A_add = max(0, V_maxWish - P)`.
- If positive:
  `v_h = v_h + w * min(A_add, a_air * Δt)`.
- Air acceleration: 26.0.
- Velocity may rotate toward wish direction using:
  `RotateTowards(v_h, w, (a_air * 1.6 / max(|v_h|, 6)) * Δt)`.
- The spec says each smooth curve adds roughly 1.5–3.5 m/s; this is a stated design observation, not an independently verified test result.

Do not assume this exact formula is correct or optimal. Test it and keep the useful behavior.

### 3.6 Slide bowling

- While sliding and current speed >= 7.5 m/s, OnCollisionEnter checks `IKineticTarget`.
- Calls `OnPlayerSlideRam(CurrentSpeed)`.
- Barricades can shatter; crates/enemy swarmers receive impact/damage/launch without stopping the player unnaturally.

### 3.7 Jump and gravity

- Vertical jump velocity: 6.6 m/s.
- Baseline claims approximate jump height 1.6 m using `h = v_y²/(2g)`.
- Standard descent gravity scale: 2.4× world gravity.
- Focus gravity scale: 0.5× world gravity for extended hang time.

**Mathematical conflict:** with ordinary 9.81 m/s² gravity, 6.6 m/s gives about 2.22 m ballistic height. With 2.4× 9.81 m/s² from takeoff, it gives about 0.93 m. A 1.6 m result requires another effective gravity/profile. Measure the actual implementation and derive the height/time from the actual piecewise gravity curve.

### 3.8 Capsule and ceiling clearance

- Standing: height 2.0 m, center Y 0.0 m, nominal feet local Y -1.0 m.
- Crouch/slide: height 1.15 m, center Y -0.425 m.
- Before standing, upward SphereCast radius 0.4 m and distance 0.95 m checks clearance.

Verify capsule center, transform scale, ground offset, and foot alignment in the actual prefab.

### 3.9 Mantling and safe spawn

Mantling:
- Airborne/falling condition described as `rb.linearVelocity.y < 1.0 m/s`.
- Chest forward ray at Y +0.2 m detects wall.
- Top raycast from 0.8 m above wall finds flat ledge.
- Arrest vertical velocity and move up using `Mathf.MoveTowards`.

Safe spawn:
- Raycast downward from Y +50 m to highest solid platform.
- Set feet at hit point Y +1.1 m; update active checkpoint idempotently.
- Setup menu can snap player to safe spawn; arena deck top is documented at Y=9.6 m.

Check whether mantle/respawn preserve or reset horizontal momentum intentionally.

## 4. Camera — `MouseLook.cs`

- Mouse X rotates root yaw around Y.
- Mouse Y rotates CameraPivot pitch around X.
- Pitch clamp: -85° to +85°.
- Dynamic FOV:
  `FOV = Lerp(baseFOV, maxFOV, Clamp01(speed / 30))`.
  Base 85°, max 105° at 30 m/s.
- Camera banking target: `-moveInput.x * 2.5°`, max ±2.5°, smoothed with bankSpeed 8.
- Head-bob: vertical sinusoid tied to speed; no horizontal sway to preserve aim.
- Landing dip: up to 0.15 m downward based on downward velocity; spring over 0.2 s.
- Trauma shake: trauma decay `max(0, trauma - Δt*1.5)`; offset scales with trauma squared and a randomized directional vector.
- Trauma triggers include rocket jumps, crate launches, grapple snaps, and target impacts.

Keep presentation motion separate from physics yaw and movement direction. Camera feedback must not obscure aim.

## 5. Inputs — `PlayerAbilityInput.cs`

Unity New Input System, input polling in Update:

- WASD → `PlayerMovement.SetMoveInput`.
- Mouse delta → `MouseLook.SetLookInput`.
- LMB → Push, or Crate Cannon launch when holding a crate.
- RMB → Pull/Grapple start/stop.
- Space → jump press and continuous `SetJumpHoldInput(isPressed)`.
- C/Ctrl → crouch/slide.
- Q/F → Slow Motion toggle.
- R → safe platform respawn.
- Escape → cursor lock toggle.
- Left Shift → sprint.

Inspect actual Input Actions asset and resolve conflicts/rebinding/duplicate action delivery.

## 6. Pneumatic Wind Cannon — `PushAbility.cs`

- No bullets; pneumatic pressure blast.
- Range: 12 m.
- Target impulse: 18 N·s to Rigidbody/`IKineticTarget`.
- Player recoil only if a surface is within 3.8 m; open air beyond this distance gives zero player recoil.
- Recoil speed formula:
  `v_recoil = v_kick * (0.8 + (1 - d/3.8) * 0.4)`.
- `v_kick = 13.5 m/s`.
- At nearer surfaces, recoil is stronger.
- Aiming down at a floor yields a reported 4.5–6 m vertical launch.
- `hasAirRocketJumped` limits to one upward rocket jump while airborne.
- Subsequent airborne blasts cancel the vertical component but preserve horizontal redirection.
- Wall kick when hit normal Y < 0.6:
  `d_recoil = Normalize(Lerp(-d_aim, n_wall, 0.5))`.
- Slide recoil redirects forward along slide trajectory: +10 m/s, cap 35 m/s.

Preserve the surface proximity rule and pneumatic fantasy. Ensure recoil is applied as a velocity change/impulse through one coherent movement authority.

## 7. Pull / Grapple / Vortex — `PullAbility.cs`

### Target arbitration

On RMB:
1. Raycast 16 m using `Interactables` LayerMask. Movable prop/crate can be snatched and held 2.5 m ahead of hands.
2. If no interactable, raycast + SphereCast radius 0.55 m up to 75 m with `Environment` LayerMask for wall/tower/floor/spire grapple.
3. If no valid target, idle.

### Prop snatching

- Spring/damper target force described as `F_pull = (P_target - P_crate) * 10.0`.
- Hover distance: 2.5 m.
- Angular damping: 6.0.
- Collider stays active so object can block sniper/laser projectiles.
- Anti-box-riding: do not pull if player feet are touching the held prop (`groundedObject == prop.transform`).

### Grapple traversal

- Maximum range: 75 m.
- Ray origin: camera optical center.
- SphereCast fallback: radius 0.55 m.
- Do not overwrite all velocity with anchor direction; preserve sideways momentum.
- Inward direction: `u = Normalize(P_anchor - P_player)`.
- Pull acceleration: 55 m/s², integrated over delta time.
- `v_new = v_current + a_pull * Δt`.
- Inward component: `v_inward = v_new · u`.
- Cap inward component to target 24–35 m/s; subtract excess along `u` while leaving tangential velocity.
- Result: intended centrifugal/pendulum-like arcs around pillars/towers.
- Downward/non-upward grapple auto-slide if `pullDir.y <= 0.25`; slide target speed is described as `max(CurrentSpeed + 4, targetSpeed)` and 24–34 m/s.
- Release by RMB or tap Space mid-grapple.
- Fling speed formula:
  `v_boosted = min(max(v_current, 16) + 6.5, 35) m/s`.
- Direction:
  `d_fling = Normalize(Slerp(d_momentum, d_aim, 0.70))`.
- LOS obstruction probe from camera +0.8 m to anchor + normal*0.35 m; ignore the hooked collider itself; break only on a real intervening obstruction.

The exact ordering and whether the inward cap is always desired must be audited and tested. Keep grapple pull and crate suction distinct.

## 8. Crate Cannon

While holding an object with Pull, LMB:
1. Release the held object.
2. Set projectile velocity to aim direction *35 m/s.
3. Play procedural launch punch/hiss.
4. Player recoil: -aim direction *6.5 m/s.
5. Draw 20-step LineRenderer parabolic trajectory prediction.
6. On impact call `target.OnPhysicsCrush(impactSpeed, prop)` for kinetic shatter damage.

## 9. Focus Time Dilation — `SlowMotionAbility.cs`

- `Time.timeScale = 0.25`.
- `Time.fixedDeltaTime = 0.02 * 0.25 = 0.005 s` in the documented setup.
- Input, mouse aim, camera look, and audio use unscaled delta time where required.
- `infiniteSlowMo = true`, no drain/cooldown in baseline.
- Camera FOV +8°; disable screen-obscuring haze.

Confirm no repeated scaling of fixedDeltaTime occurs if toggled more than once. Restore original fixed timestep robustly. Ensure movement force integration uses the intended time domain.

## 10. HUD — `AbilityHUD.cs`

Four targeting modes:
1. Grab Object: amber/yellow square, within 16 m, text like `[RMB] GRAB · BLOCK_MEDIUM_5KG · 4.2m`.
2. Grapple Anchor: electric cyan diamond/plus, within 75 m, `[RMB] GRAPPLE · 38.5m`.
3. Armed Crate Cannon: neon-green missile reticle, `[LMB] FIRE CRATE CANNON (35 m/s)`.
4. Neutral open air beyond 75 m: translucent white crosshair and hidden readout.

Speedometer labels:
- `GRAPPLE · 28.4 m/s` cyan.
- `SLIDE · 22.1 m/s` green.
- `B-HOP · 18.5 m/s` cyan.
- `SPRINT · 11.5 m/s` yellow.
- Generic speed in white.

## 11. Procedural audio — `PlayerAudioEffects.cs`

Runtime audio uses `AudioClip.Create` and generated float buffers/frequency modulation; no external audio files required.

Documented sounds:
- Footstep: 75 Hz sine, exponential decay `e^-18t`, 0.12 s.
- Wind Cannon: white noise + 90 Hz sub-bass, 0.35 s.
- Vortex: modulated 110 Hz turbine hum, loop.
- Crate Cannon: 120 Hz pneumatic impulse + 2400 Hz hiss, 0.28 s.
- Grapple attach: 1200 Hz metallic ping, 0.15 s.
- Slingshot: 400→1800 Hz whoosh sweep, 0.40 s.
- Focus enter: 160→40 Hz sub-bass drop, 0.45 s.
- Slide scrape: filtered white noise, pitch mapped to slide speed, loop.
- Target switch: A5 880 Hz + A6 1760 Hz dual tone, 0.45 s.

## 12. Interactive props and targets

### `InteractableProp.cs`
- Light crate: ~1 kg cyan; agile projectile.
- Medium crate: ~5 kg yellow; standard missile/shield.
- Heavy block: ~25 kg red; ground drag only (`hoverTarget.y = rb.position.y`); immovable cover.

### `TargetButton.cs`
Triggers:
1. Crate Cannon hit with impactSpeed >= 4 m/s.
2. Slide Bowling Ram with slideSpeed >= 5.5 m/s.
3. Direct Push blast.

Feedback:
- Core depresses inward 0.18 m.
- Material bright red → neon green.
- A5/A6 chime.
- Camera shake.
- Refills Focus energy (confirm relevance with infinite Focus setting).

## 13. Shared physics contract

`Assets/_Project/Scripts/Abilities/IKineticTarget.cs` is documented as:

```csharp
public interface IKineticTarget
{
    bool CanBePulled { get; }
    bool CanBePushed { get; }
    void OnPushed(Vector3 forceDirection, float forceMagnitude, Vector3 hitPoint);
    void OnPulled(Transform pullOrigin, float pullForce);
    void OnPhysicsCrush(float impactSpeed, GameObject flyingProp);
    void OnPlayerSlideRam(float slideSpeed);
}
```

The interface is shared among player abilities, enemy AI, and puzzle objects. Inspect all implementers before changing signatures.

## 14. Editor setup and test arena

`PlayerRigBuilder.cs` menu actions:
- `Tools > REBOUND > Create Redliner Grapple Arena (200m)`.
- `Tools > REBOUND > Setup Complete Player Rig`.
- `Tools > REBOUND > Snap Player to Safe Spawn`.

Documented arena:
- Floor: 200×200 m centered at (0, -0.5, 60).
- Perimeter walls: 35 m tall.
- Central grapple spire: 36 m at (0,18,60), four anchor brackets.
- Four 28 m towers at (±55,14,25) and (±55,14,95).
- Aerial swing gantries at Y=25 m.
- Surf ramp: 38 m long, 26° incline, starts around Y=16 m.
- Ski-jump kicker: 18° incline.
- Six target buttons at spire apex, towers, sky deck, launch ramps.
- Hazard pit: invisible floor Y=-12 m; instant reset without scene reload.

Audit whether this arena is already in the repository and whether its surfaces are suitable for traversal tests.

## 15. Master constants from the supplied baseline

| Constant | Value |
|---|---:|
| walkSpeed | 6.5 m/s |
| sprintSpeed | 11.5 m/s |
| crouchSpeed | 3.5 m/s |
| airAcceleration | 26 |
| jumpImpulse | 6.6 m/s |
| bHopBoost | +2.5 m/s |
| runThreshold | 7.5 m/s |
| slideBoost | +4.5 m/s |
| minSlideEntrySpeed | 14 m/s |
| minSlideSpeed | 3.5 m/s |
| slideFriction | 2.2 m/s² |
| slideDrag | 0.035 |
| slopeAcceleration | 28 m/s² |
| maxSpeed | 35 m/s |
| surfaceProximityDistance | 3.8 m |
| surfaceKickVelocity | 13.5 m/s |
| maxPullRange | 16 m |
| maxGrappleRange | 75 m |
| grappleFlingBoost | +6.5 m/s |
| launchSpeed | 35 m/s |
| slowMoTimeScale | 0.25 |
| infiniteSlowMo | true |

Do not copy these into code before checking current serialized values and use sites.


---

<!-- SOURCE FILE: 03_REDLINER_MOVEMENT_RESEARCH.md -->

# REDLINER — Movement Research Reference for REBOUND

**Target Roblox experience:** `https://www.roblox.com/games/94987506187454/REDLINER`  
**Research date represented by the collected public material:** November 13, 2026  
**Purpose:** Extract movement behaviors and design principles that can inform REBOUND. This is not a claim to possess REDLINER source code.

## 1. Identity and officially listed controls

The official Roblox listing identifies REDLINER as a fast-paced FPS combining swordplay, gunplay, parrying, and an in-depth movement system. It advertises “no speed limit.” The listing's default PC controls are:

| Input | Official listed action |
|---|---|
| LMB | Melee |
| Q | Gun |
| F | Parry (including bullets) |
| RMB | Grapple |
| Left Shift | Dash |
| Left Ctrl | Slide |
| Space | Wallrun against a wall |

Keybinds are changeable. The official listing identifies the developers/credits and the exact experience ID. See `08_SOURCE_REGISTER.md`.

**Important control nuance:** Community guides describe Space as Jump on the ground and Wallrun near a wall, while the official listing labels it Wallrun. Treat contextual jump/wallrun behavior as community-reported until tested in the live client.

## 2. Evidence and confidence rules

Every claim in this document uses one of these labels:

- **[D] Developer/official:** listed by the game's official Roblox page.
- **[C] Community-reported:** described by a fan guide, experienced-player documentation, player discussion, or community tutorial.
- **[M] Measured:** independently measured under a repeatable test. This research pack contains no new direct in-game measurement.
- **[O] Observed:** directly inspected gameplay footage or reproduced live. This research pass did not perform new frame-by-frame or live-gameplay experiments.
- **[I] Inferred:** a plausible technical model that could produce the reported behavior.
- **[U] Unknown:** insufficient reliable evidence.

Confidence is separate from evidence type. A technique can be well described by several community guides but still not have a verified implementation model.

## 3. Main movement philosophy

The most consistent theme in the guides is **preserving and redirecting speed while controlling the camera**. The four main tools are described as parts of one route-building system rather than isolated abilities:

- Dash: quick repositioning/directional burst.
- Slide: low-profile movement and momentum retention/exit.
- Grapple: large route change, pull, and redirection.
- Wallrun: use suitable geometry as a movement route.

Community material describes bhopping, bhop strafing, slide hopping, grapple redirects, slingshots, downward grapple/Slam, dash-grapple combinations, wallrun combinations, ziplines, and bounce pads. Some are advanced, patch-sensitive, or potentially exploit-like. They are not all necessarily current or intended mechanics.

The practical experience principle is not “always maximize speed.” Guides also emphasize looking ahead, keeping the route readable, and being able to end a movement chain with control.

## 4. Base locomotion and jumping

### 4.1 Ground locomotion

- **[U]** Exact ordinary walk/run speed, acceleration, friction, reverse movement scaling, strafing scaling, and maximum normal ground speed are not established by the sources gathered.
- **[D]** Shift is Dash, not Sprint.
- **[C]** Some tutorials describe moving forward as the normal basis for movement techniques, but they do not give repeatable baseline physics values.
- **[I]** A persistent velocity controller is a sensible recreation approach, but no public source proves REDLINER's exact internal controller architecture.

Do not invent base speeds, acceleration values, friction constants, or a Quake/Source implementation claim.

### 4.2 Jump and contextual Space behavior

- **[D]** Official listing calls Space Wallrun against a wall.
- **[C]** Fan guides describe Space as Jump on the ground and Wallrun near a wall.
- **[C]** Community guides describe holding Space while moving to bunny hop.
- **[U]** Exact initial jump speed, gravity, jump height, airtime, buffering, coyote time, and input edge semantics are unknown.

### 4.3 Bunny hop and air strafing

Community movement guides describe:

- Holding Space while moving to bhop.
- Forward movement plus smooth camera turning to bhop-strafe.
- Smooth rather than abrupt camera turns.
- Slide hopping by sliding around each ground contact while continuing a bhop route.

**[C]** These techniques are documented.  
**[U]** Exact air-acceleration formula, speed increment per turn, and whether holding Jump is always sufficient in the current build remain unknown.  
**[I]** Camera direction and wish direction likely contribute to how the player shapes the aerial trajectory, but the exact relationship has not been measured.

## 5. Dash

Official input: **Left Shift [D]**.

The Wave 1 community mechanics document reports:

- Dash consumes one stamina bar [C].
- Dash temporarily increases speed to approximately **110 u/s** [C].
- An airborne dash moves in the direction the player is looking [C].
- Dash has significant speed falloff [C].

Unknown:

- Whether 110 u/s describes total speed, additive impulse, or a temporary set-speed state.
- Dash duration, cooldown, charge recovery, grounded direction rules, vertical velocity preservation, and cancel conditions.
- Whether the reported behavior remains current in the November 2026 client.

**Candidate interpretation [I]:** Dash may add a temporary directional velocity component or apply a short acceleration/velocity target followed by decay. Do not claim this is original source behavior.

## 6. Slide

Official input: **Left Ctrl [D]**.

Wave 1 community documentation reports:

- A minor speed boost on slide entry [C].
- Momentum retention for roughly two seconds by making the player frictionless [C].
- Dash/slide combinations as a stable base route [C].

Other guides emphasize sliding immediately before or after landing to preserve route flow and exit a line of fire.

Unknown:

- Minimum activation speed.
- Whether the input is tap/hold/toggle in the current build.
- Exact friction profile and duration.
- Exact collider/stance change.
- Whether slide itself has an independent speed cap.

Do not copy the reported “two seconds frictionless” statement as an exact REBOUND tuning value.

## 7. Grapple

Official default PC input: **RMB [D]**. An older community guide uses E for an earlier/alternate binding; the official listing is the authority for the default PC control.

Community sources describe the grapple as a major movement route changer for vertical/horizontal travel and momentum redirection.

### 7.1 Ground Slam

The Wave 1 guide reports that grappling toward the ground while high in the air initiates Ground Slam [C]. The guide also reports a yellow crosshair indicator and a 60% Instability effect on impact, which are combat-specific details and are **not to be imported into REBOUND**.

For movement research, the relevant behavior is that downward grapple can create a distinct pending slam/impact state and can be cancelled or combined with other movement actions.

### 7.2 Chase grapple

The Wave 1 guide reports that grappling toward an airborne player triggers Chase and travels toward that player at 250 u/s [C]. This player-targeting combat feature is outside REBOUND's movement-only scope and must not be copied.

### 7.3 Grapple physics

- **[C]** Community guide descriptions refer to grapple elasticity and slingshotting.
- **[C]** BDG and other techniques imply grapple can interact with pre-existing dash and lateral momentum.
- **[I]** A radial/tangential decomposition or elastic tether could reproduce some of these behaviors.
- **[U]** Exact rope constraint, spring constant, damping, range, attachment validation, force curve, release impulse, and resource model are unknown.

### 7.4 Grapple slingshot/redirect

The Wave 1 guide describes slingshot as redirecting movement through grapple elasticity to gain speed [C]. It also describes grappling sideways or choosing routes to redirect rather than merely pulling straight toward the anchor.

Candidate behavior [I]: modify radial velocity toward the anchor while retaining tangential/lateral velocity, then detach with the resulting velocity. No exact formula is verified.

## 8. Advanced movement-tech encyclopedia

These techniques are community-documented. Their availability and strength can change after patches.

### 8.1 Back Dash Grapple (BDG)

**Reported sequence [C]:**

1. Jump.
2. Dash backward.
3. Grapple forward simultaneously or within a very tight timing window.
4. Use grapple elasticity to propel forward.

Wave 1 guide reports approximately **130–145 u/s** on a good timing/angle. The guide says a follow-up turn/slingshot can redirect without using another dash. It reports that slingshot can avoid ordinary dash speed falloff.

**Technical implication [I]:** movement transitions overlap; the grapple may preserve/transform the backward dash impulse instead of letting the dash component simply decay.

**Version note:** A player discussion dated August 21, 2026 describes BDG as still usable around 120–130 ups but says retaining its speed in combat felt more difficult after an update. Treat historical values as version-dependent.

### 8.2 Slam Slide / Sage Slide

**Reported sequence [C]:**

1. Look down.
2. Jump.
3. Dash backward.
4. Grapple the floor to initiate Ground Slam.
5. Crouch/slide at a narrow timing window to cancel the slam.
6. Convert the downward/pending-slam transition into forward speed.

The guide reports 125 u/s base, +5 u/s when holding slide briefly after activation, and up to about 150 u/s with slingshot. These are community values with no independently verified unit calibration.

### 8.3 Slam Storage

The guide describes a pending Ground Slam that remains armed as long as the player avoids touching the ground. It can reportedly be delayed by wallrunning or directional dashes, then resolves on a later ground contact.

Movement-only relevance: an ability can remain pending while another movement state occurs. This is a useful design pattern for REBOUND only if a corresponding mechanic is intentionally designed; do not add REDLINER's combat Slam state verbatim.

### 8.4 Willow Walling / Hyper Blitz

Reported sequence [C]:

1. Start with movement against/along a wall.
2. Dash.
3. Initiate wallrun during the dash.
4. The wallrun reportedly makes another dash available.
5. Repeat to accumulate speed.
6. Slingshot after the last dash to avoid the reported falloff.

The Wave 1 guide reports high-200 u/s speeds and around 400 u/s as difficult. These are historical, unverified, and should not be converted to Unity m/s.

### 8.5 Super Jump

Reported sequence [C]: dash while jumping with precise timing. The guide claims the dash momentum transfers into the jump without ordinary dash falloff, launching the player into the air. Exact frames and behavior are unknown.

### 8.6 JarBoost / Super WallJump

Reported sequence [C]:

1. Grapple to the ground to start Ground Slam.
2. Immediately wall-jump and dash in the direction intended for flight.
3. The guide says the second action must happen almost instantaneously after grapple contact.
4. Curved surfaces reportedly create more momentum than flat surfaces.
5. The pending Slam may remain stored until eventual ground contact.

This is a highly timing-sensitive community technique, not a required REBOUND feature.

### 8.7 Slide Redirect

Reported as an intended REDLINER mechanic [C]: slide backward at landing with good timing; the player slides backward while retaining forward momentum. The guide describes a unique backward-slide sound cue.

Movement principle: requested slide direction and inherited velocity direction can differ during a landing transition.

### 8.8 Wallrun Redirect

Reported [C]: wallrun, then hold forward toward the intended release direction as the player exits. The guide claims it can redirect away from different walls in the camera-facing direction.

### 8.9 Wall Sling

Reported combination [C]: BDG followed by wallrun to extend speed, then slingshot/redirect off the wall. The guide claims wallrunning prevents dash decay and historical versions added 10–15 speed. Version-sensitive.

### 8.10 Zipline and Zipline Redirect

Wave 1 guide reports:

- Press R near a hanging zipline to attach.
- A reported travel speed around 135 u/s.
- Jumping off/redirecting with Space immediately after grabbing can send the player toward the camera-facing direction.
- Attaching may refresh wallrun availability.

These environmental systems are not currently listed as REBOUND player features. Treat as optional future design inspiration only.

### 8.11 Zip Boost / Zip Dash

Reported sequence [C]: near a hanging zipline, initiate Ground Slam and attach to the line at nearly the same time, then cancel Slam. The guide claims speeds above 200 u/s. This is highly timing- and geometry-dependent.

### 8.12 Bounce Pads

Wave 1 guide reports that bounce pads launch upward while retaining forward movement; a developer version reportedly launches at 100 u/s. No current numeric behavior verified.

### 8.13 Combat-origin movement — EXCLUDED FROM REBOUND IMPORT

Community sources describe gun recoil and parry knockback as movement sources. These are not to be implemented as REDLINER-style guns/parries in REBOUND. REBOUND already has its own pneumatic recoil and physical crate projectile system; improve those in their own design context only.

## 9. Momentum findings

### 9.1 What is supported

- **[D]** REDLINER advertises no speed limit.
- **[C]** Dash has falloff.
- **[C]** Slide is described as preserving momentum.
- **[C]** Bhop/strafe and slide hop are described as carrying speed across jumps.
- **[C]** Grapple elasticity can redirect movement and slingshot.
- **[C]** BDG can combine backward dash and forward grapple.
- **[C]** Wallrun routes can interact with repeated dash/slingshot.
- **[C]** Environmental geometry and launch pads can contribute to momentum.

### 9.2 What remains unknown

- Ordinary grounded and airborne friction constants.
- Ground acceleration and max wish speed.
- Exact vector decomposition and controller type.
- Whether the game has any conditional speed clamps.
- Whether “no speed limit” applies to every state and every version.
- Exact momentum behavior on collision, landing, and grapple detachment.
- Exact current numeric behavior of all advanced techniques.

### 9.3 Historical community-reported numeric references

| Technique/property | Reported value | Evidence and caution |
|---|---:|---|
| Dash temporary speed | 110 u/s | [C], Wave 1 guide; meaning/units not calibrated |
| Ground Chase grapple | 250 u/s | [C], player-targeting combat feature; do not import |
| Slide momentum retention | About 2 s | [C], frictionless claim from Wave 1 guide |
| Grapple charges | 2 | [C], old grapple guide; resource system conflict exists |
| Per-charge recharge | 2 s | [C], old grapple guide; verify current build |
| Slam Slide base speed | 125 u/s | [C], uncalibrated |
| Slam Slide held-slide bonus | +5 u/s | [C], uncalibrated |
| Slam Slide + slingshot | Up to about 150 u/s | [C], uncalibrated |
| BDG | 130–145 u/s | [C], historical |
| Willow Walling | High 200s u/s | [C], historical |
| Difficult Willow Walling run | About 400 u/s | [C], historical |
| Zipline | 135 u/s | [C], historical |
| Zip Boost | Above 200 u/s | [C], historical |
| Wallrun after zipline | Up to 130 u/s | [C], historical |
| Developer bounce pad | 100 u/s | [C], not necessarily player-accessible |

**Do not paste these numbers into REBOUND.** The `u/s` scale is not calibrated to meters or Unity world units. These are research leads and relative behavior indicators.

## 10. Candidate physics abstractions for recreation

These are implementation patterns that may reproduce observed effects. They are **not claims about REDLINER's hidden source code**.

### 10.1 Shared velocity state

Represent player velocity as:

`v = (vx, vy, vz)`

Use movement abilities to apply explicit acceleration, impulses, projections, or friction rather than setting the entire velocity to a fixed speed every frame.

### 10.2 Directional acceleration

`v_next = v_current + a * dt`

Control acceleration along the requested direction while preserving perpendicular momentum.

### 10.3 State-dependent drag

`v_next = v_current * exp(-k * dt)`

Use different drag coefficients for ground, air, slide, dash, grapple, and other intentional states. This is a convenient candidate model, not a measured REDLINER formula.

### 10.4 Grapple radial/tangential decomposition

For anchor direction `u = normalize(anchor - player)`:

`v_radial = dot(v, u) * u`

`v_tangent = v - v_radial`

Modify radial motion through pull/tension and preserve some or all tangent velocity. This can yield swinging and slingshot-like behavior.

### 10.5 Wall-plane projection

For wall normal `n`:

`v_wall = v - dot(v, n) * n`

Then apply controlled adhesion, acceleration, gravity, and detach logic.

### 10.6 Landing transitions

Separate the landing event from the resulting locomotion state. Resolve vertical contact, preserve or modify horizontal velocity, then process buffered jump/slide/ability transitions in a deterministic order.

## 11. Version and source limitations

- The Wave 1 community mechanics document labels itself as a community/players' guide and warns that tech can change in future updates.
- It contains December 2025 change-log entries; this is historical documentation.
- A June 2026 guide describes core movement and practical routing.
- An October 25, 2026 fan guide describes movement as dash, slide, wallrun, grapple, Slam, bhop, redirects, and recoil movement.
- A player discussion in August 2026 reports BDG speed retention feeling different after an update. This is anecdotal, not patch documentation.
- The official Roblox listing establishes the game identity and control labels but does not explain physics values.
- No live-gameplay testing or new frame-by-frame measurement was conducted for this pack.

## 12. Movement features to borrow for REBOUND (not copy blindly)

Prioritize these principles:

1. Persistent velocity across locomotion and abilities.
2. Camera-aware but not camera-snapping air steering.
3. Reliable hold/buffered bunny hopping.
4. Slide that changes friction and preserves speed.
5. Landing-based slide redirect.
6. Grapple with tangential momentum preservation and deliberate release.
7. Clear combinations between slide, jump, grapple, and REBOUND's pneumatic recoil.
8. Optional wallrunning only where the level geometry and control design support it.
9. Feedback that communicates speed/state without obscuring aim.
10. Skill expression through timing and trajectory choice, not hidden random boosts.

Do not implement REDLINER's combat systems, gun recoil, sword/parry behavior, instability, or its exact UI/game identity.


---

<!-- SOURCE FILE: 04_REBOUND_TARGET_MOVEMENT_SPEC.md -->

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


---

<!-- SOURCE FILE: 05_ARCHITECTURE_AND_IMPLEMENTATION_PLAN.md -->

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


---

<!-- SOURCE FILE: 06_VALIDATION_AND_TEST_SUITE.md -->

# REBOUND Movement Upgrade — Validation and Test Suite

## 1. Test setup requirements

Use a reproducible test scene or the documented 200 m REBOUND grapple arena. The test area should include:

- Large flat floor with visible distance markings.
- Flat elevated platform for jump/drop tests.
- 26° ramp and at least one opposing uphill section.
- Long clear run-up lane.
- Vertical walls at multiple distances and orientations.
- Overhang/ceiling for crouch/stand clearance.
- Grapple anchors forward, sideways, overhead, and below the player.
- Light, medium, and heavy props.
- Target button and destructible kinetic target.
- Hazard/reset volume.

Use identical player spawn position, rotation, Rigidbody state, and input sequence for before/after comparisons.

## 2. Instrumentation required

Record at minimum:

- Timestamp and time domain.
- Player position.
- Total velocity, horizontal velocity, vertical speed, speed magnitude.
- Input vector and camera forward.
- Grounded flag, ground normal and slope angle.
- Locomotion/traversal states.
- Current friction/drag/acceleration values.
- Jump/slide timers and active input flags.
- Grapple anchor/distance/radial/tangential velocity.
- Ability impulse source and delta-v.
- Whether any speed cap or safety clamp engaged.
- Collision events and landing transition.

Use a debug HUD and, where practical, CSV output. Hide debug tools in normal play builds.

## 3. Core test cases

For every test, record: starting conditions, input sequence, result, measured values, pass/fail, and notes. Repeat important tests at least five times.

### A. Ground locomotion

| ID | Test | Procedure | Expected behavior |
|---|---|---|---|
| G01 | Start from rest | Hold forward from stationary for 2 s | Smooth acceleration; no unexplained snap |
| G02 | Release input | Accelerate, release all movement | Predictable deceleration; velocity does not instantly zero |
| G03 | Reverse braking | Build forward speed, hold backward | Controlled braking and eventual reversal; no unstable oscillation |
| G04 | Strafe while moving | Build forward speed, hold lateral input | Lateral velocity develops while useful forward velocity remains |
| G05 | Camera turn | Maintain movement, turn camera 90° | Camera responds immediately; velocity changes progressively |
| G06 | Diagonal normalization | Compare forward vs diagonal input from rest | Diagonal input does not gain free magnitude |
| G07 | Slope down | Slide/run downhill | Acceleration matches configured slope model; no double gravity |
| G08 | Slope up | Slide/run uphill | Energy loss is coherent and configurable |
| G09 | Ground collision | Run into wall at increasing speeds | Collision resolves without tunneling or excessive unexplained velocity injection |

### B. Jump and aerial control

| ID | Test | Procedure | Expected behavior |
|---|---|---|---|
| A01 | Jump from rest | Tap Space from grounded | Consistent takeoff and measured height/airtime |
| A02 | Jump while moving | Build speed, jump | Horizontal momentum is preserved |
| A03 | Hold-to-bhop | Hold Space across repeated landings | Repeatable hop only on eligible ground contact |
| A04 | Buffered jump | Press Space shortly before landing | Jump occurs on landing if within buffer |
| A05 | Coyote jump | Walk off edge and press Space within configured grace | Jump accepted within window only |
| A06 | Air steer | Jump and hold lateral/forward directions | Trajectory changes predictably without instant velocity replacement |
| A07 | Air strafe curve | Hold forward and smoothly turn camera | Curved trajectory; measurable acceleration; no random speed spikes |
| A08 | Air no-input | Jump, release all input | Air drag/gravity behavior is predictable |
| A09 | Downward velocity | Fall from known height | Fall speed and gravity behavior are consistent |
| A10 | Landing at speed | Land after fast horizontal travel | Horizontal velocity retention is measured and intentional |

### C. Slide and transitions

| ID | Test | Procedure | Expected behavior |
|---|---|---|---|
| S01 | Crouch while slow | Hold crouch while stationary/walking | Low crouch, no accidental power slide |
| S02 | Slide entry | Sprint forward then crouch | Eligible slide begins with defined momentum result |
| S03 | Slide below threshold | Repeat at speeds below entry threshold | Correctly remains crouch-walk or rejects slide |
| S04 | Slide drag | Enter slide on flat floor, no input | Measured speed decay matches tuning |
| S05 | Slide steer | Slide and steer left/right | Controlled curvature, not instant snap |
| S06 | Slide jump | Jump during slide | Defined slide exit and horizontal velocity retention |
| S07 | Slide hop | Repeated slide/landing/jump chain | Stable repeated transitions |
| S08 | Landing-to-slide | Hold slide before landing at eligible speed | Slide enters without an unwanted full-friction frame |
| S09 | Slide redirect | Land with directional/reverse slide request | New slide direction can differ from inherited velocity; result is stable and learnable |
| S10 | Slide min duration | Enter/try cancel rapidly | No state flicker; minimum duration works if retained |
| S11 | Ceiling slide | Slide under a low ceiling then attempt to stand | Capsule cannot stand through geometry |
| S12 | Slide collision ram | Slide into an IKineticTarget above its threshold | Target receives one valid ram event; player response is controlled |

### D. Grapple traversal

| ID | Test | Procedure | Expected behavior |
|---|---|---|---|
| P01 | Target selection | Aim at nearby Interactable and environment anchor | Existing priority is respected |
| P02 | Max range | Test just inside/outside each range | Valid/invalid states match configured range |
| P03 | Forward pull | Grapple forward from rest | Consistent pull acceleration and arrival behavior |
| P04 | Lateral pull | Grapple to side anchor while moving forward | Useful forward/tangential momentum is retained |
| P05 | Overhead pull | Grapple to overhead anchor | Vertical and horizontal response is stable |
| P06 | Downward grapple | Aim at ground from elevated position | Existing auto-slide logic behaves correctly |
| P07 | Swing arc | Begin with lateral velocity and grapple a pillar | Tangential velocity supports an arc; no straight-line snap |
| P08 | Release | Release while moving around an anchor | Current velocity is retained; no unexplained zero or oversized impulse |
| P09 | Re-aim/re-attach | Release and grapple another anchor | New transition does not corrupt prior velocity/state |
| P10 | LOS obstruction | Put wall between player and anchor | Tether breaks or rejects correctly; anchor collider does not self-block |
| P11 | Thin anchor | Target narrow beam/pillar | SphereCast fallback works as intended |
| P12 | Held object + grapple | Snatch prop and release/cannon | Prop behavior remains separate from traversal grapple |
| P13 | Anti-box-riding | Stand on a prop then try to pull it | Anti-levitation rule works |

### E. REBOUND propulsion and ability chains

| ID | Test | Procedure | Expected behavior |
|---|---|---|---|
| R01 | Wind Cannon in open air | Fire with no surface inside proximity threshold | No player recoil |
| R02 | Wind Cannon near surface | Repeat at known distances below threshold | Recoil magnitude follows configured curve |
| R03 | Wall kick | Fire toward vertical wall | Recoil deflects away from wall predictably |
| R04 | Slide recoil | Fire while sliding | Forward slide boost stacks with current momentum and respects cap policy |
| R05 | Air rocket-jump limit | Use downward Push in air, then fire again | First upward launch allowed; later behavior matches intended restriction |
| R06 | Grapple → release → air steer | Swing, release, steer | Useful tangent velocity remains and is steerable |
| R07 | Grapple → Push | Attach/swing then use Push | Recoil integrates with grapple momentum without two controllers fighting |
| R08 | Slide → Push | Slide and fire near surface | Defined forward boost and no duplicate impulse |
| R09 | Crate Cannon | Hold and fire crate | Projectile speed, player recoil, arc, impact callback all work |
| R10 | Slide ram → target | Ram button/target at threshold | Target activates exactly once |
| R11 | High-speed ability chain | Run → slide → hop → grapple → release | No unexplained speed reset or broken state |
| R12 | Respawn after fall | Enter hazard pit | Safe spawn works and resets intended transient states |

### F. Optional wallrun and environment tests

Only run these if those features are implemented.

| ID | Test | Procedure | Expected behavior |
|---|---|---|---|
| W01 | Wall eligibility | Approach valid and invalid wall surfaces | Only designated valid surfaces attach |
| W02 | Wallrun entry | Jump into wall within angle/range | Stable entry, no floor/ceiling false positive |
| W03 | Wallrun travel | Maintain wall contact and move | Tangential velocity remains controllable |
| W04 | Wallrun exit | Detach/jump | Clear exit vector and no snapping |
| W05 | Wallrun → grapple | Grapple while traversing wall | Deterministic transition and valid anchor |
| W06 | Wallrun collision | Approach corner/edge | Detach safely or transition as designed |
| E01 | Launch surface | Hit launch pad with forward velocity | Upward boost while preserving intended horizontal component |
| E02 | Zipline attach/exit | Attach, traverse, detach | No velocity reset and clear endpoint behavior |

### G. Camera, audio, and time dilation

| ID | Test | Procedure | Expected behavior |
|---|---|---|---|
| C01 | Aim at speed | Move at highest tested speed and track target | Mouse look remains responsive and readable |
| C02 | Dynamic FOV | Accelerate/decelerate | FOV changes smoothly and not excessively |
| C03 | Camera bank | Strafe/turn | Subtle presentation only; no physics yaw coupling |
| C04 | Landing dip | Land from different fall speeds | Dip is proportional, bounded, and recovers smoothly |
| C05 | Trauma shake | Trigger push/grapple/target effects | Shake does not obscure control |
| C06 | Audio state changes | Enter/exit slide, grapple, focus | Useful cues occur once and at correct transitions |
| T01 | Focus toggle | Toggle repeatedly | Time scale and fixed timestep restore correctly |
| T02 | Focus movement | Move, jump, grapple while at 0.25 timeScale | Input/look remain responsive; physics scales intentionally |
| T03 | Focus timer domains | Test buffer/coyote while Focus active | Grace windows match documented gameplay-time policy |

## 4. Quantitative measures

For position samples `p1`, `p2` separated by `dt`:

`v = (p2 - p1) / dt`

For velocity samples `v1`, `v2`:

`a = (v2 - v1) / dt`

Record:
- Peak horizontal speed.
- Peak total speed.
- Time to reach target speed.
- Speed decay after input release.
- Speed retained at takeoff/landing.
- Velocity angle change after input/ability.
- Grapple radial and tangential velocity before/after.
- Repeatability across trials (mean and range).

Compare velocity **vectors**, not only scalar speed. A player traveling 20 m/s in the wrong direction is not a successful redirect.

## 5. Acceptance gates

### Gate 1 — Stability

- No new compile errors.
- No new runtime exceptions.
- No repeatable tunneling or physics explosions.
- No unwanted player rotation/tipping.
- No ceiling penetration when standing.

### Gate 2 — Locomotion

- Ground acceleration/braking is repeatable.
- Jump height and timing are measured and consistent.
- Hold-to-bhop reliably fires on valid landings.
- Slide does not erase faster incoming velocity.
- Landing transitions do not insert an accidental full-friction frame.

### Gate 3 — Ability integration

- Grapple retains tangential velocity.
- Release retains current velocity unless a documented boost is intended.
- Wind Cannon recoil stacks correctly and respects the 3.8 m surface rule.
- Crate snatching/cannon/puzzle callbacks remain functional.

### Gate 4 — Feel and readability

- Camera remains controllable at speed.
- State transitions are understandable.
- No excessive shake/FOV that reduces aim.
- Movement cues are synchronized with actual state transitions.

### Gate 5 — Regression

- Existing target button behavior works.
- `IKineticTarget` implementers still compile and work.
- Focus time dilation toggles/restores correctly.
- Respawn, mantle, low-ceiling checks, and hazard reset still work.

## 6. Test result template

For each case:

```text
Test ID:
Build/commit:
Scene/prefab:
Unity version:
Starting position/rotation:
Starting velocity:
Input sequence and timing:
Camera orientation:
Expected result:
Observed result:
Measured values:
Pass / Fail / Not Run:
Reproduction notes:
Screenshot/video/log path:
```

Never mark an unexecuted manual test as passed. Use `Not Run` and explain why.


---

<!-- SOURCE FILE: 07_PARAMETER_LEDGER.md -->

# REBOUND Movement Upgrade — Parameter Ledger

This ledger prevents accidental mixing of REBOUND's current values with REDLINER community-reported values or unverified engineering proposals.

## 1. Status labels

- **Baseline-Provided:** stated in the REBOUND design specification; must still be checked against code/Inspector.
- **REDLINER-Community:** reported by a public community guide, version-sensitive and not independently measured here.
- **Unknown:** no defensible value established.
- **Candidate/Tunable:** implementation parameter to introduce or tune through tests; not an original REDLINER value.

## 2. REBOUND baseline values

| Parameter | Supplied value | Units | Status | Required action |
|---|---:|---|---|---|
| Rigidbody mass | 75 | kg | Baseline-Provided | Verify prefab |
| Walk speed | 6.5 | m/s | Baseline-Provided | Verify code/config |
| Sprint speed | 11.5 | m/s | Baseline-Provided | Verify code/config |
| Crouch speed | 3.5 | m/s | Baseline-Provided | Verify code/config |
| Air acceleration | 26 | m/s² assumed | Baseline-Provided | Verify units/formula |
| Jump takeoff velocity | 6.6 | m/s | Baseline-Provided | Verify and calculate actual jump height |
| Bunny-hop bonus | +2.5 | m/s | Baseline-Provided | Verify not repeatedly stacked unintentionally |
| Run/ram threshold | 7.5 | m/s | Baseline-Provided | Resolve relationship to 6 m/s slide threshold |
| Slide entry boost | +4.5 | m/s | Baseline-Provided | Verify actual transition |
| Minimum slide entry speed | 14 | m/s | Baseline-Provided | Resolve whether applied after boost or before |
| Minimum slide speed | 3.5 | m/s | Baseline-Provided | Verify exit rule |
| Slide friction/deceleration | 2.2 | m/s² | Baseline-Provided | Verify model; don't assume Coulomb friction if custom |
| Slide drag coefficient | 0.035 | unclear | Baseline-Provided | Establish equation and units |
| Slope acceleration factor | 28 | m/s² | Baseline-Provided | Establish meaning; avoid double-counting gravity |
| General/ability speed cap | 35 | m/s | Baseline-Provided | Determine whether hard/soft cap and which states use it |
| Coyote time | 0.15 | s | Baseline-Provided | Verify scaled/unscaled time policy |
| Jump buffer | 0.15 | s | Baseline-Provided | Verify scaled/unscaled time policy |
| Minimum slide duration | 0.35 | s | Baseline-Provided | Verify state transition |
| Ground check local Y | -1.0 | m | Baseline-Provided | Verify prefab scale |
| Capsule radius | 0.5 | m | Baseline-Provided | Verify |
| Standing capsule height | 2.0 | m | Baseline-Provided | Verify |
| Crouch capsule height | 1.15 | m | Baseline-Provided | Verify center/feet math |
| Standing camera height | 1.65 | m | Baseline-Provided | Verify |
| Crouched camera height | 1.05 | m | Baseline-Provided | Verify |
| FOV base | 85 | degrees | Baseline-Provided | Test camera comfort |
| FOV max | 105 | degrees | Baseline-Provided | Test; potentially too wide |
| FOV speed interpolation denominator | 30 | m/s | Baseline-Provided | Verify and tune |
| Max camera bank | ±2.5 | degrees | Baseline-Provided | Verify |
| Camera bank smoothing | 8 | likely 1/s | Baseline-Provided | Verify units |
| Landing dip | 0.15 | m | Baseline-Provided | Verify |
| Landing recovery | 0.2 | s | Baseline-Provided | Verify |
| Trauma decay | 1.5 | 1/s | Baseline-Provided | Verify |
| Surface recoil distance | 3.8 | m | Baseline-Provided | Preserve core rule |
| Wind Cannon kick speed | 13.5 | m/s | Baseline-Provided | Verify formula/actual delta-v |
| Wind Cannon target impulse | 18 | N·s | Baseline-Provided | Verify target mass response |
| Push range | 12 | m | Baseline-Provided | Verify |
| Push slide bonus | +10 | m/s | Baseline-Provided | Integrate with momentum/cap policy |
| Grapple object range | 16 | m | Baseline-Provided | Verify Interactables query |
| Environment grapple range | 75 | m | Baseline-Provided | Verify Environment query |
| SphereCast fallback radius | 0.55 | m | Baseline-Provided | Verify |
| Grapple pull acceleration | 55 | m/s² | Baseline-Provided | Integrate once with fixedDeltaTime |
| Grapple inward target range | 24–35 | m/s | Baseline-Provided | Define exact target rule |
| Grapple sling boost | +6.5 | m/s | Baseline-Provided | Validate if needed after physically based release |
| Grapple min base speed | 16 | m/s | Baseline-Provided | Validate formula |
| Grapple speed cap | 35 | m/s | Baseline-Provided | Align with global cap policy |
| Prop hover distance | 2.5 | m | Baseline-Provided | Verify |
| Prop pull coefficient | 10 | unclear | Baseline-Provided | Verify force/acceleration units |
| Prop angular damping | 6 | likely damping units | Baseline-Provided | Verify |
| Crate launch speed | 35 | m/s | Baseline-Provided | Verify |
| Crate launch player recoil | 6.5 | m/s | Baseline-Provided | Verify |
| Focus timescale | 0.25 | ratio | Baseline-Provided | Verify restoration |
| Focus fixed timestep | 0.005 | s | Baseline-Provided | Verify Unity version and original value |
| Mantle chest ray height | +0.2 | m | Baseline-Provided | Verify |
| Mantle top ray origin | +0.8 | m above wall | Baseline-Provided | Verify |
| Spawn ray origin | +50 | m | Baseline-Provided | Verify |
| Spawn foot offset | +1.1 | m | Baseline-Provided | Verify feet clearance |

## 3. REDLINER community-reported historical values

These are not REBOUND target parameters. Their units are reported as `u/s` and have not been calibrated to meters or Unity units.

| Property/technique | Reported value | Status | Use in REBOUND |
|---|---:|---|---|
| Dash temporary speed | 110 u/s | REDLINER-Community | Qualitative reference only; do not copy numerically |
| Chase grapple speed | 250 u/s | REDLINER-Community | Combat/player-targeting; exclude |
| Slide momentum retention | ~2 s frictionless | REDLINER-Community | Test concept only; do not copy duration blindly |
| Grapple charges | 2 | REDLINER-Community | Not applicable; REBOUND resource design is separate |
| Grapple recharge | ~2 s per charge | REDLINER-Community | Not applicable unless REBOUND design adopts resources |
| Slam Slide base speed | 125 u/s | REDLINER-Community | Qualitative reference only |
| Slam Slide extra boost | +5 u/s | REDLINER-Community | Qualitative reference only |
| Slam Slide + slingshot | ~150 u/s | REDLINER-Community | Qualitative reference only |
| BDG | 130–145 u/s | REDLINER-Community | Study overlap/timing principle, not numeric target |
| Willow Walling | high 200s u/s | REDLINER-Community | Historical, patch-sensitive; do not copy |
| Difficult Willow Walling | ~400 u/s | REDLINER-Community | Historical, patch-sensitive; do not copy |
| Zipline | ~135 u/s | REDLINER-Community | Optional environmental inspiration only |
| Zip Boost | above 200 u/s | REDLINER-Community | Historical exploit-like tech; do not copy blindly |
| Developer bounce pad | 100 u/s | REDLINER-Community | Not a REBOUND parameter |

## 4. Candidate parameters to introduce only if needed

These should be centralized, serialized, and tuned through measurements:

| Parameter | Purpose | Initial state |
|---|---|---|
| groundAcceleration | Rate to build control velocity | Unknown; tune |
| groundBrakingAcceleration | Opposing input braking | Unknown; tune |
| groundIdleDrag | Friction without directional input | Unknown; tune |
| groundMaxWishSpeed | Maximum acceleration target along wish direction | Unknown; tune separately from total speed |
| airAcceleration | Air trajectory shaping | Existing 26 candidate; verify |
| airMaxWishSpeed | Desired speed projection limit | Existing 16 candidate; verify |
| airDrag | Air momentum decay | Unknown; tune |
| slideDrag | Sliding momentum decay | Existing formula unclear; measure |
| slideSteeringAcceleration | Turn slide path | Unknown; tune |
| slideRedirectWindow | Landing redirect input window | Unknown; tune |
| landingHorizontalRetention | Horizontal speed retained on contact | Unknown; measure |
| grappleRadialAcceleration | Pull toward anchor | Existing 55 m/s² candidate; verify |
| grappleRadialSpeedLimit | Limit inward component | Existing 24–35 m/s range; clarify |
| grappleTangentialRetention | Preserve swing momentum | Target high; tune via test |
| grappleReleaseImpulse | Additional release component | Prefer zero until demonstrated necessary |
| wallrunAdhesion | Hold against wall | Only if wallrun is implemented |
| wallrunGravityScale | Vertical gravity while wallrunning | Only if implemented; unknown |
| wallrunDetachImpulse | Jump/exit from wall | Only if implemented; unknown |
| softSpeedCap | Prevent instability without hard-clamping every state | Evaluate against current 35 m/s cap |

## 5. Mandatory reconciliation before final tuning

1. Resolve 6 m/s slide eligibility vs 7.5 m/s run/ram threshold.
2. Resolve 14 m/s minimum slide entry vs `CurrentSpeed + 4.5 m/s` boost.
3. Derive jump height from actual upward/descent gravity behavior; reconcile 6.6 m/s and claimed 1.6 m.
4. Establish whether slide drag is constant deceleration, linear drag, or quadratic drag; document units.
5. Establish whether slope acceleration is natural gravity projected onto the plane or a gameplay multiplier.
6. Establish whether the 35 m/s cap applies to total speed, horizontal speed, ability delta-v, or only selected ability effects.
7. Establish which timers use scaled vs unscaled time under Focus.
8. Ensure grapple acceleration is integrated exactly once per FixedUpdate.
9. Verify collider and camera heights against actual capsule geometry.

## 6. Measurement rules

- Measure velocity from world positions over a known time interval or read Rigidbody velocity directly at a consistent physics tick.
- Record vectors, not just magnitudes.
- Record initial conditions and repeat at least five times.
- Use consistent timestep/frame rate and timeScale.
- Never compare Roblox `u/s` to Unity `m/s` without calibration.
- For every final tuning value, record: value, units, source, reason, measurement/test, and confidence.


---

<!-- SOURCE FILE: 08_SOURCE_REGISTER.md -->

# REDLINER Source Register and Evidence Notes

The sources below were used to assemble the movement reference. Community guides are not official source code or developer documentation. Treat advanced techniques and numerical values as community-reported and version-sensitive.

## S1. Official Roblox experience listing

**URL:** https://www.roblox.com/games/94987506187454/REDLINER  
**Type:** Official Roblox game page  
**Can establish:** Exact experience identity, public description, advertised no-speed-limit statement, default listed controls, keybinds are changeable, credits.  
**Cannot establish:** Acceleration, friction, jump height, dash equations, grapple force, actual cap behavior, or hidden implementation.  
**Evidence status:** [D] for public listing facts.

## S2. REDLINER Wave 1 community mechanics guide

**URL:** https://redliner.likegravity.dev/wiki  
**Type:** Player/community-maintained mechanics and advanced-tech guide (“Wave 1 Liners”).  
**Topics:** Dash, slide, momentum, grapple Ground Slam, Chase, Slam Slide/Sage Slide, slingshot, BDG, Willow Walling/Hyper Blitz, Super Jump, Slam Storage, JarBoost, Slide Redirect, Wallrun Redirect, Zipline Redirect, Wall Sling, Zip Boost, bounce pads, historical changelog.  
**Can establish:** What the contributing community documented/reported, technique names and historical input sequences, reported speeds and resources.  
**Cannot establish:** Current November 2026 behavior, developer-confirmed internals, independently calibrated units, or repeatable exact physics.  
**Evidence status:** [C]. The guide itself warns that techniques can change in future updates; its changelog includes December 2025 entries.

## S3. REDLINER movement guide — redliner.net

**URL:** https://www.redliner.net/movement/  
**Type:** Fan-made movement guide; listed as updated October 25, 2026.  
**Topics:** Core controls, slide, dash, wallrun, grapple, Ground Slam, Slam Cancel, bhop, bhop strafe, slide hop, grapple redirects, BDG, gun recoil movement.  
**Can establish:** A recent community description of how players are taught to use the movement system.  
**Cannot establish:** Exact internal physics or experimentally measured parameter values.  
**Evidence status:** [C].

## S4. REDLINER controls — redliner.net

**URL:** https://www.redliner.net/controls/  
**Type:** Fan-made controls guide; listed as updated October 25, 2026.  
**Topics:** Default keybinds, rebinding, reachable movement/aim inputs.  
**Can establish:** Contemporary fan-guide description of default controls and control ergonomics.  
**Cannot establish:** Hidden input priority or engine implementation.  
**Evidence status:** [C], with official controls still sourced from S1.

## S5. REDLINER movement guide — redliner.org

**URL:** https://redliner.org/movement/  
**Type:** Fan-made movement route guide; updated June 14, 2026.  
**Topics:** Dash, slide, grapple, wallrun, routes, camera-first movement, visible-keystroke video companion.  
**Can establish:** Practical community route recommendations and UX principles.  
**Cannot establish:** Exact physics parameters.  
**Evidence status:** [C].

## S6. REDLINER movement basics — redliner.org

**URL:** https://redliner.org/how-to-play/movement-basics/  
**Type:** Fan-made tutorial; updated June 14, 2026.  
**Topics:** Dash, slide, grapple, wallrun basics, camera-first route planning, movement practice.  
**Can establish:** Player-facing technique recommendations.  
**Cannot establish:** Source-code implementation or numerical physics.  
**Evidence status:** [C].

## S7. REDLINER slide guide — redliner.org

**URL:** https://redliner.org/movement/slide/  
**Type:** Fan-made slide guide; June 2026.  
**Topics:** Slide exits, momentum/route flow, dash-slide combinations, camera positioning.  
**Can establish:** Community usage advice.  
**Cannot establish:** Exact friction curve or state duration.  
**Evidence status:** [C].

## S8. REDLINER resources index

**URL:** https://www.redliner.net/resources/  
**Type:** Fan-made resources index.  
**Can establish:** Links and the publisher's guide taxonomy.  
**Cannot establish:** Physics.  
**Evidence status:** [C].

## S9. REDLINER mechanics wiki

**URL:** https://redliners.wiki/wiki/mechanics/  
**Type:** Fan wiki.  
**Can establish:** Community descriptions of grappling, dash, slide, wallrun, jump, and combat mechanics.  
**Cannot establish:** Whether precise numeric claims or implementation claims were tested or current.  
**Evidence status:** [C]; use cautiously, especially for exact claims.

## S10. REDLINER official Roblox metrics mirror (secondary)

**URL:** https://www.rolimons.com/game/94987506187454  
**Type:** Third-party Roblox game data mirror.  
**Can establish:** Additional listing metadata and update recency snapshot.  
**Cannot establish:** Movement patch details.  
**Evidence status:** Secondary reference only.

## S11. Qitadel movement demonstration

**URL:** https://www.youtube.com/watch?v=VaHC5DVGzdc  
**Type:** Gameplay/movement demonstration identified by community reference material.  
**Can establish:** Potential visual reference when inspected directly.  
**Cannot establish from the metadata alone:** Precise input sequences, frame timings, or velocity values.  
**Evidence status:** Visual lead; requires direct playback/frame analysis.

## S12. MineBlox movement guide

**Reference:** YouTube video titled “REDLINER Movement Guide” by MineBlox; June 18, 2026.  
**Type:** Gameplay tutorial.  
**Can establish:** Demonstrated basic movement techniques if playback is inspected.  
**Cannot establish from description alone:** Exact physics or hidden parameters.  
**Evidence status:** Community demonstration lead.

## S13. GaIaxy momentum tutorial

**Reference:** YouTube video titled “How to NEVER lose your MOMENTUM AGAIN in REDLINER” by GaIaxy; June 19, 2026.  
**Type:** Gameplay tutorial.  
**Can establish:** Player-recommended bunny-hop and slide-redirect technique if playback is inspected.  
**Cannot establish from description alone:** Exact engine physics or calibrated speed.  
**Evidence status:** Community demonstration lead.

## S14. Cato advanced-tech video

**URL:** https://www.youtube.com/watch?v=zh7pocajrTw  
**Title:** “USE these TECHS after BGD NERF” (July 18, 2026).  
**Type:** Advanced movement tutorial.  
**Can establish:** Historical community response to reported BDG balance change if directly inspected.  
**Cannot establish:** Current November behavior.  
**Evidence status:** Community demonstration lead.

## S15. August 2026 player discussion

**Reference URL:** https://www.reddit.com/ (discussion surfaced in research results; exact thread link should be retrieved before relying on it in a formal citation).  
**Type:** Player anecdote.  
**Topic:** Reported difficulty retaining BDG speed in combat after an update.  
**Can establish:** At least one player perception of a version change.  
**Cannot establish:** Actual patch contents or measured speed reduction.  
**Evidence status:** Anecdotal [C], low confidence. Do not cite as patch fact.

## Source use rules for the coding agent

1. The official game page is the only source in this register for officially listed REDLINER controls and identity.
2. The Wave 1 guide is the primary source for named advanced techniques and historical reported values, but those are community evidence and version-sensitive.
3. The 2026 movement guides are useful for current player-facing vocabulary, camera-first route habits, and learning sequence.
4. No source here proves REDLINER's original code architecture or exact physics equations.
5. Do not copy their numerical `u/s` values into REBOUND.
6. Do not import gun, sword, parry, instability, or combat designs into REBOUND.
7. If the agent chooses to research more, it should prefer recent developer/official material and direct footage with visible input overlays, and record exact timestamps only after inspecting the video.
