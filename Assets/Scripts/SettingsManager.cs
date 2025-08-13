using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class SettingsManager : MonoBehaviour
{
    [Header("Boolean Settings - Scene Independent")]
    public bool audioEnabled = true;
    public bool showArrows = true;
    public bool showGrid = true;
    public bool showEnemyHealthBars = true;

    [Header("UI Toggle References")]
    public Toggle audioToggle;
    public Toggle arrowsToggle;
    public Toggle gridToggle;
    public Toggle healthBarToggle;

    [Header("Audio Source")]
    public AudioSource gameAudioSource;

    // Events for other scripts to subscribe to
    public System.Action<bool> OnArrowsToggled;
    public System.Action<bool> OnGridToggled;
    public System.Action<bool> OnHealthBarsToggled;
    public System.Action<bool> OnAudioToggled;

    void Start()
    {
        // Apply settings and set up toggles
        SetupToggleListeners();
        ApplyAllSettings();
    }

    void SetupToggleListeners()
    {
        // Set up toggle listeners if they exist
        if (audioToggle != null)
        {
            audioToggle.onValueChanged.RemoveAllListeners(); // Clear existing listeners
            audioToggle.onValueChanged.AddListener(SetAudioEnabled);
            audioToggle.isOn = audioEnabled;
            Debug.Log($"Audio toggle setup complete - isOn: {audioToggle.isOn}");
        }

        if (arrowsToggle != null)
        {
            arrowsToggle.onValueChanged.RemoveAllListeners();
            arrowsToggle.onValueChanged.AddListener(SetArrowsEnabled);
            arrowsToggle.isOn = showArrows;
            Debug.Log($"Arrows toggle setup complete - isOn: {arrowsToggle.isOn}");
        }

        if (gridToggle != null)
        {
            gridToggle.onValueChanged.RemoveAllListeners();
            gridToggle.onValueChanged.AddListener(SetGridEnabled);
            gridToggle.isOn = showGrid;
            Debug.Log($"Grid toggle setup complete - isOn: {gridToggle.isOn}");
        }

        if (healthBarToggle != null)
        {
            healthBarToggle.onValueChanged.RemoveAllListeners();
            healthBarToggle.onValueChanged.AddListener(SetHealthBarsEnabled);
            healthBarToggle.isOn = showEnemyHealthBars;
            Debug.Log($"Health bar toggle setup complete - isOn: {healthBarToggle.isOn}");
        }
    }

    // Audio setting
    public void SetAudioEnabled(bool enabled)
    {
        Debug.Log($"SetAudioEnabled called with: {enabled}");
        audioEnabled = enabled;
        ApplyAudioSetting();
    }

    // Arrows setting
    public void SetArrowsEnabled(bool enabled)
    {
        Debug.Log($"SetArrowsEnabled called with: {enabled}");
        showArrows = enabled;
        ApplyArrowsSetting();
    }

    // Grid setting
    public void SetGridEnabled(bool enabled)
    {
        Debug.Log($"SetGridEnabled called with: {enabled}");
        showGrid = enabled;
        ApplyGridSetting();
    }

    // Health bars setting
    public void SetHealthBarsEnabled(bool enabled)
    {
        Debug.Log($"SetHealthBarsEnabled called with: {enabled}");
        showEnemyHealthBars = enabled;
        ApplyHealthBarsSetting();
    }

    // Public methods to toggle settings and sync UI (called by keyboard shortcuts)
    public void ToggleArrows()
    {
        bool newValue = !showArrows;
        SetArrowsEnabled(newValue);
        if (arrowsToggle != null)
        {
            arrowsToggle.isOn = newValue;
        }
    }

    public void ToggleGrid()
    {
        bool newValue = !showGrid;
        SetGridEnabled(newValue);
        if (gridToggle != null)
        {
            gridToggle.isOn = newValue;
        }
    }

    public void ToggleHealthBars()
    {
        bool newValue = !showEnemyHealthBars;
        SetHealthBarsEnabled(newValue);
        if (healthBarToggle != null)
        {
            healthBarToggle.isOn = newValue;
        }
    }

    public void ToggleAudio()
    {
        bool newValue = !audioEnabled;
        SetAudioEnabled(newValue);
        if (audioToggle != null)
        {
            audioToggle.isOn = newValue;
        }
    }

    void ApplyAllSettings()
    {
        ApplyAudioSetting();
        ApplyArrowsSetting();
        ApplyGridSetting();
        ApplyHealthBarsSetting();
    }

    void ApplyAudioSetting()
    {
        if (gameAudioSource != null)
        {
            gameAudioSource.enabled = audioEnabled;
            Debug.Log($"Audio source enabled: {audioEnabled}");
        }
        else
        {
            // Try to find audio source if not assigned
            gameAudioSource = Object.FindAnyObjectByType<AudioSource>();
            if (gameAudioSource != null)
            {
                gameAudioSource.enabled = audioEnabled;
                Debug.Log($"Found and set audio source enabled: {audioEnabled}");
            }
        }

        // Notify other scripts about audio setting change
        OnAudioToggled?.Invoke(audioEnabled);
    }

    void ApplyArrowsSetting()
    {
        // Notify other scripts about arrows setting change
        OnArrowsToggled?.Invoke(showArrows);
        Debug.Log($"Arrows visibility set to: {showArrows}");
    }

    void ApplyGridSetting()
    {
        // Notify other scripts about grid setting change
        OnGridToggled?.Invoke(showGrid);
        Debug.Log($"Grid visibility set to: {showGrid}");
    }

    void ApplyHealthBarsSetting()
    {
        // Notify all health bar managers about the setting
        OnHealthBarsToggled?.Invoke(showEnemyHealthBars);

        // Apply to all existing health bar managers in current scene
        EnemyHealthBarManager[] healthBarManagers = Object.FindObjectsByType<EnemyHealthBarManager>(FindObjectsSortMode.None);
        foreach (var manager in healthBarManagers)
        {
            manager.SetHealthBarEnabled(showEnemyHealthBars);
        }

        Debug.Log($"Enemy health bars enabled: {showEnemyHealthBars}");
    }

    [ContextMenu("Apply All Settings")]
    public void ManualApplyAllSettings()
    {
        ApplyAllSettings();
    }
}