using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls physical Egyptian mastery gates between tutorial chambers.
/// Unlocks and smoothly retracts when the chamber's required physical action or combat condition is met.
/// </summary>
public class TutorialMasteryGate : MonoBehaviour, IKineticTarget
{
    [Header("Chamber Attribution")]
    [SerializeField] private int chamberIndex = 1;

    public int ChamberIndex => chamberIndex;

    [Header("Unlock Conditions")]
    [SerializeField] private bool unlockOnPlayerSlide = false;
    [SerializeField] private bool unlockOnKineticImpact = false;
    [SerializeField] private bool unlockOnEnemiesDefeated = false;
    [SerializeField] private List<GameObject> requiredEnemies = new List<GameObject>();

    [Header("Gate Motion")]
    [SerializeField] private Vector3 openOffset = new Vector3(0f, -5.5f, 0f);
    [SerializeField] private float openSpeed = 4.0f;

    private Vector3 closedPosition;
    private Vector3 targetPosition;
    private bool isUnlocked = false;

    public bool IsUnlocked => isUnlocked;

    private void Start()
    {
        closedPosition = transform.position;
        targetPosition = closedPosition;
    }

    private void Update()
    {
        if (unlockOnEnemiesDefeated && !isUnlocked && requiredEnemies.Count > 0)
        {
            CheckEnemyStatus();
        }

        if (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, openSpeed * Time.deltaTime);
        }
    }

    private void CheckEnemyStatus()
    {
        bool anyAlive = false;
        for (int i = 0; i < requiredEnemies.Count; i++)
        {
            if (requiredEnemies[i] != null && requiredEnemies[i].activeInHierarchy)
            {
                anyAlive = true;
                break;
            }
        }

        if (!anyAlive)
        {
            UnlockGate();
        }
    }

    public void UnlockGate()
    {
        if (isUnlocked) return;
        isUnlocked = true;
        targetPosition = closedPosition + openOffset;

        if (TutorialHUDPrompt.Instance != null)
        {
            TutorialHUDPrompt.Instance.SetChamberCompleted(true);
        }

        // Play crunch/stone scraping sound if audio listener exists
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    public void AddRequiredEnemy(GameObject enemy)
    {
        if (enemy != null && !requiredEnemies.Contains(enemy))
        {
            requiredEnemies.Add(enemy);
            unlockOnEnemiesDefeated = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isUnlocked) return;

        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        if (player != null)
        {
            if (unlockOnPlayerSlide && (player.IsSliding || player.IsDashing))
            {
                UnlockGate();
            }
        }
    }

    // IKineticTarget Implementation
    public bool CanBePulled => false;
    public bool CanBePushed => unlockOnKineticImpact;

    public void OnPushed(Vector3 forceDirection, float forceMagnitude, Vector3 hitPoint)
    {
        if (unlockOnKineticImpact)
        {
            UnlockGate();
        }
    }

    public void OnPulled(Transform pullOrigin, float pullForce) { }

    public void OnPhysicsCrush(float impactSpeed, GameObject flyingProp)
    {
        if (unlockOnKineticImpact)
        {
            UnlockGate();
        }
    }

    public void OnKineticImpact(in KineticImpactContext context)
    {
        if (unlockOnKineticImpact)
        {
            UnlockGate();
        }
    }

    public void OnPlayerSlideRam(float impactSpeed)
    {
        if (unlockOnPlayerSlide && impactSpeed >= 5.0f)
        {
            UnlockGate();
        }
    }
}
