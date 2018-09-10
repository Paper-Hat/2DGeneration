using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
#if UNITY_EDITOR
[System.Serializable]
[ExecuteInEditMode]
public class RoomBuilder : MonoBehaviour
{
    //Up, down, left, right
    [Range(0f,1f)] [SerializeField] private float horizontalModification = .5f, verticalModification = .5f;
    [SerializeField] public PatternBuilder.Pattern.RoomType roomType;
    [SerializeField] public List<PatternBuilder.Pattern.RoomType> compatible;
    [SerializeField] public GameObject spriteHolder;
    [SerializeField] public Room editing;
    private Sprite roomSprite;
    private float extentsX, centerX, extentsY, centerY;
    [SerializeField] public List<Exit> exits = new List<Exit>();
    [SerializeField] float xMod = 0f, yMod = 0f;
    public bool initialized;
    private string reason;
    /*Get extents on either side of image bounds, subtract or add extents on x or y
    dependent on where the exit is on the image */
    public string GetReason() { return reason; }
    public bool Init()
    {
        bool failed = false;
        if (!initialized){
            roomSprite = spriteHolder.GetComponent<SpriteRenderer>().sprite;
            if (roomSprite){
                initialized = true;
                extentsX = roomSprite.bounds.extents.x;
                centerX = roomSprite.bounds.center.x;
                extentsY = roomSprite.bounds.extents.y;
                centerY = roomSprite.bounds.center.y;
                xMod = 0f;
                yMod = 0f;
            }
            else{
                failed = true;
                reason = "Room sprite not set in inspector.";
            }
        }
        else{
            failed = true;
            reason = "Builder already initialized.";
        }
        return failed;
    }
    private void OnValidate()
    {
        if (exits.Count > 0)
            HandleExits();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if(exits.Count > 0)
            foreach(Exit e in exits)
                Gizmos.DrawSphere(e.location, .05f);
    }
    public bool CreateExit(Exit.Orientation o)
    {
        bool failed = false;
        if (initialized){
            if (roomSprite){
                Vector3 createdLoc = new Vector3();
                //up
                if (o == Exit.Orientation.Up)
                    createdLoc = new Vector3(centerX, (centerY + extentsY));
                //down
                else if (o == Exit.Orientation.Down)
                    createdLoc = new Vector3(centerX, (centerY - extentsY));
                //left
                else if (o == Exit.Orientation.Left)
                    createdLoc = new Vector3((centerX - extentsX), centerY);
                //assume right if none of the above
                else
                    createdLoc = new Vector3((centerX + extentsX), centerY);
                exits.Add(new Exit(createdLoc, o));
            }
            else{
                failed = true;
                reason = "Room sprite not set in inspector.";
            }
        }
        else
        {
            failed = true;
            reason = "Builder not initialized.";
        }

        return failed;
    }
    public bool CreateRoom()
    {
        bool failed = false;
        if (initialized) {
            if (exits.Count > 0
                && compatible.Count > 0
                && CheckDuplicates<PatternBuilder.Pattern.RoomType>(compatible))
            {
                editing = Instantiate(new Room(roomType, compatible, exits, roomSprite));
                initialized = false;
            }
            else if (!CheckDuplicates<PatternBuilder.Pattern.RoomType>(compatible)) {
                failed = true;
                reason = "List of compatible rooms contains duplicates.";
            }
            else if (compatible.Count <= 0) {
                failed = true;
                reason = "Room does not have compatible types set.";
            }
            else {
                failed = true;
                reason = "Room has no exits.";
            }
        }
        else
        {
            failed = true;
            reason = "Builder not initialized.";
        }
        return failed;
    }
    /*change exit location by multiplying range 'exit' var in %(.xx)*/
    //clamp values between left and right bounds for top & bottom, up/down bounds for left/right
    private void HandleExits()
    {
        foreach(Exit e in exits){
            Vector3 t = e.location;
            if (e.GetOrientation() == Exit.Orientation.Up || e.GetOrientation() == Exit.Orientation.Down){
                t.x = Mathf.Clamp((centerX - extentsX)
                    + (horizontalModification * ((centerX + extentsX) - (centerX - extentsX))), centerX - extentsX, centerX + extentsX);
                xMod = centerX - (centerX + t.x);
            }
            else if(e.GetOrientation() == Exit.Orientation.Left || e.GetOrientation() == Exit.Orientation.Right){
                t.y = Mathf.Clamp((centerY - extentsY)
                    + (verticalModification * ((centerY + extentsY) - (centerY - extentsY))), centerY - extentsY, centerY + extentsY);
                yMod = centerY - (centerY + t.y);
            }
            e.location = t;
            e.xMod = xMod;
            e.yMod = yMod;
        }
    }
    public void DebugList()
    {
        Debug.Log("Number of items in 'exits': " + exits.Count);
        Debug.Log("Full list of exits:");
        foreach(Exit e in exits)
            Debug.Log(e.ToString());
    }
    private static bool CheckDuplicates<T>(List<T> elements) { return (elements.Count == elements.Distinct().ToList().Count); }
}
#endif