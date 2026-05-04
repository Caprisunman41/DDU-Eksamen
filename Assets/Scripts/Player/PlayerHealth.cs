using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public float health;
    public float maxHealth = 4;
    public bool isDead = false;
    public float invincibilityDuration = 1f;
    private bool _isInvincible = false;
    public bool IsInvincible => _isInvincible;

    private CharacterController _controller;
    private PlayerAttack _attack;
    private SpriteRenderer _sr;
    private Rigidbody2D _rb;

    public static float SavedHealth = -1f;

    void Start()
    {
        health = SavedHealth > 0f ? SavedHealth : maxHealth;
        SavedHealth = -1f;

        _controller = GetComponent<CharacterController>();
        _attack = GetComponent<PlayerAttack>();
        _sr = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();

        CheckpointManager.Instance.SetCheckpoint(transform.position, null);
    }

    public void TakeDamage(float amount)
    {
        if (isDead || _isInvincible) return;

        health -= amount;
        if (health <= 0)
            Die();
        else
            StartCoroutine(InvincibilityFrames());
    }

    private void Die()
    {
        isDead = true;
        _rb.linearVelocity = Vector2.zero;
        _controller.enabled = false;
        _attack.enabled = false;
        _sr.enabled = false;
    }

    public void Respawn()
    {
        isDead = false;
        health = maxHealth;
        transform.position = CheckpointManager.Instance.RespawnPosition;
        _controller.enabled = true;
        _attack.enabled = true;
        _sr.enabled = true;
        ManaManager.Instance.ResetMana();
        EnemyRespawnManager.Instance.RespawnAllEnemies();
    }

    private IEnumerator InvincibilityFrames()
    {
        _isInvincible = true;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        float flashInterval = 0.1f;
        float elapsed = 0f;

        while (elapsed < invincibilityDuration)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        sr.enabled = true;
        _isInvincible = false;
    }
}