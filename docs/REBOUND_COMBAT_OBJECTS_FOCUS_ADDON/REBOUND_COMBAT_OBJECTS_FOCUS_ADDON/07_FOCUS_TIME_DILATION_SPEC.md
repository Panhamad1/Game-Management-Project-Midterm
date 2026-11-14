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
