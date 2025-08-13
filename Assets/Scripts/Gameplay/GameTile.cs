using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Represents a single tile in the game board grid system.
/// Manages pathfinding, neighbor relationships, content assignment, and visual path indicators.
/// </summary>
public class GameTile : MonoBehaviour
{
    // <summary>
    /// Transform reference to the directional arrow used for path visualization
    /// </summary>
    [SerializeField]
    Transform arrow = default;

    /// <summary>
    /// Current content placed on this tile (walls, towers, etc.)
    /// </summary>
    GameTileContent content;

    /// <summary>
    /// References to neighboring tiles in cardinal directions
    /// </summary>
    GameTile north, east, south, west;

    /// <summary>
    /// Reference to the next tile in the optimal path toward destinations
    /// </summary>
    GameTile nextOnPath;

    /// <summary>
    /// Distance value used in pathfinding algorithm (steps to nearest destination)
    /// </summary>
    int distance;

    /// <summary>
    ///  access to the next tile in the pathfinding route
    /// </summary>
    /// <value>The next GameTile to move to when following the optimal path</value>
    public GameTile NextTileOnPath => nextOnPath;

    /// <summary>
    /// rotation values for arrow directions
    /// </summary>
    static Quaternion
        northRotation = Quaternion.Euler(90f, 0f, 0f),
        eastRotation = Quaternion.Euler(90f, 90f, 0f),
        southRotation = Quaternion.Euler(90f, 180f, 0f),
        westRotation = Quaternion.Euler(90f, 270f, 0f);


    /// <summary>
    /// Static method to establish bidirectional neighbor relationship between east and west tiles.
    /// Ensures tiles are properly linked for pathfinding algorithms.
    /// </summary>
    /// <param name="east">The tile that will be on the east side</param>
    /// <param name="west">The tile that will be on the west side</param>
    public static void MakeEastWestNeighbors(GameTile east, GameTile west)
    {
        Debug.Assert(
            west.east == null && east.west == null, "Redefined neighbors!"
        );

        west.east = east;
        east.west = west;
    }


    /// <summary>
    /// Static method to establish bidirectional neighbor relationship between north and south tiles.
    /// Ensures tiles are properly linked for pathfinding algorithms.
    /// </summary>
    /// <param name="north">The tile that will be on the north side</param>
    /// <param name="south">The tile that will be on the south side</param>
    public static void MakeNorthSouthNeighbors(GameTile north, GameTile south)
    {
        Debug.Assert(
            south.north == null && north.south == null, "Redefined neighbors!"
        );
        south.north = north;
        north.south = south;
    }

    /// <summary>
    /// Clears pathfinding data by resetting distance to maximum and removing path reference.
    /// Used when recalculating paths after board changes.
    /// </summary>
    public void ClearPath()
    {
        distance = int.MaxValue;
        nextOnPath = null;
    }

    /// <summary>
    /// Marks this tile as a destination point in the pathfinding system.
    /// Sets distance to 0 and clears the next path reference since destinations are endpoints.
    /// </summary>
    public void BecomeDestination()
    {
        distance = 0;
        nextOnPath = null;
    }

    /// <summary>
    /// Property indicating whether this tile has a valid path to a destination.
    /// Used to validate pathfinding results and ensure all tiles are reachable.
    /// </summary>
    /// <value>True if the tile has a calculated path, false if unreachable</value>
    public bool HasPath
    {
        get
        {
            return distance != int.MaxValue;
        }
    }

    /// <summary>
    /// Attempts to extend the pathfinding algorithm to a neighboring tile.
    /// Updates neighbor's distance and path reference if it doesn't already have a shorter path.
    /// </summary>
    /// <param name="neighbor">The neighboring tile to potentially add to the path</param>
    /// <returns>The neighbor tile if path was extended, null if blocked or already has path</returns>
    GameTile GrowPathTo(GameTile neighbor)
    {
        Debug.Assert(HasPath, "No path!");

        // Skip if neighbor doesn't exist or already has a path
        if (neighbor == null || neighbor.HasPath)
        {
            return null;
        }

        // Set neighbor's pathfinding data
        neighbor.distance = distance + 1;
        neighbor.nextOnPath = this;

        // Return neighbor only if it doesn't block paths (for continued expansion)
        return neighbor.Content.BlocksPath ? null : neighbor;
    }

    /// <summary>
    /// Extends pathfinding to the north neighbor tile.
    /// </summary>
    /// <returns>North neighbor if path extended successfully, null otherwise</returns>
    public GameTile GrowPathNorth() => GrowPathTo(north);

    /// <summary>
    /// Extends pathfinding to the east neighbor tile.
    /// </summary>
    /// <returns>East neighbor if path extended successfully, null otherwise</returns>
    public GameTile GrowPathEast() => GrowPathTo(east);

    /// <summary>
    /// Extends pathfinding to the south neighbor tile.
    /// </summary>
    /// <returns>South neighbor if path extended successfully, null otherwise</returns>
    public GameTile GrowPathSouth() => GrowPathTo(south);

    /// <summary>
    /// Extends pathfinding to the west neighbor tile.
    /// </summary>
    /// <returns>West neighbor if path extended successfully, null otherwise</returns>
    public GameTile GrowPathWest() => GrowPathTo(west);

    /// <summary>
    /// Property used for alternate tile coloring or processing patterns.
    /// Set during board initialization based on tile position.
    /// </summary>
    /// <value>True if this tile uses alternative processing/coloring, false otherwise</value>
    public bool IsAlternative { get; set; }

    /// <summary>
    /// Activates and orients the directional arrow to show the path direction.
    /// Hides arrow for destination tiles (distance 0) since they are endpoints.
    /// </summary>
    public void ShowPath()
    {
        // Hide arrow for destination tiles
        if (distance == 0)
        {
            arrow.gameObject.SetActive(false);
            return;
        }

        // Show and orient arrow based on next tile direction
        arrow.gameObject.SetActive(true);
        arrow.localRotation =
            nextOnPath == north ? northRotation :
            nextOnPath == east ? eastRotation :
            nextOnPath == south ? southRotation :
            westRotation;
    }

    /// <summary>
    /// Hides the directional arrow, typically when path visualization is disabled.
    /// </summary>
    public void HidePath()
    {
        arrow.gameObject.SetActive(false);
    }

    /// <summary>
    /// Property for getting and setting the content placed on this tile.
    /// Handles cleanup of previous content and positioning of new content.
    /// </summary>
    /// <value>The GameTileContent currently placed on this tile</value>
    public GameTileContent Content
    {
        get => content;
        set
        {
            Debug.Assert(value != null, "Null assigned to content!");

            // Recycle previous content if it exists
            if (content != null)
            {
                content.Recycle();
            }

            // Assign new content and position it correctly
            content = value;
            content.transform.localPosition = transform.localPosition;
        }
    }

}
