using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Controller class that has UnityEvents tied to actions associated
/// with grapple point actions
/// </summary>
public class GrapplePointController : MonoBehaviour
{
    /// <summary>
    /// Invoked when a grapple gets connected to this point
    /// (this could be used to play a sound effect, or start a timer which can cause this point to self destruct)
    /// </summary>
    public UnityEvent OnGrappleConnect;

    /// <summary>
    /// Invoked when a grapple gets disconnected from this point
    /// </summary>
    public UnityEvent OnGrappleDisconnect;
}
