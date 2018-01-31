using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiPatrolling : MonoBehaviour {

	//Publics
	public AiDirection aiDirection;
	public bool aroundEdge, flipAtStart, isPatrolling;
	public LayerMask enemyMask;
	public int speed = 1;
	public float raylength;
	public bool mimic;
	[HideInInspector]
	public bool isMoving, isMimic; //in order to differentiate neutralized mimics from other patrolling AIs;
	public bool rayOffsetToggle = true; // should the raycast be offset in x?
	public float offSetForRay = 1.2f;

	//Privates
	private bool isGrounded, isBlocked, shootRays, transforming, canTransform;
	public bool faceingLeft = true;
	float myWidth, myHeight;
	private Vector3 edgeOffset = new Vector3(-0.8f, -0.8f, 0f);
	private Vector3 wallOffset = new Vector3(0f, 0f, 0f);
	private Coroutine co;

	//For lineCasting
	private Vector2 rayOffset;
	private Vector2 lineCastPos;
	private Vector2 wallCheck;
	private Vector2 startPos;
	private Vector2 groundCheck;

	//Components
	Transform myTrans;
	BoxCollider2D myBoxCol;
	Animator animator;

	//For resetting
	private Vector3 startPosition;
	private Quaternion startRotation;
	public AiDirection startDirection;
	private AiGrumpy grumpyScript;
	private AiHandler aiHandler;
	[HideInInspector]
	public int startSpeed;

	public enum AiDirection
	{
		Floor,
		Cieling,
		Rightwall,
		Leftwall
	}


	void Start ()
	{
		startPosition = transform.position;
		startRotation = transform.rotation;
		startDirection = aiDirection;
		startSpeed = speed;

		isMoving = true;
		shootRays = true;
		canTransform = true; 
		isMimic = (mimic)?true:false;
		animator = this.transform.GetComponentInChildren<Animator>();
		myTrans = this.transform;
		myBoxCol = this.GetComponent<BoxCollider2D>();
		grumpyScript = (GetComponent<AiGrumpy>() == null)?null:GetComponent<AiGrumpy>();
		aiHandler = GetComponent<AiHandler>();

		InitiateAi();
	}

	void FixedUpdate ()
	{
		//Animations
		if (animator != null) {
			animator.SetBool("isMoving", isMoving);
			animator.SetBool("transforming", transforming);
		}

		if (isPatrolling)
		{
			Move();
			switch(aiDirection)
			{
			//When the AI is walking on the floor
			case AiDirection.Floor:
				//This is the collision detection and lineshooting
				startPos = Vector2.up;
				groundCheck = Vector2.down;
				wallCheck = myTrans.right.toVector2();

				ShootLines(startPos, groundCheck, wallCheck);

				//If the AI has reached an edge or is blocked by a wall
				if(!isGrounded || isBlocked)
				{
					//This is for the AI to go around an edge when blocked
					if (aroundEdge && isBlocked)
					{
						if(faceingLeft)
						{
							co = StartCoroutine(TransformAI(wallOffset, AiDirection.Leftwall, 0f, -90f));
						}
						else if (!faceingLeft)
						{
							co = StartCoroutine(TransformAI(wallOffset, AiDirection.Rightwall, 0f, -90f));
						}

					}
					//This is for the AI to go around an edge when egde
//					else if (aroundEdge && !isGrounded)
//					{
//						if(faceingLeft)
//						{
//							co = StartCoroutine(TransformAI(edgeOffset, AiDirection.Rightwall, 0f, 90f));
//						}
//						else if (!faceingLeft)
//						{
//							co = StartCoroutine(TransformAI(edgeOffset, AiDirection.Leftwall, 0f, 90f));
//						}
//					}
					//This flips the AI in the opposite direction - for back and forth patrolling.
					else
					{
						Flip(false);
					}
				}
				break;

			//When the AI is walking on the cieling
			case AiDirection.Cieling:
				
				startPos = Vector2.down;
				groundCheck = Vector2.up;
				wallCheck = myTrans.right.toVector2();	

				ShootLines(startPos, groundCheck, wallCheck);

				if(!isGrounded || isBlocked)
				{
					if (aroundEdge && isBlocked)
					{
						if(faceingLeft)
						{
							co = StartCoroutine(TransformAI(wallOffset, AiDirection.Rightwall, 0f, -90f));
						}
						else if (!faceingLeft)
						{
							co = StartCoroutine(TransformAI(wallOffset, AiDirection.Leftwall, 0f, -90f));
						}
					}
//					else if (aroundEdge && !isGrounded)
//					{
//						if(faceingLeft)
//						{
//							co = StartCoroutine(TransformAI(edgeOffset, AiDirection.Leftwall, 0f, 90f));
//						}
//						else if (!faceingLeft)
//						{
//							co = StartCoroutine(TransformAI(edgeOffset, AiDirection.Rightwall, 0f, 90f));
//						}
//					}
					else 
					{
						Flip(false);	
					}
				}
				break;
			
				//When the AI is walking on a left wall
			case AiDirection.Leftwall:
				
				startPos = Vector2.right;
				groundCheck = Vector2.left;
				wallCheck = myTrans.right.toVector2();

				ShootLines(startPos, groundCheck, wallCheck);

				if(!isGrounded || isBlocked)
				{
					if (aroundEdge && isBlocked)
					{
						if(faceingLeft)
						{
							co = StartCoroutine(TransformAI(wallOffset, AiDirection.Cieling, 0f, -90f));
						}
						else if (!faceingLeft)
						{
							co = StartCoroutine(TransformAI(wallOffset, AiDirection.Floor, 0f, -90f));
						}
					}
//					else if (aroundEdge && !isGrounded)
//					{
//						if(faceingLeft)
//						{
//							co = StartCoroutine(TransformAI(edgeOffset, AiDirection.Floor, 0f, 90f));
//						}
//						else if (!faceingLeft)
//						{
//							co = StartCoroutine(TransformAI(edgeOffset, AiDirection.Cieling, 0f, 90f));
//						}
//					}
					else
					{
						Flip(true);	
					}
				}

				break;
			//When the AI is walking on a right wall
			case AiDirection.Rightwall:
				startPos = Vector2.left;
				groundCheck = Vector2.right;
				wallCheck = myTrans.right.toVector2();

				ShootLines(startPos, groundCheck, wallCheck);

				if(!isGrounded || isBlocked)
				{
					if (aroundEdge && isBlocked)
					{
						if(faceingLeft)
						{
							co = StartCoroutine(TransformAI(wallOffset, AiDirection.Floor, 0f, -90f));
						}
						else if (!faceingLeft)
						{
							co = StartCoroutine(TransformAI(wallOffset, AiDirection.Cieling, 0f, -90f));
						}
					}
//					else if (aroundEdge && !isGrounded)
//					{
//						if(faceingLeft)
//						{
//							co = StartCoroutine(TransformAI(edgeOffset, AiDirection.Cieling, 0f, 90f));
//						}
//						else if (!faceingLeft)
//						{
//							co = StartCoroutine(TransformAI(edgeOffset, AiDirection.Floor, 0f, 90f));
//						}
//					}
					else
					{
						Flip(true);	
					}
				}
				break;
			}
		}
		if (mimic)
		{
			if (!NoEffectArea.noTransformEffect)
			{
				if (IntoLine.instance.transforming && canTransform)
				{
					if (aiDirection == AiDirection.Floor)
					{
						StartCoroutine(TransformAI(new Vector3(0f, -0.8f, 0f), AiDirection.Cieling, 180f, 180f));	
					}
					else if (aiDirection == AiDirection.Cieling)
					{
						StartCoroutine(TransformAI(new Vector3(0f, -0.8f, 0f), AiDirection.Floor, 180f, 180f));
					}
					else if (aiDirection == AiDirection.Rightwall)
					{
						StartCoroutine(TransformAI(new Vector3(0f, -0.8f, 0f), AiDirection.Leftwall, 180f, 180f));
					}
					else if (aiDirection == AiDirection.Leftwall)
					{
						StartCoroutine(TransformAI(new Vector3(0f, -0.8f, 0f), AiDirection.Rightwall, 180f, 180f));
					}
				}		
			}
		}

	}

	void InitiateAi()
	{
		if (aiDirection == AiDirection.Rightwall || aiDirection == AiDirection.Leftwall)
		{
			myWidth = myBoxCol.bounds.extents.y;
			myHeight = myBoxCol.bounds.extents.x;

			//This is so we can have the AI move in different direction from beginning
			if (flipAtStart)
			{
				Flip(true);
			}
		}
		else if (aiDirection == AiDirection.Floor || aiDirection == AiDirection.Cieling)
		{
			myWidth = myBoxCol.bounds.extents.x;
			myHeight = myBoxCol.bounds.extents.y;	
			if (flipAtStart)
			{
				Flip(false);
			}
		}
	}

	//This function shoots lines for collision detection
	void ShootLines(Vector2 startPos, Vector2 groundCheck, Vector2 wallCheck)
	{
		if (shootRays)
		{
			if (rayOffsetToggle) {
				if (aiDirection == AiDirection.Cieling || aiDirection == AiDirection.Floor) {
					rayOffset = new Vector2 (-offSetForRay * wallCheck.x, 0);
				} else if (aiDirection == AiDirection.Rightwall || aiDirection == AiDirection.Leftwall) {
					rayOffset = new Vector2 (0, -offSetForRay * wallCheck.y);
				}
			}

			lineCastPos = myTrans.position.toVector2() - myTrans.right.toVector2() * myWidth + startPos * myHeight/4 + rayOffset;	

			//Shooting towards ground
			isGrounded = Physics2D.Linecast(lineCastPos, lineCastPos + (groundCheck * raylength), enemyMask);
			Debug.DrawLine(lineCastPos, lineCastPos + (groundCheck * raylength), Color.blue);

			//Shooting in front of the AI
			isBlocked = Physics2D.Linecast(lineCastPos, lineCastPos - wallCheck * .05f, enemyMask);
			Debug.DrawLine(lineCastPos, lineCastPos - wallCheck * 0.05f, Color.red);
		}
	}

	//This moves the AI
	void Move()
	{
		if (isMoving)
		{
			myTrans.position -= myTrans.right * speed * Time.deltaTime;	
		}
	}

	//This makes the AI turn around a corner
	void AroundEdge(int childInt, Vector3 rotation)
	{
		Vector3 pivot = transform.GetChild(childInt).position;
		transform.Rotate(rotation, Space.Self);
		pivot -= transform.GetChild(childInt).position;
		transform.position += pivot;
	}

	//This flips the AI for patrolling back and forth
	void Flip(bool walls)
	{
		if (faceingLeft)
		{
			faceingLeft = false;
		}
		else if (!faceingLeft)
		{
			faceingLeft = true;
		}
		if (!walls)
		{
			Vector3 currRot = myTrans.eulerAngles;
			currRot.y += 180;
			myTrans.eulerAngles = currRot;
		}
		else if (walls)
		{
			Vector3 currRot = myTrans.eulerAngles;
			currRot.x += 180;
			myTrans.eulerAngles = currRot;

		}
	}

	public IEnumerator TransformAI(Vector3 transformation, AiDirection directionState,  float yRotation, float zRotation)
	{
		transforming = true;						//Needed for animation
		DisableAiFunctionality();
		aiDirection = directionState;				//Changing directionState before transforming in order to too shoot new correct collision rays.
		ParticleSystem particleEffect = gameObject.transform.GetChild(1).GetChild(0).GetComponent<ParticleSystem>();
		particleEffect.Play();
		animator.SetTrigger("goDown");				//animate go down
		yield return new WaitForSeconds(0.8f);

		particleEffect.Stop();
		transform.Translate(transformation); 		//translate splace
		transform.Rotate(0f, yRotation, zRotation);	//rotate 180 on y-axis
		yield return new WaitForSeconds(0.1f);

		animator.SetTrigger("goUp");				//animate go up
		particleEffect.Play();
		yield return new WaitForSeconds(0.8f);

		EnableAiFunctionality();
		particleEffect.Stop();
		transforming = false;

	}

	//Move this to AiHandler - it makes no sense to have it in AiPatrolling.
	public IEnumerator ResetAi()
	{
		if(co == null)
		{
			yield return new WaitForEndOfFrame();
		}
		else
		{
			StopCoroutine(co);	
		}
		DisableAiFunctionality();
		if (animator != null) {
			animator.ResetTrigger("goDown"); //stopping Animation
			animator.ResetTrigger("goUp");	//stopping Animation
		}
			
		yield return new WaitForEndOfFrame();
	
		transform.position = startPosition;
		transform.rotation = startRotation;
		aiDirection = startDirection;
		if (grumpyScript != null)
		{
			grumpyScript.grumpy = AiGrumpy.Grumpy.Relaxed;	
		}
		if (aiHandler.neutralised)
		{
			Debug.Log("Hostalize");
			aiHandler.neutralised = false;
			//aiHandler.behaviour = AiBehaviour.Patrol;
			aiHandler.HostalizseAI();
		}
		aiHandler.behaviour = AiBehaviour.Patrol;
		yield return new WaitForEndOfFrame();

		transforming = false;
		EnableAiFunctionality();
		InitiateAi();
		if (flipAtStart)
		{
			faceingLeft = false;
		}
	}

	void DisableAiFunctionality()
	{
		canTransform = false;						//Mimic cannot transform
		isMoving = false;							//Shouldnt be moving
		isGrounded = true;							//Grounded is set to true to keep Ai in place
		isBlocked =	false;							//isBlocked is set to false to keep Ai in place
		shootRays = false;							//shootRays is set to false to stop detecting collisions while transforming
	}
	void EnableAiFunctionality()
	{
		shootRays = true;							//start shooting rays
		isMoving = true;							//start moving again
		canTransform = true;						//Mimic can transform again
	}
}
