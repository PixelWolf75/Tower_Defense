using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Main game controller that manages the core tower defense gameplay loop.
/// Handles initialization, UI updates, input processing, enemy spawning, health management,
/// and game state transitions (game over/victory). Acts as the central coordinator between
/// all game systems including the board, enemies, scenarios, and settings.
/// </summary>
public class Game : MonoBehaviour
{
    /// <summary>
    /// Singleton instance reference for static access from other game systems
    /// </summary>
    static Game instance;

    /// <summary>
    /// Size of the game board in tiles (width x height)
    /// Default is 11x11 tiles, minimum enforced as 3x3 in OnValidate
    /// </summary>
    [SerializeField]
    Vector2Int boardSize = new Vector2Int(11, 11); //Set as default as 11x11

    /// <summary>
    /// Number of destination tiles to randomly place on the board during initialization
    /// Minimum value of 1 enforced by Unity Inspector
    /// </summary>
    [SerializeField, Min(1)]
    int numDestinations = 1;

    /// <summary>
    /// Number of spawn point tiles to randomly place on the board during initialization
    /// Minimum value of 1 enforced by Unity Inspector
    /// </summary>
    [SerializeField, Min(1)]
    int numSpawnPoints = 1;

    /// <summary>
    /// Factory responsible for creating and managing different types of tile content
    /// (walls, towers, spawn points, destinations, empty tiles)
    /// </summary>
    [SerializeField]
    GameTileContentFactory tileContentFactory = default;

    /// <summary>
    /// UI GameObject that displays the "Game Over" text when the player loses
    /// Activated when player health reaches zero
    /// </summary>
    [SerializeField]
    GameObject gameOverText = default;

    /// <summary>
    /// UI GameObject that displays the "Victory" text when the player wins
    /// Activated when all waves are completed and no enemies remain
    /// </summary>
    [SerializeField]
    GameObject gameClearText = default;

    /// <summary>
    /// UI text component displaying the current number of towers available for placement
    /// Updated whenever towers are placed or enemies are killed
    /// </summary>
    [SerializeField]
    TextMeshProUGUI numTowersText = default;

    /// <summary>
    /// UI text component displaying the current number of walls available for placement
    /// Updated whenever walls are placed or enemies are killed
    /// </summary>
    [SerializeField]
    TextMeshProUGUI numWallsText = default;

    /// <summary>
    /// UI text component displaying the current player score
    /// Score increases when enemies are killed
    /// </summary>
    [SerializeField]
    TextMeshProUGUI scoreText = default;

    /// <summary>
    /// UI panel that appears during game over or victory states
    /// Allows the player to start a new game or adjust settings
    /// </summary>
    [SerializeField]
    GameObject NewGamePanel = default;

    /// <summary>
    /// Reference to the main game board that manages tiles, pathfinding, and tile content
    /// Handles all spatial game logic and tile-based interactions
    /// </summary>
    [SerializeField]
    GameBoard board = default;


    /// <summary>
    /// Starting health value for the player when a new game begins
    /// Range: 0-100, determines how many enemies can reach the destination before game over
    /// </summary>
    [SerializeField, Range(0, 100)]
    int startingPlayerHealth = 10;

    /// <summary>
    /// ScriptableObject that defines enemy waves, spawn timing, and victory conditions
    /// Controls the progression and difficulty of the tower defense scenario
    /// </summary>
    [SerializeField]
    GameScenario scenario = default;

    /// <summary>
    /// Game speed multiplier for non-paused gameplay
    /// Range: 1.0-10.0, allows players to speed up or slow down the action
    /// </summary>
    [SerializeField, Range(1f, 10f)]
    float playSpeed = 1f;

    /// <summary>
    /// Current state of the active scenario, tracks wave progression and completion
    /// Updated each frame to spawn enemies and check victory conditions
    /// </summary>
    GameScenario.State activeScenario;

    /// <summary>
    /// UI component that visually represents the player's remaining health
    /// Updated when enemies reach destinations or when resetting the game
    /// </summary>
    [SerializeField]
    HealthBar healthBar = default;

    /// <summary>
    /// Reference to the global settings manager for handling user preferences
    /// Manages visual settings like grid display, health bars, and audio
    /// </summary>
    private SettingsManager settingsManager;

