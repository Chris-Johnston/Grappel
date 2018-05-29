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
    /// At what point does the range of easy sections end
    /// and the medium sections begin
    /// </summary>
    [Range(0, 150)]
    public float EasyHeightEnd = 50f;

    /// <summary>
    /// At what point does the range of medium sections end
    /// and the hard sections begin
    /// </summary>
    [Range(50, 350)]
    public float MediumHeightEnd = 150f;

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
    /// the point height that the player is at
    /// highest Y
    /// </summary>
    private float PointHeight = 0f;

    /// <summary>
    /// Reference the water object
    /// </summary>
    public GameObject Water;

    /// <summary>
    /// How quick the water should move upwards
    /// </summary>
    [Range(0, 20)]
    public float WaterMovementSpeed = 0.7f;

    /// <summary>
    /// the height at which the water movement speed will start increasing
    /// </summary>
    [Range(0, 150)]
    public float WaterMovementMultiplierHeight = 80.0f;

    /// <summary>
    /// The target position for the water to move towards
    /// </summary>
    public Vector3 WaterTargetPosition = new Vector3();

    /// <summary>
    /// the max delta that water should move towards
    /// this is so that it appears to move smoothly
    /// </summary>
    // [Range(0, 10)]
    public float MaxDeltaWaterMovementSpeed = 500.0f;

    /// <summary>
    /// If the player has collided with the water
    /// </summary>
    public bool IsPlayerDead = false;

    void Start()
    {
        // set the reference to the player's rope system
        PlayerRopeSystemReference = PlayerReference.GetComponentInChildren<RopeSystem>();

        // programatically add a listener to on player grapple hit
        PlayerRopeSystemReference.OnPlayerGrappleHit.AddListener(new UnityEngine.Events.UnityAction(OnPlayerGrappleHit));

        // set the camera controller component
        CameraController = SceneCamera.GetComponent<DynamicSwingUpCameraController>();

        // add the area trigger handler for the water
        var areaTrigger = Water.GetComponent<AreaTriggerController>();
        areaTrigger.events.AddListener(new UnityEngine.Events.UnityAction(OnWaterCollide));

        // spawn a few items as soon as the scene starts
        for (int i = 0; i < InitialSpawnAmount; i++)
        {
            //SpawnSection();
        }
    }

    void Update()
    {
        CheckIfPlayerFarAhead();
        // only update the water if the player has started the game already
        // and they can do that by grabbing on to the first point
        if (IsStarted)
        {
            UpdateWater();
        }
        CheckIfNewSpawnsNeeded();
    }

    /// <summary>
    /// How many sections ahead should we spawn new sections so that the player doesn't risk 
    /// seeing nothing
    /// </summary>
    private int SectionSpacingThreshold = 2;

    /// <summary>
    /// Checks to see if the player is getting too close to the center point of the last spawned section
    /// and if so, should spawn a new section.
    /// 
    /// If the list of sections gets to be over the limit, then will start deleting the old ones
    /// so that Unity doesn't explode
    /// </summary>
    private void CheckIfNewSpawnsNeeded()
    {
        var playerY = PlayerReference.transform.position.y;

        // if the difference between the last spawned section and the players distance 
        // is less than the spacing between two of the sections, then it is likely 
        // that we should spawn a new one
        if ((LastSpawnedCenterWorldCoordinates.y - playerY) < (SectionSpacing.y * SectionSpacingThreshold))
        {
            SpawnSection();
        }
    }

    /// <summary>
    /// How far above the target camera position will we consider the player to be too far ahead and we need to push the camera forward?
    /// </summary>
    [Range(0, 10)]
    public float PlayerAheadDistance = 5f;

    /// <summary>
    /// When the water further than this distance away from the player, it will catch up to be this distance
    /// away from the target camera position
    /// </summary>
    [Range(0, 20)]
    public float WaterBehindDistance = 5f;

    /// <summary>
    /// Checks if the player is moving further ahead than the camera was ready for
    /// in that case, update the camera target position and give the water a boost
    /// </summary>
    private void CheckIfPlayerFarAhead()
    {
        var playerHeight = PlayerReference.transform.position.y;

        // if the player is above the target position plus this distance
        if (playerHeight > (CameraController.TargetCameraPosition.y + PlayerAheadDistance))
        {
            // the player is considered to be too far ahead
            // update the y component of the target camera position
            // to match that of the player
            CameraController.TargetCameraPosition.y = playerHeight;
        }
    }

    /// <summary>
    /// Updates the position of the water
    /// </summary>
    private void UpdateWater()
    {
        // update the target position
        WaterTargetPosition += Vector3.up * Time.deltaTime * (WaterMovementSpeed + (PointHeight / WaterMovementMultiplierHeight));

        // check if the player is really far ahead
        var playerHeight = PlayerReference.transform.position.y;
        
        Debug.Log($"Player {playerHeight} > Water {(WaterTargetPosition.y + WaterBehindDistance)}");

        if (playerHeight > (WaterTargetPosition.y + WaterBehindDistance))
        {
            Debug.Log("Water is catching up to the player");
            // set the new position so that it can catch up
            // base this off of the camera
            WaterTargetPosition = new Vector3(0, CameraController.TargetCameraPosition.y - WaterBehindDistance, 0);
        }

        // move the water towards the position
        Water.transform.position = Vector3.MoveTowards(Water.transform.position, WaterTargetPosition, MaxDeltaWaterMovementSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Called when the player makes contact with a grapple point
    /// This should start the game if it isn't started already
    /// and also should be responsible for telling the camera to start moving to the target position
    /// </summary>
    private void OnPlayerGrappleHit()
    {
        Debug.Log("AA");

        // start the level if it isn't already
        if (!IsStarted)
        {
            LevelStart();
        }

        var connectedPoint = GetConnectedGrapplePoint();

        // if the connected point height is above the current height
        if (connectedPoint.y >= PointHeight)
            PointHeight = connectedPoint.y;

        // set the camera position to focus on the point that was just grabbed
        CameraController.TargetCameraPosition = new Vector3(0, PointHeight, 0);
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

        // if too many items exist, delete some old ones
        if (SectionPrefabInstances.Count > MaxSectionCount)
        {
            // remove one of the instances
            // disable it
            // and destroy it
            var item = SectionPrefabInstances.Dequeue();
            item.SetActive(false);
            Destroy(item);
        }
    }

    /// <summary>
    /// The max number of sections to have instantiated at once
    /// </summary>
    [Range(3, 30)]
    public int MaxSectionCount = 10;

    /// <summary>
    /// Gets the next section of the world to spawn based on the current difficulty
    /// </summary>
    /// <returns></returns>
    private GameObject GetPrefabSectionToSpawnNext()
    {
        if (PointHeight < EasyHeightEnd)
        {
            return GetRandom(EasySections);
        }
        else if (PointHeight < MediumHeightEnd)
        {
            return GetRandom(MediumSections);
        }
        else
        {
            return GetRandom(HardSections);
        }
    }

    /// <summary>
    /// Gets a random element out of the list of game objects
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    private GameObject GetRandom(List<GameObject> list)
    {
        return list[Random.Range(0, list.Count)];
    }

    /// <summary>
    /// When the player collides with the water
    /// </summary>
    public void OnWaterCollide()
    {
        PlayerDead();
    }

    public void PlayerDead()
    {
        if (!IsPlayerDead)
            Debug.Log("Player died!");

        IsPlayerDead = true;
    }
}