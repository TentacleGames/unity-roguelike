using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class DungeonGenerator: MonoBehaviour
{
   //!!!ATTENTION!!! generating more then one level at once is impossible cause of static fields!
   //output fields
   public List<List<int>> dungeonMap;
   public List<Portal> portals = new List<Portal>();
   public List<Corridor> corridors = new List<Corridor>();
   public List<Room> rooms = new List<Room>();
   private Dictionary<string, int> currParams = new Dictionary<string, int>();
   private int MAX_ATTEMPTS = 10;
   private List<Point> blockedPoints = new List<Point>();
   private List<List<int>> waveField;
   private int[][] neighborhood = new int[][] { new int[] {0,-1}, new int[] {0,1}, new int[] {-1,0}, new int[] {1,0} };
   private Dictionary<int,HashSet<int>> connections = new Dictionary<int,HashSet<int>>();
   private Dictionary<int,HashSet<int>> not_connected = new Dictionary<int,HashSet<int>>();
   public int playerX, playerY;
   

   //class generator
   public DungeonGenerator()
   {
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
   public struct Point {public int x, y;}   

   public class Room
   {
      public int id;
      public int x, y, wd, hd;
   }

   public abstract class ProtoTransition {
      public Point P1, P2;
      public int[] rooms;
      public int id;
   }

   public class Portal:ProtoTransition { }

   public class Corridor:ProtoTransition
   {
      public bool door1, door2;
      public List<Point> points;
   }

   private Point GetRandomPoint(Room room)
   {
      Point point = new Point();

      for (int i=0; i<MAX_ATTEMPTS; i++) {
         point.x = Random.Range(room.x, room.x + room.wd);
         point.y = Random.Range(room.y, room.y + room.hd);
         if (!blockedPoints.Contains(point))
            break;
      }
      return point;
   }

   private Portal GeneratePortal(Room room_a, Room room_b)
   {
      if (room_a.id == room_b.id)
         return null;
      Portal portal = new Portal();
      portal.P1 = GetRandomPoint(room_a);
      portal.P2 = GetRandomPoint(room_b);
      portal.rooms = new int[] {room_a.id, room_b.id};
      portal.id = portals.Count;
      if (! (portal.P1.x>0 && portal.P1.y>0 && portal.P2.x>0 && portal.P2.y>0))
         portal = null;
      return portal;
   }

   private struct RoomEntrance {public Point point; public string direction;};

   private RoomEntrance GetDoorPoint(Room room, string direction, bool opposite=false)
   {
      Dictionary<string, string> oppositions = new Dictionary<string, string>();
      oppositions.Add("N","S");
      oppositions.Add("S","N");
      oppositions.Add("E","W");
      oppositions.Add("W","E");
      RoomEntrance result = new RoomEntrance();
      result.direction = direction[Random.Range(0,direction.Length)].ToString();
      if (opposite)
         result.direction = oppositions[result.direction];
      switch (result.direction)
      {
         case "N":
            result.point.x = Random.Range(room.x, room.x+room.wd);
            result.point.y = room.y-1;
         break;
         case "S":
            result.point.x = Random.Range(room.x, room.x+room.wd);
            result.point.y = room.y+room.hd;
         break;
         case "E":
            result.point.x = room.x+room.wd;
            result.point.y = Random.Range(room.y, room.y+room.hd);
         break;
         case "W":
            result.point.x = room.x-1;
            result.point.y = Random.Range(room.y, room.y+room.hd);
         break;
      }
      return result;
   }

   private Point GetNextPoint (RoomEntrance ent)
   {
      Dictionary<string, int[]> directions = new Dictionary<string, int[]>();
      directions.Add("N", new int[] {0,-1});
      directions.Add("S", new int[] {0,+1});
      directions.Add("E", new int[] {+1,0});
      directions.Add("W", new int[] {-1,0});
      Point result = new Point();
      result.x = ent.point.x+directions[ent.direction][0];
      result.y = ent.point.y+directions[ent.direction][1];
      return result;
   }

   private List<List<int>> GetWaveField()
   {
      List<List<int>> wf = SetVoidMap();
      foreach (Room room in rooms)
      {
         for (int x=room.x-1; x<=room.x+room.wd; x++)
            for (int y=room.y-1; y<=room.y+room.hd; y++)
               wf[y][x] = -1;
      }
      return wf;
   }

   private int MarkPoint(Point point, int idx=0, bool need_mark=true)
   {
      int x = point.x;
      int y = point.y;
      if (x>0 && y>0 && y<=waveField.Count-2 && x<=waveField[y].Count -2)
      {
         if(need_mark)
            if(waveField[y][x] == 0)
            {
               waveField[y][x] = idx;
               return idx;
            }
            else
            {
               return 0;
            }
         else
            return waveField[y][x];
      }
      return 0;
   }

   private bool SetDistance(List<Point> points, Point stop_point)
   {
      List<Point> newPoints = new List<Point>();
      foreach (Point point in points)
      {
         int idx = waveField[point.y][point.x] + 1;
         if (idx <=0) {
            Debug.Log("point:");
            Debug.Log(point.x.ToString());
            Debug.Log(point.y.ToString());
            Debug.Log(idx.ToString());
            }
         foreach(int[] neighbor in neighborhood)
         {
            Point check = new Point();
            check.x = point.x+neighbor[0];
            check.y = point.y+neighbor[1];
            int tryMark = MarkPoint(check, idx, true);
            if (tryMark>0)
               newPoints.Add(check);
         }
      }
      if (newPoints.Count >0)
         if (newPoints.Contains(stop_point))
            return true;
         else
            return SetDistance(newPoints, stop_point);
      else
         return false;
   }

   private List<Point> GetPath(Point start_p, Point dest_p)
   {
      List<Point> result = new List<Point>();
      Point cur_p = dest_p;
      result.Add(cur_p);
      int pathType = currParams["corridor_curves"]==2 ? Random.Range(0,2): currParams["corridor_curves"];
      while (cur_p.x != start_p.x || cur_p.y != start_p.y)
      {
         int cur_idx = waveField[cur_p.y][cur_p.x];
         List<Point> possibleMoves = new List<Point>();
         foreach(int[] neighbor in neighborhood)
         {
            Point point = new Point();
            point.x = cur_p.x + neighbor[0];
            point.y = cur_p.y + neighbor[1];
            int idx = MarkPoint(point, need_mark:false);
            if (idx > 0 && idx == cur_idx-1)
               possibleMoves.Add(point);
         }
         if (possibleMoves.Count>0)
         {
            int i = pathType==0 ? 0 : Random.Range(0, possibleMoves.Count);
            cur_p = possibleMoves[i];
            result.Add(cur_p);
         }
         else
            return new List<Point>();
      }
      return result;
   }

   private List<Point> CalculatePath(Point start_p, Point dest_p)
   {
      //generates path, using Lee algorithm (wave algorithm)
      List<Point> path = new List<Point>();

      waveField = GetWaveField();
      waveField[start_p.y][start_p.x] = 1;
      List<Point> l = new List<Point>();
      l.Add(start_p);
      bool passible = SetDistance(l, dest_p);
      if (passible)
         path = GetPath(start_p, dest_p);
      return path;
   }

   private Corridor GenerateCorridor(Room room_a, Room room_b)
   {
      Corridor corridor = new Corridor();
      string direction = "";
      // finding the direction from room A to B
      if (room_a.id == room_b.id)
      {
         while (direction == "")
            direction = string.Concat(" NS"[Random.Range(0,3)]," WE"[Random.Range(0,3)]).Trim();
      }
      else
      {
         if (room_a.y != room_b.y)
            direction += room_a.y>room_b.y ? 'N' : 'S' ;
         if (room_a.x != room_b.x)
            direction += room_a.x>room_b.x ? 'W' : 'E' ;
      }
      //randomizing doors
      corridor.door1 = Random.Range(0,2)>0;
      corridor.door2 = Random.Range(0,2)>0;

      // creating door-points (on the edge of the rooms)
      RoomEntrance dp;
      dp = GetDoorPoint(room_a, direction);
      corridor.P1 = dp.point;
      Point start_p = GetNextPoint(dp);
      dp = GetDoorPoint(room_b, direction, true);
      corridor.P2 = dp.point;
      Point dest_p = GetNextPoint(dp);

      // generating path
      List<Point> path = CalculatePath(start_p, dest_p);
      if (path.Count>0)
      {
         corridor.points = path;
         corridor.rooms = new int[] {room_a.id, room_b.id};
         corridor.id = corridors.Count;
         return corridor;
      }
      else
         return null;
   }

   private void initParams(Dictionary<string, int> parameters)
   {
      foreach( KeyValuePair<string, int> kvp in parameters )
      {
         try
         {
            currParams[kvp.Key] = kvp.Value;
         }
         catch
         {
            Debug.Log("Undefined parameters key");
         }
         
      }
   }   

   public void generate(Dictionary<string, int> parameters)
   {
      initParams(parameters);
      for (int i=0; i<currParams["rooms_count"];i++)
      {
         Room new_room = GenerateRoom();
         if (new_room != null)
            rooms.Add(new_room);
      }
      foreach(Room room in rooms)
      {
         HashSet<int> con = new HashSet<int>();
         con.Add(room.id);
         connections.Add(room.id, con);
      }
      SetConnections();
      SetPlayer();
      //SetExits();
      GetResult();
      
   }

   private void SetPlayer()
   {
      int rand_room = Random.Range(0, rooms.Count);
      playerX = Random.Range(rooms[rand_room].x+1, rooms[rand_room].x+rooms[rand_room].wd);
      playerY = Random.Range(rooms[rand_room].y+1, rooms[rand_room].y+rooms[rand_room].hd);
   }

   private Room FindRoom(Room room, List<Room> rooms)
   {
      Room result = null;
      if(rooms == null || rooms.Count == 0)
         return result;
      int param = currParams["base_connecting"];
      if (param == 2) // random
      {
         int i = Random.Range(0, rooms.Count);
         result = rooms[i];
      }
      else
      {
         Point mid = new Point();
         mid.x = room.x + room.wd/2;
         mid.y = room.y + room.hd/2;
         float dist;
         if (param == 0) //closest
            dist = Mathf.Pow(currParams["width"],2) + Mathf.Pow(currParams["height"],2); //init min distance
         else
            dist = 0; //init max distance
         foreach(Room check in rooms)
         {
            if (check.id==room.id)
               continue;
            Point mid_c = new Point();
            mid_c.x = check.x + check.wd/2;
            mid_c.y = check.y + check.hd/2;
            float dist_c = Mathf.Pow(mid.x-mid_c.x, 2) + Mathf.Pow(mid.x-mid_c.x, 2);
            //squared dist is ok. if squared is minimal/maximal, then is't really minimal/maximal
            if (dist_c<dist && param==0 || dist_c>dist && param == 1)
            {
               dist = dist_c;
               result = check;
            }
         }
      }
      return result;
   }

   private void AddConnection(Room room_a, Room room_b)
   {
      int currentType;
      if (new List<int> {0, 1}.Contains(currParams["transitions_type"]))
         currentType = currParams["transitions_type"];
      else
         currentType = Random.Range(0, 100)>=currParams["portals_percent"] ? 0 : 1;
      
      ProtoTransition new_trans;

      if (currentType == 0)
      {
         new_trans = GenerateCorridor(room_a, room_b);
      }else
      {
         new_trans = GeneratePortal(room_a, room_b);
      }

      if (new_trans != null && new_trans.GetType() == typeof(Corridor))
      {
         corridors.Add((Corridor)new_trans);
      }
      else if(new_trans != null && new_trans.GetType() == typeof(Portal))
      {
         portals.Add((Portal)new_trans);
         blockedPoints.Add(new_trans.P1);
         blockedPoints.Add(new_trans.P2);
      }
      foreach(int r in new HashSet<int>(connections[room_b.id]))
         connections[r].UnionWith(connections[room_a.id]);
      foreach(int r in new HashSet<int>(connections[room_a.id]))
         connections[r].UnionWith(connections[room_b.id]);
   }

   private bool IsConnected()
   {
      HashSet<int> keys = new HashSet<int>(connections.Keys);
      foreach (KeyValuePair<int,HashSet<int>> kvp in connections)
      {
         if (not_connected.ContainsKey(kvp.Key))
            not_connected[kvp.Key] = new HashSet<int>(keys);
         else
            not_connected.Add(kvp.Key, new HashSet<int>(keys));
         not_connected[kvp.Key].ExceptWith(kvp.Value);
      }
      HashSet<int> con = new HashSet<int>();
      foreach(int x in keys)
      {
         con.UnionWith(not_connected[x]);
      }
      return con.Count == 0;

   }

   private Room GetRoom(int id)
   {
      foreach(Room r in rooms)
         if (r.id == id)
            return r;
      return null;
   }

   private List<ProtoTransition> FindPair()
   {
      List<ProtoTransition> result = new List<ProtoTransition>();
      List<ProtoTransition> transitions = new List<ProtoTransition>();
      foreach (ProtoTransition p in portals)
         transitions.Add(p);
      foreach (ProtoTransition p in corridors)
         transitions.Add(p);
      foreach(ProtoTransition check in transitions)
      {
         foreach(ProtoTransition item in transitions)
         {
            if(item.id == check.id)
               continue;
            if (new HashSet<int>(item.rooms).SetEquals(new HashSet<int>(check.rooms)))
               result.Add(item);
               result.Add(check);
         }
      }
        return result;
   }

   private bool RemoveConnection()
   {
      bool result = false;

      if (currParams["are_connected"] == 1)
      {
         result = true;

         List<ProtoTransition> pair = FindPair();
         if (pair.Count < 2)
            result = false;
         else
         {
            ProtoTransition for_del = pair[Random.Range(0,2)];
            if (for_del.GetType() == typeof(Corridor))
            {
               corridors.Remove((Corridor)for_del);
               result = true;
            }
            else if (for_del.GetType() == typeof(Portal))
            {
               portals.Remove((Portal)for_del);
               result = true;
            }
         }
      }
      else
      {
         if ((Random.Range(0,2)==0 || portals.Count ==0) && corridors.Count > 0)
         {
            corridors.RemoveAt(Random.Range(0, corridors.Count));
            result = true;
         }
         else if (portals.Count > 0)
         {
            portals.RemoveAt(Random.Range(0, portals.Count));
            result = true;
         }
      }
      return result;
   }

   private void SetConnections() {
      // if we need at least one transition from room to another
      if (currParams["each_room_transitions"] == 1)
         foreach(Room room_a in rooms) {
            Room room_b = FindRoom(room_a, rooms);
            AddConnection(room_a, room_b);
         }
      if (currParams["are_connected"] == 1)
         while (!IsConnected())
         {
            Room room_a = null;
            Room room_b = null;
            foreach(KeyValuePair<int, HashSet<int>> kvp in not_connected)
            {
               if(kvp.Value.Count > 0)
               {
                  room_a = GetRoom(kvp.Key);
                  List<Room> bRooms = new List<Room>();
                  foreach(int i in kvp.Value)
                     bRooms.Add(GetRoom(i));
                  room_b = FindRoom(room_a, bRooms);
                  break;
               }
            }
            if (room_a != null && room_b != null)
               AddConnection(room_a, room_b);
         }
         bool removed = true;
         while (corridors.Count+portals.Count-rooms.Count > currParams["max_connections_delta"] && removed)
            removed = RemoveConnection();
   }

   private void GetResult() {
      //setting result map
      dungeonMap = SetVoidMap();
      foreach(Room room in rooms)
      {
         int start_x = room.x;
         int start_y = room.y;
         int end_x = room.x + room.wd -1;
         int end_y = room.y + room.hd -1;
         //printing floor
         for(int x=start_x; x<=end_x; x++)
            for(int y=start_y; y<=end_y; y++)
               dungeonMap[y][x] = 1;
         //printing walls
         for(int x = start_x - 1;x <= end_x + 1;x++)
         {
            dungeonMap[start_y - 1][x] = 2;
            dungeonMap[end_y + 1][x] = 2;
         }
         for(int y = start_y;y <= end_y;y++)
         {
            dungeonMap[y][start_x - 1] = 2;
            dungeonMap[y][end_x + 1] = 2;
         }
      }
      // printing corridors
      foreach(Corridor corr in corridors)
      {
         foreach(Point point in corr.points)
         {
            dungeonMap[point.y][point.x] = 5;
            if (dungeonMap[point.y-1][point.x] == 0) dungeonMap[point.y-1][point.x] = 6;
            if (dungeonMap[point.y-1][point.x-1] == 0) dungeonMap[point.y-1][point.x-1] = 6;
            if (dungeonMap[point.y-1][point.x+1] == 0) dungeonMap[point.y-1][point.x+1] = 6;

            if (dungeonMap[point.y][point.x-1] == 0) dungeonMap[point.y][point.x-1] = 6;
            if (dungeonMap[point.y][point.x+1] == 0) dungeonMap[point.y][point.x+1] = 6;

            if (dungeonMap[point.y+1][point.x] == 0) dungeonMap[point.y+1][point.x] = 6;
            if (dungeonMap[point.y+1][point.x-1] == 0) dungeonMap[point.y+1][point.x-1] = 6;
            if (dungeonMap[point.y+1][point.x+1] == 0) dungeonMap[point.y+1][point.x+1] = 6;

         }
         //printing doors
         dungeonMap[corr.P1.y][corr.P1.x] = corr.door1 ? 3 : 5;
         dungeonMap[corr.P2.y][corr.P2.x] = corr.door2 ? 4 : 5;
      }
      //printing portals
      foreach(Portal portal in portals)
      {
         dungeonMap[portal.P1.y][portal.P1.x] = 7;
         dungeonMap[portal.P2.y][portal.P2.x] = 7;
      }
      
   }

   private Room GenerateRoom() {
      Room room = null;
      bool collide = true;
      int attempt =0;
      while(collide && attempt<MAX_ATTEMPTS) {
         attempt++;
         room = new Room();
         room.x = Random.Range(2, currParams["width"]-currParams["min_room_size"]-1);
         room.y = Random.Range(2, currParams["height"]-currParams["min_room_size"]-1);
         room.wd = Random.Range(currParams["min_room_size"], currParams["max_room_size"]+1);
         room.hd = Random.Range(currParams["min_room_size"], currParams["max_room_size"]+1);
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
