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
[ExecuteInEditMode]
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
    /// How much to offset where to draw the starting point of the player's
    /// end of the rope LineRenderable? This should end up looking like it's in
    /// the players hand, instead of emerging from their torso
    /// 
    /// The Z component should probably be 0
    /// </summary>
    public Vector3 PlayerRopeDrawOffset;

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
            // ensure that the line renderer is enabled when the rope is connected
            // and the distance joint
            RopeLineRenderer.enabled = true;
            RopeDistanceJoint.enabled = true;

            RopeDistanceJoint.connectedAnchor = RopeAnchorPoint.transform.position;

            // update the first point to be the same as the player origin w/ the offset
            //TODO: should later expand on the player offset to compensate for any rotation of the player, if that is planned to be used
            // could have the player rotate to match the swinging
            RopeLineRenderer.SetPosition(0, transform.position + PlayerRopeDrawOffset);

            // set the last point to be the same as the anchor point
            RopeLineRenderer.SetPosition(1, RopeAnchorPoint.position);
        }
        else
        {
            // when the rope isn't connected, just disable the renderer
            // and the distance joint
            RopeLineRenderer.enabled = false;
            RopeDistanceJoint.enabled = false;
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
        // update the rope renderable positions
        UpdateRopeRenderablePositions();
    }

}
