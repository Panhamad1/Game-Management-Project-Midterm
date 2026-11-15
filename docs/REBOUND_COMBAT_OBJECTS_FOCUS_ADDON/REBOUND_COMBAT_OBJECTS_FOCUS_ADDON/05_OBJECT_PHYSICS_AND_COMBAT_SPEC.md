# REBOUND — Object Classes, Physics and Physical Combat Specification

## 1. Objective

Create a small, extensible physical-object system in which each object has a clear combat and interaction purpose. The system should support the existing Pull, Push, Crate Cannon, shielding, TargetButton, and enemy impact interactions.

Do not build a new inventory/crafting system. Do not create new levels or map content. Do not introduce a large set of object types.

## 2. Core object classes

The existing handoff provides three mass references. Verify actual prefab masses and retain them as starting values unless playtesting demonstrates a problem.

| Class | Existing reference mass | Pull behavior | Launch behavior | Main gameplay roles |
|---|---:|---|---|---|
| Light | ~1 kg | Fast lift/hold | Crate Cannon eligible | Quick projectile, small enemy hits, rapid interaction |
| Medium | ~5 kg | Normal lift/hold | Crate Cannon eligible | Main projectile, shield, exposed weak-point attack |
| Heavy | ~25 kg | Ground drag only | Not normal Crate Cannon ammunition | Cover, pressure weight, movable obstruction, ground-impact tool |

Mass is not the only property defining a class. Use a serialized profile or the project's current data-driven pattern.

## 3. Object profile fields

Use only fields that are useful to the current project. A suggested profile includes:

### Identity and capability
- Weight class: Light / Medium / Heavy.
- Can be pulled.
- Can be held.
- Can be ground-dragged.
- Can be launched by Crate Cannon.
- Can block projectiles.
- Can activate physical buttons/targets.
- Can be broken (optional).

### Physical tuning
- Rigidbody mass.
- Linear drag/damping.
- Angular drag/damping.
- Pull spring strength.
- Pull damping.
- Maximum pull force/acceleration.
- Maximum angular stabilization.
- Launch speed modifier.
- Player velocity inheritance modifier.
- Collision detection mode.
- Physics material/friction profile where required.

### Combat tuning
- Impact damage multiplier.
- Knockback multiplier.
- Minimum impact threshold.
- Maximum damage per impact.
- Weak-point multiplier is applied by the target, not permanently stored on the prop.
- Shield durability/resistance if implemented.

### Presentation
- Highlight color/state.
- Hold label.
- Launch/impact sound and VFX references.
- Break effect if applicable.

Avoid putting every possible value on every prefab if ScriptableObject profiles would be more maintainable. Follow the existing project's data architecture and do not introduce an elaborate asset pipeline unnecessarily.

## 4. Different physical roles

### Light
- Lowest inertia and easiest to pull.
- Fastest response to the Pull spring.
- Intended for quick, reactive throws.
- Lower impact energy at the same speed because of lower mass.
- May hit multiple light enemies if physics naturally allows; do not force penetration through all targets.
- Not a reliable long-duration shield unless current balance says otherwise.

### Medium
- Main all-purpose portable object.
- Strong enough to make the Crate Cannon a meaningful attack.
- Can be carried as a temporary shield against supported projectiles.
- Good candidate for attacking the Hulk's existing exposed weak point.
- Heavier/slower handling than Light.

### Heavy
- Cannot be lifted or held by the normal object Pull mode.
- Pull applies horizontal ground force toward a projected target point.
- Can block sightlines and projectiles when physically positioned as cover.
- Can be pushed or nudged, but its response should reflect its mass.
- Can interact with pressure plates/physical switches when the level's existing objects support it.
- Not launched by normal Crate Cannon.

## 5. Optional object traits

Only add these after the three core classes are stable and tested.

- **Fragile:** Breaks when impact energy exceeds a configurable threshold; may spawn a small, pooled set of fragments. Fragment count must be limited for performance.
- **Dense/Reinforced:** Higher impact multiplier or shield resistance, but also heavier/slower to pull.
- **Bouncy:** Higher restitution for ricochet-oriented puzzles or combat, only if a current use case exists.

