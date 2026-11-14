# REBOUND — Enemy Design & Implementation Plan

> **Document purpose:** A practical guide for the teammate building the enemies in Unity. **Build gameplay first; visuals can stay as cubes, capsules, and spheres.** All health, damage, range, speed, and cooldown values should be editable in the Unity Inspector instead of being hardcoded.
>
> **Status:** Design and implementation plan — **not** a statement that the features have been coded or tested.
>
> **Source:** `TEAM_DEVELOPMENT_HANDBOOK.md` (REBOUND). The handbook is the latest technical reference provided to this document. Where this document recommends a simpler first implementation, it is explicitly labeled **MVP**. It does not replace the team's final decisions.

## 1. The big picture

**REBOUND** is a **3D first-person, high-speed physics-action and movement game**. There are **no conventional player guns** in the handbook. The player attacks with Wind Push, object Pull/Grab, launched crates, high-speed slides, and deflection. The player can use Slow Motion and fast movement to avoid danger.

**Environment theme:** Our team plans to use a **pyramid-style map/design**. For the enemy developer, this means enemies should function correctly in pyramid-themed chambers, corridors, ramps, stairways, elevated ledges, and open arenas. This is a **level theme**, not a new enemy mechanic. The exact visual style, room layout, and enemy placement are the level designer's responsibility.

**Our enemy goal:** Enemies must detect the player, attack according to their type, receive physics-based attacks, die, and notify the stage/level system. They should also work when the player is sliding, airborne, grappling, on a wall, or using Slow Motion.

### Four enemy types

| ID | Enemy | Core behavior | Player's intended counter |
|---|---|---|---|
| E1 | **Small Fast Swarmer** | Fast small crawler; moves along floors/walls and leaps at close range | High-speed sliding impact, Crate Cannon, and (per handbook) Vacuum Grab |
| E2 | **Heavy Hulk Leaper** | Slow, tanky ground enemy; leaps, lands, sends ground shockwave, then exposes a weak point | Jump/grapple away from shockwave; throw a medium crate into its back weak point during stun |
| E3 | **Slow Deadly Sniper** | Stationary/elevated long-range shooter; shows laser lock, then fires one powerful projectile | Break line of sight; dodge or Push-deflect the projectile |
| E4 | **Fast Rapid-Laser Sentry** | Stationary ranged turret; fires rapid shots in a burst, then pauses | Carry a crate as a shield or flank while turret pauses |

> **Important naming detail:** The handbook calls E3 **Sniper** and E4 **Rapid-Laser Sentry**. The earlier conversation called them “single-shot gun enemy” and “burst turret.” We use the handbook's names here.

## 2. Non-goals — do NOT build these first

- Detailed enemy models, skeletal animation, polished VFX, sound design, ragdolls, or complex boss cinematics.
- Multiplayer or network synchronization.
- An advanced AI behavior tree or a large custom navigation system.
- Multiple separate maps or multiple scenes for enemy logic.
- Fixed damage values that require code changes to rebalance.
- Direct edits to the teammate's player movement scripts.

**Minimum playable version (MVP):** Each of the four enemies can be represented by simple 3D shapes, and all four can attack, take damage, and be defeated in a dedicated test scene.

## 3. Shared enemy architecture

### 3.1 Suggested scripts

| File | Purpose | Shared or specific? |
|---|---|---|
| `EnemyHealth.cs` | Health, damage, death, notify stage | All enemy types |
| `EnemyTargeting.cs` | Player reference, distance, line-of-sight check | All enemy types |
| `EnemyPhysicsReceiver.cs` | Implement team `IKineticTarget` interface; push/pull/crush/ram responses | All, configured per enemy |
| `EnemyProjectile.cs` | Move projectile, detect collision, deal damage once, destroy after lifetime | Sniper and sentry (with settings) |
| `SwarmerEnemy.cs` | Pursue, leap and close-range attack | E1 |
| `HulkEnemy.cs` | Ground pursuit, melee, jump/slam state sequence | E2 |
| `HulkShockwave.cs` | Show expanding shockwave and damage at most once per slam | E2 |
| `SniperEnemy.cs` | Visible aim/laser lock, warning, single shot, reload | E3 |
| `SentryEnemy.cs` | Rotate toward player, burst fire and pause | E4 |

