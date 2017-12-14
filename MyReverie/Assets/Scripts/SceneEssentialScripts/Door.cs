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
	private GameObject isKey;
	public bool open, closed;
	private TextMesh myTextMesh;
	private ParticleSystem.MainModule mainSystem;
	private SpriteRenderer sRendererEnd;
	//private SpriteRenderer sRendererMid;
	private BoxCollider2D myCollider;
	public ParticleSystem pSystem;
	private bool needKey;

	void Start () 
	{
		sRendererEnd = transform.GetChild(0).GetComponent<SpriteRenderer>();
		//sRendererMid = transform.GetChild(1).GetComponent<SpriteRenderer>();
		mainSystem = transform.GetChild(3).GetComponent<ParticleSystem>().main;
		myTextMesh = transform.GetComponentInChildren<TextMesh>();
		pSystem = transform.GetChild(4).GetComponent<ParticleSystem>();

		myCollider = GetComponent<BoxCollider2D>();
		isKey = (GameObject.FindWithTag("key") == null)?null:GameObject.FindWithTag("key").gameObject;

		if (isKey != null) //&& isKey.gameObject != this.gameObject
		{
			needKey = true; //midPoint = true;
			//sRendererMid.enabled = true;
			sRendererEnd.enabled = true; //sRendererEnd.enabled = false;
			//open = true;
		}
//		else if (isKey != null && isKey.gameObject == this.gameObject)
//		{
//			endPoint = true;
//			sRendererMid.enabled = false;
//			sRendererEnd.enabled = true;
//			closed = true;
//		}
		else if (isKey == null)
		{
			//endPoint = true;
			//sRendererMid.enabled = false;
			sRendererEnd.enabled = true;
			//open = true;
			needKey = false;
		}
	}

	void Update () 
	{
		if (!needKey) //open
		{
			//myTextMesh.color = openColor;
			mainSystem.startColor = openColor;
			sRendererEnd.color = openColor;
			//sRendererMid.color = openColor;
		}
		else if (needKey) //closed
		{
			//myTextMesh.color = closedColor;
			mainSystem.startColor = closedColor;
			sRendererEnd.color = closedColor;
			//sRendererMid.color = closedColor;
		}
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.name == "Player")
		{
			if (needKey && Key.hasKey) //before i had endPoint instead of needKey and open instead of Key.hasKey
			{
				AkSoundEngine.PostEvent ("EndLevel", gameObject);
				CompletedLevel();
			}
			else if (!needKey)
			{
				AkSoundEngine.PostEvent ("EndLevel", gameObject);
				CompletedLevel();
			}
//			else if (midPoint && open)
//			{
//				myCollider.enabled = false;
//				sRendererMid.enabled = false;
//				isKey.open = true;
//				pSystem.Play();
//			}
		}
	}
		
	void CompletedLevel() 
	{
		StartCoroutine(LevelManager.instance.LoadNextLevel());	
	}
}
