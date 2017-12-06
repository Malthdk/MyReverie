using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

	public float maxJumpHeight = 3.5f;				//Max JumpHeight
	public float maxJumoHeightPainting;
	public float minJumpHeight = 1f;				//Minimum JumpHeight
	[HideInInspector]
	public float timeToJumpApex = .65f;				//Time to reach highest point (4875 was before)
	public float accelerationTimeAirborn = .2f;		//Acceleration while airborne
	public float accelerationTimeGrounded = .5f;	//Acceleration while grounded
	[HideInInspector]
	public float moveSpeed = 9;	
	public float airSpeed = 6.4f, groundSpeed = 9.4f;
	public float paintingSpeed;
	[HideInInspector]
	public Vector2 input;
	[HideInInspector]
	public int wallDirX;

	public float gravity;					//gramaxJumpVelocity to player
	[HideInInspector]
	public float maxJumpVelocity, minJumpVelocity;			//Min jump velocity
	public Vector3 velocity;				//velocity
	[HideInInspector]
	public float velocityXSmoothing;		//smoothing on velocity

	Controller2D controller;					//calling controller class
	IntoLine intoLine;

	private Animator animator;		//ANIMATION
	[HideInInspector]
	public float gravityModifierFall;
	public float gravityModifierJump;

	// Has player landed?
	private bool landed = true;

	public float ghostJumpingBuffer = 0.15f;
	private float timeSinceJump;

	//ParticleSystems for jump
	private ParticleSystem jumpParticles;

	public SpriteMask spriteMask;

	//Can the character be moved on X or Y axis. 
	[HideInInspector]
	public bool movementUnlocked;
	private float targetVelocityX;

	//Is the controlls inverse
	[HideInInspector]
	public bool inverseControlX;
	[HideInInspector]
	public static Player _instance;

	// The players collider
	private BoxCollider2D boxCol;

	// The starting values for the players collider
	private Vector2 startColSize;


	private float pitchValue;

	public static Player instance {	// Makes it possible to call script easily from other scripts
		get {
			if (_instance == null) {
				_instance = FindObjectOfType<Player>();
			}
			return _instance;
		}
	}

	void Start () 
	{
		movementUnlocked = true;
		boxCol = GetComponent<BoxCollider2D>();
		startColSize = boxCol.size;
		controller = GetComponent<Controller2D>();
		intoLine = GetComponent<IntoLine>();
		animator = transform.GetComponentInChildren<Animator>();		//ANIMATION
		jumpParticles = transform.GetChild(3).GetChild(0).GetComponent<ParticleSystem>();
		spriteMask = transform.GetChild(4).GetComponent<SpriteMask>();
	}

	void Update () 
	{
		gravity = -(2* maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);		//Gravity defined based of jumpheigmaxJumpVelocityto reach highest point

		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;				//Max jump velocity defined based on gravity and time to reach highest point
		minJumpVelocity = Mathf.Sqrt(2*Mathf.Abs(gravity) * minJumpHeight);	//Min jump velocity defined based on gravity and min jump height

		input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));		//Input from player

		if (inverseControlX)
		{
			if (input.x > 0 || input.y > 0)
			{
				input.x = -input.x;
				input.y = -input.y;
			}
			else if (input.x < 0 || input.y < 0)
			{
				input.x = Mathf.Abs(input.x);
				input.y = Mathf.Abs(input.y);
			}
				
		}
		wallDirX = (controller.collisions.left)? -1:1;											//wall direction left or right

		targetVelocityX = (controller.playerOnLeftWall || controller.playerOnRightWall)?input.y * moveSpeed:input.x * moveSpeed;


		if (input.x == 1 || input.x == -1) //[BUG REPORT] Minor problem with changing direction and keeping same speed
		{
			accelerationTimeGrounded = 0.25f;
		}
		else
		{
			accelerationTimeGrounded = 0.05f; //0.05
		}

		velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below)?accelerationTimeGrounded:accelerationTimeAirborn);		//Calculating velocity x both airborn and on ground with smoothing

		animator.SetFloat("speed", Mathf.Abs(velocity.x));			//ANIMATION
		animator.SetBool("ground", controller.collisions.below);	//ANIMATION
		animator.SetBool("transforming", intoLine.transforming);
		animator.SetFloat("vSpeed", velocity.y);					//ANIMATION
			
		if (!controller.collisions.below) {
			timeSinceJump += Time.deltaTime;
		} else {
			timeSinceJump = 0f;
		}

		if (Input.GetButtonDown("Jump"))
		{
			if (controller.collisions.below)
			{
				animator.SetBool("jumping", true);
				Jump();
			}
		}

		if (!controller.collisions.below)
		{
			landed = false;
			moveSpeed = airSpeed;
		}
		else if (controller.collisions.below && landed == false)
		{
			moveSpeed = groundSpeed;
			landed = true;
			animator.SetBool("landed", true);
			spriteMask.enabled = true;
			boxCol.size = startColSize;
		}
		if (Mathf.Sign(velocity.y) == -1)
		{
			timeToJumpApex = gravityModifierFall; //55 //0.75
			jumpParticles.Stop();		
		}
		else 
		{
			timeToJumpApex = gravityModifierJump;//0.92f;	//72
		}

		if (Input.GetButtonUp("Jump"))					//For variable jump
		{
			if (velocity.y > minJumpVelocity) 
			{
				velocity.y = minJumpVelocity;									//When space is released set velocity y to minimum jump velocity
				AkSoundEngine.SetRTPCValue ("Pitch", 2);	//Sound
			}
		}
	if (movementUnlocked == true)
		{
			velocity.y += gravity * Time.deltaTime;	
		}
		controller.Move((movementUnlocked)?velocity * Time.deltaTime:new Vector3(0,0,0), (movementUnlocked)?input:new Vector2(0,0));//Moving character && Checking if Movement is unlocked. If it is add small downward velocity and freeze input.


		if (controller.collisions.above  || controller.collisions.below)		//If raycasts hit above or below, velocity on y axis stops
		{
			velocity.y = 0;
		}
	}

	void Jump() 
	{
		Vector2 newSize = new Vector2(1.4f, 1.41f);
		boxCol.size = newSize;
		velocity.y = maxJumpVelocity;
		spriteMask.enabled = false;
		AkSoundEngine.SetRTPCValue ("Pitch", 0);
		AkSoundEngine.PostEvent ("Jump", gameObject);
		//jumpParticles.Play();						//ParticleSystem
		animator.SetBool("jumping", false);
	}
}