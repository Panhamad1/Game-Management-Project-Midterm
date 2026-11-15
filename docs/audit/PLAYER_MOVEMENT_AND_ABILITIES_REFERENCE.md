# PLAYER MOVEMENT & ABILITIES REFERENCE

**Document ID:** `AUDIT-PLY-002`  
**Classification:** Gameplay Mechanics, Mathematics & Physics Extraction  
**Project:** REBOUND | **Branch:** `monirath/game-rework`  
**Date:** 2026-11-15  

---

## 1. IMPLEMENTED MOVEMENT MECHANICS INVENTORY

Every mechanic below has been verified in `PlayerMovement.cs` and `PullAbility.cs` [V]:

### 1.1 Walking & Running (Locomotion)
- **Trigger:** WASD input without Shift.
- **Speed:** $5.5\text{ m/s}$ forward/strafe.
- **Physics Model:** Custom kinematic-style acceleration curve applied directly to `rb.linearVelocity` in `FixedUpdate()`.
- **Acceleration:** $35.0\text{ m/s}²$. Reaches full speed in $\approx 0.16\text{s}$.
- **Braking:** $35.0\text{ m/s}²$ opposing-input deceleration. Stops in $\approx 0.16\text{s}$ when input is zeroed.
- **Normal Grounding:** Input is projected onto the detected ground surface plane using `Vector3.ProjectOnPlane(inputDir, groundNormal).normalized`.

### 1.2 Sprinting
- **Trigger:** Holding `Left Shift` while providing forward/diagonal WASD input.
- **Speed:** $8.5\text{ m/s}$.
- **Exit Condition:** Releasing Shift, zeroing WASD input, or entering a crouch/slide.
- **Airborne Sprint Memory:** Releasing Shift while airborne preserves the sprinting state until touchdown (`airborneRunPreserved = true`).

### 1.3 Crouching & Crouch-Walking
- **Trigger:** Pressing or holding `C` or `Left Ctrl` when horizontal speed is below the run threshold ($7.0\text{ m/s}$).
- **Speed:** $3.5\text{ m/s}$.
- **Capsule Adjustment:** Height shrinks smoothly from $2.0\text{m}$ to $1.15\text{m}$ at rate $12.0\text{ s}^{-1}$.
- **Floor Anchoring:** Capsule bottom is locked at local $Y = -1.0\text{m}$ to prevent ground penetration or camera pop.
- **Eye Height:** Camera pivot interpolates from $Y = 0.65\text{m}$ to $Y = 0.05\text{m}$ ($1.05\text{m}$ above the floor).
- **Overhead Clearance:** An upward raycast (`overheadClearanceMask`) checks for low ceilings; the player cannot uncrouch if obstructed.

### 1.4 Kinetic Power Slide
- **Trigger:** Pressing `C` or `Left Ctrl` while sprinting with horizontal speed $\ge 7.0\text{ m/s}$.
- **Entry Boost:** $0.0\text{ m/s}$ (strictly preserves incoming sprint velocity; free speed boost was removed in Stage 3 redesign).
- **Slide Direction:** Locked to current horizontal travel vector on entry: $\vec{D}_{\text{slide}} = \text{ProjectOnPlane}(\vec{v}, \vec{n}_{\text{ground}})$.
- **Steering:** Player can steer up to $110.0^\circ/\text{s}$ using mouse look and A/D keys.
- **Deceleration Equation:**
  $$\frac{dv}{dt} = -\mu_{\text{slide}} - C_{\text{drag}} \cdot v^2 + \vec{g} \cdot \vec{D}_{\text{slide}}$$
  where $\mu_{\text{slide}} = 2.0\text{ m/s}²$ (ground friction) and $C_{\text{drag}} = 0.025\text{ m}^{-1}$ (aerodynamic drag).
- **Speed Cap:** Clamped at $14.0\text{ m/s}$.
- **Minimum Duration:** Guaranteed minimum slide duration of $0.35\text{s}$ before input cancellation can occur.
- **Exit Condition:** Speed drops below $3.5\text{ m/s}$, player releases crouch, or player presses Space to jump.

