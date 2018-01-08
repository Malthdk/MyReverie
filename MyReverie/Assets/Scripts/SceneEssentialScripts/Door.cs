using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {

	//Publics
	public bool midPoint;
	public bool endPoint;
	public string nextLevelName;
	private SpriteRenderer openSprite, lockedSprite;

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
		openSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
		lockedSprite = transform.GetChild(1).GetComponent<SpriteRenderer>();
		myCollider = GetComponent<BoxCollider2D>();
		isKey = (GameObject.FindWithTag("key") == null)?null:GameObject.FindWithTag("key").gameObject;

		if (isKey != null) //&& isKey.gameObject != this.gameObject
		{
			needKey = true; //midPoint = true;
			//sRendererMid.enabled = true;
			//sRendererEnd.enabled = true; //sRendererEnd.enabled = false;
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
			//sRendererEnd.enabled = true;
			//open = true;
			needKey = false;
		}
	}

	void Update () 
	{
		if (!needKey) //open
		{
			openSprite.enabled = true;
			lockedSprite.enabled = false;
		}
		else if (needKey) //closed
		{
			lockedSprite.enabled = true;
			openSprite.enabled = false;
		}
		if (needKey && Key.hasKey) {
			openSprite.enabled = true;
			lockedSprite.enabled = false;
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
