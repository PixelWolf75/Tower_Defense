using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Game : MonoBehaviour
{

    static Game instance;

    [SerializeField]
    Vector2Int boardSize = new Vector2Int(11, 11); //Set as default as 11x11

    [SerializeField, Min(1)]
    int numDestinations = 1;

    [SerializeField, Min(1)]
    int numSpawnPoints = 1;

    [SerializeField]
	GameTileContentFactory tileContentFactory = default;

    [SerializeField]
    GameObject gameOverText = default;

    [SerializeField]
    GameObject gameClearText = default;

    [SerializeField]
    TextMeshProUGUI numTowersText = default;

    [SerializeField]
    TextMeshProUGUI numWallsText = default;

    [SerializeField]
    TextMeshProUGUI scoreText = default;

    [SerializeField]
    GameObject NewGamePanel = default;

    //[SerializeField]
    //EnemyFactory enemyFactory = default;

    [SerializeField]
    GameBoard board = default;

    //[SerializeField, Range(0.1f, 10f)]
    //float spawnSpeed = 1f;

    [SerializeField, Range(0, 100)]
    int startingPlayerHealth = 10;

    [SerializeField]
    GameScenario scenario = default;

    [SerializeField, Range(1f, 10f)]
    float playSpeed = 1f;

    GameScenario.State activeScenario;

    [SerializeField]
    HealthBar healthBar = default;

    //float spawnProgress;

    int playerHealth;

    int currency;

    int score;

    int numTowers;
    int numWalls;

    bool isGameOverOrCleared;
    bool isInitialized = false; // Track if game is properly initialized

    const float pausedTimeScale = 0f;

    GameBehaviourCollection enemies = new GameBehaviourCollection();

    Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);

    void Awake()
    {
        instance = this;

        /*
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
    

        board.Initialize(boardSize, tileContentFactory, numSpawnPoints, numDestinations);
        board.ShowGrid = true;
        activeScenario = scenario.Begin();
        */

        Debug.Log("Game Awake - Initializing scene");
        InitializeGame();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Additional initialization in Start to ensure everything is ready
        if (!isInitialized)
        {
            Debug.LogWarning("Game not initialized in Awake, initializing in Start");
            InitializeGame();
        }

        // Force scenario to start fresh
        ResetScenario();
    }

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

    // Update is called once per frame
    void Update () {

        if (!isInitialized) return; // Don't update if not properly initialized

        if (Input.GetMouseButtonDown(0)) {
            //Debug.Log("Left Click");			
            HandleTouchWalls();
		}
        else if (Input.GetMouseButtonDown(1)) {
            //Debug.Log("Right Click");
			HandleTouchTowers();
		}

        if (Input.GetKeyDown(KeyCode.V))
        {
            board.ShowPaths = !board.ShowPaths;
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            board.ShowGrid = !board.ShowGrid;
        }

        /*
        spawnProgress += spawnSpeed * Time.deltaTime;
        while (spawnProgress >= 1f)
        {
            spawnProgress -= 1f;
            SpawnEnemy();
        }
        */

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
                    //BeginNewGame();
                    activeScenario.Progress();
                }
            }

            enemies.GameUpdate();
            Physics.SyncTransforms();
            board.GameUpdate();
        }
    }

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

    public static void EnemyReachedDestination()
    {
        if (instance == null) return;

        instance.playerHealth -= 1;
        instance.healthBar.ReduceHealth(1); // Update the UI
    }

    public static void EnemyHasBeenKilled()
    {
        if (instance == null) return;

        instance.currency += 1;
        instance.score += 1;
        instance.numWalls += 1;
        instance.numTowers += 1;

        instance.UpdateUI();
    }

    void HandleTouch () {
		GameTile tile = board.GetTile(TouchRay);
		if (tile != null) {
            //tile.Content = tileContentFactory.Get(GameTileContentType.Destination);
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (numTowers > 0)
                {
                    if(board.ToggleTower(tile))
                    {
                        numTowers--;
                    }
                }
                
            }
            else
            {
                if (numWalls > 0)
                {
                    if(board.ToggleWall(tile))
                    {
                        numWalls--;
                    }
                }
            }

            UpdateUI();
        }
	}

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

    void UpdateUI()
    {
        scoreText.text = score.ToString();
        numTowersText.text = numTowers.ToString();
        numWallsText.text = numWalls.ToString();
    }

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

    void HandleAlternativeTouch () {
		GameTile tile = board.GetTile(TouchRay);
		if (tile != null) {
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

    // Validate is called when script is loaded or after a value change
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

    void GameOver()
    {
        isGameOverOrCleared = true;
        Time.timeScale = 0f; // Freeze time once
        gameOverText.SetActive(true);
        Debug.Log("NewGamePanel is set to Active");
        NewGamePanel.SetActive(true);
    }

    void GameClear()
    {
        isGameOverOrCleared = true;
        Time.timeScale = 0f; // Freeze time once
        gameClearText.SetActive(true);
        Debug.Log("NewGamePanel is set to Active");
        NewGamePanel.SetActive(true);
    }

    // Clean up when leaving the scene
    void OnDestroy()
    {
        Debug.Log("Game instance destroyed");
        if (instance == this)
        {
            instance = null;
        }
    }
}
