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
    GameBoard board = default;

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
    }

	void HandleTouch () {
		GameTile tile = board.GetTile(TouchRay);
		if (tile != null) {
            //tile.Content = tileContentFactory.Get(GameTileContentType.Destination);
            Debug.Log("Toggle Wall");
            board.ToggleWall(tile);
        }
	}

    void HandleAlternativeTouch () {
		GameTile tile = board.GetTile(TouchRay);
		if (tile != null) {
			board.ToggleDestination(tile);
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
