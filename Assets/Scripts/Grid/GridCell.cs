using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grid
{
    public class GridCell : MonoBehaviour
    {
        public Vector2Int GridPosition { get; private set; }

        public bool IsOccupied { get; private set; }

        public int OccupiedByBuildingId { get; private set; }

        public GridCell(Vector2Int gridPosition)
        {
            GridPosition = gridPosition;
            IsOccupied = false;
            OccupiedByBuildingId = -1;
        }

        public void Occupy(int buildingId)
        {
            IsOccupied = true;
            OccupiedByBuildingId = buildingId;
        }

        public void Free()
        {
            IsOccupied = false;
            OccupiedByBuildingId = -1;
        }

        public Vector3 GetWorldPosition()
        {
            return new Vector3(GridPosition.x, GridPosition.y, 0);
        }
    }
}

