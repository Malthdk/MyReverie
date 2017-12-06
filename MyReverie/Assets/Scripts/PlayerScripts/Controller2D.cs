using UnityEngine;
using System.Collections;

public class Controller2D : RaycastController {

	float maxClimbAngle = 80;					//Highest angle for climbing slopes
	float maxDescendAngle = 75;					//Highest angle for controlling descension on sloeps

	public Collisioninfo collisions;			//Calling collisions info from struct
	public LevelManager levelmanager;

	[HideInInspector]
	public Vector2 playerInput;
	public Checkpoint checkpoint;

	[HideInInspector]
	public bool facingRight = true;		//For switching face direction

	//Graphics and splat
	private Transform graphicsTransform;
	public float splatTime = 0.0005f;

	[HideInInspector]
	public static Controller2D _instance;

	//PaintParticles
	private ParticleSystem paintParticles;
	private GameObject paintParticleGO;
	private bool isEmitting;

	private int LayerGround;

	public bool playerOnLeftWall, playerOnRightWall, playerOnground, playerOnCieling;

	[HideInInspector]
	public Vector2 horizontalRayOrigin, verticalRayOrigin;

	public static Controller2D instance {	// Makes it possible to call script easily from other scripts
		get {
			if (_instance == null) {
				_instance = FindObjectOfType<Controller2D>();
			}
			return _instance;
		}
	}



	public override void Start()
	{
		base.Start();
		LayerGround = LayerMask.NameToLayer("MovingPlatform");
		collisions.faceDir = 1;					//Face direction set to 1
		levelmanager = FindObjectOfType<LevelManager>();
		checkpoint = FindObjectOfType<Checkpoint>();
		graphicsTransform = transform.GetChild(0).GetComponent<Transform>();
	}

	public override void Update()
	{
		base.Update();
	}

	public void Move(Vector3 velocity, bool standingOnPlatform)		//Small overload function for the platformcontroller to use without any player input. Ergo Vector2.zero. 
	{
		Move (velocity, Vector2.zero, standingOnPlatform);
	}

	public void Move(Vector3 velocity, Vector2 input, bool standingOnPlatform = false)		//Move function
	{
		UpdateRaycastOrigins ();
		collisions.Reset();							//Resetting all collisions
		collisions.velocityOld = velocity;			
		playerInput = input;						//The player input

		if (velocity.x != 0)						
		{
			collisions.faceDir = (int)Mathf.Sign (velocity.x);

			if (collisions.faceDir == -1 && facingRight)
			{
				Flip (); //only for flipping graphics
			}
			if (collisions.faceDir == 1 && !facingRight)
			{
				Flip (); //only for flipping graphics
			}
		}
		if (velocity.y <0)							
		{
			DescendSlope(ref velocity);
		}

		if (playerOnLeftWall)
		{
			HorizontalCollisions( ref velocity, true, false, false, false);	
		}
		else if (playerOnRightWall)
		{
			HorizontalCollisions( ref velocity, false, true, false, false);	
		}
		else if (playerOnground)
		{
			HorizontalCollisions( ref velocity, false, false, true, false);	
		}
		else if (playerOnCieling)
		{
			HorizontalCollisions( ref velocity, false, false, false, true);	
		}

		if (velocity.y != 0)
		{
			if (playerOnLeftWall)
			{
				VerticalCollisions (ref velocity, true, false, false, false);	
			}
			else if (playerOnRightWall)
			{
				VerticalCollisions (ref velocity, false, true, false, false);
			}
			else if (playerOnground)
			{
				VerticalCollisions (ref velocity, false, false, true, false);
			}
			else if (playerOnCieling)
			{
				VerticalCollisions (ref velocity, false, false, false, true);
			}
		}

		transform.Translate (velocity);

		if (standingOnPlatform)						//Tracing collisions below player when on platform
		{
			collisions.below = true;
		}
	}

