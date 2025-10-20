using UnityEngine;
using UnityEngine.UI;

public class PlaceScript : MonoBehaviour
{
    [Header("References")]
    public BuildingManager buildingManager;


    public void OnBuildingButtonClick(int buildingId)
    {
        buildingManager.StartPlacement(buildingId);
        Debug.Log($"Button clicked: building {buildingId}");
    }
}