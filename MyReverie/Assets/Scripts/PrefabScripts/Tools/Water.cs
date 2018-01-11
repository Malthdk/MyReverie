using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : RaycastController {

	//Public statics
	public static bool playerOnPortal;
	public static bool leftArea;

	//Public
	public bool perpendicular, rotated;
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
	private SpriteRenderer playerRender;
	private bool hitTop = true, hitBot = true;
	private int hitCount = 0;

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
		player = GameObject.Find("Player").GetComponent<Player>();
		playerCol = player.GetComponent<BoxCollider2D>();
		playerRender = player.transform.GetChild(0).GetComponent<SpriteRenderer>();

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
			hitCount = 0;
			hitBot = true;
			hitTop = true;
			shootRays = true;
			maskTop.gameObject.SetActive(false);
			maskBot.gameObject.SetActive(false);
		}
	}
	//// TO DO 
	////
	/// Test med moving camera. Needs a smooth camera to work
	/// 
	/// BUGS: Releasing "space" can fuck it up sometimes - fx. mid transport
	/// Going from portal set to portal set can fuck it up. Think it is when you hit your own fx. top rays and then hit the other's top rays.
	/// Building momentum has to be more intuitive if we want to use those types of puzzles.
	/// 

	//Holds all possible interaction scenarios
	void MovePlayer()
	{
		if (!perpendicular)
		{
			if (intolineScript.direction == IntoLine.Direction.Floor && hitCount == 2)
			{
				StartCoroutine(TransformPlayer(IntoLine.Direction.Cieling));
			}
			if (intolineScript.direction == IntoLine.Direction.Cieling && hitCount == 2)
			{
				StartCoroutine(TransformPlayer(IntoLine.Direction.Floor));
			}

			if (intolineScript.direction == IntoLine.Direction.Rightwall && hitCount == 2)
			{
				StartCoroutine(TransformPlayer(IntoLine.Direction.Leftwall));
			}
			if (intolineScript.direction == IntoLine.Direction.Leftwall && hitCount == 2)
			{
				StartCoroutine(TransformPlayer(IntoLine.Direction.Rightwall));
			}
		}
		else if (perpendicular)
		{
			if (intolineScript.direction == IntoLine.Direction.Floor && hitCount == 2)
			{
				StartCoroutine(TransformPlayer(IntoLine.Direction.Leftwall));
			}
			if (intolineScript.direction == IntoLine.Direction.Leftwall && hitCount == 2)
			{
				StartCoroutine(TransformPlayer(IntoLine.Direction.Floor));
			}

			if (intolineScript.direction == IntoLine.Direction.Rightwall && hitCount == 2)
			{
				StartCoroutine(TransformPlayer(IntoLine.Direction.Cieling));
			}
			if (intolineScript.direction == IntoLine.Direction.Cieling && hitCount == 2)
			{
				StartCoroutine(TransformPlayer(IntoLine.Direction.Rightwall));
			}
		}

	}
		
	//Handles the transformation of player step by step
	public IEnumerator TransformPlayer(IntoLine.Direction newDirection)
	{
		//Stop shooting rays on both portals
		shootRays = false;
		otherWater.shootRays = false;

		//Calculating entrance point on water portal
		CalculatePosition();

		yield return new WaitForEndOfFrame();

		//Making player invisible to ensure that he is not seen outsite mask
		playerRender.enabled = false;

		//Setting the new directionState for the player
		IntoLine.instance.direction = newDirection;

		yield return new WaitForEndOfFrame();

		//Making player visible again
		playerRender.enabled = true;

		//Changing the player's position based on whether he is going to a rotated or a nonrotated water portal
		playerCol.transform.position = (otherWater.rotated)?new Vector3(otherPortal.position.x, otherPortal.position.y - offsetInWater, playerCol.transform.position.z):new Vector3(otherPortal.position.x - offsetInWater, otherPortal.position.y, playerCol.transform.position.z);

		//Offset to make it look like comming out of water
		playerCol.transform.Translate(new Vector3(0f, -2f, 0f));

		//Setting the player's velocity
		CalculateVelocity();

		//We start shooting rays again
		shootRays = true;

		yield return new WaitForSeconds(0.5f);

		//Resetting hitCount and hit booleans and starts to shoot rays on opposing portal again
		hitCount = 0;
		otherWater.shootRays = true;
		hitBot = true;
		hitTop = true;
	}

	//Calculates the velocity with which the player should emerge from the water based on the velocity when he landed on the water
	void CalculateVelocity()
	{
		//Creating a new constant velocity if the player hits the water for the first time (this prevents increminental build up of velocity)
		if (playerVelocity == 0)
		{
			//Hardcoding the minimum velocity for portal looping
			if (Mathf.Abs(player.velocity.y) < 12)
			{
				playerVelocity = 15f;
				otherWater.playerVelocity = 15f;
			}
			else
			{
				playerVelocity = player.velocity.y;
				otherWater.playerVelocity = player.velocity.y;	
			}
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
		float centerOfWater;
		float distanceToPlayer;

		if (rotated)
		{
			centerOfWater = bCol.bounds.center.y;
			distanceToPlayer = centerOfWater - playerCol.gameObject.transform.position.y;
		}
		else
		{
			centerOfWater = bCol.bounds.center.x;
			distanceToPlayer = centerOfWater - playerCol.gameObject.transform.position.x;	
		}
		offsetInWater = distanceToPlayer;
	}


	//Handles the masks that hide the player
	void HandleMasks(bool isTop, bool isBelow)
	{
		if (isTop)
		{
			if ( hitCount == 0)
			{
				//Resetting other water so that we know that we can go from water to water without going through water or landing on ground but simply through the air.
				ResetOtherPortal();

				otherTop.gameObject.SetActive(true);
				maskBot.gameObject.SetActive(true);

				maskTop.gameObject.SetActive(false);
				otherBot.gameObject.SetActive(false);
			}
		}
		else if (isBelow)
		{
			if ( hitCount == 0)
			{
				ResetOtherPortal();

				maskTop.gameObject.SetActive(true);
				otherBot.gameObject.SetActive(true);

				maskBot.gameObject.SetActive(false);
				otherTop.gameObject.SetActive(false);
			}
		}
	}

	//Resets the other portal so that it can freshly be interacted with again
	void ResetOtherPortal()
	{
		otherWater.hitCount = 0;
		otherWater.hitTop = true;
		otherWater.hitBot = true;
	}

	///ALL BELOW
	///Needs to be formatted into 1 function
	void ShootTopRays()
	{
		if (!rotated)
		{
			float rayLength = 3f;
			for (int i = 0; i < verticalRayCount; i++)
			{
				Vector2 rayOrigin = raycastOrigins.topLeft;
				rayOrigin.y = rayOrigin.y + 1.25f;

				rayOrigin += Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, mask);

				Debug.DrawRay(rayOrigin, Vector2.up * rayLength, Color.red);

				if (hit)
				{
					HandleMasks(true, false);
					if (hitTop)
					{
						hitCount ++;
						hitTop = false;
					}
					MovePlayer();
				}
			}	
		}
		if (rotated)
		{
			float rayLength = 3f;
			for (int i = 0; i < horizontalRayCount; i++)
			{
				Vector2 rayOrigin = raycastOrigins.bottomLeft;
				rayOrigin.x = rayOrigin.x - 1.25f;

				rayOrigin += Vector2.up * (horizontalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.left, rayLength, mask);

				Debug.DrawRay(rayOrigin, Vector2.left * rayLength, Color.red);

				if (hit)
				{
					HandleMasks(true, false);
					if (hitTop)
					{
						hitCount ++;
						hitTop = false;
					}
					MovePlayer();
				}
			}	
		}
	}

	void ShootBottomRays()
	{
		if (!rotated)
		{
			float rayLength = 3f;
			for (int i = 0; i < verticalRayCount; i++)
			{
				Vector2 rayOrigin = raycastOrigins.bottomLeft;
				rayOrigin.y = rayOrigin.y - 1.25f;

				rayOrigin += Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, mask);

				Debug.DrawRay(rayOrigin, Vector2.down * rayLength, Color.green);

				if (hit)
				{
					HandleMasks(false, true);
					if (hitBot)
					{
						hitCount ++;
						hitBot = false;
					}
					MovePlayer();
				}
			}	
		}
		if (rotated)
		{
			float rayLength = 3f;
			for (int i = 0; i < horizontalRayCount; i++)
			{
				Vector2 rayOrigin = raycastOrigins.bottomRight;
				rayOrigin.x = rayOrigin.x + 1.25f;

				rayOrigin += Vector2.up * (horizontalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right, rayLength, mask);

				Debug.DrawRay(rayOrigin, Vector2.right * rayLength, Color.green);

				if (hit)
				{
					HandleMasks(false, true);
					if (hitBot)
					{
						hitCount ++;
						hitBot = false;
					}
					MovePlayer();
				}
			}	
		}
	}
}
