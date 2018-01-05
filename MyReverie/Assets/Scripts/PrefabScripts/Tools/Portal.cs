using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour {

	//Public statics
	public static bool playerOnPortal;
	public static bool leftArea;

	//Public
	public bool perpendicular;
	public Transform otherPortal;

	//Private
	private IntoLine intolineScript;
	private ParticleSystem ownSide1;
	private ParticleSystem ownSide2;
	private ParticleSystem otherSide1;
	private ParticleSystem otherSide2;

	private BoxCollider2D bCol;

	private float xPortalOffsetRight = 0.85f;
	private float xPortalOffsetLeft = -0.85f;
	private float yPortalOffsetUpDown = -0.85f;

	void Start () 
	{
		bCol = gameObject.GetComponent<BoxCollider2D>();

		ownSide1 = transform.GetChild(0).GetComponent<ParticleSystem>();
		ownSide2 = transform.GetChild(1).GetComponent<ParticleSystem>();
		otherSide1 = otherPortal.gameObject.transform.GetChild(0).GetComponent<ParticleSystem>();
		otherSide2 = otherPortal.gameObject.transform.GetChild(1).GetComponent<ParticleSystem>();
		intolineScript = IntoLine.instance.GetComponent<IntoLine>();
	}

	void Update () 
	{

		if (leftArea && !IntoLine.instance.transforming)
		{
			playerOnPortal = false;
		}
	}

	void OnTriggerStay2D(Collider2D other)
	{
		if (other.tag == "player" && Controller2D.instance.collisions.below)
		{
			leftArea = false;
			playerOnPortal = true;
		}

		if (other.tag == "player")
		{
			IntoLine.instance.otherPortal = this.otherPortal;
			if(perpendicular)
			{
				if (intolineScript.direction == IntoLine.Direction.Floor)
				{
					intolineScript.portalDirection = IntoLine.Direction.Leftwall;
					intolineScript.portalTransformation.x = xPortalOffsetRight;

					//ParticleHandler(true);
				}
				else if (intolineScript.direction == IntoLine.Direction.Cieling)
				{
					intolineScript.portalDirection = IntoLine.Direction.Rightwall;
					intolineScript.portalTransformation.x = xPortalOffsetRight;

					//ParticleHandler(false);
				}
				else if (intolineScript.direction == IntoLine.Direction.Leftwall)
				{
					intolineScript.portalDirection = IntoLine.Direction.Floor;
					intolineScript.portalTransformation.x = xPortalOffsetLeft;

					ParticleHandler(false);
				}
				else if (intolineScript.direction == IntoLine.Direction.Rightwall)
				{
					intolineScript.portalDirection = IntoLine.Direction.Cieling;
					intolineScript.portalTransformation.x = xPortalOffsetLeft;

					ParticleHandler(true);
				}
			}
			else if (!perpendicular)
			{
				if (intolineScript.direction == IntoLine.Direction.Floor)
				{
					intolineScript.portalDirection = IntoLine.Direction.Cieling;
					intolineScript.portalTransformation.y = yPortalOffsetUpDown;

					//ParticleHandler(true);
				}
				else if (intolineScript.direction == IntoLine.Direction.Cieling)
				{
					intolineScript.portalDirection = IntoLine.Direction.Floor;
					intolineScript.portalTransformation.y = yPortalOffsetUpDown;

					//ParticleHandler(false);
				}
				else if (intolineScript.direction == IntoLine.Direction.Leftwall)
				{
					intolineScript.portalDirection = IntoLine.Direction.Rightwall;
					intolineScript.portalTransformation.y = yPortalOffsetUpDown;

					ParticleHandler(true);
				}
				else if (intolineScript.direction == IntoLine.Direction.Rightwall)
				{
					intolineScript.portalDirection = IntoLine.Direction.Leftwall;
					intolineScript.portalTransformation.y = yPortalOffsetUpDown;

					ParticleHandler(false);
				}
			}

			Debug.Log("player top: " + Mathf.Abs(other.bounds.center.y));
			Debug.Log("own center: " + Mathf.Abs(bCol.bounds.max.y));

			Debug.Log("transform");

			//OVERVEJ AT GØR DET TIL EN LÅST VELOCITY SOM REGISTRERES VED FØRSTE COLLISION SÅ AT DEN IKKE BLIVER STØRRE ****************LAV NYT SCRIPT*****************
			////
			/// Vi skal bruge:
			/// CalculatePosition
			/// CalculateVelocity
			/// TransformPlayer
			/// 
			/// Test med alle tænkelige situationer
			/// Hvordan kan det maskeres?
			////
			if (intolineScript.direction == IntoLine.Direction.Floor)
			{
				if (Mathf.Abs(other.bounds.center.y) >= Mathf.Abs(bCol.bounds.max.y))
				{
					
					intolineScript.portalDirection = IntoLine.Direction.Cieling;
					intolineScript.portalTransformation.y = yPortalOffsetUpDown;

					other.transform.Translate(new Vector3(0f, yPortalOffsetUpDown, 0f));
					other.transform.position = new Vector3(otherPortal.position.x, otherPortal.position.y, otherPortal.position.z);
					IntoLine.instance.direction = IntoLine.Direction.Cieling;

					if (Player.instance.velocity.y < 0)
					{
						Player.instance.velocity.y = Mathf.Abs(Player.instance.velocity.y);
						Player.instance.velocity.x = -Player.instance.velocity.x;//negative x to continue movement
					}
				}

			}
			else if (intolineScript.direction == IntoLine.Direction.Cieling)
			{
				if (Mathf.Abs(other.bounds.center.y) <= Mathf.Abs(bCol.bounds.max.y))
				{
					intolineScript.portalDirection = IntoLine.Direction.Floor;
					intolineScript.portalTransformation.y = yPortalOffsetUpDown;

					other.transform.Translate(new Vector3(0f, yPortalOffsetUpDown, 0f));
					other.transform.position = new Vector3(otherPortal.position.x, otherPortal.position.y, otherPortal.position.z);
					IntoLine.instance.direction = IntoLine.Direction.Floor;

					if (Player.instance.velocity.y < 0)
					{
						Player.instance.velocity.y = Mathf.Abs(Player.instance.velocity.y);
						Player.instance.velocity.x = -Player.instance.velocity.x; //negative x to continue movement
					}
				}
			}
		}

	}
		
	void ParticleHandler(bool side1)
	{
		if (side1)
		{
			if (!ownSide1.isEmitting)
			{
				ownSide1.Play();	
			}
			ownSide2.Stop();
			ownSide2.Clear();

			if (!otherSide2.isEmitting)
			{
				otherSide2.Play();	
			}
			otherSide1.Stop();
			otherSide1.Clear();
		}	
		else
		{
			if (!ownSide2.isEmitting)
			{
				ownSide2.Play();	
			}
			ownSide1.Stop();
			ownSide1.Clear();

			if (!otherSide1.isEmitting)
			{
				otherSide1.Play();	
			}
			otherSide2.Stop();
			otherSide2.Clear();
		}
	}

	void ClearAllParticles()
	{
		ownSide1.Stop();
		ownSide1.Clear();

		ownSide2.Stop();
		ownSide2.Clear();

		otherSide1.Stop();
		otherSide1.Clear();

		otherSide2.Stop();
		otherSide2.Clear();
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if (other.tag == "player")
		{
			leftArea = true;
			ClearAllParticles();
		}
	}
}
