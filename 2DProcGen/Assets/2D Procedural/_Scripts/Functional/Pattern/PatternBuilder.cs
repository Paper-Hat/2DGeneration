using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
[System.Serializable]
[ExecuteInEditMode]
public class PatternBuilder : MonoBehaviour
{
    #region Room_Builder Variables
    //ScriptableObject pattern vars
    [SerializeField] public List<GridCell> placements;
    [SerializeField] public Pattern.RoomType typing;
    [SerializeField] public Pattern editing;

    //Builder-only vars
    //canvas = Canvas object, gridRef = panel object
    [SerializeField] private int columns = 10, rows = 10;
    [SerializeField] private float size = 10;
    [SerializeField] private bool hasReferenceImage;
    [SerializeField] private Sprite imgSprite;
    [SerializeField] private GameObject gridButton;
    [SerializeField] private GameObject[,] buttonGrid;
    //List<Object> roomsAsObjects = Resources.LoadAll("_ScriptableObjects/Rooms", typeof(Room)).ToList();
    private GameObject gridRef, canvas, imgRef;
    private Canvas canvasRef;
    private GridLayoutGroup glg;
    private bool filled;
    private string reason;
    private List<Pattern> allPatterns;
    #endregion
    
    #region PatternBuilder_Functionality

    public void Init()
    {
        List<Object> ptrnObjs = Resources.LoadAll("_ScriptableObjects/Patterns", typeof(Pattern)).ToList();
        allPatterns = ptrnObjs.Cast<Pattern>().ToList();
        placements = new List<GridCell>();
        Debug.Log("Current patterns in list: " + allPatterns.Count);
    }
    //Canvas and grid creators use manual object creation; this could be sped up (processor-wise) by creating scriptableobjects.
    //However, not relying on setup/prefab placement allows lesser burden of information on users.
    public void CreateCanvas()
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
    }
    public void CreateGrid()
    {
        buttonGrid = new GameObject[columns, rows];
        gridRef = new GameObject("Grid Reference");
        gridRef.AddComponent<CanvasRenderer>();
        gridRef.transform.parent = canvas.transform;
        glg = gridRef.AddComponent<GridLayoutGroup>();
        if (hasReferenceImage){
            Vector2 gridRect = glg.GetComponent<RectTransform>().sizeDelta = canvasRef.GetComponent<RectTransform>().sizeDelta;
            //Grid Square x: rect x/columns
            //Grid Square y: rect y/rows
            glg.cellSize = new Vector2(gridRect.x / columns, gridRect.y / rows);
            gridButton.GetComponent<RectTransform>().sizeDelta = glg.cellSize;
        }
        else{
            glg.cellSize = new Vector2(size, size);
            //Match the button size to the respective cell size
            gridButton.transform.localScale = new Vector3(1f, 1f, gridButton.transform.localScale.z);
        }
        glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
        glg.startAxis = GridLayoutGroup.Axis.Horizontal;
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = columns;
        glg.constraintCount = rows;
    }
    public void ClearCanvasAndGrid()
    {
        DestroyImmediate(gridRef);
        DestroyImmediate(canvas);
        filled = false;
    }
    public bool FillGrid()
    {
        bool failed = false;
        if (!filled && gridRef){
            for (int i = 0; i < columns; i++){
                for (int j = 0; j < rows; j++){
                    buttonGrid[i, j] = Instantiate(gridButton, gridRef.transform);
                }
            }
            filled = true;
        }
        else if (filled){
            failed = true;
            reason = "Grid has already been filled. Clear it before attempting to fill it again.";
        }
        else{
            failed = true;
            reason = "Grid not created.";
        }
        return failed;
    }
    
    //TODO: Compare against all current stored patterns
    public bool CreatePattern()
    {
        bool failed = false;
        if (filled){
            editing = Instantiate(ScriptableObject.CreateInstance<Pattern>());
            foreach (var g in buttonGrid){
                GCVisualComponent c = g.GetComponent<GCVisualComponent>();
                c.SetLocation(g.transform.position);
                if (c.GetSpawnType() != GridCell.SpawnType.None || c.GetWallType() != GridCell.WallType.None)
                {
                    placements.Add(new GridCell(c.CellRef));
                }
            }
            editing.Placements = new List<GridCell>(placements);
            editing.PatternType = typing;
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
            reason = "Grid not filled.";
        }
        return failed;
    }
    public bool EmptyGrid()
    {
        bool failed = false;
        if (buttonGrid == null){
            failed = true;
            reason = "Grid not created.";
        }
        else{
            foreach (GameObject g in buttonGrid)
                DestroyImmediate(g);
            filled = false;
        }
        return failed;
    }
    public bool ResetCells()
    {
        bool failed = false;
        if (buttonGrid == null){
            reason = "Grid not created.";
            failed = true;
        }
        else{
            foreach (GameObject g in buttonGrid){
                GCVisualComponent cell = g.GetComponent<GCVisualComponent>();
                cell.SetSpawnType(GridCell.SpawnType.None);
                cell.SetWallType(GridCell.WallType.None);
                cell.SetSpawnChance(0f);
            }

        }
        return failed;
    }
    public void ResetBuilder()
    {
        DestroyImmediate(gridRef);
        DestroyImmediate(canvas);
        DestroyImmediate(editing);
        filled = false;
    }

    public GameObject[,] GetButtonGrid()
    {
        return buttonGrid;
    }
    
    public string GetReason()       { return reason;}
    public GameObject GetCanvas()   { return canvas;}
    #endregion
}
#endif