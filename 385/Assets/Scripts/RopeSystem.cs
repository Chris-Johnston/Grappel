using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Script for controlling the behavior of a rope, which is connected to the player
/// and directly affects the physics that affect the player.
/// 
/// The ExecuteInEditModeAttribute allows the script update to run in the unity editor
/// so that the previews are accurate.
/// </summary>
public class RopeSystem : MonoBehaviour
{
    /// <summary>
    /// Toggle that determines if the rope system can fire out
    /// This is disabled if the controller is in the deadzone, if a controller is being used
    /// or when the grapple point is connected. This is set in PlayerController
    /// </summary>
    public bool CanFire = true;

    /// <summary>
    /// The anchor point that the player's rope should be connected to,
    /// null if unset.
    /// </summary>
    public Rigidbody2D RopeAnchorPoint;

    /// <summary>
    /// A reference to the last grapple point controller that a grapple was connected to
    /// when the grapple is released, the release events will be invoked
    /// null if unset
    /// </summary>
    private GrapplePointController LastAnchorPointController;

    /// <summary>
    /// The DistanceJoint2D that is used to simulate the rope physics.
    /// </summary>
    public DistanceJoint2D RopeDistanceJoint;

    /// <summary>
    /// The LineRenderer that is used to draw the representation of the rope.
    /// </summary>
    public LineRenderer RopeLineRenderer;
    
    /// <summary>
    /// Axis used for shooting the grapple hook. Set to either *_Mouse or *_Controller
    /// string in Start() based on user control preference (stored in GameControl object).
    /// </summary>
    private string FireAxis; // Fire_P2 for Player 2

	public string FireAxis_Mouse = "Fire";
	public string FireAxis_Controller = "Fire_P2";

    private AxisButton FireButton;

    /// <summary>
    /// Which axis is used to indicate that the player wants to climb or descend.
    /// Set to either *_Controller or *_Mouse public string based on user control setting
    /// (stored in GameControl object).
    /// </summary>
    private string ClimbDescendAxis; // ClimbDescend_P2 for Player 2

	public string ClimbDescendAxis_Mouse = "ClimbDescend";
	public string ClimbDescendAxis_Controller = "ClimbDescend_P2";

    /// <summary>
    /// How many units per second the player climbs or descends at
    /// </summary>
    [Range(0, 10f)]
    public float ClimbDescendSpeed = 4f;

    /// <summary>
    /// Angle of where the player is aiming
    /// </summary>
    public float AimAngle = 0f;

    /// <summary>
    /// The current distance that the rope is casting at
    /// or the current distance of the rope
    /// </summary>
    public float CurrentCastDistance = 0f;

    /// <summary>
    /// Is the player casting a rope towards a point
    /// </summary>
    public bool IsCasting = false;

    /// <summary>
    /// How much to reel in on the original cast
    /// </summary>
    [Range(0, 1)]
    public float CastInitialReelIn = 0.02f;

    /// <summary>
    /// Minimum casting distance
    /// </summary>
    [Range(0, 1)]
    public float MinCastDistance = 0.5f;

    /// <summary>
    /// Maximum casting distance
    /// </summary>
    [Range(0, 20)]
    public float MaxCastDistance = 10f;

    /// <summary>
    /// How far to cast per second
    /// </summary>
    [Range(0, 15)]
    public float CastingSpeed = 1.0f;

    /// <summary>
    /// How much to offset where to draw the starting point of the player's
    /// end of the rope LineRenderable? This should end up looking like it's in
    /// the players hand, instead of emerging from their torso
    /// 
    /// The Z component should probably be 0
    /// </summary>
    public Vector3 PlayerRopeDrawOffset;

    /// <summary>
    /// Collider that represents the end of the grapple hook and the grapple line
    /// Gets longer, translatesa and rotates around
    /// </summary>
    public CapsuleCollider2D RopeAndHookCollider;

    /// <summary>
    /// Shared reference to the player controller script
    /// </summary>
    public PlayerController PlayerControllerScript;

    /// <summary>
    /// Reference to the grapple hook object
    /// </summary>
    public GameObject HookSpriteObject;

    /// <summary>
    /// Invoked when the player fires thie Grappling hook
    /// </summary>
    public UnityEvent OnPlayerGrappleFire;

    /// <summary>
    /// Invoked when the players grappling hook hits an object
    /// </summary>
    public UnityEvent OnPlayerGrappleHit;

    /// <summary>
    /// Invoked when the player releases the grappling hook, even if it is connected or not
    /// </summary>
    public UnityEvent OnPlayerGrappleRelease;

    void Start()
    {
        // Set axis strings based on the user's controller selection from the menu screen
        if (GameControl.ControllerMode) 
        {
          FireAxis = FireAxis_Controller;
          ClimbDescendAxis = ClimbDescendAxis_Controller;
        } 
        else 
        {
          FireAxis = FireAxis_Mouse;
          ClimbDescendAxis = ClimbDescendAxis_Mouse;
        }

        // set up the fire button axis
        FireButton = new AxisButton(FireAxis);
    }

