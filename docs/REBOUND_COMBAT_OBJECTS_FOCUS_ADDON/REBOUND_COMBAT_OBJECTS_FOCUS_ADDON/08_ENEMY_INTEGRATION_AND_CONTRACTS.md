# REBOUND — Enemy Integration and Physics Interaction Contracts

## 1. Scope boundary

The enemy roster and enemy behavior are not being redesigned in this task. Preserve the existing enemy AI, navigation, attack timing, telegraphs, health values, and encounter logic.

The task is to make existing enemies respond correctly to the player's Push, Pull (where currently allowed), Crate Cannon, slide ram, and projectile deflection systems.

## 2. Existing enemy/hazard inventory to preserve

### Small fast Swarmer
Keep its existing pack behavior, movement, leap/attack, and health. Integrate only the currently intended physical responses:
- Eligible Push knockback or interruption.
- Slide ram/bowling response.
- Crate Cannon impact damage.
- Pull only if the current enemy is explicitly configured as pullable/lightweight.

### Heavy Hulk Leaper
Keep its current leap, predicted landing, slam, shockwave, and stun recovery. Preserve its heavy/resistant status:
- Do not allow ordinary Pull to lift it.
- Do not allow ordinary Push to move it substantially unless current design already permits a small stagger.
- Keep armor/body resistance.
- Apply the existing weak-point multiplier only when the exposed weak-point collider is actually hit.
- Medium Crate Cannon projectile remains the intended strong weak-point attack if that is already configured.

### Sniper
Keep current aiming, laser lock, charge time, firing cadence, damage, and placement behavior. The player's new interaction is:
- Eligible sniper projectile can be deflected by Push during the valid window.
- Reflected projectile should be attributed so it can damage the original sniper.
- Do not make every sniper attack automatically return to sender.

### Rapid-Laser Sentry
Keep existing burst length, firing cadence, target tracking, and damage. New/verified interactions:
- Eligible bolts collide with a held object before player when the object physically intercepts the shot.
- Preserve the existing flanking and Crate Cannon damage routes.
- Do not alter sentry AI or fire pattern.

### Laser Tripwire hazard
Preserve activation and damage behavior. Do not redesign its placement or geometry. Focus affects a moving tripwire only if its current implementation belongs to a time-scaled simulation system; test and document.

## 3. Contract version mismatch — mandatory audit

The supplied documentation contains two different `IKineticTarget` shapes.

### Newer player subsystem specification

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

### Older team handoff

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

These signatures are not interchangeable. The actual current repository is authoritative.

The agent must:
1. Locate the real interface.
2. Find all implementers, callers, and test doubles.
3. Choose one consistent contract or keep the existing contract and add an adapter/context object.
4. Update all implementations and call sites atomically if migrating.
5. Compile the project and verify every receiver.
6. Do not leave a mix of old and new signatures.

## 4. Recommended impact context (optional modernization)

If the current interface is too limited for the required damage and weak-point work, consider a small immutable context structure rather than adding many positional parameters:

```csharp
public struct KineticImpactContext
{
    public GameObject SourceObject;
    public Vector3 Point;
    public Vector3 Normal;
    public Vector3 Direction;
    public float RelativeImpactSpeed;
    public float ImpactEnergy;
    public bool HitWeakPoint;
}
```

A possible receiver method is:

```csharp
void OnKineticImpact(in KineticImpactContext context);
```

This is a design option, not a mandate. Use it only if it simplifies the actual project. If adopted, migrate all callers and implementers in one compile-safe change and preserve adapters for existing gameplay events.

## 5. Weak-point detection

- Do not detect weak points by GameObject name strings.
- Use an explicit component, serialized collider reference, or dedicated layer as the project's architecture permits.
- Determine weak-point status from the actual contact collider/contact point.
- Apply the multiplier once.
- If the weak point is not exposed/active according to the current Hulk state, do not apply the weak-point bonus.
- Keep the existing enemy vulnerability window and stun state unchanged.

## 6. Projectile deflection contract

If current projectile scripts do not expose a safe method, add a small interface or adapter such as:

```csharp
public interface IDeflectableProjectile
{
    bool CanBeDeflected { get; }
    bool TryDeflect(Vector3 newDirection, GameObject newOwner);
}
```

The actual signature may follow project style. Requirements:
- One successful deflection only by default.
- Update owner/faction/team so reflected shots can hit their former source.
- Prevent immediate self-hit and infinite reflection.
- Preserve projectile damage and collision behavior unless current rules require a change.
- Do not deflect non-projectile hazards or unapproved attacks.

## 7. Enemy damage and status rules

- Do not create a parallel enemy health system.
- Reuse the existing damage receiver/health component.
- Apply impact damage through one authoritative path.
- Do not apply both `TakeDamage` and an interface callback that internally calls `TakeDamage` for the same collision.
- Physical push/knockback and damage should be separate explicit outputs, each applied once.
- Preserve immunities and heavy-target resistance.
- Do not allow ordinary props to bypass intended boss/armor states without weak-point eligibility.

## 8. Player ram / slide collision

The old and new documentation also disagree on the player-ram callback name and whether a direction vector is included.

Use the current real project signature. Ensure:
- One collision produces one ram event.
- The current slide-speed threshold is read from one authoritative setting.
- Light enemies can receive the intended ram response.
- Heavy Hulk behavior remains its existing special rule (player may be stopped/penalized as currently designed).
- The player's movement controller owns any velocity loss or collision response; enemy receivers should not independently overwrite player Rigidbody velocity.

## 9. Acceptance criteria

- Enemy AI and attack behavior remain unchanged.
- Each enemy receives only the physical interactions configured for its archetype.
- Weak-point hits are detected by collider/component, not names.
- Projectile reflection updates ownership and cannot loop forever.
- Interface signatures compile across all implementers and callers.
- No duplicate damage or duplicate impulses occur from compound colliders.
