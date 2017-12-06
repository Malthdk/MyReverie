using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntoLine : RaycastController {
	
	//Publics
	public Direction direction;
	public float yOffsetRightLeft = 0.2f;
	public float yOffsetUpDown = -0.8f;
	[HideInInspector]
	public bool transforming;
	[HideInInspector]
	public bool inputLocked;
	[HideInInspector]
	public bool downArrow, upArrow, rightArrow, leftArrow;

	//This is for LovedOne mechanic
	public bool LovedOne;

	//Privates
	private Player player;
	private Controller2D controller;
	private Animator animator;
	public static bool cannotTransform;

	//For portal
	[HideInInspector]
	public Transform otherPortal;
	public Direction portalDirection;
	public Vector3 portalTransformation = new Vector3(0f, 0f, 0f);

	//To check if we can transform
	private bool transformBlocked;
	private BoxCollider2D myCollider;
	private float myWidth;
	public LayerMask hitMask;

	//For checking if grounded
	private Vector2 verticalRayOrigin;
	RaycastHit2D snapHit;

	public static IntoLine _instance;

	public static IntoLine instance {	// Makes it possible to call script easily from other scripts
		get {
			if (_instance == null) {
				_instance = FindObjectOfType<IntoLine>();
			}
			return _instance;
		}
	}

	public enum Direction
	{
		Floor,
		Cieling,
		Rightwall,
		Leftwall
	}

	public override void Start () 
	{
		base.Start();
		player = GetComponent<Player>();
		controller = GetComponent<Controller2D>();
		//direction = Direction.Floor;
		animator = transform.GetComponentInChildren<Animator>();
		myCollider = gameObject.GetComponent<BoxCollider2D>();

		//This is for locking transitions on corners and T-sections - Width has to be calculated differently based on initial position when starting
		if (direction == Direction.Floor || direction == Direction.Cieling)
		{
			myWidth = myCollider.bounds.extents.x;	
		}
		else if (direction == Direction.Rightwall || direction == Direction.Leftwall)
		{
			myWidth = myCollider.bounds.extents.y;
		}

		inputLocked = false;
	}

	public override void Update () 
	{
		base.Update();
		//Check if on T-section - move to when transform is about to happen.
		CheckIfCanTransform();

		//Input variables
		downArrow = Input.GetKey(KeyCode.DownArrow);
		upArrow = Input.GetKey(KeyCode.UpArrow);
		rightArrow = Input.GetKey(KeyCode.RightArrow);
		leftArrow = Input.GetKey(KeyCode.LeftArrow);

		//Direction states for the player
		switch(direction)
		{
		case Direction.Floor:
			//INPUT
			if (inputLocked == false && !transformBlocked)
			{
//				Debug.Log("pltFloorLeft " + pltFloorLeft);
				if (downArrow && controller.collisions.below)
				{
					StartCoroutine(TransformPlayer(new Vector3(0f, yOffsetUpDown, 0f), Direction.Cieling));
				}
				else if (((LovedOne)?leftArrow:rightArrow) && controller.collisions.right)
				{
					StartCoroutine(TransformPlayer(new Vector3(0f, yOffsetRightLeft, 0f), Direction.Rightwall));
				}
				else if (((LovedOne)?rightArrow:leftArrow)  && controller.collisions.left)
				{
					StartCoroutine(TransformPlayer(new Vector3(0, yOffsetRightLeft, 0f), Direction.Leftwall));
				}
			}
			//TRANSFORMATIONS
			transform.rotation = Quaternion.Euler(0f, 0f, 0f);
			controller.playerOnground = true;
			controller.playerOnCieling = controller.playerOnRightWall = controller.playerOnLeftWall = false;

			//This is for inverse control on LovedOne
			player.inverseControlX = (LovedOne)?true:false;

			break;

		case Direction.Cieling:
			//INPUT
			if (inputLocked == false && !transformBlocked)
			{
				if (upArrow && controller.collisions.below)
				{
					StartCoroutine(TransformPlayer(new Vector3(0f, yOffsetUpDown, 0f), Direction.Floor));
				}
				else if (((LovedOne)?leftArrow:rightArrow) && controller.collisions.left)
				{
					StartCoroutine(TransformPlayer(new Vector3(0f, yOffsetRightLeft, 0f), Direction.Rightwall));
				}
				else if (((LovedOne)?rightArrow:leftArrow) && controller.collisions.right)
				{
					StartCoroutine(TransformPlayer(new Vector3(0f, yOffsetRightLeft, 0f), Direction.Leftwall));
				}
			}

			//TRANSFORMATIONS
			transform.rotation = Quaternion.Euler(0f, 0f, 180f);
			controller.playerOnCieling = true;
			controller.playerOnground = controller.playerOnRightWall = controller.playerOnLeftWall = false;

			//This is for inverse control on LovedOne
			player.inverseControlX = (LovedOne)?false:true;
			break;

		case Direction.Rightwall:
			//INPUT
			if (inputLocked == false && !transformBlocked)
			{
				if (rightArrow && controller.collisions.below)
				{
					StartCoroutine(TransformPlayer(new Vector3(0f, yOffsetUpDown, 0f), Direction.Leftwall));
				}
				else if (((LovedOne)?downArrow:upArrow) && controller.collisions.right)
				{
					StartCoroutine(TransformPlayer(new Vector3(0f, yOffsetRightLeft, 0f), Direction.Cieling));
				}
				else if (((LovedOne)?upArrow:downArrow) && controller.collisions.left)
				{
					StartCoroutine(TransformPlayer(new Vector3(0f, yOffsetRightLeft, 0f), Direction.Floor));
				}
			}

			//TRANSFORMATIONS
			transform.rotation = Quaternion.Euler(0f, 0f, 90f);
			controller.playerOnRightWall = true;
			controller.playerOnground = controller.playerOnCieling = controller.playerOnLeftWall = false;

			//This is for inverse control on LovedOne
			player.inverseControlX = (LovedOne)?true:false;
			break;

		case Direction.Leftwall:
			//INPUT
			if (inputLocked == false && !transformBlocked)
			{
				if (leftArrow && controller.collisions.below)
				{
					StartCoroutine(TransformPlayer(new Vector3(0f, yOffsetUpDown, 0f), Direction.Rightwall));
				}
				else if (((LovedOne)?upArrow:downArrow) && controller.collisions.right)
				{
					StartCoroutine(TransformPlayer(new Vector3(0f, yOffsetRightLeft, 0f), Direction.Floor));
				}
				else if (((LovedOne)?downArrow:upArrow) && controller.collisions.left)
				{
					StartCoroutine(TransformPlayer(new Vector3(0f, yOffsetRightLeft, 0f), Direction.Cieling));
				}
			}

			//TRANSFORMATIONS
			transform.rotation = Quaternion.Euler(0f, 0f, -90f);
			controller.playerOnLeftWall = true;
			controller.playerOnground = controller.playerOnCieling = controller.playerOnRightWall = false;

			//This is for inverse control on LovedOne
			player.inverseControlX = (LovedOne)?false:true;

			break;
		}
	}

	public void ResetDirection(Direction directionState)
	{
		direction = directionState;
	}

	public IEnumerator TransformPlayer(Vector3 transformation, Direction directionState)
	{
		inputLocked = true;

		//If we are in air and transforming onto side we want to first rotate character before transforming, for animations sake.
		//if (!controller.collisions.below)
		//{
		//	if (controller.collisions.right)
		//	{
		//		direction = Direction.Rightwall;
		//	}
		//	else if (controller.collisions.left)
		//	{
		//		direction = Direction.Leftwall;
		//	}
		//}
		yield return new WaitForEndOfFrame();

		AkSoundEngine.PostEvent ("Transition", gameObject);
		transforming = true;
		player.movementUnlocked = false; //Locks movement and stops raycasting
		player.velocity.x = 0;
		player.velocity.y = 0;
		ParticleSystem particleEffect = player.gameObject.transform.GetChild(2).GetChild(0).GetComponent<ParticleSystem>();
		particleEffect.Play();
		animator.SetTrigger("goDown");
		controller.collisions.left = controller.collisions.right = false; //Quick fix for at sørge for at den ikke fortsat tror er er collisions. Dette gjorde at man kunne transforme lige efter en transform.


		yield return new WaitForSeconds(0.4f);

		Player.instance.spriteMask.enabled = false;

		particleEffect.Stop();
		if (Portal.playerOnPortal)
		{
			PortalTransform(otherPortal); //If is on portal
		}
		else
		{
			direction = directionState;
			transform.Translate(transformation);
		}

		yield return new WaitForSeconds(0.1f);

		Player.instance.spriteMask.enabled = true;
		CheckIfOnGround(); //Shoot a ray downwards to check if we are on ground, if we are not then make sure that we are. 
		animator.SetTrigger("goUp");
		particleEffect.Play();

		yield return new WaitForSeconds(0.5f);

		particleEffect.Stop();
		transforming = false;

		yield return new WaitForEndOfFrame();

		inputLocked = false;
		player.movementUnlocked = true;		//Unlocks movement and starts raycasting

		yield return new WaitForEndOfFrame();
	}

	void PortalTransform(Transform portal)
	{
		if (Portal.playerOnPortal)
		{
			transform.position = new Vector3(portal.position.x, portal.position.y, portal.position.z);
			direction = portalDirection;
			transform.Translate(portalTransformation);
		}
	}

	void CheckIfCanTransform()
	{
		if (controller.collisions.below)
		{
			Vector2 lineCastPos = transform.position.toVector2() - transform.right.toVector2() * (myWidth -0.15f) + -transform.up.toVector2() * 1.2f;//-Vector2.down * 1.2f;	

			transformBlocked = Physics2D.Linecast(lineCastPos, lineCastPos + transform.right.toVector2() * (myWidth -0.15f) * 2, hitMask); //new Vector2(lineCastPos.x + myWidth * 2, lineCastPos.y)

			Debug.DrawLine(lineCastPos, lineCastPos + transform.right.toVector2() * (myWidth -0.15f) * 2, Color.green);	
		}
		else
		{
			transformBlocked = false;
		}
	}

	//This function shoots rays downwards and moves the player with the distance of the rays (used for snapping after transformation)
	void CheckIfOnGround()
	{
		//we have to shoot different rays based on the players current Direction state
		for (int i = 0; i < verticalRayCount; i ++)
		{
			if (controller.playerOnLeftWall)
			{
				verticalRayOrigin = raycastOrigins.bottomLeft;
				verticalRayOrigin += Vector2.up * (horizontalRaySpacing * i);	
			}
			else if (controller.playerOnRightWall)
			{
				verticalRayOrigin = raycastOrigins.bottomRight;
				verticalRayOrigin += Vector2.up * (horizontalRaySpacing * i);	
			}
			else if (controller.playerOnCieling)
			{
				verticalRayOrigin = raycastOrigins.topLeft;
				verticalRayOrigin += Vector2.right * (verticalRaySpacing * i);	
			}	
			else if (controller.playerOnground)
			{
				verticalRayOrigin = raycastOrigins.bottomLeft;
				verticalRayOrigin += Vector2.right * (verticalRaySpacing * i);	
			}
			snapHit = Physics2D.Raycast(verticalRayOrigin, -this.transform.up, 5f, hitMask);
			Debug.DrawRay(verticalRayOrigin, -this.transform.up * 5f, Color.green); 
		}

		//if snaphit hits then we move the player based on the distance of the ray
		if (snapHit)
		{
			Debug.Log("hit distance=" + snapHit.distance);
			transform.Translate(new Vector3(0f, -snapHit.distance, 0f));
		}
	}
}
