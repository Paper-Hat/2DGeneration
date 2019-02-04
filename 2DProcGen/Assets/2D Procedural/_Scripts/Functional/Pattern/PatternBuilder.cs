using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using jkGenerator;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
[System.Serializable]
[ExecuteInEditMode]
public class PatternBuilder : MonoBehaviour
{
    #region Room_Builder Variables
    //ScriptableObject pattern vars
    [SerializeField] public List<Cell> placements;
    [SerializeField] public Pattern.RoomType typing;
    [SerializeField] public Pattern editing;


    [SerializeField] private int subdivisions; //columns = 10, rows = 10;
    [SerializeField] private float size;
    [SerializeField] private Sprite imgSprite;
    [SerializeField] private JKGrid grid;
    public bool allowDisplay;
    private GameObject patternBuilderObj, backdropObj;
    private Canvas canvasRef;
    private string reason;
    private List<Pattern> allPatterns;
    #endregion
    
    #region PatternBuilder_Functionality

    public void Init()
    {
        placements = new List<Cell>();
        allPatterns = Resources.LoadAll("_ScriptableObjects/Patterns").Cast<Pattern>().ToList();
        Debug.Log("Current # of patterns in list: " + allPatterns.Count);
        patternBuilderObj = new GameObject("Pattern Builder");
        patternBuilderObj.transform.parent = gameObject.transform;
    }

    //TODO: Fix backdrop creation (NECESSARY)
    void CreateBackdrop()
    {
        if (!backdropObj)
        {
            backdropObj = new GameObject("RoomBackdrop");
            SpriteRenderer rp = backdropObj.AddComponent<SpriteRenderer>();
            rp.sprite = imgSprite;
            backdropObj.transform.parent = gameObject.transform;
            rp.size = new Vector2(grid.GetSize(), grid.GetSize());
            rp.sortingOrder = -1;

        }
    }
    public void CreateGrid()
    {
        if (imgSprite != null)
            CreateBackdrop();
        grid = imgSprite ? new JKGrid(imgSprite, subdivisions, patternBuilderObj.transform.position, patternBuilderObj) : new JKGrid(size, subdivisions, patternBuilderObj.transform.position, patternBuilderObj);

    }
    
    public bool SetGridObject((int, int) index, GameObject gridObject, float spawnChance)
    {
        bool failed = false;
        if (patternBuilderObj)
        {
            grid.Set(index.Item1, index.Item2, gridObject);
            Cell temp = new Cell(gridObject, spawnChance, grid.GetIndexes()[index.Item1, index.Item2].Item2);
            if (!placements.Any(x => Equals(x, temp)) && gridObject != null)
                placements.Add(temp);
        }
        else
        {
            failed = true;
            reason = "Grid not created.";
        }
        return failed;

    }
    
    public bool CreatePattern()
    {
        List<Object> ptrnObjs = Resources.LoadAll("_ScriptableObjects/Patterns", typeof(Pattern)).ToList();
        allPatterns = ptrnObjs.Cast<Pattern>().ToList();
        bool failed = false;
        if (patternBuilderObj){
            editing = Instantiate(ScriptableObject.CreateInstance<Pattern>());
            editing.Placements = new List<Cell>(placements);
            editing.roomType = typing;
            editing.scale = new Vector3(grid.GetSize() / subdivisions, grid.GetSize() / subdivisions);
            if(allPatterns.Count > 0){
                foreach (var p in allPatterns){
                    if (editing.Equals(p)){
                        failed = true;
                        reason = "Pattern exists.";
                    }
                }
            }
        }
        else {
            failed = true;
            reason = "Pattern Builder Object nonexistent.";
        }
        return failed;
    }
    public bool EmptyGrid()
    {
        bool failed = false;
        if (grid == null){
            failed = true;
            reason = "Grid not created.";
        }
        else{
            grid.RemoveAll();
        }
        return failed;
    }
    public void ResetBuilder()
    {
        DestroyImmediate(backdropObj);
        DestroyImmediate(patternBuilderObj);
        if(grid != null)
            EmptyGrid();
        //filled = false;
    }
    public JKGrid GetGrid()
    {
        return grid;
    }
    public GameObject GetBuilder()
    {
        return patternBuilderObj;
    }
    public string GetReason()       { return reason;}
    #endregion
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (allowDisplay)
        {
            foreach (var element in grid.GetIndexes())
                Gizmos.DrawSphere(element.Item2, grid.GetCellSize()*.1f);
            //Horizontal
            Gizmos.DrawLine(
                new Vector3(grid.GetWorldPos().x - .5f * grid.GetSize(), grid.GetWorldPos().y + .5f * grid.GetSize()),
                new Vector3(grid.GetWorldPos().x + .5f * grid.GetSize(), grid.GetWorldPos().y + .5f * grid.GetSize()));
            Gizmos.DrawLine(
                new Vector3(grid.GetWorldPos().x - .5f * grid.GetSize(), grid.GetWorldPos().y - .5f * grid.GetSize()),
                new Vector3(grid.GetWorldPos().x + .5f * grid.GetSize(), grid.GetWorldPos().y - .5f * grid.GetSize()));
            //Vertical
            Gizmos.DrawLine(
                new Vector3(grid.GetWorldPos().x - .5f * grid.GetSize(), grid.GetWorldPos().y + .5f * grid.GetSize()),
                new Vector3(grid.GetWorldPos().x - .5f * grid.GetSize(), grid.GetWorldPos().y - .5f * grid.GetSize()));
            Gizmos.DrawLine(
                new Vector3(grid.GetWorldPos().x + .5f * grid.GetSize(), grid.GetWorldPos().y + .5f * grid.GetSize()),
                new Vector3(grid.GetWorldPos().x + .5f * grid.GetSize(), grid.GetWorldPos().y - .5f * grid.GetSize()));
        }
    }
}
#endif