### 1.5 Slope Surfing & Hill Launches
- **Downhill:** Gravity accelerates the slide along the slope: $a_{\text{slope}} = 22.0\text{ m/s}² \times \sin(\theta)$.
- **Uphill:** Gravity steeply decelerates the slide up to $-22.0\text{ m/s}²$.
- **Ballistic Launch:** Sliding off a downward crest or ramp launches the player into the air, fully preserving horizontal and vertical takeoff vectors into ballistic flight.

### 1.6 Jump & Auto-Bunny-Hop (B-Hop)
- **Jump Impulse:** $\vec{J} = +8.7\text{ m/s}$ along $\vec{n}_{\text{ground}}$ (achieves precisely $1.6\text{m}$ vertical apex under $2.4\times$ standard gravity).
- **Coyote Time:** $0.15\text{s}$ window after walking off a ledge where jumping is still accepted.
- **Jump Buffering:** $0.15\text{s}$ window before landing where pressing Space queues a jump upon touching the ground.
- **Slide-Hop:** Pressing Space during a slide converts ground velocity into air velocity with 100% horizontal momentum preservation.
- **Auto-Bunny-Hop:** Holding Space continuously re-triggers jumps upon ground contact. Bunny-hop landing boost: $0.0\text{ m/s}$ (preserves speed without uncontrolled infinite acceleration).

### 1.7 Air Strafing (Quake / Source Style)
- **Air Wish Speed:** $11.0\text{ m/s}$.
- **Air Acceleration:** $10.0\text{ m/s}²$.
- **Air Speed Cap:** $12.0\text{ m/s}$ for pure air strafing without external boost.
- **Velocity Projection:**
  $$v_{\text{current}} = \vec{v}_{\text{horizontal}} \cdot \vec{D}_{\text{wish}}$$
  $$a_{\text{add}} = \max\left(0, v_{\text{wish\_max}} - v_{\text{current}}\right)$$
  $$\Delta \vec{v} = \vec{D}_{\text{wish}} \cdot \min\left(a_{\text{air}} \cdot \Delta t, a_{\text{add}}\right)$$
  Turning smoothly into the movement direction while holding A or D curves the trajectory and preserves momentum without drag.

### 1.8 Grapple Slingshot Traversal
- **Trigger:** Right-Click hold targeting static geometry within $22.0\text{m}$.
- **Inward Pull Target:** $14.0\text{ m/s}$ inward speed along the tether line.
- **Acceleration:** $22.0\text{ m/s}²$ pulling toward anchor point.
- **Tangential Velocity:** $100\%$ of perpendicular swing velocity is preserved; the inward component is accelerated toward the target speed.
- **Detachment:** Automatically detaches when within $2.5\text{m}$ of anchor. Releasing RMB or tapping Space triggers a slingshot release preserving current velocity up to `transientGrappleCap` ($17.0\text{ m/s}$).

---

## 2. MOVEMENT PARAMETERS SUMMARY TABLE

