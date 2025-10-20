using UnityEngine;

public class BuildModeController : MonoBehaviour
{
    [SerializeField] private BuildingManager buildingManager;

    private void Awake()
    {
        if (buildingManager == null)
            buildingManager = FindObjectOfType<BuildingManager>();
    }
    private void Start()
    {
        buildingManager.SetMode(BuildMode.Placement);
    }

    /// <summary>
    /// Включить режим размещения зданий
    /// </summary>
    public void EnablePlacementMode()
    {
        buildingManager.SetMode(BuildMode.Placement);
    }

    public void EnableDeleteMode()
    {
        buildingManager.SetMode(BuildMode.Delete);
    }

    //public void DisableAllModes()
    //{
    //    buildingManager.SetMode(BuildMode.None);
    //}
}
