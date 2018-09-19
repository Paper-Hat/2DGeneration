using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class overriding array of Vector2 structs - forced serialization workaround within unity
[System.Serializable]
public class V2List{

    [SerializeField] private Vector2[] vlist;
    public Vector2[] GetValue() { return vlist; }
    public int Length           {  get { return vlist.Length; }  }
    public Vector2 this[int key]
    {
        get
        {
            return vlist[key];
        }
        set
        {
            vlist[key] = value;
        }
    }
    public V2List(Vector2[] other)
    {
        vlist = new Vector2[other.Length];
        for(int i = 0; i < vlist.Length; i++)
            vlist[i] = other[i];
    }
    public void Clear() {   Array.Clear(vlist, 0, vlist.Length);    }
    public override string ToString()
    {
        string ps = "Vector2 [" + vlist[0];
        for(int i = 1;i< vlist.Length; i++)
        {
            ps += ", " + vlist[i];
        }
        ps += "]";
        return ps;
    }
}
