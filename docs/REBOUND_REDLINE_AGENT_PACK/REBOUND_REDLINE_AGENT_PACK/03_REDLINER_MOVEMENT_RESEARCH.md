# REDLINER — Movement Research Reference for REBOUND

**Target Roblox experience:** `https://www.roblox.com/games/94987506187454/REDLINER`  
**Research date represented by the collected public material:** November 13, 2026  
**Purpose:** Extract movement behaviors and design principles that can inform REBOUND. This is not a claim to possess REDLINER source code.

## 1. Identity and officially listed controls

The official Roblox listing identifies REDLINER as a fast-paced FPS combining swordplay, gunplay, parrying, and an in-depth movement system. It advertises “no speed limit.” The listing's default PC controls are:

| Input | Official listed action |
|---|---|
| LMB | Melee |
| Q | Gun |
| F | Parry (including bullets) |
| RMB | Grapple |
| Left Shift | Dash |
| Left Ctrl | Slide |
| Space | Wallrun against a wall |

Keybinds are changeable. The official listing identifies the developers/credits and the exact experience ID. See `08_SOURCE_REGISTER.md`.

**Important control nuance:** Community guides describe Space as Jump on the ground and Wallrun near a wall, while the official listing labels it Wallrun. Treat contextual jump/wallrun behavior as community-reported until tested in the live client.

## 2. Evidence and confidence rules

Every claim in this document uses one of these labels:

- **[D] Developer/official:** listed by the game's official Roblox page.
- **[C] Community-reported:** described by a fan guide, experienced-player documentation, player discussion, or community tutorial.
- **[M] Measured:** independently measured under a repeatable test. This research pack contains no new direct in-game measurement.
- **[O] Observed:** directly inspected gameplay footage or reproduced live. This research pass did not perform new frame-by-frame or live-gameplay experiments.
- **[I] Inferred:** a plausible technical model that could produce the reported behavior.
- **[U] Unknown:** insufficient reliable evidence.

Confidence is separate from evidence type. A technique can be well described by several community guides but still not have a verified implementation model.

## 3. Main movement philosophy

The most consistent theme in the guides is **preserving and redirecting speed while controlling the camera**. The four main tools are described as parts of one route-building system rather than isolated abilities:

- Dash: quick repositioning/directional burst.
- Slide: low-profile movement and momentum retention/exit.
- Grapple: large route change, pull, and redirection.
- Wallrun: use suitable geometry as a movement route.

Community material describes bhopping, bhop strafing, slide hopping, grapple redirects, slingshots, downward grapple/Slam, dash-grapple combinations, wallrun combinations, ziplines, and bounce pads. Some are advanced, patch-sensitive, or potentially exploit-like. They are not all necessarily current or intended mechanics.

The practical experience principle is not “always maximize speed.” Guides also emphasize looking ahead, keeping the route readable, and being able to end a movement chain with control.

## 4. Base locomotion and jumping

### 4.1 Ground locomotion

- **[U]** Exact ordinary walk/run speed, acceleration, friction, reverse movement scaling, strafing scaling, and maximum normal ground speed are not established by the sources gathered.
- **[D]** Shift is Dash, not Sprint.
- **[C]** Some tutorials describe moving forward as the normal basis for movement techniques, but they do not give repeatable baseline physics values.
- **[I]** A persistent velocity controller is a sensible recreation approach, but no public source proves REDLINER's exact internal controller architecture.

Do not invent base speeds, acceleration values, friction constants, or a Quake/Source implementation claim.

### 4.2 Jump and contextual Space behavior

- **[D]** Official listing calls Space Wallrun against a wall.
- **[C]** Fan guides describe Space as Jump on the ground and Wallrun near a wall.
- **[C]** Community guides describe holding Space while moving to bunny hop.
- **[U]** Exact initial jump speed, gravity, jump height, airtime, buffering, coyote time, and input edge semantics are unknown.

### 4.3 Bunny hop and air strafing

Community movement guides describe:

- Holding Space while moving to bhop.
- Forward movement plus smooth camera turning to bhop-strafe.
- Smooth rather than abrupt camera turns.
- Slide hopping by sliding around each ground contact while continuing a bhop route.

**[C]** These techniques are documented.  
**[U]** Exact air-acceleration formula, speed increment per turn, and whether holding Jump is always sufficient in the current build remain unknown.  
**[I]** Camera direction and wish direction likely contribute to how the player shapes the aerial trajectory, but the exact relationship has not been measured.

