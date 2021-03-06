﻿using System;
using System.Collections.Generic;
using jkGenerator;
using UnityEngine;
[System.Serializable]
public class Pattern : ScriptableObject
{
    public enum RoomType { Sq, Rect, L, End, T };
    [SerializeField] public List<Cell> Placements;
    [SerializeField] public RoomType roomType;
    [SerializeField] public Vector3 scale;
    public override bool Equals(object other)
    {
        if (!(other is Pattern))
            return false;
        var ptrn = (Pattern)other;
        return (UsefulFunctions.ListEquals(Placements, ptrn.Placements)
               && (roomType == ptrn.roomType)
               && (scale == ptrn.scale));
    }
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + Placements.GetHashCode();
            hash = hash * 23 + roomType.GetHashCode();
            hash = hash * 23 + scale.GetHashCode();
            return hash;
        }
    }
}
