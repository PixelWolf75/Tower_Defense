using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;


public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    [Header("Settings")]
    public float currentBrightness = 1f;

    // Start is called before the first frame update
    void Awake()
    {
        // Singleton pattern - only one SettingsManager exists
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("Brightness", currentBrightness);
        PlayerPrefs.Save();
        Debug.Log($"Settings saved - Brightness: {currentBrightness}");
    }


    public void LoadSettings()
    {
        currentBrightness = PlayerPrefs.GetFloat("Brightness", 1f);
        Debug.Log($"Settings loaded - Brightness: {currentBrightness}");
    }

    public void SetBrightness(float brightness)
    {
        currentBrightness = brightness;
        SaveSettings();
        ApplyBrightness(); // Will apply immediately to all active volumes
    }

    void ApplyBrightness()
    {
        PostProcessVolume[] volumes = Object.FindObjectsByType<PostProcessVolume>(FindObjectsSortMode.None);

        Debug.Log($"Found {volumes.Length} PostProcessVolumes in scene");

        foreach (PostProcessVolume volume in volumes)
        {
            if (volume.profile != null)
            {
                // Try Color Grading first
                if (volume.profile.TryGetSettings(out ColorGrading colorGrading))
                {
                    colorGrading.postExposure.value = currentBrightness;
                    Debug.Log($"Applied brightness {currentBrightness} (Post Exposure) to {volume.name}");
                }
                // Fallback to Auto Exposure if no Color Grading
                else if (volume.profile.TryGetSettings(out AutoExposure exposure))
                {
                    exposure.minLuminance.value = exposure.maxLuminance.value = currentBrightness;
                    Debug.Log($"Applied brightness {currentBrightness} (Auto Exposure) to {volume.name}");
                }
                else
                {
                    Debug.LogWarning($"No Color Grading or Auto Exposure found in {volume.name}");
                }
            }
            else
            {
                Debug.LogWarning($"No profile assigned to {volume.name}");
            }
        }
    }

    // Public method to manually trigger brightness application (for testing)
    [ContextMenu("Apply Brightness")]
    public void ManualApplyBrightness()
    {
        ApplyBrightness();
    }

    void OnEnable()
    {
        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(WatchForNewVolumes());
    }

    void OnDisable()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");
        StartCoroutine(ApplySettingsAfterSceneLoad());
    }

    private IEnumerator ApplySettingsAfterSceneLoad()
    {
        yield return null; // Wait for scene objects to be ready
        ApplyBrightness();
    }

    private IEnumerator WatchForNewVolumes()
    {
        var knownVolumes = new HashSet<PostProcessVolume>();

        while (true)
        {
            var volumes = Object.FindObjectsByType<PostProcessVolume>(FindObjectsSortMode.None);
            foreach (var v in volumes)
            {
                if (!knownVolumes.Contains(v))
                {
                    knownVolumes.Add(v);
                    if (v.profile != null && v.profile.TryGetSettings(out AutoExposure exposure))
                    {
                        exposure.keyValue.value = currentBrightness;
                        Debug.Log($"Auto-applied brightness to NEW volume: {v.name}");
                    }
                }
            }
            yield return new WaitForSeconds(0.5f); // Check twice a second
        }
    }
}
