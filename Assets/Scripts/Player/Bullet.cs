using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 15f;
    public float damage = 2f;
    public float lifetime = 3f;

    public float maxDistance = 15f;

    private Vector2 _direction;
    private Vector2 _startPosition;
    private Collider2D _ownerCollider;

    public void Init(Vector2 direction, Collider2D owner = null)
    {
        _direction = direction.normalized;
        _startPosition = transform.position;
        _ownerCollider = owner;

        Collider2D ownCollider = GetComponent<Collider2D>();
        if (owner != null && ownCollider != null)
            Physics2D.IgnoreCollision(ownCollider, owner, true);

        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        transform.Translate(_direction * speed * Time.fixedDeltaTime, Space.World);
        if (Vector2.Distance(_startPosition, transform.position) >= maxDistance)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == _ownerCollider) return;

        // Ignore anything in the player's hierarchy (root, parent, or self)
        if (other.GetComponentInParent<PlayerHealth>() != null) return;
        if (other.GetComponentInParent<CharacterController>() != null) return;

        EnemyProjectile enemyProj = other.GetComponentInParent<EnemyProjectile>();
        if (enemyProj != null)
        {
            Destroy(enemyProj.gameObject);
            Destroy(gameObject);
            return;
        }

        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, _direction);
            Destroy(gameObject);
            return;
        }

        // Only destroy on solid (non-trigger) colliders like walls/ground
        if (!other.isTrigger)
            Destroy(gameObject);
    }
}
