using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CRandom = System.Random;
using URandom = UnityEngine.Random;

/*TODO: 
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
    [SerializeField] private int numRooms;
    [SerializeField] private List<GameObject> hasExits = new List<GameObject>();
    [SerializeField] private List<GameObject> placedRooms = new List<GameObject>();
    [SerializeField] private int seed;
    public RoomMap map;
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
    public void Generate()
    {
        map = new RoomMap(defaultRoomSprite);
        GenerateUnitySeed();
        Debug.Log("Seed: " + seed);
        //Initialize starting room
        RoomMap.RoomCell startCell = new RoomMap.RoomCell(new Vector2(0f, 0f), Instantiate(GetRandom(startingRooms)));
        Debug.Log("Starting Cell Info: " + startCell.ToString());
        CreateRoomObject(startCell);
        //PlaceAtLocation(new Vector2(0, 1));
        while( numRooms > map.cells.Where(x=> x.filled).ToList().Count)
            MatchRandomViaMap();
    }
    #region Room_Manipulation
    private GameObject CreateRoomObject(RoomMap.RoomCell cell)
    {
        //Debug.Log("Using Cell: " + cell.ToString());
        //Set map location to cell
        map[(int)cell.index.x, (int)cell.index.y] = cell;
        //Debug.Log("Cell Now has info: " + map[(int)cell.index.x, (int)cell.index.y]);
        cRoom = (GameObject)Instantiate(baseRoom);
        cRoom.transform.parent = transform;
        cRoom.transform.position = cell.cellPos;
        RoomDisplay display = cRoom.GetComponent<RoomDisplay>();
        display.SetRoom(cell.room);
        display.Init();
        cHList.Add(display.cHolder);
        hasExits.Add(cRoom);
        placedRooms.Add(cRoom);
        return cRoom;

    }
    /// <summary>
    /// Generation method marches outwards; picks random existing cells and builds outward based on existing adjacents.
    /// </summary>
    private void MatchRandomViaMap()
    {
        //Get a random existing cell

        List<RoomMap.RoomCell> filledCells = new List<RoomMap.RoomCell>(map.cells.Where(x => x.filled).ToList());
        RoomMap.RoomCell randExistingCell = GetRandom(filledCells.Where(x => !map.CheckCellCompletion(x)).ToList());

        //Get a random cell adjacent to the existing cell that does not have anything placed into it
        //Can change this single line to prioritize rooms building outward, inward, etc.
        RoomMap.RoomCell newCell = GetRandom(map.GetAdjacentCells(randExistingCell.index).Where(x => !x.filled).ToList());
        while (newCell.room == null){
            randExistingCell = GetRandom(map.cells.Where(x => x.filled).ToList());
            newCell = GetRandom(map.GetAdjacentCells(randExistingCell.index).Where(x => !x.filled).ToList());
            newCell.room = DetermineRoom(newCell.index);
        }
        newCell.filled = true;
        CreateRoomObject(newCell);
    }

    private void PlaceAtLocation(Vector2 index)
    {
        RoomMap.RoomCell newCell = new RoomMap.RoomCell(index, DetermineRoom(index));
        newCell.filled = true;
        CreateRoomObject(newCell);
    }
    /// <summary>
    /// Determines possible rooms to place based on cells adjacent to Vector2 index.
    /// </summary>
    /// <param name="prevCell"></param>
    /// <param name="nextCell"></param>
    /// <returns></returns>
    private Room DetermineRoom(Vector2 index)
    {

        RoomMap.RoomCell curr = map[(int)index.x, (int)index.y];;
        //Will always contain 4 elements no matter what.
        RoomMap.RoomCell[] adjacentMapCells = map.GetAdjacentCells(index).ToArray();

        //List of all enums to 
        List<PatternBuilder.Pattern.RoomType> types = new List<PatternBuilder.Pattern.RoomType>(Enum.GetValues(typeof(PatternBuilder.Pattern.RoomType))
                                                         .Cast<PatternBuilder.Pattern.RoomType>()
                                                         .ToList());

        List<Room> possible = new List<Room>(rooms);
        foreach (RoomMap.RoomCell cell in adjacentMapCells){
            if (cell.filled){
                types = types.Intersect(cell.room.compatibleTypes).ToList();
                possible = possible.Where(x => curr.ContainsExitMatchIn(cell, x)).ToList();
            }
        }

        possible = possible.Where(x => types.Any(y => y == x.roomType)).ToList();
        Debug.Log("Possible thinned to: " + possible.Count + " elements.");
        //foreach (Room p in possible)
        //  Debug.Log(p.roomType);
        if (possible.Count > 0)
            return Instantiate(GetRandom(possible));
        else{
            Debug.Log("Room could not be placed.");
            return null;
        }
    }

    private bool ListContainsList<T>(List<T> list1, List<T> list2)
    {
        int counter = 0;
        foreach(T sLE in list2){
            if (list1.Any(x => x.Equals(sLE)))
                counter++;
        }
        if (counter == list2.Count)
            return true;
        else
            return false;
    }

    #endregion
    private void OnDrawGizmos()
    {
        if (map.cells.Count > 1)
        {
            foreach (RoomMap.RoomCell c in map.cells)
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
    }
    public void ResetGenerator()
    {
        hasExits.Clear();
        placedRooms.Clear();
        seed = 0;
        connecting = null;
        RoomDisplay.counter = 0;
        map.ClearMap();
        map = new RoomMap(defaultRoomSprite);
        while (gameObject.transform.childCount > 0)
            foreach (Transform child in gameObject.transform)
                DestroyImmediate(child.gameObject);
        foreach(GameObject g in cHList)
            DestroyImmediate(g);
        cHList.Clear();
    }
}
