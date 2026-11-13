# REBOUND — Movement Repository Audit & Redliner Upgrade Roadmap

This document satisfies the **Section C Repository Audit** mandated by `01_MASTER_AGENT_PROMPT.md` and establishes the authoritative architecture roadmap for upgrading REBOUND's movement system to capture the kinetic feel of REDLINER.

---

## 1. Environment & Package Manifest

- **Unity Editor Version:** `6000.3.8f1 (1c7db571dde0)` (Unity 6 LTS)
- **Key Packages:**
  - `com.unity.inputsystem`: `1.18.0` (Unity New Input System)
  - `com.unity.render-pipelines.universal`: `17.3.0` (Universal Render Pipeline)
  - `com.unity.test-framework`: `1.6.0` (Unity Test Runner / NUnit)
  - `com.unity.ugui`: `2.0.0`
  - `com.unity.ai.navigation`: `2.0.10`
  - `com.unity.modules.physics`: `1.0.0` (PhysX 4.x runtime)
  - `com.unity.modules.audio`: `1.0.0`
- **Compiler State:** 0 compile errors; clean Tundra build; exit code 0.

---

## 2. Rig Hierarchy, Geometry & PhysX Settings

### Root GameObject: `Player_Rig`
- **Layer:** `Player` (Layer 3 or dynamic)
- **Tag:** `Player`
- **Rigidbody Configuration:**
  - `mass`: `75.0 kg`
  - `linearDamping`: `0.05f` in builder, overridden to `0.0f` during active movement simulation
  - `angularDamping`: `0.05f`
  - `useGravity`: `true` (PhysX base gravity = -9.81 m/s²; custom gravity multiplier added in code)
  - `isKinematic`: `false`
  - `interpolation`: Currently `None` in `PlayerMovement.Awake()`; will upgrade to `RigidbodyInterpolation.Interpolate` to eliminate first-person camera judder at high speeds (35 m/s).
  - `collisionDetectionMode`: `CollisionDetectionMode.ContinuousDynamic` (critical to prevent tunneling through thin barricades at 35 m/s).
  - `constraints`: `FreezeRotationX | FreezeRotationZ` (yaw controlled programmatically).
- **CapsuleCollider Configuration:**
  - `radius`: `0.5m`
  - `standingHeight`: `2.0m`
  - `crouchHeight`: `1.15m`
  - `feetLocalY`: `-1.0m` (Floor anchor guarantee: bottom of capsule is permanently anchored at local Y = -1.0 so resizing never causes floor penetration).
  - `center`: Standing `(0, 0, 0)`; Crouching `(0, -0.425, 0)`.
  - `material`: Frictionless `PlayerFrictionless` (`dynamicFriction = 0`, `staticFriction = 0`, `frictionCombine = Minimum`, `bounciness = 0`).

### Child Hierarchy
- `Player_Rig`
  - `CameraPivot` (Local Y = 1.65m standing / 1.05m crouched; component: `MouseLook.cs`)
    - `MainCamera` (Universal Additional Camera Data, AudioListener, base FOV 85°, dynamic FOV 85°–105°, near clip plane 0.03m)
      - `AbilityRayOrigin` (0.35m in front of lens)
      - `LeftHandVisual` (`PushAbility.cs`, LineRenderer shockwave ring)
      - `RightHandVisual` (`PullAbility.cs`, grapple LineRenderer, vacuum rings)
      - `SlowMotionController` (`SlowMotionAbility.cs`)
  - `GroundCheck` (Local Y = -0.85m; sphere overlap radius 0.35m + downward raycast fallback)
  - `AbilityHUD_Canvas` (`AbilityHUD.cs`, Screen Space Overlay dynamic crosshair & speedometer)

---

## 3. Scripts, Velocity Writers & Authority Model

### Current Writers to `Rigidbody.linearVelocity`:
1. `PlayerMovement.cs`:
   - `ApplyMovementVelocity()`: Overwrites `rb.linearVelocity` for grounded walk/sprint/crouch, slope surfing, and airborne strafe.
   - `ProcessBufferedJump()`: Sets `rb.linearVelocity.y = jumpImpulse` and horizontal velocity with `bHopBoost`.
   - `ApplyRecoil()`: Adds recoil vector directly to `rb.linearVelocity`.
   - `ApplyGrapplePull()`: Modifies `rb.linearVelocity` along pull direction and clamps inward component.
   - `FlingGrapple()`: Overwrites `rb.linearVelocity` with slingshot boost vector.
   - `ApplyCustomGravity()`: Calls `rb.AddForce(extraGravity, ForceMode.VelocityChange)`.
   - `SnapToPosition()` / `RespawnAtCheckpoint()`: Resets velocity to zero on respawn.
