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

    [SerializeField]
    Blink blinker = default;

    private EnemyHealthBarManager healthBarManager;

    EnemyFactory originFactory;

    GameTile tileFrom, tileTo;
    Vector3 positionFrom, positionTo;
    float progress;

    private bool bIsBlinking = false;
    
    public float Scale { get; private set; }
    private float maxScale;

    float Health { get; set; }
    float MaxHealth { get; set; } // Added to track max health

    public EnemyFactory OriginFactory
    {
        get => originFactory;
        set
        {
            Debug.Assert(originFactory == null, "Redefined origin factory!");
            originFactory = value;
        }
    }

    void Awake()
    {
        // Get the health bar manager component
        healthBarManager = GetComponent<EnemyHealthBarManager>();
    }

    public void Initialize(float scale, float health)
    {
        Scale = scale;
        maxScale = Scale;
        model.localScale = new Vector3(scale, scale, scale);

        Health = health;
        MaxHealth = health;

        // Update health bar if available
        if (healthBarManager != null)
        {
            healthBarManager.UpdateHealth(Health, MaxHealth);
        }
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
        //Debug.Assert(tile.NextTileOnPath != null, "Nowhere to go!", this);
        tileFrom = tile;
        tileTo = tile.NextTileOnPath;
        positionFrom = tileFrom.transform.localPosition;
        positionTo = tileTo.transform.localPosition;
        progress = 0f;

        // Show health bar when spawned
        if (healthBarManager != null)
        {
            //Debug.Log($"[{gameObject.name}] Found healthBarManager, calling ShowHealthBar(true)");
            healthBarManager.ShowHealthBar(true);
        }
        else
        {
            //Debug.LogError($"[{gameObject.name}] healthBarManager is null in SpawnOn!");
        }
    }

    public override bool GameUpdate()
    {
        if(Health <= 50f && !bIsBlinking)
        {
            //Debug.Log("Start blinking");
            blinker.StartBlinking();
            bIsBlinking = true;
        }

        if(Health <= 10f)
        {
            blinker.SetBlinkInterval(0.01f);
        }

        if (Health <= 0f)
        {
            blinker.CancelBlinking();

            // Hide health bar before recycling
            if (healthBarManager != null)
            {
                healthBarManager.ShowHealthBar(false);
            }
            Game.EnemyHasBeenKilled();
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
                // Hide health bar when reaching destination
                if (healthBarManager != null)
                {
                    healthBarManager.ShowHealthBar(false);
                }

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

        Shrink();

        if (healthBarManager != null)
        {
            healthBarManager.UpdateHealth(Health, MaxHealth);
        }
    }

    void Shrink()
    {
        Scale = maxScale * (Health / MaxHealth);
        Debug.Log(Scale);
        model.localScale = new Vector3(Scale, Scale, Scale);
    }

    public override void Recycle()
    {
        if (healthBarManager != null)
        {
            healthBarManager.ShowHealthBar(false);
        }

        OriginFactory.Reclaim(this);
    }
}
