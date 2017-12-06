using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour {

	//Publics
	public Button levelButton;
	public GUIStyle buttonStyle;

	//Privates
	private const int buttonsPerRow = 3;
	private const int rowsPerPage = 5;

	private static int numColumns = 2*buttonsPerRow + 1;
	private static int numRows = 2*rowsPerPage + 1;

	private float gridBoxWidth = Screen.width / numColumns;
	private float gridBoxHeight = Screen.height / numRows;

	private static int numLevels = 15;
	public static int completedLevelsAct1 = 15; //this probably have to be divided into acts
	public static int completedLevelsAct2 = 15;

	private string levelName;

	void OnGUI()
	{
		for (var i = 0; i < numLevels; i++)
		{

			int buttonRow = (int)i / buttonsPerRow;
			int gridRow = 2 * buttonRow + 3;
			float top = gridBoxHeight * gridRow;

			int buttonIndex = i % buttonsPerRow;
			int gridColumn = 2 * buttonIndex + 1;
			float left = gridBoxWidth * gridColumn;

			Scene activeScene = SceneManager.GetActiveScene();

			if (activeScene.name == "LevelSelectAct" + "1")
			{
				levelName = "LEVEL1." + (i + 1);
				//This is for making certain levels locked
				if ( i > completedLevelsAct1 - 1)
				{
					GUI.enabled = false;
				}
			}
			else if (activeScene.name == "LevelSelectAct" + "2")
			{
				levelName = "LEVEL2." + (i + 1);
				//This is for making certain levels locked
				if ( i > completedLevelsAct2 - 1)
				{
					GUI.enabled = false;
				}
			}
			//Button button = (Button) Instantiate(levelButton);
			//buton.
			if (GUI.Button (new Rect (left, top, gridBoxWidth, gridBoxHeight), levelName, buttonStyle))
			{
				SceneManager.LoadScene (levelName);
			}
		}
	}
}
