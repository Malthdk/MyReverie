using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGParticles : MonoBehaviour {

	//Publics
	[HideInInspector]
	public bool hasCollected = false;
	public Mesh mesh1;
	public Mesh mesh2;

	//Privates
	private ParticleSystem pSystem;
	private ParticleSystem.NoiseModule noiseModule;
	private ParticleSystem.MainModule mainModule;
	private ParticleSystem.LimitVelocityOverLifetimeModule limVelModule;
	private ParticleSystem.EmissionModule emissionModule;
	private ParticleSystem.VelocityOverLifetimeModule velLifeModule;
	private ParticleSystem.ColorOverLifetimeModule colLifeModule;
	private ParticleSystem.RotationBySpeedModule rotSpeedModule;
	private ParticleSystem.ShapeModule shapeModule;

	private ParticleSystem.Particle[] particles;
	private float startSpeedMax;
	private float startSpeedMin;

	//For fading
	Color newColor = new Color(0.137f, 0.137f, 0.137f, 1f);
	Gradient grad = new Gradient();

	[HideInInspector]
	public static BGParticles bgParticles;
	public static BGParticles instance {	// Makes it possible to call script easily from other scripts
		get {
			if (bgParticles == null) {
				bgParticles = FindObjectOfType<BGParticles>();
			}
			return bgParticles;
		}
	}

	void Start () 
	{
		
		pSystem = GetComponent<ParticleSystem>();
		noiseModule = pSystem.noise;
		mainModule = pSystem.main;
		startSpeedMax = mainModule.startSpeed.constantMax;
		limVelModule = pSystem.limitVelocityOverLifetime;
		velLifeModule = pSystem.velocityOverLifetime;
		emissionModule = pSystem.emission;
		colLifeModule = pSystem.colorOverLifetime;
		rotSpeedModule = pSystem.rotationBySpeed;

	}
	

	void Update () 
	{
		//This is for when the player is transforming
		if (IntoLine.instance.transforming)
		{
			//Some effect

			//noiseModule.frequency = 5f;
			//noiseModule.strength = 5f;
		}
		else if (!IntoLine.instance.transforming && !hasCollected)
		{
			//Some effect


			noiseModule.frequency = 0.5f;
			noiseModule.strength = 0.15f;
		}

		//This is for when the player is dead
		if ( LevelManager.instance.isRespawning)
		{
			
			limVelModule.enabled = false;
			velLifeModule.enabled = true;
		}
		else
		{

			//mainModule.startSpeed = 0.1f;
			limVelModule.enabled = true;
			velLifeModule.enabled = false;
			//emissionModule.rateOverTime = 20f;
			//mainModule.startLifetime = 10f;
		}

		//This is for when the player has collected a collectable
		if (hasCollected)
		{
			//For changing mesh
			//ChangeMesh(mesh2);

			//DeLifetime();
			//StartCoroutine("DeNoise");
			//noiseModule.frequency = 1f;



			//StartCoroutine("Brighten");
			//rotSpeedModule.enabled = true;

		}
		else
		{
			//ChangeMesh(mesh1);

			//StartCoroutine("Darken");
			//rotSpeedModule.enabled = false;
		}
	}

	public void DeLifetime()
	{
		int numParticlesAlive = pSystem.GetParticles(particles);

		// Change only the particles that are alive
		for (int i = 0; i < numParticlesAlive; i++)
		{
			particles[i].startLifetime = 2f;
			//particles[i].velocity += Vector3.up * 0.01f;
		}

		pSystem.SetParticles(particles, numParticlesAlive);
	}

	public IEnumerator DeNoise()
	{
		while (noiseModule.strength.constant >= 0.001f)
		{
			ParticleSystem.MinMaxCurve str = noiseModule.strength;
			str.constant -= 0.001f;
			noiseModule.strength = str;
			//noiseModule.strength.constant -= 0.001f;
			yield return new WaitForEndOfFrame();
		}
		yield return new WaitForSeconds(8f);
		hasCollected = false;
	}

	public void ChangeMesh(Mesh newMesh)
	{
		var sh = pSystem.shape;
		sh.shapeType = ParticleSystemShapeType.Mesh;
		sh.mesh = newMesh;
	}


	//This brightens the particlesystem
	public IEnumerator Brighten()
	{
		while ( newColor.r < 1f && newColor.g < 1f && newColor.b < 1f)
		{
			//newColor = Color.Lerp(new Color(0.137f, 0.137f, 0.137f), Color.white, 1f); //lerping doesnt work - but the current method is quite primitive.
			newColor.r += 0.005f;
			newColor.g += 0.005f;
			newColor.b += 0.005f;
				
			grad.SetKeys(new GradientColorKey[] 
				{ 
					new GradientColorKey(newColor, 0.0f),
					new GradientColorKey(newColor, 1.0f) 
				}, 
				new GradientAlphaKey[] 
				{ 
					new GradientAlphaKey(1.0f, 0.0f),
					new GradientAlphaKey(1.0f, 0.58f),
					new GradientAlphaKey(0.0f, 1.0f) 
				});

			colLifeModule.color = grad;

			yield return new WaitForEndOfFrame();
		}

		yield return new WaitForSeconds(2f);
		hasCollected = false;
	}

	//This darkens the particlesystem
	public IEnumerator Darken()
	{
		while ( newColor.r > 0.392f && newColor.g > 0.392f && newColor.b > 0.392f)
		{
			newColor.r -= 0.005f;
			newColor.g -= 0.005f;
			newColor.b -= 0.005f;

			grad.SetKeys(new GradientColorKey[] 
				{ 
					new GradientColorKey(newColor, 0.0f),
					new GradientColorKey(newColor, 0.0f)
				}, 
				new GradientAlphaKey[] 
				{ 
					new GradientAlphaKey(1.0f, 0.0f),
					new GradientAlphaKey(1.0f, 0.58f),
					new GradientAlphaKey(0.0f, 1.0f) 
				});

			colLifeModule.color = grad;

			yield return new WaitForEndOfFrame();
		}
			
	}
}
