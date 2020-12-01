﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadLevelByName : MonoBehaviour {

	//private string[] levelNamesInSerial = new string[] {"Level_01", "Level_02_New", "Level_03_New"};
	public int totalNumOfAvailableLevels = 10;

	[SerializeField] PlayerStats ps;

	public void LoadLevelGivenTheName(string levelName){
		MatControlsStatManager.gameStateChanged(GameState.GAME_UI);
		SceneManager.LoadScene (levelName);
	}

	public void RetryLevel(){
		//PlayerPrefs.SetInt("PLAYER_LIFE", 5);

		/*
		PlayerPrefs.DeleteKey("IS_CHKP_REACHED");
		PlayerPrefs.DeleteKey("CHKP_X");
		PlayerPrefs.DeleteKey("CHKP_Y");
		PlayerPrefs.DeleteKey("CHKP_Z");
		*/
		ps.CheckPointPassed = false;

		//PlayerPrefs.SetInt("PLAYER_LIFE", 3);
		ps.PlayerLives = 3;
		SceneManager.LoadScene (PlayerPrefs.GetInt("CURRENT_LEVEL_SERIAL"));

	}

	public void LoadNextLevel(){

		if (PlayerPrefs.GetInt ("CURRENT_LEVEL_SERIAL") + 1 <= totalNumOfAvailableLevels) 
		{
			PlayerPrefs.SetInt("CURRENT_LEVEL_SERIAL", PlayerPrefs.GetInt ("CURRENT_LEVEL_SERIAL") + 1 );
			//PlayerPrefs.SetInt("PLAYER_LIFE", 3);
			ps.PlayerLives = 3;
			SceneManager.LoadScene (PlayerPrefs.GetInt("CURRENT_LEVEL_SERIAL"));

		}


		//PlayerPrefs.SetInt("PLAYER_LIFE", 5);
		//string lastLevelName = PlayerPrefs.GetString ("LAST_LEVEL");


		/*
		int lastLevelSerial = System.Array.IndexOf (levelNamesInSerial, lastLevelName);
		if (lastLevelSerial > -1 && lastLevelSerial < levelNamesInSerial.Length) {
			int nextLevel = lastLevelSerial + 1;
			LoadLevelGivenTheName(levelNamesInSerial[nextLevel]);
		}*/

	}

	public void rateThisApp(){
		Application.OpenURL("market://details?id=com.codespec.trapped");
	}
	public void visitMySocialPage(string webURL){
		Application.OpenURL(webURL);
	}

}
