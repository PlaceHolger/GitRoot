using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [Header("Inputs")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference sprintAction;

    public bool allowJump = true;
    public UnityEvent OnJumpButtonEvent;
    
    private Vector2 _rawInput;
    private Move _characterController;
    //private Transform _cameraTransform;
    
    private void Awake()
    {
        _characterController = GetComponent<Move>();
        //_cameraTransform = FindObjectOfType<Camera>().transform;
        moveAction.asset.Enable();
        jumpAction.asset.Enable();
        sprintAction.asset.Enable();
        jumpAction.action.performed += JumpInputPerformed;
        jumpAction.action.canceled += JumpInputCanceled;
    }

    private void Update()
    {
        // Move
        _rawInput = moveAction.action.ReadValue<Vector2>();
        Vector3 forward = Vector3.forward; //new Vector3( _cameraTransform.forward.x, 0, _cameraTransform.forward.z ).normalized;
        Vector3 right = Vector3.right; // new Vector3( _cameraTransform.right.x, 0, _cameraTransform.right.z ).normalized;
        _characterController.MoveInput = right * _rawInput.x + forward * _rawInput.y;
        
        // Sprint
        _characterController.IsSprinting = sprintAction.action.IsPressed();
    }

    private void JumpInputPerformed(InputAction.CallbackContext context)
    {
        OnJumpButtonEvent.Invoke();
        if(allowJump)
            _characterController.TryJump();
    }

    private void JumpInputCanceled(InputAction.CallbackContext context)
    {
        _characterController.InterruptJump();
    }
}