using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public sealed class HulkShockwave : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float radius = 14f;
    [SerializeField, Min(0.1f)] private float speed = 12f;
    [SerializeField, Min(0f)] private float damage = 30f;
    [SerializeField, Min(0f)] private float clearanceHeight = 0.8f;
    [SerializeField, Min(0f)] private float knockbackSpeed = 13f;
    [SerializeField, Min(0f)] private float launchSpeed = 9f;

    private const int Segments = 64;
    private LineRenderer ring;
    private Vector3 origin;
    private PlayerMovement player;
    private CapsuleCollider playerCapsule;
    private float distance;
    private float previousPlayerDistance;
    private bool crossedPlayer;

    private void Awake()
    {
        ring = GetComponent<LineRenderer>();
        ring.positionCount = Segments;
        ring.loop = true;
        ring.useWorldSpace = true;
        ring.enabled = false;
        enabled = false;
    }

    public void Emit(Vector3 point, PlayerMovement target)
    {
        origin = point;
        player = target;
        playerCapsule = player != null ? player.GetComponent<CapsuleCollider>() : null;
        distance = 0f;
        previousPlayerDistance = player != null ? HorizontalDistance() : float.PositiveInfinity;
        crossedPlayer = false;
        DrawRing();
        ring.enabled = true;
        enabled = true;
    }

    private void FixedUpdate()
    {
        float previousDistance = distance;
        distance = Mathf.Min(radius, distance + speed * Time.fixedDeltaTime);
        if (!crossedPlayer && playerCapsule != null)
        {
            float playerDistance = HorizontalDistance();
            if (previousPlayerDistance >= previousDistance && playerDistance <= distance)
            {
                crossedPlayer = true;
                float footHeight = playerCapsule.bounds.min.y - origin.y;
                if (footHeight >= -clearanceHeight && footHeight <= clearanceHeight)
                {
                    Vector3 outward = Vector3.ProjectOnPlane(player.Rb.position - origin, Vector3.up);
                    if (outward.sqrMagnitude < 0.001f)
                    {
                        outward = -player.transform.forward;
                    }
                    float strength = Mathf.Lerp(1f, 0.55f, playerDistance / radius);
                    player.ApplyEnemyKnockback((outward.normalized * knockbackSpeed + Vector3.up * launchSpeed) * strength);
                    player.GetComponent<SwarmerTestHealth>()?.TakeDamage(damage);
                }
            }
            previousPlayerDistance = playerDistance;
        }
        DrawRing();
        if (distance < radius) return;
        ring.enabled = false;
        enabled = false;
    }

    private float HorizontalDistance() =>
        Vector3.ProjectOnPlane(player.transform.position - origin, Vector3.up).magnitude;

    private void DrawRing()
    {
        for (int i = 0; i < Segments; i++)
        {
            float angle = 2f * Mathf.PI * i / Segments;
            ring.SetPosition(i, origin + new Vector3(Mathf.Cos(angle) * distance, 0.08f, Mathf.Sin(angle) * distance));
        }
    }
}
