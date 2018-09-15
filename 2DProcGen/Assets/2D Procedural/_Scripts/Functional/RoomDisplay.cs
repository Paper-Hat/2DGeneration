using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class RoomDisplay : MonoBehaviour {

    public static int counter = 0;
    [SerializeField] public Room room;
    [SerializeField] public PatternBuilder.Pattern.RoomType type;
    [SerializeField] public List<PatternBuilder.Pattern.RoomType> cTypes;
    [SerializeField] public List<Exit> roomExits;
    [SerializeField] public SpriteRenderer runtimeSprite;
    [SerializeField] public PatternBuilder.Pattern pattern;
    [SerializeField] public GameObject roomColGO;
    public List<V2List> cpts;
    [SerializeField] private List<EdgeCollider2D> edgeColliders;
    public void Init() {
        //set room to an instantiated copy so as not to destroy the original instance
        room = Instantiate(room);
        type = room.roomType;
        cTypes = room.compatibleTypes;
        roomExits = room.exits;
        runtimeSprite.sprite = room.roomSprite;
        cpts = room.collPoints;

        //Later: set up spawning based on the pattern
        pattern = room.pattern;

        AdjustExitPositions();
        BuildColliders();
        room.name = gameObject.name = "Room #" + ++counter;
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

    private void AdjustColliderPoints()
    {

    }
    private void BuildColliders()
    {
        foreach(V2List vArr in cpts){
            EdgeCollider2D coll = ((GameObject)Instantiate(roomColGO, gameObject.transform)).GetComponent<EdgeCollider2D>();
            coll.points = vArr.vlist;
            edgeColliders.Add(coll);
        }
    }

}
