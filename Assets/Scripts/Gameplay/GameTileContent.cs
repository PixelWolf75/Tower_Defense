using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enumeration defining all possible types of content that can be placed on game tiles
/// </summary>
public enum GameTileContentType
{
    Empty,      // No content, passable
    Destination,// End point for enemy paths
    Wall,       // Blocks enemy movement and paths
    SpawnPoint, // Starting point for enemy spawning
    Tower       // Defensive structure that attacks enemies
}

/// <summary>
/// Base class for all content that can be placed on game tiles.
/// Provides common functionality for content management, factory relationships, and pathfinding interaction.
/// </summary>
[SelectionBase] // Makes this the primary selection target in Unity's scene view
public class GameTileContent : MonoBehaviour
{
    /// <summary>
    /// The specific type of content this instance represents
    /// </summary>
    [SerializeField]
    GameTileContentType type = default;

    /// <summary>
    /// Read-only property exposing the content type for external queries
    /// </summary>
    /// <value>The GameTileContentType of this content instance</value>
    public GameTileContentType Type => type;

    /// <summary>
    /// Property indicating whether this content blocks enemy pathfinding.
    /// </summary>
    /// <value>True if content blocks paths (walls and towers), false otherwise</value>
    public bool BlocksPath => Type == GameTileContentType.Wall || Type == GameTileContentType.Tower;

    /// <summary>
    /// Reference to the factory that created this content instance, used for object pooling
    /// </summary>
    GameTileContentFactory originFactory;


    /// <summary>
    /// Virtual method for game-specific update logic called by the game loop.
    /// </summary>
    public virtual void GameUpdate() { }

    /// <summary>
    /// Property for setting the origin factory reference. Can only be set once to prevent reassignment.
    /// Used for object pooling and proper cleanup when content is recycled.
    /// </summary>
    /// <value>The GameTileContentFactory that created this content instance</value>
    public GameTileContentFactory OriginFactory
    {
        get => originFactory;
        set
        {
            Debug.Assert(originFactory == null, "Redefined origin factory!");
            originFactory = value;
        }
    }

    /// <summary>
    /// Recycles this content instance back to its origin factory for cleanup and reuse.
    /// Called when content is removed from tiles or when the game resets.
    /// </summary>
    public void Recycle()
    {
        originFactory.Reclaim(this);
    }
}
