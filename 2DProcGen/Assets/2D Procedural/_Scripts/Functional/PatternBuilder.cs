using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
[ExecuteInEditMode]
public class PatternBuilder : MonoBehaviour
{
    #region Room_Builder Variables
    [SerializeField] private int columns = 10, rows = 10;
    [SerializeField] private float size = 10;
    [SerializeField] private bool hasReferenceImage;
    [SerializeField] private Sprite imgSprite;
    [SerializeField] private GameObject gridButton;
    [SerializeField] private GameObject[,] buttonGrid;
    public List<Vector3> enemySpawns, obstacleSpawns;
    public List<Pattern> patternList = new List<Pattern>();
    public Pattern.RoomType typing;

    //canvas = Canvas object, gridRef = panel object
    private GameObject gridRef, canvas, imgRef, cntnr;
    private Canvas canvasRef;
    private GridLayoutGroup glg;
    private bool filled;
    private string reason, prefabFilePath = "_Prefabs/PatternContainerPrefabs/PatternContainer";
    #endregion
    #region Struct(s)
    [System.Serializable]
    public struct Pattern
    {
        public enum RoomType { Square, Rectangle, L_Shape };
        public List<Vector3> eSpawns, oSpawns;
        public RoomType patternType;
        public override bool Equals(object other)
        {
            if (!(other is Pattern))
                return false;
            Pattern ptrn = (Pattern)other;
            return (eSpawns.SequenceEqual<Vector3>(ptrn.eSpawns))
                && (oSpawns.SequenceEqual<Vector3>(ptrn.oSpawns))
                && (patternType == ptrn.patternType);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + eSpawns.GetHashCode();
                hash = hash * 23 + oSpawns.GetHashCode();
                hash = hash * 23 + patternType.GetHashCode();
                return hash;
            }
        }
        public Pattern(List<Vector3> es, List<Vector3> os, RoomType type)
        {
            eSpawns = es;
            oSpawns = os;
            patternType = type;
        }
    }
    #endregion
    #region PatternBuilder_Functionality
    //Canvas and grid creators use manual object creation; this could be sped up (processor-wise) by creating prefabs.
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
            RectTransform canvasRect = canvasRef.GetComponent<RectTransform>();
            RectTransform imgRect = img.GetComponent<RectTransform>();
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
                    #region Fully Scripted Object Creation (deprecated)
                    /*GameObject tempRef;
                    tempRef = buttonGrid[i, j] = Instantiate(gridButton, gridRef.transform);
                    tempRef.AddComponent<CanvasRenderer>();
                    Button gBtnFunct = tempRef.AddComponent<Button>();
                    Image buttonImg = tempRef.AddComponent<Image>();
                    GridCell cell = tempRef.AddComponent<GridCell>();
                    gBtnFunct.targetGraphic = buttonImg;
                    buttonImg.sprite = unselected;
                    UnityAction<GameObject> action = new UnityAction<GameObject>(OnButtonClick);
                    UnityEventTools.AddObjectPersistentListener<GameObject>(gBtnFunct.onClick, action, tempRef);*/
                    #endregion
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
    public bool CreatePattern()
    {
        bool failed = false;
        if (filled){
            enemySpawns = new List<Vector3>();
            obstacleSpawns = new List<Vector3>();
            foreach (GameObject g in buttonGrid){
                GridCell c = g.GetComponent<GridCell>();
                c.SetLocation(g.transform.localPosition);
                if (c.GetSpawnType() == GridCell.SpawnType.Enemy) { enemySpawns.Add(c.location); }
                else if (c.GetSpawnType() == GridCell.SpawnType.Obstacle) { obstacleSpawns.Add(c.location); }
            }
            Pattern toAdd = new Pattern(enemySpawns, obstacleSpawns, typing);
            foreach (Pattern p in patternList){      
                if (toAdd.Equals(p)){
                    failed = true;
                    reason = "Pattern Exists.";
                }
            }
            if (!failed)
                patternList.Add(toAdd);

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
            enemySpawns = null;
            obstacleSpawns = null;
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
                GridCell cell = g.GetComponent<GridCell>();
                cell.SetSpawnType(GridCell.SpawnType.None);
                cell.spawnChance = 0f;
            }
            enemySpawns = null;
            obstacleSpawns = null;
        }
        return failed;
    }
    public void ResetBuilder()
    {
        DestroyImmediate(gridRef);
        DestroyImmediate(canvas);
        DestroyImmediate(cntnr);
        ClearStoredPatterns();
        enemySpawns = null;
        obstacleSpawns = null;
        filled = false;
    }
    public bool SavePatterns()
    {
        bool failed = false;
        if (patternList != null && patternList.Count > 0){
            if (cntnr)
                DestroyImmediate(cntnr, false);
            cntnr = Instantiate(Resources.Load(prefabFilePath) as GameObject, gameObject.transform);
            cntnr.GetComponent<PatternContainer>().Setup(patternList);
        }
        else{
            failed = true;
            reason = "No patterns to save.";
        }
        return failed;
    }
    public bool ClearStoredPatterns()
    {
        bool failed = false;
        if (patternList != null && patternList.Count > 0)
            patternList.Clear();
        else {
            failed = true;
            reason = "No patterns to clear.";
        }
        return failed;
    }
    public string GetReason()       { return reason;}
    public string GetFilePath()     { return prefabFilePath; }
    public GameObject GetContainer(){ return cntnr; }
    public GameObject GetCanvas()   { return canvas;}
    public bool ListEquals(List<Pattern> list1, List<Pattern> list2){   return list1.SequenceEqual<Pattern>(list2); }
    #endregion
}
#endif