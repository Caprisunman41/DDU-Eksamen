using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 4f;
    public float damage = 0.5f;
    public float lifetime = 8f;

    private Vector2 _direction;
    private bool _initialized;

    public void Init(Vector2 direction)
    {
        _direction = direction.normalized;
        _initialized = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Debug.Log($"[EnemyProjectile] Init called. speed={speed}, direction={_direction}, hasRb={rb != null}");
        if (rb != null)
        {
            Debug.Log($"[EnemyProjectile] RB bodyType={rb.bodyType}, velocity={rb.linearVelocity}, gravity={rb.gravityScale}");
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
        }

        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (!_initialized) return;
        transform.Translate(_direction * speed * Time.fixedDeltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>()?.TakeDamage(damage);
            other.GetComponent<CharacterController>()?.TakeKnockback(_direction.x < 0);
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Enemy") && !other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
