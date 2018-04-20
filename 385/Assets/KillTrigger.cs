using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillTrigger : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// Adds kill trigger to player 
	void OnTriggerEnter2D (Collider2D other) {
		if (other.gameObject.tag == "Player") {
			Debug.Log ("Player had died");
			Destroy (other.gameObject);
		}
	}

}