Do not add explosive, elemental, crafting, or special ammunition systems as part of this pack unless already present in the project.

## 6. Physical impact calculation

Impact should be based on the object's relative velocity against the target, not just the object's world speed.

Let:
- `m` = projectile Rigidbody mass in kg.
- `vRel` = projectile velocity relative to target velocity in m/s.
- `n` = normalized collision contact normal.
- `vNormal = abs(dot(vRel, n))` = normal component of relative impact speed in m/s.
- `Eimpact = 0.5 * m * vNormal * vNormal` = normal impact energy in joules.

This is a useful impact-energy estimate, not a full material/fracture solver. It avoids treating a very glancing contact as a full head-on impact.

Suggested configurable damage form:

`damage = clamp((Eimpact - minimumImpactEnergy) * damagePerJoule * objectImpactMultiplier, 0, maxDamagePerImpact)`

Then apply the target's weak-point multiplier if the contact point intersects an explicitly configured weak-point collider/component.

Requirements:
- Keep damage thresholds, coefficient, multiplier, and cap in configurable data.
- Use the target's relative velocity if both bodies move.
- Use one collision impact event per target/projectile contact event.
- Do not apply damage from `OnCollisionStay` every physics frame.
- Maintain a per-projectile set or short cooldown of targets already hit so one contact cannot produce duplicate damage through multiple colliders.
- Deduplicate multiple child colliders belonging to one enemy.
- Consider whether the object should bounce, slow, break, or continue based on actual Unity physics and its profile.
- Do not guarantee that a projectile passes through multiple enemies; penetration is an optional trait/behavior, not a default assumption.

## 7. Collision and high-speed reliability

- Use an appropriate continuous collision mode for fast-moving Crate Cannon projectiles. `ContinuousDynamic` may be suitable for dynamic projectiles, but inspect performance and Unity version limitations.
- Ensure targets that must receive fast collisions have compatible collision detection settings.
- Check collider thickness and scale in existing prefabs; do not modify map geometry in this task.
- Use Rigidbody/Collider physics for actual object motion. Do not move dynamic objects by assigning Transform position every frame.
- Prevent duplicate callbacks caused by compound colliders.
- Use collision layers and masks; do not rely on GameObject names.
- Pool fragments/effects if breakage is implemented.

## 8. Shield durability and projectile interaction

If shield durability is added:
- It is stored on the object or its profile.
- It is reduced only by projectiles that are currently designed to be blocked.
- A blocked projectile produces one block event.
- When durability reaches zero, the object may break, become unusable as a shield, or remain as damaged cover depending on its profile.
- A medium object should provide meaningful but not unlimited protection unless intentionally balanced that way.

Do not make a shield absorb bullets through walls or when it is not physically between the player and the projectile.

## 9. TargetButton and physical switches

Preserve existing TargetButton behavior and event hookups. It should continue to support the project's current activation routes, including:
- Crate Cannon impact above its configured threshold.
- Player slide/ram above its configured threshold.
- Direct Push blast.

The exact method names and threshold values must be taken from current code, not copied blindly from an older handoff. Do not award Focus repeatedly for multiple colliders or repeated contact with the same already-activated button.

## 10. Performance and cleanup

- Cache components and profile references.
- Do not search the scene for all objects every frame.
- Avoid per-FixedUpdate allocations.
- Keep active physical debris limited.
- If a prop despawns or breaks while held, safely clear the player's held reference.
- On respawn/scene unload, restore all altered collision settings and object simulation settings.

## 11. Acceptance criteria

- Light, medium, and heavy objects behave differently in a visible and useful way.
- The player can immediately tell which objects are portable and which are ground-drag only.
- Thrown-object impact strength responds to both mass and relative impact speed.
- Damage is bounded and applied once per impact.
- Weak-point hits are based on actual configured colliders/components.
- Heavy objects are not accidentally lifted or launched by the normal Crate Cannon.
- Physical shielding works only when the object actually intercepts the projectile.
- The system performs consistently at the game's target frame rate.
