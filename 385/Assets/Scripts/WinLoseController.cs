using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinLoseController : MonoBehaviour
{
    /// <summary>
    /// Object to enable when the player wins
    /// </summary>
    public GameObject WinObject;

    /// <summary>
    /// Object to enable when the player loses
    /// </summary>
    public GameObject LoseObject;

	void Start ()
    {
        // disable both objects by default
        WinObject?.SetActive(false);
        LoseObject?.SetActive(false);
	}
	
    /// <summary>
    /// Enable the win object, disable the lose object
    /// </summary>
	public void Win()
    {
        WinObject?.SetActive(true);
        LoseObject?.SetActive(false);
    }

    /// <summary>
    /// Enable the lose object, disable the win object
    /// </summary>
    public void Lose()
    {
        WinObject?.SetActive(false);
        LoseObject?.SetActive(true);
    }

	public void ChangeToScene (string sceneToChangeTo)
    {
		SceneManager.LoadScene (sceneToChangeTo);
	}
}
