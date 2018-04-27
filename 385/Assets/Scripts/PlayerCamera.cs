using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Assets.Scripts;

public class PlayerCamera : MonoBehaviour {
    // player object camera is attached to
    public GameObject player;

    // offset of camera from player
    private Vector3 offset;

    private Vector3 initPos;

    // camera that follows player
    public Camera playerCam;

    // checks Camera1 to change camera settings
    public string CameraAxis = "Camera1";

    // used to check is camera button is pushed
    private AxisButton CameraButton;

    public bool trackPlayer = false;

    public float moveSpeed = 1;

    public float distance = 2, X;

    // gets offset for camera and starts camera in static view
    void Start () {
        float Y = distance / 2;
        Vector3 temp = new Vector3(-X, Y, distance);
        offset = transform.position - player.transform.position + temp;

        initPos = transform.position;

        // camera axis button created
        CameraButton = new AxisButton(CameraAxis, 0.5f);
    }
	
    // updates camera button and camera position
	void LateUpdate () {
        CameraButton.Update();

        // checks if camera button has been pressed
        if (CameraButton.IsButtonClicked())
        {
            trackPlayer = !trackPlayer;
        }

        if (trackPlayer)
        {
            float step = moveSpeed * Time.deltaTime;
            Vector3 temp = player.transform.position + offset;
            transform.position = Vector3.MoveTowards(transform.position,temp, step);

        }
        else
        {
            float step = moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, initPos, step);
        }
    }

}
