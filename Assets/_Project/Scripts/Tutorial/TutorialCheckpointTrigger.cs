using UnityEngine;

/// <summary>
/// Trigger volume placed at the entrance of each tutorial chamber.
/// Updates the player's respawn checkpoint and configures the HUD with that chamber's active mechanics.
/// </summary>
public class TutorialCheckpointTrigger : MonoBehaviour
{
    [Header("Chamber Configuration")]
    [SerializeField] private int chamberIndex = 1;
    [SerializeField] private string chamberTitle = "CHAMBER 1: LOCOMOTION & DASH-SLIDE";
    [TextArea(2, 4)]
    [SerializeField] private string chamberObjective = "Sprint forward [W+SHIFT] and press [CTRL / C] to slide under the low gate.";
    [SerializeField] private string chamberControls = "[W + SHIFT] Sprint  •  [SHIFT / Q] Dash  •  [CTRL / C] Slide";

    [Header("Respawn Anchor")]
    [SerializeField] private Transform respawnPoint;

    private bool hasTriggered = false;

    public bool HasTriggered => hasTriggered;

    private void Start()
    {
        if (respawnPoint == null)
        {
            respawnPoint = transform;
        }

        // Ensure collider is set to trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    public void SetChamberData(int index, string title, string objective, string controls, Transform spawn)
    {
        chamberIndex = index;
        chamberTitle = title;
        chamberObjective = objective;
        chamberControls = controls;
        respawnPoint = spawn;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        if (player != null)
        {
            Vector3 safeRespawn = respawnPoint != null ? respawnPoint.position : transform.position;
            player.SetCheckpoint(safeRespawn);

            if (TutorialHUDPrompt.Instance != null)
            {
                TutorialHUDPrompt.Instance.SetChamber(chamberIndex, chamberTitle, chamberObjective, chamberControls);
            }

            hasTriggered = true;
        }
    }
}
