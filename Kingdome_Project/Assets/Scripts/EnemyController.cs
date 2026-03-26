using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
    public float speed = 2f;
    public Transform[] points;

    private int i;
    private Animator animator;
    private SpriteRenderer sprite;
    private Rigidbody2D rb;

    [Header("Health")]
    public int health = 3;
    private bool isDead = false;
    private bool isHurt = false;

    [Header("Knockback")]
    public float knockbackForce = 4f;
    public float hurtDuration = 0.4f;

    [Header("Attack")]
    public float detectRange = 2f;
    public float attackRange = 1f;
    public float attackCooldown = 1f;
    public LayerMask playerLayer;
    public Vector2 attackBoxSize = new Vector2(1f, 1f);

    private float attackTimer = 0f;
    private bool isAttacking = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // Stop everything if dead or hurt
        if (isDead || isHurt) return;

        attackTimer -= Time.fixedDeltaTime;

        bool playerDetected = DetectPlayer(detectRange);
        bool playerInAttackRange = DetectPlayer(attackRange);

        if (playerInAttackRange && attackTimer <= 0f)
        {
            Attack();
        }
        else if (playerDetected)
        {
            isAttacking = false;
        }
        else
        {
            isAttacking = false;
            Patrol();
        }

        SetAnimation(playerDetected);
        sprite.flipX = (transform.position.x - points[i].position.x) < 0f;
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isHurt) return;

        health -= damage;

        if (health <= 0)
            StartCoroutine(DieRoutine());
        else
            StartCoroutine(HurtRoutine());
    }

    private IEnumerator HurtRoutine()
    {
        isHurt = true;
        animator.SetTrigger("Hurt");

        // Knockback — push away from player
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            float knockDir = transform.position.x > player.transform.position.x ? 1f : -1f;
            rb.linearVelocity = new Vector2(knockDir * knockbackForce, 2f);
        }

        yield return new WaitForSeconds(hurtDuration);

        rb.linearVelocity = Vector2.zero;
        isHurt = false;
    }

    private IEnumerator DieRoutine()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        animator.SetTrigger("Death");

        // Wait for death animation length
        yield return new WaitForSeconds(1.5f);

        Destroy(gameObject);
    }

    // ── Everything below unchanged from your original ─────────────────────

    private bool DetectPlayer(float range)
    {
        Vector2 origin = (Vector2)transform.position;
        Vector2 size = new Vector2(attackBoxSize.x, attackBoxSize.y);

        RaycastHit2D hitLeft = Physics2D.BoxCast(origin, size, 0f, Vector2.left, range, playerLayer);
        RaycastHit2D hitRight = Physics2D.BoxCast(origin, size, 0f, Vector2.right, range, playerLayer);

        return hitLeft.collider != null || hitRight.collider != null;
    }

    private void Attack()
    {
        isAttacking = true;
        attackTimer = attackCooldown;
        animator.SetTrigger("Attack");

        Vector2 origin = (Vector2)transform.position;
        RaycastHit2D hitLeft = Physics2D.BoxCast(origin, attackBoxSize, 0f, Vector2.left, attackRange, playerLayer);
        RaycastHit2D hitRight = Physics2D.BoxCast(origin, attackBoxSize, 0f, Vector2.right, attackRange, playerLayer);

        RaycastHit2D validHit = hitLeft.collider != null ? hitLeft : hitRight;

        if (validHit.collider != null)
        {
            PlayerController player = validHit.collider.GetComponent<PlayerController>();
            if (player != null)
            {
                float knockDir = player.transform.position.x < transform.position.x ? -1f : 1f;
                player.TakeHit(knockDir);
            }
        }
    }

    private void Patrol()
    {
        if (Mathf.Abs(transform.position.x - points[i].position.x) < 0.1f)
        {
            i++;
            if (i == points.Length) i = 0;
        }

        float newX = Mathf.MoveTowards(
            transform.position.x,
            points[i].position.x,
            speed * Time.fixedDeltaTime
        );
        transform.position = new Vector2(newX, transform.position.y);
    }

    private void SetAnimation(bool playerDetected)
    {
        if (isDead || isHurt) return;

        if (playerDetected || isAttacking)
            animator.SetInteger("AnimState", 1);
        else
            animator.SetInteger("AnimState", 2);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
            ReverseDirection();
    }

    void ReverseDirection()
    {
        i--;
        if (i < 0) i = points.Length - 1;
    }

    private void OnDrawGizmosSelected()
    {
        float direction = sprite != null ? (sprite.flipX ? 1f : -1f) : 1f;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            transform.position + new Vector3(direction * detectRange / 2f, 0f),
            new Vector3(detectRange, attackBoxSize.y)
        );

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            transform.position + new Vector3(direction * attackRange, 0f),
            new Vector3(attackBoxSize.x, attackBoxSize.y)
        );
    }
}