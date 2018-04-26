using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Script attached to the Win and Kill area triggers
/// that executes some action when a player interacts with one of thems
/// </summary>
public class AreaTriggerController : MonoBehaviour
{
    /// <summary>
    /// List of UnityEvents to execute when the player is triggered by this
    /// collider
    /// 
    /// See this for reference:
    /// https://answers.unity.com/questions/1021048/unity-editor-inspector-delegate-function-pointer.html
    /// </summary>
    public UnityEvent events = new UnityEvent();

    /// <summary>
    /// Should this log to console when the trigger hits?
    /// </summary>
    public bool LogToConsole = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == Tags.TAG_PLAYER)
        {
            if (LogToConsole)
                Debug.Log("Collided with the player");

            // run the UnityEvent which can contain multiple calls
            events.Invoke();
        }
    }
}
