using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float moveSpeed;
    public int patrolDestination;
    public Transform playerTransform;
    public bool isChasing;
    public float chaseDistance;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (playerTransform == null) return;

        if (!isChasing && Vector2.Distance(transform.position, playerTransform.position) < chaseDistance)
            isChasing = true;

        if (isChasing)
        {
            if (transform.position.x > playerTransform.position.x)
            {
                transform.localScale = new Vector3(2f, 2f, 2f);
                rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);
            }
            else if (transform.position.x < playerTransform.position.x)
            {
                transform.localScale = new Vector3(-2f, 2f, 2f);
                rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
            }
        }
        else
        {
            if (patrolDestination == 0)
            {
                rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);

                if (Mathf.Abs(transform.position.x - patrolPoints[0].position.x) < .2f)
                {
                    transform.localScale = new Vector3(2f, 2f, 2f);
                    patrolDestination = 1;
                }
            }
            else if (patrolDestination == 1)
            {
                rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);

                if (Mathf.Abs(transform.position.x - patrolPoints[1].position.x) < .2f)
                {
                    transform.localScale = new Vector3(-2f, 2f, 2f);
                    patrolDestination = 0;
                }
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            CharacterController player = col.gameObject.GetComponent<CharacterController>();
            bool fromRight = transform.position.x > col.transform.position.x;
            player.TakeKnockback(fromRight);
        }
    }
}