//using Coherence;
//using Coherence.Toolkit;

using Unity.Netcode.Components;
using UnityEngine;

public class Actions : MonoBehaviour
{
    public Animator animator;
    public NetworkAnimator networkAnimator;
    public RootCreator rootCreator;

    [field: Header("Inputs")] public Vector3 MoveInput { get; set; }

    [Header("Horizontal movement")] public float movementSpeed = 8f;
    public float runMultiplier = 1.3f;
    public float rotationSpeed = 4f;
    public float acceleration = 2f;
    public float airAcceleration = .5f;
    public float deceleration = 8f;
    public float airDeceleration = 0;

    [Header("Jump")] public float weight = 2f;
    public float maxFallSpeed = 1;

    [Header("Spring")] public float cruiseHeight = .5f;
    public float raycastLength = .8f;
    public float dampenFactor = 20f;
    public LayerMask walkableLayers;

    private bool _isGrounded;
    private float _currentSpeed;
    private Rigidbody _rigidbody;
    //private CoherenceSync _sync;
    private Vector3 _horizontalVelocity;
    private Vector3 _verticalVelocity;
    private Vector3 _previousInputVector;
    private float _previousInputMagnitude;
    private float _timeSinceGrounded = 0;
    private bool _isFalling = true;
    private bool _isShooting = false;
    private bool _isStunned = false;
    
    public bool IsStunned
    {
        get => _isStunned;
        set => _isStunned = value;
    }
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        //_sync = GetComponent<CoherenceSync>();
    }

    // private void Update()
    // {
    // }

    private void FixedUpdate()
    {
        Vector3 velocity = _rigidbody.velocity;

        VerticalMovement(velocity);

        // Horizontal movement
        float inputMagnitude = MoveInput.magnitude;

        float inputAcceleration = inputMagnitude > _previousInputMagnitude
            ?
            _isGrounded ? acceleration : airAcceleration
            :
            _isGrounded
                ? deceleration
                : airDeceleration;

        Vector3 lerpedInputVector = Vector3.Lerp(_previousInputVector, MoveInput, Time.deltaTime * inputAcceleration);
        float lerpedMagnitude = lerpedInputVector.magnitude;

        _currentSpeed = (_isShooting || _isStunned) ? 0 : movementSpeed;
        _horizontalVelocity = lerpedInputVector * _currentSpeed;

        // Caching for next frame
        _previousInputMagnitude = lerpedMagnitude;
        _previousInputVector = lerpedInputVector;

        // Cap vertical velocity when falling 
        if (_isFalling && _verticalVelocity.magnitude > maxFallSpeed)
            _verticalVelocity = _verticalVelocity.normalized * maxFallSpeed;

        // Apply both components to find final frame velocity
        _rigidbody.velocity = _horizontalVelocity + _verticalVelocity;
        _verticalVelocity = !_isGrounded ? Vector3.Project(_rigidbody.velocity, Vector3.up) : Vector3.zero;

        if (inputMagnitude > 0f /*&& !_isShooting*/)
        {
            Quaternion newRotation = Quaternion.LookRotation(MoveInput);
            transform.rotation = Quaternion.Lerp(_rigidbody.rotation, newRotation, Time.deltaTime * rotationSpeed);
        }

        UpdateAnimatorParameters();
    }

    private void VerticalMovement(Vector3 velocity)
    {
        // Vertical movement
        float rayLength = _isGrounded ? raycastLength : cruiseHeight;

        // Check for ground
        bool wasGrounded = _isGrounded;
        Ray ray = new(transform.position, Vector3.down);
        _isGrounded = Physics.Raycast(ray, out RaycastHit raycastHit, rayLength, walkableLayers,
            QueryTriggerInteraction.Ignore);

        if (!_isGrounded)
        {
            ApplyGravity();
        }
    }

    private void UpdateAnimatorParameters()
    {
        // This is used to blend between the Walk and Run animation clips in the Animator
        float animationSpeed = (_currentSpeed != 0) ? _horizontalVelocity.magnitude / _currentSpeed : 0;

        animator.SetFloat("MoveSpeed", animationSpeed);
    }

    private void ApplyGravity()
    {
        _verticalVelocity += Vector3.up * (-weight * Time.deltaTime);
        _timeSinceGrounded += Time.deltaTime;
    }

    public void TryShoot()
    {
        bool isSomethingInMeleeRange = rootCreator.IsSomethingInMeleeRange();
        if (!isSomethingInMeleeRange)
        {
            _isShooting = true;
            animator.SetBool("isShooting", true);
            Shoot();
        }
        else
        {
            if(networkAnimator)
                networkAnimator.SetTrigger("Attack");
            else
                animator.SetTrigger("Attack");
        }
    }

    public void InterruptShoot()
    {
        if (_isShooting)
        {
            _isShooting = false;
            animator.SetBool("isShooting", false);
        }
    }

    private void Shoot()
    {
        if(networkAnimator)
            networkAnimator.SetTrigger("Shoot");
        else
            networkAnimator.SetTrigger("Shoot");
        //_sync.SendCommand<Animator>(nameof(Animator.SetTrigger), MessageTarget.Other, "Shoot");
    }

    public void ApplyActions(bool shoot, Vector2 moveDir)
    {
        if (shoot) TryShoot();
        MoveInput = Vector3.right * moveDir.x + Vector3.forward * moveDir.y;
    }

    public void Reset()
    {
        InterruptShoot();
    }
}