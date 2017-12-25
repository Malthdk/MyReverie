using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RaycastController {

	public LayerMask passengerMask;

	public Vector3[] localWaypoints;
	Vector3[] globalWaypoints;

	public bool stopStart;
	public bool elevator;
	public bool onAndOff;
	public float speed;
	public bool cyclic;
	public float waitTime;
	[Range(0,2)]
	public float easeAmount;

	int fromWaypointIndex;
	float percentBetweenWaypoints;
	float nextMoveTime;
	float directionX;
	float directionY;

	List<PassengerMovement> passengerMovement; //A list of our structs
	Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>(); 

	private IntoLine.Direction playerDirection;
	private IntoLine intoLine;

	public Platform platform;
	private bool pltCanChange;

	private Vector3 velocity;
	private bool hasReset;

	public enum Platform
	{
		Move,
		Stop
	}

	public override void Awake () 
	{
		base.Awake();
		intoLine = GameObject.Find("Player").GetComponent<IntoLine>();
			
		globalWaypoints = new Vector3[localWaypoints.Length];
		for (int i = 0; i < localWaypoints.Length; i++)
		{
			globalWaypoints[i] = localWaypoints[i] + transform.position;
		}
	}

	void Update () 
	{
		playerDirection = GameObject.Find("Player").GetComponent<IntoLine>().direction;

		if (!IntoLine.instance.transforming)
		{
			pltCanChange = true;
		}

		switch (platform)
		{
		case Platform.Move:
			UpdateRaycastOrigins();
			CalculateRaySpacing();

			velocity = CalculatePlatformMovement();
			CalculatePassengerMovement(velocity);
			MovePassengers(true);
			transform.Translate(velocity);
			MovePassengers(false);

			if (IntoLine.instance.transforming && pltCanChange)
			{
				StartCoroutine(StartStop(Platform.Move));
			}	
			break;

		case Platform.Stop:
			if (IntoLine.instance.transforming && pltCanChange && !elevator)
			{
				StartCoroutine(StartStop(Platform.Stop));
			}	
			break;
		}

		if (elevator == false && !onAndOff)
		{
			platform = Platform.Move;
		}
	}

	float Ease(float x) //Mathimatical formular for easing.
	{
		float a = easeAmount + 1;
		return Mathf.Pow(x,a)/ (Mathf.Pow(x,a) + Mathf.Pow(1-x,a));
	}

	Vector3 CalculatePlatformMovement()
	{
		if (Time.time < nextMoveTime)
		{
			return Vector3.zero;
		}

		fromWaypointIndex %= globalWaypoints.Length; //% globalWaypoints.Lengt resets to zero everytime it reaches globalwaypoints.length
		int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
		float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
		percentBetweenWaypoints += Time.deltaTime * speed/distanceBetweenWaypoints;
		percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints); //if not clamped might get strange results from ease function
		float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints); //eased percentage

		Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

		if (hasReset)
		{
			toWaypointIndex = 0;
			distanceBetweenWaypoints = 0f;
			easedPercentBetweenWaypoints = 0f;
			percentBetweenWaypoints = 0f;
			fromWaypointIndex = 0;
			nextMoveTime = 0f;
			newPos = globalWaypoints[0];
			hasReset = false;
		}

		if (percentBetweenWaypoints >= 1) //reached next waypoint
		{
			percentBetweenWaypoints = 0;
			fromWaypointIndex ++; //move to next set of waypoints

			if (!cyclic)
			{
				if ( fromWaypointIndex >= globalWaypoints.Length -1) //reached end of array
				{
					fromWaypointIndex = 0;
					System.Array.Reverse(globalWaypoints); //reverse array to move backwards
				}	
			}
			nextMoveTime = Time.time + waitTime;
		}
		return newPos - transform.position;


	}

	void MovePassengers(bool beforeMovePlatform)
	{
		foreach(PassengerMovement passenger in passengerMovement)
		{
			if (!passengerDictionary.ContainsKey(passenger.transform)) //if theres no Controller2D add it. (This reduces GetComponent calls.)
			{
				passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller2D>());
			}

			if (passenger.moveBeforePlatform == beforeMovePlatform)
			{
				passengerDictionary[passenger.transform].Move(passenger.velocity, passenger.standingOnPlatform);
			}
		}
	}


	void CalculatePassengerMovement( Vector3 velocity)
	{
		HashSet<Transform> movedPassengers = new HashSet<Transform>(); //makes sure that the player is not moved by each ray he is hit by 
		passengerMovement = new List<PassengerMovement>();

		directionX = Mathf.Sign(velocity.x);
		directionY = Mathf.Sign(velocity.y);

		//Vertically moving platform
		if (velocity.y != 0)
		{
			float rayLength = Mathf.Abs(velocity.y) + skinWidth;

			for (int i = 0; i < verticalRayCount; i++)
			{
				Vector2 rayOrigin = (directionY == -1)?raycastOrigins.bottomLeft:raycastOrigins.topLeft;
				rayOrigin += Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

				Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

				if (hit)
				{
					if (!movedPassengers.Contains(hit.transform))
					{
						if (playerDirection == IntoLine.Direction.Floor)
						{
							movedPassengers.Add(hit.transform);
							float pushX = (directionY == 1)?velocity.x:0;
							float pushY = velocity.y - (hit.distance - skinWidth) * directionY;

							passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true)); //Adding to our list of structs, giving all required arguments in the struct.	
						}
						if (playerDirection == IntoLine.Direction.Cieling)
						{
							movedPassengers.Add(hit.transform);
							float pushX = (directionY == -1)?velocity.x:0;
							float pushY = -velocity.y - (hit.distance - skinWidth) * directionY;

							passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == -1, true));
						}
						if (playerDirection == IntoLine.Direction.Leftwall)
						{
							movedPassengers.Add(hit.transform);
							float pushX = -velocity.y - (hit.distance - skinWidth) * -directionY;
							float pushY = -skinWidth;	//small downward force so that the player knows he is grounded.

							passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
						}
						if (playerDirection == IntoLine.Direction.Rightwall)
						{
							movedPassengers.Add(hit.transform);
							float pushX = velocity.y - (hit.distance - skinWidth) * directionY;
							float pushY = -skinWidth;	//small downward force so that the player knows he is grounded.

							passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
						}
					}
				}
			}
		}

		//Horizontally moving platform
		if (velocity.x != 0)
		{
			float rayLength = Mathf.Abs(velocity.x) + skinWidth;

			for (int i = 0; i < horizontalRayCount; i++)
			{
				Vector2 rayOrigin = (directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight;
				rayOrigin += Vector2.up * (horizontalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

				Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

				if (hit)
				{
					if (!movedPassengers.Contains(hit.transform))
					{
						if (playerDirection == IntoLine.Direction.Floor)
						{
							movedPassengers.Add(hit.transform);
							float pushX = velocity.x - (hit.distance - skinWidth) * directionX;
							float pushY = -skinWidth;	//small downward force so that the player knows he is grounded.

							passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
						}
						if (playerDirection == IntoLine.Direction.Cieling)
						{
							movedPassengers.Add(hit.transform);
							float pushX = -velocity.x - (hit.distance - skinWidth) * -directionX;
							float pushY = -skinWidth;	//small downward force so that the player knows he is grounded.

							passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
						}

						if (playerDirection == IntoLine.Direction.Leftwall)
						{
							movedPassengers.Add(hit.transform);
							float pushX = 0f; //Kan omskrives?
							float pushY = (directionX == 1)?velocity.x:0;

							passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionX == 1, true));
						}

						if (playerDirection == IntoLine.Direction.Rightwall)
						{
							movedPassengers.Add(hit.transform);
							float pushX = 0f; //Kan omskrives?
							float pushY = (directionX == -1)?-velocity.x:0;

							passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionX == -1, true));
						}		
					}
				}
			}
		}
		//Passenger on left of a vertically or left moving platform
		if (directionX == -1 || velocity.x == 0 && velocity.y != 0)
		{
			float rayLength = skinWidth * 2;

			for (int i = 0; i < horizontalRayCount; i++)
			{
				Vector2 rayOrigin = raycastOrigins.topRight + Vector2.down * (horizontalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right, rayLength, passengerMask);

				Debug.DrawRay(rayOrigin, Vector2.right * rayLength * 2, Color.red);
				if (hit)
				{
					//This is for when the player is standing on left of platform
					if (!movedPassengers.Contains(hit.transform) && playerDirection == IntoLine.Direction.Leftwall)
					{
						movedPassengers.Add(hit.transform);
						float pushX = -velocity.y;
						float pushY = velocity.x;

						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
					}
					//This is for when the player is jumping onto the platform from Cieling direction state.
					else if (!movedPassengers.Contains(hit.transform) && playerDirection == IntoLine.Direction.Cieling)
					{
						movedPassengers.Add(hit.transform);
						float pushX = velocity.x; // we have to flip y and x  and reverse (with -)
						float pushY = -velocity.y;

						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
					}
					//This is for when the player is jumping onto the platform from Floor direction state.
					else if (!movedPassengers.Contains(hit.transform) && playerDirection == IntoLine.Direction.Floor)
					{
						movedPassengers.Add(hit.transform);
						float pushX = -velocity.x; // we have to flip y and x 
						float pushY = velocity.y;

						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
					}
				}
			}
		}

		//Passenger on right of a vertically or right moving platform
		if (directionX == 1 || velocity.x == 0 && velocity.y != 0)
		{
			float rayLength = skinWidth * 2;

			for (int i = 0; i < horizontalRayCount; i++)
			{
				Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.down * (horizontalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.left, rayLength, passengerMask);

				Debug.DrawRay(rayOrigin, Vector2.left * rayLength * 2, Color.green);
				if (hit)
				{
					//This is for when the player is standing on right of platform
					if (!movedPassengers.Contains(hit.transform) && playerDirection == IntoLine.Direction.Rightwall)
					{
						movedPassengers.Add(hit.transform);
						float pushX = velocity.y;
						float pushY = -velocity.x;

						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
					}
					//This is for when the player is jumping onto the platform from Cieling direction state.
					else if (!movedPassengers.Contains(hit.transform) && playerDirection == IntoLine.Direction.Cieling)
					{
						movedPassengers.Add(hit.transform);
						float pushX = velocity.x; // we have to flip y and x
						float pushY = -velocity.y;

						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
					}
					//This is for when the player is jumping onto the platform from Floor direction state.
					else if (!movedPassengers.Contains(hit.transform) && playerDirection == IntoLine.Direction.Floor)
					{
						movedPassengers.Add(hit.transform);
						float pushX = -velocity.x; // we have to flip y and x  and reverse (with -)
						float pushY = velocity.y;

						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
					}
				}
			}
		}

		//Passenger on top of a horizontally or downward moving platform
		if (directionY == -1 || velocity.y == 0 && velocity.x != 0)
		{
			float rayLength = skinWidth * 2;

			for (int i = 0; i < verticalRayCount; i++)
			{
				Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

				Debug.DrawRay(rayOrigin, Vector2.up * rayLength * 2, Color.red);

				if (hit)
				{
					//This is for when the player is standing on the platform
					if (!movedPassengers.Contains(hit.transform) && playerDirection == IntoLine.Direction.Floor)
					{
						movedPassengers.Add(hit.transform);
						float pushX = velocity.x;
						float pushY = velocity.y;

						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
					}
					//This is for when the player is jumping onto the platform from LeftWall direction state.
					else if (!movedPassengers.Contains(hit.transform)  && playerDirection == IntoLine.Direction.Leftwall)
					{
						movedPassengers.Add(hit.transform);
						float pushX = velocity.y; //we have to flip y and x 
						float pushY = velocity.x;

						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
					}
					//This is for when the player is jumping onto the platform from RightWall direction state.
					else if (!movedPassengers.Contains(hit.transform)  && playerDirection == IntoLine.Direction.Rightwall)
					{
						movedPassengers.Add(hit.transform);
						float pushX = -velocity.y; // we have to flip y and x  and reverse (with -)
						float pushY = -velocity.x;

						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
					}
				}
			}
		}
		//Passenger below of a horizontally or down moving platform
		if (directionY == 1 || velocity.y == 0 && velocity.x != 0)
		{
			float rayLength = skinWidth * 2;

			for (int i = 0; i < verticalRayCount; i++)
			{
				Vector2 rayOrigin = raycastOrigins.bottomLeft + Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, passengerMask);

				Debug.DrawRay(rayOrigin, Vector2.down * rayLength * 2, Color.red);

				if (hit)
				{
					//This is for when the player is standing below the platform
					if (!movedPassengers.Contains(hit.transform) && playerDirection == IntoLine.Direction.Cieling)
					{
						movedPassengers.Add(hit.transform);
						float pushX = -velocity.x;
						float pushY = -velocity.y;

						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
					}
					//This is for when the player is jumping onto the platform from LeftWall direction state.
					else if (!movedPassengers.Contains(hit.transform) && playerDirection == IntoLine.Direction.Leftwall)
					{
						movedPassengers.Add(hit.transform);
						float pushX = velocity.y;
						float pushY = velocity.x;

						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
					}
					//This is for when the player is jumping onto the platform from RightWall direction state.
					else if (!movedPassengers.Contains(hit.transform)  && playerDirection == IntoLine.Direction.Rightwall)
					{
						movedPassengers.Add(hit.transform);
						float pushX = -velocity.y; //we have to reverse y velocity
						float pushY = -velocity.x;

						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
					}
				}
			}
		}

	}

	//Creating a struct with all necessary information to move the player
	struct PassengerMovement
	{
		public Transform transform;
		public Vector3 velocity;
		public bool standingOnPlatform;
		public bool moveBeforePlatform;

		public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform)
		{
			transform = _transform;
			velocity = _velocity;
			standingOnPlatform = _standingOnPlatform;
			moveBeforePlatform = _moveBeforePlatform;
		}
	}

	void OnTriggerEnter2D(Collider2D other) 
	{
		if (other.name == "Player") 
		{	
			elevator = false;
		}
	}

	void OnDrawGizmos()
	{
		if (localWaypoints != null)
		{
			Gizmos.color = Color.red;
			float size = .3f;

			for (int i = 0; i < localWaypoints.Length; i++)
			{
				Vector3 globalWaypointPos = (Application.isPlaying)?globalWaypoints[i]:localWaypoints[i] + transform.position;
				Gizmos.DrawSphere(globalWaypointPos, size);
			}
		}
	}

	IEnumerator StartStop(Platform currentState)
	{
		pltCanChange = false;
		yield return new WaitForEndOfFrame();

		if (currentState == Platform.Move)
		{
			platform = Platform.Stop;
		}
		else if (currentState == Platform.Stop)
		{
			platform = Platform.Move;
		}
	}

	public void ResetPlatform()
	{
		gameObject.transform.position = globalWaypoints[0];
		velocity = new Vector3(0f, 0f, 0f);
		//if (isElevator)
		//{
		//	elevator = true;			
		//}
		//else if (isPlatformActivator)
		//{
		//	platformActivator = true;
		//}
		hasReset = true;
		platform = Platform.Stop;
	}


}
