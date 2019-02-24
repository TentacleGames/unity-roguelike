using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Player : MonoBehaviour {
   
   private Rigidbody2D rb2d;       //Store a reference to the Rigidbody2D component required to use 2D Physics.
   private float speed = 3.5f;
   
   private Text textBaloon;
   private DoorScript activated;
   private int MAX_ACT_CD = 50;
   private int act_cd = 0;
   private int MAX_SPCH_CD = 300;
   private int speechCooldown = 300;


    // Use this for initialization
    void Start()
    {
        //Get and store a reference to the Rigidbody2D component so that we can access it.
        rb2d = GetComponent<Rigidbody2D> ();
        textBaloon = this.GetComponentInChildren<Text>();
    }

    //FixedUpdate is called at a fixed interval and is independent of frame rate. Put physics code here.
   void FixedUpdate()
   {
      // tick cooldowns
      if (act_cd>0)
         act_cd -=1;
      if (speechCooldown>0)
      {
         speechCooldown -=1;
      }
      // *tick cooldowns

      float use = Input.GetAxis("Fire1");
      if (use>0)
         sendtAction();

      // movement
      float xx = Input.GetAxis ("Horizontal");
      float yy = Input.GetAxis ("Vertical");
      rb2d.velocity = new Vector2(xx*speed, yy*speed);

      // Use the two store floats to create a new Vector2 variable movement.
      //Vector3 movement = new Vector3 (xx, yy, 0f);
      //Call the AddForce function of our Rigidbody2D rb2d supplying movement multiplied by speed to move our player.
      //transform.position += movement * speed * Time.fixedDeltaTime;
      // *movement

      updateText();
   }

   private void updateText()
   {
      
      if (speechCooldown>0)
      {
         return;
      }else {
         if (textBaloon.text == "") {
            string newText = "";
            switch (Random.Range(0,20))
            {
               case 0:
               case 1:
                  newText = "It's lonely here...";
               break;
               case 2:
               case 3:
                  newText = "I'm bored.";
               break;
               case 4:
               case 5:
                  newText = "Hello?";
               break;
               case 6:
               case 7:
                  newText = "Anyone here?";
               break;
               case 8:
               case 9:
                  newText = "...";
               break;
               case 10:
               case 11:
                  newText = "Anyone hear me?";
               break;
               case 12:
               case 13:
                  newText = "Hey!";
               break;
               case 14:
               case 15:
                  newText = "Boring...";
               break;
               case 18:
                  newText = "Frank, you are totally f*cked!";
               break;
               case 19:
                  newText = "F*ck this shit!";
               break;
               default:
                  newText = "...";
               break;
            }
            textBaloon.text = newText;
            speechCooldown = MAX_SPCH_CD;
         }else {
            textBaloon.text = "";
            speechCooldown = MAX_SPCH_CD*2;
         }
         
         
      }
      
   }
   
   public void sendtAction()
   {
      if (activated == null)  // no activated object - do nothing
         return;
      if (act_cd > 0) // action on cooldown - do nothing
         return;
      
      act_cd = MAX_ACT_CD;
      activated.GetAction();
      
   }

   private void OnTriggerEnter2D(Collider2D collision)
   {

      if (collision.gameObject.tag == "Activated")
      {
         if (activated != null)
            activated.ToggleTooltip();
         activated = collision.gameObject.GetComponent<DoorScript>();
         activated.ToggleTooltip();
      }

   }
   private void OnTriggerExit2D(Collider2D collision)
   {
      if (activated != null)
         if (collision.gameObject == activated.gameObject)
         {
            activated.ToggleTooltip();
            activated = null;
         }
   }

}
