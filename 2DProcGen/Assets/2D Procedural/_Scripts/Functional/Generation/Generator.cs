using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace jkGenerator
{
/*TODO: Spawn Player On Edge, Determine Exit Room, Determine Generation based on level "Types", "Secret" Rooms, Directional Walks
 * TODO: Static batching for rendering combinations
 * Algorithmic Placement,
 * Pattern Overlay  
     */
    [Serializable]
    public static class Generator
    {
        #region Constraints
        [Serializable]
        public struct Constraints
        {
            public enum Style
            {
                Random,
                Organized,
                LargeRooms
            };

            [Flags]
            public enum Types
            {
                RoomCount = 1,
                StartPos = 2,
                BoundsX = 4,
                BoundsY = 8,
                SecretRoom = 16
            }

            public int RoomCount;
            public (int, int) XRange, YRange;
            public Vector2 StartPos;
            public Style GenStyle;
            public Types ConstraintTypes;

            public Constraints(Style s, Types t, int rc, (int, int) xb, (int, int) yb, Vector2 sPos)
            {
                GenStyle = s;
                ConstraintTypes = t;
                RoomCount = rc;
                XRange = xb;
                YRange = yb;
                StartPos = sPos;
            }

            public override string ToString()
            {
                return "Style: " + GenStyle + "\n Types: " + ConstraintTypes + "\n RoomCount: " + RoomCount + 
                       "\n XConstraints: " + "(" + XRange.Item1 + ", " + XRange.Item2 + ") " + 
                       "\n YConstraints: " + "(" + YRange.Item1 + ", " + YRange.Item2 + ") " + 
                       "\n Start Position: " + StartPos;
            }
        }

        #endregion

        #region Generator_Variables

        private static GameObject _generatorObj, _baseRoom;
        private static Constraints _gc;
        private static (int, int) _xCouple, _yCouple;
        private static Sprite _defaultRoomSprite;
        private static List<GameObject> _placedRooms = new List<GameObject>();
        private static int _seed;
        private static List<Room> _rooms, _startingRooms;
        private static List<GameObject> _enemies = new List<GameObject>();
        private static FloorMap _map;
        private static List<GameObject> _cHList = new List<GameObject>();

        #endregion

        #region Generation Constraint(s) Definition and Types

        public static void SetGenConstraints(Constraints.Style style, Constraints.Types types, int numRooms,
            (int, int) xRange,
            (int, int) yRange, Vector2 sPos)
        {
            _gc = new Constraints(style, types, numRooms, xRange, yRange, sPos);
        }

        public static void Init()
        {
            _generatorObj = new GameObject("Generator");
            //Load Sprite from file
            _defaultRoomSprite = Resources.Load<Sprite>("_Prefabs/Generator/DefaultRoomSprite");
            //Load all created Rooms from file folder
            List<Object> roomsAsObjects = Resources.LoadAll("_ScriptableObjects/Rooms", typeof(Room)).ToList();
            _rooms = roomsAsObjects.Cast<Room>().ToList();
            //All rooms with 4 exits are designated as "starting rooms". Modify this to change what rooms generation can begin with
            _startingRooms = _rooms.Where(x => x.exits.Count == 4).ToList();
            //Initialize base room
            _baseRoom = Resources.Load<GameObject>("_Prefabs/Generator/RoomBase");
            Debug.Log("Rooms Initialized: " + _rooms.Count);
        }

        /// <summary>
        /// TODO: Generates rooms based on enum style and number of iterations (constraints)
        /// </summary>
        public static void Generate()
        {
            //Map is indexed and sized dependent on the sizing of a SINGLE 1x1 room 
            _map = new FloorMap(_defaultRoomSprite);
            //Create generation seed
            Randomization.GenerateUnitySeed(_seed);
            Debug.Log("Seed: " + _seed);
            //Choose generation style & begin generation
            switch (_gc.GenStyle)
            {
                case Constraints.Style.Random:
                    GenerateRandom(_gc);
                    break;
                case Constraints.Style.Organized:
                    GenerateOrganized(_gc);
                    break;
                case Constraints.Style.LargeRooms:
                    GenerateLRooms(_gc);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Random walks outward from the designated start point toward, completed after all cells within grid are completed
        /// </summary>
        /// <param name="constraints"></param>
        public static void GenerateRandom(Constraints constraints)
        {
            //Place Start at given position, or at default (0,0)
            if ((constraints.ConstraintTypes & Constraints.Types.StartPos) == Constraints.Types.StartPos)
                PlaceStart(constraints.StartPos);
            else
            {
                PlaceStart();
            }

            //Limit x-axis cells
            if ((constraints.ConstraintTypes & Constraints.Types.BoundsX) == Constraints.Types.BoundsX)
                _xCouple = (constraints.XRange.Item1 < constraints.XRange.Item2) ? constraints.XRange : (0, 0);

            //Limit y-axis cells
            if ((constraints.ConstraintTypes & Constraints.Types.BoundsY) == Constraints.Types.BoundsY)
                _yCouple = (constraints.YRange.Item1 < constraints.YRange.Item2) ? constraints.YRange : (0, 0);

            //Allow placement of secret room(s)
            if ((constraints.ConstraintTypes & Constraints.Types.SecretRoom) == Constraints.Types.SecretRoom)
            {
            }

            //Limited RoomCount for Random Generation means that the generator will generate until it reaches a fixed number of rooms,
            //then loop back and complete the level
            if ((constraints.ConstraintTypes & Constraints.Types.RoomCount) == Constraints.Types.RoomCount)
            {
                while (constraints.RoomCount > _map.Cells.Where(x => x.filled).ToList().Count && !_map.Completed())
                    MatchRandomViaMap();
            }
            else
            {
                Debug.Log("No RoomCount heuristic");
                while (!_map.Completed())
                    MatchRandomViaMap();
            }



        }

        public static void GenerateOrganized(Constraints constraints)
        {
            if ((constraints.ConstraintTypes & Constraints.Types.StartPos) == Constraints.Types.StartPos)
                PlaceStart(constraints.StartPos);
            else
            {
                PlaceStart();
            }

            //Limit x-axis cells
            if ((constraints.ConstraintTypes & Constraints.Types.BoundsX) == Constraints.Types.BoundsX)
                _xCouple = (constraints.XRange.Item1 < constraints.XRange.Item2) ? constraints.XRange : (0, 0);

            //Limit y-axis cells
            if ((constraints.ConstraintTypes & Constraints.Types.BoundsY) == Constraints.Types.BoundsY)
                _yCouple = (constraints.YRange.Item1 < constraints.YRange.Item2) ? constraints.YRange : (0, 0);
            //Allow placement of secret room(s)
            if ((constraints.ConstraintTypes & Constraints.Types.SecretRoom) == Constraints.Types.SecretRoom)
            {
            }

            //Set weighting for "squarish" patterns to appear more frequently
        }

        public static void GenerateLRooms(Constraints constraints)
        {

        }

        #endregion

        #region Room Placement Functions

        /// <summary>
        /// Place random compatible room at index (x, y)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void PlaceRandAtLocation(int x, int y)
        {
            var index = new Vector2(x, y);
            var newCell = new FloorMap.RoomCell(index, Randomization.GetRandomWeightedRoom(GetCompatibleRooms(index)))
                {filled = true};
            CreateRoomObject(newCell);
        }

        /// <summary>
        /// Place room at index (x, y) with selected ScriptableObject room.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="r"></param>
        public static void PlaceAtLocation(int x, int y, Room r)
        {
            Debug.Log(r);
            //Create index from x/y integers
            var index = new Vector2(x, y);

            //Create a list of rooms available for placement at this index
            var confirmAgainst = GetCompatibleRooms(index);

            //If the room chosen can be placed at this index, then create the RoomCell with the room and create an object over it
            if (confirmAgainst.Any(cp => cp == r))
            {
                var newCell = new FloorMap.RoomCell(index, r)
                {
                    filled = true
                };
                CreateRoomObject(newCell);
            }
            else
                Debug.Log("Selected room is not compatible with index.");
        }

        /// <summary>
        /// Place starting room to build out from at default index (0,0)
        /// </summary>
        private static void PlaceStart()
        {
            PlaceAtLocation(0, 0, Randomization.GetRandom(_startingRooms));
            Debug.Log("Starting Cell Info: " + _map[0, 0]);
        }

        /// <summary>
        /// Place starting room to build out from at designated index
        /// </summary>
        /// <param name="index"></param>
        private static void PlaceStart(Vector2 index)
        {
            PlaceAtLocation((int) index.x, (int) index.y, Randomization.GetRandom(_startingRooms));
            Debug.Log("Starting Cell Info: " + _map[(int) index.x, (int) index.y]);
        }

        #endregion

        #region Room_Manipulation

        /// <summary>
        /// Create GameObject/RoomDisplay with cell Information
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        private static void CreateRoomObject(FloorMap.RoomCell cell)
        {
            //Set map location to cell
            _map[(int) cell.index.x, (int) cell.index.y] = cell;

            //Create GameObject Room, initialize as child with cell position value
            var cRoomGO = Object.Instantiate<GameObject>(_baseRoom);
            cRoomGO.transform.parent = _generatorObj.transform;
            cRoomGO.transform.position = cell.cellPos;

            //Initialize room display with ScriptableObject room from cell
            var display = cRoomGO.GetComponent<RoomDisplay>();
            display.SetRoom(cell.room);
            display.Init();
            //Add colliders and gameobjects to reference lists
            _cHList.Add(display.BuildColliders());
            _placedRooms.Add(cRoomGO);
        }

        /// <summary>
        /// Generation method marches outwards; picks random existing cells and builds outward based on existing adjacents.
        /// </summary>
        private static void MatchRandomViaMap()
        {
            //Get a random existing cell that is not "completed", or surrounded in all directions where it has exits
            var filledCells = new List<FloorMap.RoomCell>(_map.Cells.Where(x => x.filled).ToList());
            var randExistingCell =
                Randomization.GetRandom(filledCells.Where(x => !_map.CheckCellCompletion(x)).ToList());

            //Get a random cell adjacent to the existing cell that does not have anything placed into it, but is built off its exits
            //Can change this single line to prioritize rooms building outward, inward, etc.
            var newCell =
                Randomization.GetRandom(_map.GetAdjacentWithFilter(randExistingCell).Where(x => !x.filled).ToList());

            /*If it fails, find another existing cell on the map to generate from.
              This loop should not be accessed more than once if the generator is fed enough rooms.
             */
            while (newCell.room == null)
            {
                randExistingCell =
                    Randomization.GetRandom(filledCells.Where(x => !_map.CheckCellCompletion(x)).ToList());
                newCell = Randomization.GetRandom(_map.GetAdjacentWithFilter(randExistingCell).Where(x => !x.filled)
                    .ToList());
                newCell.room = Randomization.GetRandomWeightedRoom(GetCompatibleRooms(newCell.index));
            }

            newCell.filled = true;
            CreateRoomObject(newCell);
        }

        /// <summary>
        /// Determines possible rooms to place based on cells adjacent to Vector2 index.
        /// </summary>
        /// <param name="prevCell"></param>
        /// <param name="nextCell"></param>
        /// <returns></returns>
        private static List<Room> GetCompatibleRooms(Vector2 index)
        {

            var currentCell = _map[(int) index.x, (int) index.y];

            //Will always contain 4 elements no matter what.
            var adjacentMapCells = _map.GetAdjacentCells(index).Where(x => x.filled).ToList();

            //Only contains cells that this room will be connected to (to thin connecting types)
            var typeCheckCells = new List<FloorMap.RoomCell>(_map.GetAdjacentFilterInverse(currentCell));

            //List of all enums to thin out types based on adjacent cells
            var types = new List<PatternBuilder.Pattern.RoomType>(Enum
                .GetValues(typeof(PatternBuilder.Pattern.RoomType))
                .Cast<PatternBuilder.Pattern.RoomType>()
                .ToList());

            List<Room> possible = new List<Room>(_rooms);

            //Starting with list of all rooms, thin by matching exits
            foreach (var cell in adjacentMapCells)
                possible = possible.Where(x => currentCell.ContainsExitMatchIn(cell, x)).ToList();

            if (possible.Count == 0) return null;

            //Thin list of types by type compatibility of surrounding cells
            foreach (var tcCell in typeCheckCells)
                types = types.Intersect(tcCell.room.compatibleTypes).ToList();

            if (types.Count == 0) return null;
            //Final cut using only valid types
            possible = possible.Where(room => types.Contains(room.roomType)).ToList();

            //TODO: This may be bad engineering. Fix if possible later down the road.

            //Null check - change x-y constraints to fit constraints if they exist
            if (Math.Abs(_xCouple.Item1 - _xCouple.Item2) > 0 || Math.Abs(_yCouple.Item1 - _yCouple.Item2) > 0)
                possible = ApplyBoundaryConstraints(index, possible);

            //If the generator is fed enough rooms with proper types/exits, this will never return null
            return possible.Count == 0 ? null : possible;
        }

        /// <summary>
        /// Apply X/Y boundary constraints if set, using list of possible rooms & index of cell in question
        /// </summary>
        /// <param name="index"></param>
        /// <param name="thinList"></param>
        /// <returns></returns>
        private static List<Room> ApplyBoundaryConstraints(Vector2 index, List<Room> thinList)
        {
            //If X constraint(s) exist
            if (!_xCouple.Equals((0, 0)))
            {
                var indexX = (int) index.x;
                //Items on the leftmost X-bound cannot exit to the left
                if (indexX == _xCouple.Item1)
                    thinList = thinList.Where(x => x.exits.All(y => y.GetOrientation() != Exit.Orientation.Left))
                        .ToList();
                //Items on the rightmost X-bound cannot exit to the right
                if (indexX == _xCouple.Item2)
                    thinList = thinList.Where(x => x.exits.All(y => y.GetOrientation() != Exit.Orientation.Right))
                        .ToList();
            }

            //If Y constraint(s) exist
            if (!_yCouple.Equals((0, 0)))
            {
                var indexY = (int) index.y;
                //Items on the lower Y-bound cannot exit downwards
                if (indexY == _yCouple.Item1)
                    thinList = thinList.Where(x => x.exits.All(y => y.GetOrientation() != Exit.Orientation.Down))
                        .ToList();
                //Items on the upper Y-bound cannot exit upwards
                if (indexY == _yCouple.Item2)
                    thinList = thinList.Where(x => x.exits.All(y => y.GetOrientation() != Exit.Orientation.Up))
                        .ToList();
            }

            return thinList;
        }

        #endregion

        #region Setters/Getters

        public static FloorMap GetMap()
        {
            return _map;
        }

        public static Constraints GetConstraints()
        {
            return _gc;
        }

        #endregion

        #region Editor Functions

#if UNITY_EDITOR
        /// <summary>
        /// Visual cues: Visualizes map based on filled map cell rows and columns:
        /// Gizmos place yellow circles for filled cells, green circles for empty cells, nothing for uninitiated cells
        /// </summary>
        /*private void OnDrawGizmos()
        {
            if (GetMap().Cells.Count <= 1) return;
            foreach (var c in GetMap().Cells)
            {
                if (!c.filled)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(c.cellPos, .25f);
                }
                else
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(c.cellPos, .25f);
                }
            }
            GetMap().VisualizeMap();
        }*/
        public static void ResetGenerator()
        {
            _placedRooms.Clear();
            _seed = 0;
            RoomDisplay.counter = 0;
            _map.ClearMap();
            _map = new FloorMap(_defaultRoomSprite);
            while (_generatorObj.transform.childCount > 0)
                foreach (Transform child in _generatorObj.transform)
                    Object.DestroyImmediate(child.gameObject);
            foreach (GameObject g in _cHList)
                Object.DestroyImmediate(g);
            _cHList.Clear();
        }
#endif

        #endregion
    }
}