using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingUpController : MonoBehaviour
{

    /// <summary>
    /// Reference to the camera to use
    /// </summary>
    public GameObject ActiveMainCamera;

    /// <summary>
    /// How quick the water should move up per second
    /// </summary>
    public Vector3 CameraMoveRatePerSecond;

    /// <summary>
    /// Reference to an instance of the player prefab
    /// </summary>
    public GameObject PlayerReference;

    /// <summary>
    /// Reference to the player's rope system which is set in Start
    /// </summary>
    private RopeSystem PlayerRopeSystemReference;

    /// <summary>
    /// The position of the last grapple point that was grabbed
    /// </summary>
    private Vector3 CameraGrabPos;

    /// <summary>
    /// The position of the lower left bound to spawn objects
    /// </summary>
    public Vector3 SpawnPositionBottomLeftBound;

    /// <summary>
    /// The position o the upper right bound to spawn objects
    /// </summary>
    public Vector3 SpawnPositionUpperRightBound;

    /// <summary>
    /// Which object to instantiate
    /// </summary>
    public GameObject GrapplePointPrefab;

    /// <summary>
    /// List of all instances of the points
    /// </summary>
    public List<GameObject> PointInstances = new List<GameObject>();

    /// <summary>
    /// Reference to the water that will expand
    /// </summary>
    public GameObject WaterReference;

    /// <summary>
    /// How far to move the water per second
    /// </summary>
    public Vector3 WaterMoveRatePerSecond;

    /// <summary>
    /// Called when the player hits the water collision
    /// </summary>
    public void PlayerDie()
    {
        Debug.Log("Ded");
        // enable the lose text, show the score
        // wait a few seconds and then reload the scene
    }

	void Start ()
    {
        // get the reference to the player's rope system
        PlayerRopeSystemReference = PlayerReference.GetComponentInChildren<RopeSystem>();

        // set the initial position for the camera
        CameraGrabPos = ActiveMainCamera.transform.position;
	}

    /// <summary>
    /// Spawns a new grapple point in the region between the bounds of the SpawnPositionBounds
    /// </summary>
    public void SpawnNewGrapplePoint()
    {
        // spawn a new grapple point in the region between the two vector3s
        var range = SpawnPositionUpperRightBound - SpawnPositionBottomLeftBound;
        var point = SpawnPositionBottomLeftBound + new Vector3(range.x * Random.value, range.y * Random.value, range.z * Random.value);

        var newItem = Instantiate(GrapplePointPrefab);
        // should set the parent of this prefab to be this object

        // set the position of this new object
        GrapplePointPrefab.transform.position = point;

        // add to the list of points
        PointInstances.Add(newItem);
    }

	void Update ()
    {
        // if connected
	    if (PlayerRopeSystemReference.IsRopeConnected())
        {
            // get the anchor point
            var point = PlayerRopeSystemReference.RopeAnchorPoint;

            // get the position of the anchored point
            var position = point.transform.position;

            // if the camera grab pos is unset, or the new anchor point is greater than the current point
            if (CameraGrabPos.y < position.y)
            {
                //Debug.Log("New Y position");

                // then update the y position of the grab point
                CameraGrabPos = new Vector3(0, position.y, -10);

                

                // spawn two points when this happens
                SpawnNewGrapplePoint();
                SpawnNewGrapplePoint();
            }
        }
        else
        {
            // check that the player isn't too far above
            if (CameraGrabPos.y < PlayerReference.transform.position.y + 4)
            {
                // Debug.Log("The player went too high");
            }
        }

        // update the position of the camera
        if (CameraGrabPos != null)
        {
            ActiveMainCamera.transform.position = CameraGrabPos;
        }

        // update the positioon of the water
        WaterReference.transform.position += WaterMoveRatePerSecond * Time.deltaTime;
            
	}
}
