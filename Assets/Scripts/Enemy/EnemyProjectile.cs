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
        if (other.GetComponentInParent<EnemyHealth>() != null) return;

        PlayerHealth ph = other.GetComponentInParent<PlayerHealth>();
        CharacterController cc = other.GetComponentInParent<CharacterController>();
        if (ph != null && !other.isTrigger)
        {
            ph.TakeDamage(damage);
            if (cc != null) cc.TakeKnockback(_direction.x < 0);
            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger)
            Destroy(gameObject);
    }
}
