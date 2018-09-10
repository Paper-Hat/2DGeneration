using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using CRandom = System.Random;
using URandom = UnityEngine.Random;
using System;
#region Procedural Generation
public class Generator : MonoBehaviour
{
    [SerializeField] private GameObject baseRoom;
    [SerializeField] public List<Room> rooms = new List<Room>(), startingRooms = new List<Room>();
    [SerializeField] public List<GameObject> enemies = new List<GameObject>();
    [SerializeField] private int iterations;
    [SerializeField] private List<GameObject> placedRooms;
    [SerializeField] private int seed;
    private GameObject connecting;

    #region Randomized_Functions
    private static T GetRandom<T>(List<T> list) { return list[URandom.Range(0, list.Count)]; }
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
    private Room GetAppropriateRandom(List<Room> roomObjs, PatternBuilder.Pattern.RoomType matchingType)
    {
        //Fix this to cycle through the rooms' compatibleTypes array
        List<Room> appropriateTypes = roomObjs.Where(x => x.compatibleTypes.Contains(matchingType) && x.exits.Count > 0).ToList();
        if (appropriateTypes.Count <= 0)
            Debug.Log("Failed to find appropriate room.");
        return GetRandom(appropriateTypes);
    }
    #endregion
    public void Start()//CreateStartingRoom()
    {
        GenerateUnitySeed();
        Debug.Log(seed);
        Room startRoom = Instantiate(GetRandom(startingRooms));
        CreateRoomObject(startRoom, new Vector3(0f, 0f, 0f));
        for(int i = 0; i<iterations;i++)
            PlaceRoomObject();
    }
    //TODO: fix this to return a gameobject, set it to private variable and remove connecting exit on exit matches
    private GameObject CreateRoomObject(Room toDisplay, Vector3 position)
    {
        foreach (GameObject g in placedRooms)
        {
            if (g.transform.position == position)
                return null;
        }
        baseRoom = (GameObject)Instantiate(baseRoom, position, Quaternion.identity);
        //Debug.Log("Object Position:" + baseRoom.transform.position + "\n Object LocalPosition: "+ baseRoom.transform.localPosition);
        baseRoom.transform.parent = transform;
        RoomDisplay display = baseRoom.GetComponent<RoomDisplay>();
        SpriteRenderer renderer = baseRoom.GetComponent<SpriteRenderer>();
        display.runtimeSprite = renderer;
        display.room = toDisplay;
        display.Init();
        placedRooms.Add(baseRoom);
        return baseRoom;

    }
    private void PlaceRoomObject()
    {
        GameObject connectToObj = GetRandom(placedRooms);
        MatchExits(connectToObj);
    }
    private void MatchExits(GameObject connectingTo)
    {
        RoomDisplay r = connectingTo.GetComponent<RoomDisplay>();
        Room a = r.room;
        Room b = GetAppropriateRandom(rooms, a.roomType);
        Dictionary<Exit, Exit> exitPairs = new Dictionary<Exit, Exit>();
        List<Exit> existing = a.exits, prospective = b.exits;
        for (int i = 0; i < existing.Count; i++){
            Exit matched = prospective.FirstOrDefault(x => x.Matches(existing[i].GetOrientation()));
            //Debug.Log("Prospective exit: " + matched.ToString());
            if(matched != null)
                exitPairs.Add(existing[i], matched);
        }
        if (exitPairs.Count > 0){
            Exit connectToPicked = GetRandom(exitPairs.Keys.ToList());
            //Debug.Log("Picked: " + connectToPicked.GetOrientation() + " \n Match: " + exitPairs[connectToPicked].GetOrientation());
            Vector3 newPos = DeterminePosition(a, b, connectToPicked, exitPairs[connectToPicked]);
            connecting = CreateRoomObject(b, newPos);
            if (connecting){
                
                //Remove exit on object that is to be placed
                List<Exit> connectedToExits = connecting.GetComponent<RoomDisplay>().roomExits;
                Exit newRemove = connectedToExits.FirstOrDefault(x => x.location == connectToPicked.location);
                //Remove exit on previous object
                r.roomExits.Remove(r.roomExits.FirstOrDefault(x => x.location == connectToPicked.location));
                connectedToExits.Remove(newRemove);
            }
        }
        else
        {
            Debug.Log("Matching pairs not found on iteration.");
        }
    }
    private Vector3 DeterminePosition(Room current, Room next, Exit currExit, Exit nextExit)
    {
        Bounds eBounds = current.roomSprite.bounds;
        Bounds cBounds = next.roomSprite.bounds;
        float newPosX = currExit.location.x, newPosY = currExit.location.y;
        if (currExit.GetOrientation() == Exit.Orientation.Down)
        {
            newPosY -= cBounds.extents.y;
            newPosX += nextExit.xMod;
        }
        else if (currExit.GetOrientation() == Exit.Orientation.Up)
        {
            newPosY += cBounds.extents.y;
            newPosX -= nextExit.xMod;
        }
        else if (currExit.GetOrientation() == Exit.Orientation.Right)
        {
            newPosX += cBounds.extents.x;
            newPosY -= nextExit.yMod;
        }
        else if (currExit.GetOrientation() == Exit.Orientation.Left)
        {
            newPosX -= cBounds.extents.x;
            newPosY += nextExit.yMod;
        }
        return new Vector3(newPosX, newPosY);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (GameObject g in placedRooms)
            foreach(Exit e in g.GetComponent<RoomDisplay>().room.exits)
                Gizmos.DrawSphere(e.location, 0.1f);
    }

}
#endregion