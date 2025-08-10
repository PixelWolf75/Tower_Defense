using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameBehaviourCollection
{

    List<GameBehaviour> behaviours = new List<GameBehaviour>();

    public bool IsEmpty => behaviours.Count == 0;

    public void Add(GameBehaviour behaviour)
    {
        behaviours.Add(behaviour);
    }

    public void GameUpdate()
    {
        for (int i = 0; i < behaviours.Count; i++)
        {
            if (!behaviours[i].GameUpdate())
            {
                int lastIndex = behaviours.Count - 1;
                behaviours[i] = behaviours[lastIndex];
                behaviours.RemoveAt(lastIndex);
                i -= 1;
            }
        }
    }

    public void Clear()
    {
        for (int i = 0; i < behaviours.Count; i++)
        {
            behaviours[i].Recycle();
        }
        behaviours.Clear();
    }
}