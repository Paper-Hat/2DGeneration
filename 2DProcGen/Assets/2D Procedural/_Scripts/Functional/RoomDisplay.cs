using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomDisplay : MonoBehaviour {

    public static int counter = 0;
    public Room room;
    [SerializeField] private PatternBuilder.Pattern.RoomType type;
    [SerializeField] private List<PatternBuilder.Pattern.RoomType> cTypes;
    public List<Exit> roomExits;
    [SerializeField] private SpriteRenderer runtimeSprite;
    public PatternBuilder.Pattern pattern;
    [SerializeField] private GameObject roomColGO;

    public List<PatternBuilder.Pattern.RoomType> GetCTypes() { return cTypes; }
    public PatternBuilder.Pattern.RoomType GetRType() { return type; }
    public void SetRoom(Room r) {    room = Instantiate(r);          }

    public void Init() {
        //set room to an instantiated copy so as not to destroy the original instance
        type = room.roomType;
        cTypes = new List<PatternBuilder.Pattern.RoomType>(room.compatibleTypes);
        roomExits = new List<Exit>(room.exits);

        runtimeSprite = gameObject.GetComponent<SpriteRenderer>();
        runtimeSprite.sprite = room.roomSprite;

        //Later: set up spawning based on the pattern
        pattern = room.pattern;

        AdjustExitPositions();
        room.name = gameObject.name = "Room #" + ++counter;
        BuildColliders();
        //g.transform.parent = gameObject.transform;
    }
    private void AdjustExitPositions()
    {
        Sprite modSprite = runtimeSprite.sprite;

        foreach(Exit e in roomExits)
        {
            float newX = gameObject.transform.position.x, newY = gameObject.transform.position.y;
            if (e.GetOrientation() == Exit.Orientation.Up)
            {
                newY += modSprite.bounds.extents.y;
                newX += e.xMod;
            }
            else if (e.GetOrientation() == Exit.Orientation.Down)
            {
                newY -= modSprite.bounds.extents.y;
                newX += e.xMod;
            }
            else if (e.GetOrientation() == Exit.Orientation.Left)
            {
                newX -= modSprite.bounds.extents.x;
                newY += e.yMod;
            }
            else if (e.GetOrientation() == Exit.Orientation.Right)
            {
                newX += modSprite.bounds.extents.x;
                newY += e.yMod;
            }
            //Debug.Log("xMod: " + e.xMod + "\n yMod: " + e.yMod);
            e.location = new Vector3(newX, newY);
        }
    }
    private GameObject BuildColliders()
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

}
