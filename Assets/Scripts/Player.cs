using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
   
    private Rigidbody2D rb2d;       //Store a reference to the Rigidbody2D component required to use 2D Physics.
   private float speed = 5f;
    // Use this for initialization
    void Start()
    {
        //Get and store a reference to the Rigidbody2D component so that we can access it.
        rb2d = GetComponent<Rigidbody2D> ();
    }

    //FixedUpdate is called at a fixed interval and is independent of frame rate. Put physics code here.
   void FixedUpdate()
   {
      float xx = Input.GetAxis ("Horizontal");
      float yy = Input.GetAxis ("Vertical");
        
      //Use the two store floats to create a new Vector2 variable movement.
      Vector3 movement = new Vector3 (xx, yy, 0f);
      Debug.Log(movement.ToString());

      //Call the AddForce function of our Rigidbody2D rb2d supplying movement multiplied by speed to move our player.
      transform.position += movement * speed * Time.fixedDeltaTime;
   }

}
