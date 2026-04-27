using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private GameObject attackArea = default;
    private PolygonCollider2D attackCollider;
    private AttackArea attackAreaScript;
    private Animator animator;

    private bool attacking = false;
    private float timeToAttack = 0.25f;
    private float attackCooldown = 0.2f;
    private float timer = 0f;
    private float cooldownTimer = 0f;

    void Start()
    {
        attackArea = transform.GetChild(0).GetChild(1).gameObject; // Colliders -> attackArea
        attackCollider = attackArea.GetComponent<PolygonCollider2D>();
        attackAreaScript = attackArea.GetComponent<AttackArea>();
        attackCollider.enabled = false;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (InputManager.AttackWasPressed && !attacking && cooldownTimer <= 0f)
        {
            Attack();
        }

        if (attacking)
        {
            timer += Time.deltaTime;

            if (timer >= timeToAttack)
            {
                timer = 0f;
                attacking = false;
                cooldownTimer = attackCooldown;
                attackCollider.enabled = false;
                attackAreaScript.ClearHits();
                animator.SetBool("isAttacking", false);
            }
        }
    }

    private void Attack()
    {
        attacking = true;
        timer = 0f;
        attackCollider.enabled = true;
        animator.SetBool("isAttacking", true);
    }
}