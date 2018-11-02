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
    public Vector2 StartIndex { get; set; }
    [SerializeField] private int _x1, _x2, _y1, _y2;
    public void Awake()
    {
        Style = Generator.Constraints.Style.Random;
        Types = (Generator.Constraints.Types.BoundsX | Generator.Constraints.Types.BoundsY);
        XRangeConstraint = (_x1, _x2);
        YRangeConstraint = (_y1, _y2);
        Generator.Init();
        Generator.SetGenConstraints(Style, Types, NRooms, XRangeConstraint, YRangeConstraint, StartIndex);
        Debug.Log(Generator.GetConstraints().ToString());
        Generator.Generate();
    }
}
