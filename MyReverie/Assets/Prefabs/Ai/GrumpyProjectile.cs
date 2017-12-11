using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrumpyProjectile : MonoBehaviour {

	//Publics
	public ParticleSystem deathParticles;

	//Privates
	private SpriteRenderer sRenderer;

	void Start () 
	{
	}
	

	void Update () 
	{
		
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "killTag")
		{
			other.gameObject.SetActive(false);
			DestroyProjectile();
		}
		if (other.tag == "ground")
		{
			DestroyProjectile();
		}
	}

	void DestroyProjectile()
	{
		gameObject.SetActive(false);
		deathParticles.Play();
	}
}
