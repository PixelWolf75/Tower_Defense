using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Core game board system that manages the grid-based tower defense playing field.
/// Handles tile creation, pathfinding, content placement (walls, towers, spawn points, destinations),
/// visual display options (grid, path arrows), and ensures valid paths exist between spawn points and destinations.
/// Coordinates with the GameTileContentFactory to create and manage different types of tile content.
/// </summary>
public class GameBoard : MonoBehaviour
{
    /// <summary>
    /// Transform reference to the ground plane GameObject that serves as the visual base of the board.
    /// Used for scaling to match board dimensions and applying grid textures.
    /// </summary>
    [SerializeField]
    Transform ground = default;

    /// <summary>
    /// Prefab reference for creating individual game tiles that make up the board grid.
    /// Each tile handles its own content, pathfinding data, and neighbor relationships.
    /// </summary>
    [SerializeField]
    GameTile tilePrefab = default;

    /// <summary>
    /// Texture applied to the ground plane to display a grid pattern when ShowGrid is enabled.
    /// Tiled across the ground surface based on board dimensions to align with tile positions.
    /// </summary>
    [SerializeField]
    Texture2D gridTexture = default;

    /// <summary>
    /// Dimensions of the game board in tiles (width x height).
    /// Set during initialization and used for tile creation and coordinate calculations.
    /// </summary>
    Vector2Int size;

    /// <summary>
    /// Array containing all tile instances on the board, indexed by position.
    /// Layout: tiles[x + y * size.x] for tile at coordinates (x, y).
    /// </summary>
    GameTile[] tiles;

    /// <summary>
    /// Total number of spawn points that should be placed on the board during initialization.
    /// Used for random placement during board setup.
    /// </summary>
    int totalSpawnPoints;

    /// <summary>
    /// Total number of destinations that should be placed on the board during initialization.
    /// Used for random placement during board setup.
    /// </summary>
    int totalDestinations;

    /// <summary>
    /// Dynamic list of all active spawn point tiles on the board.
    /// Updated when spawn points are added or removed, used for enemy spawning.
    /// </summary>
    List<GameTile> spawnPoints = new List<GameTile>();

    /// <summary>
    /// Queue used for breadth-first search pathfinding algorithm.
    /// Temporarily stores tiles during path calculation from destinations to all reachable tiles.
    /// </summary>
    Queue<GameTile> searchFrontier = new Queue<GameTile>();

    /// <summary>
    /// List of tile content that requires per-frame updates (such as towers with targeting logic).
    /// Automatically updated during GameUpdate() method each frame.
    /// </summary>
    List<GameTileContent> updatingContent = new List<GameTileContent>();

    /// <summary>
    /// Factory reference for creating and managing different types of tile content.
    /// Provides centralized content creation and type management.
    /// </summary>
    GameTileContentFactory contentFactory;

    /// <summary>
    /// Flag controlling visibility of pathfinding arrows on tiles.
    /// When true, tiles display directional indicators showing the optimal path to destinations.
    /// </summary>
    bool showPaths, showGrid;

    /// <summary>
    /// Property that controls the visibility of pathfinding arrows on all tiles.
    /// When set to true, shows directional arrows indicating optimal paths to destinations.
    /// When set to false, hides all pathfinding visual indicators.
    /// </summary>
    public bool ShowPaths
    {
        get => showPaths;
        set
        {
            showPaths = value;
            if (showPaths)
            {
                foreach (GameTile tile in tiles)
                {
                    tile.ShowPath();
                }
            }
            else
            {
                foreach (GameTile tile in tiles)
                {
                    tile.HidePath();
                }
            }
        }
    }

    /// <summary>
    /// Property that controls the visibility of the grid texture on the ground plane.
    /// When true, applies the gridTexture with proper scaling to align with tile positions.
    /// When false, removes the texture to show a plain ground surface.
    /// </summary>
    public bool ShowGrid
    {
        get => showGrid;
        set
        {
            showGrid = value;
            Material m = ground.GetComponent<MeshRenderer>().material;
            if (showGrid)
            {
                m.mainTexture = gridTexture;
                m.SetTextureScale("_MainTex", size);
            }
            else
            {
                m.mainTexture = null;
            }
        }
    }

    /// <summary>
    /// Current number of active spawn points on the board.
    /// Used by the game system to determine spawn locations and validate enemy spawning.
    /// </summary>
    public int SpawnPointCount => spawnPoints.Count;

