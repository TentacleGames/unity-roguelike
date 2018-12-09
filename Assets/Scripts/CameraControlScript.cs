using UnityEngine;
using System.Collections;

public class CameraControlScript : MonoBehaviour {

   private GameObject player;

	// Update is called once per frame
	void LateUpdate () {
      if (player != null)
      {
         if (player.transform.position.x != transform.position.x || player.transform.position.y != transform.position.y)
            transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -10f);
      }
      else
      {
         FindPlayer();
      }
	}

   private void FindPlayer()
   {
      if (player == null)
      {
         player = GameObject.FindGameObjectWithTag("Player");
      }
   }

}