## 5. Dash

Official input: **Left Shift [D]**.

The Wave 1 community mechanics document reports:

- Dash consumes one stamina bar [C].
- Dash temporarily increases speed to approximately **110 u/s** [C].
- An airborne dash moves in the direction the player is looking [C].
- Dash has significant speed falloff [C].

Unknown:

- Whether 110 u/s describes total speed, additive impulse, or a temporary set-speed state.
- Dash duration, cooldown, charge recovery, grounded direction rules, vertical velocity preservation, and cancel conditions.
- Whether the reported behavior remains current in the November 2026 client.

**Candidate interpretation [I]:** Dash may add a temporary directional velocity component or apply a short acceleration/velocity target followed by decay. Do not claim this is original source behavior.

## 6. Slide

Official input: **Left Ctrl [D]**.

Wave 1 community documentation reports:

- A minor speed boost on slide entry [C].
- Momentum retention for roughly two seconds by making the player frictionless [C].
- Dash/slide combinations as a stable base route [C].

Other guides emphasize sliding immediately before or after landing to preserve route flow and exit a line of fire.

Unknown:

- Minimum activation speed.
- Whether the input is tap/hold/toggle in the current build.
- Exact friction profile and duration.
- Exact collider/stance change.
- Whether slide itself has an independent speed cap.

Do not copy the reported “two seconds frictionless” statement as an exact REBOUND tuning value.

## 7. Grapple

Official default PC input: **RMB [D]**. An older community guide uses E for an earlier/alternate binding; the official listing is the authority for the default PC control.

Community sources describe the grapple as a major movement route changer for vertical/horizontal travel and momentum redirection.

### 7.1 Ground Slam

The Wave 1 guide reports that grappling toward the ground while high in the air initiates Ground Slam [C]. The guide also reports a yellow crosshair indicator and a 60% Instability effect on impact, which are combat-specific details and are **not to be imported into REBOUND**.

For movement research, the relevant behavior is that downward grapple can create a distinct pending slam/impact state and can be cancelled or combined with other movement actions.

### 7.2 Chase grapple

The Wave 1 guide reports that grappling toward an airborne player triggers Chase and travels toward that player at 250 u/s [C]. This player-targeting combat feature is outside REBOUND's movement-only scope and must not be copied.

### 7.3 Grapple physics

- **[C]** Community guide descriptions refer to grapple elasticity and slingshotting.
- **[C]** BDG and other techniques imply grapple can interact with pre-existing dash and lateral momentum.
- **[I]** A radial/tangential decomposition or elastic tether could reproduce some of these behaviors.
- **[U]** Exact rope constraint, spring constant, damping, range, attachment validation, force curve, release impulse, and resource model are unknown.

### 7.4 Grapple slingshot/redirect

The Wave 1 guide describes slingshot as redirecting movement through grapple elasticity to gain speed [C]. It also describes grappling sideways or choosing routes to redirect rather than merely pulling straight toward the anchor.

Candidate behavior [I]: modify radial velocity toward the anchor while retaining tangential/lateral velocity, then detach with the resulting velocity. No exact formula is verified.

## 8. Advanced movement-tech encyclopedia

These techniques are community-documented. Their availability and strength can change after patches.

### 8.1 Back Dash Grapple (BDG)

**Reported sequence [C]:**

1. Jump.
2. Dash backward.
3. Grapple forward simultaneously or within a very tight timing window.
4. Use grapple elasticity to propel forward.

Wave 1 guide reports approximately **130–145 u/s** on a good timing/angle. The guide says a follow-up turn/slingshot can redirect without using another dash. It reports that slingshot can avoid ordinary dash speed falloff.

**Technical implication [I]:** movement transitions overlap; the grapple may preserve/transform the backward dash impulse instead of letting the dash component simply decay.

**Version note:** A player discussion dated August 21, 2026 describes BDG as still usable around 120–130 ups but says retaining its speed in combat felt more difficult after an update. Treat historical values as version-dependent.

### 8.2 Slam Slide / Sage Slide

**Reported sequence [C]:**

1. Look down.
2. Jump.
3. Dash backward.
4. Grapple the floor to initiate Ground Slam.
5. Crouch/slide at a narrow timing window to cancel the slam.
6. Convert the downward/pending-slam transition into forward speed.

