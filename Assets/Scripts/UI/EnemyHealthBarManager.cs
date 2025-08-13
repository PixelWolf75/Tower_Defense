using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages health bar UI elements for individual enemies, including creation, positioning, and visibility.
/// Handles world-space health bars that follow enemies and face the camera.
/// </summary>
public class EnemyHealthBarManager : MonoBehaviour
{
    /// <summary>
    /// Prefab reference for the health bar UI element
    /// </summary>
    [SerializeField]
    private GameObject healthBarPrefab;

    /// <summary>
    /// 3D offset from the enemy position where the health bar should appear
    /// </summary>
    [SerializeField]
    private Vector3 healthBarOffset = new Vector3(0, 2f, 0);

    /// <summary>
    /// Reference to the main camera for health bar orientation
    /// </summary>
    private Camera mainCamera;

    /// <summary>
    /// Component reference to the HealthBar script on the instantiated health bar
    /// </summary>
    private HealthBar healthBarComponent;

    /// <summary>
    /// GameObject instance of the health bar created from the prefab
    /// </summary>
    private GameObject healthBarInstance;

    /// <summary>
    /// Canvas component of the health bar for world space rendering
    /// </summary>
    private Canvas healthBarCanvas;

    /// <summary>
    /// Flag indicating whether health bars are currently enabled for this enemy
    /// </summary>
    private bool healthBarsEnabled = true;

    /// <summary>
    /// Public property to check if health bars are enabled and visible
    /// </summary>
    /// <value>True if health bars are enabled and instance exists, false otherwise</value>
    public bool AreHealthBarsEnabled => healthBarsEnabled && healthBarInstance != null;

    /// <summary>
    /// initializes camera reference, connects to game settings, and creates health bar.
    /// Called before the first frame update.
    /// </summary>
    void Start()
    {
        // Get main camera reference if not already set
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            Debug.Log(mainCamera);
        }

        // Find and connect to game settings
        Game gameScript = FindAnyObjectByType<Game>();
        if (gameScript != null)
        {
            healthBarsEnabled = gameScript.showEnemyHealthBars;
            gameScript.OnHealthBarsToggled += SetHealthBarEnabled;
        }

        // Create health bar if enabled
        if (healthBarsEnabled)
        {
            CreateHealthBar();
        }
    }

    /// <summary>
    /// Creates a new health bar instance from the prefab and configures it for world space rendering.
    /// Sets up canvas properties and initial scaling.
    /// </summary>
    void CreateHealthBar()
    {
        // Don't create if disabled or already exists
        if (!healthBarsEnabled || healthBarInstance != null)
            return;

        if (healthBarPrefab != null && mainCamera != null)
        {
            // Instantiate the health bar at enemy position with offset
            healthBarInstance = Instantiate(healthBarPrefab, transform.position + healthBarOffset, Quaternion.identity);

            // Get required components
            healthBarComponent = healthBarInstance.GetComponent<HealthBar>();
            healthBarCanvas = healthBarInstance.GetComponent<Canvas>();

            if (healthBarCanvas != null)
            {
                // Configure canvas for world space rendering
                healthBarCanvas.renderMode = RenderMode.WorldSpace;
                healthBarCanvas.worldCamera = mainCamera;

                // Scale down for appropriate world space size
                healthBarInstance.transform.localScale = Vector3.one * 0.0001f;
            }
        }
    }

    /// <summary>
    /// updates health bar position and rotation to follow enemy and face camera.
    /// Called once per frame.
    /// </summary>
    void Update()
    {
        if (healthBarsEnabled && healthBarInstance != null && mainCamera != null)
        {
            // Update position to follow enemy with offset
            healthBarInstance.transform.position = transform.position + healthBarOffset;

            // Rotate health bar to face camera
            Vector3 directionToCamera = mainCamera.transform.position - healthBarInstance.transform.position;
            healthBarInstance.transform.rotation = Quaternion.LookRotation(directionToCamera);
        }
    }

    /// <summary>
    /// Updates the health bar display with current health values.
    /// Converts health to percentage and updates the HealthBar component.
    /// </summary>
    /// <param name="currentHealth">Current health points of the enemy</param>
    /// <param name="maxHealth">Maximum health points of the enemy</param>
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (healthBarComponent != null && healthBarsEnabled)
        {
            // Calculate health percentage and update display
            int healthPercentage = Mathf.RoundToInt((currentHealth / maxHealth) * 100f);
            healthBarComponent.ResetHealth(100); // Set max to 100%
            healthBarComponent.ReduceHealth(100 - healthPercentage); // Reduce to current percentage
        }
    }

    /// <summary>
    /// Shows or hides the health bar based on the show parameter and enabled state.
    /// </summary>
    /// <param name="show">True to show health bar, false to hide</param>
    public void ShowHealthBar(bool show)
    {
        if (healthBarInstance != null)
        {
            bool shouldShow = show && healthBarsEnabled;
            healthBarInstance.SetActive(shouldShow);
        }
    }

    /// <summary>
    /// Called by the SettingsManager when the health bar setting changes.
    /// Creates, shows, or hides health bars based on the new enabled state.
    /// </summary>
    /// <param name="enabled">New enabled state for health bars</param>
    public void SetHealthBarEnabled(bool enabled)
    {
        healthBarsEnabled = enabled;

        if (enabled && healthBarInstance == null)
        {
            // Create health bar if enabling and it doesn't exist
            CreateHealthBar();
        }
        else if (healthBarInstance != null)
        {
            // Show/hide existing health bar based on setting
            healthBarInstance.SetActive(enabled);
        }

        Debug.Log($"[{gameObject.name}] Health bar enabled set to: {enabled}");
    }

    /// <summary>
    /// cleans up event subscriptions and health bar instances.
    /// Called when the GameObject is destroyed.
    /// </summary>
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        Game gameScript = FindAnyObjectByType<Game>();
        if (gameScript != null)
        {
            gameScript.OnHealthBarsToggled -= SetHealthBarEnabled;
        }

        // Clean up health bar instance
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }
    }
}