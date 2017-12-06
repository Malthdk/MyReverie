using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {
	
	[HideInInspector]
	public static PlayerManager _instance;

	public static PlayerManager instance {	// Makes it possible to call script easily from other scripts
		get {
			if (_instance == null) {
				_instance = FindObjectOfType<PlayerManager>();
			}
			return _instance;
		}
	}
		
	public void KillPlayer()
	{
		AkSoundEngine.PostEvent ("Die", gameObject);
		LevelManager.lManager.Respawn();
	}
}
