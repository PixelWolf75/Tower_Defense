using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Serializable collection class that manages multiple GameBehaviour objects.
/// Handles batch updates and automatic removal of objects that return false from GameUpdate().
/// </summary>
[System.Serializable]
public class GameBehaviourCollection
{
    /// <summary>
    /// Internal list storing all active GameBehaviour objects
    /// </summary>
    List<GameBehaviour> behaviours = new List<GameBehaviour>();

    /// <summary>
    /// Read-only property indicating whether the collection contains any GameBehaviour objects
    /// </summary>
    /// <value>True if no objects are in the collection, false if one or more objects exist</value>
    public bool IsEmpty => behaviours.Count == 0;

    /// <summary>
    /// Adds a GameBehaviour object to the collection for management and updates.
    /// </summary>
    /// <param name="behaviour">The GameBehaviour object to add to the collection</param>
    public void Add(GameBehaviour behaviour)
    {
        behaviours.Add(behaviour);
    }

    /// <summary>
    /// Automatically removes objects that return false from GameUpdate() using efficient swap-and-pop technique.
    /// </summary>
    public void GameUpdate()
    {
        for (int i = 0; i < behaviours.Count; i++)
        {
            // Call GameUpdate on current behaviour
            if (!behaviours[i].GameUpdate())
            {
                // Object should be removed - use efficient removal technique
                int lastIndex = behaviours.Count - 1;
                behaviours[i] = behaviours[lastIndex]; // Replace current with last item
                behaviours.RemoveAt(lastIndex); // Remove last item (now duplicate)
                i -= 1; // Decrement index to re-check the swapped item
            }
        }
    }

    /// <summary>
    /// Clears the entire collection by calling Recycle() on all objects and emptying the list.
    /// Ensures proper cleanup of all managed GameBehaviour objects before clearing.
    /// </summary>
    public void Clear()
    {
        // Recycle all objects before clearing
        for (int i = 0; i < behaviours.Count; i++)
        {
            behaviours[i].Recycle();
        }

        // Clear the collection
        behaviours.Clear();
    }
}