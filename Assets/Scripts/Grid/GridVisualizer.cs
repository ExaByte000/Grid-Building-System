using UnityEngine;


    /// <summary>
    /// Отвечает за визуализацию сетки
    /// </summary>
    [RequireComponent(typeof(GridManager))]
    public class GridVisualizer : MonoBehaviour
    {
        [Header("Visualization Settings")]
        [SerializeField] private bool showGrid = true;
        [SerializeField] private Color lightCellColor = new Color(0.9f, 0.9f, 0.9f, 0.3f);
        [SerializeField] private Color darkCellColor = new Color(0.7f, 0.7f, 0.7f, 0.3f);
        [SerializeField] private bool showInGameView = false; 

        private GridManager gridManager;
        private GameObject cellsContainer;

        private void Awake()
        {
            gridManager = GetComponent<GridManager>();
        }

        private void Start()
        {
            if (showInGameView)
            {
                CreateLineRenderers();
            }
        }
        /// <summary>
        /// Создание шахматной сетки из спрайтов
        /// </summary>
        private void CreateLineRenderers()
        {
            cellsContainer = new GameObject("GridCells");
            cellsContainer.transform.SetParent(transform);

            for (int x = 0; x < gridManager.Width; x++)
            {
                for (int y = 0; y < gridManager.Height; y++)
                {
                    CreateCellSprite(x, y);
                }
            }
        }

        /// <summary>
        /// Создание одной клетки-спрайта
        /// </summary>
        private void CreateCellSprite(int x, int y)
        {
            GameObject cellObj = new GameObject($"Cell_{x}_{y}");
            cellObj.transform.SetParent(cellsContainer.transform);

            Vector3 worldPos = new Vector3(
                x + gridManager.Offset.x + 0.5f,
                y + gridManager.Offset.y + 0.5f,
                0
            );
            cellObj.transform.position = worldPos;

            SpriteRenderer sr = cellObj.AddComponent<SpriteRenderer>();

            sr.sprite = CreateSquareSprite();

            bool isLight = (x + y) % 2 == 0;
            sr.color = isLight ? lightCellColor : darkCellColor;

            sr.sortingOrder = 2;
        }

        /// <summary>
        /// Создание квадратного спрайта 1x1
        /// </summary>
        private Sprite CreateSquareSprite()
        {
            // Создаём белую текстуру 1x1
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            // Преобразуем в спрайт
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f), 
                1f
            );

            return sprite;
        }

        /// <summary>
        /// Рисование сетки в редакторе (Gizmos)
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!showGrid)
                return;

            
            if (gridManager == null)
                gridManager = GetComponent<GridManager>();

            if (gridManager == null)
                return;

            Gizmos.color = Color.white;

            Vector2Int offset = gridManager.Offset;
            int width = gridManager.Width;
            int height = gridManager.Height;

            
            for (int x = 0; x <= width; x++)
            {
                Vector3 start = new Vector3(x + offset.x, offset.y, 0);
                Vector3 end = new Vector3(x + offset.x, height + offset.y, 0);
                Gizmos.DrawLine(start, end);
            }

            
            for (int y = 0; y <= height; y++)
            {
                Vector3 start = new Vector3(offset.x, y + offset.y, 0);
                Vector3 end = new Vector3(width + offset.x, y + offset.y, 0);
                Gizmos.DrawLine(start, end);
            }
        }

        /// <summary>
        /// Включить/выключить отображение сетки
        /// </summary>
        public void SetGridVisibility(bool visible)
        {
            showGrid = visible;

            if (cellsContainer != null)
            {
                cellsContainer.SetActive(visible);
            }
        }
    }
