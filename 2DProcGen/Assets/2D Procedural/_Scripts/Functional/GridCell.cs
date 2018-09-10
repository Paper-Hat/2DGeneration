using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[ExecuteInEditMode]
[System.Serializable]
public class GridCell : MonoBehaviour {

    [SerializeField] public enum SpawnType { None, Enemy, Obstacle };
    [SerializeField] private SpawnType spawnType;
    [SerializeField] public float spawnChance;
    [SerializeField] public Vector3 location;

    public void SetLocation(Vector3 loc) { location = loc; }
    public SpawnType GetSpawnType() { return spawnType; }
    public void SetSpawnType(SpawnType choice)
    {
        if (choice == SpawnType.Obstacle)
            GetComponent<Image>().color = Color.yellow;
        else if (choice == SpawnType.Enemy)
            GetComponent<Image>().color = Color.red;
        else{
            GetComponent<Image>().color = Color.white;
            spawnChance = 0f;
        }
        spawnType = choice;
    }
    #region overrides
    public override bool Equals(object other)
    {
        var item = other as GridCell;
        if (item == null)
            return false;
        return (spawnType == item.spawnType)
            && (spawnChance == item.spawnChance)
            && (location == item.location);
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
    #endregion
}
