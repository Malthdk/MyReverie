using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevealButton : MonoBehaviour {

	public List <GameObject> revealObjects;
	public List <SpriteRenderer> fadeObjects;

//	public bool isEnabled;
	private SpriteRenderer spriteRend;
	private Color color;

	void Start () 
	{
		spriteRend = transform.GetComponentInChildren<SpriteRenderer>();
		color = Color.black;
		//FLipState(revealObjects);
	}
	

	void Update () 
	{
		
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.name == "Player")
		{
			spriteRend.enabled = false;
			StartCoroutine("HideOrShow");
		}
	}


	IEnumerator HideOrShow()
	{
		FLipState(revealObjects);

		yield return new WaitForSeconds(0.5f);

		while (color.a > 0)
		{
			Fade(fadeObjects);
			yield return new WaitForEndOfFrame();
		}
	}

	void Fade(List<SpriteRenderer> theList)
	{
		for (int i = 0; i < theList.Count; i++)
		{
			Debug.Log("Fading Color");
			color = theList[i].material.color;
			color.a -= 0.05f;
			theList[i].material.color = color;
		}
	}

	void FLipState(List<GameObject> theList)
	{
		Debug.Log("Showing or hiding");
		for (int i = 0; i < theList.Count; i++)
		{
//			if (!isEnabled)					//this is for if we want it to be hidden again - could be an interesting mechanic
//			{
			theList[i].SetActive(true);
//				isEnabled = true;
//			}
//			else if (isEnabled)
//			{
//				theList[i].SetActive(false);
//				isEnabled = true;
//			}
		}
	}

	public IEnumerator ResetRevealButton()
	{
		spriteRend.enabled = true;
		ResetRevealObjects(revealObjects, fadeObjects);
		yield return new WaitForEndOfFrame();
	}

	void ResetRevealObjects(List<GameObject> revealList, List<SpriteRenderer> fadeList)
	{
		for (int i = 0; i < fadeList.Count; i++)
		{
			color = fadeList[i].material.color;
			color.a = 1f;
			fadeList[i].material.color = color;
		}

		for (int i = 0; i < revealList.Count; i++)
		{
			revealList[i].SetActive(false);
		}
	}
}
