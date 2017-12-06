using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveParticles : MonoBehaviour {

	public Transform target;
	public float speed = 3f;

	private ParticleSystem pSystem;
	public ParticleSystem.Particle[] particles;
	void Start()
	{
		pSystem = GetComponent<ParticleSystem>();

	}

	void Update()
	{
		particles = new ParticleSystem.Particle[pSystem.particleCount];

		pSystem.GetParticles(particles);


		for (int i = 0; i < particles.Length; i++)
		{
			float distance = Vector3.Distance(target.position, particles[i].position);
			if (distance > 0.1f)
			{
				particles[i].position = Vector3.MoveTowards(particles[i].position, target.position, Time.deltaTime * speed);
			}
		}

		pSystem.SetParticles(particles, particles.Length);
	}
}

