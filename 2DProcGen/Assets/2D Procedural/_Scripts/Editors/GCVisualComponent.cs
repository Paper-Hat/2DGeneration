
using UnityEngine;
[System.Serializable]
[ExecuteInEditMode]
public class GCVisualComponent : MonoBehaviour
{
    public GridCell CellRef;
    public void SetLocation(Vector3 loc) { CellRef.SetLocation(loc); }
    public GridCell.SpawnType GetSpawnType(){return CellRef.GetSpawnType(); }
    public GridCell.WallType GetWallType(){ return CellRef.GetWallType();}
    //if set as spawn, resets wall type to none
    public void SetSpawnType(GridCell.SpawnType choice)
    {
        if (choice == GridCell.SpawnType.None) CellRef.SetSpawnChance(0f);
        CellRef.SetWallType(GridCell.WallType.None);
        CellRef.SetSpawnType(choice);
    }
    //if set as wall, resets enemy spawntype
    public void SetWallType(GridCell.WallType choice)
    {
        CellRef.SetSpawnType(GridCell.SpawnType.None);
        CellRef.SetWallType(choice);
    }
    public void SetSpawnChance(float chance){ CellRef.SetSpawnChance(chance);}
    #region overrides
    #endregion
}
