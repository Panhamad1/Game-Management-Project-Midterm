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