	//HORIZONTAL COLLISIONS
	void HorizontalCollisions(ref Vector3 velocity, bool onLeftWall, bool onRightWall, bool onFloor, bool onCieling) 				
	{
		float directionX = collisions.faceDir;
		float rayLength = Mathf.Abs (velocity.x) + skinWidth;

		if (Mathf.Abs(velocity.x) < skinWidth)  			 //So we can jump even without horizontal input
		{
			rayLength = 2*skinWidth;						//Setting raylenght to be skinwidth + 1 skinwidth outsite of player for wall detection without horizontal input
		}

		for (int i = 0; i < horizontalRayCount; i ++)
		{
//			Vector2 rayOrigin;			//Raycast origin either bottom left or bottom right

			if (onLeftWall)
			{
				horizontalRayOrigin = (directionX == -1)?raycastOrigins.topLeft:raycastOrigins.bottomLeft;	
				horizontalRayOrigin += Vector2.right * (verticalRaySpacing * i);
			}
			else if (onRightWall)
			{
				horizontalRayOrigin = (directionX == -1)?raycastOrigins.bottomRight:raycastOrigins.topRight;	
				horizontalRayOrigin += Vector2.left * (verticalRaySpacing * i);
			}
			else if (onCieling)
			{
				horizontalRayOrigin = (directionX == -1)?raycastOrigins.topRight:raycastOrigins.topLeft;	
				horizontalRayOrigin += Vector2.down * (horizontalRaySpacing * i);
			}
			else if (onFloor)
			{
				horizontalRayOrigin = (directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight;	
				horizontalRayOrigin += Vector2.up * (horizontalRaySpacing * i);		
			}													
			RaycastHit2D hit = Physics2D.Raycast(horizontalRayOrigin, this.transform.right * directionX, rayLength, collisionMask);		//Raycast hit - from origin in directionX with rayLength and only on collision layer

			Debug.DrawRay(horizontalRayOrigin, this.transform.right * directionX * rayLength * 5, Color.green); 								//Drawing red rays

			if (hit)
			{
				if (hit.distance == 0)
				{
					continue;
				}

				float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);

//				if (i == 0 && slopeAngle <= maxClimbAngle)
//				{
//					if (collisions.descendingSlope)							// Makes it so we can go from descending to ascending slope. 
//					{
//						collisions.descendingSlope = false;
//						velocity = collisions.velocityOld; 
//					}
//					float distanceToSlopeStart = 0;
//					if (slopeAngle != collisions.slopeAngleOld)
//					{
//						distanceToSlopeStart = hit.distance - skinWidth;
//						velocity.x -= distanceToSlopeStart * directionX;
//					}
//					ClimbSlope(ref velocity, slopeAngle);
//					velocity.x += distanceToSlopeStart * directionX;
//				}
				if (!collisions.climbingSlope || slopeAngle > maxClimbAngle)
				{
						velocity.x = (hit.distance - skinWidth) * directionX;
						rayLength = hit.distance;

						//					if (collisions.climbingSlope)
						//					{
						//						velocity.y = Mathf.Tan (collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
						//					}
						collisions.left = directionX == -1;
						collisions.right = directionX == 1;
				}
				if (hit.collider.gameObject.CompareTag("killTag"))
				{
					PlayerManager.instance.KillPlayer();
				}

			}
		}
	}

