using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
[ExecuteInEditMode]
public class Map {
    //TODO: (refactor) change all "indexing" variables to (int, int) instead of cast Vector2
    #region Individual Cell Definition
    [System.Serializable]
    public struct Node
    {
        #region Cell Variables
        public static Vector2 dimensions;
        public Vector3 cellPos;
        public (int, int) index;
        public Room room;
        public bool filled;
        #endregion
        #region Cell Constructors
        public Node((int, int) i, Room r)
        {
            index = i;
            filled = r != null;
            room = r;
            cellPos = new Vector3(index.Item1 * dimensions.x,
                                  index.Item2 * dimensions.y, 0f);
        }
        public Node((int, int) i)
        {
            index = i;
            room = null;
            filled = false;
            cellPos = new Vector3(index.Item1 * dimensions.x,
                                  index.Item2 * dimensions.y, 0f);
        }
        public Node(Node other)
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
        public bool ContainsExitMatchIn(Node other, Room prospective)
        {
            int thisX = (int)this.index.Item1, otherX = (int)other.index.Item1;
            int thisY = (int)this.index.Item2, otherY = (int)other.index.Item2;
            
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
                + ") \n Index: (" + index.Item1 + "," + index.Item2
                + ") \n Room: \n" + room.ToString()
                + "\n Filled: " + filled;
            }
            else
                return "Dimensions: (" + dimensions.x + "," + dimensions.y
                + ") \n Position: (" + cellPos.x + "," + cellPos.y + "," + cellPos.z
                + ") \n Index: (" + index.Item1 + "," + index.Item2
                + ") \n Room: null \n"
                + "\n Filled: " + filled;
        }
        #endregion

    }
    #endregion
    #region Map Variables/Constructor
    //Modify the size fields to activate OnValidate and create the grid
    public List<Node> Cells = new List<Node>();
    public Node StartingNode;
    private Vector3 _start;
    private int _numRows = 0, _numCols = 0;
    
    /// <summary>
    /// Constructor defines dimensions of each cell (and by extension, the map) based on a "default" 1x1 cell sprite
    /// </summary>
    /// <param name="defaultRef"></param>
    public Map(Sprite defaultRef)
    {
        if (defaultRef != null)
            Node.dimensions = new Vector2(2 * defaultRef.bounds.extents.x, 2 * defaultRef.bounds.extents.y);
    }
    #endregion
    #region Indexer/Cell Accessor

    //Indexer allows access to list "Cells" with coordinates
    public Node this[int xKey, int yKey]
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
    private Node AccessCell(int x, int y)
    {
        Node target;
        //If the cell already exists in the list of cells
        if(Cells.Any(c => c.index.Item1 == x && c.index.Item2 == y)){
            //assign target to found cell
            target = Cells.First(c => (int)c.index.Item1 == x && c.index.Item2 == y);
        }
        //If the cell does not yet exist
        else{
            //create blank cell for possible override
            target = new Node((x, y));
            Cells.Add(target);
        }

        #if UNITY_EDITOR
        DetermineVisualDimensions();
        #endif

        return target;
    }
    #endregion
    #region Setters/Getters/Adjacency

    /// <summary>
    /// Returns whether a cell is "Completed"
    /// Completion occurs when a cell is surrounded on all of it's exiting sides.
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public bool CheckCellCompletion(Node c)
    {
        int fillCount = 0;
        foreach (Exit e in c.room.exits)
        {
            switch (e.GetOrientation())
            {
                case Exit.Orientation.Up:
                    if (this[c.index.Item1, c.index.Item2 + 1].filled)
                        fillCount++;
                    break;
                case Exit.Orientation.Down:
                    if (this[c.index.Item1, c.index.Item2 - 1].filled)
                        fillCount++;
                    break;
                case Exit.Orientation.Left:
                    if (this[c.index.Item1 - 1, c.index.Item2].filled)
                        fillCount++;
                    break;
                case Exit.Orientation.Right:
                    if (this[c.index.Item1 + 1, c.index.Item2].filled)
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
    public List<Node> GetAdjacentCells((int, int) index)
    {
        Node[] adj = {  this[index.Item1 - 1, index.Item2],
                        this[index.Item1 + 1, index.Item2],
                        this[index.Item1, index.Item2 + 1],
                        this[index.Item1, index.Item2 - 1] };
        return adj.ToList();
    }
    /// <summary>
    /// Returns list of accessed adjacent cells such that each accessed cell is in the direction of an exit on the cell parameter.
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public List<Node> GetAdjacentWithFilter(Node cell)
    {
        List<Node> adj = new List<Node>();
        foreach (Exit e in cell.room.exits){
            switch (e.GetOrientation())
            {
                case Exit.Orientation.Up:
                    adj.Add(this[cell.index.Item1, cell.index.Item2 + 1]);
                    break;
                case Exit.Orientation.Down:
                    adj.Add(this[cell.index.Item1, cell.index.Item2 - 1]);
                    break;
                case Exit.Orientation.Left:
                    adj.Add(this[cell.index.Item1 - 1, cell.index.Item2]);
                    break;
                case Exit.Orientation.Right:
                    adj.Add(this[cell.index.Item1 + 1, cell.index.Item2]);
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
    public List<Node> GetAdjacentFilterInverse(Node cell)
    {
        List<Node> adj = new List<Node>();
        foreach (var c in GetAdjacentCells(cell.index).Where(x=> x.filled).ToList())
        {
            int cellX = cell.index.Item1,
                cellY = cell.index.Item2,
                compareX = c.index.Item1,
                compareY = c.index.Item2;
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
        List<Node> activeCells = Cells.Where(x => x.filled).ToList();
        Debug.Log("Active Cells: " + activeCells.Count);
        foreach (Node cell in activeCells)
            Debug.Log(cell.ToString());
    }
    /// <summary>
    /// Determines NxN grid size based on current placed cells, used for gizmo drawing.
    /// </summary>
    private void DetermineVisualDimensions()
    {
        List<int> xIndexes = new List<int>(), yIndexes = new List<int>();

        foreach(Node cell in Cells){
            xIndexes.Add((int)cell.index.Item1);
            yIndexes.Add((int)cell.index.Item2);
        }

        _numCols = (xIndexes.Min() >= 0) ? xIndexes.Max() : xIndexes.Max() - xIndexes.Min();
        _numRows = (yIndexes.Min() >= 0) ? yIndexes.Max() : yIndexes.Max() - yIndexes.Min();

        _numCols = (_numCols > _numRows) ? _numCols : _numRows;
        _numRows = (_numCols > _numRows) ? _numCols : _numRows;
        if(_numCols %2 == 0){
            _numCols++;
            _numRows++;
        }
        _start =  new Vector3(   .5f * Node.dimensions.x * -_numRows,
                                .5f * Node.dimensions.y * _numCols,
                                 0f);
    }

    //Call in OnDrawGizmos
    public void VisualizeMap()
    {
        if (_numRows > 0 && _numCols > 0){
            Gizmos.color = Color.red;
            //Draw Horizontal Lines
            for (int i = 0; i <= _numCols; i++)
                Gizmos.DrawLine(new Vector3(_start.x, _start.y - (i * Node.dimensions.y)),
                                new Vector3(_start.x + (Node.dimensions.x * _numRows), _start.y - (i * Node.dimensions.y)));
            //Draw Vertical Lines
            for (int j = 0; j <= _numRows; j++)
                Gizmos.DrawLine(new Vector3(_start.x + (j * Node.dimensions.x), _start.y),
                                new Vector3(_start.x + (j * Node.dimensions.x), _start.y - (Node.dimensions.y * _numCols)));
        }
    }
    public void ClearMap()
    {
        Cells.Clear();
        _start = Vector3.zero;
        _numRows = 0;
        _numCols = 0;
        Node.dimensions = new Vector2(0f, 0f);
    }
#endif
#endregion
}
