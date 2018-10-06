using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
//TODO: Add editor wrappers for visualization methodss
[System.Serializable]
[ExecuteInEditMode]
public class RoomMap {
    [System.Serializable]
    public struct RoomCell
    {
        #region Cell Variables
        public static Vector2 dimensions;
        public Vector3 cellPos;
        public Vector2 index;
        public Room room;
        public bool filled;
        #endregion
        #region Cell Constructors
        public RoomCell(Vector2 i, Room r)
        {
            index = i;
            filled = (r != null) ? true : false;
            room = r;
            cellPos = new Vector3(index.x * dimensions.x,
                                  index.y * dimensions.y, 0f);
        }
        public RoomCell(Vector2 i)
        {
            index = i;
            room = null;
            filled = false;
            cellPos = new Vector3(index.x * dimensions.x,
                                  index.y * dimensions.y, 0f);
        }
        public RoomCell(RoomCell other)
        {
            cellPos = other.cellPos;
            index = other.index;
            room = other.room;
            filled = true;
        }
        #endregion
        /// <summary>
        /// Determines if the cell compared could contain a matching room based on this cells location.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool ContainsExitMatchIn(RoomCell other, Room prospective)
        {
            int thisX = (int)this.index.x, otherX = (int)other.index.x;
            int thisY = (int)this.index.y, otherY = (int)other.index.y;
            
            if (otherX == thisX - 1 && otherY == thisY) //other cell is left
            {
                // Debug.Log("Comparing Left");
                if (prospective.exits.Any(x => x.GetOrientation() == Exit.Orientation.Left) && other.room.exits.Any(x => x.GetOrientation() == Exit.Orientation.Right))
                    return true;
                // Exact opposite case means it also "matches"
                else if (!prospective.exits.Any(x => x.GetOrientation() == Exit.Orientation.Left) && !other.room.exits.Any(x => x.GetOrientation() == Exit.Orientation.Right))
                    return true;
            }
            else if (otherX == thisX + 1 && otherY == thisY) //" " right
            {
               // Debug.Log("Comparing right.");
                if (prospective.exits.Any(x => x.GetOrientation() == Exit.Orientation.Right) && other.room.exits.Any(x => x.GetOrientation() == Exit.Orientation.Left))
                    return true;
                else if (!prospective.exits.Any(x => x.GetOrientation() == Exit.Orientation.Right) && !other.room.exits.Any(x => x.GetOrientation() == Exit.Orientation.Left))
                    return true;
            }
            else if (otherX == thisX && otherY == thisY + 1) //" " up
            {
               // Debug.Log("Comparing Upwards.");
                if (prospective.exits.Any(x => x.GetOrientation() == Exit.Orientation.Up) && other.room.exits.Any(x => x.GetOrientation() == Exit.Orientation.Down))
                    return true;
                else if (!prospective.exits.Any(x => x.GetOrientation() == Exit.Orientation.Up) && !other.room.exits.Any(x => x.GetOrientation() == Exit.Orientation.Down))
                    return true;
            }
            else if (otherX == thisX && otherY == thisY - 1) //" " down
            {
               // Debug.Log("Comparing downwards.");
                if (prospective.exits.Any(x => x.GetOrientation() == Exit.Orientation.Down) && other.room.exits.Any(x => x.GetOrientation() == Exit.Orientation.Up))
                    return true;
                else if (!prospective.exits.Any(x => x.GetOrientation() == Exit.Orientation.Down) && !other.room.exits.Any(x => x.GetOrientation() == Exit.Orientation.Up))
                    return true;

            }
            return false;
        }
        public override string ToString()
        {
            if (room != null)
            {
                return "Dimensions: (" + dimensions.x + "," + dimensions.y
                + ") \n Position: (" + cellPos.x + "," + cellPos.y + "," + cellPos.z
                + ") \n Index: (" + index.x + "," + index.y
                + ") \n Room: \n" + room.ToString()
                + "\n Filled: " + filled;
            }
            else
                return "Dimensions: (" + dimensions.x + "," + dimensions.y
                + ") \n Position: (" + cellPos.x + "," + cellPos.y + "," + cellPos.z
                + ") \n Index: (" + index.x + "," + index.y
                + ") \n Room: null \n"
                + "\n Filled: " + filled;
        }
    }

