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
        Debug.Log("Costco Price (subtract from total): " +
                  (3.49 + 16.99 + 11.89 + 7.49 + 10.79 + 2.80 + 12.89 + 1.22 + (.5*4.49) + (.5*13.99) + (.5* 8.39) + (.5*17.61)));
    }

    public void SetTypes()
    {
        if (Count) Types |= Generator.Constraints.Types.RoomCount;
        if (BoundsX) Types |= Generator.Constraints.Types.BoundsX;
        if (BoundsY) Types |= Generator.Constraints.Types.BoundsY;
    }
}
