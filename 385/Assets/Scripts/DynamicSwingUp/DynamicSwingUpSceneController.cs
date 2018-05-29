using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicSwingUpSceneController : MonoBehaviour
{
    /// <summary>
    /// Collection of all of the map sections that are easy
    /// </summary>
    public List<GameObject> EasySections = new List<GameObject>();

    /// <summary>
    /// Collection of all the map sections that are considered "medium" difficulty
    /// </summary>
    public List<GameObject> MediumSections = new List<GameObject>();

    /// <summary>
    /// Collection of all of the map sections that are considered "hard" in difficutly
    /// </summary>
    public List<GameObject> HardSections = new List<GameObject>();

    /// <summary>
    /// How far away should each of the origin sections be placed
    /// </summary>
    public Vector3 SectionSpacing = new Vector3(0, 30f, 0);

    /// <summary>
    /// Reference to the player game object reference
    /// </summary>
    public GameObject PlayerReference;

    /// <summary>
    /// Rope System Component of the player, which is set at runtime on start
    /// </summary>
    private RopeSystem PlayerRopeSystemReference;

    /// <summary>
    /// Counter of how many prefabs have been spawned so far
    /// </summary>
    private int SpawnedCounter = 0;

    /// <summary>
    /// The world coordinates of the last prefab that was spawned
    /// </summary>
    private Vector3 LastSpawnedCenterWorldCoordinates = new Vector3();

    /// <summary>
    /// Queue of all of the section prefab instances
    /// as more are added on, sections are deleted in FIFO order
    /// </summary>
    private Queue<GameObject> SectionPrefabInstances = new Queue<GameObject>();

    /// <summary>
    /// Which transform should all of the sections be parented to
    /// </summary>
    public Transform SectionParentTransform;

    /// <summary>
    /// Reference to the scene camera which has a DynamicSwingUpCameraController
    /// </summary>
    public Camera SceneCamera;

    /// <summary>
    /// SceneCamera's camera controller componenet
    /// </summary>
    private DynamicSwingUpCameraController CameraController;

    /// <summary>
    /// is the level started?
    /// the level should start when the player connects to an object for the first time.
    /// </summary>
    public bool IsStarted = false;

    /// <summary>
    /// How many areas to spawn at the very start
    /// </summary>
    [Range(0, 10)]
    public int InitialSpawnAmount = 3;

    /// <summary>
    /// The current height that the player is at
    /// </summary>
    private float Height = 0f;

    void Start()
    {
        // set the reference to the player's rope system
        PlayerRopeSystemReference = PlayerReference.GetComponentInChildren<RopeSystem>();

        // programatically add a listener to on player grapple hit
        PlayerRopeSystemReference.OnPlayerGrappleHit.AddListener(new UnityEngine.Events.UnityAction(OnPlayerGrappleHit));

        // set the camera controller component
        CameraController = SceneCamera.GetComponent<DynamicSwingUpCameraController>();

        for (int i = 0; i < InitialSpawnAmount; i++)
        {
            SpawnSection();
        }
    }

    /// <summary>
    /// Called when the player makes contact with a grapple point
    /// This should start the game if it isn't started already
    /// and also should be responsible for telling the camera to start moving to the target position
    /// </summary>
    private void OnPlayerGrappleHit()
    {
        // start the level if it isn't already
        if (!IsStarted)
        {
            LevelStart();
        }

        var connectedPoint = GetConnectedGrapplePoint();

        // if the connected point height is above the current height
        if (connectedPoint.y >= Height)
            Height = connectedPoint.y;

        // set the camera position to focus on the point that was just grabbed
        CameraController.TargetCameraPosition = new Vector3(0, Height, 0);
    }

    /// <summary>
    /// Get the player's connected grapple point
    /// </summary>
    /// <returns></returns>
    private Vector2 GetConnectedGrapplePoint()
    {
        return PlayerRopeSystemReference.RopeAnchorPoint.position;
    }

    /// <summary>
    /// Actions that are a result of starting the game
    /// should also disable text that tells the user what's going on
    /// </summary>
    private void LevelStart()
    {
        IsStarted = true;
    }

    /// <summary>
    /// Spawns a new section of the world after the previous one
    /// </summary>
    private void SpawnSection()
    {
        // get the section that we are going to spawn
        var section = GetPrefabSectionToSpawnNext();
        // spawn a copy of it as a child of this object, in world space
        var copy = Instantiate(section, SectionParentTransform);
        // set the position to be the difference from the last position
        var newPosition = LastSpawnedCenterWorldCoordinates + SectionSpacing;
        // set the new position
        copy.transform.position = newPosition;
        // update the last spawned center 
        LastSpawnedCenterWorldCoordinates = newPosition;

        // add this to the collection of instances
        SectionPrefabInstances.Enqueue(copy);
        SpawnedCounter++;
    }

    /// <summary>
    /// Gets the next section of the world to spawn based on the current difficulty
    /// </summary>
    /// <returns></returns>
    private GameObject GetPrefabSectionToSpawnNext()
    {
        // return the first easy section, since we are testing
        //TODO determine a prefab section based on the current difficulty
        return EasySections[0];
    }

}