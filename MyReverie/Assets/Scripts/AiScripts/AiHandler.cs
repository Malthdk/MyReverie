using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AiBehaviour
{
	Patrol,
	Agro,
	Chase,
	Neutralised
}

public class AiHandler : MonoBehaviour {

	//Publics
	public AiBehaviour behaviour;
	[HideInInspector]
	public SpriteRenderer sRenderer; //This should be referenced by all children instead of making their own call individually
	public GameObject graphics;

	//Privates
	public bool neutralised;
	public AiPatrolling patrollingScript;
	public AiGrumpy grumpyScript;
	public AiTurncoat turncoatScript;
	private Color startColor;

	void Awake () 
	{
		neutralised = false;
		graphics = transform.GetChild(0).gameObject;
		sRenderer = graphics.GetComponent<SpriteRenderer>();

		if (gameObject.GetComponent<AiPatrolling>() == null) 
		{
		}
		else 
		{
			patrollingScript = gameObject.GetComponent<AiPatrolling>();
		}

		if (gameObject.GetComponent<AiTurncoat>() == null) 
		{
		}
		else 
		{
			turncoatScript = gameObject.GetComponent<AiTurncoat>();
		}

		if (gameObject.GetComponent<AiGrumpy>() == null) 
		{
		}
		else 
		{
			grumpyScript = gameObject.GetComponent<AiGrumpy>();
		}

	}

	void Start()
	{
		startColor = sRenderer.color;	
	}
	

	void Update () 
	{

		switch(behaviour)
		{
		case AiBehaviour.Patrol:
			patrollingScript.isPatrolling = true;
			break;

		case AiBehaviour.Agro:
			patrollingScript.isPatrolling = false;
			break;

		case AiBehaviour.Chase:
			patrollingScript.isPatrolling = false;
			break;

		case AiBehaviour.Neutralised:
			if (!neutralised)
			{
				NeutraliseAI();	
				neutralised = true;
			}
			break;
		}
	}

	void NeutraliseAI()
	{
		patrollingScript.speed = 2;
		sRenderer.color = Color.white;
		graphics.tag = "Untagged";

		if (patrollingScript.mimic == true)
		{
			patrollingScript.mimic = false;
		}

		if (grumpyScript != null)
		{
			grumpyScript.enabled = false;
		}
			
		if (turncoatScript != null)
		{
			turncoatScript.enabled = false;
		}
	}

	public void HostalizseAI()
	{
		if (behaviour != AiBehaviour.Patrol)
		{
			Debug.Log("changing to patrol");
			behaviour = AiBehaviour.Patrol;	
		}

		patrollingScript.speed = patrollingScript.startSpeed;
		sRenderer.color = startColor;

		if (patrollingScript.isMimic == true)
		{
			patrollingScript.mimic = true;
			graphics.tag = "killTag";
		}

		if (grumpyScript != null)
		{
			grumpyScript.enabled = true;
			grumpyScript.canSee = true;
			grumpyScript.relaxing = false;
			grumpyScript.isSpooked = false;
		}

		if (turncoatScript != null)
		{
			turncoatScript.enabled = true;
		}

	}
}
