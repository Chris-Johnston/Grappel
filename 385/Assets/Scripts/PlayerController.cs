using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

/// <summary>
/// Controller script for the player
/// </summary>
public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// How much deadzone in the joystick before it should be considered pointing in some direction
    /// </summary>
    [Range(0, 1f)]
    public float JoystickDeadzone = 0.19f;

	/// <summary>
	/// True if a player is using controller mode. Should be set by some option in the UI later on.
	/// </summary>
	public bool ControllerMode = false;

	/// <summary>
	/// Vector holding the joystick position. Unused if not in controller mode.
	/// </summary>
	private Vector3 joystickPosition;

    /// <summary>
    /// Which axis does the player use to strafe left and right on the ground, or adjust
    /// their velocity when swinging?
    /// </summary>
    public string StrafeAxis = "Strafe"; // use Strafe_P2 for Player 2

    /// <summary>
    /// Which axis to check that indicates that the player wanted to jump
    /// </summary>
    public string JumpAxis = "Jump"; // use Jump_P2 for Player 2

	/// <summary>
	/// Horizontal axis name for the xbox controller
	/// </summary>
	public string AimHorizontalAxis = "Horizontal";

	/// <summary>
	/// Vertical axis name for the xbox controller
	/// </summary>
	public string AimVerticalAxis = "Vertical";

    /// <summary>
    /// Which axis will fire the grapple hook
    /// </summary>
    public string FireAxis = "Fire"; // use Fire_P2 for Player2

    // util wrapper for this class
    // that helps determine if the button is pressed or clicked
    private AxisButton JumpButton;

    /// <summary>
    /// Util wrapper that allows for an axis to act as a button
    /// </summary>
    private AxisButton FireButton;

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
	/// The maximum length that the grappling hoook can travel
	/// </summary>
	[Range(0,8f)]
	public float HookFireDistance = 1f;

    /// <summary>
    /// The speed that the hook moves per second
    /// </summary>
    [Range(0, 4f)]
    public float HookFireSpeed = 1f;

    /// <summary>
    /// Toggle to show the debugging lines
    /// </summary>
    public bool ShowDebugging = true;

    /// <summary>
    /// Reference to the RopeSystem attribute of the Player.
    /// If there are more than one, could make another one (or an array).
    /// </summary>
    public RopeSystem RopeSystem;

    // ref to the player's rigidbody
    private Rigidbody2D PlayerRigidBody;

    //This works for back and forth
    /// <summary>
    /// How much force to give to the player when they strafe, per second
    /// </summary>
    [Range(0, 500)]
    public float StrafingForce = 150.0f;

    /// <summary>
    /// How much force to give to the player when they jump
    /// </summary>
    [Range(0, 50)]
    public float JumpingForce = 10.0f;

    // is the player colliding with the ground?
    private bool onGround;

	/// <summary>
	/// Holds value of transform.position for convenience
	/// </summary>
	private Vector3 playerPos;

	/// <summary>
	/// Reference to the Reticle game object (child of Player object)
	/// </summary>
	public GameObject AimingReticle;

    /// <summary>
    /// Reference to the object that is responsible for being thrown and hooking on if it collides
    /// with an anchor point
    /// </summary>
    public GameObject ThrowingHook;

    // Use this for initialization
    void Start ()
    {
        PlayerRigidBody = GetComponent<Rigidbody2D>();

        // default to being on the ground
        onGround = true;

        // set up the jump button axis
        JumpButton = new AxisButton(JumpAxis, 0.5f);
        // and the fire button axis
        FireButton = new AxisButton(FireAxis);

		// Initialize the player position
		playerPos = transform.position;

		// Initialize the joystick position Vector3
		joystickPosition = new Vector3(0, 0, 0);
    }

    //Ground check for player - can only jump while on ground
    void OnCollisionEnter2D(Collision2D collide)
    {
        //any game objects tagged as Ground will allow the player to jump
        if (collide.gameObject.tag == Tags.TAG_GROUND)
        {
            onGround = true;
        }
        else if (collide.gameObject.tag == Tags.TAG_GRAPPLE_HOOK)
        {
            // Debug.Log("Oh no this player was hit with a grapple hook!");
        }
    }

    void OnCollisionExit2D(Collision2D collide)
    {
        if (collide.gameObject.tag == Tags.TAG_GROUND)
        {
            onGround = false;
        }
    }

    // Update is called once per frame
    void Update ()
    {
        // update the axisbutton utils first
        JumpButton.Update();
        FireButton.Update();

		// Update the player position
		playerPos = transform.position;

		if (ControllerMode) 
		{
			joystickPosition.x = Input.GetAxis (AimHorizontalAxis);
			joystickPosition.y = Input.GetAxis (AimVerticalAxis);
		}

        // Check that the ref to RopeSystem is not null, and the rope system is connected
        // the == true is not redundant because of the ?. operator
        // if this were expanded for multiple ropes, would just need to check that any are connected
        if (RopeSystem?.IsRopeConnected() == true)
        {
            // check the strafe axis
            var strafeAmount = Input.GetAxis(StrafeAxis);

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
        // Player is not suspended by a rope
        else
        {
			/* ~~ DISPLAY AIMING RETICLE ~~ */

			//Debug.Log("joystickX " + joystickPosition.x + "  joystickY: " + joystickPosition.y);

			float aimAngle;

			if (!ControllerMode) 
			{
                // Get the mouse position, using a ray from the camera that hits a plane that the world is on ( Z = 0 )
                // see here: https://answers.unity.com/questions/566519/camerascreentoworldpoint-in-perspective.

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                // create a new plane at the origin that is facing forward
                Plane xy = new Plane(Vector3.forward, new Vector3(0, 0, 0));
                float distance;
                xy.Raycast(ray, out distance);
                var rayPoint = ray.GetPoint(distance);
                var playerAimingLocation = rayPoint - playerPos;

                Debug.DrawLine(Vector3.zero, rayPoint);

                // Angle (radians) from the horizontal line going through the player to the mouse position
                aimAngle = Mathf.Atan2 (playerAimingLocation.y, playerAimingLocation.x);
			} 
			else 
			{
				joystickPosition.x = Input.GetAxis (AimHorizontalAxis);
				joystickPosition.y = Input.GetAxis (AimVerticalAxis);
    
                // enable the reticle if the joystick is active
                AimingReticle.SetActive(!joystickIsDead());

				// Calculate the angle based on joystick position. Invert y axis first
				aimAngle = Mathf.Atan2 ((0 - joystickPosition.y), joystickPosition.x);
			}

			// Keep it positive (e.g., straight below the player is 3pi/2 rad, not -pi/2)
			if (aimAngle < 0) 
			{
				aimAngle = (Mathf.PI * 2) + aimAngle;
			}

            if (ShowDebugging)
            {
                Debug.Log ("aimAngle: " + aimAngle + " rad");
            }

            // Update the reticle endpoint and draw a line from the player to the endpoint
            UpdateReticlePosition (aimAngle);

			/* ~~ END DISPLAY AIMING RETICLE */


            //Left and Right strafing movement 
            float moveHorizontal = Input.GetAxis(StrafeAxis);

            if (ShowDebugging)
            {
                Debug.Log($"Axis: {StrafeAxis} Value: {moveHorizontal}");
            }

            Vector2 movement = new Vector2(moveHorizontal, 0);
            PlayerRigidBody.AddForce(movement * StrafingForce * Time.deltaTime);

            //Jump if the player is on the ground and they just clicked the button
            if (JumpButton.IsButtonClicked() && onGround)
            {
                PlayerRigidBody.velocity = new Vector2(PlayerRigidBody.velocity.x, JumpingForce);
                onGround = false;
            }
        }
	}


	/// <summary>
	/// Updates the reticle position.
	/// </summary>
	/// <param name="aimAngle">Aim angle.</param>
	private void UpdateReticlePosition(float aimAngle)
	{
		float reticleEndX = transform.position.x + HookFireDistance * Mathf.Cos (aimAngle);
		float reticleEndY = transform.position.y + HookFireDistance * Mathf.Sin (aimAngle);
		AimingReticle.transform.position = new Vector3 (reticleEndX, reticleEndY, 0);
	}

	/// <summary>
	/// Checks if the joystick is in the deadzone. Currently used to know when to not display the reticle
	/// </summary>
	private bool joystickIsDead()
	{
		float joystickX = Mathf.Abs(Input.GetAxis (AimHorizontalAxis));
		float joystickY = Mathf.Abs(Input.GetAxis (AimVerticalAxis));
		return (joystickX <= JoystickDeadzone && joystickY <= JoystickDeadzone);
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
