using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jkGenerator
{
    public class Wall : MonoBehaviour
    {
        [SerializeField] private GridCell.WallType gcWT;

        public GridCell.WallType GetWallType()
        {
            return gcWT;
        }

        public void SetWallType(GridCell.WallType wt)
        {
            gcWT = wt;
        }
    }
}