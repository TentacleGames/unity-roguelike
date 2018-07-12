using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;      //Allows us to use SceneManager

//Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
public class Unit : BaseUnit {
   private Animator animator;                  //Used to store a reference to the Player's animator component.
   public bool userControlled = false;
   public int controlledTime = -1; // -1 - infinity
        
   //Start overrides the Start function of MovingObject
   protected override void Start (){
      //Get a component reference to the Player's animator component
      animator = GetComponent<Animator>();
            
      //Call the Start function of the MovingObject base class.
      base.Start ();
   }
        
   //This function is called when the behaviour becomes disabled or inactive.
   private void OnDisable ()
   {
      //When Player object is disabled, store the current local food total in the GameManager so it can be re-loaded in next level.
      //GameManager.instance.playerFoodPoints = food;
   }
        
   private void Update (){
      
      if (userControlled==true) {
         int horizontal = 0;     //Used to store the horizontal move direction.
         int vertical = 0;       //Used to store the vertical move direction.
              
         //Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
         horizontal = (int) (Input.GetAxisRaw ("Horizontal"));
            
         //Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
         vertical = (int) (Input.GetAxisRaw ("Vertical"));
            
         //Check if moving horizontally, if so set vertical to zero.
         if(horizontal != 0)
         {
               vertical = 0;
         }
            
         //Check if we have a non-zero value for horizontal or vertical
         if(horizontal != 0 || vertical != 0){
               //Call AttemptMove passing in the generic parameter Wall, since that is what Player may interact with if they encounter one (by attacking it)
               //Pass in horizontal and vertical as parameters to specify the direction to move Player in.
               AttemptMove<BaseObject> (horizontal, vertical);
         }
      }else {
         //AI logic
      }
   }
        
   //AttemptMove overrides the AttemptMove function in the base class MovingObject
   //AttemptMove takes a generic parameter T which for Player will be of the type Wall, it also takes integers for x and y direction to move in.
   protected override void AttemptMove <T> (int xDir, int yDir){
            
      //Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
      base.AttemptMove <T> (xDir, yDir);            
   }
   public void takeTurn() {
      if (controlledTime >=0) {
         controlledTime -=1; //decreace control time
         if (controlledTime == -1) //if contolled time is over, reverse the control
            userControlled = !userControlled;
      }
   }
        
   //OnCantMove overrides the abstract function OnCantMove in MovingObject.
   //It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
   protected override void OnCantMove <T> (T component){
   }
        
   //OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
   private void OnTriggerEnter2D (Collider2D other){
      //Check if the tag of the trigger collided with is Exit.
      if(other.tag == "Exit"){
            //Disable the player object since level is over.
            enabled = false;
      }   
      //Check if the tag of the trigger collided with is Food.
      else if(other.tag == "Food"){
            //Disable the food object the player collided with.
            other.gameObject.SetActive (false);
      }
      //Check if the tag of the trigger collided with is Soda.
      else if(other.tag == "Soda"){          
            //Disable the soda object the player collided with.
            other.gameObject.SetActive (false);
      }
   }

}