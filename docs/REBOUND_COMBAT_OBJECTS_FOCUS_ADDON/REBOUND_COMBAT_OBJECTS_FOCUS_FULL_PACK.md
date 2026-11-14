# REBOUND — Combat, Object Physics & Focus Upgrade Pack

This is the combined reading copy of the add-on resource pack. For agent use, also provide the ZIP so the documents remain separately addressable.


---

# FILE: 00_READ_ME_FIRST.md

# REBOUND — Combat, Object Physics & Focus Upgrade Pack

**Package type:** Add-on to the existing REBOUND × REDLINER movement resource pack  
**Implementation target:** The existing REBOUND Unity project (C#)  
**Scope:** Push/Wind Cannon, Pull/object manipulation, object classes and physical combat, Crate Cannon, Focus/slow motion, and integration with the existing enemy receivers.  
**Explicit exclusions:** Map design, Egyptian theme, level layout, new enemy archetypes, enemy AI redesign, and replacement of the movement upgrade already in progress.

---

## 1. What this pack is for

This pack is intended to be used **alongside the previous REBOUND movement resource pack**. The previous pack addresses the player's movement and momentum system. This add-on addresses how the player uses pneumatic abilities and physical objects to interact with enemies and the world while moving quickly.

The goal is to improve the existing game, not restart it. The coding agent must inspect the current Unity repository and the movement work already completed or in progress before editing anything.

## 2. Desired result

The player should be able to:

- Keep moving while fighting instead of having to stop and use a separate combat mode.
- Push enemies and physics objects with predictable results.
- Pull and carry eligible objects, use them as temporary cover, and launch them.
- Drag heavy objects along the ground without lifting them.
- Use the Crate Cannon as a reliable physical projectile system.
- Deflect eligible incoming projectiles with well-timed Push input.
- Use Focus to make difficult moments easier to read without removing the need for timing and skill.
- Receive clear visual, audio, and HUD feedback for every successful interaction.

The existing enemy roster and its attack/AI behavior remain intact. Improvements are made to the player's combat options and to the interfaces through which existing enemies receive physical impacts.

## 3. Files in this pack

1. `01_MASTER_AGENT_PROMPT.txt` — paste this into the coding agent after attaching the package.
2. `02_SCOPE_AND_DESIGN_BRIEF.md` — non-negotiable scope and intended gameplay behavior.
3. `03_PUSH_WIND_CANNON_SPEC.md` — Push, recoil, blast targeting, and projectile deflection.
4. `04_PULL_AND_OBJECT_MANIPULATION_SPEC.md` — object targeting, carrying, shielding, dragging, and grapple/object mode separation.
5. `05_OBJECT_PHYSICS_AND_COMBAT_SPEC.md` — weight classes, object traits, physical impacts, damage calculation, and breakage rules.
6. `06_CRATE_CANNON_SPEC.md` — launching, velocity inheritance, collision, weak points, and trajectory prediction.
7. `07_FOCUS_TIME_DILATION_SPEC.md` — rechargeable Focus design and Unity time-domain risks.
8. `08_ENEMY_INTEGRATION_AND_CONTRACTS.md` — preserve enemy behavior while integrating physical responses safely.
9. `09_UNITY_ARCHITECTURE_AND_MIGRATION.md` — implementation architecture and staged migration.
10. `10_VALIDATION_AND_PLAYTEST_SUITE.md` — required automated and manual tests.
11. `11_PARAMETER_LEDGER_AND_CONFLICTS.md` — baseline values, provisional tuning values, and documentation conflicts.
12. `12_ACCEPTANCE_AND_DELIVERY_CHECKLIST.md` — definition of done and required agent handoff.

`REBOUND_COMBAT_OBJECTS_FOCUS_FULL_PACK.md` contains all documents in one file for convenient reading.

## 4. How to give it to the coding agent

Provide:

- The previous REBOUND movement resource pack (if the agent does not already have it in context or repository notes).
- This complete add-on ZIP.
- The actual Unity project or repository.
- `01_MASTER_AGENT_PROMPT.txt` as the task instruction.

Do not ask the agent to reimplement the movement pack. Tell it that the movement work is already underway and this is the next feature package to integrate.

## 5. Required safe workflow

Before modifying code, the agent must:

1. Inspect the Unity version and scripting API compatibility.
2. Inspect Git status, current branch, uncommitted work, and project structure. Never discard, reset, overwrite, or stash the user's work without permission.
3. Read the current implementations of `PlayerMovement`, `PlayerAbilityInput`, `PushAbility`, `PullAbility`, `SlowMotionAbility`, `InteractableProp`, `TargetButton`, `IKineticTarget`, projectile scripts, and enemy receiver scripts where present.
4. Identify which parts of the previous movement pack have already been implemented.
5. Identify duplicate controllers, duplicate input subscriptions, conflicting event calls, and interface signature differences.
6. Produce a short integration plan before making edits.
7. Implement in small stages and compile/test after each stage.

The actual repository is the source of truth for code. The handoff documents are design context and may be stale.

## 6. Important implementation rule

Do not create a second movement controller, second grapple controller, second Push input reader, second Focus manager, or second object registry if one already exists. Extend the current systems or add narrowly scoped collaborators.

If the previous movement work is modifying a shared file, make a minimal compatible integration change. Do not overwrite the movement implementation merely to make this add-on easier.

---

# FILE: 01_MASTER_AGENT_PROMPT.txt

# MASTER CODING-AGENT PROMPT — REBOUND Combat, Object Physics & Focus Upgrade

You are a senior Unity gameplay engineer working inside the user's existing REBOUND Unity project. Your task is to implement the attached **Combat, Object Physics & Focus Upgrade Pack** as an additive upgrade to the movement work that is already underway.

## Primary objective

Improve the way the player fights using the existing pneumatic Push/Wind Cannon, dual-mode Pull/Vacuum Tether, physical objects, Crate Cannon, and Focus slow motion. The player must be able to use these systems while maintaining the high-speed movement behavior being implemented in the previous REBOUND movement upgrade.

This is an implementation task, not a request for a design-only answer. Inspect the project, plan the integration, modify the real scripts and prefabs, compile, test, and report exactly what changed.

## Mandatory scope

Implement and integrate:

1. Push/Wind Cannon targeting, impact handling, recoil integration, and projectile deflection.
2. Pull's object-manipulation mode: target selection, pulling, carrying, retraction, shielding, and heavy-object dragging.
3. A data-driven object classification/behavior system for light, medium, and heavy objects, with optional traits that do not require a map redesign.
4. Physical projectile impact handling and balanced damage/knockback from thrown objects.
5. Crate Cannon firing, velocity inheritance, recoil, prediction line, collision handling, and weak-point support.
6. Focus as a fair, rechargeable slow-motion resource, while preserving responsive player input and movement as far as the existing Rigidbody architecture permits.
7. Safe integration with the existing enemy receiver/interface scripts without redesigning enemy AI or replacing enemy archetypes.
8. Readable audio, visual, reticle, and HUD feedback for these systems.
9. Automated or reproducible validation for the interactions above.

## Explicitly out of scope

Do NOT:

- Design, generate, or modify the map, Egyptian theme, level layout, environment art, or level progression.
- Add, remove, replace, or redesign enemy archetypes.
- Rewrite enemy navigation, attack telegraphs, attack cadence, health, or AI state machines unless a very small adapter is strictly required for a new physical interaction. If an enemy-side change is necessary, preserve its current behavior and explain the change.
- Reimplement the movement upgrade from the previous resource pack.
- Replace the player's current movement controller or grapple traversal controller just to simplify the new systems.
- Change controls without checking the actual Input Actions and asking for permission if a genuine conflict cannot be resolved safely.
- Invent unverified REDLINER numerical values and copy them into REBOUND.
- Add a large progression, loot, crafting, inventory, or weapon system.
- Add dozens of unique object types. The requested object system is based on a small number of useful physical roles.

## Source-of-truth order

Use this order when information conflicts:

1. The actual Unity project code, serialized assets, prefabs, Input Actions, and current scene configuration.
2. The most recent REBOUND player subsystem specification and the work already implemented from the previous movement pack.
3. The current team handoff supplied by the user.
4. This add-on pack's design recommendations.

The handoff is explicitly described as outdated. Treat it as context, not guaranteed current code.

## Integration rule: movement work already exists or is underway

The previous resource pack focuses on movement and momentum. This package is a companion task. Do not reset or duplicate that work.

Before editing:
- Inspect Git status and do not destroy uncommitted work.
- Identify the current owner of player velocity, input, grapple movement, time dilation, and target interfaces.
- Write down the exact files that already contain movement-pack changes.
- Identify the smallest integration points for Push recoil, Crate Cannon recoil, object manipulation, and Focus.
- If a change overlaps an in-progress movement edit, integrate carefully instead of replacing the file.

All player impulses must go through the project's authoritative movement/velocity API if one exists. Do not let Push, Crate Cannon, Grapple, Slide, and the movement controller independently overwrite Rigidbody velocity.

## Required inspection

Inspect at minimum, where present:

- Player root prefab and Rigidbody/CapsuleCollider configuration.
- `PlayerMovement.cs` or its current successor.
- `PlayerAbilityInput.cs` and Input Actions asset.
- `PushAbility.cs`.
- `PullAbility.cs`.
- `SlowMotionAbility.cs`.
- `InteractableProp.cs` and all object prefabs.
- `TargetButton.cs`.
- `IKineticTarget.cs` and every class implementing it.
- Enemy physics receiver scripts and weak-point colliders.
- Sniper projectile and rapid-laser projectile scripts.
- HUD/reticle scripts.
- Audio effects scripts.
- Current layers, collision matrix, physics settings, and fixed timestep.

Do not assume a file path from documentation is correct. Locate the actual file.

## Core design requirements

### A. Push / Wind Cannon

- Keep the current LMB behavior and current Push identity.
- Push eligible objects and enemies through one consistent targeting/impact pipeline.
- Preserve the existing 12 m range and 18 N·s starting impulse only if those values match the current project and feel correct in testing. Treat them as tunable starting values, not hard facts.
- Preserve the existing close-surface recoil, wall kick, rocket-jump restriction, and slide-forward boost behavior if already implemented. Integrate those through the movement controller's external-impulse API.
- Do not apply player recoil in open air when the current design says the surface-proximity condition is not met.
- Add/verify projectile deflection for eligible incoming sniper rounds and rapid projectiles. Deflection must be a deliberate, readable, timing-based interaction, not an automatic global reflection of every projectile.
- If the player is carrying a valid object, LMB fires the Crate Cannon instead of also firing the normal Push. Prevent double activation.
- Respect target eligibility (`CanBePushed`, projectile deflectability, line of sight, layers, and one-shot input semantics).

### B. Pull / object manipulation

- Preserve dual-mode Pull: eligible movable target within the object range is selected for object manipulation; otherwise an eligible environment anchor can be selected for grapple traversal.
- Do not rewrite the movement grapple if it has already been upgraded. Keep object manipulation and traversal as explicit modes with a single, predictable target decision.
- Preserve current starting values if confirmed in project: 16 m object selection range, 75 m grapple traversal range, and approximately 2.5 m object hold distance.
- Light and medium objects may be lifted/held. Heavy objects are ground-drag only.
- Use a stable spring-damper or equivalent critically damped controller rather than snapping or repeatedly teleporting the object.
- Retract the held object's desired position when an obstruction is detected; the old handoff's 1.2 m narrow-space fallback is a starting reference, not a mandatory hard-coded value.
- Keep the object's collider active so it can physically block eligible projectiles and interact with the world. Prevent it from colliding with the player in a way that causes jitter or pushes the player. Restore collision settings when released.
- Prevent self-levitation/box-riding: if the object is supporting the player's feet, do not pull it into a held state until the player steps off.
- Heavy objects should not hover or become portable shields. Drag them on the ground, and allow them to be repositioned or pushed where their mass and scene geometry permit.
- Do not allow every enemy to be grabbed. Only existing enemies explicitly configured as pullable may be pulled. Preserve their existing AI and damage behavior.

### C. Object variety and physical combat

Implement a small, data-driven object system with three core classes:

- **Light:** quick to acquire and throw, low impact energy, useful against weak enemies or multiple light targets.
- **Medium:** main all-purpose combat object, useful against ordinary enemies and exposed weak points, can be held as a projectile shield.
- **Heavy:** ground-drag only, strong environmental blocker/weight/cover, not launched by the normal Crate Cannon.

Starting design references from the existing handoff are approximately 1 kg, 5 kg, and 25 kg. Inspect actual prefabs and current values before changing them. Mass alone must not define the entire role: configure pull acceleration, max force, launch eligibility, impact multiplier, shield durability, drag behavior, and material response as data.

Avoid needless object proliferation. Optional special traits such as fragile, dense, or reinforced may be supported by the data model, but implement them only if the core three classes are stable and there is a meaningful gameplay use. Do not build a new inventory or crafting system.

### D. Crate Cannon

- When a valid portable object is held, LMB launches it and suppresses the normal Push for that input event.
- Keep the current 35 m/s launch-speed component as a starting value if confirmed by current code. Decide and document whether this is total world speed or an aim-directed launch component plus inherited player velocity. Prefer preserving some player velocity without silently producing extreme uncontrolled speeds.
- Apply the current player recoil reference (approximately 6.5 m/s opposite aim) through the movement controller, not by replacing the whole Rigidbody velocity.
- Preserve or rebuild the 20-segment trajectory line using the same launch velocity, gravity, drag, and Focus assumptions as the actual projectile.
- Use Continuous Dynamic or an appropriate continuous collision mode while the prop is moving fast; choose the least expensive mode that reliably prevents tunneling.
- Compute damage from relative impact speed / normal impact energy, use configurable thresholds and caps, and support weak-point multipliers only when the actual impact point intersects a configured weak-point collider/component.
- One projectile must not apply damage every physics frame or repeatedly trigger the same target from `OnCollisionStay`. Use impact-event deduplication/cooldowns.
- Let physics resolve the impact and bounce/stop behavior. Do not make all projectiles disappear on every hit unless the object's data explicitly says it breaks.

### E. Focus / slow motion

- Replace infinite free use with a rechargeable resource unless current project direction or existing implementation shows the player explicitly rejected this recommendation. The target design is frequent tactical use, not a rare ultimate.
- Use a configurable meter, drain, recovery delay, recovery rate, and confirmed-action rewards.
- Suggested initial tuning only: 100 capacity, 25 units per unscaled second while active, 0.75 s recovery delay, and 15 units per unscaled second while recovering. These are playtest starting values, not final balance.
- Keep input, camera aim, UI, audio pitch handling, and the Focus meter responsive in real time.
- Slow enemy movement, enemy animations/logic, eligible projectiles, and movable object simulation consistently.
- The player should retain normal practical movement control and speed during Focus if the intended design still requires it. Unity global `Time.timeScale` also slows Rigidbody simulation; do not claim player movement is normal merely because mouse input uses `unscaledDeltaTime`. Inspect the actual physics architecture, implement a correct player/world time-domain solution, and validate displacement, gravity, collisions, and impulses under Focus. If the current dynamic Rigidbody architecture cannot safely provide this behavior with a small change, explain the tradeoff and implement the safest tested solution rather than adding arbitrary velocity multipliers.
- Prevent pausing, UI focus, respawning, scene changes, and application pause from leaving global timeScale or fixedDeltaTime corrupted.
- Reward meaningful confirmed actions, not spam. Suggested reward events are successful projectile deflection, successful weak-point impact, and meaningful objective activation. Add per-target/per-action anti-farming controls.

### F. Enemy integration

Do not change the enemy roster or AI. Preserve the existing four archetypes and tripwire hazard:
- Swarmer: keep current movement/attack behavior; allow existing intended slide-ram, push, pull, and crate-impact responses.
- Hulk: keep its leap/slam/stun behavior; retain resistance to ordinary Push/Pull and support damage to its existing exposed weak point.
- Sniper: keep target tracking and firing behavior; make its eligible projectile deflectable and allow the reflected shot to travel back toward its source.
- Rapid sentry: keep firing pattern; allow a carried eligible object to physically block its bolts and support existing flanking/crate impacts.
- Tripwire: preserve current hazard behavior; Focus may affect it only if it is already implemented as a time-scaled simulation object.

If interfaces have changed, add a compatibility adapter or migrate all implementers and callers atomically. Do not leave half the project using old method signatures.

## Required implementation stages

### Stage 0 — Audit and integration plan
- Inspect code/assets/settings.
- Record current values and existing behavior.
- Identify conflicts and in-progress movement changes.
- Produce a file-level integration plan.
- Do not change runtime code yet.

### Stage 1 — Shared target/interaction contracts
- Resolve `IKineticTarget` signature differences.
- Establish one consistent impact context or a safe adapter.
- Establish a deflectable-projectile contract/adapter.
- Compile all current implementers and callers.

### Stage 2 — Object data and Pull object mode
- Define/verify object weight classes and capabilities.
- Implement stable pulling/holding and obstruction retraction.
- Implement heavy ground dragging.
- Implement owner-collision handling and anti-box-riding.
- Validate object mode does not break grapple traversal.

### Stage 3 — Crate Cannon and physical impacts
- Implement valid launch rules and input priority.
- Implement player velocity inheritance and recoil.
- Implement trajectory prediction.
- Implement impact energy, damage caps, weak points, and event deduplication.

### Stage 4 — Push and projectile deflection
- Integrate blast targets and player recoil.
- Implement incoming projectile eligibility, timing/cone/range, reflection ownership, and feedback.
- Validate no unintended recoil or double-fire.

### Stage 5 — Focus
- Implement recharge and reward rules.
- Implement the world/player time-domain behavior.
- Validate real-time player movement, collision, physics, enemies, projectiles, held objects, audio, and cleanup.

### Stage 6 — Polish and balancing
- Reticle states, labels, audio, impact VFX, camera feedback, optional small flow/chain feedback.
- Tune values with playtests and profiler data.
- Do not add map work.

### Stage 7 — Regression and handoff
- Run all tests in `10_VALIDATION_AND_PLAYTEST_SUITE.md`.
- Run the previous movement pack's regression tests as well.
- Provide files changed, settings changed, known limitations, and exact manual test steps.

## Engineering standards

- Follow the project's current Unity version and coding conventions.
- Use serialized/configurable tuning parameters rather than unexplained magic numbers.
- Use one authoritative owner for each input and each player velocity modification.
- Use `FixedUpdate`/physics-step logic for forces and Rigidbody movement; use `Update` for input collection and UI, with buffering when required.
- Avoid allocating garbage every physics frame (LINQ, repeated GetComponent, new collections, string concatenation).
- Cache references and validate missing dependencies.
- Handle disable/destroy/respawn correctly: release held objects, restore ignored collisions, release projectile ownership, stop time dilation, and clear events.
- Use layers and collision masks explicitly; do not rely on object names.
- Keep physics interactions deterministic enough to test; do not use `transform.position` teleporting for dynamic objects every frame.
- Do not silently change player controls, enemy balance, or damage tuning.
- Do not claim tests passed unless they were actually run.

## Required final report

When finished, report:

1. What was inspected and what movement-pack work was already present.
2. The exact systems implemented and their behavior.
3. Files added/modified and why.
4. Any interface migration or adapter.
5. All changed serialized values, layers, collision matrix entries, and Input Actions.
6. Focus time-domain implementation and measured player/world behavior.
7. Object classes and actual tuned values.
8. Tests run, results, and tests not run.
9. Known limitations and remaining decisions.
10. A short playtest route for Push, Pull, Crate Cannon, Focus, and enemy interaction.

Do not report success for untested features. A compilable partial implementation with clearly identified remaining tests is better than claiming a complete result without evidence.

---

# FILE: 02_SCOPE_AND_DESIGN_BRIEF.md

# REBOUND — Scope and Gameplay Design Brief

## 1. Product objective

Improve REBOUND's existing combat interaction systems so that the player can fight continuously while maintaining high-speed movement. Combat should use the existing pneumatic abilities and physical objects rather than introducing conventional guns or a separate stop-and-shoot mode.

The movement upgrade is handled by the previous resource pack. This pack makes the combat abilities and physical objects work correctly with that movement system.

## 2. Core player combat loop

1. The player moves through the encounter using the upgraded locomotion system.
2. The player identifies an enemy, incoming projectile, or useful physical object.
3. The player uses Push, Pull, grapple traversal, or Focus without losing control of movement.
4. The player grabs, drags, shields with, or launches an object when useful.
5. The resulting impact, deflection, or enemy opening gives the player feedback and an opportunity to continue moving.
6. The player enters the next action without unnecessary state locks.

The loop must remain readable at high speed. Reticle state, target highlights, audio cues, and impact feedback must tell the player what happened.

## 3. Keep these existing systems

- First-person high-speed player movement and momentum.
- Wind Cannon / Push.
- Dual-mode Vacuum Tether / Pull (object manipulation and environment grapple traversal).
- Crate Cannon.
- Focus slow motion.
- Existing physical props and TargetButton interactions.
- Existing Spider Sense / threat warning, if present.
- Existing enemy roster: swarmers, Hulk Leaper, sniper, rapid sentry, plus laser tripwire hazard.
- Existing health, damage, AI, attack telegraphs, and encounter logic.

## 4. What changes

### Push
- Improve target eligibility, impact consistency, projectile deflection, feedback, and safe integration with movement recoil.
- Keep Push responsive and usable without requiring the player to stop.

### Pull
- Improve target selection and object mode.
- Make light/medium carrying stable and heavy-object dragging reliable.
- Make held objects useful as physical projectile cover.
- Keep object manipulation separate from the existing grapple traversal mode so they do not conflict.

### Objects
- Make light, medium, and heavy objects meaningfully different.
- Make mass, pull response, launch eligibility, impact potential, shield role, and durability configurable.
- Give objects deliberate combat roles without requiring map redesign.

### Crate Cannon
- Make launch behavior reliable and consistent with player momentum.
- Improve physical impact calculation, weak-point recognition, and feedback.

### Focus
- Move from infinite use toward a rechargeable tactical resource.
- Make world time dilation consistent and player control demonstrably responsive.

### Feedback
- Improve target highlight, reticle prompts, impact sounds, projectile deflection cues, Focus meter, and small optional chain feedback.

## 5. What must not change

- Do not change or replace the enemy roster.
- Do not redesign enemy AI, attack timing, movement, or telegraphs.
- Do not create new map layouts or environment art.
- Do not implement Egyptian-themed objects or level content as a task requirement. Generic physical props are sufficient; theme-specific art is another teammate's responsibility.
- Do not duplicate the movement work from the previous resource pack.
- Do not add a conventional gun, melee weapon, inventory, crafting, or loot system.

## 6. Combat principles

### Multiple valid solutions
Every enemy encounter must retain at least one reliable solution that does not depend on finding a particular movable object. Physical objects add options; they must not become the only way to progress.

### Objects have roles
A light object is not merely a small medium object. Weight class and traits should change how it handles, what it can block, how it impacts, and whether it can be carried.

### Movement remains active
Do not lock the player in a long object-throw animation or disable movement while aiming. Any brief input lock must be justified by existing design and tested.

### Clear target intent
When RMB is pressed, the reticle must communicate whether the player is about to grab an object/enemy or grapple an environmental anchor. No silent or surprising mode switching.

### Impact should be physical and bounded
Impact speed and mass should matter, but damage must be capped and tuned. Avoid one small prop instantly killing every enemy or repeated collision callbacks multiplying damage.

### Focus is tactical, not mandatory
Focus should make precision easier, but ordinary movement, dodging, and projectile reactions must remain viable without it.

## 7. Priority order

**P0 — Reliability:** no input duplication, no stuck held props, no broken timeScale, no compile/interface mismatch, no velocity overwrite conflict.  
**P1 — Core gameplay:** stable Pull, object roles, Crate Cannon, Push recoil and deflection, Focus time behavior.  
**P2 — Feedback:** reticle/highlight, audio, VFX, shield feedback, impact cues.  
**P3 — Optional polish:** simple action-chain/Flow feedback and a small set of special object traits, only after P0–P2 are stable.

---

# FILE: 03_PUSH_WIND_CANNON_SPEC.md

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

---

# FILE: 04_PULL_AND_OBJECT_MANIPULATION_SPEC.md

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

---

# FILE: 05_OBJECT_PHYSICS_AND_COMBAT_SPEC.md

# REBOUND — Object Classes, Physics and Physical Combat Specification

## 1. Objective

Create a small, extensible physical-object system in which each object has a clear combat and interaction purpose. The system should support the existing Pull, Push, Crate Cannon, shielding, TargetButton, and enemy impact interactions.

Do not build a new inventory/crafting system. Do not create new levels or map content. Do not introduce a large set of object types.

## 2. Core object classes

The existing handoff provides three mass references. Verify actual prefab masses and retain them as starting values unless playtesting demonstrates a problem.

| Class | Existing reference mass | Pull behavior | Launch behavior | Main gameplay roles |
|---|---:|---|---|---|
| Light | ~1 kg | Fast lift/hold | Crate Cannon eligible | Quick projectile, small enemy hits, rapid interaction |
| Medium | ~5 kg | Normal lift/hold | Crate Cannon eligible | Main projectile, shield, exposed weak-point attack |
| Heavy | ~25 kg | Ground drag only | Not normal Crate Cannon ammunition | Cover, pressure weight, movable obstruction, ground-impact tool |

Mass is not the only property defining a class. Use a serialized profile or the project's current data-driven pattern.

## 3. Object profile fields

Use only fields that are useful to the current project. A suggested profile includes:

### Identity and capability
- Weight class: Light / Medium / Heavy.
- Can be pulled.
- Can be held.
- Can be ground-dragged.
- Can be launched by Crate Cannon.
- Can block projectiles.
- Can activate physical buttons/targets.
- Can be broken (optional).

### Physical tuning
- Rigidbody mass.
- Linear drag/damping.
- Angular drag/damping.
- Pull spring strength.
- Pull damping.
- Maximum pull force/acceleration.
- Maximum angular stabilization.
- Launch speed modifier.
- Player velocity inheritance modifier.
- Collision detection mode.
- Physics material/friction profile where required.

### Combat tuning
- Impact damage multiplier.
- Knockback multiplier.
- Minimum impact threshold.
- Maximum damage per impact.
- Weak-point multiplier is applied by the target, not permanently stored on the prop.
- Shield durability/resistance if implemented.

### Presentation
- Highlight color/state.
- Hold label.
- Launch/impact sound and VFX references.
- Break effect if applicable.

Avoid putting every possible value on every prefab if ScriptableObject profiles would be more maintainable. Follow the existing project's data architecture and do not introduce an elaborate asset pipeline unnecessarily.

## 4. Different physical roles

### Light
- Lowest inertia and easiest to pull.
- Fastest response to the Pull spring.
- Intended for quick, reactive throws.
- Lower impact energy at the same speed because of lower mass.
- May hit multiple light enemies if physics naturally allows; do not force penetration through all targets.
- Not a reliable long-duration shield unless current balance says otherwise.

### Medium
- Main all-purpose portable object.
- Strong enough to make the Crate Cannon a meaningful attack.
- Can be carried as a temporary shield against supported projectiles.
- Good candidate for attacking the Hulk's existing exposed weak point.
- Heavier/slower handling than Light.

### Heavy
- Cannot be lifted or held by the normal object Pull mode.
- Pull applies horizontal ground force toward a projected target point.
- Can block sightlines and projectiles when physically positioned as cover.
- Can be pushed or nudged, but its response should reflect its mass.
- Can interact with pressure plates/physical switches when the level's existing objects support it.
- Not launched by normal Crate Cannon.

## 5. Optional object traits

Only add these after the three core classes are stable and tested.

- **Fragile:** Breaks when impact energy exceeds a configurable threshold; may spawn a small, pooled set of fragments. Fragment count must be limited for performance.
- **Dense/Reinforced:** Higher impact multiplier or shield resistance, but also heavier/slower to pull.
- **Bouncy:** Higher restitution for ricochet-oriented puzzles or combat, only if a current use case exists.

Do not add explosive, elemental, crafting, or special ammunition systems as part of this pack unless already present in the project.

## 6. Physical impact calculation

Impact should be based on the object's relative velocity against the target, not just the object's world speed.

Let:
- `m` = projectile Rigidbody mass in kg.
- `vRel` = projectile velocity relative to target velocity in m/s.
- `n` = normalized collision contact normal.
- `vNormal = abs(dot(vRel, n))` = normal component of relative impact speed in m/s.
- `Eimpact = 0.5 * m * vNormal * vNormal` = normal impact energy in joules.

This is a useful impact-energy estimate, not a full material/fracture solver. It avoids treating a very glancing contact as a full head-on impact.

Suggested configurable damage form:

`damage = clamp((Eimpact - minimumImpactEnergy) * damagePerJoule * objectImpactMultiplier, 0, maxDamagePerImpact)`

Then apply the target's weak-point multiplier if the contact point intersects an explicitly configured weak-point collider/component.

Requirements:
- Keep damage thresholds, coefficient, multiplier, and cap in configurable data.
- Use the target's relative velocity if both bodies move.
- Use one collision impact event per target/projectile contact event.
- Do not apply damage from `OnCollisionStay` every physics frame.
- Maintain a per-projectile set or short cooldown of targets already hit so one contact cannot produce duplicate damage through multiple colliders.
- Deduplicate multiple child colliders belonging to one enemy.
- Consider whether the object should bounce, slow, break, or continue based on actual Unity physics and its profile.
- Do not guarantee that a projectile passes through multiple enemies; penetration is an optional trait/behavior, not a default assumption.

## 7. Collision and high-speed reliability

- Use an appropriate continuous collision mode for fast-moving Crate Cannon projectiles. `ContinuousDynamic` may be suitable for dynamic projectiles, but inspect performance and Unity version limitations.
- Ensure targets that must receive fast collisions have compatible collision detection settings.
- Check collider thickness and scale in existing prefabs; do not modify map geometry in this task.
- Use Rigidbody/Collider physics for actual object motion. Do not move dynamic objects by assigning Transform position every frame.
- Prevent duplicate callbacks caused by compound colliders.
- Use collision layers and masks; do not rely on GameObject names.
- Pool fragments/effects if breakage is implemented.

## 8. Shield durability and projectile interaction

If shield durability is added:
- It is stored on the object or its profile.
- It is reduced only by projectiles that are currently designed to be blocked.
- A blocked projectile produces one block event.
- When durability reaches zero, the object may break, become unusable as a shield, or remain as damaged cover depending on its profile.
- A medium object should provide meaningful but not unlimited protection unless intentionally balanced that way.

Do not make a shield absorb bullets through walls or when it is not physically between the player and the projectile.

## 9. TargetButton and physical switches

Preserve existing TargetButton behavior and event hookups. It should continue to support the project's current activation routes, including:
- Crate Cannon impact above its configured threshold.
- Player slide/ram above its configured threshold.
- Direct Push blast.

The exact method names and threshold values must be taken from current code, not copied blindly from an older handoff. Do not award Focus repeatedly for multiple colliders or repeated contact with the same already-activated button.

## 10. Performance and cleanup

- Cache components and profile references.
- Do not search the scene for all objects every frame.
- Avoid per-FixedUpdate allocations.
- Keep active physical debris limited.
- If a prop despawns or breaks while held, safely clear the player's held reference.
- On respawn/scene unload, restore all altered collision settings and object simulation settings.

## 11. Acceptance criteria

- Light, medium, and heavy objects behave differently in a visible and useful way.
- The player can immediately tell which objects are portable and which are ground-drag only.
- Thrown-object impact strength responds to both mass and relative impact speed.
- Damage is bounded and applied once per impact.
- Weak-point hits are based on actual configured colliders/components.
- Heavy objects are not accidentally lifted or launched by the normal Crate Cannon.
- Physical shielding works only when the object actually intercepts the projectile.
- The system performs consistently at the game's target frame rate.

---

# FILE: 06_CRATE_CANNON_SPEC.md

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

---

# FILE: 07_FOCUS_TIME_DILATION_SPEC.md

# REBOUND — Focus / Slow-Motion Technical Specification

## 1. Design objective

Focus is a tactical ability that lets the player react to difficult attacks, line up object throws, and perform precise projectile deflections. It should be usable frequently, but not infinitely without cost.

The proposed direction is a rechargeable resource rather than unlimited slow motion.

Focus must not become mandatory for normal movement or basic enemy survival.

## 2. Desired player experience

When Focus is active:

- Enemy movement and enemy attack progression slow down.
- Eligible enemy projectiles slow down.
- Movable props and launched physical objects slow down consistently.
- Player input, camera look, reticle, UI, and Focus meter remain responsive in real time.
- The player's practical movement control should remain as close to normal as the existing physics architecture safely allows.
- Push and Pull remain usable.
- The world resumes normal simulation smoothly when Focus ends.

## 3. Meter design

Use configurable values. Suggested initial playtest values (not final balance):

| Parameter | Starting proposal | Units |
|---|---:|---|
| Maximum Focus | 100 | units |
| Active drain | 25 | units per unscaled second |
| Recovery delay | 0.75 | unscaled seconds |
| Recovery rate | 15 | units per unscaled second |
| Minimum activation amount | 5 | units |
| Successful projectile deflection reward | +15 | units |
| Successful weak-point impact reward | +10 | units |
| Meaningful objective/TargetButton reward | Use current behavior; suggested +15 to +25 | units |

These are a first tuning profile only. Expose them in the Inspector or a configuration asset. Playtest and adjust.

### Reward rules

Reward only confirmed meaningful actions:
- An eligible hostile projectile is successfully deflected and the deflection resolves.
- A launched prop makes a meaningful enemy/weak-point hit.
- A target/button/objective successfully activates.

Anti-farming rules:
- A single projectile rewards Focus only once.
- A single enemy/weak-point impact rewards once per impact event.
- A single already-activated TargetButton does not refill Focus repeatedly.
- Repeated hits on an already disabled target do not reward energy.
- Do not reward merely firing Push, pulling and releasing a prop, or hitting an empty wall.

If the current game already gives a full Focus refill from TargetButton, do not remove it without a playtest rationale; make it an explicit configurable reward and ensure it can trigger only once per activation.

## 4. Input and controls

The documentation conflicts on the Focus binding:
- The newer player subsystem describes Q/F toggle.
- The older team handoff also lists Left Shift in one control table.

The actual Unity Input Actions asset and current `PlayerAbilityInput` are authoritative. Preserve the binding already in use. Do not reassign Shift if it is now used by the movement upgrade. Update stale documentation after confirming the actual binding.

Use the current toggle or hold interaction as implemented. Do not change it casually.

## 5. Unity time-dilation warning — critical

Unity's global `Time.timeScale` affects the entire simulation, including Rigidbody physics. Using `Time.unscaledDeltaTime` for mouse input does **not** by itself keep a Rigidbody player moving at normal real-time speed.

The agent must inspect how player movement and world objects are simulated before implementing Focus.

The existing specification references:
- World time scale: 0.25.
- `Time.fixedDeltaTime = baseFixedDeltaTime * timeScale`.
- Input, camera, and audio responsiveness using unscaled time.

Those settings slow normal Unity physics for the player as well as the world. Therefore the following behavior must be measured rather than assumed:

**Required target:** enemy/world simulation runs at the Focus scale, while player input/aim remains responsive and practical player movement remains near its intended real-time speed, without unstable collision behavior.

### Implementation decision process

1. Identify whether the player is a fully dynamic Rigidbody, a custom motor, or a hybrid controller.
2. Identify whether enemy/projectile/prop movement relies on Rigidbody physics, `Update`, `FixedUpdate`, Animator, NavMesh, or custom simulation.
3. Choose a time-domain design that the current project can actually support.
4. Do not blindly multiply every player velocity/force by `1 / timeScale`. That can create excessive collision velocities and incorrect impulses.
5. Do not claim the player is at 1x speed merely because camera input uses unscaled time.
6. If using global `Time.timeScale`, test player displacement, gravity, landing, collisions with slowed props/enemies, Push recoil, grapple constraints, Crate Cannon recoil, and respawn under Focus.
7. If the current dynamic Rigidbody architecture cannot provide the desired player/world separation safely with a minimal change, explain the exact limitation and implement the safest tested compromise. Do not hide the compromise.

Potential approaches to evaluate (not preselected):
- A current custom player movement motor that can explicitly integrate its own time domain.
- A carefully controlled player Rigidbody compensation layer with collision and impulse tests.
- A broader local simulation-time abstraction for enemy/projectile/prop systems, if the project architecture makes this practical.

Do not build a complicated second physics engine unless there is no reasonable alternative. Keep the solution compatible with the existing movement work.

## 6. Time domains

At minimum, classify systems:

| System | Desired Focus behavior | Time source to evaluate |
|---|---|---|
| Player input sampling | Normal responsiveness | Unscaled/input event time |
| Camera look | Normal responsiveness | Unscaled time |
| Focus drain/recovery | Normal real time | Unscaled time |
| HUD and reticle | Normal responsiveness | Unscaled time |
| Player locomotion | Near-normal practical movement | Must be explicitly implemented/tested |
| Player gravity/impulses | Preserve coherent movement | Must be explicitly implemented/tested |
| Enemy locomotion/AI | Slowed | Scaled/local simulation time |
| Enemy attack telegraphs | Slowed consistently | Scaled/local simulation time |
| Enemy projectiles | Slowed | Scaled/local simulation time |
| Movable props | Slowed | Scaled/local simulation time |
| Crate Cannon projectile | Slowed consistently | Scaled/local simulation time |
| Static UI/menus | Normal | Unscaled time |
| Audio | Intended slowed-world effect without pitch glitches | Audio-specific time/pitch handling |

## 7. Enter/exit safety

- Cache the project's original `Time.timeScale` and `Time.fixedDeltaTime` values.
- Restore the exact prior values when Focus ends, the player dies, the scene unloads, the application pauses, or the Focus component is disabled.
- Avoid repeatedly multiplying an already scaled fixed timestep. Always derive it from a cached base value.
- Ensure two Focus sources cannot stack into 0.25 × 0.25 unintentionally.
- Prevent Focus from remaining active after respawn or menu pause unless that is an explicit design.
- Do not leave projectiles or objects with stale local time multipliers after Focus ends.
- Preserve UI input and cursor behavior.

## 8. Meter feedback

HUD should show:
- Current Focus amount.
- Whether Focus is active.
- Whether it is draining or recovering.
- A clear low-energy state.
- A readable activation cue.

Audio/visual feedback should communicate entry, active state, and exit without obscuring target visibility. Preserve existing procedural Focus sound if present.

## 9. Acceptance criteria

- Focus drains at the configured real-time rate, independent of timeScale.
- Focus cannot activate below the minimum amount.
- Focus regenerates after the configured delay.
- Valid skill events reward once and cannot be farmed.
- Enemies/projectiles/props slow consistently.
- Camera/input/UI remain responsive.
- Player movement, gravity, collision, and impulses under Focus are measured and documented.
- Exiting Focus restores timeScale and fixedDeltaTime exactly.
- Death, disable, pause, and scene transitions cannot leave the game permanently slowed.

---

# FILE: 08_ENEMY_INTEGRATION_AND_CONTRACTS.md

# REBOUND — Enemy Integration and Physics Interaction Contracts

## 1. Scope boundary

The enemy roster and enemy behavior are not being redesigned in this task. Preserve the existing enemy AI, navigation, attack timing, telegraphs, health values, and encounter logic.

The task is to make existing enemies respond correctly to the player's Push, Pull (where currently allowed), Crate Cannon, slide ram, and projectile deflection systems.

## 2. Existing enemy/hazard inventory to preserve

### Small fast Swarmer
Keep its existing pack behavior, movement, leap/attack, and health. Integrate only the currently intended physical responses:
- Eligible Push knockback or interruption.
- Slide ram/bowling response.
- Crate Cannon impact damage.
- Pull only if the current enemy is explicitly configured as pullable/lightweight.

### Heavy Hulk Leaper
Keep its current leap, predicted landing, slam, shockwave, and stun recovery. Preserve its heavy/resistant status:
- Do not allow ordinary Pull to lift it.
- Do not allow ordinary Push to move it substantially unless current design already permits a small stagger.
- Keep armor/body resistance.
- Apply the existing weak-point multiplier only when the exposed weak-point collider is actually hit.
- Medium Crate Cannon projectile remains the intended strong weak-point attack if that is already configured.

### Sniper
Keep current aiming, laser lock, charge time, firing cadence, damage, and placement behavior. The player's new interaction is:
- Eligible sniper projectile can be deflected by Push during the valid window.
- Reflected projectile should be attributed so it can damage the original sniper.
- Do not make every sniper attack automatically return to sender.

### Rapid-Laser Sentry
Keep existing burst length, firing cadence, target tracking, and damage. New/verified interactions:
- Eligible bolts collide with a held object before player when the object physically intercepts the shot.
- Preserve the existing flanking and Crate Cannon damage routes.
- Do not alter sentry AI or fire pattern.

### Laser Tripwire hazard
Preserve activation and damage behavior. Do not redesign its placement or geometry. Focus affects a moving tripwire only if its current implementation belongs to a time-scaled simulation system; test and document.

## 3. Contract version mismatch — mandatory audit

The supplied documentation contains two different `IKineticTarget` shapes.

### Newer player subsystem specification

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

### Older team handoff

```csharp
public interface IKineticTarget
{
    bool CanBePulled { get; }
    bool CanBePushed { get; }
    void OnPushed(Vector3 forceDirection, float forceMagnitude, Vector3 hitPoint);
    void OnPulled(Transform pullOrigin, float pullForce);
    void OnPhysicsCrush(float impactSpeed, GameObject flyingProp, bool hitWeakPoint);
    void OnPlayerRam(float playerSpeed, Vector3 ramDirection);
}
```

These signatures are not interchangeable. The actual current repository is authoritative.

The agent must:
1. Locate the real interface.
2. Find all implementers, callers, and test doubles.
3. Choose one consistent contract or keep the existing contract and add an adapter/context object.
4. Update all implementations and call sites atomically if migrating.
5. Compile the project and verify every receiver.
6. Do not leave a mix of old and new signatures.

## 4. Recommended impact context (optional modernization)

If the current interface is too limited for the required damage and weak-point work, consider a small immutable context structure rather than adding many positional parameters:

```csharp
public struct KineticImpactContext
{
    public GameObject SourceObject;
    public Vector3 Point;
    public Vector3 Normal;
    public Vector3 Direction;
    public float RelativeImpactSpeed;
    public float ImpactEnergy;
    public bool HitWeakPoint;
}
```

A possible receiver method is:

```csharp
void OnKineticImpact(in KineticImpactContext context);
```

This is a design option, not a mandate. Use it only if it simplifies the actual project. If adopted, migrate all callers and implementers in one compile-safe change and preserve adapters for existing gameplay events.

## 5. Weak-point detection

- Do not detect weak points by GameObject name strings.
- Use an explicit component, serialized collider reference, or dedicated layer as the project's architecture permits.
- Determine weak-point status from the actual contact collider/contact point.
- Apply the multiplier once.
- If the weak point is not exposed/active according to the current Hulk state, do not apply the weak-point bonus.
- Keep the existing enemy vulnerability window and stun state unchanged.

## 6. Projectile deflection contract

If current projectile scripts do not expose a safe method, add a small interface or adapter such as:

```csharp
public interface IDeflectableProjectile
{
    bool CanBeDeflected { get; }
    bool TryDeflect(Vector3 newDirection, GameObject newOwner);
}
```

The actual signature may follow project style. Requirements:
- One successful deflection only by default.
- Update owner/faction/team so reflected shots can hit their former source.
- Prevent immediate self-hit and infinite reflection.
- Preserve projectile damage and collision behavior unless current rules require a change.
- Do not deflect non-projectile hazards or unapproved attacks.

## 7. Enemy damage and status rules

- Do not create a parallel enemy health system.
- Reuse the existing damage receiver/health component.
- Apply impact damage through one authoritative path.
- Do not apply both `TakeDamage` and an interface callback that internally calls `TakeDamage` for the same collision.
- Physical push/knockback and damage should be separate explicit outputs, each applied once.
- Preserve immunities and heavy-target resistance.
- Do not allow ordinary props to bypass intended boss/armor states without weak-point eligibility.

## 8. Player ram / slide collision

The old and new documentation also disagree on the player-ram callback name and whether a direction vector is included.

Use the current real project signature. Ensure:
- One collision produces one ram event.
- The current slide-speed threshold is read from one authoritative setting.
- Light enemies can receive the intended ram response.
- Heavy Hulk behavior remains its existing special rule (player may be stopped/penalized as currently designed).
- The player's movement controller owns any velocity loss or collision response; enemy receivers should not independently overwrite player Rigidbody velocity.

## 9. Acceptance criteria

- Enemy AI and attack behavior remain unchanged.
- Each enemy receives only the physical interactions configured for its archetype.
- Weak-point hits are detected by collider/component, not names.
- Projectile reflection updates ownership and cannot loop forever.
- Interface signatures compile across all implementers and callers.
- No duplicate damage or duplicate impulses occur from compound colliders.

---

# FILE: 09_UNITY_ARCHITECTURE_AND_MIGRATION.md

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

---

# FILE: 10_VALIDATION_AND_PLAYTEST_SUITE.md

# REBOUND — Validation and Playtest Suite

## 1. Purpose

Use this suite to verify that the Push, Pull, object, Crate Cannon, Focus, and enemy-integration upgrades work in the actual Unity game and do not regress the movement upgrade.

Do not mark a test passed unless it was run. Record Unity version, target frame rate, physics timestep, Focus state, object class, and relevant starting velocity for numerical tests.

## 2. Test recording template

For every test, record:
- Test ID and build/commit.
- Unity version.
- Scene/test area (existing test area only; no map redesign required).
- Player starting position and velocity.
- Target object/enemy class and Rigidbody values.
- Input sequence.
- Focus active/inactive.
- Expected result.
- Actual result.
- Pass/fail and notes.
- Video/screenshot/log if useful.

## 3. Push tests

| ID | Setup | Action | Expected result |
|---|---|---|---|
| P01 | Eligible light prop within range and visible | Press Push once | Prop receives one outward impulse |
| P02 | Medium prop | Press Push | Response is stronger/more substantial according to its profile, not duplicated |
| P03 | Heavy object | Press Push | It moves/nudges according to its mass and allowed capability; it is not treated like a light prop |
| P04 | Target behind solid wall | Press Push | Target is not hit through wall |
| P05 | Enemy with `CanBePushed=false` | Press Push | No unintended push/damage |
| P06 | Player in open air outside recoil condition | Press Push | No ordinary surface recoil |
| P07 | Valid nearby surface | Press Push | Existing recoil occurs once with correct direction |
| P08 | Player power-sliding | Press Push | Existing forward slide boost occurs; no conflicting backward kick |
| P09 | Already used airborne rocket jump | Push downward again | No unlimited second rocket jump if current rule restricts it |
| P10 | Push hits multiple child colliders on one enemy | Press Push | One logical target reaction |

## 4. Projectile deflection tests

| ID | Setup | Action | Expected result |
|---|---|---|---|
| D01 | Eligible sniper projectile approaching player in valid window | Press Push | Projectile deflects toward its source |
| D02 | Eligible rapid bolt approaching | Press Push | Only if current design marks it deflectable; correct reflected behavior |
| D03 | Non-deflectable projectile | Press Push | No reflection |
| D04 | Projectile outside detection window | Press Push | No impossible long-range reflection |
| D05 | Projectile behind player | Press Push | No forward-cone deflection unless design explicitly supports it |
| D06 | Reflected projectile hits original shooter | Observe | Correct enemy damage/ownership |
| D07 | Reflected projectile contacts player or held prop immediately | Observe | No self-hit loop or repeated reflection |
| D08 | Multiple projectiles near player | Press Push | Deterministic selected candidate/defined group; no uncontrolled scene-wide reflection |
| D09 | Focus active/inactive | Repeat D01 | Timing behavior is consistent and documented |

## 5. Pull and object-mode tests

| ID | Setup | Action | Expected result |
|---|---|---|---|
| O01 | Light prop within 16 m (or current configured range) | Aim and Pull | Prop selected and approaches smoothly |
| O02 | Medium prop | Pull/hold | Stable hold; no uncontrolled spinning/jitter |
| O03 | Heavy prop on floor | Pull | Ground drag only; no lift/hover |
| O04 | Heavy prop not on floor/blocked | Pull | Safe failure/limited force, no teleport through geometry |
| O05 | Player standing on light/medium prop | Pull | Anti-box-riding rule prevents self-lift exploit |
| O06 | Held prop and narrow doorway | Move through doorway | Prop retracts smoothly or stops safely without clipping |
| O07 | Held prop and clear open space | Move/turn quickly | Hold follows without large oscillation |
| O08 | Held prop between projectile and player | Incoming eligible projectile | Prop physically blocks it |
| O09 | Held prop not between projectile and player | Incoming projectile | Player is not protected through the prop |
| O10 | Held prop destroyed | Observe | Held reference clears; no null errors |
| O11 | Player respawns while holding prop | Respawn | Object/collision/layer state restored |
| O12 | Valid object and grapple anchor both in view | Press Pull | Predictable target priority and correct reticle label |
| O13 | Only anchor in view | Pull | Existing traversal grapple works unchanged |
| O14 | Object target behind wall | Pull | No pull through geometry |
| O15 | Optional pullable light enemy (if supported) | Pull/release | Existing AI recovers; no permanent held state |

## 6. Object class tests

| ID | Setup | Test | Expected result |
|---|---|---|---|
| C01 | Light prop | Pull speed and launch | Fast response, lower impact potential |
| C02 | Medium prop | Pull, shield, launch | Balanced all-purpose role |
| C03 | Heavy prop | Pull and push | Ground movement only; not portable/launchable |
| C04 | Same speed, different masses | Impact same test target | Impact calculation responds to mass |
| C05 | Same object, different impact speeds | Hit target | Damage responds to relative impact speed/energy |
| C06 | Glancing versus head-on impact | Hit target | Normal impact component produces sensible difference |
| C07 | Compound target collider | Hit once | One logical damage event |
| C08 | Repeated collision frames | Keep object in contact | No repeated damage each FixedUpdate |
| C09 | Weak point inactive | Hit body | No weak-point multiplier |
| C10 | Weak point active and directly struck | Hit weak point | Correct multiplier once |
| C11 | Breakable trait (only if implemented) | High-energy hit | Breaks once; debris count is bounded |

## 7. Crate Cannon tests

| ID | Setup | Action | Expected result |
|---|---|---|---|
| K01 | Holding Light | LMB | One launch; normal Push suppressed |
| K02 | Holding Medium | LMB | One launch with correct class behavior |
| K03 | Heavy drag object | LMB | No normal Crate Cannon launch |
| K04 | Player stationary | Fire | Launch speed matches documented definition |
| K05 | Player moving forward | Fire | Configured player velocity inheritance occurs |
| K06 | Player moving sideways | Fire | Inherited velocity behaves as documented |
| K07 | Player sliding | Fire | Movement momentum remains coherent; recoil integrated once |
| K08 | Object predicted against wall | Aim | Trajectory line indicates collision/does not pass through geometry if supported |
| K09 | Object launched under Focus | Observe | Prediction and actual projectile use same time-domain assumptions |
| K10 | Object hits ordinary enemy | Impact | Correct damage/knockback once |
| K11 | Medium object hits exposed Hulk weak point | Impact | Weak-point multiplier applied only on actual collider hit |
| K12 | Object hits TargetButton | Impact | Existing button activation occurs once |
| K13 | Fast projectile through thin collider | Impact | No tunneling at target frame rate |
| K14 | Player respawns before launch completion | Observe | No held-object/ownership leak |

## 8. Focus tests

| ID | Setup | Action | Expected result |
|---|---|---|---|
| F01 | Full meter | Activate Focus | World slows to configured scale |
| F02 | Focus active | Move/look/aim | Camera/input responsive; player displacement measured |
| F03 | Focus active | Observe enemy | Enemy movement/AI/telegraph slow consistently |
| F04 | Focus active | Observe projectile | Projectile slows consistently |
| F05 | Focus active | Hold object | Object physics/hold spring stable |
| F06 | Focus active | Fire Crate Cannon | Actual flight and trajectory prediction agree |
| F07 | Focus active | Use Push recoil | Player impulse behaves consistently; no instability |
| F08 | Focus active | Use grapple | Tether/movement remains stable and no solver explosion |
| F09 | Keep Focus active until empty | Observe | Meter drains at unscaled rate and exits correctly |
| F10 | Focus off after use | Observe | Meter recovers after configured delay |
| F11 | Repeat successful deflection on same projectile | Observe meter | Only one Focus reward |
| F12 | Repeatedly hit same active button | Observe meter | No repeated farming reward |
| F13 | Die while Focus active | Respawn | TimeScale/fixedDeltaTime restored |
| F14 | Open pause/menu while Focus active | Resume | Time restored or paused intentionally, no broken scale |
| F15 | Disable Focus component/scene unload | Observe | Global time values restored |
| F16 | Toggle Focus rapidly | Repeat | No scale stacking or fixedDeltaTime drift |

For F02, compare real-time displacement (world units per actual wall-clock second) with Focus off. Camera responsiveness alone is not a pass.

## 9. Enemy behavior regression tests

| ID | Enemy | Required preservation |
|---|---|---|
| E01 | Swarmer | Existing movement, leap, and attack unchanged; intended ram/prop responses work |
| E02 | Hulk | Existing leap, slam, shockwave, stun, and armor behavior unchanged |
| E03 | Hulk weak point | Existing vulnerability timing preserved; correct prop impact applies damage |
| E04 | Sniper | Existing laser lock, charge, shot timing, and damage unchanged |
| E05 | Sniper deflection | Eligible shot can return to source without changing sniper AI |
| E06 | Rapid sentry | Existing burst/fire pattern unchanged |
| E07 | Rapid sentry shield | Held prop blocks only when physically in shot path |
| E08 | Tripwire | Existing activation/damage behavior unchanged |

## 10. Previous movement-pack regression tests

Run the prior pack's tests for:
- Ground acceleration and steering.
- Air control/bunny hop.
- Slide entry, slope behavior, and slide exit.
- Grapple attach, swing, release, and redirection.
- Wall interactions if implemented.
- Dash/ability chaining if implemented.
- Collision at high speed.

Combat integration must not regress these behaviors.

## 11. Performance checks

- No per-frame allocations from target scans or impact handling.
- No unbounded fragment spawning.
- No scene-wide projectile scans every Update if a registry/layer overlap query is available.
- No multiple active audio sources/loops after release or respawn.
- No lingering ignored collision pairs.
- Profile physics CPU at the target frame rate with multiple props and projectiles active.

## 12. Definition of tested

A test is complete only when the agent records actual results. “Implemented” is not equivalent to “tested.” Record any untested manual test explicitly in the final handoff.

---

# FILE: 11_PARAMETER_LEDGER_AND_CONFLICTS.md

# REBOUND — Parameter Ledger and Documentation Conflicts

## 1. Purpose

This ledger separates existing REBOUND reference values from proposed tuning values and unresolved conflicts. The coding agent must inspect current Unity code/prefabs before changing any value.

**Do not blindly copy a number from this document into code.** The actual project is the source of truth.

## 2. Existing REBOUND references from the supplied player subsystem

| System | Reference value | Units | Status / notes |
|---|---:|---|---|
| Player Rigidbody mass | 75 | kg | Existing rig specification; verify prefab |
| Push range | 12 | m | Existing reference |
| Push target impulse | 18 | N·s | Existing reference; ensure API uses impulse units |
| Push surface proximity | 3.8 | m | Existing reference |
| Push kick velocity | 13.5 | m/s | Existing reference formula |
| Slide Push boost | +10 | m/s | Existing reference; movement owner must apply |
| Air rocket jump limit | 1 per airborne window | count | Existing reference |
| Object acquisition range | 16 | m | Existing reference |
| Grapple traversal range | 75 | m | Movement pack reference; not redesigned here |
| Normal held-object distance | 2.5 | m | Existing reference |
| Narrow-space held distance | 1.2 | m | Older team handoff reference |
| Light prop mass | ~1 | kg | Existing reference |
| Medium prop mass | ~5 | kg | Existing reference |
| Heavy prop mass | ~25 | kg | Existing reference; ground drag only |
| Crate Cannon launch component | 35 | m/s | Existing reference; definition of total vs additive velocity must be audited |
| Crate Cannon player recoil | 6.5 | m/s | Existing reference |
| Trajectory prediction segments | 20 | points/steps | Existing reference |
| TargetButton crate impact threshold | 4 | m/s | Older handoff; verify current code |
| Slide/ram activation threshold | 5.5 or 7.5 | m/s | Conflicting docs; current code is authoritative |
| Focus time scale | 0.25 | ratio | Existing reference |
| Focus resource | Infinite | — | Existing design; proposed to change to rechargeable |

## 3. Suggested Focus starting profile

These are provisional playtest values, not final balance:

| Parameter | Proposed start | Units |
|---|---:|---|
| Capacity | 100 | units |
| Drain | 25 | units / unscaled second |
| Recovery delay | 0.75 | unscaled seconds |
| Recovery | 15 | units / unscaled second |
| Minimum activation | 5 | units |
| Projectile deflection reward | +15 | units |
| Weak-point impact reward | +10 | units |
| Objective reward | +15 to +25 | units |

Tune based on the length of actual encounters and observed average time spent in Focus. Ensure a player can use Focus multiple times in a normal encounter without making it effectively unlimited.

## 4. Suggested object-profile tuning fields

Do not use unverified fixed force constants. Expose:
- Spring strength.
- Spring damping.
- Maximum pull force/acceleration.
- Hold-point offset/distance.
- Maximum angular stabilization.
- Ground drag force.
- Launch eligibility.
- Launch speed modifier.
- Player velocity inheritance factor.
- Impact threshold/energy coefficient/cap.
- Knockback multiplier.
- Shield durability/resistance.
- Break threshold and debris count if breakable.

Start from existing prefab values and tune using test cases. A stable spring-damper response should be mass-aware and clamped.

## 5. Known documentation conflicts

### A. `IKineticTarget` signature

Newer player spec:
- `OnPhysicsCrush(float impactSpeed, GameObject flyingProp)`
- `OnPlayerSlideRam(float slideSpeed)`

Older team handoff:
- `OnPhysicsCrush(float impactSpeed, GameObject flyingProp, bool hitWeakPoint)`
- `OnPlayerRam(float playerSpeed, Vector3 ramDirection)`

Resolve by inspecting the actual interface and all implementers. Update atomically or use an adapter/context object.

### B. Focus input

- Newer player spec: Q/F toggle.
- Older handoff: Left Shift listed in one table, but Q/F elsewhere.
- Shift may now be used by the movement upgrade.

Actual Input Actions and latest code decide. Do not rebind without permission.

### C. Focus resource

- Existing new player spec: infinite slow motion.
- Older handoff: TargetButton refills Focus energy.
- This add-on recommends rechargeable Focus.

Inspect actual implementation. Migrate to a resource meter only after understanding any existing recharge/reward behavior. Keep rewards once per event.

### D. Slide/ram thresholds

The documents cite 5.5 m/s, 7.5 m/s, and other conditions for different slide/ram systems. Do not merge them into one magic number. Find actual call sites and distinguish slide eligibility from enemy ram eligibility and TargetButton activation.

### E. Crate Cannon launch velocity

The 35 m/s value is not documented as either total world speed or aim-directed speed added to inherited player velocity. Inspect current code and choose/document one definition.

### F. Pull range and target masks

The current player subsystem describes 16 m for object interaction and 75 m for grapple traversal. Verify actual layers/masks and target priority. Do not rely on stale numeric layer IDs.

## 6. Tuning process

For each parameter:
1. Record current value from code/prefab.
2. Record the source/document that introduced it.
3. Change one category at a time.
4. Run the corresponding validation tests.
5. Record the tested build and result.
6. Keep only changes that improve behavior without breaking movement or combat readability.

Do not leave unexplained magic numbers. Use named serialized fields or shared settings assets according to the project's existing style.

---

# FILE: 12_ACCEPTANCE_AND_DELIVERY_CHECKLIST.md

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
