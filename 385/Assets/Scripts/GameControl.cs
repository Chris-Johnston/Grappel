using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton class to store settings or data that must persist across scenes.
/// </summary>
public class GameControl : MonoBehaviour
{
    public static GameControl instance;

    /// <summary>
    /// Did the user select Controller mode from the menu screen?
    /// </summary>
    public static bool ControllerMode = false;

    /// <summary>
    /// Method to toggle Controller mode. Used by the OnClick() method of menu
    /// buttons for selecting mouse/controller mode.
    /// </summary>
    /// <param name="ControllerModeOn">Turn on controller mode?</param>
    public void ToggleControllerMode(bool ControllerModeOn)
    {
        ControllerMode = ControllerModeOn;
    }

    void Awake ()
    {
        if (instance == null) // if there's no existing instance of this class
        {
            // Set the reference
            instance = this;

            // This makes the instance persist across scenes
            DontDestroyOnLoad(gameObject);
        }
        // if there's an existing instance and it's not this
        else if (instance != this)
        {
            Destroy(gameObject); // burn
        }
	}
}
