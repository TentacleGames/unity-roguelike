using UnityEngine;
using System;
using System.Collections.Generic;       //Allows us to use Lists.
using Random = UnityEngine.Random;      //Tells Random to use the Unity Engine random number generator.

// 0: ' ', # void
// 1: '.', # floor
// 2: '#', # wall
// 3: '+', # door
// 4: '+', # door2
// 5: '.', # corridor floor
// 6: '#', # corridor wall
// 7: '0', # portal
// 8: '<', # ladder up
// 9: '>', # ladder down
    
public class BoardManager : MonoBehaviour
{
           
   public int columns = 10;                                         //Number of columns in our game board.
   public int rows = 10;                                            //Number of rows in our game board.
   public GameObject exit;                                         //Prefab to spawn for exit.
   public GameObject enter;                                         //Prefab to spawn for exit.
   public GameObject portal;                                         //Prefab to spawn for exit.
   public GameObject door;                                         //Prefab to spawn for exit.
   public GameObject[] floorTiles;                                 //Array of floor prefabs.
   public GameObject[] floorRoomTiles;                                 //Array of floor prefabs.
   public GameObject[] wallTiles;                                  //Array of wall prefabs.
   public GameObject[] wallRoomTiles;                                  //Array of wall prefabs.
   public GameObject[] enemyTiles;                                 //Array of enemy prefabs.
   public GameObject[] outerWallTiles;                             //Array of outer tile prefabs.
   public GameObject playerTile;
   public int px, py;
        
   private Transform boardHolder;                                  //A variable to store a reference to the transform of our Board object.
   private DungeonGenerator dungeonScript;
        
   //SetupScene initializes our level and calls the previous functions to lay out the game board
   public void SetupScene(int level)
   {
      
      Dictionary<string, int> parameters = new Dictionary<string, int>();
      parameters.Add("width", columns);
      parameters.Add("height", rows);
      parameters.Add("rooms_count", 15);
      parameters.Add("min_room_size", 3);
      parameters.Add("max_room_size", 7);
      parameters.Add("transitions_type", 0);
      parameters.Add("are_connected", 1);
      parameters.Add("max_connections_delta", 1);
      parameters.Add("portals_percent", 10);
      
      dungeonScript = GetComponent<DungeonGenerator>();
      
      dungeonScript.generate(parameters);
      List<List<int>> dungeonMap = dungeonScript.dungeonMap;
      
      BoardSetup (dungeonMap); //Creates the outer walls and floor.
   }
   
   //Sets up the outer walls and floor (background) of the game board.
   void BoardSetup(List<List<int>> dungeonMap)
   {
      //Instantiate Board and set boardHolder to its transform.
      boardHolder = new GameObject ("Board").transform;
      for(int x = -1; x < columns + 1; x++) {
            for(int y = -1; y < rows + 1; y++) {
               GameObject toInstantiate = null;
                    
               //Check if we current position is at board edge, if so choose a random outer wall prefab from our array of outer wall tiles.
               if(x == -1 || x == columns || y == -1 || y == rows)
                  toInstantiate = outerWallTiles [Random.Range (0, outerWallTiles.Length)];
               else
                  switch(dungeonMap[x][y])
                  {
                     case 0: break;
                     case 1: toInstantiate = floorRoomTiles[Random.Range (0, floorTiles.Length)]; break;
                     case 2: toInstantiate = wallRoomTiles[Random.Range (0, wallTiles.Length)]; break;
                     case 3:
                     case 4: toInstantiate = door; break;
                     case 5: toInstantiate = floorTiles[Random.Range (0, floorTiles.Length)]; break;
                     case 6: toInstantiate = wallTiles[Random.Range (0, wallTiles.Length)]; break;
                     case 7: toInstantiate = portal; break;
                     case 8: toInstantiate = enter; break;
                     case 9: toInstantiate = exit; break;
                     default: Debug.Log("Unexpected value"); break;

                  }
               if(toInstantiate != null) {
                  GameObject instance = Instantiate (toInstantiate, new Vector3 (x, y, 0f), Quaternion.identity) as GameObject;
                  instance.transform.SetParent (boardHolder); //Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
               }
            }
      }
      py = dungeonScript.playerX;
      px = dungeonScript.playerY;
      GameObject p_instance = Instantiate (playerTile, new Vector3 (px, py, 0f), Quaternion.identity) as GameObject;
      p_instance.transform.SetParent (boardHolder); //Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
      
   }
}