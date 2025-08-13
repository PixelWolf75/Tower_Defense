using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a targetable point on an enemy for tower targeting systems.
/// Provides position data and enemy reference while ensuring proper component setup through assertions.
/// Must be attached to objects with SphereCollider components on layer 6 for tower detection.
/// </summary>
public class TargetPoint : MonoBehaviour
{
    /// <summary>
    /// reference to the Enemy component this target point belongs to.
    /// Automatically found in the root GameObject hierarchy during initialization.
    /// </summary>
    /// <value>The Enemy component associated with this target point</value>
    public Enemy Enemy { get; private set; }

    /// <summary>
    /// the world position of this target point.
    /// Used by tower targeting systems to calculate distances and aim directions.
    /// </summary>
    /// <value>Current world position of this target point's transform</value>
    public Vector3 Position => transform.position;

    /// <summary>
    /// validates component setup and finds the associated Enemy.
    /// Called when the object is first created, before Start().
    /// </summary>
    void Awake()
    {
        // Find the Enemy component in the root of the hierarchy
        Enemy = transform.root.GetComponent<Enemy>();
        Debug.Assert(Enemy != null, "Target point without Enemy root!", this);

        // Ensure proper collider setup for tower detection
        Debug.Assert(
            GetComponent<SphereCollider>() != null,
            "Target point without sphere collider!", this
        );

        // Verify correct layer assignment for targeting system
        Debug.Assert(gameObject.layer == 6, "Target point on wrong layer!", this);
    }
}
