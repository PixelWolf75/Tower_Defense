using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defensive tower that automatically targets and attacks enemies within range.
/// Inherits from GameTileContent to integrate with the tile placement system.
/// Features laser beam visualization and dynamic targeting with damage over time.
/// </summary>
public class Tower : GameTileContent
{
    /// <summary>
    /// Maximum distance at which the tower can target enemies (1.5-10.5 units)
    /// </summary>
    [SerializeField, Range(1.5f, 10.5f)]
    float targetingRange = 1.5f;

    /// <summary>
    /// Transform reference to the rotating turret component
    /// </summary>
    [SerializeField]
    Transform turret = default;

    /// <summary>
    /// Transform reference to the laser beam visual effect
    /// </summary>
    [SerializeField]
    Transform laserBeam = default;

    /// <summary>
    /// Damage dealt per second to targeted enemies (1-100 DPS)
    /// </summary>
    [SerializeField, Range(1f, 100f)]
    float damagePerSecond = 10f;

    /// <summary>
    /// Currently targeted enemy point, null when no target is acquired
    /// </summary>
    TargetPoint target;

    /// <summary>
    /// Layer mask for detecting enemy objects (layer 6 only)
    /// </summary>
    const int enemyLayerMask = 1 << 6;

    /// <summary>
    /// Static buffer array for physics overlap queries to avoid garbage collection
    /// </summary>
    static Collider[] targetsBuffer = new Collider[100];

    /// <summary>
    /// Cached original scale of the laser beam for length scaling
    /// </summary>
    Vector3 laserBeamScale;


    /// <summary>
    /// Caches the laser beam's original scale values.
    /// Called when the object is first created.
    /// </summary>
    void Awake()
    {
        laserBeamScale = laserBeam.localScale;
    }

    /// <summary>
    /// Unity Gizmos method - draws targeting range and current target line in the Scene view.
    /// Only visible when the tower GameObject is selected.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 position = transform.localPosition;
        position.y += 0.01f; // Slight vertical offset for visibility
        Debug.Log("Drawing gizmos");
        Gizmos.DrawWireSphere(position, targetingRange);

        // Draw line to current target if one exists
        if (target != null)
        {
            Gizmos.DrawLine(position, target.Position);
        }
    }

    /// <summary>
    /// Attempts to find and acquire a new enemy target within range.
    /// Uses physics overlap detection to find enemies on layer 6.
    /// </summary>
    /// <returns>True if a target was successfully acquired, false otherwise</returns>
    bool AcquireTarget()
    {
        Vector3 a = transform.localPosition;
        Vector3 b = a;
        b.y += 3f; // Create capsule height for detection

        // Use capsule overlap to detect enemies in range
        int hits = Physics.OverlapCapsuleNonAlloc(
            a, b, targetingRange, targetsBuffer, enemyLayerMask
        );

        if (hits > 0)
        {
            // Select random target from detected enemies
            target = targetsBuffer[Random.Range(0, hits)].GetComponent<TargetPoint>();
            Debug.Assert(target != null, "Targeted non-enemy!", targetsBuffer[0]);
            return true;
        }

        target = null;
        return false;
    }

    /// <summary>
    /// Checks if the current target is still within range and valid.
    /// Accounts for enemy scale when calculating effective targeting range.
    /// </summary>
    /// <returns>True if target is still valid and in range, false otherwise</returns>
    bool TrackTarget()
    {
        if (target == null)
        {
            return false;
        }

        // Calculate distance to target
        Vector3 a = transform.localPosition;
        Vector3 b = target.Position;
        float x = a.x - b.x;
        float z = a.z - b.z;

        // Adjust range based on enemy scale
        float r = targetingRange + 0.125f * target.Enemy.Scale;

        // Check if target is out of range (using squared distance for performance)
        if (x * x + z * z > r * r)
        {
            target = null;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Main update method called by the game loop.
    /// Handles target acquisition, tracking, and shooting behavior.
    /// </summary>
    public override void GameUpdate()
    {
        if (TrackTarget() || AcquireTarget())
        {
            Debug.Log("Locked on target!");
            Shoot();
        }
        else
        {
            // Hide laser beam when no target
            laserBeam.localScale = Vector3.zero;
        }
    }

    /// <summary>
    /// Executes the shooting behavior including aiming, laser visualization, and damage application.
    /// Orients turret and laser beam toward target, scales beam length, and applies continuous damage.
    /// </summary>
    void Shoot()
    {
        Vector3 point = target.Position;

        // Aim turret at target
        turret.LookAt(point);
        laserBeam.localRotation = turret.localRotation;

        // Calculate beam length and scale
        float d = Vector3.Distance(turret.position, point);
        laserBeamScale.z = d;
        laserBeam.localScale = laserBeamScale;

        // Position beam center between turret and target
        laserBeam.localPosition =
            turret.localPosition + 0.5f * d * laserBeam.forward;

        // Apply damage over time to the target
        target.Enemy.ApplyDamage(damagePerSecond * Time.deltaTime);
    }
}
