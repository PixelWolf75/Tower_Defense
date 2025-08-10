using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Small, Medium, Large
}

public class Enemy : GameBehaviour
{
    [SerializeField]
    Transform model = default;


    EnemyFactory originFactory;

    GameTile tileFrom, tileTo;
    Vector3 positionFrom, positionTo;
    float progress;

    
    public float Scale { get; private set; }

    float Health { get; set; }

    public EnemyFactory OriginFactory
    {
        get => originFactory;
        set
        {
            Debug.Assert(originFactory == null, "Redefined origin factory!");
            originFactory = value;
        }
    }

    public void Initialize(float scale, float health)
    {
        Scale = scale;
        model.localScale = new Vector3(scale, scale, scale);

        Health = health;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnOn(GameTile tile)
    {
        Debug.Assert(tile.NextTileOnPath != null, "Nowhere to go!", this);
        tileFrom = tile;
        tileTo = tile.NextTileOnPath;
        positionFrom = tileFrom.transform.localPosition;
        positionTo = tileTo.transform.localPosition;
        progress = 0f;
    }

    public override bool GameUpdate()
    {
        if (Health <= 0f)
        {
            Recycle();
            return false;
        }

        progress += Time.deltaTime;
        while (progress >= 1f)
        {
            tileFrom = tileTo;
            tileTo = tileTo.NextTileOnPath;

            if (tileTo == null)
            {
                Game.EnemyReachedDestination();
                Recycle();
                return false;
            }

            positionFrom = positionTo;
            positionTo = tileTo.transform.localPosition;
            progress -= 1f;
        }
        transform.localPosition = Vector3.LerpUnclamped(positionFrom, positionTo, progress);
        return true;
    }

    public void ApplyDamage(float damage)
    {
        Debug.Assert(damage >= 0f, "Negative damage applied.");
        Health -= damage;
    }

    public override void Recycle()
    {
        Game.EnemyHasBeenKilled();
        OriginFactory.Reclaim(this);
    }
}
