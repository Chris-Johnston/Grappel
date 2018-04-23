using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

public class PlayerCamera : MonoBehaviour {
    // player object camera is attached to
    public GameObject player;

    // offset of camera from player
    private Vector3 offset;

    // camera that follows player
    public Camera playerCam;

    // checks Camera1 to change camera settings
    public string CameraAxis = "Camera1";

    // used to check is camera button is pushed
    private AxisButton CameraButton;

    // gets offset for camera and starts camera in static view
    void Start () {
        offset = transform.position - player.transform.position;
        playerCam.enabled = false;

        // camera axis button created
        CameraButton = new AxisButton(CameraAxis, 0.5f);
    }
	
    // updates camera button and camera position
	void LateUpdate () {
        CameraButton.Update();
        transform.position = player.transform.position + offset;

        // checks if camera button has been pressed
        if (CameraButton.IsButtonClicked())
        {
            playerCam.enabled = !playerCam.enabled;
        }
    }

}
