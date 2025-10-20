using UnityEngine;

[System.Serializable]
public class BuildingData
{
    public int id;
    public string name;
    public Vector2Int size;
    public Color color;
    public Sprite sprite;

    public BuildingData(int id, string name, int sizeX, int sizeY, Color color)
    {
        this.id = id;
        this.name = name;
        this.size = new Vector2Int(sizeX, sizeY);
        this.color = color;
    }
}