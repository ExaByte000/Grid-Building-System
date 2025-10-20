using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Управляет логической сеткой: хранит состояние клеток, 
/// конвертирует координаты, проверяет доступность
/// </summary>
public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 20;
    [SerializeField] private int gridHeight = 20;
    [SerializeField] private Vector2Int gridOffset = Vector2Int.zero;

    private const float CELL_SIZE = 1f;

    private GridCell[,] cells;

    public int Width => gridWidth;
    public int Height => gridHeight;
    public Vector2Int Offset => gridOffset;

    private void Awake()
    {
        Initialize();
    }

    /// <summary>
    /// Инициализация сетки - создание всех клеток
    /// </summary>
    private void Initialize()
    {
        cells = new GridCell[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                //Vector2Int gridPos = new Vector2Int(x, y) + gridOffset;
                Vector2Int gridPos = new Vector2Int(x + gridOffset.x, y + gridOffset.y);
                cells[x, y] = new GridCell(gridPos);

                Debug.Log($"Created cell at array[{x},{y}] with gridPos={gridPos}");
            }
        }
    }

    #region Конвертация координат

    /// <summary>
    /// Конвертирует мировые координаты в координаты сетки
    /// </summary>
    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        int gridX = Mathf.FloorToInt(worldPosition.x);
        int gridY = Mathf.FloorToInt(worldPosition.y);

        return new Vector2Int(gridX, gridY);
    }

    /// <summary>
    /// Конвертирует координаты сетки в мировые координаты (центр клетки)
    /// </summary>
    public Vector3 GridToWorld(Vector2Int gridPosition)
    {
        return new Vector3(
            gridPosition.x * CELL_SIZE,
            gridPosition.y * CELL_SIZE,
            0
        );
    }

    #endregion

    #region Проверка доступности

    /// <summary>
    /// Проверяет, находится ли позиция в пределах сетки
    /// </summary>
    public bool IsValidGridPosition(Vector2Int gridPosition)
    {
        int localX = gridPosition.x - gridOffset.x;
        int localY = gridPosition.y - gridOffset.y;

        bool valid = localX >= 0 && localX < gridWidth &&
                     localY >= 0 && localY < gridHeight;

        // ДОБАВЬ ДЕТАЛЬНЫЙ ЛОГ:
        //Debug.Log($"IsValidGridPosition({gridPosition}): offset=({gridOffset.x},{gridOffset.y}), " +
        //          $"local=({localX},{localY}), gridSize=({gridWidth},{gridHeight}), valid={valid}");

        return valid;
    }

    /// <summary>
    /// Проверяет, свободна ли одна клетка
    /// </summary>
    public bool IsCellAvailable(Vector2Int position)
    {
        //Debug.Log($"IsCellAvailable called for {position}");
        if (!IsValidGridPosition(position))
        {
            //Debug.Log($"Position {position} is INVALID (out of bounds)");
            return false;
        }
            

        GridCell cell = GetCell(position);
        bool available = cell != null && !cell.IsOccupied;

        //Debug.Log($"Position {position}: cell exists={cell != null}, occupied={cell?.IsOccupied}, available={available}");
        return available;
    }

    /// <summary>
    /// Проверяет, свободна ли область (для зданий размером больше 1x1)
    /// </summary>
    public bool IsAreaAvailable(Vector2Int position, Vector2Int size)
    {
        //Debug.Log($"=== Checking area at {position}, size {size} ===");

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int checkPos = new Vector2Int(position.x + x, position.y + y);

                bool available = IsCellAvailable(checkPos);
                //Debug.Log($"Cell {checkPos}: available={available}");

                if (!available)
                {
                    //Debug.Log($"Cell {checkPos} is NOT available! Stopping check.");
                    return false;
                }
            }
        }
        //Debug.Log("All cells available!");
        return true;
    }

    #endregion

    #region Управление занятостью

    /// <summary>
    /// Занять область клеток зданием
    /// </summary>
    public void OccupyCells(Vector2Int position, Vector2Int size, int buildingId)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int cellPos = new Vector2Int(position.x + x, position.y + y);
                GridCell cell = GetCell(cellPos);

                if (cell != null)
                {
                    cell.Occupy(buildingId);
                }
            }
        }
    }

    /// <summary>
    /// Освободить область клеток
    /// </summary>
    public void FreeCells(Vector2Int position, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int cellPos = new Vector2Int(position.x + x, position.y + y);
                GridCell cell = GetCell(cellPos);

                if (cell != null)
                {
                    cell.Free();
                }
            }
        }
    }

    #endregion

    #region Получение информации

    /// <summary>
    /// Получить клетку по координатам сетки
    /// </summary>
    public GridCell GetCell(Vector2Int gridPosition)
    {
        if (!IsValidGridPosition(gridPosition))
            return null;

        int localX = gridPosition.x - gridOffset.x;
        int localY = gridPosition.y - gridOffset.y;

        //Debug.Log($"GetCell({gridPosition}): localX={localX}, localY={localY}, " +
        //      $"arraySize=[{cells.GetLength(0)}, {cells.GetLength(1)}]");

        //// ПРОВЕРЬ ЧТО ИНДЕКСЫ В ГРАНИЦАХ МАССИВА:
        //if (localX < 0 || localX >= cells.GetLength(0) ||
        //    localY < 0 || localY >= cells.GetLength(1))
        //{
        //    Debug.LogError($"Array index out of bounds! localX={localX}, localY={localY}");
        //    return null;
        //}

        return cells[localX, localY];
    }

    /// <summary>
    /// Получить ID здания в указанной позиции
    /// </summary>
    public int GetBuildingIdAt(Vector2Int position)
    {
        GridCell cell = GetCell(position);
        return cell != null ? cell.OccupiedByBuildingId : -1;
    }

    /// <summary>
    /// Получить размер сетки
    /// </summary>
    public Vector2Int GetGridSize()
    {
        return new Vector2Int(gridWidth, gridHeight);
    }

    #endregion

}
