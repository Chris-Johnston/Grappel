using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script for controlling the behavior of a rope, which is connected to the player
/// and directly affects the physics that affect the player.
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
    /// Util method to determine if this rope system is connected to a Rigidbody2D
    /// </summary>
    /// <returns>True, if the anchor point of the rope is connected, or false if not.</returns>
    public bool IsRopeConnected()
    {
        return RopeAnchorPoint == null;
    }


}
