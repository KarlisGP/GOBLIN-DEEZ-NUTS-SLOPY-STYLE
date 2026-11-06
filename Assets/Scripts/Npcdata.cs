using UnityEngine;

/// <summary>Define each NPC type as an asset: Create > NPC > NPC Data.</summary>
[CreateAssetMenu(menuName = "NPC/NPC Data", fileName = "NewNPCData")]
public class NPCData : ScriptableObject
{
    [Header("General")]
    public string displayName = "NPC";
    public NPC prefab;                 // prefab needs the NPC component (+ NPCAnimator if animated)
    public string faction = "Neutral";
    public float maxHealth = 100f;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float turnSpeed = 360f;          // degrees per second
    public float stoppingDistance = 0.2f;   // how close counts as "arrived"
    public float followDistance = 2f;       // how close to get when following a target

    [Header("Wander")]
    public float wanderRadius = 10f;        // around the spawn point
    public Vector2 idleTimeRange = new Vector2(2f, 5f);

    [Header("World (set these to your ground / wall layers)")]
    [Tooltip("Ground to stand on. NPCs snap to it. Leave on Nothing to keep the spawn height.")]
    public LayerMask groundLayers = ~0;
    [Tooltip("Walls/props that block movement. Leave on Nothing to ignore obstacles.")]
    public LayerMask obstacleLayers = 0;
    public float obstacleCheckDistance = 0.6f;
}