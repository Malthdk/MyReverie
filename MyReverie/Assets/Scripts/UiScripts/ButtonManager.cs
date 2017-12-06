using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ButtonManager : MonoBehaviour {
	
	// For loading
//	public Slider loadingbar;
//	public GameObject loadingImage;
//	private AsyncOperation async;


	void Update() {
		if (Input.GetKeyDown(KeyCode.Escape)) {
			LoadLevelSelection();
		}
	}

	// Loads scene without a loadscreen
	public void LoadScene(string sceneName) {
		SceneManager.LoadScene(sceneName);
	}

//	// Loads scene with a loadscreen
//	public void LoadSceneWithLoadingImage(string sceneName){ 
//		GetLevel(sceneName);
//	}

	public void LoadLevelSelection() {
		Destroy(GameObject.Find("Canvas"));
		LevelManager.instance.NextLevel("LevelSelection");
	}

	// Exits the game
	public void ExitApplication() {
		Application.Quit();
	}
		
	// Sets loading image to active and starts coroutine
//	private void GetLevel(string level)
//	{
//		loadingImage.SetActive(true);
//		StartCoroutine(LoadLevelWithBar(level));
//	}
//
//	// While level is loading display loadingbar
//	IEnumerator LoadLevelWithBar (string level)
//	{
//		async = Application.LoadLevelAsync(level);
//		while (!async.isDone)
//		{
//			loadingbar.value = async.progress;
//			yield return null;
//		}
//	}

}
