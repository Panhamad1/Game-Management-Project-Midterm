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
