using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireGrappleHook : MonoBehaviour {

	/* Flag to indicate if currently "casting" the hook.
	 * True on click, reset to false after collision/miss */
	private bool casting;

	// Represents the world location of a click/cast
	private Vector2 endPoint;

	// Time taken for the object to travel from start to endPoint
	public float castDuration = 15.0f;

	// Vertical position of the object
	private float yAxis;

	// Use this for initialization
	void Start () 
	{
		yAxis = gameObject.transform.position.y;
	}
	

	void FixedUpdate () 
	{
		if (Input.GetMouseButtonDown(0) && !casting) 
		{
			RaycastHit hit;

			// Create a ray on the clicked position
			Ray ray;
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			casting = true;
			endPoint = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			//Debug.Log (endPoint);
		}

		// Check if the "casting" flag is true and if the object position has not reached endPoint
		if (casting && !Mathf.Approximately (gameObject.transform.position.magnitude, endPoint.magnitude)) {
			
			/* Move the object to the desired position. Vector2.Lerp moves the object from the source location
			 * to the destination location. castDuration is multiplied by the distance to keep constant speed
			 * independent of the distance travelled. */
			gameObject.transform.position = Vector2.Lerp (gameObject.transform.position, endPoint, 
				1 / (castDuration * (Vector2.Distance (gameObject.transform.position, endPoint))));
		} 
		// Else, if the object arrived at endPoint...
		else if (casting && Mathf.Approximately (gameObject.transform.position.magnitude, endPoint.magnitude)) 
		{
			casting = false;
			Debug.Log ("Game object arrived at clicked point.");
		}

		// If not casting, the hook should be with the player
		if (!casting) 
		{
			ResetToPlayerLocation ();
		}
	}

	// Check for a collision; if it's a GrapplePoint, log for now (later this is where swinging logic will
	// be initiated).
	void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.collider.tag == "Player") 
		{
			Physics2D.IgnoreCollision (gameObject.GetComponent<Collider2D>(), collision.collider);
			return;
		}
		casting = false;
		if (collision.collider.tag == "GrapplePoint") 
		{
			Debug.Log ("GrappleHook collided with GrapplePoint: " + collision.collider.name);
			/* Player attaches to the point and starts swinging. Here we'll probably need to keep "casting" as true 
			 * until the player lets go of the rope. Might need to return in this if block to prevent the hook from 
			 * being reset to the Player coordinates (shouldn't happen until they let go of the swinging rope) */
		}

		// Shouldn't be called if the player hits a GrapplePoint.
		ResetToPlayerLocation ();
	}

	// Sets the location of the grappling hook to the player location. Use after collision or whiffed hook
	void ResetToPlayerLocation ()
	{
		// Grappling hook should go back to location of player object. Difference between Player's and
		// GrappleHook's x and y coordinates should be subtracted from GrappleHook's coordinates to achieve this.
		float resetX = gameObject.transform.parent.transform.position.x - gameObject.transform.position.x;
		float resetY = gameObject.transform.parent.transform.position.y - gameObject.transform.position.y;

		// Construt a new Vector3 with the correct offset values to be added to the current GrappleHook coordinates.
		Vector3 reset = new Vector3 (resetX, resetY, 0);
		gameObject.transform.position += reset;

		/* Clear the TrailRenderer which showed the path of the hook. When combined with latching/swinging
		 * mechanic, we'll want to replace this with the rope texture. Maybe there's a better way to do this? */
		gameObject.GetComponent<TrailRenderer> ().Clear ();
	}
}
