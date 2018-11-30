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
                BoundsX = 2,
                BoundsY = 4,
                SecretRoom = 8
            }

            public int RoomCount;
            //because grid is preset, entire structure can be changed to perform better
            public (int, int) XRange, YRange, StartPos;
            public Style GenStyle;
            public Types ConstraintTypes;

            public Constraints(Style s, Types t, int rc, (int, int) xb, (int, int) yb, (int, int) sPos)
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

        private static GameObject _generatorObj, _baseRoom, _roomContainer, _entityContainer;
        private static Constraints _gc;
        private static (int, int) _xCouple, _yCouple;
        private static Sprite _defaultRoomSprite;
        private static Dictionary<(int, int), (Room, GameObject)> PlacedRooms = new Dictionary<(int, int), (Room, GameObject)>();
        private static int _seed;
        private static List<Room> _rooms, _startingRooms, _exitRooms;
        private static List<Pattern> _patterns;
        private static List<GameObject> _enemies = new List<GameObject>();
        private static List<GameObject> _obstacles = new List<GameObject>();
        private static List<GameObject> _walls = new List<GameObject>();
        private static Map _map;
        private static int overlayCounter = 0;
        #endregion

        #region Generation
        /// <summary>
        /// Loads all assets to be used for the generator from the file hierarchy
        /// </summary>
        public static void Init()
        {
            _generatorObj = new GameObject("Generator");
            _roomContainer = new GameObject("Rooms");
            _roomContainer.transform.parent = _generatorObj.transform;
            
            //Load Sprite from file
            _defaultRoomSprite = Resources.Load<Sprite>("_Prefabs/Generator/DefaultRoomSprite");
            //Load all created Rooms from file folder
            var basicRoomObjs = Resources.LoadAll("_ScriptableObjects/_BasicRooms", typeof(Room)).ToList();
            _rooms = basicRoomObjs.Cast<Room>().ToList();
            var exitRoomObjs = Resources.LoadAll("_ScriptableObjects/_ExitRooms", typeof(Room)).ToList();
            _exitRooms = exitRoomObjs.Cast<Room>().ToList();
            //All rooms with 4 exits are designated as "starting rooms". Modify this to change what rooms generation can begin with
            _startingRooms = _rooms.Where(x => x.exits.Count == 4).ToList();
            //Initialize base room
            _baseRoom = Resources.Load<GameObject>("_Prefabs/Generator/RoomBase");
            Overlay.LoadEntities(_generatorObj);
        }

        /// <summary>
        /// TODO: Generates rooms based on enum style and number of iterations (constraints)
        /// </summary>
        public static void Generate()
        {
            //Map is indexed and sized dependent on the sizing of a SINGLE 1x1 room 
            _map = new Map(_defaultRoomSprite);
            //Create generation seed
            Randomization.GenerateUnitySeed(_seed);
            Debug.Log("Seed: " + _seed);
            //Choose generation style & generate room floors
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

            PlaceExit();
            Overlay.Create(_map);
            Overlay.SpawnPlayer(_map, (_gc.StartPos.Item1, _gc.StartPos.Item2));
            
        }
        #endregion

        #region Spawn Things

        #endregion
        
        #region Generation Constraint(s) Definition and Types

        public static void SetGenConstraints(Constraints.Style style, Constraints.Types types, int numRooms,
            (int, int) xRange,
            (int, int) yRange, (int, int) sPos)
        {
            _gc = new Constraints(style, types, numRooms, xRange, yRange, sPos);
        }



        /// <summary>
        /// Random walks outward from the designated start point toward, completed after all cells within grid are completed
        /// </summary>
        /// <param name="constraints"></param>
        private static void GenerateRandom(Constraints constraints)
        {
            //Place Start at given position
            PlaceStart(constraints.StartPos);

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
                while (!_map.Completed())
                    MatchRandomViaMap();
            }
        }

        private static void GenerateOrganized(Constraints constraints)
        {
            PlaceStart(constraints.StartPos);
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

        private static void GenerateLRooms(Constraints constraints)
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
            var newCell = new Map.Node((x, y), Randomization.GetRandomWeightedRoom(GetCompatibleRooms((x, y), _rooms)))
                {filled = true};
            CreateRoomObject(newCell);
        }

        /// <summary>
        /// Place room at index (x, y) with selected ScriptableObject room, bool for forced placement disregarding rules.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="r"></param>
        /// <param name="forced"></param>
        private static void PlaceAtLocation(int x, int y, Room r, bool forced)
        {

            if (!forced)
            {
                //Create a list of rooms available for placement at this index
                var confirmAgainst = GetCompatibleRooms((x, y), _rooms);

                //If the room chosen can be placed at this index, then create the Node with the room and create an object over it
                if (confirmAgainst.Any(cp => cp == r))
                {
                    var newCell = new Map.Node((x, y), r)
                    {
                        filled = true
                    };
                    CreateRoomObject(newCell);
                }
                else
                    Debug.Log("Selected room is not compatible with index.");
            }
            else
            {
                CreateRoomObject(new Map.Node((x, y), r));
            }
        }

        /// <summary>
        /// Place starting room to build out from at default index (0,0)
        /// </summary>
        private static void PlaceStart()
        {
            PlaceAtLocation(0, 0, Randomization.GetRandom(_startingRooms), true);
            Debug.Log("Starting Cell Info: " + _map[0, 0]);
        }

        /// <summary>
        /// Place starting room to build out from at designated index
        /// </summary>
        /// <param name="index"></param>
        private static void PlaceStart((int, int) index)
        {
            PlaceAtLocation(index.Item1, index.Item2, Randomization.GetRandom(_startingRooms), true);
            _map.StartingNode = _map[index.Item1, index.Item2];
            Debug.Log("Starting Cell Info: " + _map[index.Item1, index.Item2]);
        }
        
        /// <summary>
        /// Determine dead end node FURTHEST from the starting point (preferrably on an outer edge of the map).
        /// </summary>
        private static void PlaceExit()
        {
            (int, int) startIndex = _map.StartingNode.index;
            (int, int) chosenIndex = (0, 0);
            double distance = 0;
            double maxDist = 0;
            //Iterate over dictionary test
            Debug.Log("----------------------------------------");
            foreach (var item in PlacedRooms)
            {
                if (item.Value.Item1.roomType == Pattern.RoomType.End)
                {
                    //Distance formula
                    distance = Math.Sqrt((item.Key.Item1 - startIndex.Item1) * (item.Key.Item1 - startIndex.Item1)
                               + ((item.Key.Item2 - startIndex.Item2) * (item.Key.Item2 - startIndex.Item2)));
                    if (distance > maxDist)
                        chosenIndex = item.Key;
                }
            }
            Debug.Log(PlacedRooms[chosenIndex].Item2.name);
            Object.DestroyImmediate(PlacedRooms[chosenIndex].Item2);
            PlacedRooms.Remove(chosenIndex);
            //TODO: Replace with "exit" rooms
            PlaceAtLocation(chosenIndex.Item1, chosenIndex.Item2, Randomization.GetRandom(_exitRooms), false);
        }
        #endregion

        #region Room_Manipulation

        /// <summary>
        /// Create GameObject/RoomDisplay with cell Information
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        private static void CreateRoomObject(Map.Node cell)
        {
            //Set map location to cell
            _map[cell.index.Item1, cell.index.Item2] = cell;

            //Create GameObject Room, initialize as child with cell position value
            var cRoomGo = Object.Instantiate(_baseRoom);
            cRoomGo.transform.parent = _generatorObj.transform;
            cRoomGo.transform.position = cell.cellPos;

            //Initialize room display with ScriptableObject room from cell
            var display = cRoomGo.GetComponent<RoomDisplay>();
            display.SetRoom(cell.room);
            display.Init();
            //Add colliders and gameobjects to reference lists
            GameObject collHolder = display.BuildColliders();
            collHolder.transform.parent = cRoomGo.transform;
            cRoomGo.transform.parent = _roomContainer.transform;
            PlacedRooms.Add(cell.index, (display.GetRoom(), cRoomGo));
        }

        /// <summary>
        /// Generation method marches outwards; picks random existing cells and builds outward based on existing adjacents.
        /// </summary>
        private static void MatchRandomViaMap()
        {
            //Get a random existing cell that is not "completed", or surrounded in all directions where it has exits
            var filledCells = new List<Map.Node>(_map.Cells.Where(x => x.filled).ToList());
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
                newCell.room = Randomization.GetRandomWeightedRoom(GetCompatibleRooms(newCell.index, _rooms));
            }

            newCell.filled = true;
            CreateRoomObject(newCell);
        }

        /// <summary>
        /// Determines possible rooms to place based on cells adjacent to Vector2 index.
        /// </summary>
        /// <returns></returns>
        private static List<Room> GetCompatibleRooms((int, int) index, List<Room> rooms)
        {

            var currentCell = _map[index.Item1, index.Item2];

            //Will always contain 4 elements no matter what.
            var adjacentMapCells = _map.GetAdjacentCells(index).Where(x => x.filled).ToList();

            //Only contains cells that this room will be connected to (to thin connecting types)
            var typeCheckCells = new List<Map.Node>(_map.GetAdjacentFilterInverse(currentCell));

            //List of all enums to thin out types based on adjacent cells
            var types = new List<Pattern.RoomType>(Enum
                .GetValues(typeof(Pattern.RoomType))
                .Cast<Pattern.RoomType>()
                .ToList());

            List<Room> possible = new List<Room>(rooms);

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
        private static List<Room> ApplyBoundaryConstraints((int, int) index, List<Room> thinList)
        {
            //If X constraint(s) exist
            if (!_xCouple.Equals((0, 0)))
            {
                var indexX = index.Item1;
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
                var indexY = index.Item2;
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

        public static Map GetMap()
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
            PlacedRooms.Clear();
            _seed = 0;
            RoomDisplay.counter = 0;
            _map.ClearMap();
            _map = new Map(_defaultRoomSprite);
            while (_generatorObj.transform.childCount > 0)
                foreach (Transform child in _generatorObj.transform)
                    Object.DestroyImmediate(child.gameObject);
        }
#endif

        #endregion
    }
}