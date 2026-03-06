using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public float speed = 3f;
    public float jumpForce = 5f;

    public GameObject waterPrefab;
    public Transform firePoint;
    public float bulletSpeed = 8f;

    public float fireCooldown = 2f;
    private float lastFireTime = 0f;


    private Rigidbody2D rb;
    private bool isGrounded;
    private Vector3 originalScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
    }

    void Update()
    {
        float move = Input.GetAxis("Horizontal");

        rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);

        // quay mặt
        if (move > 0)
            transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
        else if (move < 0)
            transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);

        // nhảy
        if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
        }

        // bắn nước
        if(Input.GetKeyDown(KeyCode.Space) && Time.time - lastFireTime >= fireCooldown)
        {
            Shoot();
            lastFireTime = Time.time;
        }
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(waterPrefab, firePoint.position, Quaternion.identity);

        bullet.transform.localScale = transform.localScale;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }
}