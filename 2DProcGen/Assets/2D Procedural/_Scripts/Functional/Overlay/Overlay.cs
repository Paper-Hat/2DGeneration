﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace jkGenerator
{
    //Separation of overlay functions from generator
    public static class Overlay
    {
        private static GameObject _entityContainer;
        private static List<Pattern> _patterns;
        private static List<GameObject> _enemies = new List<GameObject>();
        private static List<GameObject> _obstacles = new List<GameObject>();
        private static List<GameObject> _walls = new List<GameObject>();
        private static List<GameObject> _guns = new List<GameObject>();
        private static int overlayCounter = 0;

        public static void LoadEntities(GameObject parentedObj)
        {
            _entityContainer = new GameObject("Entities");
            if (parentedObj) _entityContainer.transform.parent = parentedObj.transform;
            _entityContainer.transform.position = Vector3.zero;
            
            var patternObjs = Resources.LoadAll("_ScriptableObjects/Patterns", typeof(Pattern)).ToList();
            _patterns = patternObjs.Cast<Pattern>().ToList();
            var enemyObjs = Resources.LoadAll("_Prefabs/_Overlay/_Enemies").ToList();
            _enemies = enemyObjs.Cast<GameObject>().ToList();
            var obstacleObjs = Resources.LoadAll("_Prefabs/_Overlay/_Obstacles").ToList();
            _obstacles = obstacleObjs.Cast<GameObject>().ToList();
            var wallObjs = Resources.LoadAll("_Prefabs/_Overlay/_Walls").ToList();
            _walls = wallObjs.Cast<GameObject>().ToList();
            var gunObjs = Resources.LoadAll("_Prefabs/_Overlay/_Guns");
            _guns = gunObjs.Cast<GameObject>().ToList();

        }
        public static void Create(Map map)
        {
            //skip first element (starting room)
            foreach (var cell in map.Cells.Where(x => x.filled).ToList())
            {
                if (_patterns.All(x => x.roomType != cell.room.roomType)) continue;
                var pickFrom = _patterns.Where(x => x.roomType == cell.room.roomType).ToList();
                cell.room.pattern = Randomization.GetRandom(pickFrom);
                PlaceEntities(cell);
            }
        }
        public static void SpawnPlayer(Map map, (int, int) coordinates)
        {
            Vector3 spawnCellPos = map[coordinates.Item1, coordinates.Item2].cellPos;
            Object.Instantiate(Resources.Load<GameObject>("Player"),spawnCellPos, Quaternion.identity);
        }
        private static void PlaceEntities(Map.Node node)
        {
            Pattern p = node.room.pattern;
            GameObject ovlContainer = new GameObject("Room #"+ ++overlayCounter);
            ovlContainer.transform.parent = _entityContainer.transform;
            if (p == null || p.Placements.Count <= 0) return;
            foreach (Cell cell in p.Placements)
            {
                //Debug.Log("Active Cell Location: " + cell.GetLocation());
                GameObject ovlObj = null;
                switch (cell.GetSpawnType())
                {
                    case Cell.SpawnType.Enemy:
                        ovlObj = Object.Instantiate(Randomization.GetRandom(_enemies), cell.GetLocation(),
                            Quaternion.identity);
                        break;
                    case Cell.SpawnType.Obstacle:
                        ovlObj = Object.Instantiate(Randomization.GetRandom(_obstacles), cell.GetLocation(),
                            Quaternion.identity);
                        break;
                    case Cell.SpawnType.None:
                        ovlObj = Object.Instantiate(Randomization.GetRandom(_walls
                                .Where(x => x.GetComponent<Wall>().GetWallType() == cell.GetWallType()).ToList()),
                            cell.GetLocation(), Quaternion.identity);
                        break;
                }
                if (!ovlObj) continue;
                ovlObj.transform.position += node.cellPos;
                ovlObj.transform.parent = ovlContainer.transform;
            }
        }
    }
}