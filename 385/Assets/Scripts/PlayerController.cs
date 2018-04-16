using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    //This works for back and forth
    [Range (0,40)]
    public float speed;
    [Range (0,40)]
    public float jumpHeight;

    private Rigidbody2D rigid;
    private bool onGround;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        onGround = true;
    }

    void FixedUpdate()
    {
        //Left and Right movement
        float moveHorizontal = Input.GetAxis("Horizontal");
        //float moveVertical = Input.GetAxis("Vertical");
        Vector2 movement = new Vector2(moveHorizontal, 0);
        rigid.AddForce(movement * speed);

        //Jump
        if (Input.GetKeyDown(KeyCode.Space) && onGround)
        {
            rigid.velocity = new Vector2(0, jumpHeight);
            onGround = false;
        }
    }

    //Ground check for player - can only jump while on ground
    void OnCollisionEnter2D (Collision2D collide)
    {
        //any game objects tagged as Ground will allow the player to jump
        if (collide.gameObject.tag == "Ground")
        {
            onGround = true;
        }
    }
    
    void OnCollisionExit ()
    {
        onGround = true;
    }

}
