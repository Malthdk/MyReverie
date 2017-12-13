using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour {

	private bool isShaking = false;
	private float baseX, baseY, baseZ;
	private float intensity = 0.1f;
	private int shakes = 0;

	void Start () 
	{

	}
	

	void Update () 
	{
		baseX = transform.position.x;
		baseY = transform.position.y;
		baseZ = transform.position.z;

		if (isShaking)
		{
			float randomShakeX = Random.Range(-intensity, intensity);
			float randomShakeY = Random.Range(-intensity, intensity);
			float randomShakeZ = Random.Range(0, intensity * 2);

			transform.position = new Vector3(baseX + randomShakeX, baseY + randomShakeY, baseZ + randomShakeZ);

			shakes--;

			if (shakes <= 0)
			{
				isShaking = false;
			}
		}

		//OBS!!! This needs to synergieze with current camera script before it actualy works. This includes boundaries, vertical and horizontal lock. 

		//shake when transforming
		//if (IntoLine.instance.transforming)
		//{
		//	Shake(0.05f, 5);
		//}

		//shake when death
		//if (LevelManager.instance.isRespawning)
		//{
		//	Shake(1f, 10);
		//}
	}

	public void Shake(float in_intensity, int in_shakes)
	{
		isShaking = true;
		shakes = in_shakes; //number of shakes
		intensity = in_intensity; //intensity of each shake (0.05 - 1f) recommended
	}
}
