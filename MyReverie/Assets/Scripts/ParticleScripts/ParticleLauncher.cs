using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleLauncher : MonoBehaviour {

	public ParticleSystem particleLauncher;
	public ParticleSystem splatterParticles;
	public Gradient particleColorGradient;
	public ParticleDecalPool splatDecalPool;

	List<ParticleCollisionEvent> collisionEvents;

	[HideInInspector]
	public LevelManager lvlManager;

	void Start () 
	{
		collisionEvents = new List<ParticleCollisionEvent> ();
		lvlManager = GameObject.Find ("LevelManager").GetComponent<LevelManager> ();
	}

	void OnParticleCollision(GameObject other)
	{
		ParticlePhysicsExtensions.GetCollisionEvents (particleLauncher, other, collisionEvents);

		for (int i = 0; i < collisionEvents.Count; i++) 
		{
			splatDecalPool.ParticleHit (collisionEvents [i], particleColorGradient);
			EmitAtLocation (collisionEvents[i]);
		}

	}

	public void EmitAtLocation(ParticleCollisionEvent particleCollisionEvent)
	{
		splatterParticles.transform.position = particleCollisionEvent.intersection;
		splatterParticles.transform.rotation = Quaternion.LookRotation (particleCollisionEvent.normal);
		ParticleSystem.MainModule psMain = splatterParticles.main;
		psMain.startColor = particleColorGradient.Evaluate (Random.Range (0.6f, 0.8f));
		splatterParticles.Emit (40);
	}

	void Update () 
	{
		if (lvlManager.respawning) 
		{
			ParticleSystem.MainModule psMain = particleLauncher.main;
			psMain.startColor = particleColorGradient.Evaluate (Random.Range (0f, .8f));
			particleLauncher.Emit (1);
			lvlManager.respawning = false;
		}
	}
}