using UnityEngine;

public class AttackArea : MonoBehaviour
{
    private float normalAttack = 1f;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.GetComponent<EnemyHealth>() != null)
        {
            EnemyHealth enemy = collider.GetComponent<EnemyHealth>();
            Vector2 knockbackDir = (collider.transform.position - transform.position).normalized;
            enemy.TakeDamage(normalAttack, knockbackDir);
            Debug.Log("Ramte fjende!");
        }
    }
}