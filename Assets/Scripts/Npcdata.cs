using UnityEngine;

/// <summary>Define each NPC type as an asset: Create > NPC > NPC Data.</summary>
[CreateAssetMenu(menuName = "NPC/NPC Data", fileName = "NewNPCData")]
public class NPCData : ScriptableObject
{
    public string displayName = "NPC";
    public NPC prefab;                 // prefab must have NPC + NavMeshAgent
    public string faction = "Neutral";
    public float maxHealth = 100f;
    public float moveSpeed = 3.5f;
    public float wanderRadius = 10f;
    public Vector2 idleTimeRange = new Vector2(2f, 5f);
}