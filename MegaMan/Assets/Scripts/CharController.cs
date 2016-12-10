using UnityEngine;
using System.Collections;

public class CharController : MonoBehaviour {

    public float maxSpeed = 3.0f;
    bool facingRight = true;

    Animator anim = new Animator(); // is this legit?
    Rigidbody2D rbody;

    bool grounded = false;
    public Transform groundTransform;
    float groundedLen = 0.2f;
    public LayerMask whatIsGround;
    public float jumpForce = 700;

    bool doubleJump = false;

    float maxAnimSpeed = 10.0f;

    void Start ()
    {
        anim = GetComponent<Animator>();
        rbody = GetComponent<Rigidbody2D>();
	}

    // Update is called once per frame

    void FixedUpdate()
    {
        //check if grounded
        //grounded = Physics2D.Linecast(groundTransform.position, new Vector2(groundTransform.position.x, groundTransform.position.y + groundedLen), whatIsGround);//original
        grounded = Physics2D.OverlapBox(groundTransform.position, new Vector2(groundedLen, groundedLen) , 0.0f, whatIsGround); // TODO make sure box isn't too big

        anim.SetBool("Ground", grounded);

        //Check if we need to reset double jump
        if (grounded)
            doubleJump = false;

        //Set float var for animator
        anim.SetFloat("vSpeed", rbody.velocity.y);

        /*//Set animation speed
        if (Mathf.Abs(rbody.velocity.x) < maxAnimSpeed){
            float speed = Mathf.Abs(rbody.velocity.x) * 0.2f;
            anim.speed = (speed <= 1.0f) ? speed  : 1.0f; // if animation speed set to 0 we get wierd stuff
        }*/

        //Player input
        float move = Input.GetAxis("Horizontal");
        anim.SetFloat("Speed", Mathf.Abs(move));
        GetComponent<Rigidbody2D>().velocity = new Vector2(move * maxSpeed, GetComponent<Rigidbody2D>().velocity.y);

        if (move > 0 && !facingRight && grounded)
            Flip();
        else if (move < 0 && facingRight && grounded)
            Flip();
    }

    void Update() // input read more accurately in update vs fixed update
    { 
        //jump and double jump
        if (Input.GetKeyDown(KeyCode.Space) && (grounded || !doubleJump)) //do this method only if you want to have keys not be remappable
        {
            anim.SetBool("Ground", false);
            rbody.velocity = new Vector2(0, jumpForce);
            if (!doubleJump && !grounded) //if we are jumping and not grounded, we better trigger doublejump
                anim.SetTrigger("DoubleJump");
                doubleJump = true;
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}
