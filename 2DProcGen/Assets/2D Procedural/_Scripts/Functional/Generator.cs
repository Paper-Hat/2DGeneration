using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CRandom = System.Random;
using URandom = UnityEngine.Random;

/*TODO: Generation based on level "Types": GenerationStyle Enum, "Secret" Rooms, Fix Room Type Tailoring
 * Room Map Class [X],
 * Algorithmic Placement,
 * Position Based on Map Cells/"Default" Room Size,
 * GameObject Instantiation AFTER Proper Map Generation,
 * Pattern Overlay  
     */
[System.Serializable]
[ExecuteInEditMode]
public class Generator : MonoBehaviour
{
    #region Generator_Variables
    [SerializeField] private GameObject baseRoom;
    private GameObject cRoom;
    [SerializeField] public List<Room> rooms = new List<Room>(), startingRooms = new List<Room>();
    [SerializeField] public List<GameObject> enemies = new List<GameObject>();
    [SerializeField] private Sprite defaultRoomSprite;

    [SerializeField] private List<GameObject> placedRooms = new List<GameObject>();
    [SerializeField] private int seed;
    public FloorMap map;
    private GameObject connecting;
    public List<GameObject> cHList = new List<GameObject>();
    #endregion
    #region Randomized_Functions
    private static T GetRandom<T>(List<T> list) {   return list[URandom.Range(0, list.Count)]; }
    private static int GenerateCRandom()
    {
        int rand = (int)(DateTime.Now.Ticks & 0x0000FFFF);
        CRandom seed = new CRandom(rand);
        return seed.Next();
    }
    private void GenerateUnitySeed()
    {
        if(seed == 0)
            seed = GenerateCRandom();
        URandom.InitState(seed);
    }
    #endregion
    #region Room Placement Functions

    /// <summary>
    /// Generates rooms based on enum style and number of iterations (constraints)
    /// </summary>
    /// <param name="iterations"></param>
    public void Generate(int iterations)
    {
        //Map is indexed and sized dependent on the sizing of a SINGLE 1x1 room 
        map = new FloorMap(defaultRoomSprite);
        //Create generation seed
        GenerateUnitySeed();
        Debug.Log("Seed: " + seed);
        //Initialize starting room
        PlaceStart();

        while( iterations > map.Cells.Where(x=> x.filled).ToList().Count)
            MatchRandomViaMap();
    }
    /// <summary>
    /// Place random compatible room at index (x, y)
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void PlaceRandAtLocation(int x, int y)
    {
        Vector2 index = new Vector2(x, y);
        FloorMap.RoomCell newCell = new FloorMap.RoomCell(index, GetRandom(GetCompatibleRooms(index)));
        newCell.filled = true;
        CreateRoomObject(newCell);
    }
    /// <summary>
    /// Place room at index (x, y) with selected ScriptableObject room.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="r"></param>
    public void PlaceAtLocation(int x, int y, Room r)
    {
        //Create index from x/y integers
        Vector2 index = new Vector2(x, y);

        //Create a list of rooms available for placement at this index
        List<Room> confirmAgainst = GetCompatibleRooms(index);

        //If the room chosen can be placed at this index, then create the RoomCell with the room and create an object over it
        if (confirmAgainst.Any(cp => cp == r)){
            FloorMap.RoomCell newCell = new FloorMap.RoomCell(index, r)
            {
                filled = true
            };
            CreateRoomObject(newCell);
        }
        else
            Debug.Log("Selected room is not compatible with index.");
    }
    public void PlaceStart()
    {
        FloorMap.RoomCell startCell = new FloorMap.RoomCell(new Vector2(0f, 0f), Instantiate(GetRandom(startingRooms)));
        Debug.Log("Starting Cell Info: " + startCell.ToString());
        CreateRoomObject(startCell);
    }
    #endregion
    #region Room_Manipulation
    private GameObject CreateRoomObject(FloorMap.RoomCell cell)
    {
        //Debug.Log("Using Cell: " + cell.ToString());
        //Set map location to cell
        map[(int)cell.index.x, (int)cell.index.y] = cell;
        
        //Create GameObject Room, initialize as child with cell position value
        cRoom = (GameObject)Instantiate(baseRoom);
        cRoom.transform.parent = transform;
        cRoom.transform.position = cell.cellPos;

        //Initialize room display with ScriptableObject room from cell
        RoomDisplay display = cRoom.GetComponent<RoomDisplay>();
        display.SetRoom(cell.room);
        display.Init();

        //Add colliders and gameobjects to reference lists
        cHList.Add(display.cHolder);
        placedRooms.Add(cRoom);

        return cRoom;

    }
    /// <summary>
    /// Generation method marches outwards; picks random existing cells and builds outward based on existing adjacents.
    /// </summary>
    private void MatchRandomViaMap()
    {
        //Get a random existing cell that is not "completed", or surrounded in all directions where it has exits
        var filledCells = new List<FloorMap.RoomCell>(map.Cells.Where(x => x.filled).ToList());
        var randExistingCell = GetRandom(filledCells.Where(x => !map.CheckCellCompletion(x)).ToList());
        
        //Get a random cell adjacent to the existing cell that does not have anything placed into it, but is built off its exits
        //Can change this single line to prioritize rooms building outward, inward, etc.
        var newCell = GetRandom(map.GetAdjacentWithFilter(randExistingCell).Where(x=>!x.filled).ToList());
        
        /*If it fails, find another existing cell on the map to generate from.
          This loop should not be accessed if the generator is fed enough rooms.
         */
        while (newCell.room == null){
            randExistingCell = GetRandom(filledCells.Where(x => !map.CheckCellCompletion(x)).ToList());
            newCell = GetRandom(map.GetAdjacentWithFilter(randExistingCell).Where(x => !x.filled).ToList());
            newCell.room = GetRandom(GetCompatibleRooms(newCell.index));
        }
        newCell.filled = true;
        CreateRoomObject(newCell);
    }

