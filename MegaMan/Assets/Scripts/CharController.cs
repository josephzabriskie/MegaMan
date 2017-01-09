using UnityEngine;
using System.Collections;

public class CharController : MonoBehaviour {

	enum jump { none, jump, doublejump, walljump };
	int jumpFlag;
	float horizInput;
	float vertInput;

	//player public variables
	float maxRunSpeed = 10.0f; //How fast can the player move through running
	float maxSpeedx = 30.0f; // What is our x speed capped at
	float maxSpeedy = 30.0f;// What is our y speed capped at
	float groundAccel = 50.0f; // how fast do we accelerate when pressing movement keys
	float groundDecel = 75.0f; // how fast do we decelerate when we don't press movement keys
	float airAccel = 15.0f;
	float airDecel = 0.0f; // how fast do we decelerate in the air?
	float jumpForce = 11.5f; // how high do we jump
	float gravAccel = -32.0f;
	float wallFriction = 25.0f;
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
	//bool wallStuckRight = false;
	//bool wallStuckLeft = false;
	bool nearWallRight = false;
	bool nearWallLeft = false;
	//bool canWallStick = true; // we can only stick to walls once

	//Gameplaystates
	bool canDoubleJump = false;
	bool canWallJumpRight = false;
	bool canWallJumpLeft = false;
	bool isWallSliding = false;

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

	void UpdatePlayerStateP() { // update player states - Physics. This is the only place where we set these values. Hopefully
		//Check for grounded
		grounded = Physics2D.OverlapBox(groundBox.transform.position, groundBox.size / 2.0f, 0.0f, whatIsGround);
		anim.SetBool("Ground", grounded);

		nearWallLeft = Physics2D.OverlapBox(wallBoxLeft.transform.position, wallBoxLeft.size / 2.0f, 0.0f, whatIsGround);
		nearWallRight = Physics2D.OverlapBox(wallBoxRight.transform.position, wallBoxRight.size / 2.0f, 0.0f, whatIsGround);
		//checked if we are wall sliding
		wallSlideChar();

		//Flip character if we're not facing the right way (only on ground)
		if (horizInput > 0 && grounded)
			setFacingRight(true);
		else if (horizInput < 0 && grounded)
			setFacingRight(false);
		
		if (grounded) {
			canDoubleJump = true;
			//canWallStick = true;
		}
	}

