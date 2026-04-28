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
    private Vector2 _smoothDampVelocity;
    private bool _isFacingRight;
    public bool IsFacingRight => _isFacingRight;

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

    // wall variables
    private bool _isTouchingWallRight;
    private bool _isTouchingWallLeft;
    private bool _isWallSliding;
    private float _wallJumpTimer;
    private float _wallSlideTimer;
    private bool _wallSlideExhausted;

	//dash variables
	private bool _isDashing;
	private bool _canDash = true;
	private float _dashTimer;
	private Vector2 _dashDirection;
    private float _dashCooldownTimer;
    private float _dashCurrentSpeed;
    public float DashCooldownProgress
    {
        get
        {
            if (_canDash && _dashCooldownTimer <= 0f) return 1f;
            return 1f - Mathf.Clamp01(_dashCooldownTimer / MoveStats.DashCooldown);
        }
    }
    
    //KB
    public float kbForce;
    public float kbCounter;
    public float kbTotalTime;

    public bool knockFromRight;


    private void Awake()
    {
        _isFacingRight = true;
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;

        PhysicsMaterial2D noFriction = new PhysicsMaterial2D { friction = 0f, bounciness = 0f };
        _bodyColl.sharedMaterial = noFriction;
    }

    private void Update()
    {
        CountTimers();
        JumpChecks();
		DashChecks();
    }

    private void FixedUpdate()
    {
        CollisionChecks();
        Dash();
        Jump();

        if (kbCounter > 0)
        {
            kbCounter -= Time.fixedDeltaTime;
            float t = 1f - Mathf.Clamp01(kbCounter / kbTotalTime);
            float kbX = knockFromRight ? -kbForce : kbForce;
            Vector2 kbVelocity = new Vector2(kbX, kbForce * 0.5f);
            _rb.linearVelocity = Vector2.Lerp(kbVelocity, Vector2.zero, t);
            _moveVelocity = new Vector2(_rb.linearVelocity.x, 0f);
        }
        else
        {
            if (_isGrounded)
                Move(MoveStats.GroundAcceleration, MoveStats.GroundDeceleration, InputManager.Movement);
            else
                Move(MoveStats.AirAcceleration, MoveStats.AirDeceleration, InputManager.Movement);
        }
    }

    #region Movement

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
		if (_isDashing) return;
		
        if (moveInput.magnitude > 0.01f)
        {
            if (!_isWallSliding) TurnCheck(moveInput);

            Vector2 targetVelocity = InputManager.RunIsHeld
                ? new Vector2(moveInput.x, 0f) * MoveStats.MaxRunSpeed
                : new Vector2(moveInput.x, 0f) * MoveStats.MaxWalkSpeed;

            _moveVelocity = Vector2.SmoothDamp(_moveVelocity, targetVelocity, ref _smoothDampVelocity, 1f / acceleration);
            _rb.linearVelocity = new Vector2(_moveVelocity.x, _rb.linearVelocity.y);
        }
        else
        {
            _moveVelocity = Vector2.SmoothDamp(_moveVelocity, Vector2.zero, ref _smoothDampVelocity, 1f / deceleration);
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

        if (InputManager.JumpWasPressed && _isWallSliding && (InventoryManager.Instance == null || InventoryManager.Instance.HasWallJump))
        {
            float wallDir = _isTouchingWallRight ? -1f : 1f;
            _moveVelocity = new Vector2(wallDir * MoveStats.WallJumpForce, 0f);
            _smoothDampVelocity = Vector2.zero;
            _wallJumpTimer = MoveStats.WallJumpLockoutTime;
            _wallSlideTimer = 0f;
            _numberOfJumpsUsed = 0;
            InitiateJump(1);
            return;
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
            _smoothDampVelocity = Vector2.zero;

            VerticalVelocity = -2f;
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
        if (_isDashing) return;

        bool pressingIntoWall = (_isTouchingWallRight && InputManager.Movement.x > 0.01f) ||
                                (_isTouchingWallLeft  && InputManager.Movement.x < -0.01f);

        bool touchingWall = _isTouchingWallRight || _isTouchingWallLeft;

        if (!touchingWall)
        {
            _wallSlideTimer = 0f;
            _wallSlideExhausted = false;
        }

        _isWallSliding = !_isGrounded && touchingWall && VerticalVelocity < 0f && _wallJumpTimer <= 0f && !_wallSlideExhausted;

        if (_isWallSliding)
        {
            if (_isTouchingWallRight && _isFacingRight) Turn(false);
            else if (_isTouchingWallLeft && !_isFacingRight) Turn(true);

            _wallSlideTimer += Time.fixedDeltaTime;

            if (_wallSlideTimer >= MoveStats.WallSlideMaxDuration)
            {
                _isWallSliding = false;
                _wallSlideExhausted = true;
            }
            else
            {
                VerticalVelocity = Mathf.Max(VerticalVelocity, -MoveStats.WallSlideSpeed);
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, VerticalVelocity);
                return;
            }
        }

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
		if (!_isDashing)
        	_rb.linearVelocity = new Vector2(_rb.linearVelocity.x, VerticalVelocity);
    }

    #endregion

	#region Dash

    private void DashChecks()
    {
        if (InputManager.DashWasPressed && _canDash && _dashCooldownTimer <= 0f && (InventoryManager.Instance == null || InventoryManager.Instance.HasDash))
        {
            _isDashing = true;
            _canDash = false;
            _dashTimer = MoveStats.DashDuration;
            _dashCooldownTimer = MoveStats.DashCooldown;
            _dashCurrentSpeed = 0f;
            
			Vector2 input = InputManager.Movement;

			if (input.magnitude <= 0.01f)
			{
    			_dashDirection = new Vector2(_isFacingRight ? 1f : -1f, 0f);
			}
			else
			{
    			float x = Mathf.Abs(input.x) > 0.3f ? Mathf.Sign(input.x) : 0f;
    			float y = Mathf.Abs(input.y) > 0.3f ? Mathf.Sign(input.y) : 0f;


    			_dashDirection = new Vector2(x, y).normalized;
			}

				VerticalVelocity = 0f;
		}
    }

    private void Dash()
    {
        if (_isDashing)
        {
            _dashTimer -= Time.fixedDeltaTime;

            float rampRate = MoveStats.DashSpeed / (MoveStats.DashDuration * 0.25f);
            _dashCurrentSpeed = Mathf.MoveTowards(_dashCurrentSpeed, MoveStats.DashSpeed, rampRate * Time.fixedDeltaTime);
            _rb.linearVelocity = _dashDirection * _dashCurrentSpeed;

            if (_dashTimer <= 0f)
            {
                _isDashing = false;
                _moveVelocity = new Vector2(_dashDirection.x * (_dashCurrentSpeed * 0.5f), 0f);
                _smoothDampVelocity = Vector2.zero;
            }
            return;
        }

        if (_isGrounded)
            _canDash = true;
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
        DebugGround(capsuleCastOrigin, capsuleCastSize);
    }
	private void BumpedHead()
	{
    	Vector2 castOrigin = new Vector2(_bodyColl.bounds.center.x, _bodyColl.bounds.max.y);
    	Vector2 castSize = new Vector2(_bodyColl.bounds.size.x * MoveStats.HeadWidth, MoveStats.HeadDetectionRayLength);

    	_headHit = Physics2D.CapsuleCast(
        	castOrigin,
        	castSize,
        	CapsuleDirection2D.Horizontal,
        	0f,
        	Vector2.up,
        	MoveStats.HeadDetectionRayLength,
        	MoveStats.GroundLayer
    	);

    	_bumpHead = _headHit.collider != null;
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
        BumpedHead();
        DetectWalls();
    }

    private void DetectWalls()
    {
        float dist = _bodyColl.bounds.extents.x + MoveStats.WallDetectionRayLength;
        Vector2 upper = new Vector2(_bodyColl.bounds.center.x, _bodyColl.bounds.max.y - 0.1f);
        Vector2 lower = new Vector2(_bodyColl.bounds.center.x, _bodyColl.bounds.min.y + 0.1f);

        _isTouchingWallRight = Physics2D.Raycast(upper, Vector2.right, dist, MoveStats.GroundLayer) ||
                               Physics2D.Raycast(lower, Vector2.right, dist, MoveStats.GroundLayer);
        _isTouchingWallLeft  = Physics2D.Raycast(upper, Vector2.left,  dist, MoveStats.GroundLayer) ||
                               Physics2D.Raycast(lower, Vector2.left,  dist, MoveStats.GroundLayer);
    }

    #endregion

    #region Timers

    private void CountTimers()
    {
        _jumpBufferTimer -= Time.deltaTime;
        if (_dashCooldownTimer > 0f)
            _dashCooldownTimer -= Time.deltaTime;
        if (_wallJumpTimer > 0f)
            _wallJumpTimer -= Time.deltaTime;

        if (!_isGrounded)
            _coyoteTimer -= Time.deltaTime;
        else
            _coyoteTimer = MoveStats.JumpCoyoteTime;
    }
    
    #endregion
    
    #region Knockback
    public void TakeKnockback(bool fromRight)
    {
        knockFromRight = fromRight;
        kbCounter = kbTotalTime;
    }
    
    #endregion
}
