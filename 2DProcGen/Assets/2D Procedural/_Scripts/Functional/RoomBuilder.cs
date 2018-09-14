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
    [Header("-------Room Information-------")]
    [Space(10)]
    [SerializeField] public PatternBuilder.Pattern.RoomType roomType;
    [SerializeField] public List<PatternBuilder.Pattern.RoomType> compatible;
    [SerializeField] public GameObject spriteHolder;
    [SerializeField] public Room editing;
    private Sprite roomSprite;
    private float extentsX, centerX, extentsY, centerY;
    [Space(10)]

    [Header("-------Exit Information-------")]
    [Space(10)]
    [Range(0f, 1f)] [SerializeField] private float horizontalModification = .5f;
    [Range(0f, 1f)] [SerializeField] private float verticalModification = .5f;
    [SerializeField] public List<Exit> exits = new List<Exit>();
    private float xMod = 0f, yMod = 0f;
    [Space(10)]

    [Header("-------Collider Creation Information-------")]
    [Space(10)]
    [SerializeField] public Vector2 addColliderPoint = new Vector2();
    private int colliderCount = 0;
    [SerializeField] private List<Vector2> currColPoints = new List<Vector2>();
    private Dictionary<int, Vector2[]> edgeColliders = new Dictionary<int, Vector2[]>();
    [Range(0f, 1f)] [SerializeField] private float addX = .5f;
    [Range(0f, 1f)] [SerializeField] private float addY = .5f;

    [Space(10)]

    [Header("-------Debug Information-------")]
    [Space(10)]
    public bool initialized;
    private string reason;

    /*Get extents on either side of image bounds, subtract or add extents on x or y
    dependent on where the exit is on the image */
    public string GetReason() { return reason; }
    public bool Init()
    {
        bool failed = false;
        if (!initialized) {
            roomSprite = spriteHolder.GetComponent<SpriteRenderer>().sprite;
            if (roomSprite) {
                initialized = true;
                extentsX = roomSprite.bounds.extents.x;
                centerX = roomSprite.bounds.center.x;
                extentsY = roomSprite.bounds.extents.y;
                centerY = roomSprite.bounds.center.y;
                xMod = 0f;
                yMod = 0f;
            }
            else {
                failed = true;
                reason = "Room sprite not set in inspector.";
            }
        }
        else {
            failed = true;
            reason = "Builder already initialized.";
        }
        return failed;
    }
    private void OnValidate()
    {
        if (exits.Count > 0)
            HandleExits();
        HandleColPts();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (exits.Count > 0)
            foreach (Exit e in exits)
                Gizmos.DrawSphere(e.location, .05f);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(addColliderPoint, .05f);
        if (edgeColliders.Count > 0) {
            foreach (KeyValuePair<int, Vector2[]> edge in edgeColliders) {
                for (int i = 0; i < edge.Value.Length - 1; i++)
                    Gizmos.DrawLine(edge.Value[i], edge.Value[i + 1]);
            }
        }
    }
    public bool CreateExit(Exit.Orientation o)
    {
        bool failed = false;
        if (initialized) {
            if (roomSprite) {
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
            else {
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
    #region Collider_Creation
    public void AddColliderPoint(){     currColPoints.Add(addColliderPoint);    }
    public bool CompleteCollider()
    {
        bool failed = false;
        if (currColPoints.Count < 2) {
            failed = true;
            reason = "Not enough points to create an edge collider.";
        }
        else {
            edgeColliders.Add(colliderCount++, currColPoints.ToArray());
            currColPoints.Clear();
        }
        return failed;
    }
    public bool ClearPoints()
    {
        bool failed = false;
        if (currColPoints.Count < 1)
        {
            failed = true;
            reason = "No points to clear.";
        }
        else
            currColPoints.Clear();
        return failed;
    }
    public bool ClearDict()
    {
        bool failed = false;
        if (edgeColliders.Count < 1)
        {
            failed = true;
            reason = "No edges set.";
        }
        else
            edgeColliders.Clear();
        return failed;
    }
    #endregion

    /*change exit location by multiplying range 'exit' var in %(.xx)*/
    //clamp values between left and right bounds for top & bottom, up/down bounds for left/right
    private void HandleExits()
    {
        foreach (Exit e in exits) {
            Vector3 t = e.location;
            if (e.GetOrientation() == Exit.Orientation.Up || e.GetOrientation() == Exit.Orientation.Down) {
                t.x = Mathf.Clamp((centerX - extentsX)
                    + (horizontalModification * ((centerX + extentsX) - (centerX - extentsX))), centerX - extentsX, centerX + extentsX);
                xMod = centerX - (centerX + t.x);
            }
            else if (e.GetOrientation() == Exit.Orientation.Left || e.GetOrientation() == Exit.Orientation.Right) {
                t.y = Mathf.Clamp((centerY - extentsY)
                    + (verticalModification * ((centerY + extentsY) - (centerY - extentsY))), centerY - extentsY, centerY + extentsY);
                yMod = centerY - (centerY + t.y);
            }
            e.location = t;
            e.xMod = xMod;
            e.yMod = yMod;
        }
    }
    private void HandleColPts()
    {
        Vector2 t = addColliderPoint;
        t.x = Mathf.Clamp((centerX - extentsX) + addX * ((centerX + extentsX) - (centerX - extentsX)), (centerX - extentsX), centerX + extentsX);
        t.y = Mathf.Clamp((centerY - extentsY) + addY * ((centerY + extentsY) - (centerY - extentsY)), (centerY - extentsY), centerY + extentsY);
        addColliderPoint = t;
    }
    #region Debugging
    public void DebugList()
    {
        Debug.Log("Number of items in 'exits': " + exits.Count);
        Debug.Log("Full list of exits:");
        foreach(Exit e in exits)
            Debug.Log(e.ToString());
    }
    public void DebugDict()
    {
        if (edgeColliders.Count > 0){
            foreach (KeyValuePair<int, Vector2[]> edge in edgeColliders){
                Debug.Log("Edge: " + edge.Key);
                Vector2[] iterArr = edge.Value;
                for (int i = 0; i < iterArr.Length; i++)
                    Debug.Log("Point " + i + ":" + iterArr[i].ToString());
            }
        }
    }
    private static bool CheckDuplicates<T>(List<T> elements) { return (elements.Count == elements.Distinct().ToList().Count); }
    #endregion
}
#endif