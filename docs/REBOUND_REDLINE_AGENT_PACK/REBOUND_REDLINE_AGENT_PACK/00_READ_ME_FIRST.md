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
