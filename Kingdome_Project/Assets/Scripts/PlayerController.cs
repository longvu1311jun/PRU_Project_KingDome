using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;

    [Header("Jumping")]
    public float jump = 10f;

    [Header("Jump Delay")]
    public float jumpDelay = 0.2f;
    private bool isJumping;

    [Header("Health")]
    public int health = 3;

    [Header("Attack")]
    public float attackRange = 1f;
    public int attackDamage = 10;
    public float attackCooldown = 0.4f;
    public Transform attackPoint;       // empty child at fist/weapon tip
    public LayerMask enemyLayer;

    [Header("Hit Effect")]
    public GameObject swingEffectPrefab;
    public GameObject hitEffectPrefab;
    public float hitEffectDuration = 0.5f;

    private float _lastAttackTime = -99f;
    private bool isAttacking = false;

    [Header("Knockback")]
    public float knockbackForce = 5f;
    public float invincibleDuration = 0.5f;
    private bool isInvincible = false;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Score")]
    public int scores;
    public TextMeshProUGUI scoreText;
    public GameObject winUI;
    public Button nextBtn;
    public TextMeshProUGUI winText;

    private bool isGrounded;
    private bool isFacingRight = true;
    private float movementX;
    private float movementY;

    private bool isHurt = false;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        //Vector2 movement = new Vector2(movementX, movementY);
        rb.linearVelocity = new Vector2(movementX * speed, rb.linearVelocity.y);
        HandleFlip();
        SetAnimation();
        SetScoreText();

        if (transform.position.y < -7)
        {
            Vector3 pos = transform.position;
            pos.y += 5f;
            transform.position = pos;
            Die();
        }
    }

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        movementX = movementVector.x;
        movementY = movementVector.y;
    }

    void OnJump()
    {
        if (isGrounded && !isJumping)
        {
            StartCoroutine(JumpWithDelay());
        }
    }

    IEnumerator JumpWithDelay()
    {
        isJumping = true;

        animator.ResetTrigger("Jump");
        animator.SetTrigger("Jump");

        yield return new WaitForSeconds(jumpDelay);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jump);

        isJumping = false;
    }

    void OnAttack()
    {
        Debug.Log("Attack called!");

        if (Time.time < _lastAttackTime + attackCooldown) return;
        if (isHurt) return;

        _lastAttackTime = Time.time;
        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        // Always spawn the swing slash visual
        SpawnSwingEffect();

        // Small delay so the animation wind-up plays before hit detection
        yield return new WaitForSeconds(0.1f);

        DetectHits();

        yield return new WaitForSeconds(0.2f);
        isAttacking = false;
    }

    private void SpawnSwingEffect()
    {
        if (swingEffectPrefab == null) return;
        GameObject fx = Instantiate(
            swingEffectPrefab,
            attackPoint.position,
            Quaternion.identity,
            transform  // parent to player so it flips with them
        );
    }

    private void DetectHits()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayer
        );

        foreach (Collider2D col in hits)
        {
            // Works with any enemy that has IDamageable
            if (col.TryGetComponent<IDamageable>(out var target))
                target.TakeDamage(attackDamage);

            // Spawn hit effect at closest point on the collider
            SpawnHitEffect(col.ClosestPoint(attackPoint.position));
        }
    }

    private void SpawnHitEffect(Vector2 position)
    {
        if (hitEffectPrefab == null) return;
        GameObject fx = Instantiate(hitEffectPrefab, position, Quaternion.identity);
        Destroy(fx, hitEffectDuration);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isInvincible) return;

        if (collision.gameObject.CompareTag("Damage") || collision.gameObject.CompareTag("Enemy"))
        {
            float knockDir = transform.position.x < collision.transform.position.x ? -1f : 1f;
            TakeHit(knockDir);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Your existing ground check visual
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // Attack range visual
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }

    private IEnumerator HurtRoutine()
    {
        // Set all flags together
        isInvincible = true;
        isHurt = true;

        animator.SetTrigger("Hurt");

        // Blink red
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;

        // Wait remaining hurt duration
        yield return new WaitForSeconds(invincibleDuration - 0.1f);

        // Clear all flags together
        isHurt = false;
        isInvincible = false;
    }

    public void TakeHit(float knockDir)
    {
        if (isInvincible) return;

        health -= 1;
        rb.linearVelocity = new Vector2(knockDir * knockbackForce, jump * 0.5f);
        StartCoroutine(HurtRoutine());

        if (health <= 0)
            Die();
    }

    private void Die()
    {
        Time.timeScale = 0;
        winUI.SetActive(true);
        nextBtn.gameObject.SetActive(false);
        winText.text = "Game Over!";
    }

    private void HandleFlip()
    {
        if (rb.linearVelocityX < 0 && isFacingRight) Flip();
        else if (rb.linearVelocityX > 0 && !isFacingRight) Flip();
    }
    private void Flip()
    {
        isFacingRight = !isFacingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void SetScoreText()
    {
        scoreText.text = "Score: " + scores.ToString();
    }

    private void SetAnimation()
    {
        if (isHurt) return;

        animator.SetFloat("yVelocity", rb.linearVelocity.y);

        if (isGrounded && !isJumping)
        {
            animator.SetBool("isGround", true);
            animator.SetBool("isLeviation", false);
            animator.SetBool("isFall", false);
        }
        else
        {
            animator.SetBool("isGround", false);
            if (rb.linearVelocityY > 0)
            {
                animator.SetBool("isLeviation", true);
                animator.SetBool("isFall", false);
            }
            else
            {
                animator.SetBool("isLeviation", false);
                animator.SetBool("isFall", true);
            }
        }
    }
}
