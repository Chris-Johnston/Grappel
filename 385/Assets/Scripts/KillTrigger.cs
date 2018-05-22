using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillTrigger : MonoBehaviour
{
	/// <summary>
    /// Destroys the game objects with the tag "Player"
    /// when they collide with something that uses this behavior
    /// </summary>
    /// <param name="other"></param>
	void OnTriggerEnter2D (Collider2D other)
    {
		if (other.gameObject.tag == Tags.TAG_PLAYER) {
			Debug.Log ("Player had died");
		    Destroy (other.gameObject);
		}
	}
}
