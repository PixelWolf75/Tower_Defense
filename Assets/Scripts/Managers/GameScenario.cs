using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ScriptableObject representing a complete game scenario composed of multiple enemy waves.
/// Manages wave progression, cycling, and difficulty scaling through multiple playthroughs.
/// </summary>
[CreateAssetMenu]
public class GameScenario : ScriptableObject
{

    /// <summary>
    /// Array of enemy waves that make up this scenario, executed sequentially
    /// </summary>
    [SerializeField]
    EnemyWave[] waves = { };

    /// <summary>
    /// Number of times to repeat the entire wave sequence (0 means infinite cycles)
    /// </summary>
    [SerializeField, Range(0, 10)]
    int cycles = 1;

    /// <summary>
    /// Speed multiplier increase per cycle completion (0.0-1.0 range for progressive difficulty)
    /// </summary>
    [SerializeField, Range(0f, 1f)]
    float cycleSpeedUp = 0.5f;

    /// <summary>
    /// Creates and returns a new State instance for this game scenario.
    /// Used to begin execution of the entire scenario sequence.
    /// </summary>
    /// <returns>New State initialized with this scenario's configuration</returns>
    public State Begin() => new State(this);


    /// <summary>
    /// State structure that tracks the progress of an entire game scenario.
    /// Manages wave progression, cycle counting, and speed scaling across multiple cycles.
    /// </summary>
    [System.Serializable]
    public struct State
    {
        /// <summary>
        /// Reference to the game scenario configuration this state represents
        /// </summary>
        GameScenario scenario;

        /// <summary>
        /// Index of the currently executing wave within the current cycle
        /// </summary>
        int index;

        /// <summary>
        /// Current cycle number (how many complete wave sequences have been completed)
        /// </summary>
        int cycle;

        /// <summary>
        /// Current time scale multiplier for increasing difficulty/speed
        /// </summary>
        float timeScale;

        /// <summary>
        /// State of the current enemy wave being executed
        /// </summary>
        EnemyWave.State wave;

        /// <summary>
        /// Initializes a new state for the given game scenario.
        /// Sets up the first wave and validates scenario configuration.
        /// </summary>
        /// <param name="scenario">The GameScenario this state will manage</param>
        public State(GameScenario scenario)
        {
            this.scenario = scenario;
            index = 0;
            cycle = 0;
            timeScale = 1f;

            // Ensure scenario has at least one wave
            Debug.Assert(scenario.waves.Length > 0, "Empty scenario!");

            // Initialize with the first wave
            wave = scenario.waves[0].Begin();
        }

        /// <summary>
        /// Progresses the game scenario by executing the current wave and managing transitions.
        /// Handles wave completion, cycle progression, and speed scaling automatically.
        /// </summary>
        /// <returns>True if scenario should continue, false if all cycles are complete</returns>
        public bool Progress()
        {
            // Progress current wave with scaled time
            float deltaTime = wave.Progress(timeScale * Time.deltaTime);

            // Handle wave completion and advance through scenario
            while (deltaTime >= 0f)
            {
                // Move to next wave
                if (++index >= scenario.waves.Length)
                {
                    // Check if all cycles are complete
                    if (++cycle >= scenario.cycles && scenario.cycles > 0)
                    {
                        return false; // Scenario complete
                    }

                    // Start new cycle with increased speed
                    index = 0;
                    timeScale += scenario.cycleSpeedUp;
                }

                // Initialize the next wave
                wave = scenario.waves[index].Begin();
                deltaTime = wave.Progress(deltaTime);
            }

            return true; // Scenario still active
        }
    }
}