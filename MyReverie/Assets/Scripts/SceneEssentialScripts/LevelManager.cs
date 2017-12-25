using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

	//Publics
	public GameObject currentCheckpoint;
//	public string currentTag;
	public float respawnTime = 0.5f;
	public Player player;
	public int coinCount;
	public List<AiPatrolling> ais;
	public List<RevealButton> revButtons;
	public List<HeartPower> heartPower;
//	public List<GameObject> stateObjects;
	public List<PlatformController> platforms;
//	public List<Lever> levers;
//	public List<FallingPlatform> fallingPlatforms;
//	public List<PickUpGlobe> orbs;
//	public List<PickUpSecret> secrets;
	public int numberOrbs;
	public bool respawning;
	public bool isRespawning = false; //Used for other stuff to know that the player is dead.

	//For act 2 collectables
	public int heartsCollected;
	public bool act2WinCondition;

	//Privates
	private LovedOne lovedOne;
	private IntoLine intoLine;
	private Scene scene;
	private Checkpoint check;

	[HideInInspector]
	public static LevelManager lManager;

	public static LevelManager instance {	// Makes it possible to call script easily from other scripts
		get {
			if (lManager == null) {
				lManager = FindObjectOfType<LevelManager>();
			}
			return lManager;
		}
	}

	void Awake()
	{
		if(lManager == null)
		{
			DontDestroyOnLoad(gameObject);
			lManager = this;
		}
		else if(lManager != this)
		{
			Destroy(gameObject);
		}


	}

	void Start () 
	{
//		scene = SceneManager.GetActiveScene();
//		if (GameObject.Find("Music") == null && GameObject.Find("Music(Clone)") == null) {
//			Debug.Log("StartMusic");
//			GameObject instance = (GameObject)Instantiate(Resources.Load("Music")); // Instantiates music if none is found
//		}
		lovedOne = ((GameObject.FindGameObjectWithTag("lovedOne") == null)?null:GameObject.FindGameObjectWithTag("lovedOne").GetComponent<LovedOne>());
		player = GameObject.Find("Player").GetComponent<Player>();
		intoLine = player.gameObject.GetComponent<IntoLine>();
		FillLists();
	}

	void Update()
	{
		if (act2WinCondition)
		{
			if (heartsCollected == 0)
			{
				StartCoroutine("LoadNextLevel");
			}	
		}

		//Temporary oppotunity to get out of levels.
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			SceneManager.LoadScene("StartMenu");
		}
		//spawnPoint = GameObject.FindGameObjectWithTag("spawnpoint");

		if (player == null)
		{
			player = FindObjectOfType<Player>();
		}

		/*if (SceneManager.GetActiveScene() != scene) {
			Debug.Log("Scene changed");
			FillLists();
			scene = SceneManager.GetActiveScene();
		}*/
	}

	//Handles all player respawning
	IEnumerator Respawned() 
	{
		if (lovedOne != null)
		{
			lovedOne.StartCoroutine("ResetLovedOne");	//Reset LovedOne	
		}
		respawning = true;
		isRespawning = true;
		GameObject graphics = player.gameObject.transform.GetChild(0).gameObject;
		ParticleSystem particleEffect = player.gameObject.transform.GetChild(1).GetChild(0).GetComponent<ParticleSystem>();
		BoxCollider2D boxCol = player.gameObject.GetComponent<BoxCollider2D>();
		player.enabled = false;
		graphics.SetActive(false);
		particleEffect.Play();

		yield return new WaitForSeconds(0.9f);

		ResetAis(ais);				//Resetting AIs
		ResetReveals(revButtons);	//Resetting Reveal Buttons
		ResetHearts(heartPower);	//Resetting Heart Power Ups
		ResetPlatforms(platforms);	//Resetting Moving Platforms

		particleEffect.Stop();
		player.transform.position = currentCheckpoint.transform.position;
//		player.tag = currentTag;
		intoLine.ResetDirection(currentCheckpoint.GetComponent<Checkpoint>().currentDirection);
//		StartCoroutine(	ColorStates.instance.ChangeColor(Color.white, 1f));
		player.velocity.x = 0f;
		player.velocity.y = 0f;

		yield return new WaitForSeconds(respawnTime);

		player.enabled = true;
		graphics.SetActive(true);
		boxCol.enabled = true;
		isRespawning = false;
		Debug.Log ("Respawned!");
	}

	public void Respawn()
	{
		StartCoroutine(Respawned());
	}

	public IEnumerator NextLevel(string myLevel)
	{
		Destroy(player.gameObject);
		Destroy (this.gameObject);
		yield return new WaitForEndOfFrame();

		//fade out the level and load next
		float fadeTime = GameObject.Find("_GM").GetComponent<Fading>().BeginFade(1);
		yield return new WaitForSeconds(fadeTime);

		Application.LoadLevel(myLevel);
	}

	public IEnumerator LoadNextLevel()
	{
		Destroy(player.gameObject);
		Destroy (this.gameObject);
		yield return new WaitForEndOfFrame();	

		float fadeTime = GameObject.Find("_GM").GetComponent<Fading>().BeginFade(1);
		yield return new WaitForSeconds(fadeTime);

		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
	}

	void ResetAis(List<AiPatrolling> theList)
	{
		if (theList.Count == 0)
		{
			return;
		}
		else 
		{
			for (int i = 0; i < theList.Count; i++)
			{
				theList[i].StartCoroutine("ResetAi");
			}	
		}
	}
		
	void ResetReveals(List<RevealButton> theList)
	{
		if (theList.Count == 0)
		{
			return;
		}
		else 
		{
			for (int i = 0; i < theList.Count; i++)
			{
				theList[i].StartCoroutine("ResetRevealButton");
			}	
		}
	}

	void ResetHearts(List<HeartPower> theList)
	{
		if (theList.Count == 0)
		{
			return;
		}
		else 
		{
			for (int i = 0; i < theList.Count; i++)
			{
				theList[i].ResetHeart();
			}	
		}
	}