| Parameter Name | Inspector Identifier | Default | Code Location | Status |
|---|---|---|---|---|
| Walk Speed | `walkSpeed` | 5.5 m/s | `PlayerMovement.cs:22` | Verified [V] |
| Sprint Speed | `sprintSpeed` | 8.5 m/s | `PlayerMovement.cs:25` | Verified [V] |
| Crouch Speed | `crouchSpeed` | 3.5 m/s | `PlayerMovement.cs:28` | Verified [V] |
| Ground Acceleration | `groundAcceleration` | 35.0 m/s² | `PlayerMovement.cs:31` | Verified [V] |
| Ground Braking | `groundBraking` | 35.0 m/s² | `PlayerMovement.cs:34` | Verified [V] |
| Momentum Drag | `groundMomentumDrag` | 3.0 s⁻¹ | `PlayerMovement.cs:37` | Verified [V] |
| Air Wish Speed | `maxAirWishSpeed` | 11.0 m/s | `PlayerMovement.cs:40` | Verified [V] |
| Air Acceleration | `airAcceleration` | 10.0 m/s² | `PlayerMovement.cs:43` | Verified [V] |
| Air Speed Cap | `airSpeedCap` | 12.0 m/s | `PlayerMovement.cs:46` | Verified [V] |
| Jump Impulse | `jumpImpulse` | 8.7 m/s | `PlayerMovement.cs:49` | Verified [V] |
| B-Hop Boost | `bHopBoost` | 0.0 m/s | `PlayerMovement.cs:52` | Verified [V] |
| Slide Threshold | `runThreshold` | 7.0 m/s | `PlayerMovement.cs:56` | Verified [V] |
| Slide Speed Cap | `slideSpeedCap` | 14.0 m/s | `PlayerMovement.cs:65` | Verified [V] |
| Slide Drop Speed | `minSlideSpeed` | 3.5 m/s | `PlayerMovement.cs:68` | Verified [V] |
| Slide Friction | `slideFriction` | 2.0 m/s² | `PlayerMovement.cs:71` | Verified [V] |
| Slide Drag | `slideDrag` | 0.025 m⁻¹ | `PlayerMovement.cs:74` | Verified [V] |
| Slope Acceleration | `slopeAcceleration` | 22.0 m/s² | `PlayerMovement.cs:77` | Verified [V] |
| Slide Steering Rate | `slideSteerRate` | 110.0 deg/s | `PlayerMovement.cs:80` | Verified [V] |
| Slide Recoil Boost | `slideRecoilBoost` | 6.0 m/s | `PlayerMovement.cs:83` | Verified [V] |
| Emergency Ceiling | `emergencySpeedCeiling` | 19.0 m/s | `PlayerMovement.cs:86` | Verified [V] |
| Grapple Inward Speed | `grappleInwardSpeed` | 14.0 m/s | `PlayerMovement.cs:96` | Verified [V] |
| Grapple Pull Accel | `grapplePullAcceleration` | 22.0 m/s² | `PlayerMovement.cs:99` | Verified [V] |
| Grapple Transient Cap| `transientGrappleCap` | 17.0 m/s | `PlayerMovement.cs:102` | Verified [V] |
| Coyote Time | `coyoteTime` | 0.15 s | `PlayerMovement.cs:114` | Verified [V] |
| Jump Buffer Time | `jumpBufferTime` | 0.15 s | `PlayerMovement.cs:115` | Verified [V] |
| Standard Gravity | `standardGravityScale` | 2.4 x (23.54 m/s²) | `PlayerMovement.cs:123` | Verified [V] |
| Focus Gravity | `slowMoGravityScale` | 0.5 x (4.90 m/s²) | `PlayerMovement.cs:124` | Verified [V] |

---

## 3. IMPLEMENTED ABILITIES & WEAPONS REFERENCE

### 3.1 Kinetic Hammergun (`KineticHammergun.cs`)
- **Trigger:** Left Mouse Button (Hold & Release).
- **Drawback Window:** $0.00\text{s} - 0.22\text{s}$ (Priming, Snapshot ready).
- **Sweet-Spot Window:** $0.22\text{s} - 0.45\text{s}$ (Critical Release).
- **Overcharge Window:** $> 0.50\text{s}$ (Auto-fires at $1.0\text{s}$).
- **Critical Slug Output:** $70\text{ DMG}$, $85\text{ m/s}$ ballistic velocity, $16\text{ N}$ impulse, penetrates light swarmers, camera recoil punch ($0.35$ trauma).
- **Snapshot Output:** $35\text{ DMG}$, $55\text{ m/s}$ velocity, stops at first target.
- **Parry Integration:** On release, any enemy projectile in a $2.8\text{m}$ forward sphere is deflected into a radiant electric cyan projectile.

### 3.2 Pull Ability: Grapple & Snatch (`PullAbility.cs`)
- **Grapple Slingshot (Geometry/Pillars):**
  - Range: $22.0\text{m}$ (sharp raycast + $0.25\text{m}$ spherecast assist).
  - Layers: `Environment`, `GrappleAnchors`, `Default`.
  - Inward Target: $14.0\text{ m/s}$ inward pull at $22.0\text{ m/s}²$.
