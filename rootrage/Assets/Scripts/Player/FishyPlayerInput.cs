using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class FishyPlayerInput : NetworkBehaviour
{
    [Header("Inputs")]
    public InputActionReference moveAction;
    public InputActionReference shootAction;

    public UnityEvent OnShootButtonEvent;
    public UnityEvent OnShootButtonReleasedEvent;
    
    private Vector2 _rawInput;
    private Actions _characterController;
    //private Transform _cameraTransform;
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
        {
            gameObject.GetComponent<FishyPlayerInput>().enabled = false;
            Destroy(gameObject.GetComponent<FishyPlayerInput>());
        }
    }

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
        if (!_characterController.IsStunned)
        {
            _characterController.TryShoot();
            OnShootButtonEvent.Invoke();
        }
    }

    private void ShootInputCanceled(InputAction.CallbackContext context)
    {
        OnShootButtonReleasedEvent.Invoke();
        _characterController.InterruptShoot();
    }
}