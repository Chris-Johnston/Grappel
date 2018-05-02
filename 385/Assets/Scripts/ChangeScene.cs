using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour 
{
	public static bool SelectedControllerMode = false;

	public void ToggleControllerMode(bool ControllerModeOn)
	{
		SelectedControllerMode = ControllerModeOn;
	}

	public void ChangeToScene (string sceneToChangeTo) 
	{
		SceneManager.LoadScene (sceneToChangeTo);
	}
}
