using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LovedOne : MonoBehaviour {

	//Publics
	public string nextLevelName;

	//Privates
	private Vector3 startPos;
	private IntoLine intoline;
	private GameObject pSystemGO;
	private BoxCollider2D bCollider;
	private SpriteRenderer sRenderer;
	private ParticleSystem psystem;
	private Player player;

	void Start () 
	{
		pSystemGO = transform.GetChild(3).gameObject;
		psystem = transform.GetChild(1).GetChild(0).GetComponent<ParticleSystem>();
		startPos = transform.position;
		intoline = GetComponent<IntoLine>();
		bCollider = GetComponent<BoxCollider2D>();
		sRenderer = transform.GetComponentInChildren<SpriteRenderer>();
		player = GetComponent<Player>();
	}

	void Update () 
	{
		
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.name == "Player")
		{
			CompletedLevel();
		}
	}

	void CompletedLevel() 
	{
		StartCoroutine(LevelManager.instance.NextLevel(nextLevelName));	
	}

	public IEnumerator ResetLovedOne()
	{
		pSystemGO.SetActive(false);
		bCollider.enabled = false;
		sRenderer.enabled = false;
		player.enabled = false;
		psystem.Play();
		yield return new WaitForSeconds(0.9f);
		transform.position = startPos;
		intoline.ResetDirection(IntoLine.Direction.Floor);
		yield return new WaitForSeconds(0.5f);
		pSystemGO.SetActive(true);
		bCollider.enabled = true;
		sRenderer.enabled = true;
		player.enabled = true;
	}

}
