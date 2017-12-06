using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {

	//Publics
	public bool midPoint;
	public bool endPoint;
	public string nextLevelName;
	public Color openColor, closedColor;

	//Privates
	private Door cDoor;
	public bool open, closed;
	private TextMesh myTextMesh;
	private ParticleSystem.MainModule mainSystem;
	private SpriteRenderer sRendererEnd;
	private SpriteRenderer sRendererMid;
	private BoxCollider2D myCollider;
	public ParticleSystem pSystem;

	void Start () 
	{
		sRendererEnd = transform.GetChild(0).GetComponent<SpriteRenderer>();
		sRendererMid = transform.GetChild(1).GetComponent<SpriteRenderer>();
		mainSystem = transform.GetChild(3).GetComponent<ParticleSystem>().main;
		myTextMesh = transform.GetComponentInChildren<TextMesh>();
		pSystem = transform.GetChild(4).GetComponent<ParticleSystem>();

		myCollider = GetComponent<BoxCollider2D>();
		cDoor = (GameObject.FindWithTag("cPoint") == null)?null:GameObject.FindWithTag("cPoint").GetComponent<Door>();

		if (cDoor != null && cDoor.gameObject != this.gameObject)
		{
			midPoint = true;
			sRendererMid.enabled = true;
			sRendererEnd.enabled = false;
			open = true;
		}
		else if (cDoor != null && cDoor.gameObject == this.gameObject)
		{
			endPoint = true;
			sRendererMid.enabled = false;
			sRendererEnd.enabled = true;
			closed = true;
		}
		else if (cDoor == null)
		{
			endPoint = true;
			sRendererMid.enabled = false;
			sRendererEnd.enabled = true;
			open = true;
		}
	}

	void Update () 
	{
		if (open)
		{
			//myTextMesh.color = openColor;
			mainSystem.startColor = openColor;
			sRendererEnd.color = openColor;
			sRendererMid.color = openColor;
		}
		else if (closed)
		{
			//myTextMesh.color = closedColor;
			mainSystem.startColor = closedColor;
			sRendererEnd.color = closedColor;
			sRendererMid.color = closedColor;
		}
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.name == "Player")
		{
			if (endPoint && open)
			{
				AkSoundEngine.PostEvent ("EndLevel", gameObject);
				CompletedLevel();
			}
			else if (midPoint && open)
			{
				myCollider.enabled = false;
				sRendererMid.enabled = false;
				cDoor.open = true;
				pSystem.Play();
			}
		}
	}
		
	void CompletedLevel() 
	{
		StartCoroutine(LevelManager.instance.NextLevel(nextLevelName));	
	}
}
