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

        //Simple Generation - subject to change
        while (map.cells.Where(x => x.filled).ToList().Count < numRooms) 
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

        hasExits.Add(cRoom);
        placedRooms.Add(cRoom);
        return cRoom;

    }
    private void MatchRandomViaMap()
    {
        //Get a random existing cell
        RoomMap.RoomCell randExistingCell = GetRandom(map.cells.Where(x => x.filled).ToList());
        //Get a random cell adjacent to the existing cell that does not have anything placed into it
        //Can change this single line to prioritize rooms building outward, inward, etc.
        RoomMap.RoomCell newCell = GetRandom(map.GetAdjacentCells(randExistingCell).Where(x => !x.filled).ToList());
        //Debug.Log(newCell.ToString());
        newCell.room = DetermineRoom(randExistingCell, newCell);
        newCell.filled = true;
        CreateRoomObject(newCell);
    }

    //TODO: Match properly based on cells adjacent to nextCell & prevcell.roomType
    private Room DetermineRoom(RoomMap.RoomCell prevCell, RoomMap.RoomCell nextCell)
    {
        //Debug.Log("New cell off of: " + prevCell.index + "\n Placing cell at: " + nextCell.index);

        //Will always contain 4 elements no matter what.
        RoomMap.RoomCell[] adjacentMapCells = map.GetAdjacentCells(nextCell).ToArray();

        //List of all enums to 
        List<PatternBuilder.Pattern.RoomType> types = PatternBuilder.Pattern.allTypes
                                                                    .Intersect(prevCell.room.compatibleTypes)
                                                                    .ToList();

        List<Room> possible = new List<Room>(rooms);

        foreach (RoomMap.RoomCell cell in adjacentMapCells){
            if (cell.filled){
                Room comparator = cell.room;
                types = types.Intersect(comparator.compatibleTypes).ToList();

                foreach (Room r in possible)
                {
                    List<Exit> exitsMatchCheck = new List<Exit>(comparator.exits.Where(x => x.HasMatchInList(r.exits)).ToList());
                    if (exitsMatchCheck.Count == r.exits.Count)
                        Debug.Log(r.roomType + "'s matches the existing room's exits at this position.");
                }
                possible = possible
                                .Where(x => ListContainsList(x.exits, comparator.exits.Where(y => y.HasMatchInList(x.exits)).ToList()))
                                .ToList();
            }
        }

        //exits of potential room must match exits in ALL surrounding cells, provided they exist.
        #region non-functional matching
        /*
        //0: Left, 1: Right, 2: Up, 3: Down
        for (int i = 0; i < adjacentMapCells.Length ;i++){
            //Must be filled in to make comparisons
            if (adjacentMapCells[i].filled){
                switch (i){
                    case 0:
                        //Restrict types to those compatible with required connections & must contain an exit that this room can match
                        if (adjacentMapCells[i].room.exits.Any(x => x.GetOrientation() == Exit.Orientation.Right)){
                            matches.Add(adjacentMapCells[i].room.exits.First(x => x.GetOrientation() == Exit.Orientation.Right));
                            types = types.Intersect(adjacentMapCells[i].room.compatibleTypes).ToList();
                        }
                        break;
                    case 1:
                        if (adjacentMapCells[i].room.exits.Any(x => x.GetOrientation() == Exit.Orientation.Left)){
                            matches.Add(adjacentMapCells[i].room.exits.First(x => x.GetOrientation() == Exit.Orientation.Left));
                            types = types.Intersect(adjacentMapCells[i].room.compatibleTypes).ToList();
                        }
                        break;
                    case 2:
                        if (adjacentMapCells[i].room.exits.Any(x => x.GetOrientation() == Exit.Orientation.Down)){
                            matches.Add(adjacentMapCells[i].room.exits.First(x => x.GetOrientation() == Exit.Orientation.Down));
                            types = types.Intersect(adjacentMapCells[i].room.compatibleTypes).ToList();
                        }
                        break;
                    case 3:
                        if (adjacentMapCells[i].room.exits.Any(x => x.GetOrientation() == Exit.Orientation.Up)){
                            matches.Add(adjacentMapCells[i].room.exits.First(x => x.GetOrientation() == Exit.Orientation.Up));
                            types = types.Intersect(adjacentMapCells[i].room.compatibleTypes).ToList();
                        }
                        break;
                }

            }
        }*/
        #endregion
        //TODO: tailor prospective list of rooms to those that contain elements found in 'matches' exit orientation list
        possible = possible.Where(x => types.Contains(x.roomType)).ToList();
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
    }
}