    /// <summary>
    /// Determines possible rooms to place based on cells adjacent to Vector2 index.
    /// </summary>
    /// <param name="prevCell"></param>
    /// <param name="nextCell"></param>
    /// <returns></returns>
    private List<Room> GetCompatibleRooms(Vector2 index)
    {

        var currentCell = map[(int)index.x, (int)index.y];

        //Will always contain 4 elements no matter what.
        var adjacentMapCells = map.GetAdjacentCells(index).Where(x => x.filled).ToList();
        
        //Only contains cells that this room will be connected to (to thin connecting types)
        var typeCheckCells = new List<FloorMap.RoomCell>(map.GetAdjacentFilterInverse(currentCell));

        //List of all enums to thin out types based on adjacent cells
        var types = new List<PatternBuilder.Pattern.RoomType>(Enum.GetValues(typeof(PatternBuilder.Pattern.RoomType))
                                                         .Cast<PatternBuilder.Pattern.RoomType>()
                                                         .ToList());

        List<Room> possible = new List<Room>(rooms);

        //Starting with list of all rooms, thin by matching exits
        foreach (var cell in adjacentMapCells)
            possible = possible.Where(x => currentCell.ContainsExitMatchIn(cell, x)).ToList();
        //Thin list of types by type compatibility of surrounding cells
        foreach (var tcCell in typeCheckCells)
            types = types.Intersect(tcCell.room.compatibleTypes).ToList();
        
        //Final cut using only valid types
        possible = possible.Where(room => types.Contains(room.roomType)).ToList();

        //If the generator is fed enough rooms with proper types/exits, this will never return null
        return (possible.Count > 0) ? possible : null;
    }

    #endregion
    #region Editor Functions
#if UNITY_EDITOR
    /// <summary>
    /// Visual cues: Visualizes map based on filled map cell rows and columns:
    /// Gizmos place yellow circles for filled cells, green circles for empty cells, nothing for uninitiated cells
    /// </summary>
    private void OnDrawGizmos()
    {
        if (map.Cells.Count <= 1) return;
        foreach (FloorMap.RoomCell c in map.Cells)
        {
            if (!c.filled)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(c.cellPos, .25f);
            }
            else
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(c.cellPos, .25f);
            }
        }
        map.VisualizeMap();
    }
    public void ResetGenerator()
    {
        placedRooms.Clear();
        seed = 0;
        connecting = null;
        RoomDisplay.counter = 0;
        map.ClearMap();
        map = new FloorMap(defaultRoomSprite);
        while (gameObject.transform.childCount > 0)
            foreach (Transform child in gameObject.transform)
                DestroyImmediate(child.gameObject);
        foreach(GameObject g in cHList)
            DestroyImmediate(g);
        cHList.Clear();
    }
#endif
#endregion
}
