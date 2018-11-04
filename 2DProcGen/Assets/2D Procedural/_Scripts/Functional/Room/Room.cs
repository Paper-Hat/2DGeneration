using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Room : ScriptableObject
{
    [SerializeField] public Pattern.RoomType roomType;
    [SerializeField] public List<Pattern.RoomType> compatibleTypes;
    [SerializeField] public List<Exit> exits;
    [SerializeField] public Sprite roomSprite;
    [SerializeField] public Pattern pattern;
    [SerializeField] public List<V2List> colliders;
    [SerializeField] public int Weight;
    public override string ToString()
    {
        return "Type: " + roomType 
            + "\n #Compatible Types: " + compatibleTypes.Count 
            + "\n #Exits: " + exits.Count 
            + "\n #Colliders: " + colliders.Count;
    }
}
