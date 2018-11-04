using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomDisplay : MonoBehaviour {

    #region RoomDisplay Variables
    public static int counter = 0;
    private Room room;
    public List<Exit> roomExits;
    [SerializeField] private SpriteRenderer runtimeSprite;
    public Pattern pattern;
    [SerializeField] private GameObject roomColGO;
    #endregion
    #region Getter(s)/Setter(s)
    public void SetRoom(Room r) {    room = Instantiate(r);          }
    public Room GetRoom(){return room;}
    #endregion
    #region Room Initialization Functions
    /// <summary>
    /// Initializes GameObject with set ScriptableObject Room
    /// </summary>
    public void Init() {
        //set room to an instantiated copy so as not to destroy the original instance
        roomExits = new List<Exit>(room.exits);

        runtimeSprite = gameObject.GetComponent<SpriteRenderer>();
        runtimeSprite.sprite = room.roomSprite;

        //Later: set up spawning based on the pattern
        pattern = room.pattern;
        room.name = gameObject.name = "Room #" + ++counter;
    }
    /// <summary>
    /// Builds colliders based on ScriptableObject Room List of List of Vector2 points (List<V2List>)
    /// </summary>
    /// <returns></returns>
    public GameObject BuildColliders()
    {
        GameObject collHolder = new GameObject("Colliders Room#" + counter);
        //collHolder.transform.parent = gameObject.transform;
        for(int i = 0;i < room.colliders.Count; i++)
        {
            GameObject colliderGO = (GameObject)Instantiate(roomColGO);
            colliderGO.transform.position = gameObject.transform.position;
            colliderGO.name = "RC R" + counter;
            EdgeCollider2D coll = colliderGO.GetComponent<EdgeCollider2D>();
            coll.points = room.colliders[i].GetValue();
            colliderGO.transform.parent = collHolder.transform;
        }
        return collHolder;
    }
    #endregion
}
