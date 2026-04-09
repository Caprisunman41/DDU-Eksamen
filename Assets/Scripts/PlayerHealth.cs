using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public float health;
    public float maxHealth = 4;
    public bool isDead = false;
    public float invincibilityDuration = 1f;
    private bool _isInvincible = false;

    void Start()
    {
        health = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead || _isInvincible) return;
        
        health -= amount;
        if (health <= 0)
        {
            isDead = true;
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(InvincibilityFrames());
        }
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