using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Game : MonoBehaviour
{

    [SerializeField]
    Vector2Int boardSize = new Vector2Int(11, 11); //Set as default as 11x11

    [SerializeField]
	GameTileContentFactory tileContentFactory = default;

    [SerializeField]
    EnemyFactory enemyFactory = default;

    [SerializeField]
    GameBoard board = default;

    [SerializeField, Range(0.1f, 10f)]
    float spawnSpeed = 1f;
    
    float spawnProgress;

    EnemyCollection enemies = new EnemyCollection();

    Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);

    void Awake()
    {
        board.Initialize(boardSize, tileContentFactory);
        board.ShowGrid = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update () {
		if (Input.GetMouseButtonDown(0)) {
            //Debug.Log("Left Click");			
            HandleTouch();
		}
        else if (Input.GetMouseButtonDown(1)) {
            //Debug.Log("Right Click");
			HandleAlternativeTouch();
		}

        if (Input.GetKeyDown(KeyCode.V))
        {
            board.ShowPaths = !board.ShowPaths;
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            board.ShowGrid = !board.ShowGrid;
        }

        spawnProgress += spawnSpeed * Time.deltaTime;
        while (spawnProgress >= 1f)
        {
            spawnProgress -= 1f;
            SpawnEnemy();
        }

        enemies.GameUpdate();
        Physics.SyncTransforms();
        board.GameUpdate();
    }

    void SpawnEnemy()
    {
        if (board.SpawnPointCount == 0)
        {
            return; // Don't spawn if no spawn points exist
        }

        GameTile spawnPoint =
            board.GetSpawnPoint(Random.Range(0, board.SpawnPointCount));
        Enemy enemy = enemyFactory.Get();
        enemy.SpawnOn(spawnPoint);

        enemies.Add(enemy);
    }

    void HandleTouch () {
		GameTile tile = board.GetTile(TouchRay);
		if (tile != null) {
            //tile.Content = tileContentFactory.Get(GameTileContentType.Destination);
            if (Input.GetKey(KeyCode.LeftShift))
            {
                board.ToggleTower(tile);
            }
            else
            {
                board.ToggleWall(tile);
            }
        }
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

    
}