    //Modify the size fields to activate OnValidate and create the grid
    public List<RoomCell> cells = new List<RoomCell>();
    private Vector3 start;
    private int numRows = 0, numCols = 0;
    private RoomCell targ;
    public RoomMap(Sprite defaultRef)
    {
        if (defaultRef != null)
            RoomCell.dimensions = new Vector2(2 * defaultRef.bounds.extents.x, 2 * defaultRef.bounds.extents.y);
    }

    public int Rows() { return numCols; }
    public int Cols() { return numRows; }

    public RoomCell this[int xKey, int yKey]
    {
        get
        {
            return GetOrCreateCell(xKey, yKey);
        }
        set
        {
            //Debug.Log("Value Set.");
            int index = cells.IndexOf(GetOrCreateCell(xKey, yKey));
            cells[index] = value;
        }
    }

    //TODO: Fix this
    public bool CheckCellCompletion(RoomCell c)
    {
        int ct = 0;
        foreach(RoomCell check in GetAdjacentCells(c.index)){
            if (check.filled)
                ct++;
        }
        return (ct < c.room.exits.Count) ? false : true;
    }
    public RoomCell GetOrCreateCell(int x, int y)
    {
        //If the cell already exists in the list of cells
        if(cells.Any(c => c.index.x == x && c.index.y == y)){
            //assign target to found cell
            targ = cells.First(c => c.index.x == x && c.index.y == y);
        }
        //If the cell does not yet exist
        else{
            //create blank cell for possible override
            targ = new RoomCell(new Vector2(x, y));
            cells.Add(targ);
        }

        #if UNITY_EDITOR
        DetermineVisualDimensions();
        #endif

        return targ;
    }
    /// <summary>
    /// Returns list of adjacent cells, creates empty cells if they are not yet initialized.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public List<RoomCell> GetAdjacentCells(Vector2 index)
    {
        RoomCell[] adj = {  GetOrCreateCell((int)index.x - 1, (int)index.y),
                            GetOrCreateCell((int)index.x + 1, (int)index.y),
                            GetOrCreateCell((int)index.x, (int)index.y + 1),
                            GetOrCreateCell((int)index.x, (int)index.y - 1) };
        return adj.ToList();
    }
    #region Editor-Related Functions
    public void GetActiveCells()
    {
        List<RoomCell> activeCells = cells.Where(x => x.filled).ToList();
        Debug.Log("Active Cells: " + activeCells.Count);
        foreach (RoomCell cell in activeCells)
            Debug.Log(cell.ToString());
    }
    private void DetermineVisualDimensions()
    {
        List<int> xIndexes = new List<int>(), yIndexes = new List<int>();

        foreach(RoomCell cell in cells){
            xIndexes.Add((int)cell.index.x);
            yIndexes.Add((int)cell.index.y);
        }

        numCols = (xIndexes.Min() >= 0) ? xIndexes.Max() : xIndexes.Max() - xIndexes.Min();
        numRows = (yIndexes.Min() >= 0) ? yIndexes.Max() : yIndexes.Max() - yIndexes.Min();

        numCols = (numCols > numRows) ? numCols : numRows;
        numRows = (numCols > numRows) ? numCols : numRows;
        if(numCols %2 == 0){
            numCols++;
            numRows++;
        }
        start =  new Vector3(   .5f * RoomCell.dimensions.x * -numRows,
                                .5f * RoomCell.dimensions.y * numCols,
                                 0f);
    }

    //Call in OnDrawGizmos
    public void VisualizeMap()
    {
        if (numRows > 0 && numCols > 0){
            Gizmos.color = Color.red;
            //Draw Horizontal Lines
            for (int i = 0; i <= numCols; i++)
                Gizmos.DrawLine(new Vector3(start.x, start.y - (i * RoomCell.dimensions.y)),
                                new Vector3(start.x + (RoomCell.dimensions.x * numRows), start.y - (i * RoomCell.dimensions.y)));
            //Draw Vertical Lines
            for (int j = 0; j <= numRows; j++)
                Gizmos.DrawLine(new Vector3(start.x + (j * RoomCell.dimensions.x), start.y),
                                new Vector3(start.x + (j * RoomCell.dimensions.x), start.y - (RoomCell.dimensions.y * numCols)));
        }
    }
    public void ClearMap()
    {
        cells.Clear();
        start = Vector3.zero;
        numRows = 0;
        numCols = 0;
        RoomCell.dimensions = new Vector2(0f, 0f);
    }
    #endregion
}