    /// <summary>
    /// Initializes the game board with specified dimensions and content settings.
    /// Creates all tiles, establishes neighbor relationships, places spawn points and destinations,
    /// and performs initial pathfinding to ensure board validity.
    /// </summary>
    /// <param name="size">Board dimensions in tiles (width x height)</param>
    /// <param name="contentFactory">Factory for creating tile content instances</param>
    /// <param name="numSpawnPoints">Number of spawn points to randomly place</param>
    /// <param name="numDestinations">Number of destinations to randomly place</param>
    public void Initialize(Vector2Int size, GameTileContentFactory contentFactory, int numSpawnPoints, int numDestinations)
    {
        this.size = size;
        ground.localScale = new Vector3(size.x, size.y, 1f);
        this.contentFactory = contentFactory;

        this.totalSpawnPoints = numSpawnPoints;
        this.totalDestinations = numDestinations;

        Vector2 offset = new Vector2(
            (size.x - 1) * 0.5f, (size.y - 1) * 0.5f
        );
        tiles = new GameTile[size.x * size.y];

        for (int i = 0, y = 0; y < size.y; y++)
        {

            for (int x = 0; x < size.x; x++, i++)
            {
                GameTile tile = tiles[i] = Instantiate(tilePrefab);
                tile.transform.SetParent(transform, false);
                tile.transform.localPosition = new Vector3(
                    x - offset.x, 0f, y - offset.y
                );


                if (x > 0)
                {
                    GameTile.MakeEastWestNeighbors(tile, tiles[i - 1]);
                }
                if (y > 0)
                {
                    GameTile.MakeNorthSouthNeighbors(tile, tiles[i - size.x]);
                }


                tile.IsAlternative = (x & 1) == 0;
                if ((y & 1) == 0)
                {
                    tile.IsAlternative = !tile.IsAlternative;
                }

            }

        }

        Clear();

        FindPaths();

        ShowPaths = showPaths;
        ShowGrid = showGrid;
    }

