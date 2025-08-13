using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ScriptableObject factory for creating and managing game tile content instances.
/// Inherits from GameObjectFactory to provide scene management and handles all tile content types.
/// </summary>
[CreateAssetMenu]
public class GameTileContentFactory : GameObjectFactory
{
    /// <summary>
    /// Prefab reference for destination tile content
    /// </summary>
    [SerializeField]
    GameTileContent destinationPrefab = default;

    /// <summary>
    /// Prefab reference for empty tile content
    /// </summary>
    [SerializeField]
    GameTileContent emptyPrefab = default;

    /// <summary>
    /// Prefab reference for wall tile content
    /// </summary>
    [SerializeField]
    GameTileContent wallPrefab = default;

    /// <summary>
    /// Prefab reference for spawn point tile content
    /// </summary>
    [SerializeField]
    GameTileContent spawnPointPrefab = default;

    /// <summary>
    /// Prefab reference for tower tile content
    /// </summary>
    [SerializeField]
    Tower towerPrefab = default;

    /// <summary>
    /// Reclaims a tile content instance for cleanup and destruction.
    /// Validates that the content belongs to this factory before destroying it.
    /// </summary>
    /// <param name="content">The GameTileContent instance to reclaim and destroy</param>
    public void Reclaim(GameTileContent content)
    {
        Debug.Assert(content.OriginFactory == this, "Wrong factory reclaimed!");
        Destroy(content.gameObject);
    }

    /// <summary>
    /// Private helper method that creates an instance from a specific prefab.
    /// Sets up the factory reference and handles instantiation.
    /// </summary>
    /// <param name="prefab">The GameTileContent prefab to instantiate</param>
    /// <returns>Fully initialized GameTileContent instance</returns>
    GameTileContent Get(GameTileContent prefab)
    {
        GameTileContent instance = Instantiate(prefab);
        instance.OriginFactory = this;
        return instance;
    }

    /// <summary>
    /// Creates and returns a tile content instance of the specified type.
    /// Maps content types to their corresponding prefabs and creates instances.
    /// </summary>
    /// <param name="type">The type of tile content to create</param>
    /// <returns>Initialized GameTileContent instance of the requested type, or null if type is unsupported</returns>
    public GameTileContent Get(GameTileContentType type)
    {
        switch (type)
        {
            case GameTileContentType.Destination:
                return Get(destinationPrefab);
            case GameTileContentType.Empty:
                return Get(emptyPrefab);
            case GameTileContentType.Wall:
                return Get(wallPrefab);
            case GameTileContentType.SpawnPoint:
                return Get(spawnPointPrefab);
            case GameTileContentType.Tower:
                return Get(towerPrefab);
        }

        Debug.Assert(false, "Unsupported type: " + type);
        return null;
    }
}