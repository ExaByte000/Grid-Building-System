using UnityEngine;
using UnityEngine.InputSystem;

namespace Grid
{
    /// <summary>
    /// Подсвечивает клетку под курсором мыши
    /// </summary>
    public class GridHighlight : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GridManager gridManager;

        [Header("Highlight Settings")]
        [SerializeField] private Color highlightColor = new Color(1f, 1f, 0f, 0.5f);
        [SerializeField] private bool followMouse = true;

        private SpriteRenderer spriteRenderer;
        private Vector2Int currentGridPosition;
        private Vector2Int previousGridPosition;
        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
            SetupSprite();

            if (gridManager == null)
            {
                gridManager = FindObjectOfType<GridManager>();
            }

            if (gridManager == null)
            {
                Debug.LogError("GridManager not found! Assign it in Inspector.");
                enabled = false;
            }
        }

        /// <summary>
        /// Настройка спрайта для подсветки
        /// </summary>
        private void SetupSprite()
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f),
                1f
            );

            spriteRenderer.sprite = sprite;
            spriteRenderer.color = highlightColor;
            spriteRenderer.sortingOrder = 10;

            spriteRenderer.enabled = false;
        }

        private void Update()
        {
            if (!followMouse)
                return;

            UpdateHighlightPosition();
            Debug.Log($"Mouse grid pos: {currentGridPosition}, Active: {gameObject.activeSelf}");
        }

        /// <summary>
        /// Обновление позиции подсветки по курсору мыши
        /// </summary>
        private void UpdateHighlightPosition()
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            currentGridPosition = gridManager.WorldToGrid(mouseWorldPos);
            
            if (currentGridPosition != previousGridPosition)
            {
                if (gridManager.IsValidGridPosition(currentGridPosition))
                {
                    spriteRenderer.enabled = true;

                    Vector3 cellCenter = gridManager.GridToWorld(currentGridPosition);
                    cellCenter.x += 0.5f; 
                    cellCenter.y += 0.5f;
                    transform.position = cellCenter;
                }
                else
                {
                    spriteRenderer.enabled = false;
                }

                previousGridPosition = currentGridPosition;
            }
        }

        /// <summary>
        /// Получение мировых координат курсора мыши
        /// </summary>
        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mouseScreenPos = Mouse.current.position.ReadValue();

            //Vector3 mousePos = new Vector3(mouseScreenPos.x, mouseScreenPos.y, -mainCamera.transform.position.z);
            mouseScreenPos.z = -mainCamera.transform.position.z;
            return mainCamera.ScreenToWorldPoint(mouseScreenPos);
        }

        /// <summary>
        /// Установить цвет подсветки (для разных режимов)
        /// </summary>
        public void SetHighlightColor(Color color)
        {
            highlightColor = color;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
        }

        /// <summary>
        /// Получить текущую позицию на сетке
        /// </summary>
        public Vector2Int GetcurrentGridPosition()
        {
            return currentGridPosition;
        }

        /// <summary>
        /// Включить/выключить следование за мышью
        /// </summary>
        public void SetFollowMouse(bool follow)
        {
            followMouse = follow;
            if (!follow)
            {
                spriteRenderer.enabled = false;
            }
        }

        /// <summary>
        /// Показать подсветку в конкретной позиции
        /// </summary>
        public void ShowAt(Vector2Int gridPosition)
        {
            if (gridManager.IsValidGridPosition(gridPosition))
            {
                spriteRenderer.enabled = true;
                Vector3 cellCenter = gridManager.GridToWorld(gridPosition);
                cellCenter.x += 0.5f;
                cellCenter.y += 0.5f;
                transform.position = cellCenter;
            }
        }

        /// <summary>
        /// Скрыть подсветку
        /// </summary>
        public void Hide()
        {
            spriteRenderer.enabled = false;
        }
    }
}