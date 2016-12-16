using UnityEngine;
using System.Collections;

public class CharController : MonoBehaviour {

	enum jump { none, jump, doublejump, walljump};
	int jumpflag;

	//player public variables
    public float maxSpeed = 3.0f; // What is our speed capped at
	public float playerAccel = 10.0f; // how fast do we accelerate when pressing movement keys
	public float playerDecel = 10.0f; // how fast do we decellerate when we don't press movement keys
	public float jumpForce = 700; // how high do we jump
	public Transform spawnPoint; // Where in the scene do we teleport to when we die
	public BoxCollider2D wallBoxRight;
	public BoxCollider2D wallBoxLeft;
	public BoxCollider2D groundBox; // our overlapping box for ground detect
	public LayerMask whatIsGround; // What layer do we check for ground on?
	public LayerMask whatIsDeathBox; // What layer do we check for death on?

	//player states
		//Physics states
	bool facingRight = true;
    bool grounded = false;
	bool wallStuck = false;
	bool wallStuckRight = false;
	bool wallStuckLeft = false;
	bool nearWallRight = false;
	bool nearWallLeft = false;
	bool canWallStick = true;
	bool canDoubleJump = false;

	//Gameobject components
	Animator anim = new Animator(); // is this legit?
	Rigidbody2D rbody;

    void Start ()
    {
        anim = GetComponent<Animator>();
        rbody = GetComponent<Rigidbody2D>();
	}

	void UpdatePlayerStateP() { // update player states - Physics
		grounded = Physics2D.OverlapBox(groundBox.transform.position, groundBox.size, 0.0f, whatIsGround);
		anim.SetBool("Ground", grounded);
		//nearWallLeft = Physics2D.OverlapBox(wallBoxLeft.transform.position, wallBoxLeft.size, 0.0f, whatIsGround);	
		//nearWallRight = Physics2D.OverlapBox(wallBoxRight.transform.position, wallBoxRight.size, 0.0f, whatIsGround);	
		if (grounded) {
			canDoubleJump = true;
			canWallStick = true;
		}
	}

    // Update is called once per frame Fixed update is for physics stuff
    void FixedUpdate()
    {
		UpdatePlayerStateP();
		//anim.SetBool("WallStuckLeft", nearWallLeft);
		//anim.SetBool("WallStuckRight", nearWallRight);

		//if (wallStuck && !grounded && canWallStick) {
		//	rbody.constraints = RigidbodyConstraints2D.FreezePosition  | RigidbodyConstraints2D.FreezeRotation;
		//	canWallStick = false;
		//}

        //Set float var for animator
		anim.SetFloat("vSpeed", rbody.velocity.y);

		//Player running input
		//float move = Input.GetAxis("Horizontal");
		float move = 0.0f;
		if (Input.GetKey(KeyCode.LeftArrow))
			move -= 1;
		if (Input.GetKey(KeyCode.RightArrow))
			move += 1;
        anim.SetFloat("Speed", Mathf.Abs(move));
        if (move != 0) // If player is giving movement input
        {
            float vAdd = (playerAccel * Time.deltaTime); // v = v1 + a*t
            if (move < 0)
                vAdd *= -1.0f;
            rbody.velocity = new Vector2(rbody.velocity.x + vAdd, rbody.velocity.y);
		}
		else // If player is not giving movement input and we're grounded, we need to slow down
		{
            float vAdd= (playerDecel * Time.deltaTime); //Calc how much we slow down by
			if (Mathf.Abs(rbody.velocity.x) < Mathf.Abs(vAdd)) // if the slow down amound is more than our current velocity
				rbody.velocity = new Vector2(0, rbody.velocity.y); // just set to 0
			else if (rbody.velocity.x > 0)	// if we're moving right
				rbody.velocity = new Vector2(rbody.velocity.x - vAdd, rbody.velocity.y); // reduce right velocity by vAdd
			else							// else we're moving left
				rbody.velocity = new Vector2(rbody.velocity.x + vAdd, rbody.velocity.y); // reduce left velocity by vAdd
		}

		//Make sure we're not going over max speed
		if (Mathf.Abs (rbody.velocity.x) > maxSpeed) {
			if (rbody.velocity.x > 0)
				rbody.velocity = new Vector2(maxSpeed, rbody.velocity.y);
			else
				rbody.velocity = new Vector2(-maxSpeed, rbody.velocity.y);
		}
		Jump(jumpflag);
		jumpflag = (int)jump.none;

        if (move > 0 && !facingRight && grounded)
            Flip();
        else if (move < 0 && facingRight && grounded)
            Flip();
    }

    void Update() // input read more accurately in update vs fixed update
    { 
        //jump and double jump
		if (Input.GetKeyDown(KeyCode.Space)) //do this method only if you want to have keys not be remappable
		{
			if (grounded) //Do a regular jump
			{
				jumpflag = (int)jump.jump;
			}
			else if (canDoubleJump && !grounded) { //if we are jumping and not grounded, we better trigger doublejump
				jumpflag = (int)jump.doublejump;
			}
			//else if (wallStuck) { //if we're on a wall, we want to wall jump
			//	wallStuck = false;
			//	rbody.constraints = RigidbodyConstraints2D.FreezeRotation;
			//	rbody.velocity = new Vector2 (0, jumpForce);
			//}
		}
	}

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

	void Jump(int jumpnum)
	{
		if (jumpnum == (int)jump.jump) //Do a regular jump
		{
			rbody.velocity = new Vector2(0, jumpForce);
		}
		else if (jumpnum == (int)jump.doublejump)
		{ //if we are jumping and not grounded, we better trigger doublejump
			anim.SetTrigger("DoubleJump");
			canDoubleJump = false;
			rbody.velocity = new Vector2(0, jumpForce);
		}
		//else if (jumpnum == (int)jump.walljump) { //if we're on a wall, we want to wall jump
		//	wallStuck = false;
		//	rbody.constraints = RigidbodyConstraints2D.FreezeRotation;
		//	rbody.velocity = new Vector2 (0, jumpForce);
		//}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (((1<<other.gameObject.layer) & whatIsDeathBox) != 0) // if we enter deathbox
		{ //TODO check if this is only our player's physical hitbox, or if it's all attached box2d's
			//Teleport player to spawn
			transform.position = spawnPoint.position;
		}
	}
}