*These are suggested filenames, not mandatory names.* Keep the scripts small; avoid duplicating `TakeDamage()` code in four different enemy scripts.

### 3.2 Unity GameObject layout (example)

```text
EnemyTestScene
├── Floor
├── Walls
├── TestPlayer                 (temporary capsule)
├── Enemies
│   ├── Swarmer_Prefab
│   ├── Hulk_Prefab
│   │   ├── Body
│   │   ├── WeakPoint          (back collider)
│   │   └── ShockwaveVisual
│   ├── Sniper_Prefab
│   │   ├── AimOrigin
│   │   ├── Muzzle
│   │   └── LaserLine          (LineRenderer)
│   └── Sentry_Prefab
│       ├── FixedBase
│       ├── RotatingHead
│       └── Muzzle
├── Projectiles
└── GameManager_Test
```

### 3.3 Setup conventions

- Make each enemy a **Prefab** and put it on the `Enemies` layer (handbook: **Layer 12**).
- Use a `Collider` for its body and a `Rigidbody` when needed for the team's physics interface. Configure constraints so movement doesn't cause uncontrolled tipping.
- Assign the player **by `Transform` reference** for test scenes; integration can use a central player reference later.
- Set a separate `Muzzle` Transform for projectiles; never spawn bullets inside the turret's own collider.
- Give projectiles the `Projectiles` layer (handbook: **Layer 13**) and exclude the shooter from self-collisions.
- Check line of sight using a ray from the enemy's attack origin to an aim point on the player; **walls must block enemy shots**.
- Configure values in Inspector using `[SerializeField]` (speed, detection range, attack range, cooldown, projectile speed, damage, health).
- Prefer deterministic, visible attack warnings over invisible instant damage.

## 4. Player/team code contracts — coordinate before integrating

The provided handbook defines the following interface. **Reuse the exact interface that exists in your Unity repository**; do not create a conflicting duplicate.

```csharp
public interface IKineticTarget
{
    bool CanBePulled { get; }
    bool CanBePushed { get; }
    void OnPushed(Vector3 forceDirection, float forceMagnitude, Vector3 hitPoint);
    void OnPulled(Transform pullOrigin, float pullForce);
    void OnPhysicsCrush(float impactSpeed, GameObject flyingProp, bool hitWeakPoint);
    void OnPlayerRam(float playerSpeed, Vector3 ramDirection);
}
```

The handbook also supplies a draft `EnemyPhysicsReceiver.cs`. Use the teammate's actual file if it already exists; otherwise adapt that draft. **Do not assume the handbook's checked checklist proves it is already implemented.**

**Need agreement with player teammate:**

1. **Player reference:** PlayerRoot `Transform`, a known Player tag/layer, and a consistent aim target (e.g. chest/head Transform).
2. **Damage receiver:** A public method or interface such as `TakeDamage(float amount)` (exact signature to confirm). Enemy code must not directly edit the player UI or regeneration logic.
3. **Ground/air state for shockwave:** A reliable way to know whether the player is above the ground, OR use a geometric height check without depending on the player implementation.
4. **Warning HUD:** A method/event enemies can invoke for Sniper lock / Hulk slam if Spider Sense is already available.
5. **Slow Motion:** Agree whether enemy behavior uses `Time.deltaTime`, `Time.fixedDeltaTime`, scaled coroutines, or a custom time service. Under `Time.timeScale = 0.25`, a world-time attack should slow naturally; avoid mixing unscaled timers accidentally.
6. **Enemy death callback:** Stage manager interface/event (e.g. `OnEnemyDefeated`) so doors open only when the required enemies in that stage are gone.
7. **Physics attack collisions:** Who calls `OnPhysicsCrush` and `OnPlayerRam`? Avoid both player and enemy scripts applying damage for the same impact.

**Design difference to confirm:** The handbook says small enemies **can be Vacuum Grabbed**, while an earlier team conversation said **enemy Pull/Push was removed**. The enemy developer can make pullability a configurable bool and leave it off until the team confirms the final rule. Hulk is never pullable in either direction of the plan.

## 5. Enemy E1 — Small Fast Swarmer

### Goal

A small agile threat that pressures a fast-moving player. The handbook describes packs of **4–8**, floor/wall crawling, a leap when within **5m**, and vulnerability to slide rams, launched crates, and Vacuum Grab.

