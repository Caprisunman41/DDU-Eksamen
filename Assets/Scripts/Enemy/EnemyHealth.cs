using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    public float health;
    public float maxHealth = 3f;
    public bool isDead = false;
    public float knockbackForce = 15f;
    public float knockbackDuration = 0.15f;

	private Rigidbody2D rb;
    private Coroutine _knockbackCoroutine;

    void Start()
    {
        health = maxHealth;
		rb = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(float amount, Vector2 knockbackDir)
    {
        if (isDead) return;

        health -= amount;

        if (_knockbackCoroutine != null) StopCoroutine(_knockbackCoroutine);
        _knockbackCoroutine = StartCoroutine(Knockback(knockbackDir));

        if (health <= 0)
        {
            isDead = true;
            if (ManaManager.Instance == null) Debug.LogError("ManaManager mangler i scenen");
            else ManaManager.Instance.OnEnemyKilled();
            gameObject.SetActive(false);
        }
    }

	private IEnumerator Knockback(Vector2 direction)
	{
    	EnemyMovement movement = GetComponent<EnemyMovement>();
    	if (movement != null) movement.isKnockedBack = true;

        Vector2 initialVelocity = new Vector2(direction.x * knockbackForce, knockbackForce * 0.5f);
        float elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            rb.linearVelocity = Vector2.Lerp(initialVelocity, Vector2.zero, elapsed / knockbackDuration);
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

    	rb.linearVelocity = Vector2.zero;
    	if (movement != null) movement.isKnockedBack = false;
	}
}