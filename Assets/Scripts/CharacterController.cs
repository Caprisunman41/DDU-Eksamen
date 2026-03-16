using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class CharacterController : MonoBehaviour
{
    [Header("References")]
    public ControllerStats MoveStats;
    [SerializeField] private Collider2D _bodyColl;

    private Rigidbody2D _rb;

    //movement variables
    private Vector2 _moveVelocity;
    private bool _isFacingRight;

    //collision check variables
    private RaycastHit2D _groundHit;
    private bool _isGrounded;

    private void Awake()
    {
        _isFacingRight = true;
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        CollisionChecks();

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

            Vector2 targetVelocity;

            if (InputManager.RunIsHeld)
            {
                targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxRunSpeed;
            }
            else
            {
                targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxWalkSpeed;
            }

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
        if (_isFacingRight && moveInput.x < 0)
        {
            Turn(false);
        }
        else if (!_isFacingRight && moveInput.x > 0)
        {
            Turn(true);
        }
    }

    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            _isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
        else
        {
            _isFacingRight = false;
            transform.Rotate(0f, 180f, 0f);
        }
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

        #region Debug Visualization

        if (MoveStats.DebugShowIsGroundedBox)
        {
            Color rayColor = _isGrounded ? Color.green : Color.red;

            Debug.DrawRay(
                new Vector2(capsuleCastOrigin.x - capsuleCastSize.x / 2, capsuleCastOrigin.y),
                Vector2.down * MoveStats.GroundDetectionRayLength,
                rayColor);

            Debug.DrawRay(
                new Vector2(capsuleCastOrigin.x + capsuleCastSize.x / 2, capsuleCastOrigin.y),
                Vector2.down * MoveStats.GroundDetectionRayLength,
                rayColor);

            Debug.DrawRay(
                new Vector2(capsuleCastOrigin.x - capsuleCastSize.x / 2, capsuleCastOrigin.y - MoveStats.GroundDetectionRayLength),
                Vector2.right * capsuleCastSize.x,
                rayColor);
        }

        #endregion
    }

    private void CollisionChecks()
    {
        IsGrounded();
    }

    #endregion
}