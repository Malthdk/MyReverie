using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartPower : MonoBehaviour {

	//Publics
	public List<AiHandler> aiHandlers;

	//Privates
	SpriteRenderer sRenderer;
	BoxCollider2D bCollider;
	ParticleSystem pickUpParticles;
	ParticleSystem followAiParticles;
	void Start () 
	{
		FillList();
		sRenderer = transform.GetComponentInChildren<SpriteRenderer>();
		bCollider = transform.GetComponent<BoxCollider2D>();
		pickUpParticles = gameObject.transform.GetChild(1).GetComponent<ParticleSystem>(); //Particle at collision
		followAiParticles = gameObject.transform.GetChild(2).GetComponent<ParticleSystem>(); //Particle at collision
	}
	

	void Update () 
	{
		
	}

	void FillList()
	{
		///THIS IS IF WE WANT TO FIND ALL AIs at START
//		foreach(GameObject aiObject in GameObject.FindGameObjectsWithTag("ai"))
//		{
//			AiHandler aiPatrol = aiObject.GetComponent<AiHandler>();
//			aiHandlers.Add(aiPatrol);
//		}
	}

	void NeutraliseAI(List<AiHandler> theList)
	{
		if (theList.Count == 0)
		{
			return;
		}
		else 
		{
			for (int i = 0; i < theList.Count; i++)
			{
				theList[i].behaviour = AiBehaviour.Neutralised;
			}	
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.name == "Player" || other.name == "LovedOne")	
		{
			LevelManager.instance.heartsCollected --;

			NeutraliseAI(aiHandlers); //Neutralises AIs
			sRenderer.enabled = false;
			bCollider.enabled = false;

			pickUpParticles.Play();
			followAiParticles.Stop();
		}
	}

	public void ResetHeart()
	{
		sRenderer.enabled = true;
		bCollider.enabled = true;
	}
}
