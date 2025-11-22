using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public float MoveDirection { get; private set; }
    public bool JumpPressed { get; private set; }

    private PlayerInputs playerInputs;
    private PlayerController playerController;

    private void Awake()
    {
        if (playerInputs == null) playerInputs = new PlayerInputs();
        if (playerController == null) playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        playerInputs.Player.Enable();

        playerInputs.Player.Jump.started += OnJumpStarted;

        playerInputs.Player.Move.performed += OnMovePerformed;
        playerInputs.Player.Move.canceled += OnMoveCanceled;
    }
    private void OnDisable()
    {
        playerInputs.Player.Jump.started -= OnJumpStarted;

        playerInputs.Player.Move.performed -= OnMovePerformed;
        playerInputs.Player.Move.canceled -= OnMoveCanceled;

        playerInputs.Player.Disable();
    }

    private void LateUpdate()
    {
        JumpPressed = false;
    }

    private void OnJumpStarted(InputAction.CallbackContext context)
    {
        JumpPressed = true;
    }
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        MoveDirection = context.ReadValue<float>();
        playerController.IsWalking = true;
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        MoveDirection = 0f;
        playerController.IsWalking = false;
    }
}
