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
