using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Wiggles the camera around on the main menu to make it look cool or something
/// wiggle the x position of the camera along a sine wave and have the camera always point towards the origin
/// </summary>
public class MainMenuCameraWiggler : MonoBehaviour
{
    /// <summary>
    /// Where the camera should always be facing
    /// </summary>
    public Vector3 AlwaysPointPosition = Vector3.zero;

    /// <summary>
    /// How strong the camera wiggling action should be
    /// </summary>
    [Range(0, 10)]
    public float Ampltude = 5.0f;

    /// <summary>
    /// how quick the camera wiggling action is
    /// </summary>
    [Range(0.1f, 5)]
    public float Frequency = 0.3f;

    /// <summary>
    /// the original position of the camera
    /// </summary>
    private Vector3 originalPosition;

    void Start()
    {
        // store the original position so that we can use it for wigglin'
        originalPosition = transform.position;
    }

	void Update ()
    {
        // wiggle the x axis of the camera
        var pos = originalPosition;
        pos.x += Mathf.Sin(Time.time / Frequency) * Ampltude;
        transform.position = pos;

        // update the camera to always point towards the position specified
        transform.LookAt(AlwaysPointPosition);
	}
}
