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

    //Builder-only vars
    //canvas = Canvas object, gridRef = panel object
    [SerializeField] private int subdivisions; //columns = 10, rows = 10;
    //[SerializeField] private float size = 10;
    [SerializeField] private float size;
    //[SerializeField] private bool hasReferenceImage;
    [SerializeField] private Sprite imgSprite;

    public bool allowDisplay;
    //[SerializeField] private GameObject gridButton;
    //[SerializeField] private GameObject[,] buttonGrid;
    //List<Object> roomsAsObjects = Resources.LoadAll("_ScriptableObjects/Rooms", typeof(Room)).ToList();
    [SerializeField] private JKGrid grid;
    private GameObject patternBuilderObj;//, gridRef, canvas, imgRef;
    private Canvas canvasRef;
    //private GridLayoutGroup glg;
    //private bool filled;
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
    //Canvas and grid creators use manual object creation; this could be sped up (processor-wise) by creating scriptableobjects.
    //However, not relying on setup/prefab placement allows lesser burden of information on users.
    /*public void CreateCanvas()
    {
        canvas = new GameObject("Canvas");
        canvasRef = canvas.AddComponent<Canvas>();
        canvas.AddComponent<CanvasScaler>();
        canvas.AddComponent<GraphicRaycaster>();
        canvasRef.renderMode = RenderMode.WorldSpace;
        canvasRef.pixelPerfect = true;
        if(hasReferenceImage){
            imgRef = new GameObject("Reference Image");
            imgRef.transform.parent = canvas.transform;
            Image img = imgRef.AddComponent<Image>();
            img.sprite = imgSprite;
            img.SetNativeSize();
            var canvasRect = canvasRef.GetComponent<RectTransform>();
            var imgRect = img.GetComponent<RectTransform>();
            //Image rectangle transform will always be shaped to a square
            if(imgRect.sizeDelta.x > imgRect.sizeDelta.y)
                canvasRect.sizeDelta = new Vector2(imgRect.sizeDelta.x, imgRect.sizeDelta.x);
            else if(imgRect.sizeDelta.x < imgRect.sizeDelta.y)
                canvasRect.sizeDelta = new Vector2(imgRect.sizeDelta.y, imgRect.sizeDelta.y);
            else
                canvasRect.sizeDelta = imgRect.sizeDelta;
        }
    }*/
    public void CreateGrid()
    {
        grid = imgSprite ? new JKGrid(imgSprite, subdivisions, patternBuilderObj.transform.position, patternBuilderObj) : new JKGrid(size, subdivisions, patternBuilderObj.transform.position, patternBuilderObj);
    }
    public bool SetGridObject((int, int) index, GameObject gridObject, float spawnChance)
    {
        bool failed = false;
        if (patternBuilderObj)
        {
            grid.Set(index.Item1, index.Item2, gridObject);
            Cell temp = new Cell(gridObject, spawnChance, grid.GetIndexes()[index.Item1, index.Item2].Item2);
            if (!placements.Any(x => Equals(x, temp)))
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
            //filled = false;
        }
        return failed;
    }
    public void ResetBuilder()
    {
        DestroyImmediate(patternBuilderObj);
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
                Gizmos.DrawSphere(element.Item2, .5f);
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