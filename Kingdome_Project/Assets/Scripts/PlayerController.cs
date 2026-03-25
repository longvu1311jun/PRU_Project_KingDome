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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isInvincible) return;
        if (collision.gameObject.CompareTag("Damage"))
        {
            health -= 1;
            // Fix: was rb.linearVelocity.y instead of .x
            float knockDir = transform.position.x < collision.transform.position.x ? -1f : 1f;
            rb.linearVelocity = new Vector2(knockDir * knockbackForce, jump * 0.5f);
            StartCoroutine(BlinkRed());
            animator.SetTrigger("Hurt");
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            health -= 1; // © was missing
            float knockDir = transform.position.x < collision.transform.position.x ? -1f : 1f;
            rb.linearVelocity = new Vector2(knockDir * knockbackForce, jump * 0.5f); // © proper knockback
            StartCoroutine(BlinkRed());
            animator.SetTrigger("Hurt");
        }

        if (health <= 0)
            Die();
    }

    private IEnumerator BlinkRed()
    {
        isInvincible = true; // © start invincibility
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(invincibleDuration); // © covers blink + iframes
        spriteRenderer.color = Color.white;
        isInvincible = false; // © end invincibility
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
            }
            else
            {
                animator.SetBool("isLeviation", false);
                animator.SetBool("isFall", true);
            }
        }
    }
}
