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
    /// The anchor point that the player's rope should be connected to,
    /// null if unset.
    /// </summary>
    public Rigidbody2D RopeAnchorPoint;

    /// <summary>
    /// Which radius to give the player so that they don't over extend
    /// this should be very close to the size of the player, if not the same
    /// </summary>
    [Range(0.1f, 2.0f)]
    public float PlayerAntiOverExtendRadius = 0.6f;

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
    /// </summary>
    /// <returns>True, if the anchor point of the rope is connected, or false if not.</returns>
    public bool IsRopeConnected()
    {
		return (RopeAnchorPoint != null);
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

                var deltaDistance = Input.GetAxis(ClimbDescendAxis) * ClimbDescendSpeed * Time.deltaTime;

                // reel in the player
                var ropeDistance = RopeDistanceJoint.distance;
                // adjust the target distance based on the ClimbDescend Axis
                ropeDistance += deltaDistance;

                // ensure ropeDistance is in bounds
                if (ropeDistance > MaxCastDistance)
                    ropeDistance = MaxCastDistance;
                else if (ropeDistance < MinCastDistance)
                    ropeDistance = MinCastDistance;

                UpdateRopeDistance(ropeDistance, deltaDistance);

                // update the first point to be the same as the player origin w/ the offset
                // TODO: should later expand on the player offset to compensate for any rotation of the player, if that is planned to be used
                // could have the player rotate to match the swinging
                RopeLineRenderer.SetPosition(0, transform.position + PlayerRopeDrawOffset);

                // set the last point to be the same as the anchor point
                RopeLineRenderer.SetPosition(1, RopeAnchorPoint.position);
            }
        }
        else
        {
            // the rope is not connected

            // not casting, if they hold down the button then cast
            if (FireButton.IsButtonHeld())
            {    
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
            // reset when let go
            else
            {
                HookSpriteObject.SetActive(false);
                CurrentCastDistance = 0;
                IsCasting = false;
                RopeAndHookCollider.enabled = false;
                RopeLineRenderer.enabled = false;   
            }
        }        
    }

    /// <summary>
    /// Updates the distance of the rope
    /// </summary>
    private void UpdateRopeDistance(float ropeDistance, float deltaDistance)
    {
        // this is the vector that points in the direction from the anchor point to the center of the player
        // so it should be the same angle as the rope
        // and with the magnitude of the ropeDistance

        // see https://docs.unity3d.com/Manual/DirectionDistanceFromOneObjectToAnother.html
        var direction = new Vector2(transform.position.x, transform.position.y) - RopeAnchorPoint.position;
        // normalize it
        direction.Normalize();

        // ensure that the rope will not extend into another object and push the player into it
        if (!Mathf.Approximately(0f, deltaDistance) && CheckForRopeExtensionIntoObject(transform.position, direction, deltaDistance))
        {
            // set the new rope distance
            RopeDistanceJoint.distance = ropeDistance;
        }
    }

    /// <summary>
    /// Draw a ray between the desired rope length and the current rope length to ensure that it doesn't overextend into an object and cause
    /// unintended behavior, like it getting stuck inside an object or out of bounds
    /// </summary>
    /// <returns>False if an object was detected in the way of the ray that would get the way, or true if there is nothing in the way</returns>
    private bool CheckForRopeExtensionIntoObject(Vector2 PlayerCenterPosition, Vector2 DirectionVector, float distance)
    {
        var offsetDistance = (distance < 0) ? distance - PlayerAntiOverExtendRadius : distance + PlayerAntiOverExtendRadius;
        
        // debugging
        // draws a ray in the direction that is being checked for over extensions
        // only visible in the scene view, so fine to leave this in 
        Debug.DrawRay(PlayerCenterPosition, DirectionVector * offsetDistance, Color.red);
        
        // raycast from the player's origin to the direction that the rope will be extended in
        
        // cap the amount of hits from the raycast to 2
        // the first one will always be the player, because the raycast starts in the center of the player collider
        var results = new RaycastHit2D[2];

        // raycast from the center of the player in the direction of the rope to the intended length
        // get the results from this raycast
        var raycastHit = Physics2D.Raycast(PlayerCenterPosition, DirectionVector, new ContactFilter2D(), results, offsetDistance);

        // if hit more than one object, then there is something in the way
        if (raycastHit > 1f)
        {
            // check to see if this object is part of the world

            // get the tags for this last hit
            var lastHit = results[1];
            
            // check that we collided with any of the world tags
            if (lastHit.collider.CompareTag(Tags.TAG_FLOOR_WALL) ||
                lastHit.collider.CompareTag(Tags.TAG_GROUND) ||
                lastHit.collider.CompareTag(Tags.TAG_GROUND_DIS))
            {
                // if we did, then prevent extending into these
                return false;
            }

            // if the tag was something else, then dont worry about it
        }
        
        // if didn't collide with anything else, then no problem, go ahead and update the rope length
        return true;
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
