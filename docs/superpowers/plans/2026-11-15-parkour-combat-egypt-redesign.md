# REBOUND: Parkour Locomotion, Kinetic Combat & Egyptian Arena Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Transform REBOUND into a tight, parkour-focused kinetic action game with rebalanced speeds, wall-running, dash-to-slide chaining, camera juice, prop manipulation without grapple flinging, full player/enemy health with hit feedback (flashing, floating numbers), and a compact Egyptian arena packed with physics objects.

**Architecture:** Modular physics integration directly into established Rebound components (`PlayerMovement.cs`, `MouseLook.cs`, `PullAbility.cs`). Event-driven combat feedback (`PlayerHealth.cs`, `EnemyHitFlash.cs`, `FloatingDamageNumber.cs`). Automated procedural arena generation using native Unity URP materials and physics colliders.

**Tech Stack:** Unity 6000.3.8f1, C# 9.0+, Universal Render Pipeline (URP), Unity Physics / Rigidbody.

**Spec:** [docs/specs/2026-11-15-parkour-combat-egypt-redesign.md](file:///C:/Users/svaym/developer/Game-Management-Project-Midterm/docs/specs/2026-11-15-parkour-combat-egypt-redesign.md)

---

## Global Constraints

- Never use deprecated Unity APIs.
- Zero GC allocations in inner physics and tick updates (`Update`, `FixedUpdate`).
- All tests and verification must pass Roslyn compilation via Unity's internal `csc.dll` with 0 warnings and 0 errors.
- Never reintroduce the discarded rework folder (`Assets/_Project/Scripts/Player/Rework/`); work entirely within Rebound's canonical structure.

---

## Review Focus

1. Wall-run detachment edge cases when grazing corners or non-vertical geometry.
2. Dash-to-slide momentum preservation when pressing `Ctrl` mid-dash.
3. Clean prop carry and launch after grapple traversal removal.
4. Attack ID deduplication preventing multiple damage ticks per frame on the player.
5. Physics stability of 30+ props in the 45m Egyptian arena under rapid wind pushes and vortex pulls.

---

## Implementation Batches

### Parallel Batch 1: Locomotion Rebalance & Parkour Mechanics

#### Task 1: Movement Speed Rebalance & Omnidirectional Dash
- **Consumes:** `Assets/_Project/Scripts/Player/PlayerMovement.cs`
- **Produces:** Updated `PlayerMovement.cs` with calibrated speeds, dash impulse, cooldown, and dash-to-slide combo
- [ ] Step 1.1: Lower base locomotion parameters: `walkSpeed = 5.0f`, `sprintSpeed = 8.5f`, `crouchSpeed = 3.0f`, `maxSpeed = 16.0f`.
- [ ] Step 1.2: Add Dash state variables: `dashImpulse = 9.0f`, `dashCooldown = 1.2f`, `dashTimer`, `isDashing`.
- [ ] Step 1.3: Implement `PerformDash()` applying a horizontal velocity impulse in WASD direction on keybind (`LeftShift` double-tap or `Q` / `C`).
- [ ] Step 1.4: Implement Dash-to-Slide chaining: if `isDashing` and player presses or holds `Ctrl`, immediately transition into high-momentum slide with reduced friction.
- [ ] Step 1.5: Verify compilation via Roslyn compiler.

#### Task 2: Wall-Running & Wall-Jumping Engine
- **Consumes:** `Assets/_Project/Scripts/Player/PlayerMovement.cs`
- **Produces:** Updated `PlayerMovement.cs` with wall detection, wall-running physics, and outward wall-jumping
- [ ] Step 2.1: Add wall-run variables: `wallRunRayLength = 1.1f`, `wallRunGravityScale = 0.15f`, `wallRunMaxDuration = 1.8f`, `wallJumpForce = 7.5f`, `wallJumpUpForce = 6.5f`.
- [ ] Step 2.2: Implement `CheckForWallRun()` using lateral raycasts (left and right) to detect runnable vertical surfaces (>75 deg).
- [ ] Step 2.3: Implement `UpdateWallRun()` applying wall-parallel velocity and counteracting vertical gravity while attached.
- [ ] Step 2.4: Implement `ExecuteWallJump()` launching the player outward away from the wall normal on `Space`.
- [ ] Step 2.5: Verify compilation via Roslyn compiler.

#### Task 3: Procedural Camera Juice (Bobbing & Tilt)
- **Consumes:** `Assets/_Project/Scripts/Player/MouseLook.cs`
- **Produces:** Updated `MouseLook.cs` with head bobbing, wall-run roll, and slide roll
- [ ] Step 3.1: Add head bobbing parameters: `bobFrequency = 9.0f`, `bobAmplitude = 0.035f`, `bobSprintMultiplier = 1.35f`.
- [ ] Step 3.2: Implement procedural sinusoidal camera translation applied only when grounded and moving.
- [ ] Step 3.3: Implement smooth camera roll/tilt: +8 deg when wall-running right, -8 deg when wall-running left, -4 deg when sliding.
- [ ] Step 3.4: Verify compilation via Roslyn compiler.

---

### Parallel Batch 2: Traversal Ability Refinement

#### Task 4: Disable Grapple Traversal & Polish Prop Cannon
- **Consumes:** `Assets/_Project/Scripts/Abilities/PullAbility.cs`
- **Produces:** Updated `PullAbility.cs` without grapple latching, focused 100% on physics prop manipulation
- [ ] Step 4.1: Disable environmental grapple surface latching; set `isGrappleEnabled = false` and guard `ExecuteGrapple` so cable only interacts with portable objects.
- [ ] Step 4.2: Ensure tether line renders cleanly toward held props and pulls them smoothly into carry distance (1.5m).
- [ ] Step 4.3: Ensure held prop can be hurled at 28 m/s with Push for heavy kinetic crush damage.
- [ ] Step 4.4: Verify compilation via Roslyn compiler.

---

### Parallel Batch 3: Authoritative Health & Combat Feedback Pipeline

#### Task 5: Player Health & Damage Screen Overlay
- **Consumes:** `Assets/_Project/Scripts/Player/PlayerMovement.cs`
- **Produces:** `Assets/_Project/Scripts/Player/PlayerHealth.cs` and HUD damage overlay integration
- [ ] Step 5.1: Create `PlayerHealth.cs` with 100 max HP, attack ID deduplication, and events: `OnHealthChanged`, `OnDamaged`, `OnPlayerDied`, `OnPlayerRespawned`.
- [ ] Step 5.2: Implement procedural screen-space red flash vignette on damage.
- [ ] Step 5.3: Add camera shake impulse on damage via `MouseLook.AddScreenShake()`.
- [ ] Step 5.4: Implement clean death reset and safe checkpoint respawn.
- [ ] Step 5.5: Verify compilation via Roslyn compiler.

#### Task 6: Visual Feedback: Hit Flash & Floating Damage Numbers
- **Consumes:** `Assets/_Project/Scripts/Enemies/`
- **Produces:** `Assets/_Project/Scripts/Enemies/EnemyHitFlash.cs` and `Assets/_Project/Scripts/Enemies/FloatingDamageNumber.cs`
- [ ] Step 6.1: Create `EnemyHitFlash.cs` flashing material emission red/white for 0.1s upon taking damage.
- [ ] Step 6.2: Create `FloatingDamageNumber.cs` spawning 3D world-space text popups drifting upward with fade-out.
- [ ] Step 6.3: Add color coding: White for standard hits, Gold for crate/prop crush crits, Emerald for player heal.
- [ ] Step 6.4: Verify compilation via Roslyn compiler.

#### Task 7: Universal Enemy Health Integration
- **Consumes:** `Assets/_Project/Scripts/Enemies/SwarmerEnemy.cs`, `SniperEnemy.cs`, `SentryEnemy.cs`, `HulkLeaperEnemy.cs`
- **Produces:** Updated enemy scripts with full `IDamageReceiver` support, hit flash, and floating damage numbers
- [ ] Step 7.1: Ensure all enemies inherit/implement `TakeDamage(float amount, string source, int attackId)`.
- [ ] Step 7.2: Trigger `EnemyHitFlash` and `FloatingDamageNumber` on every damage instance.
- [ ] Step 7.3: Verify compilation via Roslyn compiler.

---

### Parallel Batch 4: Enemy Projectile & Combat Balancing

#### Task 8: Enemy Projectile Visuals & Speed Harmonization
- **Consumes:** `Assets/_Project/Scripts/Enemies/EnemyProjectile.cs`
- **Produces:** Updated `EnemyProjectile.cs` with bright emissive glow, line trail, and calibrated 18 m/s speed
- [ ] Step 8.1: Tune projectile speed from 32 m/s down to 18-20 m/s so player can dodge with parkour or deflect.
- [ ] Step 8.2: Attach or configure trail renderer with bright plasma glow.
- [ ] Step 8.3: Ensure projectile deals proper damage to `PlayerHealth` with unique attack ID.
- [ ] Step 8.4: Verify compilation via Roslyn compiler.

---

### Parallel Batch 5: Compact Egyptian Arena Generation & Verification

#### Task 9: Procedural Egyptian Arena Builder
- **Consumes:** `Assets/_Project/Prefabs/PyramidMaterials/`
- **Produces:** `Assets/_Project/Scripts/Editor/EgyptianArenaSetup.cs` creating a 45m x 45m Egyptian arena
- [ ] Step 9.1: Create `EgyptianArenaSetup.cs` editor menu command (`REBOUND / Build Egyptian Parkour Arena`).
- [ ] Step 9.2: Generate 45m x 45m sandstone floor and 8m perimeter walls with wall-run runnable colliders.
- [ ] Step 9.3: Generate central stepped pyramid (6m height) with jump ledges and 4 perimeter obelisks.
- [ ] Step 9.4: Procedurally place 30+ interactive physics props (Sandstone blocks, crates, explosive pots) with `InteractableProp` and `Rigidbody`.
- [ ] Step 9.5: Add enemy spawn nodes and safe player spawn point.
- [ ] Step 9.6: Verify compilation via Roslyn compiler.

#### Task 10: End-to-End System Verification
- **Consumes:** All created and modified files
- **Produces:** Complete verified build with zero errors
- [ ] Step 10.1: Run full Roslyn compiler test on both `Assembly-CSharp.dll` and `Assembly-CSharp-Editor.dll`.
- [ ] Step 10.2: Trigger arena generation in the active scene.
- [ ] Step 10.3: Confirm zero console errors in Unity `Editor.log`.
