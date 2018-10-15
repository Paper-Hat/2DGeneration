using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
[ExecuteInEditMode]
public class FloorMap {
    #region Individual Cell Definition
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
            filled = r != null;
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
        #region Cell Function(s)
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
                return Exit.ExitXOR(prospective.exits, other.room.exits, Exit.Orientation.Left, Exit.Orientation.Right);
            if (otherX == thisX + 1 && otherY == thisY) //" " right
                return Exit.ExitXOR(prospective.exits, other.room.exits, Exit.Orientation.Right, Exit.Orientation.Left);
            if (otherX == thisX && otherY == thisY + 1) //" " up
                return Exit.ExitXOR(prospective.exits, other.room.exits, Exit.Orientation.Up, Exit.Orientation.Down);
            if (otherX == thisX && otherY == thisY - 1) //" " down
                return Exit.ExitXOR(prospective.exits, other.room.exits, Exit.Orientation.Down, Exit.Orientation.Up);
            return false;
        }
        #endregion
        #region Overrides
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
        #endregion

    }
    #endregion
    #region Map Variables/Constructor
    //Modify the size fields to activate OnValidate and create the grid
    public List<RoomCell> Cells = new List<RoomCell>();
    private Vector3 _start;
    private int _numRows = 0, _numCols = 0;
    
    /// <summary>
    /// Constructor defines dimensions of each cell (and by extension, the map) based on a "default" 1x1 cell sprite
    /// </summary>
    /// <param name="defaultRef"></param>
    public FloorMap(Sprite defaultRef)
    {
        if (defaultRef != null)
            RoomCell.dimensions = new Vector2(2 * defaultRef.bounds.extents.x, 2 * defaultRef.bounds.extents.y);
    }
    #endregion
    #region Indexer/Cell Accessor

    //Indexer allows access to list "Cells" with coordinates
    public RoomCell this[int xKey, int yKey]
    {
        get
        {
            return AccessCell(xKey, yKey);
        }
        set
        {
            //Debug.Log("Value Set.");
            int index = Cells.IndexOf(AccessCell(xKey, yKey));
            Cells[index] = value;
        }
    }
    /// <summary>
    /// Returns a cell at location, or creates empty cell if one is not found.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public RoomCell AccessCell(int x, int y)
    {
        RoomCell target;
        //If the cell already exists in the list of cells
        if(Cells.Any(c => (int)c.index.x == x && (int)c.index.y == y)){
            //assign target to found cell
            target = Cells.First(c => (int)c.index.x == x && (int)c.index.y == y);
        }
        //If the cell does not yet exist
        else{
            //create blank cell for possible override
            target = new RoomCell(new Vector2(x, y));
            Cells.Add(target);
        }

        #if UNITY_EDITOR
        DetermineVisualDimensions();
        #endif

        return target;
    }
    #endregion
    #region Getters/Adjacency

    public int Rows() { return _numCols; }
    public int Cols() { return _numRows; }

    /// <summary>
    /// Returns whether a cell is "Completed"
    /// Completion occurs when a cell is surrounded on all of it's exiting sides.
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public bool CheckCellCompletion(RoomCell c)
    {
        int fillCount = 0;
        foreach (Exit e in c.room.exits)
        {
            switch (e.GetOrientation())
            {
                case Exit.Orientation.Up:
                    if (this[(int)c.index.x, (int)c.index.y + 1].filled)
                        fillCount++;
                    break;
                case Exit.Orientation.Down:
                    if (this[(int)c.index.x, (int)c.index.y - 1].filled)
                        fillCount++;
                    break;
                case Exit.Orientation.Left:
                    if (this[(int)c.index.x - 1, (int)c.index.y].filled)
                        fillCount++;
                    break;
                case Exit.Orientation.Right:
                    if (this[(int)c.index.x + 1, (int)c.index.y].filled)
                        fillCount++;
                    break;
            }
        }
        //Debug.Log("Cell: " + c.index + "\n FillCount: " + fillCount);
        return fillCount == c.room.exits.Count;
    }

    public bool Completed()
    {
        return Cells.Where(x=>x.filled).ToList().All(CheckCellCompletion);
    }
    /// <summary>
    /// Returns list of accessed adjacent cells.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public List<RoomCell> GetAdjacentCells(Vector2 index)
    {
        RoomCell[] adj = {  this[(int)index.x - 1, (int)index.y],
                            this[(int)index.x + 1, (int)index.y],
                            this[(int)index.x, (int)index.y + 1],
                            this[(int)index.x, (int)index.y - 1] };
        return adj.ToList();
    }
    /// <summary>
    /// Returns list of accessed adjacent cells such that each accessed cell is in the direction of an exit on the cell parameter.
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public List<RoomCell> GetAdjacentWithFilter(RoomCell cell)
    {
        List<RoomCell> adj = new List<RoomCell>();
        foreach (Exit e in cell.room.exits){
            switch (e.GetOrientation())
            {
                case Exit.Orientation.Up:
                    adj.Add(this[(int) cell.index.x,(int) cell.index.y + 1]);
                    break;
                case Exit.Orientation.Down:
                    adj.Add(this[(int) cell.index.x, (int) cell.index.y - 1]);
                    break;
                case Exit.Orientation.Left:
                    adj.Add(this[(int) cell.index.x - 1, (int) cell.index.y]);
                    break;
                case Exit.Orientation.Right:
                    adj.Add(this[(int) cell.index.x + 1, (int) cell.index.y]);
                    break;
            }
        }
        return adj.ToList();
    }
    /// <summary>
    /// Similar to ContainsExitMatch, except this will be used to filter room types based on compatible types instead of matching exits
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public List<RoomCell> GetAdjacentFilterInverse(RoomCell cell)
    {
        List<RoomCell> adj = new List<RoomCell>();
        foreach (var c in GetAdjacentCells(cell.index).Where(x=> x.filled).ToList())
        {
            int cellX = (int) cell.index.x,
                cellY = (int) cell.index.y,
                compareX = (int) c.index.x,
                compareY = (int) c.index.y;
            //Up
            if (compareX == cellX && compareY == cellY + 1){
                if(c.room.exits.Any( x => x.GetOrientation() == Exit.Orientation.Down))
                    adj.Add(c);
            }
            //Down
            else if (compareX == cellX && compareY == cellY - 1){
                if (c.room.exits.Any(x => x.GetOrientation() == Exit.Orientation.Up))
                    adj.Add(c);
            }
            //Left
            else if (compareX == cellX - 1 && compareY == cellY){
                if (c.room.exits.Any(x => x.GetOrientation() == Exit.Orientation.Right))
                    adj.Add(c);
            }
            //Right
            else if (compareX == cellX + 1 && compareY == cellY){
                if (c.room.exits.Any(x => x.GetOrientation() == Exit.Orientation.Left))
                    adj.Add(c);
            }
        }
        return adj;
    }
    #endregion
    #region Editor-Related Functions