    /// <summary>
    /// Util method to determine if this rope system is connected to a Rigidbody2D
    /// If the point is not null, then it means that a connection is made
    /// if the point is not null but inactive, then it means a connection was made but it has been disabled
    /// </summary>
    /// <returns>True, if the anchor point of the rope is connected, or false if not.</returns>
    public bool IsRopeConnected()
    {
        if (RopeAnchorPoint != null)
        {
            return RopeAnchorPoint?.gameObject?.activeSelf == true;
        }
        return false;
    }

    /// <summary>
    /// Updates all of the positions of the rope LineRenderable
    /// so that it is drawn accurately
    /// </summary>
    private void UpdateRopeRenderablePositions()
    {
        if (IsRopeConnected())
        {
            HookSpriteObject.SetActive(false);

            // if connected and release, then let go
            if (!FireButton.IsButtonHeld())
                Detach();
            else
            {
                // update the renderers, objects and colliders for when the rope is connected
                UpdateConnectedRope();   
            }
        }
        else
        {
            // the rope is not connected

            // not casting, if they hold down the button then cast
            if (FireButton.IsButtonHeld() && CanFire)
            {
                // update the stuff for a rope that is being cast
                UpdateCastingRope();
            }
            // reset when let go
            else
            {
                // reset to the disconnected state
                ResetRopeAndHookOnRelease();
            }
        }        
    }

    /// <summary>
    /// Update a rope that is being cast
    /// </summary>
    private void UpdateCastingRope()
    {
        // disable the distance joint when we are casting, for when the point 
        // disables itself
        RopeDistanceJoint.enabled = false;

        //HookCollider.enabled = true;
        RopeAndHookCollider.enabled = true;
        HookSpriteObject.SetActive(true);

        // start throwing if not throwing already
        if(!IsCasting)
        {
            // fireSound.Play();
            IsCasting = true;
            // reset the casting distance
            CurrentCastDistance = 0;
        }
        else
        {
            // increment the casting distance
            CurrentCastDistance += CastingSpeed * Time.deltaTime;

            // ensure it fits in the upper bound
            if (CurrentCastDistance > MaxCastDistance)
                CurrentCastDistance = MaxCastDistance;
        }

        // update the position of the line renderer
        // and the collider

        RopeLineRenderer.enabled = true;

        var directionVector = new Vector3(Mathf.Cos(AimAngle), Mathf.Sin(AimAngle), 0);
        var displayOrigin = transform.position + PlayerRopeDrawOffset;
        var displayOffset = new Vector3(CurrentCastDistance * directionVector.x, CurrentCastDistance * directionVector.y, 0);

        RopeLineRenderer.SetPosition(0, displayOrigin);
        RopeLineRenderer.SetPosition(1, transform.position + displayOffset);

        // get the midpoint
        var midPoint = Vector3.Lerp(Vector3.zero, displayOffset, 0.5f);

        var offset = new Vector2(midPoint.x, midPoint.y).magnitude;

        // set the center of the collider to the midpoint between the end of the cast
        RopeAndHookCollider.offset = new Vector2(0, CurrentCastDistance / 2);
        // size the collider to fit the cast
        RopeAndHookCollider.size = new Vector2(0.2f, CurrentCastDistance);
        // set the end of the sprite to the end of the grapple
        HookSpriteObject.transform.localPosition = CurrentCastDistance * directionVector;
        // rotate the sprite so that it looks correct
        HookSpriteObject.transform.rotation = Quaternion.Euler(0, 0, -90 + (Mathf.Rad2Deg * AimAngle));

        // rotate the object that contains the casting collider
        transform.rotation = Quaternion.Euler(0, 0, -90 + (Mathf.Rad2Deg * AimAngle));

        // if they just clicked the fire button
        if (FireButton.IsButtonClicked())
        {
            // then invoke the on fire handler
            OnPlayerGrappleFire.Invoke();
        }
    }

