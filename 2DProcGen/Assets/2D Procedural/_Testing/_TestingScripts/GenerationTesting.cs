using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using jkGenerator;
public class GenerationTesting : MonoBehaviour, IGenerable
{
    public Generator.Constraints.Style Style { get; set; }
    public Generator.Constraints.Types Types { get; set; }
    public int NRooms { get; set; }
    public (int, int) XRangeConstraint { get; set; } 
    public (int, int) YRangeConstraint { get; set; }
    public (int, int) StartIndex { get; set; }
    public bool Count, BoundsX, BoundsY;
    [SerializeField] private (int, int) _start;
    [SerializeField] private int _roomCount;
    [SerializeField] private int _x1, _x2, _y1, _y2;
    public void Awake()
    {
        Style = Generator.Constraints.Style.Random;
        SetTypes();
        XRangeConstraint = (_x1, _x2);
        YRangeConstraint = (_y1, _y2);
        StartIndex = _start;
        NRooms = _roomCount;
        Generator.Init();
        Generator.SetGenConstraints(Style, Types, NRooms, XRangeConstraint, YRangeConstraint, StartIndex);
        Debug.Log(Generator.GetConstraints().ToString());
        Generator.Generate();
    }

    public void SetTypes()
    {
        if (Count) Types |= Generator.Constraints.Types.RoomCount;
        if (BoundsX) Types |= Generator.Constraints.Types.BoundsX;
        if (BoundsY) Types |= Generator.Constraints.Types.BoundsY;
    }
}
