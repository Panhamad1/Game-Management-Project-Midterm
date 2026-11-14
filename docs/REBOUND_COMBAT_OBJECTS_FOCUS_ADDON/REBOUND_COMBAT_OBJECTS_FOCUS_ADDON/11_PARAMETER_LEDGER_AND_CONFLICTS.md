# REBOUND — Parameter Ledger and Documentation Conflicts

## 1. Purpose

This ledger separates existing REBOUND reference values from proposed tuning values and unresolved conflicts. The coding agent must inspect current Unity code/prefabs before changing any value.

**Do not blindly copy a number from this document into code.** The actual project is the source of truth.

## 2. Existing REBOUND references from the supplied player subsystem

| System | Reference value | Units | Status / notes |
|---|---:|---|---|
| Player Rigidbody mass | 75 | kg | Existing rig specification; verify prefab |
| Push range | 12 | m | Existing reference |
| Push target impulse | 18 | N·s | Existing reference; ensure API uses impulse units |
| Push surface proximity | 3.8 | m | Existing reference |
| Push kick velocity | 13.5 | m/s | Existing reference formula |
| Slide Push boost | +10 | m/s | Existing reference; movement owner must apply |
| Air rocket jump limit | 1 per airborne window | count | Existing reference |
| Object acquisition range | 16 | m | Existing reference |
| Grapple traversal range | 75 | m | Movement pack reference; not redesigned here |
| Normal held-object distance | 2.5 | m | Existing reference |
| Narrow-space held distance | 1.2 | m | Older team handoff reference |
| Light prop mass | ~1 | kg | Existing reference |
| Medium prop mass | ~5 | kg | Existing reference |
| Heavy prop mass | ~25 | kg | Existing reference; ground drag only |
| Crate Cannon launch component | 35 | m/s | Existing reference; definition of total vs additive velocity must be audited |
| Crate Cannon player recoil | 6.5 | m/s | Existing reference |
| Trajectory prediction segments | 20 | points/steps | Existing reference |
| TargetButton crate impact threshold | 4 | m/s | Older handoff; verify current code |
| Slide/ram activation threshold | 5.5 or 7.5 | m/s | Conflicting docs; current code is authoritative |
| Focus time scale | 0.25 | ratio | Existing reference |
| Focus resource | Infinite | — | Existing design; proposed to change to rechargeable |

## 3. Suggested Focus starting profile

These are provisional playtest values, not final balance:

| Parameter | Proposed start | Units |
|---|---:|---|
| Capacity | 100 | units |
| Drain | 25 | units / unscaled second |
| Recovery delay | 0.75 | unscaled seconds |
| Recovery | 15 | units / unscaled second |
| Minimum activation | 5 | units |
| Projectile deflection reward | +15 | units |
| Weak-point impact reward | +10 | units |
| Objective reward | +15 to +25 | units |

Tune based on the length of actual encounters and observed average time spent in Focus. Ensure a player can use Focus multiple times in a normal encounter without making it effectively unlimited.

## 4. Suggested object-profile tuning fields

Do not use unverified fixed force constants. Expose:
- Spring strength.
- Spring damping.
- Maximum pull force/acceleration.
- Hold-point offset/distance.
- Maximum angular stabilization.
- Ground drag force.
- Launch eligibility.
- Launch speed modifier.
- Player velocity inheritance factor.
- Impact threshold/energy coefficient/cap.
- Knockback multiplier.
- Shield durability/resistance.
- Break threshold and debris count if breakable.

Start from existing prefab values and tune using test cases. A stable spring-damper response should be mass-aware and clamped.

## 5. Known documentation conflicts

### A. `IKineticTarget` signature

Newer player spec:
- `OnPhysicsCrush(float impactSpeed, GameObject flyingProp)`
- `OnPlayerSlideRam(float slideSpeed)`

Older team handoff:
- `OnPhysicsCrush(float impactSpeed, GameObject flyingProp, bool hitWeakPoint)`
- `OnPlayerRam(float playerSpeed, Vector3 ramDirection)`

Resolve by inspecting the actual interface and all implementers. Update atomically or use an adapter/context object.

### B. Focus input

- Newer player spec: Q/F toggle.
- Older handoff: Left Shift listed in one table, but Q/F elsewhere.
- Shift may now be used by the movement upgrade.

Actual Input Actions and latest code decide. Do not rebind without permission.

### C. Focus resource

- Existing new player spec: infinite slow motion.
- Older handoff: TargetButton refills Focus energy.
- This add-on recommends rechargeable Focus.

Inspect actual implementation. Migrate to a resource meter only after understanding any existing recharge/reward behavior. Keep rewards once per event.

### D. Slide/ram thresholds

The documents cite 5.5 m/s, 7.5 m/s, and other conditions for different slide/ram systems. Do not merge them into one magic number. Find actual call sites and distinguish slide eligibility from enemy ram eligibility and TargetButton activation.

### E. Crate Cannon launch velocity

The 35 m/s value is not documented as either total world speed or aim-directed speed added to inherited player velocity. Inspect current code and choose/document one definition.

### F. Pull range and target masks

The current player subsystem describes 16 m for object interaction and 75 m for grapple traversal. Verify actual layers/masks and target priority. Do not rely on stale numeric layer IDs.

## 6. Tuning process

For each parameter:
1. Record current value from code/prefab.
2. Record the source/document that introduced it.
3. Change one category at a time.
4. Run the corresponding validation tests.
5. Record the tested build and result.
6. Keep only changes that improve behavior without breaking movement or combat readability.

Do not leave unexplained magic numbers. Use named serialized fields or shared settings assets according to the project's existing style.
