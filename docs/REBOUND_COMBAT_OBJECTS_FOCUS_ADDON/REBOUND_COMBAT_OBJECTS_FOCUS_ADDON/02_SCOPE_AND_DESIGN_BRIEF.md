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
