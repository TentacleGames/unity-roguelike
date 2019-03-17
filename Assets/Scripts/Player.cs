using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Player : MonoBehaviour {
   
   private Rigidbody2D rb2d;       //Store a reference to the Rigidbody2D component required to use 2D Physics.
   private float speed = 3.7f;
   private float NORMAL_SPEED = 3.7f;
   private float MAX_SPEED = 6f;
   private int stamina = 200;
   private int MAX_STAM = 200;

   //states
   private bool running = false;
   //*states
   
   private Text textBaloon;
   private Slider staminaSlider;
   private Image staminaBar;
   private DoorScript activated;
   private int MAX_ACT_CD = 50;
   private int act_cd = 0;
   private int MAX_SPCH_CD = 300;
   private int speechCooldown = 300;

   public int lightRadius = 3;


    // Use this for initialization
    void Start()
    {
        //Get and store a reference to the Rigidbody2D component so that we can access it.
        rb2d = GetComponent<Rigidbody2D> ();
        textBaloon = this.GetComponentInChildren<Text>();
        staminaSlider = this.GetComponentInChildren<Slider>();
        staminaSlider.maxValue = MAX_STAM;
        staminaBar = staminaSlider.fillRect.GetComponent<Image>();
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
      float shift = Input.GetAxis("Fire3");
      runningEvaluation(shift);

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

   private void runningEvaluation(float shift)
   {
      staminaSlider.value = stamina;
      if (!running && stamina<MAX_STAM) //resting
      {
         stamina +=1;
         if (speed > NORMAL_SPEED)
            speed -= 1f;
         if (speed < NORMAL_SPEED)
            speed = NORMAL_SPEED;
      } //no "else" statement to be able of running with stam less then maximum
      
      if (shift > 0 && stamina>0 && !running) //start running
      {
         running = true;
      }
      else if (shift > 0 && running && stamina>0) //running
      {
         if (speed < MAX_SPEED) //speeding up
            speed += 0.5f;
         if (speed > MAX_SPEED)
            speed = MAX_SPEED;
         stamina -=1;
      }
      else if (running && (shift > 0 && stamina<=0 || shift <=0) ) //stop running
      {
         running = false;
      }
      if (stamina == MAX_STAM && staminaBar.enabled)
         staminaBar.enabled = false;
         
      else if (stamina != MAX_STAM && !staminaBar.enabled)
         staminaBar.enabled = true;
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