### MVP behavior (build this first)

```text
Idle
  -> player detected -> Chase
Chase
  -> player close enough & attack ready -> Leap/Attack
Leap/Attack
  -> hit once or miss -> Cooldown
Cooldown
  -> return to Chase
Any state
  -> health <= 0 -> Dead
```

**Beginner implementation:** Start with a capsule or sphere that follows the player's horizontal position on a flat floor and performs a simple leap/close-range attack. After it works, add wall crawling (more difficult because it requires handling surface normals, orientation, and corners). Wall crawling is in the handbook, but **should not block the initial testable enemy**.

### Inspector values to expose

`maxHealth`, `moveSpeed`, `detectionRange`, `attackRange`, `leapForce`, `attackCooldown`, `damage`, `canBePulled`, `canBePushed`, `ramSpeedThreshold`.

### Definition of done

- [ ] Detects the test player without detecting through solid walls (unless your team explicitly wants hearing/awareness).
- [ ] Moves toward the player's current position without jittering at close range.
- [ ] Attacks only within range and after cooldown.
- [ ] Can damage the test player **once per attack**, not once per frame.
- [ ] Can be damaged by a thrown crate / approved physics attack.
- [ ] Dies at zero HP and notifies stage manager.
- [ ] **Later:** floor-and-wall traversal works in the final pyramid level.

## 6. Enemy E2 — Heavy Hulk Leaper

### Goal

A slow heavy opponent that cannot be pulled. It has a powerful, **avoidable leap + ground shockwave**, then a **2.5-second vulnerable stun** that exposes a glowing core on its back (from the handbook).

### Attack state machine

```text
Chase / Basic Melee
  -> special skill cooldown ready
Windup / Warning        (show clear cue, optional Spider Sense)
  -> Jump Up
  -> Fall / Land
  -> Spawn Expanding Shockwave
  -> Stunned / Back Weak Point Exposed (2.5s)
  -> Return to Chase
```

### Shockwave hit logic

- Spawn a visible expanding ring centered on the Hulk's **landing point**.
- The ring expands over time. Determine whether/when the ring reaches the player using **horizontal distance** from landing point.
- Only damage if the player is low enough to be hit by a *ground* wave; jumping/grappling higher than the clearance height should avoid it.
- Each shockwave has a unique attack ID or a `HashSet` of already-hit targets so it **cannot damage the same player twice**.
- Direct landing contact and the expanding wave must not accidentally deal two separate damages for one slam unless the team explicitly chooses that rule.
- The player can also evade by moving outside the wave radius.

**MVP visual:** Move a cube/capsule up and down without animation. Use a transparent expanding cylinder/ring to show the wave. Proper jump physics and polished VFX can come later.

### Weak point

- Create child `WeakPoint` on Hulk's back with its own collider.
- Enable the weak point during the stun window.
- Thrown props hitting the weak point trigger `OnPhysicsCrush(..., hitWeakPoint: true)`.
- The handbook's draft physics receiver applies a **3× weak-point damage multiplier**. Keep it configurable.

### Inspector values to expose

`maxHealth`, `walkSpeed`, `meleeRange`, `meleeDamage`, `meleeCooldown`, `slamCooldown` (initial concept: ~6 seconds), `windupTime`, `jumpHeight`, `shockwaveRadius`, `shockwaveSpeed`, `shockwaveDamage`, `shockwaveClearanceHeight`, `stunDuration` (handbook 2.5s), `weakPointMultiplier`, `canBePulled=false`.

### Definition of done

- [ ] Walks toward test player slowly.
- [ ] Basic melee attack has distance/cooldown check.
- [ ] Cannot be pulled.
- [ ] Slam has a visible warning, rise, landing, and expanding wave.
- [ ] Player can jump over or move outside shockwave and take **no damage**.
- [ ] Player in the wave takes damage **only once**.
- [ ] Hulk enters vulnerable state after slam; returns to normal after stun.
- [ ] Weak point receives special damage from a thrown crate.
- [ ] Dies cleanly and reports defeat.

## 7. Enemy E3 — Slow Deadly Sniper

### Goal

Make the player use cover, movement, Slow Motion, and Push deflection. The handbook says the sniper tracks with a visible **red laser lock for 1.5 seconds** before firing one high-speed sonic round, and that the projectile can be deflected back to destroy the sniper.

