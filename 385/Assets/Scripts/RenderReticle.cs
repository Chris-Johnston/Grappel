using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderReticle : MonoBehaviour {

	// Reference to the Player object's position
	private Vector3 playerPos;

	// Reference to the reticle's position
	private Vector3 reticlePos;

	// LineRenderer which draws the aiming reticle
	private LineRenderer reticleLineRenderer;

	/* Two vectors: player position (0) and end point of reticle (1). These are used to 
	 * draw the reticle as a line. */
	private Vector3[] reticleRendererVectors;

	// Use this for initialization
	void Start () {
		// Initialize the player position
		playerPos = GameObject.Find("Player 1").GetComponent<Transform>().position;
		reticlePos = transform.position;

		// Initialize the reticle LineRenderer
		reticleLineRenderer = GetComponent<LineRenderer>();

		// Initialize the reticle renderer element array to hold two Vector3; player pos and endpoint
		reticleRendererVectors = new Vector3[2];
		reticleRendererVectors [0].x = playerPos.x;
		reticleRendererVectors [0].y = playerPos.y;
		reticleRendererVectors [1].x = reticlePos.x;
		reticleRendererVectors [1].y = reticlePos.y;
		Debug.Log ("player x,y: " + playerPos.x + ", " + playerPos.y);
		Debug.Log ("reticle x,y: " + reticlePos.x + ", " + reticlePos.y);
	}

	// Update is called once per frame
	void Update () {
		// Don't draw the reticle when the player is casting or attached to a grapple point
		if (GameObject.Find ("GrappleHook").GetComponent<FireGrappleHook> ().casting ||
			GameObject.Find ("Player 1").GetComponent<RopeSystem>().IsRopeConnected()) 
		{
			reticleLineRenderer.enabled = false;
			GetComponent<SpriteRenderer> ().enabled = false;
		}
		else
		{
			if (!reticleLineRenderer.enabled) 
			{
				reticleLineRenderer.enabled = true;
				GetComponent<SpriteRenderer> ().enabled = true;
			}
			reticlePos = transform.position;
			playerPos = GameObject.Find ("Player 1").GetComponent<Transform> ().position;
			reticleRendererVectors [0] = playerPos;
			reticleRendererVectors [1] = reticlePos;
			reticleLineRenderer.SetPositions (reticleRendererVectors);

			//Debug.Log ("player x,y: " + playerPos.x + ", " + playerPos.y);
			//Debug.Log ("reticle x,y: " + reticlePos.x + ", " + reticlePos.y);
		}
	}
}
