using System.Collections.Generic;
using UnityEngine;

public class AttackArea : MonoBehaviour
{
    private float normalAttack = 1f;
    private readonly HashSet<EnemyHealth> _hitEnemies = new HashSet<EnemyHealth>();

    private void OnTriggerEnter2D(Collider2D collider)
    {
        EnemyProjectile proj = collider.GetComponentInParent<EnemyProjectile>();
        if (proj != null)
        {
            Destroy(proj.gameObject);
            return;
        }

        EnemyHealth enemy = collider.GetComponentInParent<EnemyHealth>();
        if (enemy != null && !_hitEnemies.Contains(enemy))
        {
            _hitEnemies.Add(enemy);
            Vector2 knockbackDir = (collider.transform.position - transform.position).normalized;
            enemy.TakeDamage(normalAttack, knockbackDir);
        }
    }

    public void ClearHits()
    {
        _hitEnemies.Clear();
    }
}