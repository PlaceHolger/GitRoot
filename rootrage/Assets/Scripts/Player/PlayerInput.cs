using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [Header("Inputs")]
    public InputActionReference moveAction;
    [Header("Horizontal movement")] public float movementSpeed = 8f;
    private Vector3 _horizontalVelocity;


    private Vector2 _rawInput;
    private float _previousInputMagnitude = 0.0f;
    private float _currentSpeed;
    private Vector3 _previousInputVector;
    public float acceleration = 2f;
    public float deceleration = 8f;
    public float rotationSpeed = 4f;

    private Rigidbody _rigidbody;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        moveAction.action.Enable();
    }

    void Update()
    {
        _rawInput = moveAction.action.ReadValue<Vector2>();
        Vector3 MoveInput = new Vector3(_rawInput.x, 0.0f, _rawInput.y);

        float inputMagnitude = _rawInput.magnitude;

        float inputAcceleration = inputMagnitude > _previousInputMagnitude
            ? acceleration
            : deceleration;

        Vector3 lerpedInputVector = Vector3.Lerp(_previousInputVector, MoveInput, Time.deltaTime * inputAcceleration);
        float lerpedMagnitude = lerpedInputVector.magnitude;

        _currentSpeed = movementSpeed;
        _horizontalVelocity = lerpedInputVector * _currentSpeed;

        // Caching for next frame
        _previousInputMagnitude = lerpedMagnitude;
        _previousInputVector = lerpedInputVector;

        // Apply both components to find final frame velocity
        _rigidbody.velocity = _horizontalVelocity;

        if (inputMagnitude > 0f)
        {
            Quaternion newRotation = Quaternion.LookRotation(MoveInput);
            transform.rotation = Quaternion.Lerp(_rigidbody.rotation, newRotation, Time.deltaTime * rotationSpeed);
        }
    }
}
