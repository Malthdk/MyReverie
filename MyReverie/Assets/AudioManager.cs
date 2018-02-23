using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour {

	public static AudioManager _instance;

	void Awake() {
		if (!_instance) {
			_instance = this;
			DontDestroyOnLoad(transform.gameObject);
		} else {
			Destroy(gameObject);
		}
	}

	void Start () {
		AkSoundEngine.PostEvent ("Background", gameObject);
	}
}
