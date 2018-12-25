using UnityEngine;
[System.Serializable]
public class Cell
{

    [SerializeField] private GameObject spawn;
    [SerializeField] private float spawnChance = 100f;
    [SerializeField] private Vector3 location;
    #region Constructor(s)
    public Cell(float chance, Vector3 loc)
    {
        spawnChance = chance;
        location = loc;
    }
    public Cell(Cell other)
    {
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
    public float GetSpawnChance(){ return spawnChance; }
    public Vector3 GetLocation(){ return location; }
    //if set as spawn, resets wall type to none
    public void SetSpawn(GameObject go)
    {
        if (go == null) spawnChance = 0f;
        spawn = go;
    }
    public GameObject GetSpawn(){    return spawn; }
    public void SetSpawnChance(float chance){ spawnChance = chance;}
    #endregion
    #region overrides
    public override bool Equals(object other)
    {
        var item = other as Cell;
        if (item == null)
            return false;
        return (spawn == item.spawn)
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
            hash = hash * 23 + spawn.GetHashCode();
            hash = hash * 23 + spawnChance.GetHashCode();
            hash = hash * 23 + location.GetHashCode();
            return hash;
        }
    }

    public override string ToString()
    {
        return "Spawning: " + spawn.name + "\n Location: " + location + " \n Spawn Chance: " + spawnChance;
    }
    #endregion
}
