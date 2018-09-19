﻿using System.Collections;
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
    [SerializeField] public List<V2List> colliders;
}
