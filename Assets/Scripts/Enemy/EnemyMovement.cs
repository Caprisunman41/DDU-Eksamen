using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Patrol")]
    public Transform[] patrolPoints;
    public int patrolDestination;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float acceleration = 8f;
    public float spriteScale = 2.5f;

    [Header("Detection")]
    public Transform playerTransform;
    public float chaseDistance = 6f;
    public float stopChaseDistance = 8f;
    [Tooltip("Stop chasing when closer than this. 0 = chase all the way (melee)")]
    public float stopAtRange = 0f;
    [Tooltip("Retreat range for ranged enemies. 0 = no retreat")]
    public float retreatRange = 0f;
    [Tooltip("Seconds before enemy reacts to player entering range")]
    public float reactionTime = 0.4f;
    [Tooltip("How often state decisions are re-evaluated (seconds)")]
    public float stateUpdateInterval = 0.1f;
    [Tooltip("Skip all logic if player is further than this")]
    public float sleepDistance = 25f;
    [Tooltip("Disable all contact effects (damage + knockback) — for ranged enemies")]
    public bool disableContactEffects = false;

    [HideInInspector] public bool isChasing;
    [HideInInspector] public bool isKnockedBack = false;

    private Rigidbody2D _rb;
    private float _reactionTimer;
    private float _currentVelocityX;
    private float _stateUpdateTimer;
    private float _cachedDistToPlayer;

    private enum State { Patrol, Chase, Hold, Retreat }
    private State _state = State.Patrol;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (isKnockedBack) return;
        if (playerTransform == null) return;

        // Sleep if player is very far - just patrol
        float sqrDist = ((Vector2)playerTransform.position - (Vector2)transform.position).sqrMagnitude;
        if (sqrDist > sleepDistance * sleepDistance)
        {
            if (_state != State.Patrol) { _state = State.Patrol; isChasing = false; }
            Patrol();
            return;
        }

        // Only re-evaluate state at intervals (not every physics step)
        _stateUpdateTimer -= Time.fixedDeltaTime;
        if (_stateUpdateTimer <= 0f)
        {
            _cachedDistToPlayer = Mathf.Sqrt(sqrDist);
            UpdateState(_cachedDistToPlayer);
            _stateUpdateTimer = stateUpdateInterval;
        }

        ExecuteState(_cachedDistToPlayer);
    }

    private void UpdateState(float distToPlayer)
    {
        switch (_state)
        {
            case State.Patrol:
                if (distToPlayer < chaseDistance)
                {
                    _reactionTimer += Time.fixedDeltaTime;
                    if (_reactionTimer >= reactionTime)
                    {
                        _state = State.Chase;
                        isChasing = true;
                        _reactionTimer = 0f;
                    }
                }
                else
                {
                    _reactionTimer = 0f;
                }
                break;

            case State.Chase:
                if (distToPlayer > stopChaseDistance)
                {
                    _state = State.Patrol;
                    isChasing = false;
                    ReturnToNearestPatrolPoint();
                }
                else if (retreatRange > 0f && distToPlayer < retreatRange)
                {
                    _state = State.Retreat;
                }
                else if (stopAtRange > 0f && distToPlayer <= stopAtRange)
                {
                    _state = State.Hold;
                }
                break;

            case State.Hold:
                if (distToPlayer > stopChaseDistance)
                {
                    _state = State.Patrol;
                    isChasing = false;
                    ReturnToNearestPatrolPoint();
                }
                else if (retreatRange > 0f && distToPlayer < retreatRange)
                {
                    _state = State.Retreat;
                }
                else if (distToPlayer > stopAtRange)
                {
                    _state = State.Chase;
                }
                break;

            case State.Retreat:
                if (distToPlayer > retreatRange * 1.2f)
                    _state = State.Hold;
                if (distToPlayer > stopChaseDistance)
                {
                    _state = State.Patrol;
                    isChasing = false;
                    ReturnToNearestPatrolPoint();
                }
                break;
        }
    }

    private void ExecuteState(float distToPlayer)
    {
        switch (_state)
        {
            case State.Patrol:
                Patrol();
                break;

            case State.Chase:
                MoveToward(playerTransform.position.x);
                break;

            case State.Hold:
                SetVelocityX(0f);
                FaceTarget(playerTransform.position.x);
                break;

            case State.Retreat:
                float retreatDir = transform.position.x > playerTransform.position.x ? 1f : -1f;
                MoveToward(transform.position.x + retreatDir * 10f);
                break;
        }
    }

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length < 2) return;
        float targetX = patrolPoints[patrolDestination].position.x;
        float dir = targetX > transform.position.x ? 1f : -1f;
        SetVelocityX(dir * moveSpeed);
        transform.localScale = new Vector3(-dir * spriteScale, spriteScale, 1f);
        if (Mathf.Abs(transform.position.x - targetX) < 0.2f)
            patrolDestination = patrolDestination == 0 ? 1 : 0;
    }

    private void MoveToward(float targetX)
    {
        float dir = targetX > transform.position.x ? 1f : -1f;
        SetVelocityX(dir * moveSpeed);
        FaceTarget(targetX);
    }

    private void SetVelocityX(float targetX)
    {
        _currentVelocityX = Mathf.MoveTowards(_currentVelocityX, targetX, acceleration * Time.fixedDeltaTime);
        _rb.linearVelocity = new Vector2(_currentVelocityX, _rb.linearVelocity.y);
    }

    private void FaceTarget(float targetX)
    {
        bool playerToRight = targetX > transform.position.x;
        transform.localScale = new Vector3(playerToRight ? -spriteScale : spriteScale, spriteScale, 1f);
    }

    private void ReturnToNearestPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length < 2) return;
        float d0 = Mathf.Abs(transform.position.x - patrolPoints[0].position.x);
        float d1 = Mathf.Abs(transform.position.x - patrolPoints[1].position.x);
        patrolDestination = d0 < d1 ? 0 : 1;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (disableContactEffects) return;
        CharacterController player = col.gameObject.GetComponent<CharacterController>();
        PlayerHealth ph = col.gameObject.GetComponent<PlayerHealth>();
        if (player == null || ph == null) return;
        if (ph.IsInvincible) return;
        bool fromRight = transform.position.x > col.transform.position.x;
        player.TakeKnockback(fromRight);
    }
}
