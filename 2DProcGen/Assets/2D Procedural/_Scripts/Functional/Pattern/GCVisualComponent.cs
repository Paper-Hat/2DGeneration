
using UnityEngine;
[System.Serializable]
[ExecuteInEditMode]
public class GCVisualComponent : MonoBehaviour
{
    public Cell CellRef;

    public void SetLocation(Vector3 loc)
    {
        Vector3 newLoc = new Vector3(loc.x/100, loc.y/100);
        CellRef.SetLocation(newLoc);
    }
    public Cell.SpawnType GetSpawnType(){return CellRef.GetSpawnType(); }
    public Cell.WallType GetWallType(){ return CellRef.GetWallType();}
    //if set as spawn, resets wall type to none
    public void SetSpawnType(Cell.SpawnType choice)
    {
        if (choice == Cell.SpawnType.None) CellRef.SetSpawnChance(0f);
        CellRef.SetWallType(Cell.WallType.None);
        CellRef.SetSpawnType(choice);
    }
    //if set as wall, resets enemy spawntype
    public void SetWallType(Cell.WallType choice)
    {
        CellRef.SetSpawnType(Cell.SpawnType.None);
        CellRef.SetWallType(choice);
    }
    public void SetSpawnChance(float chance){ CellRef.SetSpawnChance(chance);}
    #region overrides
    #endregion
}
