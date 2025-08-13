using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Enumeration defining different enemy sizes/types
/// </summary>
public enum EnemyType
{
    Small, Medium, Large
}


/// <summary>
/// Core enemy class that handles movement, health, visual effects, and game interactions.
/// Inherits from GameBehaviour to participate in the game's update loop and object pooling system.
/// </summary>
public class Enemy : GameBehaviour
{
    /// <summary>
    /// Transform reference to the enemy's visual model for scaling
    /// </summary>
    [SerializeField]
    Transform model = default;

    /// <summary>
    /// Reference to the Blink component for visual feedback when health is low
    /// </summary>
    [SerializeField]
    Blink blinker = default;

    /// <summary>
    /// Component that manages the enemy's health bar UI
    /// </summary>
    private EnemyHealthBarManager healthBarManager;

    /// <summary>
    /// Reference to the factory that created this enemy, used for object pooling
    /// </summary>
    EnemyFactory originFactory;

    /// <summary>
    /// Current tile the enemy is moving from in its path
    /// </summary>
    GameTile tileFrom;

    /// <summary>
    /// Next tile the enemy is moving towards in its path
    /// </summary>
    GameTile tileTo;

    /// <summary>
    /// World position the enemy is moving from
    /// </summary>
    Vector3 positionFrom;

    /// <summary>
    /// World position the enemy is moving towards
    /// </summary>
    Vector3 positionTo;

    /// <summary>
    /// Movement progress between tiles (0.0 to 1.0)
    /// </summary>
    float progress;

    /// <summary>
    /// Flag tracking whether the enemy is currently blinking
    /// </summary>
    private bool bIsBlinking = false;

    /// <summary>
    /// Current visual scale of the enemy (changes based on health)
    /// </summary>
    public float Scale { get; private set; }

    /// <summary>
    /// Maximum scale value, stored for health-based scaling calculations
    /// </summary>
    private float maxScale;

    /// <summary>
    /// Current health points of the enemy
    /// </summary>
    float Health { get; set; }

    /// <summary>
    /// Maximum health points, used for health bar and scaling calculations
    /// </summary>
    float MaxHealth { get; set; }


    /// <summary>
    /// Property for setting the origin factory. Can only be set once to prevent reassignment.
    /// </summary>
    /// <value>The EnemyFactory that created this enemy instance</value>
    public EnemyFactory OriginFactory
    {
        get => originFactory;
        set
        {
            Debug.Assert(originFactory == null, "Redefined origin factory!");
            originFactory = value;
        }
    }

    /// <summary>
    /// gets required components before Start().
    /// Called when the object is first created.
    /// </summary>
    void Awake()
    {
        // Get the health bar manager component
        healthBarManager = GetComponent<EnemyHealthBarManager>();
    }


    /// <summary>
    /// Initializes the enemy with specified scale and health values.
    /// Sets up visual scale, health values, and updates the health bar.
    /// </summary>
    /// <param name="scale">Visual scale multiplier for the enemy model</param>
    /// <param name="health">Starting and maximum health points</param>
    public void Initialize(float scale, float health)
    {
        Scale = scale;
        maxScale = Scale;
        model.localScale = new Vector3(scale, scale, scale);

        Health = health;
        MaxHealth = health;

        // Update health bar if available
        if (healthBarManager != null)
        {
            healthBarManager.UpdateHealth(Health, MaxHealth);
        }
    }

    /// <summary>
    /// Spawns the enemy on a specific tile and sets up initial movement path.
    /// Shows the health bar and initializes position/movement variables.
    /// </summary>
    /// <param name="tile">The GameTile where the enemy should spawn</param>
    public void SpawnOn(GameTile tile)
    {
        tileFrom = tile;
        tileTo = tile.NextTileOnPath;
        positionFrom = tileFrom.transform.localPosition;
        positionTo = tileTo.transform.localPosition;
        progress = 0f;

        // Show health bar when spawned
        if (healthBarManager != null)
        {
            healthBarManager.ShowHealthBar(true);
        }
        
    }


    /// <summary>
    /// Main game update method called by the game loop.
    /// Handles health-based visual effects, movement, death, and destination reaching.
    /// </summary>
    /// <returns>True if enemy should continue existing, false if it should be removed</returns>
    public override bool GameUpdate()
    {
        // Start blinking when health drops to 50 or below
        if (Health <= 50f && !bIsBlinking)
        {
            blinker.StartBlinking();
            bIsBlinking = true;
        }

        // Increase blinking speed when health is critically low
        if (Health <= 10f)
        {
            blinker.SetBlinkInterval(0.01f);
        }

        // Handle death
        if (Health <= 0f)
        {
            blinker.CancelBlinking();

            // Hide health bar before recycling
            if (healthBarManager != null)
            {
                healthBarManager.ShowHealthBar(false);
            }

            Game.EnemyHasBeenKilled();
            Recycle();
            return false; // Remove from game loop
        }

        // Handle movement along the path
        progress += Time.deltaTime;
        while (progress >= 1f)
        {
            // Move to next tile in path
            tileFrom = tileTo;
            tileTo = tileTo.NextTileOnPath;

            // Check if reached destination
            if (tileTo == null)
            {
                // Hide health bar when reaching destination
                if (healthBarManager != null)
                {
                    healthBarManager.ShowHealthBar(false);
                }

                Game.EnemyReachedDestination();
                Recycle();
                return false; // Remove from game loop
            }

            // Set up movement to next tile
            positionFrom = positionTo;
            positionTo = tileTo.transform.localPosition;
            progress -= 1f;
        }

        // Smoothly interpolate position between tiles
        transform.localPosition = Vector3.LerpUnclamped(positionFrom, positionTo, progress);
        return true; // Continue existing in game loop
    }

    /// <summary>
    /// Applies damage to the enemy, reducing health and triggering visual effects.
    /// Updates health bar and shrinks the enemy model based on remaining health.
    /// </summary>
    /// <param name="damage">Amount of damage to apply (must be non-negative)</param>
    public void ApplyDamage(float damage)
    {
        Debug.Assert(damage >= 0f, "Negative damage applied.");
        Health -= damage;

        // Update visual scale based on health
        Shrink();

        // Update health bar display
        if (healthBarManager != null)
        {
            healthBarManager.UpdateHealth(Health, MaxHealth);
        }
    }

    /// <summary>
    /// Shrinks the enemy model based on current health percentage.
    /// Lower health results in smaller scale, providing visual feedback.
    /// </summary>
    void Shrink()
    {
        // Calculate scale based on health percentage
        Scale = maxScale * (Health / MaxHealth);
        Debug.Log(Scale);
        model.localScale = new Vector3(Scale, Scale, Scale);
    }

    /// <summary>
    /// Recycles the enemy object back to its origin factory for reuse.
    /// Hides health bar and returns the object to the object pool.
    /// </summary>
    public override void Recycle()
    {
        // Hide health bar before recycling
        if (healthBarManager != null)
        {
            healthBarManager.ShowHealthBar(false);
        }

        // Return to factory's object pool
        OriginFactory.Reclaim(this);
    }
}
