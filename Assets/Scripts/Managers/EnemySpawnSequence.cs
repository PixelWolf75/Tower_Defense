using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Defines a sequence of enemy spawns with specific timing, quantity, and type parameters.
/// Serializable class that can be configured in the Unity Inspector for wave-based enemy spawning.
/// </summary>
[System.Serializable]
public class EnemySpawnSequence
{
    /// <summary>
    /// Factory reference used to create enemies of the specified type
    /// </summary>
    [SerializeField]
    EnemyFactory factory = default;

    /// <summary>
    /// Type of enemy to spawn in this sequence
    /// </summary>
    [SerializeField]
    EnemyType type = EnemyType.Medium;

    /// <summary>
    /// Total number of enemies to spawn in this sequence (1-100)
    /// </summary>
    [SerializeField, Range(1, 100)]
    int amount = 1;

    /// <summary>
    /// Time delay between individual enemy spawns in seconds (0.1-10.0)
    /// </summary>
    [SerializeField, Range(0.1f, 10f)]
    float cooldown = 1f;

    /// <summary>
    /// State structure that tracks the progress of an enemy spawn sequence.
    /// Contains timing and count information for active spawn sequences.
    /// </summary>
    [System.Serializable]
    public struct State
    {
        /// <summary>
        /// Reference to the spawn sequence configuration this state represents
        /// </summary>
        EnemySpawnSequence sequence;

        /// <summary>
        /// Current number of enemies spawned in this sequence
        /// </summary>
        int count;

        /// <summary>
        /// Current cooldown timer accumulating delta time
        /// </summary>
        float cooldown;

        /// <summary>
        /// Initializes a new state for the given spawn sequence.
        /// </summary>
        /// <param name="sequence">The EnemySpawnSequence this state will track</param>
        public State(EnemySpawnSequence sequence)
        {
            this.sequence = sequence;
            count = 0;
            cooldown = 0f; // Start with no cooldown delay
        }

        /// <summary>
        /// Progresses the spawn sequence by the given delta time.
        /// Spawns enemies when cooldown thresholds are met and updates internal timing.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last update in seconds</param>
        /// <returns>Excess time if sequence is complete, -1.0f if sequence is still active</returns>
        public float Progress(float deltaTime)
        {
            // Accumulate time for spawn timing
            cooldown += deltaTime;

            // Process all pending spawns based on accumulated time
            while (cooldown >= sequence.cooldown)
            {
                cooldown -= sequence.cooldown; // Remove one cooldown interval

                // Check if sequence is complete
                if (count >= sequence.amount)
                {
                    return cooldown; // Return excess time for next sequence
                }

                // Spawn next enemy and increment counter
                count += 1;
                Game.SpawnEnemy(sequence.factory, sequence.type);
            }

            return -1f; // Sequence still active, no excess time
        }
    }

    /// <summary>
    /// Creates and returns a new State instance for this spawn sequence.
    /// Used to begin execution of the spawn sequence.
    /// </summary>
    /// <returns>New State initialized with this sequence's configuration</returns>
    public State Begin() => new State(this);
}