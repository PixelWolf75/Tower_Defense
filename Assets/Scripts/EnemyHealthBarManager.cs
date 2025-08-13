using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthBarManager : MonoBehaviour
{
    [SerializeField]
    private GameObject healthBarPrefab;

    [SerializeField]
    private Vector3 healthBarOffset = new Vector3(0, 2f, 0); // Offset above enemy

    private Camera mainCamera; // Reference to main camera

    private HealthBar healthBarComponent;
    private GameObject healthBarInstance;
    private Canvas healthBarCanvas;

    private bool healthBarsEnabled = true;

    // Public getter for other scripts to check if health bars are enabled for this enemy
    public bool AreHealthBarsEnabled => healthBarsEnabled && healthBarInstance != null;

    // Start is called before the first frame update
    void Start()
    {
        if (mainCamera == null)
        {
            //Debug.Log("Got main camera");
            mainCamera = Camera.main;
            Debug.Log(mainCamera);
        }

        // Find the Game script in the scene to get health bar setting
        Game gameScript = FindAnyObjectByType<Game>();
        if (gameScript != null)
        {
            healthBarsEnabled = gameScript.showEnemyHealthBars;
            gameScript.OnHealthBarsToggled += SetHealthBarEnabled;
        }

        if (healthBarsEnabled)
        {
            CreateHealthBar();
        }
    }

    void CreateHealthBar()
    {
        // Don't create if health bars are disabled or already exists
        if (!healthBarsEnabled || healthBarInstance != null)
            return;

        if (healthBarPrefab != null && mainCamera != null)
        {
            // Instantiate the health bar prefab
            healthBarInstance = Instantiate(healthBarPrefab, transform.position + healthBarOffset, Quaternion.identity);

            // Get the health bar component
            healthBarComponent = healthBarInstance.GetComponent<HealthBar>();

            // Get the canvas component
            healthBarCanvas = healthBarInstance.GetComponent<Canvas>();

            if (healthBarCanvas != null)
            {
                // Set canvas to world space
                healthBarCanvas.renderMode = RenderMode.WorldSpace;
                healthBarCanvas.worldCamera = mainCamera;

                // Scale down the canvas for world space
                healthBarInstance.transform.localScale = Vector3.one * 0.0001f; // Adjust scale as needed
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (healthBarsEnabled && healthBarInstance != null && mainCamera != null)
        {
            // Update health bar position to follow enemy
            healthBarInstance.transform.position = transform.position + healthBarOffset;

            // Make health bar face the camera
            Vector3 directionToCamera = mainCamera.transform.position - healthBarInstance.transform.position;
            healthBarInstance.transform.rotation = Quaternion.LookRotation(directionToCamera);
        }
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (healthBarComponent != null && healthBarsEnabled)
        {
            // Convert to percentage and update
            int healthPercentage = Mathf.RoundToInt((currentHealth / maxHealth) * 100f);
            healthBarComponent.ResetHealth(100); // Set max to 100
            healthBarComponent.ReduceHealth(100 - healthPercentage); // Reduce to current percentage
        }
    }

    public void ShowHealthBar(bool show)
    {
        if (healthBarInstance != null)
        {
            bool shouldShow = show && healthBarsEnabled;
            healthBarInstance.SetActive(shouldShow);
        }
    }

    // Called by SettingsManager when health bar setting changes
    public void SetHealthBarEnabled(bool enabled)
    {
        healthBarsEnabled = enabled;

        if (enabled && healthBarInstance == null)
        {
            // Create health bar if it doesn't exist and we're enabling
            CreateHealthBar();
        }
        else if (healthBarInstance != null)
        {
            // Show/hide existing health bar based on setting
            healthBarInstance.SetActive(enabled);
        }

        Debug.Log($"[{gameObject.name}] Health bar enabled set to: {enabled}");
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        Game gameScript = FindAnyObjectByType< Game>();
        if (gameScript != null)
        {
            gameScript.OnHealthBarsToggled -= SetHealthBarEnabled;
        }

        // Clean up health bar when enemy is destroyed
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }
    }
}
