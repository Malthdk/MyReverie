using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiTurncoat : MonoBehaviour {

	//Publics
	public Turncoat turncoat;
	public Color cRed;

	//Privates
	private SpriteRenderer sRenderer;
	private ParticleSystem.MainModule pSystem;
	private bool canChange;

	public enum Turncoat
	{
		Hate,
		Like
	}

	void Start () 
	{
		sRenderer = transform.GetComponentInChildren<SpriteRenderer>();
		pSystem = transform.GetChild(1).GetChild(0).GetComponent<ParticleSystem>().main;
	}

	void Update () 
	{
		if (!IntoLine.instance.transforming)
		{
			canChange = true;
		}

		//THIS AI DONT WANT TO SEE YOUR FEET OFF THE GROUND. HE WANTS YOU TO REMAIN GROUNDED OR HE IS JEALOUX OF YOU JUMPING.
//		if (!Controller2D.instance.collisions.below)
//		{
//			turncoat = Turncoat.Hate;
//		}
//		else
//		{
//			turncoat = Turncoat.Like;
//		}

		//THIS AI WANTS YOU TO REMAIN COMPLETELY STILL IN HIS PRESENCE. PERHAPS HE IS SO VAIN THAT HE HATES YOUR ATTEMPT AT BEING AS FAST AS HIM.
//		if (Mathf.Abs(Player.instance.velocity.x) > 2)
//		{
//			turncoat = Turncoat.Hate;
//		}
//		else
//		{
//			turncoat = Turncoat.Like;
//		}
		switch (turncoat)
		{
		case Turncoat.Hate:
			sRenderer.color = cRed;
			pSystem.startColor = cRed;

			transform.GetChild(0).gameObject.tag = "killTag";
			//THIS AI CHANGES HIS MOOD EVERYTIME YOU MAKE A MOVE
			if (!NoEffectArea.noTransformEffect)
			{
				if (IntoLine.instance.transforming && canChange)
				{
					StartCoroutine(Change(Turncoat.Hate));
				}	
			}
			break;

		case Turncoat.Like:
			sRenderer.color = Color.white;
			pSystem.startColor = Color.white;
			transform.GetChild(0).gameObject.tag = "Untagged";
			if (!NoEffectArea.noTransformEffect)
			{
				if (IntoLine.instance.transforming && canChange)
				{
					StartCoroutine(Change(Turncoat.Like));
				}	
			}
			break;
		}
	}

	//Used to swap between states when the player is transforming
	IEnumerator Change(Turncoat currentState)
	{
		canChange = false;
		yield return new WaitForEndOfFrame();

		if (currentState == Turncoat.Hate)
		{
			turncoat = Turncoat.Like;
		}
		else if (currentState == Turncoat.Like)
		{
			turncoat = Turncoat.Hate;
		}
	}
}
