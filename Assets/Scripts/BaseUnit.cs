using UnityEngine;
using System.Collections;

//The abstract keyword enables you to create classes and class members that are incomplete and must be implemented in a derived class.
public abstract class BaseUnit : BaseObject
{
   public float moveTime = 0.1f;           //Time it will take object to move, in seconds.
   public override BoxCollider2D boxCollider {get; protected set;}
   public virtual Rigidbody2D rb2D {get; protected set;}
   private float inverseMoveTime;          //Used to make movement more efficient.
        
   //Protected, virtual functions can be overridden by inheriting classes.
   protected override void Start ()
   {
      boxCollider = GetComponent <BoxCollider2D> (); //Get a component reference to this object's BoxCollider2D
      rb2D = GetComponent <Rigidbody2D> (); //Get a component reference to this object's Rigidbody2D
      inverseMoveTime = 1f / moveTime;
   }
        
   //Move returns true if it is able to move and false if not. 
   //Move takes parameters for x direction, y direction and a RaycastHit2D to check collision.
   protected bool CheckMove (Vector2 start, Vector2 end, out RaycastHit2D hit)
   {
      boxCollider.enabled = false; //Disable the boxCollider so that linecast doesn't hit this object's own collider.
            
      //Cast a line from start point to end point checking collision on blockingLayer.
      hit = Physics2D.Linecast (start, end, blockingLayer);
            
      boxCollider.enabled = true; //Re-enable boxCollider after linecast
            
      //Check if anything was hit
      if(hit.transform == null)
      {
            return true; //Return true to say that Move was successful
      }
      return false; //If something was hit, return false, Move was unsuccesful.
   }
        
        
   //Co-routine for moving units from one space to next, takes a parameter end to specify where to move to.
   protected IEnumerator SmoothMovement (Vector3 end)
   {
      //Calculate the remaining distance to move based on the square magnitude of the difference between current position and end parameter. 
      //Square magnitude is used instead of magnitude because it's computationally cheaper.
      float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            
      //While that distance is greater than a very small amount (Epsilon, almost zero):
      while(sqrRemainingDistance > float.Epsilon)
      {
            //Find a new position proportionally closer to the end, based on the moveTime
            Vector3 newPostion = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
                
            //Call MovePosition on attached Rigidbody2D and move it to the calculated position.
            rb2D.MovePosition (newPostion);
                
            //Recalculate the remaining distance after moving.
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
                
            //Return and loop until sqrRemainingDistance is close enough to zero to end the function
            yield return null;
      }
   }
        
        
   //The virtual keyword means AttemptMove can be overridden by inheriting classes using the override keyword.
   //AttemptMove takes a generic parameter T to specify the type of component we expect our unit to interact with if blocked (Player for Enemies, Wall for Player).
   protected virtual void AttemptMove <T> (int xDir, int yDir)
      where T : Component
   {
      //Hit will store whatever our linecast hits when Move is called.
      RaycastHit2D hit;
      Vector2 start = transform.position;
      Vector2 end = start + new Vector2 (xDir, yDir);
            
      //Set canMove to true if Move was successful, false if failed.
      bool canMove = CheckMove (start, end, out hit);
            
      //Check if nothing was hit by linecast
      if(hit.transform == null)
            //If nothing was hit, start SmoothMovement co-routine passing in the Vector2 end as destination
            StartCoroutine (SmoothMovement (end));
            return;
            
      //Get a component reference to the component of type T attached to the object that was hit
      T hitComponent = hit.transform.GetComponent <T> ();
            
      //If canMove is false and hitComponent is not equal to null, meaning MovingObject is blocked and has hit something it can interact with.
      if(!canMove && hitComponent != null)
                
            //Call the OnCantMove function and pass it hitComponent as a parameter.
            OnCantMove (hitComponent);
   }
        
   //The abstract modifier indicates that the thing being modified has a missing or incomplete implementation.
   //OnCantMove will be overriden by functions in the inheriting classes.
   protected abstract void OnCantMove <T> (T component)
      where T : Component;
}