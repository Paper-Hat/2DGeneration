using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class overriding array of Vector2 structs - forced serialization workaround within unity
[System.Serializable]
public class V2List{
    public Vector2[] vlist;
    public int Length
    {
        get
        {
            return vlist.Length;
        }
    }
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
}
