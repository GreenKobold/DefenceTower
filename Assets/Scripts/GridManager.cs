using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int gridWidth = 20;
    public int gridHeight = 20;
    public float cellSize = 5f;
    public Material gridMaterial;
    public GameObject highlightPrefab;
    public Transform plane;                // Your grid plane
   // A simple quad or decal used for highlighting
    private GameObject highlightInstance;

    private Vector3 gridOrigin;

    private bool[,] hasTower;

    void Start()
    {
        // REAL plane size in world units (Unity Plane is 10x10 by default)
        float planeWidth = plane.localScale.x * 10f;
        float planeHeight = plane.localScale.z * 10f;

        // Recalculate grid size automatically
        gridWidth = Mathf.FloorToInt(planeWidth / cellSize);
        gridHeight = Mathf.FloorToInt(planeHeight / cellSize);

        // Compute origin correctly
        gridOrigin = plane.position - new Vector3(planeWidth * 0.5f, 0, planeHeight * 0.5f);

        // Init occupancy
        hasTower = new bool[gridWidth, gridHeight];

        // Create highlight instance
        highlightInstance = Instantiate(highlightPrefab);
        highlightInstance.SetActive(false);
        gridMaterial.SetVector("_GridCount", new Vector4(gridWidth, gridHeight, 0, 0));
    }

    void Update()
    {
        UpdateHoverHighlight();
    }

    void UpdateHoverHighlight()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
        {
            highlightInstance.SetActive(false);
            return;
        }

        // Convert world position to grid coordinates
        Vector3 localPos = hit.point - gridOrigin;

        int x = Mathf.FloorToInt(localPos.x / cellSize);
        int y = Mathf.FloorToInt(localPos.z / cellSize);

        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
        {
            highlightInstance.SetActive(false);
            return;
        }

        // Position highlight tile
        Vector3 tileCenter = GetTileCenter(x, y);
        highlightInstance.transform.position = tileCenter + Vector3.up * 0.01f; // slight offset
        highlightInstance.transform.localScale = new Vector3(cellSize, cellSize, cellSize);

        highlightInstance.SetActive(true);
    }

    public Vector3 GetTileCenter(int x, int y)
    {
        return gridOrigin + new Vector3((x + 0.5f) * cellSize, 0, (y + 0.5f) * cellSize);
    }

    public bool IsTileFree(int x, int y)
    {
        return !hasTower[x, y];
    }

    public void PlaceTower(int x, int y)
    {
        hasTower[x, y] = true;
    }
}
