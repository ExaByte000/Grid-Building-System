using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;


public enum BuildMode
{
    Placement,
    Delete
}
public class BuildingManager : MonoBehaviour
{
    [Header("Building Prefabs")]
    public GameObject GreenHouse;
    public GameObject RedHouse;
    public GameObject Desk;

    private Dictionary<int, GameObject> buildingPrefabs = new Dictionary<int, GameObject>();

    [Header("References")]
    public GridManager gridManager;
    public GridHighlight gridHighlight;

    [Header("Building Types")]
    private List<BuildingData> BuildingDatas = new List<BuildingData>();

    private bool isPlacementMode = false;
    private bool isDeleteMode = false;
    private BuildingData currentBuildingData;
    private GameObject previewObject;
    private SpriteRenderer previewRenderer;
    private Vector2Int currentGridPos;
    private bool canPlace = false;

    private int nextBuildingInstanceId = 0;

    private void Awake()
    {
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();

        if (gridHighlight == null)
            gridHighlight = FindObjectOfType<GridHighlight>();

        CreateBuildingDatas();

        buildingPrefabs[1] = GreenHouse;
        buildingPrefabs[2] = RedHouse;
        buildingPrefabs[3] = Desk;
    }

    private void CreateBuildingDatas()
    {
        BuildingDatas.Add(new BuildingData(1, "Green House", 2, 2, new Color(0.55f, 0.27f, 0.07f)));
        BuildingDatas.Add(new BuildingData(2, "Red House", 2, 2, new Color(0.13f, 0.55f, 0.13f)));
        BuildingDatas.Add(new BuildingData(3, "Desk", 1, 1, new Color(0.5f, 0.5f, 0.5f)));
    }

    private void Update()
    {

        // 🧩 Проверяем, не находится ли курсор над UI
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            // Если над UI — просто скрываем превью и выходим
            if (previewObject != null)
                previewObject.SetActive(false);

            return;
        }
        else
        {
            // Если не над UI — показываем превью обратно
            if (previewObject != null)
                previewObject.SetActive(true);
        }

