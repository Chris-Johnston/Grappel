using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundTimed : MonoBehaviour {
    public float DisappearTime = 3f;

    void OnCollisionEnter2D(Collision2D collide)
    {
        //any game objects tagged as Ground or GroundDis will allow the player to jump
        if (collide.gameObject.tag == Tags.TAG_PLAYER) {
            Land();
        }
        
    }

    public void Land() {
        StartCoroutine(CountDown());
    }

    IEnumerator CountDown() {
        yield return new WaitForSeconds(DisappearTime);

        Destroy(gameObject);
    }
}
