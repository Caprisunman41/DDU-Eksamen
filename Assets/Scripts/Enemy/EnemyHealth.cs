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

    void Start()
    {
        health = maxHealth;
		rb = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(float amount, Vector2 knockbackDir)
    {
        if (isDead) return;

        health -= amount;
		StartCoroutine(Knockback(knockbackDir));

        if (health <= 0)
        {
            isDead = true;
            Destroy(gameObject);
        }
    }
	private IEnumerator Knockback(Vector2 direction)
	{
    	Debug.Log("Knockback retning: " + direction + " kraft: " + knockbackForce);
    
    	EnemyMovement movement = GetComponent<EnemyMovement>();
    	if (movement != null) movement.isKnockedBack = true;

    	rb.linearVelocity = new Vector2(direction.x * knockbackForce, knockbackForce * 0.5f);
    	yield return new WaitForSeconds(knockbackDuration);
    	rb.linearVelocity = Vector2.zero;

    	if (movement != null) movement.isKnockedBack = false;
	}
}