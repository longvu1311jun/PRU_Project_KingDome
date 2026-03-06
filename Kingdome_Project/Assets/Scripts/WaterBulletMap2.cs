using UnityEngine;

public class WaterBulletMap2 : MonoBehaviour
{
    public float speed = 50f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        float direction = transform.localScale.x;

        rb.linearVelocity = new Vector2(direction * speed, 0);

        Destroy(gameObject, 5f); 
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}