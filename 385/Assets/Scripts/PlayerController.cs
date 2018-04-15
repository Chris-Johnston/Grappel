using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controller script for the player
/// </summary>
public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// The maximum velocity that a player can swing at
    /// </summary>
    [Range(0, 1000.0f)]
    public float MaxSwingingVelocity = 500f;

    /// <summary>
    /// How much force to add in the direction of motion
    /// to the player each second to swing the player
    /// </summary>
    [Range(0, 500.0f)]
    public float SwingForce = 200f;

    /// <summary>
    /// Toggle to show the debugging lines
    /// </summary>
    public bool ShowDebugging = true;

    // constants for the inputs to check
    // the strafe axis is defined negative going to the left and positive going
    // to the right. Currently A goes left, D goes right
    public const string STRAFE = "Strafe";

    /// <summary>
    /// Reference to the RopeSystem attribute of the Player.
    /// If there are more than one, could make another one (or an array).
    /// </summary>
    public RopeSystem RopeSystem;

    // ref to the player's rigidbody
    private Rigidbody2D PlayerRigidBody;

	// Use this for initialization
	void Start ()
    {
        PlayerRigidBody = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        // Check that the ref to RopeSystem is not null, and the rope system is connected
        // the == true is not redundant because of the ?. operator
        if (RopeSystem?.IsRopeConnected() == true)
        {
            // check the strafe axis
            var strafeAmount = Input.GetAxis(STRAFE);

            // get the perpendicular axis to the rope from the player
            var perpendicularAxis = RopeSystem.GetRopePerpendicularAxis().Value;

            // the direction of the perpendicular axis and where the player wants to go
            var directionForceAxis = strafeAmount * perpendicularAxis;

            // show some debug lines if debugging enabled
            if (ShowDebugging)
            {
                //Debug.DrawRay(transform.position, directionForceAxis, Color.red);
            }

            // get the velocity of the player in this perpendicular axis (and in the direction they want to go)
            // if the player already has a large amount of velocity in this axis
            // then don't let them get more velocity in this axis
            var velocityInDirection = Vector2Project(PlayerRigidBody.velocity, directionForceAxis);

            // show some debug lines if debugging enabled
            if (ShowDebugging)
            {
                //Debug.DrawRay(transform.position, velocityInDirection, Color.blue);
            }

            // check the velocity in the direction of motion to see if it is 
            // not too large to apply some force to it
            // compare by magnitude
            if (velocityInDirection.magnitude < (MaxSwingingVelocity * directionForceAxis).magnitude)
            {
                // if the velocity was less than the max, then we can add some velocity in that direction
                // on this frame
                var toAdd = directionForceAxis * SwingForce * Time.deltaTime;

                PlayerRigidBody.AddForce(toAdd);

                if (ShowDebugging)
                {
                    // Debug.Log(velocityInDirection.magnitude);
                    Debug.DrawRay(transform.position, toAdd, Color.green);
                }
            }
            else
            {
                // Debug.Log("low " + velocityInDirection.magnitude);
            }

        }
	}

    /// <summary>
    /// Projects vec on the direction normal
    /// <see cref="Vector3.Project(Vector3, Vector3)"/>
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    private Vector2 Vector2Project(Vector2 vec, Vector2 normal)
    {
        // convert both of these vector2s to vector3s
        // then project them using Vector3 project
        return Vector3.Project(ToVector3(vec), ToVector3(normal));
    }

    private Vector3 ToVector3(Vector2 v)
        => new Vector3(v.x, v.y, 0);
}
