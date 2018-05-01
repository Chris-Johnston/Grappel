using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// basically the same as AreaTriggerCollider
/// but has behavior on trigger exit
/// </summary>
public class WaterWarnController : MonoBehaviour
{
    public const string CollideTag = Tags.TAG_PLAYER;

    // ref to this object's renderer
    private MeshRenderer meshRenderer;

	void Start ()
    {
        meshRenderer = GetComponent<MeshRenderer>();
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(CollideTag))
        {
            meshRenderer.enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(CollideTag))
        {
            meshRenderer.enabled = false;
        }
    }
}
