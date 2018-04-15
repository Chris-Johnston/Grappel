using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controller script for the player
/// </summary>
public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// Toggle to show the debugging lines
    /// </summary>
    public bool ShowDebugging = true;

    // constants for the inputs to check
    // the strafe axis is defined negative going to the left and positive going
    // to the right. Currently A goes left, D goes right
    public const string STRAFE = "Strafe";

    /// <summary>
    /// Reference to the RopeSystem attribute of the Player.
    /// If there are more than one, could make another one (or an array).
    /// </summary>
    public RopeSystem RopeSystem;

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        // Check that the ref to RopeSystem is not null, and the rope system is connected
        // the == true is not redundant because of the ?. operator
        if (RopeSystem?.IsRopeConnected() == true)
        {
            // check the strafe axis
            var strafeAmount = Input.GetAxis(STRAFE);

            // get the perpendicular axis to the rope from the player
            var perpendicularAxis = RopeSystem.GetRopePerpendicularAxis().Value;

            // show some debug lines if debugging enabled
            if (ShowDebugging)
            {
                Debug.DrawRay(transform.position, strafeAmount * perpendicularAxis, Color.red);
            }
        }
	}
}
