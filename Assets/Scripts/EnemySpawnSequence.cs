using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class EnemySpawnSequence
{

    [SerializeField]
    EnemyFactory factory = default;

    [SerializeField]
    EnemyType type = EnemyType.Medium;

    [SerializeField, Range(1, 100)]
    int amount = 1;

    [SerializeField, Range(0.1f, 10f)]
    float cooldown = 1f;

    [System.Serializable]
    public struct State
    {

        EnemySpawnSequence sequence;

        int count;

        float cooldown;

        public State(EnemySpawnSequence sequence)
        {
            this.sequence = sequence;
            count = 0;
            cooldown = 0f;//sequence.cooldown;
        }

        public float Progress(float deltaTime)
        {
            cooldown += deltaTime;
            //Debug.Log($"Sequence Progress: deltaTime={deltaTime}, cooldown={cooldown}, sequence.cooldown={sequence.cooldown}, count={count}, amount={sequence.amount}");
            while (cooldown >= sequence.cooldown)
            {
                cooldown -= sequence.cooldown;
                if (count >= sequence.amount)
                {
                    return cooldown;
                }
                count += 1;
                //Debug.Log($"Spawning enemy #{count}");
                Game.SpawnEnemy(sequence.factory, sequence.type);
            }

            return -1f;
        }
    }

    public State Begin() => new State(this);
}