    /// <summary>
    /// Update renderers and joints when a rope is connected
    /// </summary>
    private void UpdateConnectedRope()
    {
        // ensure that the line renderer is enabled when the rope is connected
        // and the distance joint
        RopeLineRenderer.enabled = true;
        RopeDistanceJoint.enabled = true;

        RopeDistanceJoint.connectedAnchor = RopeAnchorPoint.transform.position;

        if (!HasDoneInitialReelIn)
        {
            RopeDistanceJoint.distance -= CastInitialReelIn;
            if (RopeDistanceJoint.distance < MinCastDistance)
                RopeDistanceJoint.distance = MinCastDistance;

            HasDoneInitialReelIn = true;
        }

        // reel in the player
        var ropeDistance = RopeDistanceJoint.distance;
        // adjust the target distance based on the ClimbDescend Axis
        ropeDistance += Input.GetAxis(ClimbDescendAxis) * ClimbDescendSpeed * Time.deltaTime;

        // ensure ropeDistance is in bounds
        if (ropeDistance > MaxCastDistance)
            ropeDistance = MaxCastDistance;
        else if (ropeDistance < MinCastDistance)
            ropeDistance = MinCastDistance;

        // set the new rope distance
        RopeDistanceJoint.distance = ropeDistance;

        // update the first point to be the same as the player origin w/ the offset
        // TODO: should later expand on the player offset to compensate for any rotation of the player, if that is planned to be used
        // could have the player rotate to match the swinging
        RopeLineRenderer.SetPosition(0, transform.position + PlayerRopeDrawOffset);

        // set the last point to be the same as the anchor point
        RopeLineRenderer.SetPosition(1, RopeAnchorPoint.position);
    }

    /// <summary>
    /// Resets the state of the hook sprite, the casting distance, the colliders, and the line renderers
    /// to the state that it should be when nothing is connected
    /// </summary>
    private void ResetRopeAndHookOnRelease()
    {
        RopeDistanceJoint.enabled = false;
        HookSpriteObject.SetActive(false);
        CurrentCastDistance = 0;
        IsCasting = false;
        RopeAndHookCollider.enabled = false;
        RopeLineRenderer.enabled = false;
    }

    /// <summary>
    /// Get the axis that is perpendicular to the rope from the player. This should default to pointing to the right when the player
    /// hangs directly downward from the point they are hanging from.
    /// </summary>
    /// <returns>A Vector3 that represents the direction perpendicular to the rope, or null if not connected.</returns>
    public Vector2? GetRopePerpendicularAxis()
    {
        // return null if the rope is not connected
        if (!IsRopeConnected()) return null;

        // get the vector value from the player position to the anchor point
        var playerToAnchor = new Vector2(transform.position.x, transform.position.y) - RopeAnchorPoint.position;

        return GetVector2Normal(playerToAnchor);
    }

    /// <summary>
    /// Get the perpendicular value for a Vector2
    /// </summary>
    /// <returns></returns>
    private Vector2 GetVector2Normal(Vector2 vec)
    {
        // get the perpendicular value
        var val = new Vector2(-vec.y, vec.x);
        // then normalize it
        val.Normalize();
        return val;
    }
    
    /// <summary>
    /// Update method
    /// </summary>
    private void Update()
    {
        FireButton.Update();
        // update the rope renderable positions
        UpdateRopeRenderablePositions();
    }

    /// <summary>
    /// Detaches from the anchor point
    /// </summary>
    public void Detach()
    {
        RopeAnchorPoint = null;
        // invoke the OnGrappleDisconnect events if the reference was not null
        LastAnchorPointController?.OnGrappleDisconnect.Invoke();
        // then null out the reference to the controller
        LastAnchorPointController = null;
        RopeLineRenderer.enabled = false;
        RopeDistanceJoint.enabled = false;
        RopeAndHookCollider.enabled = false;
        IsCasting = false;
        CurrentCastDistance = 0;
        // invoke the handler for the grapple point being released, unsure if it will block or not, so do this last
        OnPlayerGrappleRelease.Invoke();
    }

    private bool HasDoneInitialReelIn = false;

    /// <summary>
    /// attaches to the given anchor point
    /// </summary>
    /// <param name="point">the rigid body of the point that is being attached to</param>
    /// <param name="pointController">The GrapplePointController whose methods will be invoked</param>
    public void Attach(Rigidbody2D point, GrapplePointController pointController)
    {
        RopeAnchorPoint = point;
        LastAnchorPointController = pointController;
        // invoke the OnGrappleConnect events if a connection is made
        LastAnchorPointController?.OnGrappleConnect.Invoke();

        // use the min cast distance, or the current - the reel in
        // only if the player is on the ground

        if(PlayerControllerScript.IsOnGround)
        {
            HasDoneInitialReelIn = false;
        }

        RopeAndHookCollider.enabled = false;
    }

    /// <summary>
    /// OnTriggerEnter for the capsule collider that is used to determine the rope collision
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // if is casting and collided with a grapple point
        if (IsCasting && collision.CompareTag(Tags.TAG_GRAPPLE_POINT))
        {
            IsCasting = false;
            Attach(collision.GetComponent<Rigidbody2D>(), collision.GetComponent<GrapplePointController>());
            // invoke the grapple hit
            OnPlayerGrappleHit.Invoke();
        }
    }

    /// <summary>
    /// Sets the angle that the grapple is pointing at or aiming at
    /// Unused when the grapple is connected
    /// </summary>
    /// <param name="angle"></param>
    public void SetAngle(float angle)
    {
        AimAngle = angle;
    }
}
