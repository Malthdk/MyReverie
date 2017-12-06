using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiGrumpy : MonoBehaviour {

	//Publics
	public Grumpy grumpy;
	public Color deadly;
	public LayerMask collisionMask;
	public int stressedSpeed = 5;
	public int relaxSpeed = 2;
	public float stressedTime = 5f;
	public float spookedTime = 0.8f;
	public float visualRadius = 50f;

	//Privates
	public bool canSee, isSpooked, relaxing;
	private Transform player;
	private AiPatrolling patrollScript;
	private AiHandler aiHandler;
	private BoxCollider2D bCollider;
	private Bounds bounds;
	private Vector2 rayOrigin;
	private ParticleSystem.MainModule pSystem;
	private SpriteRenderer sRenderer;
	private GameObject parent;
	private Animator animator;
	private bool spookedAnimation;

	public enum Grumpy
	{
		Spooked,
		Stressed,
		Relaxed
	}

	void Start () 
	{
		canSee = true;
		player = GameObject.Find("Player").transform;
		patrollScript = gameObject.GetComponent<AiPatrolling>();
		aiHandler = gameObject.GetComponent<AiHandler>();
		bCollider = gameObject.GetComponent<BoxCollider2D>();
		sRenderer = transform.GetComponentInChildren<SpriteRenderer>();
		pSystem = transform.GetChild(1).GetChild(0).GetComponent<ParticleSystem>().main;
		animator = this.transform.GetComponentInChildren<Animator>();
	}

	void Update () 
	{
		animator.SetBool("aggro", spookedAnimation);

		PlaceRayOrigin();

		switch(grumpy)
		{
		case Grumpy.Spooked:
			transform.GetChild(0).gameObject.tag = "Untagged";
			sRenderer.color = Color.white;
			pSystem.startColor = Color.white;
			if (isSpooked)
			{
				isSpooked = false;
				StartCoroutine("SpookToStress");
			}

			break;

		case Grumpy.Stressed:
			sRenderer.color = deadly;
			pSystem.startColor = deadly;
			transform.GetChild(0).gameObject.tag = "killTag";
			patrollScript.speed = stressedSpeed;
			if (!TargetIsClose() && relaxing)
			{
				relaxing = false;
				StartCoroutine("StressToRelax");	
			}

			break;

		case Grumpy.Relaxed:
			sRenderer.color = Color.white;
			pSystem.startColor = Color.white;
			transform.GetChild(0).gameObject.tag = "Untagged";
			patrollScript.speed = relaxSpeed;

			if (TargetIsClose() && canSee)
			{
				canSee = false;
				ShootRay();
			}
			break;
		}
	}
		
	IEnumerator SpookToStress()
	{
		spookedAnimation = true;
		yield return new WaitForSeconds(spookedTime);
		spookedAnimation = false;
		relaxing = true;
		if (!aiHandler.neutralised)
		{
			aiHandler.behaviour = AiBehaviour.Patrol;
		}
		grumpy = Grumpy.Stressed;
	}

	IEnumerator StressToRelax()
	{
		yield return new WaitForSeconds(stressedTime);
		canSee = true;
		grumpy = Grumpy.Relaxed;
	}

	void ShootRay()
	{
		/// NEW STUFF
		Vector3 center = new Vector3(bounds.center.x, bounds.center.y, 0);
		Vector2 dir = player.position - center;

		RaycastHit2D hit = Physics2D.Raycast(rayOrigin, dir, Mathf.Infinity, collisionMask);
		Debug.DrawRay(rayOrigin, dir * 15f, Color.red);

		if (hit.collider != null) 
		{
			if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Player"))
			{
				isSpooked = true;
				grumpy = Grumpy.Spooked;
				if (!aiHandler.neutralised)
				{
					aiHandler.behaviour = AiBehaviour.Agro;	
				}
			}
			else
			{
				canSee = true;
			}
		}
	}

	bool TargetIsClose()
	{
		if ((Vector3.Distance(transform.position, player.position) > visualRadius))
		{
			return false;
		}
		else
		{
			return true;
		}

	}

	void PlaceRayOrigin()
	{
		bounds = bCollider.bounds;
		bounds.Expand (0.015f * -2);
		rayOrigin = new Vector2(bounds.center.x, bounds.center.y);
	}
}
