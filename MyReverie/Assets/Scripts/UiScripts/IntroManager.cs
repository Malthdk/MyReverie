using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour {

	//Publics
	public float waitForChapter;
	public float waitForTitle;
	public float waitForText;
	public Text actText, quoteText, personText;
	public string nextLevel;

	void Awake () 
	{
		
	}

	void Start()
	{
		StartCoroutine("InitiateScene");
	}

	void Update () 
	{
		
	}

	IEnumerator InitiateScene()
	{

		yield return new WaitForSeconds(waitForChapter);

		while (actText.color.a < 1f)
		{
			if (actText != null)
			{
				Fade(actText);	
			}
			yield return new WaitForEndOfFrame();
		}

		yield return new WaitForSeconds(waitForTitle);

		while (quoteText.color.a < 1f)
		{
			if (quoteText != null)
			{
				Fade(quoteText);	
			}
			yield return new WaitForEndOfFrame();
		}

		yield return new WaitForSeconds(waitForText);

		while (personText.color.a <= 1f)
		{
			if (personText != null)
			{
				Fade(personText);	
			}
			yield return new WaitForEndOfFrame();
		}
	}

	void Fade(Text text)
	{
		Color color;
		color = text.color;
		color.a += 0.01f;
		text.color = color;
	}
}