	// Update is called once per frame Fixed update is for physics stuff
	void FixedUpdate()
	{
		UpdatePlayerStateP();
		MoveCharX();
		applyGravity ();

		//WallStick code for wall jump
		//WallStickChar(nearWallRight, nearWallLeft);

		//Char Jump
		JumpChar(jumpFlag);
		jumpFlag = (int)jump.none;

		//Make sure we're not going over max speed
		maxSpeedCheck();

		//Set float var for animator
		anim.SetFloat("vSpeed", rbody.velocity.y);
		anim.SetFloat("Speed", Mathf.Abs(rbody.velocity.x));
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
			//else if (wallStuckLeft || wallStuckRight)
			else if (isWallSliding)
			{ //if we're on a wall, we want to wall jump
				jumpFlag = (int)jump.walljump;
			}
			else if (canDoubleJump && !grounded)
			{ //if we are jumping and not grounded, we better trigger doublejump
				jumpFlag = (int)jump.doublejump;
			}
		}
		if (Input.GetKeyDown (KeyCode.Escape)) 
		{
			Application.Quit ();
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

	void MoveCharX() // Add or subtract horizontal velocity based on given input
	{
		float vAdd;
		bool movingopposite = false; // set to true if the player's input is the opposite of their direction
		if (horizInput == 1.0f && rbody.velocity.x < 0 || horizInput == -1.0f && rbody.velocity.x > 0)
			movingopposite = true;
		
		if (horizInput != 0) // If player is giving movement input
		{
			if (grounded)
			{
				vAdd = groundAccel;
				if (movingopposite) // to help reverse direction on ground faster
					vAdd *= 1.5f;
			}
			else
				vAdd = airAccel;
			vAdd = vAdd * Time.deltaTime;
			if (horizInput < 0)
				vAdd *= -1.0f;
			bool breakmaxspeed = Mathf.Abs (rbody.velocity.x + vAdd) > maxRunSpeed;
			if (!breakmaxspeed || breakmaxspeed && movingopposite) // We add the velocity only if it doesn't put us over max run speed
				rbody.velocity = new Vector2 (rbody.velocity.x + vAdd, rbody.velocity.y);
			else //If it would put us over max
				if (Mathf.Abs (rbody.velocity.x) < maxRunSpeed)// Make sure we're not over max
					rbody.velocity = new Vector2 (maxRunSpeed * horizInput, rbody.velocity.y); //And then set to max
		}
		else // If player is not giving movement input and we're grounded, we need to slow down if grounded
		{
			if (grounded)
				vAdd = groundDecel;
			else
				vAdd = airDecel;
			vAdd = (vAdd * Time.deltaTime); //Calc how much we slow down by
			if (Mathf.Abs (rbody.velocity.x) < Mathf.Abs (vAdd)) // if the slow down amound is more than our current velocity
			{
				rbody.velocity = new Vector2 (0, rbody.velocity.y); // just set to 0
				return;
			}
			else if (rbody.velocity.x > 0)  // if we're moving right
				vAdd *= -1;
			rbody.velocity = new Vector2(rbody.velocity.x + vAdd, rbody.velocity.y); // reduce left velocity by vAdd
		}
	}

	void JumpChar(int jumpnum)
	{
		if (jumpnum == (int)jump.jump) //Do a regular jump
		{
			rbody.velocity = new Vector2(rbody.velocity.x, jumpForce);
		}
		else if (jumpnum == (int)jump.walljump) //Do a wall jump
		{
			if (nearWallLeft && !nearWallRight || nearWallRight && !nearWallLeft)
			{
				if (nearWallLeft)
					rbody.velocity = new Vector2 (1.3f * maxRunSpeed, jumpForce);
				if (nearWallRight)
					rbody.velocity = new Vector2 (-1.3f * maxRunSpeed, jumpForce);
			}
				
		}
		else if (jumpnum == (int)jump.doublejump) //If we are jumping and not grounded, we better trigger doublejump
		{
			anim.SetTrigger("DoubleJump");
			float mindoublejumpvel = 0.5f * maxRunSpeed;
			canDoubleJump = false;
			if (horizInput == 1.0f && rbody.velocity.x < 0 || horizInput == -1.0f && rbody.velocity.x > 0) // Flip velocity if moving opposite direction
				rbody.velocity = new Vector2 (rbody.velocity.x * -1.0f, jumpForce);
			else if (Mathf.Abs (rbody.velocity.x) < mindoublejumpvel && horizInput != 0.0f)
				rbody.velocity = new Vector2 (mindoublejumpvel * horizInput, jumpForce);
			else
				rbody.velocity = new Vector2(rbody.velocity.x, jumpForce);
		}
	}

	void applyGravity()
	{
		float vAdd;
		float friction = 0.0f;
		if (isWallSliding)
			friction = wallFriction * Time.deltaTime;
		if (rbody.velocity.y > 0)
			friction *= -1.0f;
		Vector2 curvel = rbody.velocity;
		vAdd = gravAccel * Time.deltaTime + friction;
		rbody.velocity = new Vector2 (curvel.x, curvel.y + vAdd);
	}

	void wallSlideChar()
	{
		if (nearWallRight && horizInput == 1.0f || nearWallLeft && horizInput == -1.0f)
			isWallSliding = true;
		if (isWallSliding)
			if (nearWallRight && horizInput == -1.0f || nearWallLeft && horizInput == 1.0f || !nearWallLeft && !nearWallRight)
				isWallSliding = false;
	}

	void maxSpeedCheck()
	{
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
	}
}