### Attack sequence

```text
Idle / Search
 -> player in range & visible
Aim / Red Laser Lock (1.5s)
 -> Spider Sense warning shortly before firing
 -> Fire ONE projectile
 -> Reload / Cooldown
 -> Search and repeat
```

**Fairness rule:** If the player hides behind a wall before the lock completes, cancel or break the lock (or make that behavior configurable). Projectiles should collide with walls and cover.

**Deflection:** Give the projectile a public method/event to reverse ownership and direction when hit by the player's Push ability. The returned projectile must be able to damage the sniper without damaging the player who deflected it. This must be coordinated with the player teammate.

### Inspector values to expose

`maxHealth`, `detectionRange`, `aimDuration=1.5`, `shotCooldown`, `projectileSpeed`, `projectileDamage`, `lineOfSightMask`, `warningLeadTime`, `canProjectileBeDeflected`.

### Definition of done

- [ ] Detects visible player at range and aims at player.
- [ ] Shows visible laser while locking.
- [ ] Waits the aim time before firing **one** projectile.
- [ ] Stops or loses lock if cover blocks view (per agreed design).
- [ ] Projectile hits wall or player only once, then disappears.
- [ ] Projectile can be deflected and damage sniper.
- [ ] Sends threat-warning event if Spider Sense integration is available.
- [ ] Dies and reports defeat.

## 8. Enemy E4 — Rapid-Laser Sentry

### Goal

Provide pressure with bursts while letting the player use cover, flanking, or a carried crate shield.

**Handbook detail:** The sentry fires **rapid lasers for 3 seconds**, then pauses. This differs from an earlier proposal of exactly *three shots* and a 1–2-second pause. Use the **3-second burst** interpretation unless the team chooses otherwise.

### Attack sequence

```text
Search -> Visible player -> Aim
Aim -> Fire Burst (multiple bolts during burst duration)
Fire Burst -> Pause
Pause -> Search / Aim -> repeat
```

### Inspector values to expose

`maxHealth`, `detectionRange`, `turnSpeed`, `burstDuration=3`, `shotsPerSecond`, `burstPause`, `projectileSpeed`, `projectileDamage`, `aimSpread`, `lineOfSightMask`.

### Damage handling

- Each projectile deals configured damage **once** upon actual hit.
- Respect player health script's damage protections if they exist; do not manually remove HP from a separate UI.
- A carried crate with a collider should block shots just like world cover. This requires correct collision layers and the player's crate-shield implementation.
- Do not make the turret shoot through walls or its own body.

### Definition of done

- [ ] Stays fixed; rotates head toward a visible player.
- [ ] Shoots multiple bolts during a configurable burst duration.
- [ ] Stops shooting for a configurable pause.
- [ ] Bolts collide correctly with walls, carried crates, and player.
- [ ] Applies damage only on confirmed hits.
- [ ] Dies and reports defeat.

## 9. Damage is configurable — implementation first

We are **not locking numeric damage now**. Instead, expose parameters on each prefab, such as:

```csharp
[SerializeField] private float maxHealth;
[SerializeField] private float damage;
[SerializeField] private float attackCooldown;
[SerializeField] private float detectionRange;
```

The player team now plans **100 HP with automatic regeneration outside recent damage**, but the enemy does **not** control player max HP, healing, UI, or Game Over. Enemy code only sends a damage request via the agreed player damage API.

**Avoid hard-coded damage in `OnTriggerStay`:** One projectile or melee attack should trigger a damage call once, with a cooldown or hit flag. Regeneration is owned by the player's health script.

**Do not assume 100 HP integration already exists:** Use a temporary `TestPlayerHealth` receiving damage and printing current HP until the real player component is ready.

## 10. Pyramid map placement (suggested, not finalized)

The pyramid is our **chosen environment theme**. Suggested fitting encounter ideas:

| Pyramid location | Enemy use | Space/cover considerations |
|---|---|---|
| Entry chamber | One easy ranged threat or swarmer introduction | Simple floor and cover; let player learn controls |
| Narrow corridor / sloped ramp | Rapid sentry | Pillars or crates to block lasers; room to slide under obstacles |
| Elevated ledge / inner chamber | Sniper | Long sightline, stone pillars as cover; meaningful deflection path |
| Large central/final chamber | Hulk plus optional other enemies | Open space to avoid shockwave; reachable back weak point; grappling/jump escape routes |

