using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpDouble : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float jumpDoubleTime;
    [SerializeField] private LayerMask jump;
    private float jumpDouble;
    private Rigidbody2D body;
    private Animator anim;
    private BoxCollider2D boxCollider;
    private float horizontalInput;

    [Header("SFX")]
    [SerializeField] private AudioClip jumpSound;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");

        // Flip player when moving left-right
        if (horizontalInput > 0.01f)
            transform.localScale = Vector3.one;
        else if (horizontalInput < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);

        // Set animator parameters
        anim.SetBool("run", Mathf.Abs(horizontalInput) > 0.01f);
        anim.SetBool("grounded", isGrounded());

        // Jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
        // Jump Height
        if (Input.GetKeyUp(KeyCode.Space) && body.linearVelocity.y > 0)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, body.linearVelocity.y / 2);
        }

        // Restore gravity when not on wall or ground
        if (!isGrounded() && !onWall())
        {
            body.gravityScale = 1;
        }
        if (isGrounded())
        {
            jumpDouble = jumpDoubleTime ;

            

        }
        else
        {
            jumpDouble -= Time.deltaTime;// quá 0,25 giây sẽ không ấn được nút cách tiếp

        }

        // Move horizontally
        body.linearVelocity = new Vector2(horizontalInput * speed, body.linearVelocity.y);
    }

    private void Jump()
    {
        if (jumpDouble <= 0 && !onWall())
        {
            return;
        }
        if (onWall())
        {
          // nothing
        }
        else
        {
            if (isGrounded())
            {
                body.linearVelocity = new Vector2(body.linearVelocity.x, jumpPower);
                AudioManager.instance.PlaySound(jumpSound);
            }
            else
            {
                if(jumpDouble > 0)
                {
                    body.linearVelocity = new Vector2(body.linearVelocity.x, jumpPower);
                }
            }
            jumpDouble = 0; 
        }
    }
   



    private bool isGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }

    private bool onWall()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, new Vector2(transform.localScale.x, 0), 0.1f, wallLayer);
        return raycastHit.collider != null;
    }
  

    public bool canAttack()
    {
        return   !onWall() ;
    }
}
