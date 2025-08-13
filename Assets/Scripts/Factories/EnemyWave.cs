using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject representing a complete wave of enemies composed of multiple spawn sequences.
/// Manages the sequential execution of enemy spawn patterns with proper timing.
/// </summary>
[CreateAssetMenu]
public class EnemyWave : ScriptableObject
{
    /// <summary>
    /// Array of spawn sequences that make up this wave, executed in order.
    /// Default contains one empty sequence for initial setup.
    /// </summary>
    [SerializeField]
    EnemySpawnSequence[] spawnSequences = {
        new EnemySpawnSequence()
    };

    /// <summary>
    /// Creates and returns a new State instance for this enemy wave.
    /// Used to begin execution of the entire wave sequence.
    /// </summary>
    /// <returns>New State initialized with this wave's spawn sequences</returns>
    public State Begin() => new State(this);

    /// <summary>
    /// State structure that tracks the progress of an entire enemy wave.
    /// Manages sequential execution of multiple spawn sequences with proper timing.
    /// </summary>
    [System.Serializable]
    public struct State
    {
        /// <summary>
        /// Reference to the enemy wave configuration this state represents
        /// </summary>
        EnemyWave wave;

        /// <summary>
        /// Index of the currently executing spawn sequence
        /// </summary>
        int index;

        /// <summary>
        /// State of the current spawn sequence being executed
        /// </summary>
        EnemySpawnSequence.State sequence;

        /// <summary>
        /// Initializes a new state for the given enemy wave.
        /// Sets up the first spawn sequence for execution.
        /// </summary>
        /// <param name="wave">The EnemyWave this state will manage</param>
        public State(EnemyWave wave)
        {
            this.wave = wave;
            index = 0;
            // Initialize with the first spawn sequence (assumes at least one exists)
            sequence = wave.spawnSequences[0].Begin();
        }

        /// <summary>
        /// Progresses the enemy wave by the given delta time.
        /// Manages sequential execution of spawn sequences, advancing to the next when current completes.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last update in seconds</param>
        /// <returns>Excess time if entire wave is complete, -1.0f if wave is still active</returns>
        public float Progress(float deltaTime)
        {
            // Progress the current spawn sequence
            deltaTime = sequence.Progress(deltaTime);

            // Handle sequence completion and advance to next sequences
            while (deltaTime >= 0f)
            {
                // Check if all sequences in the wave are complete
                if (++index >= wave.spawnSequences.Length)
                {
                    return deltaTime; // Return excess time, wave complete
                }

                // Start the next spawn sequence
                sequence = wave.spawnSequences[index].Begin();
                deltaTime = sequence.Progress(deltaTime);
            }

            return -1f; // Wave still active, no excess time
        }
    }
}