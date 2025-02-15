using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class TilemapTerritoryCapture : MonoBehaviour
{
    [SerializeField] private Tilemap upperTilemap; // Border tiles
    [SerializeField] private Tilemap lowerTilemap; // Land tiles
    [SerializeField] private Tilemap clearedTilemap; // Cleared tiles
    [SerializeField] private Tile clearedTile; // Tile to replace land tiles
    private HashSet<Vector3Int> steppedOnTiles = new HashSet<Vector3Int>();
    private HashSet<Vector3Int> allLandTiles = new HashSet<Vector3Int>();
    private bool isOnLand = false;

    private void Start()
    {
        InitializeLandTiles();
    }

    private void InitializeLandTiles()
    {
        BoundsInt bounds = lowerTilemap.cellBounds;
        allLandTiles.Clear();

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                if (lowerTilemap.HasTile(position))
                {
                    allLandTiles.Add(position); // Store all initial land tiles
                }
            }
        }

        Debug.Log($"Initialized {allLandTiles.Count} land tiles.");
    }

    private void Update()
    {
        Vector3Int currentCell = lowerTilemap.WorldToCell(transform.position);
        bool currentlyOnLand = lowerTilemap.HasTile(currentCell);
        bool currentlyOnBorder = upperTilemap.HasTile(currentCell);
        bool currentlyOnCleared = clearedTilemap.HasTile(currentCell);

        // Player steps onto the land
        if (currentlyOnLand && !isOnLand)
        {
            Debug.Log("Player stepped onto the land!");
            isOnLand = true;
            steppedOnTiles.Clear();
        }

        // Player steps onto the border or cleared tiles (completing a loop)
        if ((currentlyOnBorder || currentlyOnCleared) && isOnLand)
        {
            Debug.Log("Player completed a loop!");
            isOnLand = false;

            // Clear enclosed area
            ClearEnclosedTiles();

            steppedOnTiles.Clear();
        }

        // Player steps on a land tile while moving
        if (currentlyOnLand && isOnLand)
        {
            if (!steppedOnTiles.Contains(currentCell))
            {
                steppedOnTiles.Add(currentCell); // Track stepped tiles
                lowerTilemap.SetTile(currentCell, clearedTile); // Mark as cleared
            }
        }
    }

    private void ClearEnclosedTiles()
    {
        if (steppedOnTiles == null || steppedOnTiles.Count == 0)
        {
            Debug.Log("No tiles to check for clearing.");
            return;
        }

        HashSet<Vector3Int> enclosedTiles = new HashSet<Vector3Int>();

        // Take the first stepped-on tile as the flood fill starting point
        Vector3Int startTile = steppedOnTiles.First();
        bool isEnclosed = FloodFill(startTile, enclosedTiles);

        if (isEnclosed)
        {
            foreach (Vector3Int tile in enclosedTiles)
            {
                lowerTilemap.SetTile(tile, null); // Remove tile from lowerTilemap
                clearedTilemap.SetTile(tile, clearedTile); // Add tile to clearedTilemap
            }
            Debug.Log($"Cleared {enclosedTiles.Count} enclosed tiles.");
        }
        else
        {
            Debug.Log("No enclosed area detected.");
        }

        steppedOnTiles.Clear(); // Reset steppedOnTiles after clearing
    }

    private bool FloodFill(Vector3Int start, HashSet<Vector3Int> region)
    {
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        queue.Enqueue(start);

        bool touchesBorderOrCleared = false;

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();

            // Skip if already visited
            if (visited.Contains(current)) continue;
            visited.Add(current);

            // Add current tile to region
            region.Add(current);

            // Check if current tile touches a border or cleared area
            if (upperTilemap.HasTile(current) || !lowerTilemap.HasTile(current))
            {
                touchesBorderOrCleared = true;
            }

            // Process neighbors
            foreach (Vector3Int neighbor in GetNeighbors(current))
            {
                // Add neighbor to queue if it's part of the lowerTilemap and not visited
                if (!visited.Contains(neighbor) && lowerTilemap.HasTile(neighbor))
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        return !touchesBorderOrCleared;
    }

    private IEnumerable<Vector3Int> GetNeighbors(Vector3Int tile)
    {
        yield return tile + Vector3Int.up;
        yield return tile + Vector3Int.down;
        yield return tile + Vector3Int.left;
        yield return tile + Vector3Int.right;
    }
}