//	void ResetStates(List<GameObject> theList)
//	{
//		if (theList.Count == 0)
//		{
//			return;
//		}
//		else 
//		{
//			for (int i = 0; i < theList.Count; i++)
//			{
//				theList[i].SetActive(true);
//			}	
//		}
//	}		
//	void ResetFallingPlatforms(List<FallingPlatform> theList)
//	{
//		if (theList.Count == 0)
//		{
//			return;
//		}
//		else
//		{
//			for (int i = 0; i < theList.Count; i++)
//			{
//				theList[i].Reset();
//			}	
//		}
//	}
//
	void ResetPlatforms(List<PlatformController> theList)
	{
		if (theList.Count == 0)
		{
			return;
		}
		else
		{
			for (int i = 0; i < theList.Count; i++)
			{
				theList[i].ResetPlatform();
			}	
		}
	}
//
//	void ResetLevers(List<Lever> theList)
//	{
//		if (theList.Count == 0)
//		{
//			return;
//		}
//		else
//		{
//			for (int i = 0; i < theList.Count; i++)
//			{
//				theList[i].ResetLever();
//			}	
//		}
//	}
//
//	void ResetOrbs(List<PickUpGlobe> theList)
//	{
//		if (theList.Count == 0)
//		{
//			return;
//		}
//		else
//		{
//			for (int i = 0; i < theList.Count; i++)
//			{
//				theList[i].ResetOrb();
//			}	
//		}
//	}
//	void ResetSeOrbs(List<PickUpSecret> theList)
//	{
//		if (theList.Count == 0)
//		{
//			return;
//		}
//		else
//		{
//			for (int i = 0; i < theList.Count; i++)
//			{
//				theList[i].ResetSeOrb();
//			}	
//		}
//	}

//	void ResetParticles()
//	{
//		foreach(GameObject particle in GameObject.FindGameObjectsWithTag("DynamicParticle")) {
//
//			DynamicParticle dp = particle.GetComponent<DynamicParticle>();
//			dp.Destroy();
//		}
//	}

	void FillLists() {

		foreach(GameObject aiObject in GameObject.FindGameObjectsWithTag("ai"))
		{
			AiPatrolling aiPatrol = aiObject.GetComponent<AiPatrolling>();
			ais.Add(aiPatrol);
		}
		foreach(GameObject reObject in GameObject.FindGameObjectsWithTag("revealButton"))
		{
			RevealButton revBut = reObject.GetComponent<RevealButton>();
			revButtons.Add(revBut);
		}
		foreach(GameObject heObject in GameObject.FindGameObjectsWithTag("heartPower"))
		{
			HeartPower hPow = heObject.GetComponent<HeartPower>();
			heartPower.Add(hPow);
		}
		heartsCollected = heartPower.Count;
//		foreach(GameObject oObject in GameObject.FindGameObjectsWithTag("orb")) 
//		{
//			PickUpGlobe oOrb = oObject.GetComponent<PickUpGlobe>();
//			orbs.Add(oOrb);
//		}
//		numberOrbs = orbs.Count;
//		foreach(GameObject seObject in GameObject.FindGameObjectsWithTag("coin")) 
//		{
//			PickUpSecret sOrb = seObject.GetComponent<PickUpSecret>();
//			secrets.Add(sOrb);
//		}
//		foreach(GameObject sObject in FindGameObjectsWithTags(new string[]{"orangeDestroy", "coin"})) 
//		{
//			stateObjects.Add(sObject);
//		}
//
//		foreach(GameObject dObject in GameObject.FindGameObjectsWithTag("dissPlatform")) 
//		{
//			FallingPlatform fPlatform = dObject.GetComponent<FallingPlatform>();
//			fallingPlatforms.Add(fPlatform);
//		}
//
		foreach(GameObject pObject in GameObject.FindGameObjectsWithTag("movingPlatform"))
		{
			PlatformController pController = pObject.GetComponent<PlatformController>();
			platforms.Add(pController);
		}
//
//		foreach(GameObject lObject in GameObject.FindGameObjectsWithTag("Lever")) 
//		{
//			Lever lever = lObject.GetComponent<Lever>();
//			levers.Add(lever);
//		}
	}
}
