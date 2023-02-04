using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [Header("Inputs")]
    public InputActionReference moveAction;
    public InputActionReference shootAction;

    public bool allowShoot = true;
    public UnityEvent OnShootButtonEvent;
    
    private Vector2 _rawInput;
    private Actions _characterController;
    //private Transform _cameraTransform;
    
    private void Awake()
    {
        _characterController = GetComponent<Actions>();
        //_cameraTransform = FindObjectOfType<Camera>().transform;
        moveAction.asset.Enable();
        shootAction.asset.Enable();
        shootAction.action.performed += ShootInputPerformed;
        shootAction.action.canceled += ShootInputCanceled;
    }

    private void Update()
    {
        // Move
        _rawInput = moveAction.action.ReadValue<Vector2>();
        Vector3 forward = Vector3.forward; //new Vector3( _cameraTransform.forward.x, 0, _cameraTransform.forward.z ).normalized;
        Vector3 right = Vector3.right; // new Vector3( _cameraTransform.right.x, 0, _cameraTransform.right.z ).normalized;
        _characterController.MoveInput = Vector3.right * _rawInput.x + Vector3.forward * _rawInput.y;
    }

    private void ShootInputPerformed(InputAction.CallbackContext context)
    {
        OnShootButtonEvent.Invoke();
        if(allowShoot)
            _characterController.TryShoot();
    }

    private void ShootInputCanceled(InputAction.CallbackContext context)
    {
        _characterController.InterruptShoot();
    }
}