The guide reports 125 u/s base, +5 u/s when holding slide briefly after activation, and up to about 150 u/s with slingshot. These are community values with no independently verified unit calibration.

### 8.3 Slam Storage

The guide describes a pending Ground Slam that remains armed as long as the player avoids touching the ground. It can reportedly be delayed by wallrunning or directional dashes, then resolves on a later ground contact.

Movement-only relevance: an ability can remain pending while another movement state occurs. This is a useful design pattern for REBOUND only if a corresponding mechanic is intentionally designed; do not add REDLINER's combat Slam state verbatim.

### 8.4 Willow Walling / Hyper Blitz

Reported sequence [C]:

1. Start with movement against/along a wall.
2. Dash.
3. Initiate wallrun during the dash.
4. The wallrun reportedly makes another dash available.
5. Repeat to accumulate speed.
6. Slingshot after the last dash to avoid the reported falloff.

The Wave 1 guide reports high-200 u/s speeds and around 400 u/s as difficult. These are historical, unverified, and should not be converted to Unity m/s.

### 8.5 Super Jump

Reported sequence [C]: dash while jumping with precise timing. The guide claims the dash momentum transfers into the jump without ordinary dash falloff, launching the player into the air. Exact frames and behavior are unknown.

### 8.6 JarBoost / Super WallJump

Reported sequence [C]:

1. Grapple to the ground to start Ground Slam.
2. Immediately wall-jump and dash in the direction intended for flight.
3. The guide says the second action must happen almost instantaneously after grapple contact.
4. Curved surfaces reportedly create more momentum than flat surfaces.
5. The pending Slam may remain stored until eventual ground contact.

This is a highly timing-sensitive community technique, not a required REBOUND feature.

### 8.7 Slide Redirect

Reported as an intended REDLINER mechanic [C]: slide backward at landing with good timing; the player slides backward while retaining forward momentum. The guide describes a unique backward-slide sound cue.

Movement principle: requested slide direction and inherited velocity direction can differ during a landing transition.

### 8.8 Wallrun Redirect

Reported [C]: wallrun, then hold forward toward the intended release direction as the player exits. The guide claims it can redirect away from different walls in the camera-facing direction.

### 8.9 Wall Sling

Reported combination [C]: BDG followed by wallrun to extend speed, then slingshot/redirect off the wall. The guide claims wallrunning prevents dash decay and historical versions added 10–15 speed. Version-sensitive.

### 8.10 Zipline and Zipline Redirect

Wave 1 guide reports:

- Press R near a hanging zipline to attach.
- A reported travel speed around 135 u/s.
- Jumping off/redirecting with Space immediately after grabbing can send the player toward the camera-facing direction.
- Attaching may refresh wallrun availability.

These environmental systems are not currently listed as REBOUND player features. Treat as optional future design inspiration only.

### 8.11 Zip Boost / Zip Dash

Reported sequence [C]: near a hanging zipline, initiate Ground Slam and attach to the line at nearly the same time, then cancel Slam. The guide claims speeds above 200 u/s. This is highly timing- and geometry-dependent.

### 8.12 Bounce Pads

Wave 1 guide reports that bounce pads launch upward while retaining forward movement; a developer version reportedly launches at 100 u/s. No current numeric behavior verified.

### 8.13 Combat-origin movement — EXCLUDED FROM REBOUND IMPORT

Community sources describe gun recoil and parry knockback as movement sources. These are not to be implemented as REDLINER-style guns/parries in REBOUND. REBOUND already has its own pneumatic recoil and physical crate projectile system; improve those in their own design context only.

## 9. Momentum findings

### 9.1 What is supported

- **[D]** REDLINER advertises no speed limit.
- **[C]** Dash has falloff.
- **[C]** Slide is described as preserving momentum.
- **[C]** Bhop/strafe and slide hop are described as carrying speed across jumps.
- **[C]** Grapple elasticity can redirect movement and slingshot.
- **[C]** BDG can combine backward dash and forward grapple.
- **[C]** Wallrun routes can interact with repeated dash/slingshot.
- **[C]** Environmental geometry and launch pads can contribute to momentum.

### 9.2 What remains unknown

- Ordinary grounded and airborne friction constants.
- Ground acceleration and max wish speed.
- Exact vector decomposition and controller type.
- Whether the game has any conditional speed clamps.
- Whether “no speed limit” applies to every state and every version.
- Exact momentum behavior on collision, landing, and grapple detachment.
- Exact current numeric behavior of all advanced techniques.

