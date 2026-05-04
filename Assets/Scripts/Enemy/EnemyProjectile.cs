using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 4f;
    public float damage = 0.5f;
    public float lifetime = 8f;

    private Vector2 _direction;
    private bool _initialized;
    private Collider2D _ownerCollider;

    public void Init(Vector2 direction, Collider2D owner = null)
    {
        _direction = direction.normalized;
        _initialized = true;
        _ownerCollider = owner;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
        }

        // Ignore collision with the enemy that fired this projectile
        Collider2D ownCollider = GetComponent<Collider2D>();
        if (owner != null && ownCollider != null)
            Physics2D.IgnoreCollision(ownCollider, owner, true);

        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (!_initialized) return;
        transform.Translate(_direction * speed * Time.fixedDeltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == _ownerCollider) return;
        if (other.CompareTag("Enemy") || other.transform.root.CompareTag("Enemy")) return;

        if (other.transform.root.CompareTag("Player") && !other.isTrigger)
        {
            other.transform.root.GetComponent<PlayerHealth>()?.TakeDamage(damage);
            other.transform.root.GetComponent<CharacterController>()?.TakeKnockback(_direction.x < 0);
            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
