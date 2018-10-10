using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
//TODO: (Low Priority) Clean Up/Remove extraneous code/functions
[System.Serializable]
public class Exit
{
    #region Exit Variables
    public enum Orientation { Up, Down, Left, Right }
    [SerializeField] public Vector3 location;
    [SerializeField] public float xMod, yMod;
    [SerializeField] private Orientation orientation;
    #endregion
    #region Constructor(s)
    public Exit(Vector3 l, Orientation o)
    {
        location = l;
        orientation = o;
    }
    #endregion
    #region Comparator(s)
    /// <summary>
    /// Determine whether an exit has a match or exits (only) point in directions other than towards each other.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="ao"></param>
    /// <param name="bo"></param>
    /// <returns></returns>
    public static bool ExitXOR(List<Exit> a, List<Exit> b, Exit.Orientation ao, Exit.Orientation bo)
    {
        return (a.Any(x => x.GetOrientation() == ao) && b.Any(y => y.GetOrientation() == bo))
               || (a.All(x => x.GetOrientation() != ao) && b.All(y => y.GetOrientation() != bo));
    }
    #endregion
    #region Getter/Setter(s)
    public Orientation GetOrientation() { return orientation; }
    #endregion
    #region overrides
    public override string ToString() { return "Orientation: " + orientation + "\n Location: " + location; }
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
