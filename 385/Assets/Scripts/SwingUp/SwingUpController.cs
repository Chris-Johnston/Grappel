using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SwingUpController : MonoBehaviour
{
    /// <summary>
    /// Should the console print out debug messages
    /// </summary>
    public bool ShowDebugMessages = false;

    /// <summary>
    /// At what point should the water cheat to catch up to the player
    /// </summary>
    public const float WATER_CATCHUP_DISTANCE = 11.5f;
    /// <summary>
    /// Max number of hook isntances to have spawned all at once
    /// </summary>
    public const int MAX_NUM_INSTANCES = 25;

    /// <summary>
    /// Set to true when the player has lost
    /// </summary>
    public bool LoseState = false;

    /// <summary>
    /// the text that shows how high the player has gone
    /// in the format
    /// 
    /// Height: 123.0
    /// </summary>
    public Text HeightText;

    /// <summary>
    /// Objects that are enabled on lose
    /// </summary>
    public GameObject LoseObjects;

    /// <summary>
    /// The text that reads the final height score
    /// </summary>
    public Text FinalHeightText;

    /// <summary>
    /// The maxium height reached by the player
    /// </summary>
    public int MaxHeight = 0;

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
    /// The target position of the water level
    /// </summary>
    private Vector3 WaterTargetPosition;

    /// <summary>
    /// The position of the lower left bound to spawn objects
    /// relative to the current CameraGrabPosition
    /// </summary>
    public Vector3 SpawnPositionBottomLeftBound;

    /// <summary>
    /// The position of the upper right bound to spawn objects
    /// relative to the current CameraGrabPosition
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
        if (ShowDebugMessages)
        {
            Debug.Log("Ded");
        }
        
        FinalHeightText.text = $"Final Height: {MaxHeight}";
        LoseObjects.SetActive(true);

        // disable and clear all the points
        foreach (var point in PointInstances)
        {
            point.SetActive(false);
        }

        PointInstances.Clear();

        // enable the lose text, show the score
        // wait a few seconds and then reload the scene

        StartCoroutine(RestartCurrentLevelAfterTime(3.0f));
    }

	void Start ()
    {
        // get the reference to the player's rope system
        PlayerRopeSystemReference = PlayerReference.GetComponentInChildren<RopeSystem>();

        // set the initial position for the camera
        CameraGrabPos = ActiveMainCamera.transform.position;

        WaterTargetPosition = WaterReference.transform.position;
	}

    /// <summary>
    /// Spawns a new grapple point in the region between the bounds of the SpawnPositionBounds
    /// </summary>
    public void SpawnNewGrapplePoint()
    {
        // spawn a new grapple point in the region between the two vector3s
        var range = SpawnPositionUpperRightBound - SpawnPositionBottomLeftBound;
        var point = CameraGrabPos + SpawnPositionBottomLeftBound + new Vector3(range.x * Random.value, range.y * Random.value, range.z * Random.value);
        // enforce the Z to be at 0, ignore the camera position
        point.z = 0f;

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
                if (ShowDebugMessages)
                {
                    Debug.Log("New Y position");
                }
                
                // then update the y position of the grab point
                CameraGrabPos = new Vector3(0, position.y, -10);

                // spawn three points when this happens
                SpawnNewGrapplePoint();
                SpawnNewGrapplePoint();
                SpawnNewGrapplePoint();

                //Debug.Log($"Difference was {CameraGrabPos.y - WaterReference.transform.position.y}");

                // if the water is behind, then let it catch up
                if (CameraGrabPos.y - WaterTargetPosition.y > WATER_CATCHUP_DISTANCE)
                {
                    //Debug.Log("Update");
                    var newPos = (CameraGrabPos - new Vector3(0, WATER_CATCHUP_DISTANCE, 0));
                    newPos.z = 0;   
                    WaterTargetPosition = newPos;
                }
            }
        }
        else
        {
            // check that the player isn't going out of bounds of the camera
            if (CameraGrabPos.y < PlayerReference.transform.position.y - 3)
            {
                if (ShowDebugMessages)
                {
                    Debug.Log("Player went high");
                }

                CameraGrabPos += new Vector3(0, 4, 0);

                // spawn three points when this happens
                SpawnNewGrapplePoint();
                SpawnNewGrapplePoint();
                SpawnNewGrapplePoint();
            }
        }

        // update the position of the camera
        ActiveMainCamera.transform.position = Vector3.MoveTowards(ActiveMainCamera.transform.position, CameraGrabPos, 0.3f);

        // update the positioon of the water
        WaterTargetPosition += (1f + (MaxHeight / 70.0f)) * WaterMoveRatePerSecond * Time.deltaTime;

        if (ShowDebugMessages)
        {
            Debug.Log($"Current rate : {(1f + MaxHeight / 30.0f) * WaterMoveRatePerSecond}");
        }

        if (WaterTargetPosition != null && !LoseState)
        {
            WaterReference.transform.position = Vector3.MoveTowards(WaterReference.transform.position, WaterTargetPosition, 0.2f);
        }            

        if (HeightText != null)
        {
            if (PlayerReference.transform.position.y > MaxHeight)
            {
                MaxHeight = (int)PlayerReference.transform.position.y;
            }
            
            HeightText.text = $"Height: {MaxHeight}";
        }

        if (PointInstances?.Count > MAX_NUM_INSTANCES)
            PointInstances.RemoveRange(0, PointInstances.Count - MAX_NUM_INSTANCES);
	}

    /// <summary>
    /// Waits the specified duration then reloads the level
    /// </summary>
    /// <param name="delay">time to wait in seconds</param>
    /// <returns></returns>
    IEnumerator RestartCurrentLevelAfterTime(float delay)
    {
        yield return new WaitForSeconds(delay);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
