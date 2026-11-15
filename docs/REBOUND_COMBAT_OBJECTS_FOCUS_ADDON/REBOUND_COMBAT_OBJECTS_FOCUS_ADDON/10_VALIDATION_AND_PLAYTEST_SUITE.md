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
