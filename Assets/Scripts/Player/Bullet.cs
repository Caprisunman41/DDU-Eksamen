using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 15f;
    public float damage = 2f;
    public float lifetime = 3f;

    public float maxDistance = 15f;

    private Vector2 _direction;
    private Vector2 _startPosition;

    public void Init(Vector2 direction)
    {
        _direction = direction.normalized;
        _startPosition = transform.position;
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
        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player")) return;

        EnemyProjectile enemyProj = other.GetComponentInParent<EnemyProjectile>();
        if (enemyProj != null)
        {
            Destroy(enemyProj.gameObject);
            Destroy(gameObject);
            return;
        }

        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy != null)
            enemy.TakeDamage(damage, _direction);

        Destroy(gameObject);
    }
}
