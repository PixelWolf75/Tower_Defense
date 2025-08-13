using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


/// <summary>
/// Centralized manager for game settings and visual preferences.
/// Handles UI toggle synchronization, setting persistence, and event broadcasting to other systems.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    [Header("Boolean Settings - Scene Independent")]
    /// <summary>
    /// Master audio enable/disable setting
    /// </summary>
    public bool audioEnabled = true;

    /// <summary>
    /// Whether to show directional arrows on the game board
    /// </summary>
    public bool showArrows = true;

    /// <summary>
    /// Whether to display the grid overlay on the game board
    /// </summary>
    public bool showGrid = true;

    /// <summary>
    /// Whether to show health bars above enemies
    /// </summary>
    public bool showEnemyHealthBars = true;

    [Header("UI Toggle References")]
    /// <summary>
    /// UI Toggle component for audio setting
    /// </summary>
    public Toggle audioToggle;

    /// <summary>
    /// UI Toggle component for arrows visibility setting
    /// </summary>
    public Toggle arrowsToggle;

    /// <summary>
    /// UI Toggle component for grid visibility setting
    /// </summary>
    public Toggle gridToggle;

    /// <summary>
    /// UI Toggle component for health bar visibility setting
    /// </summary>
    public Toggle healthBarToggle;

    [Header("Audio Source")]
    /// <summary>
    /// AudioSource component to control for audio settings
    /// </summary>
    public AudioSource gameAudioSource;

    // Events for other scripts to subscribe to setting changes
    /// <summary>
    /// Event fired when arrows visibility setting changes
    /// </summary>
    public System.Action<bool> OnArrowsToggled;

    /// <summary>
    /// Event fired when grid visibility setting changes
    /// </summary>
    public System.Action<bool> OnGridToggled;

    /// <summary>
    /// Event fired when health bars visibility setting changes
    /// </summary>
    public System.Action<bool> OnHealthBarsToggled;

    /// <summary>
    /// Event fired when audio enabled setting changes
    /// </summary>
    public System.Action<bool> OnAudioToggled;

    /// <summary>
    /// Unity Start method - initializes toggle listeners and applies all current settings.
    /// Called before the first frame update.
    /// </summary>
    void Start()
    {
        SetupToggleListeners();
        ApplyAllSettings();
    }

    /// <summary>
    /// Configures UI toggle components with event listeners and initial values.
    /// Clears existing listeners to prevent duplicate subscriptions.
    /// </summary>
    void SetupToggleListeners()
    {
        // Setup audio toggle
        if (audioToggle != null)
        {
            audioToggle.onValueChanged.RemoveAllListeners();
            audioToggle.onValueChanged.AddListener(SetAudioEnabled);
            audioToggle.isOn = audioEnabled;
            Debug.Log($"Audio toggle setup complete - isOn: {audioToggle.isOn}");
        }

        // Setup arrows toggle
        if (arrowsToggle != null)
        {
            arrowsToggle.onValueChanged.RemoveAllListeners();
            arrowsToggle.onValueChanged.AddListener(SetArrowsEnabled);
            arrowsToggle.isOn = showArrows;
            Debug.Log($"Arrows toggle setup complete - isOn: {arrowsToggle.isOn}");
        }

        // Setup grid toggle
        if (gridToggle != null)
        {
            gridToggle.onValueChanged.RemoveAllListeners();
            gridToggle.onValueChanged.AddListener(SetGridEnabled);
            gridToggle.isOn = showGrid;
            Debug.Log($"Grid toggle setup complete - isOn: {gridToggle.isOn}");
        }

        // Setup health bar toggle
        if (healthBarToggle != null)
        {
            healthBarToggle.onValueChanged.RemoveAllListeners();
            healthBarToggle.onValueChanged.AddListener(SetHealthBarsEnabled);
            healthBarToggle.isOn = showEnemyHealthBars;
            Debug.Log($"Health bar toggle setup complete - isOn: {healthBarToggle.isOn}");
        }
    }

    /// <summary>
    /// Sets the audio enabled state and applies the setting immediately.
    /// </summary>
    /// <param name="enabled">True to enable audio, false to disable</param>
    public void SetAudioEnabled(bool enabled)
    {
        Debug.Log($"SetAudioEnabled called with: {enabled}");
        audioEnabled = enabled;
        ApplyAudioSetting();
    }

    /// <summary>
    /// Sets the arrows visibility state and applies the setting immediately.
    /// </summary>
    /// <param name="enabled">True to show arrows, false to hide</param>
    public void SetArrowsEnabled(bool enabled)
    {
        Debug.Log($"SetArrowsEnabled called with: {enabled}");
        showArrows = enabled;
        ApplyArrowsSetting();
    }

    /// <summary>
    /// Sets the grid visibility state and applies the setting immediately.
    /// </summary>
    /// <param name="enabled">True to show grid, false to hide</param>
    public void SetGridEnabled(bool enabled)
    {
        Debug.Log($"SetGridEnabled called with: {enabled}");
        showGrid = enabled;
        ApplyGridSetting();
    }

    /// <summary>
    /// Sets the health bars visibility state and applies the setting immediately.
    /// </summary>
    /// <param name="enabled">True to show health bars, false to hide</param>
    public void SetHealthBarsEnabled(bool enabled)
    {
        Debug.Log($"SetHealthBarsEnabled called with: {enabled}");
        showEnemyHealthBars = enabled;
        ApplyHealthBarsSetting();
    }

    /// <summary>
    /// Toggles the arrows visibility setting and synchronizes the UI toggle.
    /// Called by keyboard shortcuts or other input methods.
    /// </summary>
    public void ToggleArrows()
    {
        bool newValue = !showArrows;
        SetArrowsEnabled(newValue);
        if (arrowsToggle != null)
        {
            arrowsToggle.isOn = newValue;
        }
    }

    /// <summary>
    /// Toggles the grid visibility setting and synchronizes the UI toggle.
    /// Called by keyboard shortcuts or other input methods.
    /// </summary>
    public void ToggleGrid()
    {
        bool newValue = !showGrid;
        SetGridEnabled(newValue);
        if (gridToggle != null)
        {
            gridToggle.isOn = newValue;
        }
    }

    /// <summary>
    /// Toggles the health bars visibility setting and synchronizes the UI toggle.
    /// Called by keyboard shortcuts or other input methods.
    /// </summary>
    public void ToggleHealthBars()
    {
        bool newValue = !showEnemyHealthBars;
        SetHealthBarsEnabled(newValue);
        if (healthBarToggle != null)
        {
            healthBarToggle.isOn = newValue;
        }
    }

    /// <summary>
    /// Toggles the audio enabled setting and synchronizes the UI toggle.
    /// Called by keyboard shortcuts or other input methods.
    /// </summary>
    public void ToggleAudio()
    {
        bool newValue = !audioEnabled;
        SetAudioEnabled(newValue);
        if (audioToggle != null)
        {
            audioToggle.isOn = newValue;
        }
    }

    /// <summary>
    /// Applies all current settings to their respective systems.
    /// Called during initialization to ensure consistency.
    /// </summary>
    void ApplyAllSettings()
    {
        ApplyAudioSetting();
        ApplyArrowsSetting();
        ApplyGridSetting();
        ApplyHealthBarsSetting();
    }

    /// <summary>
    /// Applies the audio setting by enabling/disabling the game's audio source.
    /// Attempts to find an audio source if none is assigned.
    /// </summary>
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

    /// <summary>
    /// Applies the arrows visibility setting by broadcasting to subscribed systems.
    /// </summary>
    void ApplyArrowsSetting()
    {
        OnArrowsToggled?.Invoke(showArrows);
        Debug.Log($"Arrows visibility set to: {showArrows}");
    }

    /// <summary>
    /// Applies the grid visibility setting by broadcasting to subscribed systems.
    /// </summary>
    void ApplyGridSetting()
    {
        OnGridToggled?.Invoke(showGrid);
        Debug.Log($"Grid visibility set to: {showGrid}");
    }

    /// <summary>
    /// Applies the health bars visibility setting by broadcasting to all health bar managers.
    /// Updates both event subscribers and existing health bar managers in the current scene.
    /// </summary>
    void ApplyHealthBarsSetting()
    {
        // Notify event subscribers
        OnHealthBarsToggled?.Invoke(showEnemyHealthBars);

        // Apply to all existing health bar managers in current scene
        EnemyHealthBarManager[] healthBarManagers = Object.FindObjectsByType<EnemyHealthBarManager>(FindObjectsSortMode.None);
        foreach (var manager in healthBarManagers)
        {
            manager.SetHealthBarEnabled(showEnemyHealthBars);
        }

        Debug.Log($"Enemy health bars enabled: {showEnemyHealthBars}");
    }

    /// <summary>
    /// Context menu method for manually applying all settings during development.
    /// Visible in the Inspector's right-click context menu.
    /// </summary>
    [ContextMenu("Apply All Settings")]
    public void ManualApplyAllSettings()
    {
        ApplyAllSettings();
    }
}