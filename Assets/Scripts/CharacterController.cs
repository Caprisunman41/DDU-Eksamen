using System;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class CharacterController : MonoBehaviour
{
    [Header("References")]
    public ControllerStats MoveStats;
    [SerializeField] private Collider2D _bodyColl;

    private Rigidbody2D _rb;

    // movement variables
    private Vector2 _moveVelocity;
    private bool _isFacingRight;

    // collision check variables
    private RaycastHit2D _groundHit;
    private bool _isGrounded;

    // head collision
    private RaycastHit2D _headHit;
    private bool _bumpHead;

    // jump variables
    public float VerticalVelocity { get; private set; }
    public bool _isJumping;
    public bool _isFastFalling;
    public bool _isFalling;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    private int _numberOfJumpsUsed;

    // apex variables
    public float _apexPoint;
    public float _timePastApexThreshold;
    public bool _isPastApexThreshold;

    // jump buffer
    private float _jumpBufferTimer;
    private bool _jumpReleasedDuringBuffer;

    // coyote time
    private float _coyoteTimer;

    private void Awake()
    {
        _isFacingRight = true;
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f; // FIX 1: Disable Unity's built-in gravity
    }

    private void Update()
    {
        CountTimers();
        JumpChecks();
    }

    private void FixedUpdate()
    {
        CollisionChecks();
        Jump();

        if (_isGrounded)
        {
            Move(MoveStats.GroundAcceleration, MoveStats.GroundDeceleration, InputManager.Movement);
        }
        else
        {
            Move(MoveStats.AirAcceleration, MoveStats.AirDeceleration, InputManager.Movement);
        }
    }

    #region Movement

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            TurnCheck(moveInput);

            Vector2 targetVelocity = InputManager.RunIsHeld
                ? new Vector2(moveInput.x, 0f) * MoveStats.MaxRunSpeed
                : new Vector2(moveInput.x, 0f) * MoveStats.MaxWalkSpeed;

            _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector2(_moveVelocity.x, _rb.linearVelocity.y);
        }
        else
        {
            _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector2(_moveVelocity.x, _rb.linearVelocity.y);
        }
    }

    private void TurnCheck(Vector2 moveInput)
    {
        if (_isFacingRight && moveInput.x < 0) Turn(false);
        else if (!_isFacingRight && moveInput.x > 0) Turn(true);
    }

    private void Turn(bool turnRight)
    {
        _isFacingRight = turnRight;
        transform.Rotate(0f, 180f, 0f);
    }

    #endregion

    #region Jump

    private void JumpChecks()
    {
        if (InputManager.JumpWasPressed)
        {
            _jumpBufferTimer = MoveStats.JumpBufferTime;
            _jumpReleasedDuringBuffer = false;
        }

        if (InputManager.JumpWasReleased)
        {
            if (_jumpBufferTimer > 0f)
                _jumpReleasedDuringBuffer = true;

            if (_isJumping && VerticalVelocity > 0f)
            {
                if (_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                    _isFastFalling = true;
                    _fastFallTime = MoveStats.TimeForUpwardsCancel;
                    VerticalVelocity = 0f;
                }
                else
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }

        if (_jumpBufferTimer > 0f && !_isJumping && (_isGrounded || _coyoteTimer > 0f))
        {
            InitiateJump(1);

            if (_jumpReleasedDuringBuffer)
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = VerticalVelocity;
            }
        }
        else if (_jumpBufferTimer > 0f && _isJumping && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed)
        {
            _isFastFalling = false;
            InitiateJump(1);
        }
        else if (_jumpBufferTimer > 0f && _isFalling && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed - 1)
        {
            InitiateJump(2);
            _isFastFalling = false;
        }

        if ((_isJumping || _isFalling) && _isGrounded && VerticalVelocity <= 0f)
        {
            _isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _fastFallTime = 0f;
            _isPastApexThreshold = false;
            _numberOfJumpsUsed = 0;

            VerticalVelocity = -2f; // FIX 3: Use small grounding value instead of Physics2D.gravity.y
        }
    }

    private void InitiateJump(int numberOfJumpsUsed)
    {
        _isJumping = true;
        _jumpBufferTimer = 0f;
        _numberOfJumpsUsed += numberOfJumpsUsed;
        VerticalVelocity = MoveStats.InitialJumpVelocity;
    }

    private void Jump()
    {
        // FIX 2: Apply gravity when falling naturally (not jumping or fast falling)
        if (!_isJumping && !_isFastFalling && !_isGrounded)
        {
            _isFalling = true;
            VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
        }

        if (_isJumping)
        {
            if (_bumpHead) _isFastFalling = true;

            if (VerticalVelocity >= 0f)
            {
                _apexPoint = Mathf.InverseLerp(MoveStats.InitialJumpVelocity, 0f, VerticalVelocity);

                if (_apexPoint > MoveStats.ApexThreshold)
                {
                    if (!_isPastApexThreshold)
                    {
                        _isPastApexThreshold = true;
                        _timePastApexThreshold = 0f;
                    }

                    if (_timePastApexThreshold < MoveStats.ApexHangTime)
                    {
                        VerticalVelocity = 0f;
                        _timePastApexThreshold += Time.fixedDeltaTime;
                    }
                    else
                    {
                        VerticalVelocity = -0.01f;
                    }
                }
                else
                {
                    VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
                    if (_isPastApexThreshold)
                    {
                        _isPastApexThreshold = false;
                    }
                }
            }
            else if (!_isFastFalling)
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseWithMultiplier * Time.fixedDeltaTime;
            }
        }

        if (_isFastFalling)
        {
            if (_fastFallTime >= MoveStats.TimeForUpwardsCancel)
                VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
            else
                VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f, _fastFallTime / MoveStats.TimeForUpwardsCancel);

            _fastFallTime += Time.fixedDeltaTime;
        }

        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MoveStats.MaxFallSpeed, 50f);
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, VerticalVelocity);
    }

    #endregion

    #region Collision Checks

    private void IsGrounded()
    {
        Vector2 capsuleCastOrigin = new Vector2(_bodyColl.bounds.center.x, _bodyColl.bounds.min.y);
        Vector2 capsuleCastSize = new Vector2(_bodyColl.bounds.size.x, MoveStats.GroundDetectionRayLength);

        _groundHit = Physics2D.CapsuleCast(
            capsuleCastOrigin,
            capsuleCastSize,
            CapsuleDirection2D.Horizontal,
            0f,
            Vector2.down,
            MoveStats.GroundDetectionRayLength,
            MoveStats.GroundLayer
        );

        _isGrounded = _groundHit.collider != null;
        Debug.Log("Grounded: " + _isGrounded);
        DebugGround(capsuleCastOrigin, capsuleCastSize);
        Debug.Log("Origin: " + capsuleCastOrigin + "  Size: " + capsuleCastSize);
        Debug.Log("Ground hit: " + (_groundHit.collider != null ? _groundHit.collider.name : "none"));
    }

    private void DebugGround(Vector2 origin, Vector2 size)
    {
        if (!MoveStats.DebugShowIsGroundedBox) return;

        Color rayColor = _isGrounded ? Color.green : Color.red;

        Debug.DrawRay(new Vector2(origin.x - size.x / 2, origin.y),
            Vector2.down * MoveStats.GroundDetectionRayLength, rayColor);

        Debug.DrawRay(new Vector2(origin.x + size.x / 2, origin.y),
            Vector2.down * MoveStats.GroundDetectionRayLength, rayColor);

        Debug.DrawRay(new Vector2(origin.x - size.x / 2, origin.y - MoveStats.GroundDetectionRayLength),
            Vector2.right * size.x, rayColor);
    }

    private void CollisionChecks()
    {
        IsGrounded();
    }

    #endregion

    #region Timers

    private void CountTimers()
    {
        _jumpBufferTimer -= Time.deltaTime;

        if (!_isGrounded)
            _coyoteTimer -= Time.deltaTime;
        else
            _coyoteTimer = MoveStats.JumpCoyoteTime;
    }

    #endregion
}