        if (previewObject != null)
        {
            UpdatePreview();
            
        }
        HandleInput();
    }

    
    private BuildMode currentMode = BuildMode.Placement;

    public BuildMode CurrentMode => currentMode;

    public void SetMode(BuildMode mode)
    {
        CancelPlacement();
        currentMode = mode;

        switch (mode)
        {
            case BuildMode.Placement:
                isPlacementMode = true;
                isDeleteMode = false;
                gridHighlight.SetFollowMouse(true);

                if (previewObject != null)
                {
                    Destroy(previewObject);
                    previewObject = null;

                }
                //Debug.Log("Mode: Placement");
                break;

            case BuildMode.Delete:
                isPlacementMode = false;
                isDeleteMode = true;
                gridHighlight.SetFollowMouse(false);

                if (previewObject != null)
                {
                    Destroy(previewObject);
                    previewObject = null;

                }

                //Debug.Log("Mode: Delete");
                break;

        }
    }

    

    // --------- API для UI ---------

    // Запуск режима размещения для конкретного типа
    public void StartPlacement(int buildingId)
    {
        if (isDeleteMode == true)
            return;
        currentBuildingData = BuildingDatas.Find(b => b.id == buildingId);

        if (currentBuildingData == null)
        {
            Debug.LogError($"Building type {buildingId} not found!");
            return;
        }

        gridHighlight.SetFollowMouse(false);

        CreatePreview();

        //Debug.Log($"Started placement: {currentBuildingData.name}");
    }

    public void CancelPlacement()
    {
        if (previewObject != null)
            Destroy(previewObject);

        isPlacementMode = false;
        currentBuildingData = null;

        gridHighlight.SetFollowMouse(true);

        //Debug.Log("Placement cancelled");
    }

    public void EnterDeleteMode()
    {
        isDeleteMode = true;
        isPlacementMode = false;
        if (previewObject != null)
            Destroy(previewObject);

        gridHighlight.SetFollowMouse(true);
        //Debug.Log("Entered delete mode");
    }

    public void ExitDeleteMode()
    {
        isDeleteMode = false;
        
        //Debug.Log("Exited delete mode");
    }

    public bool IsInPlacementMode() => isPlacementMode;
    public bool IsInDeleteMode() => isDeleteMode;

    // --------- Preview и размещение ---------

    private void CreatePreview()
    {
        if (previewObject != null)
            Destroy(previewObject);

        GameObject prefab = buildingPrefabs[currentBuildingData.id];

        SpriteRenderer prefabSprite = prefab != null ? prefab.GetComponent<SpriteRenderer>() : null;

        previewObject = new GameObject("Preview");
        previewRenderer = previewObject.AddComponent<SpriteRenderer>();

        if (prefabSprite != null)
        {
            previewRenderer.sprite = prefabSprite.sprite;
            previewObject.transform.localScale = prefab.transform.localScale;
        }

        previewRenderer.sortingOrder = 5;
        // начальный цвет
        previewRenderer.color = new Color(0f, 1f, 0f, 0.5f);
    }

    private void UpdatePreview()
    {
        Vector3 mouseWorld = GetMouseWorldPosition();
        currentGridPos = gridManager.WorldToGrid(mouseWorld);

        canPlace = gridManager.IsAreaAvailable(currentGridPos, currentBuildingData.size);

        // Цвет preview: используем цвет здания с прозрачностью, но меняем на зел/крас для ясности
        if (previewRenderer != null)
        {
            if (canPlace)
                previewRenderer.color = new Color(0f, 1f, 0f, 0.5f);
            else
                previewRenderer.color = new Color(1f, 0f, 0f, 0.5f);
        }

        float centerX = currentGridPos.x + currentBuildingData.size.x / 2f;
        float centerY = currentGridPos.y + currentBuildingData.size.y / 2f;
        previewObject.transform.position = new Vector3(centerX, centerY, 0);
    }

    private void HandleInput()
    {
        // ЛКМ
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (isPlacementMode)
                TryPlaceBuilding();
            else if (isDeleteMode)
                TryDeleteAtMouse();
        }

        // ESC - отмена / выйти
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPlacementMode) CancelPlacement();
            if (isDeleteMode) ExitDeleteMode();
        }

        // Доп: правая кнопка отменяет placement
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (isPlacementMode) CancelPlacement();
        }
    }

    private void TryPlaceBuilding()
    {
        if (!canPlace)
        {
            //Debug.Log("Can't place here!");
            return;
        }

        GameObject prefab = buildingPrefabs[currentBuildingData.id];
        if (prefab == null)
        {
            //Debug.LogError($"Prefab for id {currentBuildingData.id} not found!");
            return;
        }

        GameObject buildingObj = Instantiate(prefab);

        Building building = buildingObj.GetComponent<Building>();
        if (building == null)
            building = buildingObj.AddComponent<Building>();

        int myInstanceId = nextBuildingInstanceId;

        building.Initialize(
            currentBuildingData.id,
            currentGridPos,
            currentBuildingData.size,
            currentBuildingData.color,
            myInstanceId
        );

        gridManager.OccupyCells(currentGridPos, currentBuildingData.size, myInstanceId);

        nextBuildingInstanceId++;

        //Debug.Log($"Placed {currentBuildingData.name} at {currentGridPos} (instance {myInstanceId})");
    }

    private void TryDeleteAtMouse()
    {
        Vector3 mouseWorld = GetMouseWorldPosition();
        Vector2Int pos = gridManager.WorldToGrid(mouseWorld);

        if (!gridManager.IsValidGridPosition(pos))
        {
            //Debug.Log("Clicked outside grid");
            return;
        }

        int instanceIdAtCell = gridManager.GetBuildingIdAt(pos);
        if (instanceIdAtCell < 0)
        {
            //Debug.Log("No building at clicked cell");
            return;
        }

        // Найдём объект Building с таким instanceId
        Building[] allBuildings = FindObjectsOfType<Building>();
        Building toDelete = null;
        foreach (var b in allBuildings)
        {
            if (b.instanceId == instanceIdAtCell)
            {
                toDelete = b;
                break;
            }
        }

        if (toDelete == null)
        {
            //Debug.LogWarning($"Found instanceId {instanceIdAtCell} in grid but no GameObject found");
            // Тем не менее освободим клетки (чтобы не остались заблокированы)
            // Но для этого нам нужен размер — попробуем получить по соседним клеткам: проще - пройдем по всем buildings and match by gridPosition maybe
            return;
        }

        // Освободим клетки
        gridManager.FreeCells(toDelete.gridPosition, toDelete.size);

        // Удаляем объект
        Destroy(toDelete.gameObject);

        //Debug.Log($"Deleted building instance {instanceIdAtCell} at {pos}");
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = new Vector3(mousePos.x, mousePos.y, -Camera.main.transform.position.z);
        return Camera.main.ScreenToWorldPoint(worldPos);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (currentBuildingData != null && previewObject != null)
        {
            Gizmos.color = canPlace ? new Color(0f, 1f, 0f, 0.4f) : new Color(1f, 0f, 0f, 0.4f);

            // нарисуем рамку по размеру здания
            Vector3 origin = new Vector3(currentGridPos.x, currentGridPos.y, 0);
            Vector3 size = new Vector3(currentBuildingData.size.x, currentBuildingData.size.y, 0);

            // прямоугольник по размеру здания
            Gizmos.DrawCube(origin + size / 2f, size);
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(origin + size / 2f, size);
        }
    }
#endif

}
