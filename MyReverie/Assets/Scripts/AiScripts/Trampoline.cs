using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trampoline : MonoBehaviour {

	//Publics
	public float firstBounce = 14f;
	public float secondBounce = 24f;

	//Privates
	private GameObject player;
	private Player playerScript;
	private Controller2D controllerScript;
	private AiHandler aiHandler;
	private static bool jumpedOnce;

	void Start () 
	{
		player = GameObject.Find("Player");
		playerScript = player.GetComponent<Player>();
		controllerScript = player.GetComponent<Controller2D>();
		aiHandler = transform.parent.GetComponent<AiHandler>();
	}

	void Update () 
	{
		//Reset second jump when player lands on ground - is done here to avoid spagheticode
		if (controllerScript.collisions.below)
		{
			jumpedOnce = false;
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "player")
		{
			if(playerScript.velocity.y < 0f && aiHandler.behaviour == AiBehaviour.Patrol && !jumpedOnce)
			{
				playerScript.velocity.y = firstBounce;
				aiHandler.behaviour = AiBehaviour.Incapacitated;
				jumpedOnce = true;

			}
			else if (playerScript.velocity.y < 0f && aiHandler.behaviour == AiBehaviour.Patrol && jumpedOnce)
			{
				playerScript.velocity.y = secondBounce;
				aiHandler.behaviour = AiBehaviour.Incapacitated;
				//jumpedOnce = false;
			}
		}
	}
}
