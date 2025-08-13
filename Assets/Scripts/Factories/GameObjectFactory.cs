using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Abstract base class for ScriptableObject-based factories that create and manage GameObjects.
/// Provides scene management functionality to organize created objects in separate scenes.
/// </summary>
public class GameObjectFactory : ScriptableObject
{
    /// <summary>
    /// Scene reference where all objects created by this factory will be placed.
    /// </summary>
    Scene scene;

    /// <summary>
    /// Creates instances of prefabs.
    /// Automatically handles scene creation and organization of instantiated objects.
    /// </summary>
    /// <typeparam name="T">Type of MonoBehaviour component on the prefab</typeparam>
    /// <param name="prefab">The prefab to instantiate</param>
    /// <returns>Instantiated object of type T, moved to the factory's dedicated scene</returns>
    protected T CreateGameObjectInstance<T>(T prefab) where T : MonoBehaviour
    {
        // Ensure we have a valid scene for this factory's objects
        if (!scene.isLoaded)
        {
            if (Application.isEditor)
            {
                // In editor, try to find existing scene first
                scene = SceneManager.GetSceneByName(name);
                if (!scene.isLoaded)
                {
                    // Create new scene if none exists
                    scene = SceneManager.CreateScene(name);
                }
            }
            else
            {
                // In builds, always create new scene
                scene = SceneManager.CreateScene(name);
            }
        }

        // Instantiate the prefab
        T instance = Instantiate(prefab);

        // Move the created object to this factory's scene for organization
        SceneManager.MoveGameObjectToScene(instance.gameObject, scene);

        return instance;
    }
}
