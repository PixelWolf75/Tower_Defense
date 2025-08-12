using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;


public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    [SerializeField]
    public float currentBrightness = 1f;

    // Start is called before the first frame update
    void Start()
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

        // Apply settings immediately
        ApplyBrightness();
    }

    public void SetBrightness(float brightness)
    {
        currentBrightness = brightness;
        ApplyBrightness();
        SaveSettings(); // Auto-save when changed
    }

    void ApplyBrightness()
    {
        // Find and apply brightness to current scene's post process
        PostProcessVolume[] volumes = Object.FindObjectsByType<PostProcessVolume>(FindObjectsSortMode.None);

        foreach (PostProcessVolume volume in volumes)
        {
            if (volume.profile != null && volume.profile.TryGetSettings(out AutoExposure exposure))
            {
                exposure.keyValue.value = currentBrightness;
                Debug.Log($"Applied brightness {currentBrightness} to {volume.name}");
            }
        }
    }

    void OnEnable()
    {
        // Subscribe to scene loaded event (modern way)
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // Unsubscribe from scene loaded event
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        StartCoroutine(ApplySettingsAfterSceneLoad());
    }

    IEnumerator ApplySettingsAfterSceneLoad()
    {
        // Wait a frame for scene to fully load
        yield return null;
        ApplyBrightness();
    }
}
