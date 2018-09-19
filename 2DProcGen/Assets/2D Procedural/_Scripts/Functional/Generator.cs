using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CRandom = System.Random;
using URandom = UnityEngine.Random;

/*TODO: 
 * Room Map Class,
 * Algorithmic Placement,
 * Position Based on Map Cells/"Default" Room Size,
 * GameObject Instantiation AFTER Proper Map Generation,
 * Pattern Overlay  
     */
public class Generator : MonoBehaviour
{
    #region Generator_Variables
    [SerializeField] private GameObject baseRoom;
    [SerializeField] public List<Room> rooms = new List<Room>(), startingRooms = new List<Room>();
    [SerializeField] public List<GameObject> enemies = new List<GameObject>();
    [SerializeField] private int numRooms;
    [SerializeField] private List<GameObject> hasExits = new List<GameObject>();
    [SerializeField] private List<GameObject> placedRooms = new List<GameObject>();
    [SerializeField] private int seed;
    private GameObject connecting;
    #endregion
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
#if UNITY_EDITOR
        UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
#endif
        GenerateUnitySeed();
        Debug.Log(seed);
        CreateRoomObject(Instantiate(GetRandom(startingRooms)), new Vector3(0f, 0f, 0f));
        while (placedRooms.Count < numRooms && hasExits.Count > 0)
            PlaceRoomObject();
        if (hasExits.Count == 0)
            Debug.Log("Ran out of exits to place rooms.");
    }
    #region Room_Manipulation
    private GameObject CreateRoomObject(Room toDisplay, Vector3 position)
    {
        baseRoom = (GameObject)Instantiate(baseRoom, position, Quaternion.identity);
        //Debug.Log("Object Position:" + baseRoom.transform.position + "\n Object LocalPosition: "+ baseRoom.transform.localPosition);
        baseRoom.transform.parent = transform;
        RoomDisplay display = baseRoom.GetComponent<RoomDisplay>();
        display.SetRoom(toDisplay);
        display.Init();
        hasExits.Add(baseRoom);
        placedRooms.Add(baseRoom);
        return baseRoom;

    }
    private void PlaceRoomObject()
    {
        GameObject current = GetRandom(hasExits);
        MatchExitsRandom(current);
    }
    private void MatchExitsRandom(GameObject current)
    {
        RoomDisplay r = current.GetComponent<RoomDisplay>();
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

        //If a matching pair of exits was found
        if (exitPairs.Count > 0){
            Exit connectToPicked = GetRandom(exitPairs.Keys.ToList());
            Vector3 newPos = DeterminePosition(a, b, connectToPicked, exitPairs[connectToPicked]);

            //Debug.Log(b.roomSprite.rect.ToString());
            //Check for collision
            if(!CheckCollision(newPos, b.roomSprite.rect))
                connecting = CreateRoomObject(b, newPos);

            if (connecting){   
                //Remove exit on object that is to be placed
                List<Exit> connectedToExits = connecting.GetComponent<RoomDisplay>().roomExits;
                Exit newRemove = connectedToExits.FirstOrDefault(x => x.location == connectToPicked.location);
                //Remove exit on previous object
                r.roomExits.Remove(r.roomExits.FirstOrDefault(x => x.location == connectToPicked.location));
                connectedToExits.Remove(newRemove);
                if (r.roomExits.Count == 0)
                    hasExits.Remove(r.gameObject);
                if (connectedToExits.Count == 0)
                    hasExits.Remove(connecting);
            }
        }
        else
        {
            //Debug.Log("Matching pairs not found on iteration.");
        }
    }
    #endregion
    #region Placement_Position_Validation
    //bounding box collision check since all sprites contain rect transform component
    private bool CheckCollision(Vector3 centerPos, Rect next)
    {
        Rect fNext = new Rect(0f, 0f, (float)Math.Round(next.width * .01f, 3, MidpointRounding.AwayFromZero), (float)Math.Round(next.height * .01f, 3, MidpointRounding.AwayFromZero))
        {
            center = centerPos
        };
        foreach (GameObject g in placedRooms){
            Rect toMod = g.GetComponent<RoomDisplay>().room.roomSprite.rect;
            Rect fAgainst = new Rect(0f, 0f, (float)Math.Round(toMod.width * .01f, 3, MidpointRounding.AwayFromZero),
                                            (float)Math.Round(toMod.height * .01f, 3, MidpointRounding.AwayFromZero))
            {
                center = g.transform.position
            };
            if (RectOverlaps(fNext, fAgainst)){
                Debug.Log("Collision!");
                return true;
            }
        }
        return false;

    }
    private Vector3 DeterminePosition(Room current, Room next, Exit currExit, Exit nextExit)
    {
        Bounds eBounds = current.roomSprite.bounds;
        Bounds cBounds = next.roomSprite.bounds;
        float newPosX = currExit.location.x, newPosY = currExit.location.y;
        if (currExit.GetOrientation() == Exit.Orientation.Down)
        {
            newPosY -= cBounds.extents.y;
            newPosX -= nextExit.xMod;
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
            newPosY -= nextExit.yMod;
        }
        return new Vector3(newPosX, newPosY);
    }
    //Custom Rect Bounds checker - Rectangles do NOT overlap if they share walls
    private bool RectOverlaps(Rect a, Rect b)
    {
        if (Math.Round(a.x, 3, MidpointRounding.AwayFromZero) < Math.Round(b.x + b.width, 3, MidpointRounding.AwayFromZero) &&
               Math.Round(a.x + a.width, 3, MidpointRounding.AwayFromZero) > Math.Round(b.x, 3, MidpointRounding.AwayFromZero) &&
               Math.Round(a.y, 3, MidpointRounding.AwayFromZero) < Math.Round(b.y + b.height, 3, MidpointRounding.AwayFromZero) &&
               Math.Round(a.y + a.height, 3, MidpointRounding.AwayFromZero) > Math.Round(b.y, 3, MidpointRounding.AwayFromZero))
            return true;
        else
            return false;
        #region Debugging bounds check
        /*Debug.Log((Math.Round(a.x, 3, MidpointRounding.AwayFromZero) + " < " + Math.Round(b.x + b.width, 3, MidpointRounding.AwayFromZero) + " \n " +
    Math.Round(a.x + a.width, 3, MidpointRounding.AwayFromZero) + " > " + Math.Round(b.x, 3, MidpointRounding.AwayFromZero) + "\n" +
    Math.Round(a.y, 3, MidpointRounding.AwayFromZero) + " < " + Math.Round(b.y + b.height, 3, MidpointRounding.AwayFromZero) + "\n" +
    Math.Round(a.y + a.height, 3, MidpointRounding.AwayFromZero) + " > " + Math.Round(b.y, 3, MidpointRounding.AwayFromZero)));*/
        #endregion
    }
    #endregion
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (GameObject g in placedRooms)
            foreach(Exit e in g.GetComponent<RoomDisplay>().roomExits)
                Gizmos.DrawSphere(e.location, 0.1f);
    }
}
