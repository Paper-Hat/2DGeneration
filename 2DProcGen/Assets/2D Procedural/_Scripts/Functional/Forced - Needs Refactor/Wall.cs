using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jkGenerator
{
    public class Wall : MonoBehaviour
    {
        [SerializeField] private Cell.WallType gcWT;

        public Cell.WallType GetWallType()
        {
            return gcWT;
        }

        public void SetWallType(Cell.WallType wt)
        {
            gcWT = wt;
        }
    }
}