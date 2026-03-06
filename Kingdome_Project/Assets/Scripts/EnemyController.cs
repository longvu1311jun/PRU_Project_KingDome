using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float speed = 2f;
    public Transform[] points;

    private int i;
    private Animator animator;
    private SpriteRenderer sprite;
    void Start()
    {
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        SetAnimation();
        if (Mathf.Abs(transform.position.x - points[i].position.x) < 0.1f)
        {
            i++;
            if (i == points.Length)
            {
                i = 0;
            }
        }
        float newX = Mathf.MoveTowards(
        transform.position.x,
        points[i].position.x,
        speed * Time.fixedDeltaTime
        );

        transform.position = new Vector2(newX, transform.position.y);

        sprite.flipX = (transform.position.x - points[i].position.x) < 0f;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            ReverseDirection();
        }
    }

    private void SetAnimation()
    {
        //Not Tested
        animator.SetInteger("AnimState", 2);
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
