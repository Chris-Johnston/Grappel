using UnityEngine;

[ExecuteInEditMode]
public class EnableDepthTexture : MonoBehaviour
{
    private Camera camera;
	
	void Start()
    {
        // enable the depth texture mode for this camera
        camera = GetComponent<Camera>();
        camera.depthTextureMode = DepthTextureMode.Depth;
	}
}