### 9.3 Historical community-reported numeric references

| Technique/property | Reported value | Evidence and caution |
|---|---:|---|
| Dash temporary speed | 110 u/s | [C], Wave 1 guide; meaning/units not calibrated |
| Ground Chase grapple | 250 u/s | [C], player-targeting combat feature; do not import |
| Slide momentum retention | About 2 s | [C], frictionless claim from Wave 1 guide |
| Grapple charges | 2 | [C], old grapple guide; resource system conflict exists |
| Per-charge recharge | 2 s | [C], old grapple guide; verify current build |
| Slam Slide base speed | 125 u/s | [C], uncalibrated |
| Slam Slide held-slide bonus | +5 u/s | [C], uncalibrated |
| Slam Slide + slingshot | Up to about 150 u/s | [C], uncalibrated |
| BDG | 130–145 u/s | [C], historical |
| Willow Walling | High 200s u/s | [C], historical |
| Difficult Willow Walling run | About 400 u/s | [C], historical |
| Zipline | 135 u/s | [C], historical |
| Zip Boost | Above 200 u/s | [C], historical |
| Wallrun after zipline | Up to 130 u/s | [C], historical |
| Developer bounce pad | 100 u/s | [C], not necessarily player-accessible |

**Do not paste these numbers into REBOUND.** The `u/s` scale is not calibrated to meters or Unity world units. These are research leads and relative behavior indicators.

## 10. Candidate physics abstractions for recreation

These are implementation patterns that may reproduce observed effects. They are **not claims about REDLINER's hidden source code**.

### 10.1 Shared velocity state

Represent player velocity as:

`v = (vx, vy, vz)`

Use movement abilities to apply explicit acceleration, impulses, projections, or friction rather than setting the entire velocity to a fixed speed every frame.

### 10.2 Directional acceleration

`v_next = v_current + a * dt`

Control acceleration along the requested direction while preserving perpendicular momentum.

### 10.3 State-dependent drag

`v_next = v_current * exp(-k * dt)`

Use different drag coefficients for ground, air, slide, dash, grapple, and other intentional states. This is a convenient candidate model, not a measured REDLINER formula.

### 10.4 Grapple radial/tangential decomposition

For anchor direction `u = normalize(anchor - player)`:

`v_radial = dot(v, u) * u`

`v_tangent = v - v_radial`

Modify radial motion through pull/tension and preserve some or all tangent velocity. This can yield swinging and slingshot-like behavior.

### 10.5 Wall-plane projection

For wall normal `n`:

`v_wall = v - dot(v, n) * n`

Then apply controlled adhesion, acceleration, gravity, and detach logic.

### 10.6 Landing transitions

Separate the landing event from the resulting locomotion state. Resolve vertical contact, preserve or modify horizontal velocity, then process buffered jump/slide/ability transitions in a deterministic order.

## 11. Version and source limitations

- The Wave 1 community mechanics document labels itself as a community/players' guide and warns that tech can change in future updates.
- It contains December 2025 change-log entries; this is historical documentation.
- A June 2026 guide describes core movement and practical routing.
- An October 25, 2026 fan guide describes movement as dash, slide, wallrun, grapple, Slam, bhop, redirects, and recoil movement.
- A player discussion in August 2026 reports BDG speed retention feeling different after an update. This is anecdotal, not patch documentation.
- The official Roblox listing establishes the game identity and control labels but does not explain physics values.
- No live-gameplay testing or new frame-by-frame measurement was conducted for this pack.

## 12. Movement features to borrow for REBOUND (not copy blindly)

Prioritize these principles:

1. Persistent velocity across locomotion and abilities.
2. Camera-aware but not camera-snapping air steering.
3. Reliable hold/buffered bunny hopping.
4. Slide that changes friction and preserves speed.
5. Landing-based slide redirect.
6. Grapple with tangential momentum preservation and deliberate release.
7. Clear combinations between slide, jump, grapple, and REBOUND's pneumatic recoil.
8. Optional wallrunning only where the level geometry and control design support it.
9. Feedback that communicates speed/state without obscuring aim.
10. Skill expression through timing and trajectory choice, not hidden random boosts.

Do not implement REDLINER's combat systems, gun recoil, sword/parry behavior, instability, or its exact UI/game identity.
