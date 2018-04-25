using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    /// The DistanceJoint2D that is used to simulate the rope physics.
    /// </summary>
    public DistanceJoint2D RopeDistanceJoint;

    /// <summary>
    /// The LineRenderer that is used to draw the representation of the rope.
    /// </summary>
    public LineRenderer RopeLineRenderer;
    
    /// <summary>
    /// Which axis to use to indicate that the player wants to fire
    /// </summary>
    public string FireAxis = "Fire"; // Fire_P2 for Player 2
    private AxisButton FireButton;

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
    /// Collider for the end of the grapple hook
    /// </summary>
    //public CircleCollider2D HookCollider;

    /// <summary>
    /// A very thin collider for the rope itself
    /// </summary>
    //public PolygonCollider2D RopeCollider;

    public CapsuleCollider2D RopeAndHookCollider;

    private Transform RopeSystemTransform;

    public PlayerController PlayerControllerScript;

    void Start()
    {
        FireButton = new AxisButton(FireAxis);

        RopeSystemTransform = GetComponent<Transform>();
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


                // update the first point to be the same as the player origin w/ the offset
                //TODO: should later expand on the player offset to compensate for any rotation of the player, if that is planned to be used
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

                // start throwing if not throwing already
                if(!IsCasting)
                {
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

                Debug.Log("" + CurrentCastDistance + " " + displayOffset.magnitude);

                // update the position of the collider
                //CastingCollider.size.Set(0.1f, CurrentCastDistance);

                // this isn't working correctly, todo
                
                // get the midpoint
                var midPoint = Vector3.Lerp(Vector3.zero, displayOffset, 0.5f);

                var offset = new Vector2(midPoint.x, midPoint.y).magnitude;

                //CastingCollider.transform.localPosition = new Vector2(midPoint.x, midPoint.y);
                //CastingCollider.offset = new Vector2(0, -midPoint.magnitude + midPoint.magnitude / 2 + 0.3f);
                //CastingCollider.size = new Vector2(0.2f, midPoint.magnitude / 2 + 0.6f);
                //HookCollider.offset = CurrentCastDistance * directionVector;
                //RopeAndHookCollider.offset = (CurrentCastDistance / 2) * directionVector;
                RopeAndHookCollider.offset = new Vector2(0, CurrentCastDistance / 2);
                RopeAndHookCollider.size = new Vector2(0.2f, CurrentCastDistance);

                // rotate the object that contains the casting collider
                RopeSystemTransform.rotation = Quaternion.Euler(0, 0, -90 + Mathf.Rad2Deg * AimAngle);
            }
            // reset when let go
            else
            {
                CurrentCastDistance = 0;
                IsCasting = false;
                //CastingCollider.enabled = false;
                //HookCollider.enabled = false;
                RopeAndHookCollider.enabled = false;
                RopeLineRenderer.enabled = false;   
            }
        }        
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
        // Debug.Log("Detach");
        RopeAnchorPoint = null;
        RopeLineRenderer.enabled = false;
        RopeDistanceJoint.enabled = false;
        //CastingCollider.enabled = false;
        //HookCollider.enabled = false;
        RopeAndHookCollider.enabled = false;
        IsCasting = false;
        CurrentCastDistance = 0;
    }

    private bool HasDoneInitialReelIn = false;

    /// <summary>
    /// attaches to the given anchor point
    /// </summary>
    /// <param name="point"></param>
    public void Attach(Rigidbody2D point)
    {
        RopeAnchorPoint = point;
       
        

        // use the min cast distance, or the current - the reel in
        // only if the player is on the ground

        if(PlayerControllerScript.IsOnGround)
        {
            HasDoneInitialReelIn = false;
        }
        
        
        //CastingCollider.enabled = false;
        //HookCollider.enabled = false;
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
            Attach(collision.GetComponent<Rigidbody2D>());
        }
    }

    public void SetAngle(float angle)
    {
        AimAngle = angle;
    }

}
