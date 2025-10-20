using UnityEngine;

public class Building : MonoBehaviour
{
    public int buildingId;
    public int instanceId;
    public Vector2Int gridPosition;
    public Vector2Int size;

    private SpriteRenderer spriteRenderer;

    public void Initialize(int id, Vector2Int pos, Vector2Int size, Color color, int instanceId)
    {
        this.buildingId = id;
        this.gridPosition = pos;
        this.size = size;
        this.instanceId = instanceId;

        spriteRenderer = GetComponent<SpriteRenderer>();
        

        // применяем цвет наружу (в т.ч. alpha = 1)
        spriteRenderer.color = color;
        spriteRenderer.sortingOrder = 1;

        float centerX = pos.x + size.x / 2f;
        float centerY = pos.y + size.y / 2f;
        transform.position = new Vector3(centerX, centerY, 0);

        gameObject.name = $"{instanceId}_Building_{id}_({pos.x},{pos.y})";
    }
}
