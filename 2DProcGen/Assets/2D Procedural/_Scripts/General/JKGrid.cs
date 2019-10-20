using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Xml.Linq;
using UnityEngine.UI;

#if UNITY_EDITOR
namespace jkGenerator
{
    //TODO: replace 2d grid object array in patternbuilder, have it use this instead.
    [ExecuteInEditMode]
    [System.Serializable]
    public class JKGrid
    {
        private float gridSize, cellSize; //Squares are used; no need to differentiate between width/height
        private int subdivisions;
        private (GameObject, Vector2)[,] index;
        private Vector2 worldSpacePos;
        private GameObject parentObj;
        public JKGrid(float size, int subdivisions, Vector2 centerPos, GameObject attachedObj)
        {
            gridSize = size;
            Debug.Log("Grid Size: " + gridSize);
            this.subdivisions = subdivisions;
            cellSize = gridSize / subdivisions;
            index = new (GameObject, Vector2)[subdivisions, subdivisions];
            worldSpacePos = centerPos;
            parentObj = attachedObj;
            Initialize();
        }

        public JKGrid(Image img, int subdivisions, Vector2 centerPos, GameObject attachedObj)
        {
            gridSize = img.sprite.rect.width;
            Debug.Log("Grid Size: " + gridSize);
            this.subdivisions = subdivisions;
            cellSize = gridSize / subdivisions;
            index = new (GameObject, Vector2)[subdivisions, subdivisions];
            worldSpacePos = centerPos;
            parentObj = attachedObj;
            Initialize();
        }
        
        public JKGrid(Sprite sprite, int subdivisions, Vector2 centerPos, GameObject attachedObj)
        {
            gridSize = sprite.bounds.size.x;
            Debug.Log("Grid Size: " + gridSize);
            this.subdivisions = subdivisions;
            cellSize = gridSize / subdivisions;
            index = new (GameObject, Vector2)[subdivisions, subdivisions];
            worldSpacePos = centerPos;
            parentObj = attachedObj;
            Initialize();
        }
        /// <summary>
        /// Initialize Grid with empty Vector2 coordinates beginning at top left
        /// </summary>
        private void Initialize()
        {
            //First "Cell" position
            float initialX = (worldSpacePos.x - .5f * gridSize) + (.5f * cellSize);
            float initialY = (worldSpacePos.y + .5f *gridSize) - (.5f * cellSize);
            Vector2 position = new Vector2(initialX, initialY);
            //initialize all grid positions
            for (int i = 0; i < index.GetLength(0); i++){
                for (int j = 0; j < index.GetLength(1); j++){
                    index[i, j].Item2 = new Vector2(position.x + (cellSize * i), position.y - (cellSize * j));
                }
            }

        }
        
        /// <summary>
        /// Create object at first available grid location, traversing from top left and iterating by row
        /// </summary>
        /// <param name="g"></param>
        public void Insert(GameObject g)
        {
            for(int i = 0;i < index.GetLength(0); i++){
                for (int j = 0; j < index.GetLength(1); j++){
                    if (index[i, j].Item1 == null){
                        index[i, j].Item1 = Object.Instantiate(g, index[i, j].Item2, Quaternion.identity, parentObj.transform);
                        return;
                    }
                }
            }
            Debug.Log("Could not insert! No empty space available.");
        }
        /// <summary>
        /// Set grid position by instantiating gameobject at integer x, y coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="g"></param>
        public void Set(int x, int y, GameObject g)
        {
            if(index[x, y].Item1 != null)
                Object.DestroyImmediate(index[x, y].Item1);
            if (g == null)
                return;
            index[x, y].Item1 = Object.Instantiate(g, index[x, y].Item2, Quaternion.identity, parentObj.transform);
            index[x, y].Item1.transform.localScale = new Vector3(cellSize, cellSize, 0f);
        }
        
        /// <summary>
        /// Set grid position by instantiating gameobject at vector2 coordinates
        /// </summary>
        /// <param name="location"></param>
        /// <param name="g"></param>
        public void Set(Vector2 location, GameObject g)
        {
            for(int i = 0;i < index.GetLength(0); i++){
                for (int j = 0; j < index.GetLength(1); j++){
                    if (index[i, j].Item2 == location){
                        if (index[i, j].Item1 != null) Object.DestroyImmediate(index[i, j].Item1);
                        if (g == null)
                            return;
                        index[i, j].Item1 = Object.Instantiate(g, index[i, j].Item2, Quaternion.identity, parentObj.transform);
                        return;
                    }
                }
            }
        }
        /// <summary>
        /// Remove object set at position using given x, y coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Remove(int x, int y)
        {
            if(index[x, y].Item1 != null)
                Object.DestroyImmediate(index[x, y].Item1);
            else
                Debug.Log("Nothing to remove at location given.");
        }
        /// <summary>
        /// Remove object set at position using Vector2 location
        /// TODO: use approx. position to prevent floating point comparison issue(s)
        /// </summary>
        /// <param name="location"></param>
        public void Remove(Vector2 location)
        {
            for(int i = 0;i < index.GetLength(0); i++){
                for (int j = 0; j < index.GetLength(1); j++){
                    if (index[i, j].Item2 == location){
                        if (index[i, j].Item1 != null) Object.DestroyImmediate(index[i, j].Item1);
                        return;
                    }
                }
            }
            Debug.Log("Either location does not exist, or there was nothing to remove.");
        }
        
        /// <summary>
        /// Remove EVERYTHING from the grid
        /// </summary>
        public void RemoveAll()
        {
            if (index != null){
                foreach (var element in index) {
                    if (element.Item1 != null)
                        Object.DestroyImmediate(element.Item1);
                    element.Item2.Set(0f, 0f);
                }
            }
        }
        #region Getter/Setters
        
        public Vector2 GetWorldPos(){ return worldSpacePos; }

        public void SetSize(float s){ gridSize = s; }
        public float GetSize(){ return gridSize; }

        public float GetCellSize(){ return cellSize; }
        
        public List<GameObject> GetObjects()
        {
            List<GameObject> gameObjects = new List<GameObject>();
            foreach (var element in index)
            {
                if(element.Item1 != null)
                    gameObjects.Add(element.Item1);
            }
            return gameObjects;
        }
        public (GameObject, Vector2)[,] GetIndexes(){ return index; }
        
        #endregion
        
        /// <summary>
        /// Print GameObject names with Vector2 locations in grid
        /// </summary>
        /// <returns></returns>
        string PrintGrid()
        {
            string g = "";
            for (int i = 0; i < index.GetLength(0); i++){
                for (int j = 0; j < index.GetLength(1); j++){
                    g += "( " + i + " , " + j + " ): "+ index[i, j].Item1 + " at location " + index[i, j].Item2;
                }
            }
            return g;
        }

        public override string ToString()
        {
            return "Grid Size: " + gridSize + "\n" +
                   "Cell Size: " + cellSize + "\n" +
                   PrintGrid();
        }
        
    }
}
#endif