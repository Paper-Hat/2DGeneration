using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Exit
{
    public enum Orientation { Up, Down, Left, Right }
    [SerializeField] public Vector3 location;
    [SerializeField] public float xMod, yMod;
    [SerializeField] private Orientation orientation;
    public Exit(Vector3 l, Orientation o)
    {
        location = l;
        orientation = o;
    }
    public bool Matches(Orientation check)
    {
        if (orientation == Orientation.Up && check == Orientation.Down
            || orientation == Orientation.Down && check == Orientation.Up
            || orientation == Orientation.Left && check == Orientation.Right
            || orientation == Orientation.Right && check == Orientation.Left)
            return true;
        else
            return false;
    }
    public void SetOrientation(Orientation o) { orientation = o; }
    public Orientation GetOrientation() { return orientation; }
    public override string ToString(){ return "Orientation: " + orientation + "\n Location: " + location; }
    #region overrides
    public override bool Equals(object other)
    {
        var item = other as Exit;
        if (item == null)
            return false;
        return (orientation == item.orientation)
            && (location == item.location);
    }
    public override int GetHashCode()
    {
        unchecked // Overflow is fine, just wrap
        {
            int hash = 17;
            // Suitable nullity checks etc, of course :)
            hash = hash * 23 + location.GetHashCode();
            hash = hash * 23 + orientation.GetHashCode();
            return hash;
        }
    }
    #endregion
}
