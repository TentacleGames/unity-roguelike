using UnityEngine;
using UnityEngine.UI;

public class DoorScript : MonoBehaviour {

	// Update is called once per frame
   private bool opened = false;
   private Animator animator;
   private Text text;


   private void Start()
   {
      animator = GetComponent<Animator>();
      text =  GetComponentInChildren<Text>();
   }

   //void OnTriggerEnter2D(Collider2D col) { useMeText(true); }

   //void OnTriggerExit2D(Collider2D col) { useMeText(false); }

   private void useMeText(bool on) { text.text = on ? "open" : ""; }

   public void ToggleTooltip()
   {
      text.text = text.text == "" ? "open" : "";
   }

   public void GetAction()
   {
      if(!opened) {
         this.GetComponent<Collider2D>().enabled = false;
         opened = true;
         animator.SetTrigger("opening");
      } else {
         this.GetComponent<Collider2D>().enabled = true;
         opened = false;
         animator.SetTrigger("closing");
      }
   }
}