	//VERTICAL COLLISIONS
	public void VerticalCollisions(ref Vector3 velocity, bool onLeftWall, bool onRightWall, bool onFloor, bool onCieling) 
	{
		float directionY = Mathf.Sign (velocity.y);
		float rayLength = Mathf.Abs (velocity.y) + skinWidth;

		for (int i = 0; i < verticalRayCount; i ++)
		{
//			Vector2 rayOrigin;

			if (onLeftWall)
			{
				verticalRayOrigin = (directionY == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight;
				verticalRayOrigin += Vector2.up * (horizontalRaySpacing * i + velocity.x);	
			}
			else if (onRightWall)
			{
				verticalRayOrigin = (directionY == -1)?raycastOrigins.bottomRight:raycastOrigins.bottomLeft;
				verticalRayOrigin += Vector2.up * (horizontalRaySpacing * i + velocity.x);	
			}
			else if (onCieling)
			{
				verticalRayOrigin = (directionY == -1)?raycastOrigins.topLeft:raycastOrigins.bottomLeft;
				verticalRayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);	
			}	
			else if (onFloor)
			{
				verticalRayOrigin = (directionY == -1)?raycastOrigins.bottomLeft:raycastOrigins.topLeft;
				verticalRayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);	
			}													

			RaycastHit2D hit = Physics2D.Raycast(verticalRayOrigin, this.transform.up * directionY, rayLength, collisionMask);

			Debug.DrawRay(verticalRayOrigin, this.transform.up * directionY * rayLength *5, Color.red);

			if (hit)
			{
				if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
				{
					PlayerManager.instance.KillPlayer();
				}
				velocity.y = (hit.distance - skinWidth) * directionY;
				rayLength = hit.distance;

				if ( collisions.climbingSlope)
				{
					velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
				}

				collisions.below = directionY == -1;
				collisions.above = directionY == 1;

				if (hit.collider.gameObject.CompareTag("killTag"))
				{
					PlayerManager.instance.KillPlayer();
				}
			}
		}
		if (collisions.climbingSlope)
		{
			float directionX = Mathf.Sign (velocity.x);
			rayLength = Mathf.Abs(velocity.x) + skinWidth;
			Vector2 rayOrigin = ((directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight) + Vector2.up * velocity.y;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask); 
			if (hit)
			{
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
				if (slopeAngle != collisions.slopeAngle)
				{
					velocity.x = (hit.distance - skinWidth) * directionX; 
					collisions.slopeAngle = slopeAngle;
				}
			}
		}
	}

	//CLIMBING SLOPES
	void ClimbSlope (ref Vector3 velocity, float slopeAngle)
	{
		float moveDistance = Mathf.Abs (velocity.x);
		float climbVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;

		if (velocity.y <= climbVelocityY) 
		{
			velocity.y = climbVelocityY;
			velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
			collisions.below = true;
			collisions.climbingSlope = true;
			collisions.slopeAngle = slopeAngle;
		}

	}

	//DESCENDING SLOPES
	void DescendSlope(ref Vector3 velocity)
	{
		float directionX = Mathf.Sign (velocity.x);
		Vector2 rayOrigin = (directionX == -1)?raycastOrigins.bottomRight : raycastOrigins.bottomLeft;  //If moving left cast ray from bottomrightcorner otherwise bottomleft.
		RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

		if (hit)
		{
			float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
			if (slopeAngle != 0 && slopeAngle <= maxDescendAngle) 
			{
				if (Mathf.Sign(hit.normal.x) == directionX)
				{
					if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x)) //If we are close enough for the slope to come into effect.
					{
						float moveDistance = Mathf.Abs(velocity.x);
						float descendVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;
						velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
						velocity.y -= descendVelocityY;

						collisions.slopeAngle = slopeAngle;
						collisions.descendingSlope = true;
						collisions.below = true; 
					}
				}
			}
		}
	}

	void ResetFallingThroughPlatform()
	{
		collisions.fallingThroughPlatform = false;
	}

	// FLIPS PLAYER
	private void Flip()
	{
		// Switch the way the player is labelled as facing.
		facingRight = !facingRight;
				
		// Multiply the player's x local scale by -1.
		Vector3 theScale = graphicsTransform.localScale;
		theScale.x *= -1;
		graphicsTransform.localScale = theScale;
		//Debug.Log(theScale);
	}


	//STRUCT WITH COLLISION INFOS
	public struct Collisioninfo 
	{
		public bool above, below;						//Collision on above or below?
		public bool left, right;						//Collision on right or left?

		public bool descendingSlope;					//Is player descending slope
		public bool climbingSlope;						//Is player climbing slope
		public float slopeAngle, slopeAngleOld;			//Slope angles and slope angle on previou slope
		public Vector3 velocityOld;
		public int faceDir;								//Player face direction
		public bool fallingThroughPlatform;

		public void Reset() 							//Function for resetting collisiondetection
		{
			above = below = false;
			left = right = false; 
			climbingSlope = false;
			descendingSlope = false; 

			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "killTag")
		{
			PlayerManager.instance.KillPlayer();
		}
	}

	public void EmitParticle(ParticleSystem particle)
	{
		if (!particle.isPlaying)
		{
			particle.Play();
		}
	}
	public void StopEmitParticle(ParticleSystem particle)
	{
		particle.Stop();
	}

}