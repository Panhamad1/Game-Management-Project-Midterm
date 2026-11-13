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
