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

        // Ranged enemies don't deal contact damage
        EnemyDamage dmg = GetComponent<EnemyDamage>();
        if (dmg != null) dmg.enabled = false;

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

        // Face the player before shooting
        bool playerToRight = _playerTransform.position.x > transform.position.x;
        float s = _movement != null ? _movement.spriteScale : 1f;
        transform.localScale = new Vector3(playerToRight ? -s : s, s, 1f);

        float facingSign = transform.localScale.x >= 0f ? -1f : 1f;
        Vector2 localOffset = firePoint != null
            ? new Vector2(Mathf.Abs(firePoint.localPosition.x) * facingSign, firePoint.localPosition.y)
            : Vector2.zero;
        Vector2 spawnPos = (Vector2)transform.position + localOffset;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.Euler(0f, 0f, angle));
        proj.GetComponent<EnemyProjectile>()?.Init(direction, GetComponent<Collider2D>());
    }
}
