using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadZone : MonoBehaviour {

	//Publics
	public bool lockHorizontalCollisions;

	//Private
	public List<Controller2D> controllerArray;
	private SpriteRenderer sRenderer;

	void Awake()
	{
		sRenderer = GetComponent<SpriteRenderer>();
		sRenderer.enabled = false;
	}

	void Start () 
	{
		FindPlayers();
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if ((other.tag == "player" || other.tag == "lovedOne") && !lockHorizontalCollisions)
		{
			for (int i = 0; i < controllerArray.Count; i++)
			{
				if (controllerArray[i].collisions.below == true)
				{
					IntoLine.cannotTransform = true;	
				}
			}	
		}
		else if ((other.tag == "player" || other.tag == "lovedOne") && lockHorizontalCollisions)
		{
			IntoLine.cannotTransform = true;
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if (other.tag == "player" || other.tag == "lovedOne" )
		{
			IntoLine.cannotTransform = false;
		}
	}

	void FindPlayers()
	{
		foreach(GameObject pObject in FindObjectsWithTags.FindGameObjectsWithTags(new string[]{"player", "lovedOne"})) 
		{
			Controller2D controllers = pObject.GetComponent<Controller2D>();
			controllerArray.Add(controllers);
		}
	}

}
