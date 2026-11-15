# PLAYER AUDIT FINDINGS & UNKNOWNS

**Document ID:** `AUDIT-PLY-005`  
**Classification:** Evidence-Based Defect, Risk & Unknowns Register  
**Project:** REBOUND | **Branch:** `monirath/game-rework`  
**Date:** 2026-11-15  

---

## 1. CONFIRMED TECHNICAL DEFECTS & ARCHITECTURAL PROBLEMS

| Finding ID | Severity | Classification | Description & Evidence | Gameplay Impact | Owner |
|---|---|---|---|---|---|
| `FIND-01` | **High** | Confirmed [V] | **No Player Prefab Asset in Project:** There is no `Player_Rig.prefab` in `Assets/_Project/Prefabs/` [V]. The player only exists as serialized scene instances or through procedural generation via `PlayerRigBuilder.cs`. | Opening a new scene or updating teammate levels causes player component drift, missing serialized references, and broken inspector links. | Player |
| `FIND-02` | **High** | Confirmed [V] | **Dual UI Architecture & Redundant Drawing:** `AbilityHUD.cs` initializes and updates UGUI Canvas GameObjects (Image, Text, Slider) while simultaneously executing a complete immediate-mode `OnGUI()` rendering loop every frame (`AbilityHUD.cs:385`). | Duplicated GPU draw calls, potential layout desynchronization across resolutions, and unnecessary memory allocation. | Player |
| `FIND-03` | **Medium** | Confirmed [V] | **Input Priority Branching Clutter:** `PlayerAbilityInput.cs:101-118` binds Left-Click to three disparate actions: Crate Cannon launch, Kinetic Hammergun drawback, and Push Ability blast. | While functioning via if/else priority order, the mechanics compete for the same input chord and cause player confusion. | Player |
| `FIND-04` | **Medium** | Confirmed [V] | **Hard Velocity Clamp Truncation:** `PlayerMovement.cs:86` enforces a hard horizontal velocity clamp at $19.0\text{ m/s}$ (`emergencySpeedCeiling`). | When exiting downhill slides or grapple slingshots with high momentum, the clamp truncates speed instantly rather than decaying smoothly. | Player |
| `FIND-05` | **Medium** | Confirmed [V] | **Stepped Surface Ground Check Jitter:** Ground detection combines a $0.35\text{m}$ sphere check with a downward raycast. On stepped Egyptian basalt edges, the two checks frequently disagree. | Results in rapid toggling between grounded and airborne states, causing micro-stuttering in movement velocity. | Player |
| `FIND-06` | **Low** | Confirmed [V] | **Unused Serialized Fields (Compiler Warnings):** `PullAbility.abilityRayOrigin` is declared but never read (warning CS0169); `vortexAngle` is assigned but never used (warning CS0414). | Code clutter and unnecessary serialized memory footprint. | Player |

---

## 2. SUSPECTED DEFECTS & DESIGN FRICTION

| Finding ID | Severity | Classification | Description & Evidence | Gameplay Impact | Owner |
|---|---|---|---|---|---|
| `SUSP-01` | **High** | Suspected [S] | **Lack of Physical Weapon Feedback:** The player has no animated 3D first-person weapon model; the Hammergun is represented by an instant 2-point `LineRenderer` tracer. | Attacks lack tactile presence, weight, and visual punch compared to industry-standard kinetic shooters. | Player |
| `SUSP-02` | **Medium** | Suspected [S] | **Grapple Detach Distance Sensitivity:** `PullAbility.cs` enforces `minGrappleDetachDistance = 2.5f`. At close range ($< 3\text{m}$), grapple shots immediately detach. | Players attempting quick vertical wall-hops at point-blank range experience immediate tether cancellation. | Player |
| `SUSP-03` | **Medium** | Suspected [S] | **Decoupled Audio Volume vs Mixers:** All audio is synthesized in code and routed directly to standard `AudioSource` components with no `AudioMixer` routing. | Master volume sliders, combat ducking, and spatial dynamic range compression cannot be centrally adjusted. | Player |

---

## 3. INCOMPLETE & UNFINISHED SYSTEMS

1. **Mantling & Ledge Grabbing [V]:**
   `PlayerRigBuilder.cs` mentions "Ledge mantling" in comments, but `PlayerMovement.cs` contains **zero ledge detection rays or mantling state logic**. The player cannot climb over waist-high obstacles without jumping over them completely.
2. **First-Person Viewmodel Rig [V]:**
   The player uses static cylinders for hands (`LeftHandVisual`, `RightHandVisual`). There is no skinned character mesh, no hand animations, and no weapon model in the project.
3. **Save & Progression Pipeline [V]:**
   There are no persistent player data containers, inventory systems, or upgrade trees. State resets completely on scene reload or checkpoint death.

---

## 4. GAPS & UNKNOWNS REQUIRING RUNTIME VALIDATION

1. **Framerate Dependence of PhysX Ground Friction [U]:**
   While `PlayerMovement` runs in `FixedUpdate()`, variable display refresh rates (e.g., $60\text{ Hz}$ vs $144\text{ Hz}$ vs $240\text{ Hz}$) with `RigidbodyInterpolation.Interpolate` may exhibit visual camera jitter if camera motion in `MouseLook.Update()` desynchronizes from fixed physics ticks.
2. **Teammate Level Geometry Collision Integrity [U]:**
   Whether all Egyptian pyramids, ramps, and ruins in teammate maps have contiguous convex mesh colliders, or whether non-convex geometry causes capsule snagging during high-speed slides.
3. **Controller / Gamepad Parity [U]:**
   `PlayerAbilityInput.cs` relies heavily on `Keyboard.current` and `Mouse.current`. Gamepad thumbsticks and triggers are not mapped in `PlayerAbilityInput.cs`, meaning full controller playability is currently unverified.
