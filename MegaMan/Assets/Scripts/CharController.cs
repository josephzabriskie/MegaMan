using UnityEngine;
using System.Collections;

public class CharController : MonoBehaviour {

	enum jump { none, jump, doublejump, walljump };
	int jumpFlag;
	float horizInput;
	float vertInput;

	//player public variables
	public float maxSpeedx = 7.5f; // What is our x speed capped at
	public float maxSpeedy = 30.0f;// What is our y speed capped at
	public float playerAccel = 10.0f; // how fast do we accelerate when pressing movement keys
	public float playerDecel = 10.0f; // how fast do we decelerate when we don't press movement keys
	public float playerAirDecel = 10.0f; // how fast do we decelerate in the air?
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
	bool wallStuckRight = false;
	bool wallStuckLeft = false;
	bool nearWallRight = false;
	bool nearWallLeft = false;
	bool canWallStick = true; // we can only stick to walls once
	bool canDoubleJump = false;

	//Gameobject components
	Animator anim = new Animator(); // is this legit?
	Rigidbody2D rbody;
	SpriteRenderer sr;

	void Start()
	{
		anim = GetComponent<Animator>();
		rbody = GetComponent<Rigidbody2D>();
		sr = GetComponent<SpriteRenderer>();
	}

	void UpdatePlayerStateP() { // update player states - Physics
		grounded = Physics2D.OverlapBox(groundBox.transform.position, groundBox.size / 2.0f, 0.0f, whatIsGround);
		anim.SetBool("Ground", grounded);
		nearWallLeft = Physics2D.OverlapBox(wallBoxLeft.transform.position, wallBoxLeft.size / 2.0f, 0.0f, whatIsGround);
		nearWallRight = Physics2D.OverlapBox(wallBoxRight.transform.position, wallBoxRight.size / 2.0f, 0.0f, whatIsGround);
		if (grounded) {
			canDoubleJump = true;
			canWallStick = true;
		}
	}

	// Update is called once per frame Fixed update is for physics stuff
	void FixedUpdate()
	{
		UpdatePlayerStateP();

		//Set float var for animator
		anim.SetFloat("vSpeed", rbody.velocity.y);

		anim.SetFloat("Speed", Mathf.Abs(horizInput));
		if (horizInput != 0) // If player is giving movement input
		{
			float vAdd = (playerAccel * Time.deltaTime); // v = v1 + a*t
			if (horizInput < 0)
				vAdd *= -1.0f;
			rbody.velocity = new Vector2(rbody.velocity.x + vAdd, rbody.velocity.y);
		}
		else // If player is not giving movement input and we're grounded, we need to slow down if grounded
		{
			float decel;
			if (!grounded)
				decel = playerAirDecel;
			else
				decel = playerDecel;
			float vAdd = (decel * Time.deltaTime); //Calc how much we slow down by
			if (Mathf.Abs(rbody.velocity.x) < Mathf.Abs(vAdd)) // if the slow down amound is more than our current velocity
				rbody.velocity = new Vector2(0, rbody.velocity.y); // just set to 0
			else if (rbody.velocity.x > 0)  // if we're moving right
				rbody.velocity = new Vector2(rbody.velocity.x - vAdd, rbody.velocity.y); // reduce right velocity by vAdd
			else                            // else we're moving left
				rbody.velocity = new Vector2(rbody.velocity.x + vAdd, rbody.velocity.y); // reduce left velocity by vAdd
		}

		//Make sure we're not going over max speed
		if (Mathf.Abs(rbody.velocity.x) > maxSpeedx) //X
		{
			if (rbody.velocity.x > 0)
				rbody.velocity = new Vector2(maxSpeedx, rbody.velocity.y);
			else
				rbody.velocity = new Vector2(-maxSpeedx, rbody.velocity.y);
		}
		if (Mathf.Abs(rbody.velocity.y) > maxSpeedy) //Y
		{
			if (rbody.velocity.y > 0)
				rbody.velocity = new Vector2(rbody.velocity.x, maxSpeedy);
			else
				rbody.velocity = new Vector2(rbody.velocity.x, -maxSpeedy);
		}

		//Char Jump
		JumpChar(jumpFlag);
		jumpFlag = (int)jump.none;

		//Char Stick to walls
		bool wallStuck = wallStuckRight || wallStuckLeft;
		if (canWallStick && !wallStuck && vertInput != -1.0f && !grounded) { // If we can stick to walls and we aren't already
			if (nearWallRight && horizInput == 1.0f)
			{
				FreezeChar(true);
				wallStuckRight = true;
				anim.SetBool("WallStuckRight", wallStuckRight);
				print("DATA");
				setFacingRight(false); // if we're stuck on the right side, we always want to face this way
			}
			if (nearWallLeft && horizInput == -1.0f)
			{
				FreezeChar(true);
				wallStuckLeft = true;
				anim.SetBool("WallStuckLeft", wallStuckLeft);
				setFacingRight(true); //see above
			}
		}
		else if (wallStuck) //check to see if we should drop
		{
			//if (!(((nearWallRight && horizInput == 1.0f) || (nearWallLeft && horizInput == -1.0f)) && vertInput != -1.0f))
			if (vertInput == -1.0f)
			{
				FreezeChar(false);
				wallStuckRight = wallStuckLeft = false;
				anim.SetBool("WallStuckRight", false);
				anim.SetBool("WallStuckLeft", false);
			}
		}


		//Flip character if we're not facing the right way
		if (horizInput > 0 && grounded)
			setFacingRight(true);
		else if (horizInput < 0 && grounded)
			setFacingRight(false);
	}

