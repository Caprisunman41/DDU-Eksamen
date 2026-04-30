using UnityEngine;
public class EnemyMovement : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float moveSpeed;
    public int patrolDestination;
    public Transform playerTransform;
    public bool isChasing;
    public float chaseDistance;
    public float stopChaseDistance = 8f;
    [Tooltip("Stop chasing when closer than this. 0 = chase all the way (melee)")]
    public float stopAtRange = 0f;
    public float spriteScale = 2.5f;
    public bool isKnockedBack = false;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (isKnockedBack) return;
        if (playerTransform == null) return;

        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (!isChasing && distToPlayer < chaseDistance)
            isChasing = true;
        if (isChasing && distToPlayer > stopChaseDistance)
        {
            isChasing = false;
            float distToPoint0 = Mathf.Abs(transform.position.x - patrolPoints[0].position.x);
            float distToPoint1 = Mathf.Abs(transform.position.x - patrolPoints[1].position.x);
            patrolDestination = distToPoint0 < distToPoint1 ? 0 : 1;
        }

        if (isChasing)
        {
            if (stopAtRange > 0f && distToPlayer <= stopAtRange)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                bool playerToRight = playerTransform.position.x > transform.position.x;
                transform.localScale = new Vector3(playerToRight ? -spriteScale : spriteScale, spriteScale, 1f);
            }
            else if (transform.position.x > playerTransform.position.x)
            {
                transform.localScale = new Vector3(spriteScale, spriteScale, 1f);
                rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);
            }
            else if (transform.position.x < playerTransform.position.x)
            {
                transform.localScale = new Vector3(-spriteScale, spriteScale, 1f);
                rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
            }
        }
        else
        {
            float targetX = patrolPoints[patrolDestination].position.x;
            float direction = targetX > transform.position.x ? 1f : -1f;
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
            transform.localScale = new Vector3(-direction * spriteScale, spriteScale, 1f);
            if (Mathf.Abs(transform.position.x - targetX) < 0.2f)
            {
                patrolDestination = patrolDestination == 0 ? 1 : 0;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        CharacterController player = col.gameObject.GetComponent<CharacterController>();
        PlayerHealth ph = col.gameObject.GetComponent<PlayerHealth>();

        if (player == null || ph == null) return;
        if (ph.IsInvincible) return;

        bool fromRight = transform.position.x > col.transform.position.x;
        player.TakeKnockback(fromRight);
    }
}
