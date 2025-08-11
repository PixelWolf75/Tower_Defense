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

    // Start is called before the first frame update
    void Start()
    {
        if (mainCamera == null)
        {
            //Debug.Log("Got main camera");
            mainCamera = Camera.main;
            Debug.Log(mainCamera);
        }

        CreateHealthBar();
    }

    void CreateHealthBar()
    {

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
        if (healthBarInstance != null && mainCamera != null)
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
        if (healthBarComponent != null)
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
            healthBarInstance.SetActive(show);
            Debug.Log($"[{gameObject.name}] Health bar visibility set to: {show}");
        }
    }

    void OnDestroy()
    {
        // Clean up health bar when enemy is destroyed
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }
    }
}