2. `PushAbility.cs`:
   - Delegated cleanly: Calls `playerMovement.ApplyRecoil(finalRecoilVelocity)`.
3. `PullAbility.cs`:
   - Delegated cleanly: Calls `playerMovement.ApplyGrapplePull(...)` in `FixedUpdate` and `playerMovement.FlingGrapple(...)` on release.

### Identified Architectural Vulnerabilities & Duplicate Authorities:
- **Ground Velocity Replacement:** In `PlayerMovement.ApplyMovementVelocity()`, when grounded, `rb.linearVelocity` is directly replaced by `targetVelocity` (`targetDirection * targetSpeed`). If the player lands with 25 m/s momentum and touches the ground while walking, velocity instantly snaps down to 6.5 m/s or 11.5 m/s unless bunny-hopping or sliding!
  - *Fix Required:* Implement an **Authoritative Kinetic Velocity Model** with separate control acceleration, directional wish vectors, ground friction/braking, and momentum decay rather than raw velocity snapping.
- **Interpolation Jitter:** `rb.interpolation` is set to `None`, which can cause camera-physics micro-stutter when moving at 35 m/s. Upgrading to `Interpolate` ensures butter-smooth visual delivery.
- **Timer Time Domain:** Some timers in `PlayerMovement` use `Time.time` while Focus (Slow Motion) alters `Time.timeScale` to 0.25 and `Time.fixedDeltaTime` to 0.005. Buffer timers must be verified in unscaled vs scaled domains.

---

## 4. Parameter Reconciliation Ledger (Resolving 07_PARAMETER_LEDGER.md)

| Conflict / Issue | Baseline Given | Ledger Question | Resolved Technical Decision |
|---|---|---|---|
| **Slide Eligibility vs Run Threshold** | 6.0 m/s slide check vs 7.5 m/s run threshold | Which threshold governs slide entry? | **6.0 m/s is the slide activation threshold** (allowing early slide entries from sprint acceleration); **7.5 m/s is the Slide Bowling Ram threshold** (requiring high kinetic momentum to shatter barricades and crush props). |
| **Slide Entry Boost vs Min Speed** | `CurrentSpeed + 4.5 m/s` vs `minSlideEntrySpeed = 14 m/s` | Is 14 m/s applied before or after boost? | Applied as `entrySpeed = Mathf.Max(CurrentSpeed + 4.5f, 14.0f)`. If entering at 11.5 m/s sprint, speed becomes `max(16.0, 14.0) = 16.0 m/s`. If entering at 6.0 m/s, speed becomes `max(10.5, 14.0) = 14.0 m/s`. |
| **Jump Height & Gravity Derivation** | `jumpImpulse = 6.6 m/s`, claimed height 1.6m vs gravity | How does 6.6 m/s yield 1.6m with 2.4x gravity? | With standard gravity $g = 9.81 \times 2.4 = 23.54\text{ m/s}^2$, $h = \frac{v_y^2}{2g} = \frac{43.56}{47.08} \approx 0.93\text{m}$. To achieve a true 1.6m jump height under $2.4\times$ gravity, $v_y = \sqrt{2 \times 23.54 \times 1.6} \approx 8.68\text{ m/s}$. We will calibrate upward takeoff impulse to **8.7 m/s** to deliver the true 1.6m vertical clearance specified! |
| **Slide Drag Equation & Units** | `decel = slideFriction + (v * slideDrag)` | What are the units of `slideDrag`? | `slideFriction = 2.2 m/s²` (Coulomb constant friction). `slideDrag = 0.035 s⁻¹` (linear aerodynamic velocity damping). Formula: $a_{\text{drag}} = 2.2 + (0.035 \times v)$. At 30 m/s, total deceleration is $2.2 + 1.05 = 3.25\text{ m/s}^2$. |
| **Slope Acceleration** | `slopeAcceleration = 28 m/s²` | Is it physical gravity or authored multiplier? | Authored gameplay acceleration factor. Physical gravity parallel to slope is $g \sin(\theta)$ ($9.81 \sin(26^\circ) \approx 4.3\text{ m/s}^2$). The $28\text{ m/s}^2$ factor delivers high-octane Redliner surf acceleration without altering world gravity. |
| **35 m/s Speed Cap Policy** | `maxSpeed = 35.0 m/s` | Is it a hard clamp on every state? | Soft limit on standard locomotion; hard clamp only on maximum ability stacking to prevent PhysX collider tunneling. Sliding, grapple flings, and rocket jumps can reach up to 35 m/s smoothly. |
| **Focus Time Domain** | `slowMoTimeScale = 0.25` | Scaled vs unscaled timer policy? | Mouse look, ability HUD, and UI timers use `Time.unscaledDeltaTime`. Movement physics timers in `FixedUpdate` integrate with `Time.fixedDeltaTime` (which scales to 0.005s), ensuring physics simulation remains completely deterministic. |
| **Grapple Inward Cap** | 24–35 m/s range | When is inward speed limited? | Inward speed towards anchor is capped at $v_{\text{target}} = \text{Lerp}(24, 35, \frac{\text{dist}}{35})$, removing excess inward velocity while leaving $100\%$ of perpendicular/tangential velocity untouched for wide orbital swings. |

