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

   private Room FindRoom(Room room, List<Room> rooms) {
      Room result = null;
      if(rooms == null || rooms.Count == 0)
         return result;
      int param = currParams["base_connecting"];
      if (param == 2) { // random
         int i = Random.Range(0, rooms.Count-1);
         result = rooms[i];
      }
      else {
         Point mid = new Point();
         mid.x = room.x + room.wd/2;
         mid.y = room.y + room.hd/2;
         float dist;
         if (param == 0) //closest
            dist = Mathf.Pow(currParams["width"],2) + Mathf.Pow(currParams["height"],2); //init min distance
         else
            dist = 0; //init max distance
         foreach(Room check in rooms) {
            if (check.id==room.id)
               continue;
            Point mid_c = new Point();
            mid_c.x = check.x + check.wd/2;
            mid_c.y = check.y + check.hd/2;
            float dist_c = Mathf.Pow(mid.x-mid_c.x, 2) + Mathf.Pow(mid.x-mid_c.x, 2);
            //squared dist is ok. if squared is minimal/maximal, then is't really minimal/maximal
            if (dist_c<dist && param==0 || dist_c>dist && param == 1) {
               dist = dist_c;
               result = check;
            }
         }
      }
      return result;
   }

   private void AddConnection(Room room_a, Room room_b) {
            //new_corridor = None
            //new_portal = None
            //if self.params.get('transitions_type') == 'corridors':
            //    new_corridor = self._generate_corridor(room_a, room_b)
            //elif self.params.get('transitions_type') == 'portals':
            //    new_portal = self._generate_portal(room_a, room_b)
            //elif self.params.get('transitions_type') == 'both':
            //    if randint(1, 100) >= self.params['portals_percent']:
            //        new_corridor = self._generate_corridor(room_a, room_b)
            //    else:
            //        new_portal = self._generate_portal(room_a, room_b)
            //if new_corridor:
            //    self.corridors.append(new_corridor)
            //if new_portal:
            //    self.portals.append(new_portal)
            //    self.blocked_points.append(new_portal.P1)
            //    self.blocked_points.append(new_portal.P2)
            //for r in self.connections[room_b.id]:
            //    self.connections[r] = self.connections[r] | self.connections[room_a.id]
            //for r in self.connections[room_a.id]:
            //    self.connections[r] = self.connections[r] | self.connections[room_b.id]
   }

   private void SetConnections() {
      // if we need at least one transition from room to another
      if (currParams["each_room_transitions"] == 1)
         foreach(Room room_a in rooms) {
            Room room_b = FindRoom(room_a, rooms);
            AddConnection(room_a, room_b);
         }
      // if we need guarantees that each rooms is connected
      //if (currParams["are_connected"] == 1)
      //   while(!IsConnected()) {
      //   ...
      //   }

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
