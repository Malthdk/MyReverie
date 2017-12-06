using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public static GameManager GM;

	void Awake()
	{
		if(GM == null)
		{
			DontDestroyOnLoad(gameObject);
			GM = this;
		}
		else if(GM != this)
		{
			Destroy(gameObject);
		}
	}
}
