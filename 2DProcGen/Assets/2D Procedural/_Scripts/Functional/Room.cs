using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Room : ScriptableObject
{
    [SerializeField] public PatternBuilder.Pattern.RoomType roomType;
    [SerializeField] public List<PatternBuilder.Pattern.RoomType> compatibleTypes;
    [SerializeField] public List<Exit> exits;
    [SerializeField] public Sprite roomSprite;
    [SerializeField] public PatternBuilder.Pattern pattern;
    public Room(PatternBuilder.Pattern.RoomType rT, List<PatternBuilder.Pattern.RoomType> cT, List<Exit> e, Sprite rS)
    {
        roomType = rT;
        compatibleTypes = cT;
        exits = e;
        roomSprite = rS;
    }
}
