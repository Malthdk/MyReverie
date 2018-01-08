using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : RaycastController {

	//Public statics
	public static bool playerOnPortal;
	public static bool leftArea;

	//Public
	public bool perpendicular;
	public Transform otherPortal;
	public LayerMask mask;

	//Private
	private IntoLine intolineScript;
	private SpriteMask maskBot;
	private SpriteMask maskTop;
	private SpriteMask otherBot;
	private SpriteMask otherTop;
	private Water otherWater;
	private Player player;

	private BoxCollider2D playerCol;
	private BoxCollider2D bCol;
	[HideInInspector]
	private float playerVelocity;
	private float offsetInWater;
	[HideInInspector]
	public bool shootRays, hasPlayer;

	public override void Awake() 
	{
		base.Awake();
		shootRays = true;

		playerCol = GameObject.Find("Player").GetComponent<BoxCollider2D>();
		player = GameObject.Find("Player").GetComponent<Player>();

		bCol = gameObject.GetComponent<BoxCollider2D>();
		intolineScript = IntoLine.instance.GetComponent<IntoLine>();
		maskBot = transform.GetChild(0).GetComponent<SpriteMask>();
		maskTop = transform.GetChild(1).GetComponent<SpriteMask>();
		otherBot = otherPortal.GetChild(0).GetComponent<SpriteMask>();
		otherTop = otherPortal.GetChild(1).GetComponent<SpriteMask>();
		otherWater = otherPortal.GetComponent<Water>();
	}
	

	public override void Update () 
	{
		base.Update();
		UpdateRaycastOrigins();
		CalculateRaySpacing();

		if (shootRays)
		{
			ShootTopRays();
			ShootBottomRays();	
		}

		//Resets a bunch of figures for next "water interaction"
		if (Controller2D.instance.collisions.below)
		{
			playerVelocity = 0;
			shootRays = true;
			maskTop.gameObject.SetActive(false);
			maskBot.gameObject.SetActive(false);
		}

//		if (hasPlayer)
//		{
//			if (intolineScript.direction == IntoLine.Direction.Floor)
//			{
//				if (Mathf.Abs(playerCol.bounds.center.y) >= Mathf.Abs(maskBot.bounds.center.y))
//				{
//					StartCoroutine(TransformPlayer(IntoLine.Direction.Cieling));
//				}
//			}
//			if (intolineScript.direction == IntoLine.Direction.Cieling)
//			{
//				if (Mathf.Abs(playerCol.bounds.center.y) <= Mathf.Abs(maskTop.bounds.center.y))
//				{
//					StartCoroutine(TransformPlayer(IntoLine.Direction.Floor));
//				}
//			}
//		}
	}
	//// TO DO 
	////
	/// NY måde at se hvornår spilleren skal transformeres
	/// Bedre Maske system - (den er uregelmæssig nu)
	/// 
	/// Når det virker "perfekt" med disse to - implimenter resten.
	////
	void OnTriggerStay2D(Collider2D other)
	{
		if (other.tag == "player")
		{
			if (intolineScript.direction == IntoLine.Direction.Floor)
			{
				if (Mathf.Abs(playerCol.bounds.center.y) >= Mathf.Abs(maskBot.bounds.center.y))
				{
					StartCoroutine(TransformPlayer(IntoLine.Direction.Cieling));
				}
			}
			if (intolineScript.direction == IntoLine.Direction.Cieling)
			{
				if (Mathf.Abs(playerCol.bounds.center.y) <= Mathf.Abs(maskTop.bounds.center.y))
				{
					StartCoroutine(TransformPlayer(IntoLine.Direction.Floor));
				}
			}
		}
	}

	void ShootTopRays()
	{
		float rayLength = 1f + skinWidth;
		for (int i = 0; i < verticalRayCount; i++)
		{
			Vector2 rayOrigin = raycastOrigins.topLeft;

			rayOrigin += Vector2.right * (verticalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, mask);

			Debug.DrawRay(rayOrigin, Vector2.up * rayLength, Color.red);

			if (hit)
			{
				Debug.Log(this.gameObject.name + " has hit a TopRay");
				HandleMasks(true, false);
			}
		}
	}

	void ShootBottomRays()
	{
		float rayLength = 1f + skinWidth;
		for (int i = 0; i < verticalRayCount; i++)
		{
			Vector2 rayOrigin = raycastOrigins.bottomLeft;

			rayOrigin += Vector2.right * (verticalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, mask);

			Debug.DrawRay(rayOrigin, Vector2.down * rayLength, Color.green);

			if (hit)
			{
				Debug.Log(this.gameObject.name + " has hit a BotRay");
				HandleMasks(false, true);
			}
		}
	}

		
	//Moves the player and controlls mask behaviour
	public IEnumerator TransformPlayer(IntoLine.Direction newDirection)
	{
		shootRays = false;
		otherWater.shootRays = false;
		CalculatePosition();

		yield return new WaitForEndOfFrame();
		//player.movementUnlocked = false;

		playerCol.transform.position = new Vector3(otherPortal.position.x - offsetInWater, otherPortal.position.y, playerCol.transform.position.z);
		IntoLine.instance.direction = newDirection;
		playerCol.transform.Translate(new Vector3(0f, 1f, 0f)); //Offset to make it look like comming out of water
		CalculateVelocity();

		yield return new WaitForEndOfFrame();
		//player.movementUnlocked = true;
		shootRays = true;
		otherWater.shootRays = true;
	}

	//Calculates the velocity with which the player should emerge from the water based on the velocity when he landed on the water
	void CalculateVelocity()
	{
		//Creating a new constant velocity if the player hits the water for the first time (this prevents increminental build up of velocity)
		if (playerVelocity == 0)
		{
			playerVelocity = player.velocity.y;
			otherWater.playerVelocity = player.velocity.y;
		}

		if (player.velocity.y < 0)
		{
			player.velocity.y = 0;
			player.velocity.y = Mathf.Abs(playerVelocity);
			player.velocity.x = -Player.instance.velocity.x; //negative x to continue movement
		}
	}

	//Calculates the position on which the player should emerge from the water based on the position he landed on the water
	void CalculatePosition()
	{
		float centerOfWater = bCol.bounds.center.x;
		float distanceToPlayer = centerOfWater - playerCol.gameObject.transform.position.x;

		offsetInWater = distanceToPlayer;
	}


	//Handles the masks that hide the player
	void HandleMasks(bool isTop, bool isBelow)
	{
		shootRays = false;
		if (isTop)
		{
			maskBot.gameObject.SetActive(true);
			maskTop.gameObject.SetActive(false);

			otherBot.gameObject.SetActive(false);
			otherTop.gameObject.SetActive(true);
		}
		else if (isBelow)
		{
			maskBot.gameObject.SetActive(false);
			maskTop.gameObject.SetActive(true);

			otherBot.gameObject.SetActive(true);
			otherTop.gameObject.SetActive(false);
		}
	}
}
