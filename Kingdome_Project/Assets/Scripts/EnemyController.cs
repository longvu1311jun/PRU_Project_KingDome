using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float speed = 2f;
    public Transform[] points;

    private int i;
    private Animator animator;
    private SpriteRenderer sprite;

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
    }

    void FixedUpdate()
    {
        attackTimer -= Time.fixedDeltaTime;

        bool playerDetected = DetectPlayer(detectRange);
        bool playerInAttackRange = DetectPlayer(attackRange);

        if (playerInAttackRange && attackTimer <= 0f)
        {
            Attack();
        }
        else if (playerDetected)
        {
            // Player detected but not in attack range — stop and wait
            isAttacking = false;
        }
        else
        {
            // No player detected — resume patrol
            isAttacking = false;
            Patrol();
        }

        SetAnimation(playerDetected);
        sprite.flipX = (transform.position.x - points[i].position.x) < 0f;
    }

    // Detect player in front using a box cast
    private bool DetectPlayer(float range)
    {
        // Check both directions since enemy patrols and may not always face player
        Vector2 origin = (Vector2)transform.position;
        Vector2 size = new Vector2(attackBoxSize.x, attackBoxSize.y);

        // Cast left
        RaycastHit2D hitLeft = Physics2D.BoxCast(
            origin, size, 0f,
            Vector2.left,
            range,
            playerLayer
        );

        // Cast right
        RaycastHit2D hitRight = Physics2D.BoxCast(
            origin, size, 0f,
            Vector2.right,
            range,
            playerLayer
        );

        return hitLeft.collider != null || hitRight.collider != null;
    }

    private void Attack()
    {
        isAttacking = true;
        attackTimer = attackCooldown;

        animator.SetTrigger("Attack");

        // Find actual direction toward player
        Vector2 origin = (Vector2)transform.position;

        RaycastHit2D hitLeft = Physics2D.BoxCast(origin, attackBoxSize, 0f, Vector2.left, attackRange, playerLayer);
        RaycastHit2D hitRight = Physics2D.BoxCast(origin, attackBoxSize, 0f, Vector2.right, attackRange, playerLayer);

        float direction = hitLeft.collider != null ? -1f : 1f;
        RaycastHit2D validHit = hitLeft.collider != null ? hitLeft : hitRight;

        if (validHit.collider != null)
        {
            PlayerController player = validHit.collider.GetComponent<PlayerController>();
            if (player != null)
            {
                player.health -= 1;

                Rigidbody2D playerRb = validHit.collider.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    Vector2 knockback = new Vector2(direction * 3f, 2f);
                    playerRb.linearVelocity = knockback;
                }
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
        if (playerDetected || isAttacking)
            animator.SetInteger("AnimState", 1);  // Combat idle when detect or attacking
        else
            animator.SetInteger("AnimState", 2);  // Run/patrol
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            ReverseDirection();
        }
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

    void ReverseDirection()
    {
        // Go back to previous point
        i--;

        if (i < 0)
        {
            i = points.Length - 1;
        }
    }
}
