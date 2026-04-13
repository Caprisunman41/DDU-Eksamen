using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private GameObject attackArea = default;
    private PolygonCollider2D attackCollider;

    private bool attacking = false;
    private float timeToAttack = 0.25f;
    private float timer = 0f;

    void Start()
    {
        attackArea = transform.GetChild(0).GetChild(1).gameObject; // Colliders -> attackArea
        attackCollider = attackArea.GetComponent<PolygonCollider2D>();
        attackCollider.enabled = false;
    }

    void Update()
    {
        if (InputManager.AttackWasPressed)
        {
            Attack();
        }

        if (attacking)
        {
            timer += Time.deltaTime;

            if (timer >= timeToAttack)
            {
                timer = 0;
                attacking = false;
                attackCollider.enabled = false;
            }
        }
    }

    private void Attack()
    {
        attacking = true;
        attackCollider.enabled = true;
    }
}