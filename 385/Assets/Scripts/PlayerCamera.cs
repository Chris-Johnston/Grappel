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

    // Static position of camera
    private Vector3 initPos;

    // camera that follows player
    public Camera playerCam;

    // checks Camera1 to change camera settings
    public string CameraAxis = "Camera1";

    // used to check is camera button is pushed
    private AxisButton CameraButton;

    public bool trackPlayer = false;

    public float moveSpeed = 1;

    public float distance = 2, X; // controls distance of camera and 
                                  // its X axis adjustment

    private bool camMoving = false;

    public RopeSystem RopeSystem;

    // gets offset for camera and starts camera in static view
    void Start () {
        float Y = distance / 2; // Y axis adjustment for changing the camera distance
        Vector3 temp = new Vector3(-X, Y, distance);  // stores adjustments
        offset = transform.position - player.transform.position + temp;

        initPos = transform.position;  // stores static camera position

        // camera axis button created
        CameraButton = new AxisButton(CameraAxis, 0.5f);
    }
	
    // updates camera button and camera position
	void LateUpdate () {
        CameraButton.Update();

        // checks if camera button has been pressed
        if (CameraButton.IsButtonClicked()) {
            camMoving = true;
            trackPlayer = !trackPlayer;
        }

        if(camMoving)  // if camera is still adjusting
        {
            // adjust move speed
            float step = moveSpeed * Time.deltaTime;

            
            if (trackPlayer){ // moving towards player

                if (RopeSystem.IsRopeConnected())
                {   // moving towards swing point
                    Vector3 camTemp = new Vector3(0, 0, -12);
                    Rigidbody2D ropeConnect = RopeSystem.getAnchor();
                    if (playerCam.transform.position !=
                        ropeConnect.transform.position + camTemp)
                    {
                        transform.position = Vector3.MoveTowards(transform.position,
                            ropeConnect.transform.position + camTemp, step);
                    }
                    else  // reached position
                    {
                        camMoving = false;
                    }

                }
                else  // moving towards player position
                {
                    // uses moveTowards to move camera
                    Vector3 temp = player.transform.position + offset;
                    if (playerCam.transform.position != temp)
                    {
                        transform.position = Vector3.MoveTowards(
                        transform.position, temp, step);
                    }
                    else  // reached position
                    {
                        camMoving = false;
                    }
                }
                
                    
             }
            else{   // moves towards init static position
                  if(playerCam.transform.position != initPos){
                    transform.position = Vector3.MoveTowards(
                        transform.position, initPos, step);
                   }
                else  // reached position
                {
                    camMoving = false;
                }
                    
            }
            
        }

        if (trackPlayer && !camMoving)
        {
            // adjust move speed
            float step = moveSpeed * Time.deltaTime;

            if (RopeSystem.IsRopeConnected())
            {
                camMoving = true;
            }
            else
            {
                transform.position = player.transform.position + offset;
            }

        }

    }

}