**These are encounter ideas, not a confirmed new stage count or enemy count.** Keep the final distribution with the level designer; don't secretly change the existing one-map/three-stage plan.

## 11. Solo development while player is unfinished

Create a separate `EnemyTestScene.unity` so enemy work does not break the team scene.

**Temporary setup:**

1. Create a Plane floor, 4 Cube walls, and a directional light.
2. Create a `TestPlayer` Capsule (Player tag/layer; collider; optional basic Rigidbody or movement script).
3. Attach `TestPlayerHealth.cs` with a configurable starting HP and public test damage method.
4. Add a simple temporary WASD move script **only if needed for tests**; do not merge it over the teammate's real player controller.
5. Place ONE enemy prefab at a time, not four together.
6. Use `Debug.Log` first to verify detect → aim → attack → damage → cooldown → death.
7. Add a wall and verify visibility blocking.
8. Simulate player jumping with a test object at elevated Y-position for the shockwave test.
9. Simulate Slow Motion with `Time.timeScale = 0.25f`; restore it to `1f` when ending the test.
10. Verify crate impacts with a Rigidbody cube and collision detection enabled.

### Testing matrix

| Scenario | Expected result |
|---|---|
| Player out of range | Enemy does not attack |
| Player in range but behind solid wall | Enemy cannot shoot through wall |
| Player in range and visible | Enemy performs type-specific attack |
| Player quickly changes height | Enemy aim / flying behavior remains stable |
| Melee enemy reaches player | One damage event per completed attack |
| Projectile hits wall | Stops/despawns; player not damaged |
| Projectile hits player | Damage receiver called once |
| Player hides behind carried crate | Blocked projectile doesn't hit player |
| Player uses Slow Motion | Enemy and projectile behavior slows consistently |
| Swarmer receives crate hit/ram | Correct physics damage and death handling |
| Hulk tries to be pulled | Pull rejected |
| Hulk wave meets airborne player | No hit when above wave clearance |
| Hulk wave meets grounded player | Hit at most once for that slam |
| Sniper round is deflected | Changes direction/owner and can damage sniper |
| Enemy health reaches zero | Stops attacking and reports defeat exactly once |
| Player leaves scene / respawns | Enemies don't spam null-reference errors |

## 12. Detailed implementation backlog (in order)

### Phase A — Setup

- [ ] Create `Assets/Scenes/EnemyTestScene.unity` and save the scene.
- [ ] Create folders: `Assets/Scripts/Enemies`, `Assets/Scripts/Projectiles`, `Assets/Prefabs/Enemies`, `Assets/Prefabs/Projectiles`, `Assets/Materials/Enemies`, `Assets/Tests` (if using tests).
- [ ] Check Project Settings → Tags and Layers: `Player`, `Environment`, `Interactables`, `Enemies`, `Projectiles`, etc.; coordinate layer numbers from handbook.
- [ ] Add `TestPlayer` Capsule and `TestPlayerHealth` in the test scene.
- [ ] Add floor, wall and cover box.
- [ ] Test that entering Play Mode has no Console errors.
- [ ] Make a Git commit for the test environment.

### Phase B — Shared systems

- [ ] Locate/reuse `IKineticTarget.cs` in project; do not duplicate it.
- [ ] Create or adapt `EnemyHealth.cs` and expose `maxHealth`.
- [ ] Implement `TakeDamage()` and one-time death logic.
- [ ] Add a defeat callback for future StageManager connection.
- [ ] Create or adapt `EnemyPhysicsReceiver.cs` with configurable pull/push/ram/crush settings.
- [ ] Add `EnemyTargeting.cs`: player reference, distance and line of sight.
- [ ] Create `EnemyProjectile.cs`: velocity, collisions, lifetime, shooter ignore, damage event.
- [ ] Test damage and death using debug buttons or a thrown test crate.
- [ ] Commit shared enemy systems.

### Phase C — Rapid Sentry (start here)

- [ ] Create `Sentry_Prefab`: base, rotating head, muzzle, collider and components.
- [ ] Create simple projectile prefab and material.
- [ ] Detect player, aim and stop aim when blocked.
- [ ] Implement burst loop and pause, both Inspector configurable.
- [ ] Instantiate projectile from muzzle with direction toward player aim target.
- [ ] Test shot hits wall, crate and TestPlayer.
- [ ] Test timeScale/slow motion effect.
- [ ] Commit working sentry prefab/scripts.

