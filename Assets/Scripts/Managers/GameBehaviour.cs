using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract base class for game objects that participate in the custom game update loop.
/// </summary>
public abstract class GameBehaviour : MonoBehaviour
{
    public virtual bool GameUpdate() => true;
    public abstract void Recycle();
}
