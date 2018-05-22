using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugPrinter : MonoBehaviour
{
    /// <summary>
    /// Exposes Debug.Log so that it can be called in a UnityEvent
    /// </summary>
    /// <param name="message"></param>
    public void Log(string message)
    {
        Debug.Log(message);
    }
}
