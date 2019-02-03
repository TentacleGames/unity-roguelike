using UnityEngine;
using System.Collections;

public class DoorScript : MonoBehaviour {

	// Update is called once per frame
   private bool opened = false;
   private int open_time = 100;
   private int MAX_OPEN_TIME = 100;
   private Animator animator;


   private void Start()
   {
      animator = GetComponent<Animator>();
   }

   void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("GameObject2 collided with " + col.name);
        openDoor();
   }

   void openDoor()
   {
      this.GetComponent<Collider2D>().enabled = false;
      opened = true;
      animator.SetTrigger("opening");
   }

   private void FixedUpdate()
   {
      if (opened && open_time>0)
      {
         open_time -= 1;
      }
      else if (open_time==0)
      {
         open_time=MAX_OPEN_TIME;
         opened = false;
         this.GetComponent<Collider2D>().enabled = true;
      }
   
   }

   public void GetAction()
   {
      if(!opened) {
         this.GetComponent<Collider2D>().enabled = false;
         opened = true;
         animator.SetTrigger("opening");
      }else {
         this.GetComponent<Collider2D>().enabled = true;
         opened = false;
         animator.SetTrigger("closing");
      }
   }
}
