using UnityEngine;

public class RangedAttack : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float attackRange = 6f;
    public float fireInterval = 2.5f;

    private float _fireTimer;
    private Transform _playerTransform;
    private EnemyMovement _movement;

    void Start()
    {
        _movement = GetComponent<EnemyMovement>();
        if (_movement != null) _movement.stopAtRange = attackRange;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) _playerTransform = player.transform;
    }

    void Update()
    {
        if (_playerTransform == null || projectilePrefab == null || firePoint == null) return;

        _fireTimer += Time.deltaTime;

        float dist = Vector2.Distance(transform.position, _playerTransform.position);
        if (_movement != null && !_movement.isChasing) return;
        if (dist > attackRange) return;

        if (_fireTimer >= fireInterval)
        {
            _fireTimer = 0f;
            Shoot();
        }
    }

    void Shoot()
    {
        Vector2 direction = (_playerTransform.position - transform.position).normalized;

        // Offset spawn point in the direction the enemy is actually facing
        float facingSign = transform.localScale.x >= 0f ? -1f : 1f;
        Vector2 localOffset = firePoint != null
            ? new Vector2(Mathf.Abs(firePoint.localPosition.x) * facingSign, firePoint.localPosition.y)
            : Vector2.zero;
        Vector2 spawnPos = (Vector2)transform.position + localOffset;

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        proj.GetComponent<EnemyProjectile>()?.Init(direction);
    }
}