    /// <summary>
    /// Resets the board to an empty state and randomly places spawn points and destinations.
    /// Clears all existing content, creates a random selection pool to avoid overlap,
    /// and ensures proper distribution of essential tiles across the board.
    /// </summary>
    public void Clear()
    {
        foreach (GameTile tile in tiles)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Empty);
        }
        spawnPoints.Clear();
        updatingContent.Clear();
        if (tiles != null && tiles.Length > 0)
        {
            if (totalDestinations + totalSpawnPoints < tiles.Length)
            {

                // Create the pool
                List<int> randNumPool = new List<int>();
                for (int i = 0; i < tiles.Length; i++)
                {
                    randNumPool.Add(i);
                }

                List<int> destIndexes = new List<int>();
                List<int> spawnIndexes = new List<int>();

                // Fill destIndexes
                for (int i = 0; i < totalDestinations; i++)
                {
                    int index = Random.Range(0, randNumPool.Count);
                    destIndexes.Add(randNumPool[index]);
                    randNumPool.RemoveAt(index); // remove so it can't be reused
                }

                // Fill spawnIndexes
                for (int i = 0; i < totalSpawnPoints; i++)
                {
                    int index = Random.Range(0, randNumPool.Count);
                    spawnIndexes.Add(randNumPool[index]);
                    randNumPool.RemoveAt(index);
                }

                foreach (int index in spawnIndexes)
                {
                    ToggleSpawnPoint(tiles[index]);
                }

                foreach (int index in destIndexes)
                {
                    ToggleDestination(tiles[index]);
                }
            }
        }
    }

    /// <summary>
    /// Updates all tile content that requires per-frame processing.
    /// Called every frame during gameplay to update dynamic content like tower targeting,
    /// projectile movement, and other time-dependent tile behaviors.
    /// </summary>
    public void GameUpdate()
    {
        for (int i = 0; i < updatingContent.Count; i++)
        {
            updatingContent[i].GameUpdate();
        }
    }

    /// <summary>
    /// Performs breadth-first search pathfinding from all destination tiles to calculate optimal paths.
    /// Uses alternating priority directions based on tile checkerboard pattern to create varied movement.
    /// Validates that all tiles have reachable paths and updates visual path indicators.
    /// </summary>
    /// <returns>True if valid paths exist from all spawn points to destinations, false otherwise</returns>
    bool FindPaths()
    {
        int wallCount = 0;
        int destinationCount = 0;

        foreach (GameTile tile in tiles)
        {
            if (tile.Content.Type == GameTileContentType.Destination)
            {
                tile.BecomeDestination();
                searchFrontier.Enqueue(tile);
                destinationCount++;
            }
            else
            {
                tile.ClearPath();
                if (tile.Content.Type == GameTileContentType.Wall)
                {
                    wallCount++;
                }
            }

        }

        if (searchFrontier.Count == 0)
        {
            return false;
        }


        while (searchFrontier.Count > 0)
        {
            GameTile tile = searchFrontier.Dequeue();
            if (tile != null)
            {
                if (tile.IsAlternative)
                {
                    searchFrontier.Enqueue(tile.GrowPathNorth());
                    searchFrontier.Enqueue(tile.GrowPathSouth());
                    searchFrontier.Enqueue(tile.GrowPathEast());
                    searchFrontier.Enqueue(tile.GrowPathWest());
                }
                else
                {
                    searchFrontier.Enqueue(tile.GrowPathWest());
                    searchFrontier.Enqueue(tile.GrowPathEast());
                    searchFrontier.Enqueue(tile.GrowPathSouth());
                    searchFrontier.Enqueue(tile.GrowPathNorth());
                }
            }
        }


        foreach (GameTile tile in tiles)
        {
            if (!tile.HasPath)
            {
                return false;
            }
        }


        if (showPaths)
        {
            foreach (GameTile tile in tiles)
            {
                tile.ShowPath();
            }
        }
        else
        {
            foreach (GameTile tile in tiles)
            {
                tile.HidePath();
            }
        }

        return true;
    }

    /// <summary>
    /// Converts a ray cast (typically from mouse input) into a specific tile on the board.
    /// Performs physics ray casting and converts world coordinates to tile array indices.
    /// </summary>
    /// <param name="ray">Ray from camera through the desired world position</param>
    /// <returns>GameTile at the ray intersection point, or null if no valid tile is hit</returns>
    public GameTile GetTile(Ray ray)
    {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, float.MaxValue, 1))
        {
            int x = (int)(hit.point.x + size.x * 0.5f);
            int y = (int)(hit.point.z + size.y * 0.5f);
            if (x >= 0 && x < size.x && y >= 0 && y < size.y)
            {
                return tiles[x + y * size.x];
            }
        }
        return null;
    }

    /// <summary>
    /// Retrieves a specific spawn point tile by index for enemy spawning.
    /// Used by the game system to randomly select spawn locations for new enemies.
    /// </summary>
    /// <param name="index">Index into the spawnPoints list (0 to SpawnPointCount-1)</param>
    /// <returns>GameTile that serves as a spawn point for enemies</returns>
    public GameTile GetSpawnPoint(int index)
    {
        return spawnPoints[index];
    }

    /// <summary>
    /// Toggles a tile between destination and empty states.
    /// Validates that removing destinations doesn't break pathfinding connectivity.
    /// If pathfinding fails after removal, automatically reverts the change.
    /// </summary>
    /// <param name="tile">Tile to toggle destination status</param>
    /// <returns>True if the tile was successfully converted to a destination, false if removed or change blocked</returns>
    public bool ToggleDestination(GameTile tile)
    {
        if (tile.Content.Type == GameTileContentType.Destination)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Empty);
            if (!FindPaths())
            {
                tile.Content = contentFactory.Get(GameTileContentType.Destination);
                FindPaths();
                return true;
            }
            return false;
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Destination);
            FindPaths();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Toggles a tile between wall and empty states with pathfinding validation.
    /// Prevents wall placement that would block all paths from spawn points to destinations.
    /// Automatically reverts wall placement if it creates an invalid board state.
    /// </summary>
    /// <param name="tile">Tile to toggle wall status</param>
    /// <returns>True if a wall was successfully placed, false if removed or placement blocked</returns>
    public bool ToggleWall(GameTile tile)
    {
        if (tile.Content.Type == GameTileContentType.Wall)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Empty);
            FindPaths();
            return false;
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Wall);
            if (!FindPaths())
            {
                tile.Content = contentFactory.Get(GameTileContentType.Empty);
                FindPaths();
                return false;
            }
            
            return true;
        }

        return false;
    }

    /// <summary>
    /// Toggles a tile between tower and empty/wall states with pathfinding validation.
    /// Can convert walls directly to towers, handles updating content that requires per-frame updates.
    /// Prevents tower placement that would block all paths from spawn points to destinations.
    /// </summary>
    /// <param name="tile">Tile to toggle tower status</param>
    /// <returns>True if a tower was successfully placed, false if removed or placement blocked</returns>
    public bool ToggleTower(GameTile tile)
    {
        if (tile.Content.Type == GameTileContentType.Tower)
        {
            updatingContent.Remove(tile.Content);
            tile.Content = contentFactory.Get(GameTileContentType.Empty);
            FindPaths();
            return false;
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Tower);
            if (FindPaths())
            {
                updatingContent.Add(tile.Content);
            }
            else
            {
                tile.Content = contentFactory.Get(GameTileContentType.Empty);
                FindPaths();
                return false;
            }
            return true;
        }
        else if (tile.Content.Type == GameTileContentType.Wall)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Tower);
            updatingContent.Add(tile.Content);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Toggles a tile between spawn point and empty states.
    /// Maintains at least one spawn point on the board to ensure enemy spawning remains possible.
    /// Updates the spawn points list for enemy spawning system integration.
    /// </summary>
    /// <param name="tile">Tile to toggle spawn point status</param>
    /// <returns>True if a spawn point was successfully added, false if removed or change blocked</returns>
    public bool ToggleSpawnPoint(GameTile tile)
    {
        if (tile.Content.Type == GameTileContentType.SpawnPoint)
        {
            if (spawnPoints.Count > 1)
            {
                spawnPoints.Remove(tile);
                tile.Content = contentFactory.Get(GameTileContentType.Empty);
                return false;
            }
            return true;
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = contentFactory.Get(GameTileContentType.SpawnPoint);
            spawnPoints.Add(tile);
            return true;
        }

        return false;
    }
}