### Phase D — Sniper

- [ ] Create `Sniper_Prefab`, muzzle and visible LineRenderer laser.
- [ ] Reuse shared target detection / projectile.
- [ ] Implement target lock duration, attack warning and cooldown.
- [ ] Fire single high-speed projectile.
- [ ] Add deflection entry point to projectile, coordinate with player team.
- [ ] Test line-of-sight cancel, dodge and deflection.
- [ ] Commit working sniper prefab/scripts.

### Phase E — Swarmer

- [ ] Create `Swarmer_Prefab` with capsule/body, collider and physics settings.
- [ ] Implement flat-ground chase first.
- [ ] Stop at safe attack range; add cooldown.
- [ ] Add leap / close-range attack; damage once per attack.
- [ ] Connect `OnPlayerRam()` and crate impact behavior.
- [ ] Test when player moves very quickly or is elevated.
- [ ] **If time:** implement surfaces/wall crawling from handbook, with slope/corner tests.
- [ ] Commit working swarmer prefab/scripts.

### Phase F — Hulk

- [ ] Create `Hulk_Prefab` with large body and collider.
- [ ] Configure heavy, slow movement and `CanBePulled = false`.
- [ ] Implement basic attack with cooldown.
- [ ] Implement warning → leap → landing → shockwave → stun state sequence.
- [ ] Create ring visual; implement expanding wave hit checks.
- [ ] Test ground dodge, jump dodge, and no repeated damage.
- [ ] Create back weak-point child collider and stun vulnerability.
- [ ] Test launched crate damage against normal body versus weak point.
- [ ] Commit working Hulk prefab/scripts.

### Phase G — Integration

- [ ] Replace TestPlayer reference with real PlayerRoot and confirmed player damage API.
- [ ] Match team collision layers/masks and actual player physics settings.
- [ ] Connect Spider Sense warnings for sniper/hulk if ready.
- [ ] Connect physics push/pull/ram/crush callbacks to player ability scripts.
- [ ] Confirm Slow Motion does not break cooldowns, projectiles or leaps.
- [ ] Connect `EnemyDefeated` callback to StageManager and locked doors.
- [ ] Place enemy prefabs in pyramid level according to designer's plan.
- [ ] Test all enemies with slide, jump, grapple, object carry, and crate shield.
- [ ] Remove TestPlayer from production scene, keep EnemyTestScene for debugging.
- [ ] Commit, open PR or merge using team's Git process.

### Phase H — Quality and submission

- [ ] Test every enemy in a standalone Windows/macOS build, not only in Editor.
- [ ] Verify no Console errors during a full start → play → win/lose → restart loop.
- [ ] Ensure enemies reset on restart and cannot report defeat twice.
- [ ] Record short video clips of each enemy demonstrating its attack and counterplay.
- [ ] Document scripts, tuning values, prefab locations, and your contribution in README.
- [ ] Credit external enemy assets, tutorials, and AI help as the course rules require.

## 13. What counts as 'finished' for YOUR enemy task?

Your enemy work is ready for teammate integration when:

1. All 4 enemy prefabs exist and function with primitive shapes.
2. Each enemy detects/attacks the temporary player with working range and cooldown logic.
3. Each attack sends a configurable amount to the agreed player damage receiver.
4. Objects, rams, and deflected shots interact with enemies through the documented contract.
5. The Hulk's shockwave is telegraphed and avoidable, not an unavoidable hit.
6. Enemies die and fire **one** defeat notification for stage progression.
7. Tested in `EnemyTestScene` and no blocking Console errors remain.
8. Teammates know which prefab to drag into the pyramid level and which Inspector fields to configure.

---

### Final reminders

- **Function first; graphics later.** A working capsule swarmer is better than a beautifully modeled enemy that doesn't attack.
- **Damage numbers are not final.** Build flexible Inspector settings and let the team balance during playtesting.
- **Coordinate before changing gameplay contracts.** The handbook and earlier conversation disagree on some Pull/Grapple details; do not silently decide for another developer.
- **MVP before advanced features.** Especially for wall crawling, physics-heavy leaps, and projectile reflection, get the simplest working version first.
