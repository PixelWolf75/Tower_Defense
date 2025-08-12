using UnityEngine;

public class ScenarioDebugger : MonoBehaviour
{
    [Header("Debug Info")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private float debugInterval = 2f;

    private float debugTimer;

    void Update()
    {
        if (!showDebugInfo) return;

        debugTimer += Time.deltaTime;
        if (debugTimer >= debugInterval)
        {
            debugTimer = 0f;
            LogDebugInfo();
        }
    }

    void LogDebugInfo()
    {
        Debug.Log("=== SCENARIO DEBUG ===");
        Debug.Log($"Time.timeScale: {Time.timeScale}");
        Debug.Log($"Time.deltaTime: {Time.deltaTime}");

        // Check if Game instance exists
        Game gameInstance = FindObjectOfType<Game>();
        if (gameInstance != null)
        {
            Debug.Log("Game instance found");
        }
        else
        {
            Debug.LogError("No Game instance found!");
        }

        // Check spawn points
        GameBoard board = FindObjectOfType<GameBoard>();
        if (board != null)
        {
            Debug.Log($"Board found. Spawn points: {board.SpawnPointCount}");
        }
        else
        {
            Debug.LogWarning("No GameBoard found!");
        }

        Debug.Log("====================");
    }
}