#if UNITY_EDITOR
    /// <summary>
    /// Returns number of cells containing rooms, prints ToString of each of these cells. 
    /// </summary>
    public void GetActiveCells()
    {
        List<RoomCell> activeCells = Cells.Where(x => x.filled).ToList();
        Debug.Log("Active Cells: " + activeCells.Count);
        foreach (RoomCell cell in activeCells)
            Debug.Log(cell.ToString());
    }
    /// <summary>
    /// Determines NxN grid size based on current placed cells, used for gizmo drawing.
    /// </summary>
    private void DetermineVisualDimensions()
    {
        List<int> xIndexes = new List<int>(), yIndexes = new List<int>();

        foreach(RoomCell cell in Cells){
            xIndexes.Add((int)cell.index.x);
            yIndexes.Add((int)cell.index.y);
        }

        _numCols = (xIndexes.Min() >= 0) ? xIndexes.Max() : xIndexes.Max() - xIndexes.Min();
        _numRows = (yIndexes.Min() >= 0) ? yIndexes.Max() : yIndexes.Max() - yIndexes.Min();

        _numCols = (_numCols > _numRows) ? _numCols : _numRows;
        _numRows = (_numCols > _numRows) ? _numCols : _numRows;
        if(_numCols %2 == 0){
            _numCols++;
            _numRows++;
        }
        _start =  new Vector3(   .5f * RoomCell.dimensions.x * -_numRows,
                                .5f * RoomCell.dimensions.y * _numCols,
                                 0f);
    }

    //Call in OnDrawGizmos
    public void VisualizeMap()
    {
        if (_numRows > 0 && _numCols > 0){
            Gizmos.color = Color.red;
            //Draw Horizontal Lines
            for (int i = 0; i <= _numCols; i++)
                Gizmos.DrawLine(new Vector3(_start.x, _start.y - (i * RoomCell.dimensions.y)),
                                new Vector3(_start.x + (RoomCell.dimensions.x * _numRows), _start.y - (i * RoomCell.dimensions.y)));
            //Draw Vertical Lines
            for (int j = 0; j <= _numRows; j++)
                Gizmos.DrawLine(new Vector3(_start.x + (j * RoomCell.dimensions.x), _start.y),
                                new Vector3(_start.x + (j * RoomCell.dimensions.x), _start.y - (RoomCell.dimensions.y * _numCols)));
        }
    }
    public void ClearMap()
    {
        Cells.Clear();
        _start = Vector3.zero;
        _numRows = 0;
        _numCols = 0;
        RoomCell.dimensions = new Vector2(0f, 0f);
    }
#endif
#endregion
}
