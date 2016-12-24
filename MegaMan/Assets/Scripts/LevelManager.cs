using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
	public Transform mainMenu, optionsMenu;

	public void LoadScene(string name)
	{
		Application.LoadLevel(name);
	}

	public void QuitGame()
	{
		Application.Quit();
	}

	public void OptionsMenu(bool clicked)
	{
		if (clicked == true) 
		{
			optionsMenu.gameObject.SetActive (true);
			mainMenu.gameObject.SetActive (false);
		}
		else
		{
			mainMenu.gameObject.SetActive (true);
			optionsMenu.gameObject.SetActive (false);
		}
	}
}
