
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
    public GameObject GetSpawn(){return CellRef.GetSpawn(); }
    public void SetSpawn(GameObject spawn)
    {
        if (spawn == null) CellRef.SetSpawnChance(0f);
        CellRef.SetSpawn(spawn);
    }
    public void SetSpawnChance(float chance){ CellRef.SetSpawnChance(chance);}
    #region overrides
    #endregion
}
