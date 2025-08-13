using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Needed for scene loading

/// <summary>
/// Used for loading Unity scenes by name.
/// Provides a clean interface for UI buttons and other components to trigger scene transitions.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    /// <summary>
    /// Loads a Unity scene by its registered name.
    /// Uses SceneManager to perform immediate scene loading, replacing the current scene.
    /// </summary>
    /// <param name="sceneName">The exact name of the scene to load (must match the scene name in Build Settings)</param>
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}