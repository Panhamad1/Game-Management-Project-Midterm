using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Central input controller for the player.
/// Reads hardware inputs using the new Unity Input System and dispatches them
/// to PlayerMovement, MouseLook, and Ability components.
/// </summary>
public class PlayerAbilityInput : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private MouseLook mouseLook;
    [SerializeField] private PushAbility pushAbility;
    [SerializeField] private PullAbility pullAbility;
    [SerializeField] private SlowMotionAbility slowMotionAbility;

    [Header("Input Options")]
    [Tooltip("If true, Slow Motion toggles on/off with Shift/Q/F. If false, it requires holding.")]
    [SerializeField] private bool toggleSlowMotion = true;

    private void Awake()
    {
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (mouseLook == null) mouseLook = GetComponentInChildren<MouseLook>();
        if (pushAbility == null) pushAbility = GetComponentInChildren<PushAbility>();
        if (pullAbility == null) pullAbility = GetComponentInChildren<PullAbility>();
        if (slowMotionAbility == null) slowMotionAbility = GetComponentInChildren<SlowMotionAbility>();
    }

    private void Update()
    {
        HandleLocomotionInput();
        HandleLookInput();
        HandleAbilityInput();
        HandleCursorState();
    }

    private void HandleLocomotionInput()
    {
        if (movement == null) return;

        Vector2 moveVector = Vector2.zero;
        var keyboard = Keyboard.current;

        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) moveVector.y += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) moveVector.y -= 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveVector.x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveVector.x += 1f;

            movement.SetMoveInput(moveVector);

            // Sprint / Run (Hold Left Shift)
            bool isSprintPressed = keyboard.leftShiftKey.isPressed;
            movement.SetSprintInput(isSprintPressed);

            // Crouch / Slide (C or Left Ctrl)
            bool isCrouchPressed = keyboard.cKey.isPressed || keyboard.leftCtrlKey.isPressed;
            movement.SetCrouchInput(isCrouchPressed);

            // Jump & Hold-Space Auto-Bunny-Hop (Redliner style)
            bool isJumpHeld = keyboard.spaceKey.isPressed;
            movement.SetJumpHoldInput(isJumpHeld);

            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                movement.Jump();
            }

            // Quick Checkpoint Reset (R Key)
            if (keyboard.rKey.wasPressedThisFrame)
            {
                movement.RespawnAtCheckpoint();
            }
        }
    }

    private void HandleLookInput()
    {
        if (mouseLook == null) return;

        var mouse = Mouse.current;
        if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 delta = mouse.delta.ReadValue();
            mouseLook.SetLookInput(delta);
        }
        else
        {
            mouseLook.SetLookInput(Vector2.zero);
        }
    }

    private void HandleAbilityInput()
    {
        var mouse = Mouse.current;
        var keyboard = Keyboard.current;

        // 1. PUSH (Left Click) & CRATE CANNON (Left Click while Pull is holding an object)
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            if (pullAbility != null && pullAbility.IsCarrying)
            {
                // Crate Cannon combo launch
                pullAbility.TryLaunchHeldObject();
            }
            else if (pushAbility != null)
            {
                pushAbility.TryPush();
            }
        }

        // 2. PULL (Right Click Hold)
        if (mouse != null && pullAbility != null)
        {
            if (mouse.rightButton.wasPressedThisFrame)
            {
                pullAbility.StartPull();
            }
            else if (mouse.rightButton.wasReleasedThisFrame)
            {
                pullAbility.StopPull();
            }
        }

        // 3. SLOW MOTION (Q, F, or Middle Mouse Click)
        if (slowMotionAbility != null)
        {
            bool triggerPressed = false;
            bool triggerReleased = false;

            if (keyboard != null)
            {
                if (keyboard.qKey.wasPressedThisFrame || keyboard.fKey.wasPressedThisFrame) triggerPressed = true;
                if (keyboard.qKey.wasReleasedThisFrame || keyboard.fKey.wasReleasedThisFrame) triggerReleased = true;
            }
            if (mouse != null)
            {
                if (mouse.middleButton.wasPressedThisFrame) triggerPressed = true;
                if (mouse.middleButton.wasReleasedThisFrame) triggerReleased = true;
            }

            if (toggleSlowMotion)
            {
                if (triggerPressed)
                {
                    if (slowMotionAbility.IsActive)
                        slowMotionAbility.DeactivateSlowMotion();
                    else
                        slowMotionAbility.ActivateSlowMotion();
                }
            }
            else
            {
                if (triggerPressed)
                {
                    slowMotionAbility.ActivateSlowMotion();
                }
                else if (triggerReleased)
                {
                    slowMotionAbility.DeactivateSlowMotion();
                }
            }
        }
    }

    private void HandleCursorState()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;

        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            if (mouseLook != null) mouseLook.UnlockCursor();
        }

        if (mouse != null && mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
        {
            if (mouseLook != null) mouseLook.LockCursor();
        }
    }
}