    /// <summary>
    /// Event that other systems can subscribe to for health bar visibility changes
    /// Invoked when enemy health bar display setting is toggled
    /// </summary>
    public System.Action<bool> OnHealthBarsToggled;

    /// <summary>
    /// Scene-specific visual settings header for Inspector organization
    /// </summary>
    [Header("Scene Settings")]

    /// <summary>
    /// Controls whether pathfinding arrows are visible on tiles
    /// Can be toggled via settings manager or direct input (V key)
    /// </summary>
    public bool showArrows = true;

    /// <summary>
    /// Controls whether the grid texture is visible on the game board
    /// Can be toggled via settings manager or direct input (G key)
    /// </summary>
    public bool showGrid = true;

    /// <summary>
    /// Controls whether enemy health bars are visible above enemies
    /// Can be toggled via settings manager or direct input (H key)
    /// </summary>
    public bool showEnemyHealthBars = true;

    /// <summary>
    /// Current player health points
    /// Game ends when this reaches zero if starting health > 0
    /// </summary>
    int playerHealth;

    /// <summary>
    /// Current currency available for purchasing upgrades or abilities
    /// Currently increases when enemies are killed but not used for purchases
    /// </summary>
    int currency;

    /// <summary>
    /// Current player score, increases when enemies are eliminated
    /// Displayed in the UI as a measure of player performance
    /// </summary>
    int score;

    /// <summary>
    /// Number of towers remaining that the player can place
    /// Decreases when towers are placed, increases when enemies are killed
    /// </summary>
    int numTowers;

    /// <summary>
    /// Number of walls remaining that the player can place
    /// Decreases when walls are placed, increases when enemies are killed
    /// </summary>
    int numWalls;

    /// <summary>
    /// Flag indicating whether the game has ended (victory or defeat)
    /// Prevents further game updates and input processing when true
    /// </summary>
    bool isGameOverOrCleared;

    /// <summary>
    /// Flag tracking whether the game has been properly initialized
    /// Prevents updates from running before all systems are ready
    /// </summary>
    bool isInitialized = false; // Track if game is properly initialized

    /// <summary>
    /// Time scale value used when the game is paused (always 0)
    /// Used in comparison with current Time.timeScale to toggle pause state
    /// </summary>
    const float pausedTimeScale = 0f;

    /// <summary>
    /// Collection that manages all active enemy instances in the game
    /// Handles enemy updates, cleanup, and provides enemy count information
    /// </summary>
    GameBehaviourCollection enemies = new GameBehaviourCollection();

