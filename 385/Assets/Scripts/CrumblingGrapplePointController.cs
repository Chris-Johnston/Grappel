using System.Collections;
using UnityEngine;

public class CrumblingGrapplePointController : MonoBehaviour
{
    /// <summary>
    /// How long in seconds until the point should become disabled
    /// </summary>
    [Range(0f, 15f)]
    public float TimeUntilDisappear = 3f;

    /// <summary>
    /// Method that should be called when the grapple is first connected to this point
    /// this starts the timer that causes the point to be disabled
    /// 
    /// This should be started by a UnityEvent
    /// </summary>
    public void Connect()
    {
        // start the coroutine
        StartCoroutine(DisableAfterSeconds());
    }

    /// <summary>
    /// Disables the grapple point after the amount of seconds specified
    /// </summary>
    /// <returns></returns>
    IEnumerator DisableAfterSeconds()
    {
        yield return new WaitForSeconds(TimeUntilDisappear);

        DisablePoint();
    }

    /// <summary>
    /// Disables the grapple point
    /// </summary>
    private void DisablePoint()
    {
        // disable the point
        gameObject.SetActive(false);
    }
}
