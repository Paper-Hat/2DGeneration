using UnityEngine;
[System.Serializable]
public class Cell{

    public enum SpawnType { None, Enemy, Obstacle, Gun };
    public enum WallType{ None, HorizBot, HorizTop, InCornLowerLeft, 
                        InCornLowerRight, InCornUpperLeft, InCornerUpperRight,
                        OutCornLowerLeft, OutCornLowerRight, OutCornerUpperLeft,
                        OutCornerUpperRight, VertLeft, VertRight }
    [SerializeField] private SpawnType spawnType;
    [SerializeField] private WallType wallType;
    [SerializeField] private float spawnChance = 100f;
    [SerializeField] private Vector3 location;
    #region Constructor(s)
    public Cell(SpawnType s, WallType w, float chance, Vector3 loc)
    {
        spawnType = s;
        wallType = w;
        spawnChance = chance;
        location = loc;
    }
    public Cell(Cell other)
    {
        spawnType = other.GetSpawnType();
        wallType = other.GetWallType();
        spawnChance = other.GetSpawnChance();
        location = other.GetLocation();
    }

    public Cell()
    {
        spawnChance = 100f;
    }
    
    #endregion
    #region Getters/Setters
    public void SetLocation(Vector3 loc) { location = loc; }
    public SpawnType GetSpawnType() { return spawnType; }
    public WallType GetWallType(){ return wallType; }
    public float GetSpawnChance(){ return spawnChance; }
    public Vector3 GetLocation(){ return location; }
    //if set as spawn, resets wall type to none
    public void SetSpawnType(SpawnType choice)
    {
        if (choice == SpawnType.None) spawnChance = 0;
        wallType = WallType.None;
        spawnType = choice;
    }
    //if set as wall, resets enemy spawntype
    public void SetWallType(WallType choice)
    {
        spawnType = SpawnType.None;
        wallType = choice;
    }
    public void SetSpawnChance(float chance){ spawnChance = chance;}
    #endregion
    #region overrides
    public override bool Equals(object other)
    {
        var item = other as Cell;
        if (item == null)
            return false;
        return (spawnType == item.spawnType)
            && (int)spawnChance == (int)item.spawnChance
            && location == item.location;
    }
    //thanks again, stack overflow
    public override int GetHashCode()
    {
        unchecked // Overflow is fine, just wrap
        {
            int hash = 17;
            // Suitable nullity checks etc, of course :)
            hash = hash * 23 + spawnType.GetHashCode();
            hash = hash * 23 + spawnChance.GetHashCode();
            hash = hash * 23 + location.GetHashCode();
            return hash;
        }
    }

    public override string ToString()
    {
        return "Spawn Type: " + spawnType + "\n Wall Type: " + wallType
               + "\n Location: " + location + " \n Spawn Chance: " + spawnChance;
    }
    #endregion
}