---

## 5. Multi-Stage Implementation Plan

### Stage 1: Diagnostic Instrumentation & Test Harness (`06_VALIDATION_AND_TEST_SUITE.md`)
- Create `Assets/_Project/Scripts/Player/PlayerMovementTelemetry.cs`:
  - Real-time physics telemetry HUD overlay (toggleable with `F3`):
    - Current velocity vector, horizontal speed, vertical velocity
    - Active locomotion state, ground normal, slope angle
    - Coyote & jump buffer timers, slide duration, grapple distance & angular velocity
    - Active force/impulse logs and speed cap alerts
- Create automated unit tests in `Assets/_Project/Tests/EditMode/MovementPhysicsTests.cs`:
  - Vector projection, air strafing math, slope acceleration, jump height calculations, and parameter clamps.

### Stage 2: Authoritative Kinetic Velocity Architecture (`PlayerMovement.cs`)
- Refactor ground movement from direct velocity snapping to **Momentum-Aware Control**:
  - Independent `groundAcceleration` (55 m/s²) and `groundBraking` (65 m/s²).
  - High-speed momentum preservation: when landing or running above sprint speed, preserve horizontal velocity and apply smooth aerodynamic drag rather than instant clamping.
  - Normalized diagonal input vector handling.
- Reconcile jump impulse to 8.7 m/s for true 1.6m clearance.

### Stage 3: Redliner Kinetic Locomotion Refinements
- **Slide Redirect & Landing Slide Queue:**
  - Support landing slide with buffered crouch (`slideBufferTimer`).
  - Implement Slide Redirect: Requesting slide direction opposite or oblique to landing momentum allows the player to carve or slide redirect while preserving momentum.
- **Continuous Hold-Space Bunny-Hop Chaining:**
  - Zero-friction transition on the exact physics tick of landing when Space is held.
  - Directional hop steering without erasing accumulated speed.
- **Slope Surfing Polish:**
  - Validate downhill surf acceleration and uphill energy conservation.

### Stage 4: Grapple Winch & Elastic Swinging Refinements (`PullAbility.cs`)
- Preserve 100% of tangential velocity during 75m winch pull.
- Dynamic inward speed targeting based on distance.
- Downward grapple auto-slide transition.
- Slingshot release with configurable fling boost (+6.5 m/s) and camera-momentum slerp.

### Stage 5: Recoil Integration & Wall Traversal Evaluation
- Wind Cannon (`PushAbility.cs`) recoil integration:
  - 1 aerial rocket jump per airtime cycle.
  - Surface proximity kick (within 3.8m).
  - Slide recoil forward conversion (+10 m/s).
- Wall Traversal Evaluation:
  - Arena perimeter walls are 35m tall, clean, and vertical.
  - If wall interaction is enabled, keep it intuitive (running along wall with Space jump-off) without clashing with the grapple.

### Stage 6: Validation Suite & Parameter Finalization
- Execute tests G01–G09, A01–A10, S01–S12, P01–P08 from `06_VALIDATION_AND_TEST_SUITE.md`.
- Record test results, verify 0 compiler errors, and update documentation.