- **Gravity Snatch (Props & Crates):**
  - Range: $14.0\text{m}$ (Light/Medium props), $9.0\text{m}$ (Heavy props).
  - Reel Speed: $18.0\text{ m/s}$ (Light), $14.0\text{ m/s}$ (Medium), $8.0\text{ m/s}$ (Heavy ground drag).
  - Hold Anchor Pocket: Local offset $(+0.42\text{m right}, -0.42\text{m down}, +2.65\text{m forward})$ leaving center crosshair unobstructed.
  - Heavy Move Penalty: Holding a heavy slab reduces player walk/sprint speed by $30\%$ ($0.70\times$ multiplier).

### 3.3 Crate Cannon Combo Launch (`PullAbility.cs`)
- **Trigger:** Left Mouse Button while an object is locked in the front pocket.
- **Ballistic Formula:**
  $$\vec{V}_{\text{launch}} = \text{ClampMagnitude}\left(\vec{D}_{\text{aim}} \times 21.0\text{ m/s} + \vec{V}_{\text{player}} \times 0.25, 28.0\text{ m/s}\right)$$
- **Damage Equation on Impact:**
  $$\text{Damage} = \min\left(v_{\text{rel}} \times 1.2 \times \sqrt{m_{\text{prop}}}, 100\text{ HP}\right)$$
  Requires minimum impact speed $v_{\text{rel}} \ge 4.0\text{ m/s}$.

### 3.4 Push Ability: Wind Cannon (`PushAbility.cs`)
- **Trigger:** Legacy LMB fallback when not armed with Hammergun.
- **Range:** $12.0\text{m}$ forward raycast. Target Impulse: $18.0\text{ N}$.
- **Surface Proximity Rocket Kick:**
  - If raycast detects solid floor or wall within $3.0\text{m}$:
    - Ground kickback: $+4.5\text{ m/s}$ opposite surface normal.
    - Airborne rocket jump: $+6.0\text{ m/s}$ opposite surface normal.
    - Slide convert boost: $+6.0\text{ m/s}$ added directly forward into slide direction.
- **Parry Window:** $0.22\text{s}$ active window in $1.1\text{m}$ radius around player camera.

### 3.5 Slow Motion: Focus (`SlowMotionAbility.cs`)
- **Trigger:** Q, F, or Middle Mouse Click (toggle mode).
- **Time Dilation:** `Time.timeScale = 0.40`. Coupled physics step: `Time.fixedDeltaTime = 0.008s` (preserves 125 Hz physics rate).
- **Energy Pool:** 100 units. Drain rate: $25\text{ units/s}$ ($4.0\text{s}$ max duration).
- **Recharge Rate:** $15\text{ units/s}$ after $0.75\text{s}$ rest delay. Accelerated by $+12\text{ units/s}$ when moving $> 6\text{ m/s}$.
- **Lockout Cooldown:** $0.0\text{s}$ (instant on/off re-engagement).

---

## 4. MOVEMENT & ABILITY CONFLICTS AND LIMITATIONS

1. **Input Priority Competition (LMB):**
   Left-Click currently controls three overlapping mechanics in `PlayerAbilityInput.cs`:
   - Crate Cannon launch (if holding prop).
   - Kinetic Hammergun drawback (if armed).
   - Push Wind Cannon (if Hammergun is disabled).
   While functioning via priority branching, this creates modal confusion for the player.

2. **Emergency Velocity Clamping Interference:**
   `PlayerMovement.cs` enforces a hard horizontal velocity clamp at $19.0\text{ m/s}$. If a player launches a high-speed grapple slingshot downhill and triggers a slide-boost, the hard clamp truncates momentum abruptly without an elastic falloff curve.

3. **Ground Check Ray vs Sphere Discrepancies:**
   Ground detection uses both a $0.35\text{m}$ radius sphere check and a downward raycast. On stepped basalt surfaces or jagged Egyptian ruins, the sphere check can report grounded while the raycast reports airborne, causing frame-by-frame friction stuttering.