    /// <summary>
    /// Property that creates a ray from the main camera through the current mouse position
    /// Used for tile selection and placement of towers/walls via mouse input
    /// </summary>
    Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);

    /// <summary>
    /// Unity lifecycle method called when the GameObject is first created.
    /// Sets up the singleton instance and initializes the game state.
    /// </summary>
    void Awake()
    {
        instance = this;
        Debug.Log("Game Awake - Initializing scene");
        InitializeGame();
    }

    /// <summary>
    /// Called before the first frame update.
    /// Ensures initialization is complete and applies initial settings from the settings manager.
    /// </summary>
    void Start()
    {
        // Additional initialization in Start to ensure everything is ready
        if (!isInitialized)
        {
            Debug.LogWarning("Game not initialized in Awake, initializing in Start");
            InitializeGame();
        }

        ConnectToSceneSettings();

        // Apply initial settings from SettingsManager
        ApplyInitialSettings();

        // Force scenario to start fresh
        ResetScenario();
    }

    /// <summary>
    /// Connects this game instance to the global settings manager and subscribes to setting change events.
    /// Synchronizes local settings with the global settings manager state.
    /// </summary>
    void ConnectToSceneSettings()
    {
        settingsManager = FindAnyObjectByType<SettingsManager>();
        if (settingsManager != null)
        {
            // Subscribe to settings events
            settingsManager.OnArrowsToggled += SetShowPaths;
            settingsManager.OnGridToggled += SetShowGrid;
            settingsManager.OnHealthBarsToggled += SetShowEnemyHealthBars;

            // Get current settings from the manager
            showArrows = settingsManager.showArrows;
            showGrid = settingsManager.showGrid;
            showEnemyHealthBars = settingsManager.showEnemyHealthBars;

            Debug.Log("Game connected to settingsManager");
        }
        else
        {
            Debug.LogWarning("No settingsManager found in scene");
        }
    }

    /// <summary>
    /// Resets and restarts the current game scenario from the beginning.
    /// Used when starting a new game or when the scenario needs to be refreshed.
    /// </summary>
    void ResetScenario()
    {
        if (scenario != null)
        {
            activeScenario = scenario.Begin();
            Debug.Log($"Scenario reset and started: {scenario.name}");
        }
        else
        {
            Debug.LogError("Cannot reset scenario - no scenario assigned!");
        }
    }

    /// <summary>
    /// Applies the initial visual settings to the game board and UI elements.
    /// Called during startup to ensure settings are properly applied from the start.
    /// </summary>
    void ApplyInitialSettings()
    {

        SetShowPaths(showArrows);
        SetShowGrid(showGrid);
        SetShowEnemyHealthBars(showEnemyHealthBars);
    }

    /// <summary>
    /// Called once per frame.
    /// Handles all real-time game logic including input processing, scenario progression,
    /// game state checks, and system updates.
    /// </summary>
    void Update()
    {

        if (!isInitialized) return; // Don't update if not properly initialized


        if (Input.GetMouseButtonDown(0))
        {		
            HandleTouchWalls();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            HandleTouchTowers();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            // Toggle arrows directly
            if (settingsManager != null)
            {
                settingsManager.ToggleArrows();
            }
            else
            {
                SetShowPaths(!showArrows);
            }
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            // Toggle grid directly
            if (settingsManager != null)
            {
                settingsManager.ToggleGrid();
            }
            else
            {
                SetShowGrid(!showGrid);
            }
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            // Toggle health bars directly
            if (settingsManager != null)
            {
                settingsManager.ToggleHealthBars();
            }
            else
            {
                SetShowEnemyHealthBars(!showEnemyHealthBars);
            }
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            // Toggle audio/music and sync UI
            if (settingsManager != null)
            {
                settingsManager.ToggleAudio();
            }
        }

        if (!isGameOverOrCleared)
        {
            if (playerHealth <= 0 && startingPlayerHealth > 0)
            {
                Debug.Log("Defeat!");
                GameOver();
                //BeginNewGame();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Time.timeScale =
                    Time.timeScale > pausedTimeScale ? pausedTimeScale : 1f;
            }
            else if (Time.timeScale > pausedTimeScale)
            {
                Time.timeScale = playSpeed;
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                BeginNewGame();
            }

            // Only progress scenario if spawn points exist
            if (board.SpawnPointCount > 0)
            {
                if (!activeScenario.Progress() && enemies.IsEmpty)
                {
                    Debug.Log("Victory!");
                    GameClear();
                    activeScenario.Progress();
                }
            }

            enemies.GameUpdate();
            Physics.SyncTransforms();
            board.GameUpdate();
        }
    }

    /// <summary>
    /// Sets the visibility of pathfinding arrows on the game board.
    /// Updates both the local setting and applies the change to the board immediately.
    /// </summary>
    /// <param name="show">True to show pathfinding arrows, false to hide them</param>
    public void SetShowPaths(bool show)
    {
        showArrows = show;
        if (board != null)
        {
            board.ShowPaths = show;
            Debug.Log($"Paths visibility set to: {show}");
        }
    }

    /// <summary>
    /// Sets the visibility of the grid texture on the game board.
    /// Updates both the local setting and applies the change to the board immediately.
    /// </summary>
    /// <param name="show">True to show the grid texture, false to hide it</param>
    public void SetShowGrid(bool show)
    {
        showGrid = show;
        if (board != null)
        {
            board.ShowGrid = show;
            Debug.Log($"Grid visibility set to: {show}");
        }
    }

    /// <summary>
    /// Sets the visibility of enemy health bars throughout the scene.
    /// Updates all existing health bar managers and notifies subscribers of the change.
    /// </summary>
    /// <param name="show">True to show enemy health bars, false to hide them</param>
    public void SetShowEnemyHealthBars(bool show)
    {
        showEnemyHealthBars = show;

        // Notify all health bar managers
        OnHealthBarsToggled?.Invoke(show);

        // Apply to all existing health bar managers in current scene
        EnemyHealthBarManager[] healthBarManagers = Object.FindObjectsByType<EnemyHealthBarManager>(FindObjectsSortMode.None);
        foreach (var manager in healthBarManagers)
        {
            manager.SetHealthBarEnabled(show);
        }

        Debug.Log($"Enemy health bars visibility set to: {show}");
    }

    /// <summary>
    /// Initializes all game systems and resets values to their starting state.
    /// Called during Awake and whenever a new game is started.
    /// Sets up the board, resets counters, and ensures proper initialization order.
    /// </summary>
    void InitializeGame()
    {
        // Reset time scale to ensure normal game speed
        Time.timeScale = 1f;

        playerHealth = startingPlayerHealth;
        currency = 0;
        score = 0;
        numTowers = 15;
        numWalls = 15;
        isGameOverOrCleared = false;

        UpdateUI();

        // Debug asset setup
        if (scenario == null)
        {
            Debug.LogError("No GameScenario assigned!");
            return;
        }

        if (board == null)
        {
            Debug.LogError("No GameBoard assigned!");
            return;
        }

        board.Initialize(boardSize, tileContentFactory, numSpawnPoints, numDestinations);
        board.ShowGrid = true;

        // Clear any existing enemies
        enemies.Clear();

        isInitialized = true;
        Debug.Log("Game initialization complete");
    }

    /// <summary>
    /// Static method for spawning enemies at random spawn points on the game board.
    /// Called by the scenario system to create new enemy instances during gameplay.
    /// </summary>
    /// <param name="factory">EnemyFactory to create the enemy instance from</param>
    /// <param name="type">Type of enemy to spawn (Small, Medium, or Large)</param>
    public static void SpawnEnemy(EnemyFactory factory, EnemyType type)
    {
        if (instance == null)
        {
            Debug.LogError("Game instance is null! Cannot spawn enemy.");
            return;
        }

        //Debug.Log($"SpawnEnemy called: factory={factory.name}, type={type}");
        if (instance.board.SpawnPointCount == 0)
        {
            return; // Don't spawn if no spawn points exist
        }

        GameTile spawnPoint =
            instance.board.GetSpawnPoint(Random.Range(0, instance.board.SpawnPointCount));
        Enemy enemy = factory.Get(type);
        //Debug.Log($"Spawning at tile: {spawnPoint.transform.position}");
        enemy.SpawnOn(spawnPoint);

        instance.enemies.Add(enemy);
    }

    /// <summary>
    /// Static callback method invoked when an enemy reaches a destination tile.
    /// Reduces player health by 1 and updates the health bar UI.
    /// Automatically called by enemy instances upon reaching their destination.
    /// </summary>
    public static void EnemyReachedDestination()
    {
        if (instance == null) return;

        instance.playerHealth -= 1;
        instance.healthBar.ReduceHealth(1); // Update the UI
    }

    /// <summary>
    /// Static callback method invoked when an enemy is destroyed by towers or other means.
    /// Increases currency, score, and available building resources, then updates the UI.
    /// Automatically called by enemy instances when they are eliminated.
    /// </summary>
    public static void EnemyHasBeenKilled()
    {
        if (instance == null) return;

        instance.currency += 1;
        instance.score += 1;
        instance.numWalls += 1;
        instance.numTowers += 1;

        instance.UpdateUI();
    }

    /// <summary>
    /// Handles mouse/touch input for placing or removing walls and towers.
    /// Left Shift + click places towers, regular click places walls.
    /// </summary>
    void HandleTouch()
    {
        GameTile tile = board.GetTile(TouchRay);
        if (tile != null)
        {
            //tile.Content = tileContentFactory.Get(GameTileContentType.Destination);
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (numTowers > 0)
                {
                    if (board.ToggleTower(tile))
                    {
                        numTowers--;
                    }
                }

            }
            else
            {
                if (numWalls > 0)
                {
                    if (board.ToggleWall(tile))
                    {
                        numWalls--;
                    }
                }
            }

            UpdateUI();
        }
    }

    /// <summary>
    /// Handles left mouse click input specifically for wall placement and removal.
    /// Attempts to toggle a wall on the clicked tile if walls are available.
    /// Decrements wall count only if a new wall was successfully placed.
    /// </summary>
    void HandleTouchWalls()
    {
        GameTile tile = board.GetTile(TouchRay);
        if (tile != null)
        {
            if (numWalls > 0)
            {
                if (board.ToggleWall(tile))
                {
                    numWalls--;
                }
            }
        }

        UpdateUI();
    }

    /// <summary>
    /// Handles right mouse click input specifically for tower placement and removal.
    /// Attempts to toggle a tower on the clicked tile if towers are available.
    /// Decrements tower count only if a new tower was successfully placed.
    /// </summary>
    void HandleTouchTowers()
    {
        GameTile tile = board.GetTile(TouchRay);
        if (tile != null)
        {
            if (numTowers > 0)
            {
                if (board.ToggleTower(tile))
                {
                    numTowers--;
                }
            }
        }

        UpdateUI();
    }

    /// <summary>
    /// Updates all UI text elements with current game values.
    /// Refreshes the display of score, available towers, and available walls.
    /// Called whenever game state changes that affect these values.
    /// </summary>
    void UpdateUI()
    {
        scoreText.text = score.ToString();
        numTowersText.text = numTowers.ToString();
        numWallsText.text = numWalls.ToString();
    }

    /// <summary>
    /// Resets the game to its initial state and toggles the new game panel.
    /// Clears all enemies, resets the board, restores player health, and restarts the scenario.
    /// Can be called via UI button
    /// </summary>
    public void BeginNewGame()
    {
        Time.timeScale = 1f;
        NewGamePanel.SetActive(!NewGamePanel.activeSelf);
        gameOverText.SetActive(false);
        gameClearText.SetActive(false);

        isGameOverOrCleared = false;

        playerHealth = startingPlayerHealth;
        healthBar.ResetHealth(startingPlayerHealth); // Reset UI
        enemies.Clear();
        //nonEnemies.Clear();
        board.Clear();

        score = 0;
        numTowers = 15;
        numWalls = 15;
        UpdateUI();

        //activeScenario = scenario.Begin();
        // Reset scenario to start fresh
        ResetScenario();
    }

    /// <summary>
    /// Alternative touch handling method for placing spawn points and destinations.
    /// Left Shift + click places destinations, regular click places spawn points.
    /// Used for level editing or testing purposes, not in normal gameplay.
    /// </summary>
    void HandleAlternativeTouch()
    {
        GameTile tile = board.GetTile(TouchRay);
        if (tile != null)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                board.ToggleDestination(tile);
            }
            else
            {
                board.ToggleSpawnPoint(tile);
            }
        }
    }

    /// <summary>
    /// Called when script is loaded or values change in Inspector.
    /// Enforces minimum board size of 3x3 tiles to ensure proper gameplay functionality.
    /// </summary>
    void OnValidate()
    {
        //Enforces the board to be a 3x3 minimum
        if (boardSize.x < 3)
        {
            boardSize.x = 3;
        }
        if (boardSize.y < 3)
        {
            boardSize.y = 3;
        }
    }

    /// <summary>
    /// Transitions the game to the defeat state when the player runs out of health.
    /// Freezes time, displays game over UI, and prevents further gameplay until restart.
    /// </summary>
    void GameOver()
    {
        isGameOverOrCleared = true;
        Time.timeScale = 0f; // Freeze time once
        gameOverText.SetActive(true);
        Debug.Log("NewGamePanel is set to Active");
        NewGamePanel.SetActive(true);
    }

    /// <summary>
    /// Transitions the game to the victory state when all waves are completed.
    /// Freezes time, displays victory UI, and prevents further gameplay until restart.
    /// </summary>
    void GameClear()
    {
        isGameOverOrCleared = true;
        Time.timeScale = 0f; // Freeze time once
        gameClearText.SetActive(true);
        Debug.Log("NewGamePanel is set to Active");
        NewGamePanel.SetActive(true);
    }

    /// <summary>
    /// Called when the GameObject is being destroyed.
    /// Unsubscribes from settings manager events and cleans up the singleton instance reference.
    /// Ensures proper cleanup when transitioning between scenes.
    /// </summary>
    void OnDestroy()
    {
        // Unsubscribe from settings events
        if (settingsManager != null)
        {
            settingsManager.OnArrowsToggled -= SetShowPaths;
            settingsManager.OnGridToggled -= SetShowGrid;
            settingsManager.OnHealthBarsToggled -= SetShowEnemyHealthBars;
        }

        if (instance == this)
        {
            instance = null;
        }
    }
}
