using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random; 

public class DungeonGenerator: MonoBehaviour{
   
   //output fields
   public List<List<int>> dungeonMap;
   

   //class generator
   public DungeonGenerator() {
      currParams.Add("width", 120);
      currParams.Add("height", 50);
      currParams.Add("rooms_count", 10);
      currParams.Add("min_room_size", 6);
      currParams.Add("max_room_size", 12);
      currParams.Add("transitions_type", 2); // 0..2: corridors/portals/both
      currParams.Add("portals_percent", 50); // portals percent, if allowed
      currParams.Add("each_room_transitions", 1); // 0-1: generate a corridor for each room
      currParams.Add("base_connecting", 2); // 0..2: closest, farest, random
      currParams.Add("are_connected", 1); // 0-1: generate additional corridors, if needed, to connect the dungeon
      currParams.Add("corridor_curves", 0); // 0..2: straight (as possible), curved, random
      currParams.Add("max_connections_delta", 10); // max delta: (corridors + portals)-rooms
   }

   //useful subclasses
   public class Point {
      public int x;
      public int y;
   }   

   public class Room {
      public int id;
      public Point position;
      public Point size;
   }

   private void initParams(Dictionary<string, int> parameters) {
      foreach( KeyValuePair<string, int> kvp in parameters ) {
         try {
            currParams[kvp.Key] = kvp.Value;
         }catch {
            Debug.Log("Undefined parameters key");
         }
         
      }
   }   

   private Dictionary<string, int> currParams = new Dictionary<string, int>();

   public void generate(Dictionary<string, int> parameters) {
      initParams(parameters);
      //for (int i=0; i<currParams["rooms_count"];i++) {
      //   Room new_room = GenerateRoom();
      //   if (new_room != null)
      //      rooms.Add(new_room);
      //}
      //SetConnections();
      //SetExits();
      GetResult();
      
   }

   private void GetResult() {
      //setting result map
      dungeonMap = new List<List<int>>();
      for (int i=0; i<currParams["width"]; i++) {
         List<int> row = new List<int>();
         for (int j=0; j<currParams["height"]; j++)
            row.Add(Random.Range(0,3));
         dungeonMap.Add(row);
      }
   }

}
