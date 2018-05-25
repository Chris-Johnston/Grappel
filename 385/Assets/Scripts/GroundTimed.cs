using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundTimed : MonoBehaviour {
    [Range(0f,10f)]
    public float DisappearTime = 3f;

    // each object is seperate and checks whether it has collided with the player, if so
    // subroutine is initiated with Land() function call
    void OnCollisionEnter2D(Collision2D collide)
    {
        //any game objects tagged as Ground or GroundDis will allow the player to jump
        if (collide.gameObject.tag == Tags.TAG_PLAYER) {
            Land();
        } 
    }

    // calls StartCoroutine function with countdown to destory object
    public void Land() {
        StartCoroutine(CountDown());
    }

    // destories game object after pre determined time, initially set to 3seconds
    IEnumerator CountDown() {
        yield return new WaitForSeconds(DisappearTime);

        Destroy(gameObject);
    }
}
