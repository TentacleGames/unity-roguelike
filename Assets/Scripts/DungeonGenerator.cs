using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random; 

public class DungeonGenerator: MonoBehaviour{
   
   //output fields
   public List<List<int>> dungeonMap;
   private List<Room> rooms = new List<Room>();
   private Dictionary<string, int> currParams = new Dictionary<string, int>();
   private int MAX_ATTEMPTS = 10;
   

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
      public int x;
      public int y;
      public int wd;
      public int hd;
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

   public void generate(Dictionary<string, int> parameters) {
      initParams(parameters);
      for (int i=0; i<currParams["rooms_count"];i++) {
         Room new_room = GenerateRoom();
         if (new_room != null)
            rooms.Add(new_room);
      }
      //SetConnections();
      //SetExits();
      GetResult();
      
   }

   private void GetResult() {
      //setting result map
      dungeonMap = SetVoidMap();
      foreach(Room room in rooms) {
         int start_x = room.x;
         int start_y = room.y;
         int end_x = room.x + room.wd -1;
         int end_y = room.y + room.hd -1;
         //printing floor
         for(int x=start_x; x<=end_x; x++)
            for(int y=start_y; y<=end_y; y++)
               dungeonMap[y][x] = 1;
         //printing walls
         for(int x = start_x - 1;x <= end_x + 1;x++){
            dungeonMap[start_y - 1][x] = 2;
            dungeonMap[end_y + 1][x] = 2;
         }
         for(int y = start_y;y <= end_y;y++){
            dungeonMap[y][start_x - 1] = 2;
            dungeonMap[y][end_x + 1] = 2;
         }

   }
   }


   private Room GenerateRoom() {
      Room room = null;
      bool collide = true;
      int attempt =0;
      while(collide && attempt<MAX_ATTEMPTS) {
         attempt++;
         room = new Room();
         room.x = Random.Range(2, currParams["width"]-currParams["min_room_size"]-2);
         room.y = Random.Range(2, currParams["height"]-currParams["min_room_size"]-2);
         room.wd = Random.Range(currParams["min_room_size"], currParams["max_room_size"]);
         room.hd = Random.Range(currParams["min_room_size"], currParams["max_room_size"]);
         collide = CheckRoomCollide(room);
      }
      if (!collide)
         room.id = rooms.Count+1;
      else
         room = null;
      return room;
   }

   private bool CheckRoomCollide(Room room) {
      bool collide = false;
      if (room.x <= 0 || room.x + room.wd + 1 >= currParams["width"] ||
          room.y <= 0 || room.y + room.hd + 1 >= currParams["height"]
         )
         collide = true;
      else {
         foreach(Room check in rooms) {
            if (check.id == room.id)
               collide = false;
            else {
               collide = !(
                           (check.x + check.wd + 1 < room.x - 1) || (check.x - 1 > room.x + room.wd + 1) ||
                           (check.y + check.hd + 1 < room.y - 1) || (check.y - 1 > room.y + room.hd + 1)
                           );
               if (collide)
                  break;
            }
         }
      }
      return collide;
   }

   private List<List<int>> SetVoidMap(int value = 0) {
      List<List<int>> result = new List<List<int>>();
      List<int> row = new List<int>();
      for(int i=0; i<currParams["width"]; i++)
         row.Add(value);
      for(int i=0; i<currParams["height"]; i++)
         result.Add(new List<int>(row));
      return result;
   }
}
