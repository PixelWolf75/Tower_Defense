using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ScriptableObject factory for creating and managing enemy instances with configurable properties.
/// Inherits from GameObjectFactory to provide scene management and object instantiation.
/// </summary>
[CreateAssetMenu]
public class EnemyFactory : GameObjectFactory
{
    /// <summary>
    /// Configuration data for a specific enemy type, containing all randomizable properties.
    /// Serializable class to allow Unity Inspector editing.
    /// </summary>
    [System.Serializable]
    class EnemyConfig
    {
        /// <summary>
        /// Prefab reference for this enemy type
        /// </summary>
        public Enemy prefab = default;

        /// <summary>
        /// Scale range for random enemy sizing (0.5x to 2x)
        /// </summary>
        [FloatRangeSlider(0.5f, 2f)]
        public FloatRange scale = new FloatRange(1f);

        /// <summary>
        /// Speed range for random enemy movement speed (0.2 to 5.0 units/second)
        /// </summary>
        [FloatRangeSlider(0.2f, 5f)]
        public FloatRange speed = new FloatRange(1f);

        /// <summary>
        /// Path offset range for random movement variation (-0.4 to 0.4 units)
        /// </summary>
        [FloatRangeSlider(-0.4f, 0.4f)]
        public FloatRange pathOffset = new FloatRange(0f);

        /// <summary>
        /// Health range for random enemy health points (10 to 1000 HP)
        /// </summary>
        [FloatRangeSlider(10f, 1000f)]
        public FloatRange health = new FloatRange(100f);
    }

    /// <summary>
    /// Configuration for small enemy type
    /// </summary>
    [SerializeField]
    EnemyConfig small = default;

    /// <summary>
    /// Configuration for medium enemy type
    /// </summary>
    [SerializeField]
    EnemyConfig medium = default;

    /// <summary>
    /// Configuration for large enemy type
    /// </summary>
    [SerializeField]
    EnemyConfig large = default;

    /// <summary>
    /// Retrieves the configuration data for a specified enemy type.
    /// </summary>
    /// <param name="type">The EnemyType to get configuration for</param>
    /// <returns>EnemyConfig containing all properties for the specified type, or null if type is unsupported</returns>
    EnemyConfig GetConfig(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Small: return small;
            case EnemyType.Medium: return medium;
            case EnemyType.Large: return large;
        }
        Debug.Assert(false, "Unsupported enemy type!");
        return null;
    }

    /// <summary>
    /// Creates and initializes a new enemy instance of the specified type.
    /// Applies random values from the type's configuration ranges.
    /// </summary>
    /// <param name="type">Type of enemy to create (defaults to Medium)</param>
    /// <returns>Fully initialized Enemy instance ready for gameplay</returns>
    public Enemy Get(EnemyType type = EnemyType.Medium)
    {
        // Get configuration for the requested type
        EnemyConfig config = GetConfig(type);

        // Create instance using inherited factory method
        Enemy instance = CreateGameObjectInstance(config.prefab);

        // Set factory reference for object pooling
        instance.OriginFactory = this;

        // Initialize with random values from configuration ranges
        instance.Initialize(config.scale.RandomValueInRange, config.health.RandomValueInRange);

        return instance;
    }

    /// <summary>
    /// Reclaims an enemy instance for cleanup/destruction.
    /// Validates that the enemy belongs to this factory before destroying it.
    /// </summary>
    /// <param name="enemy">The Enemy instance to reclaim and destroy</param>
    public void Reclaim(Enemy enemy)
    {
        // Ensure the enemy was created by this factory
        Debug.Assert(enemy.OriginFactory == this, "Wrong factory reclaimed!");

        // Destroy the enemy GameObject
        Destroy(enemy.gameObject);
    }
}
