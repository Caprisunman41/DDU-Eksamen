using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 15f;
    public float damage = 2f;
    public float lifetime = 3f;

    private Vector2 _direction;

    public void Init(Vector2 direction)
    {
        _direction = direction.normalized;
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        transform.Translate(_direction * speed * Time.fixedDeltaTime, Space.World);
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
