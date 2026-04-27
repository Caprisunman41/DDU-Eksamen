using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public EnemyMovement EnemyMovement;
    public CharacterController characterController;
    public PlayerHealth playerHealth;
    public float normalEnemy = 0.5f;
    public float attackCooldown = 0.8f;
    public float _attackTimer = 0f;

    void Update()
    {
        if (_attackTimer > 0f)
            _attackTimer -= Time.deltaTime;
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePlayerHit(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandlePlayerHit(collision);
    }

    private void HandlePlayerHit(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && _attackTimer <= 0f)
        {
            _attackTimer = attackCooldown;
            bool fromRight = collision.transform.position.x <= transform.position.x;
            characterController.TakeKnockback(fromRight);
            playerHealth.TakeDamage(normalEnemy);
        }
    }
}
