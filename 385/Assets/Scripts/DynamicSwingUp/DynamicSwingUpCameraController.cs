using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Camera controller script for the dynamic swing up scene
/// This is used to ensure that the camera stays aligned to the course
/// </summary>
public class DynamicSwingUpCameraController : MonoBehaviour
{
    /// <summary>
    /// The offset to apply to all of the camera positions
    /// </summary>
    public Vector3 CameraPositionOffset = new Vector3();

    /// <summary>
    /// The target position that the camera should move toward
    /// </summary>
    public Vector3 TargetCameraPosition = new Vector3();

    /// <summary>
    /// The max distance delta to move the camera towards each update
    /// </summary>
    [Range(0, 10)]
    public float MaxDistanceDelta = 0.5f;

	void Update ()
    {
        // get the position that the camera should move towards and set it
        var next = Vector3.MoveTowards(transform.position, CameraPositionOffset + TargetCameraPosition, MaxDistanceDelta * Time.deltaTime);
        transform.position = next;
	}
}
