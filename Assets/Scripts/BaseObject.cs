using UnityEngine;

//The abstract keyword enables you to create classes and class members that are incomplete and must be implemented in a derived class.
public abstract class BaseObject : MonoBehaviour
{
   public LayerMask blockingLayer;         //Layer on which collision will be checked.
   public virtual BoxCollider2D boxCollider {get; protected set;}
        
   //Protected, virtual functions can be overridden by inheriting classes.
   protected virtual void Start ()
   {
      boxCollider = GetComponent <BoxCollider2D> (); //Get a component reference to this object's BoxCollider2D
   }
}