	void Update() // input read more accurately in update vs fixed update
	{
		//jump and double jump
		if (Input.GetKeyDown(KeyCode.Space)) //do this method only if you want to have keys not be remappable
		{
			if (grounded) //Do a regular jump
			{
				jumpFlag = (int)jump.jump;
			}
			else if (wallStuckLeft || wallStuckRight)
			{ //if we're on a wall, we want to wall jump
				jumpFlag = (int)jump.walljump;
			}
			else if (canDoubleJump && !grounded)
			{ //if we are jumping and not grounded, we better trigger doublejump
				jumpFlag = (int)jump.doublejump;
			}
		}

		//Get horizInput
		horizInput = 0.0f;
		if (Input.GetKey(KeyCode.LeftArrow))
			horizInput -= 1;
		if (Input.GetKey(KeyCode.RightArrow))
			horizInput += 1;

		//GetvertInput
		vertInput = 0.0f;
		if (Input.GetKey(KeyCode.DownArrow))
			vertInput -= 1.0f;
		if (Input.GetKey(KeyCode.UpArrow))
			vertInput += 1.0f;
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (((1 << other.gameObject.layer) & whatIsDeathBox) != 0) // if we enter deathbox
		{ //TODO check if this is only our player's physical hitbox, or if it's all attached box2d's
		  //Teleport player to spawn
			transform.position = spawnPoint.position;
		}
	}

	void setFacingRight(bool right) //True is set to right, false is set to left
	{
		if (right != facingRight){
			Flip();
		}
	}

	void Flip()
	{
		facingRight = !facingRight;
		sr.flipX = !sr.flipX;
	}

	void MoveChar() { } // Add or subtract horizontal velocity based on given input

	void JumpChar(int jumpnum)
	{
		if (jumpnum == (int)jump.jump) //Do a regular jump
		{
			rbody.velocity = new Vector2(0, jumpForce);
		}
		else if (jumpnum == (int)jump.walljump)
		{ //if we're on a wall, we want to wall jump
			FreezeChar(false);
			print("DidThing");
			int jumpdir = 1; //sideways multiplier
			if (wallStuckRight)
				jumpdir *= -1;
			rbody.velocity = new Vector2(jumpForce * jumpdir, jumpForce * 1.2f);
			wallStuckLeft = wallStuckRight = false;
			anim.SetBool("WallStuckRight", wallStuckRight);
			anim.SetBool("WallStuckLeft", wallStuckLeft);
		}
		else if (jumpnum == (int)jump.doublejump)
		{ //if we are jumping and not grounded, we better trigger doublejump
			anim.SetTrigger("DoubleJump");
			canDoubleJump = false;
			rbody.velocity = new Vector2(0, jumpForce);
		}
	}

	void FreezeChar(bool freeze)
	{
		if (freeze) //Freeze
		{
			rbody.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
			canWallStick = false;
		}
		else //Unfreeze
		{
			rbody.constraints = RigidbodyConstraints2D.FreezeRotation;
		}
	}

}
