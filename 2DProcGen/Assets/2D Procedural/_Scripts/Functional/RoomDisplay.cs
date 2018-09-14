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
    public void Init() {
        //set room to an instantiated copy so as not to destroy the original instance
        room = Instantiate(room);
        type = room.roomType;
        cTypes = room.compatibleTypes;
        roomExits = room.exits;
        runtimeSprite.sprite = room.roomSprite;

        AdjustPositions();
        //Later: set up spawning based on the pattern
        pattern = room.pattern;
        room.name = gameObject.name = "Room #" + ++counter;
    }

    private void AdjustPositions()
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
	
}
