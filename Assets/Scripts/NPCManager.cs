using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Central NPC manager: spawning, pooling, staggered ticking, and queries.
/// Add to one GameObject in your scene.
/// </summary>
public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("Max NPCs ticked per frame. Higher = more responsive, lower = cheaper.")]
    [SerializeField] int ticksPerFrame = 50;
    [SerializeField] int maxNPCs = 200;

    readonly List<NPC> active = new List<NPC>();
    readonly List<NPC> pendingDespawn = new List<NPC>();
    readonly Dictionary<NPCData, Stack<NPC>> pools = new Dictionary<NPCData, Stack<NPC>>();
    int tickCursor;

    public IReadOnlyList<NPC> ActiveNPCs => active;
    public int Count => active.Count;

    public event Action<NPC> NPCSpawned;
    public event Action<NPC> NPCDespawned;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    void Update()
    {
        TickNPCs();
        FlushDespawns();
    }

    // ---------- Ticking (round-robin so cost is bounded) ----------
    void TickNPCs()
    {
        int n = active.Count;
        if (n == 0) return;

        int budget = Mathf.Min(ticksPerFrame, n);
        float now = Time.time;
        for (int i = 0; i < budget; i++)
        {
            if (tickCursor >= n) tickCursor = 0;
            var npc = active[tickCursor++];
            if (npc == null || !npc.IsAlive) continue;

            float dt = now - npc.LastTickTime;
            npc.LastTickTime = now;
            npc.Tick(dt);
        }
    }

    // ---------- Spawning ----------
    public NPC Spawn(NPCData data, Vector3 position, Quaternion? rotation = null)
    {
        if (data == null || data.prefab == null)
        {
            Debug.LogError("NPCManager.Spawn: missing NPCData or prefab.");
            return null;
        }
        if (active.Count >= maxNPCs)
        {
            Debug.LogWarning("NPCManager: max NPC count reached.");
            return null;
        }

        NPC npc = GetFromPool(data);
        if (NavMesh.SamplePosition(position, out var hit, 5f, NavMesh.AllAreas))
            position = hit.position;

        npc.transform.SetPositionAndRotation(position, rotation ?? Quaternion.identity);
        npc.gameObject.SetActive(true);

        var agent = npc.GetComponent<NavMeshAgent>();
        agent.Warp(position);

        npc.Initialize(data);
        npc.LastTickTime = Time.time;
        npc.Died += OnNPCDied;

        active.Add(npc);
        NPCSpawned?.Invoke(npc);
        return npc;
    }

    /// <summary>Safe to call from anywhere (even mid-tick); removal is deferred to end of frame.</summary>
    public void Despawn(NPC npc)
    {
        if (npc != null && !pendingDespawn.Contains(npc))
            pendingDespawn.Add(npc);
    }

    public void DespawnAll()
    {
        foreach (var npc in active) Despawn(npc);
    }

    void OnNPCDied(NPC npc) => Despawn(npc); // swap for a corpse/ragdoll delay if you want

    void FlushDespawns()
    {
        if (pendingDespawn.Count == 0) return;

        foreach (var npc in pendingDespawn)
        {
            if (npc == null) continue;
            npc.Died -= OnNPCDied;
            active.Remove(npc);
            NPCDespawned?.Invoke(npc);
            ReturnToPool(npc);
        }
        pendingDespawn.Clear();
        if (tickCursor >= active.Count) tickCursor = 0;
    }

    // ---------- Pooling ----------
    NPC GetFromPool(NPCData data)
    {
        if (!pools.TryGetValue(data, out var stack))
            pools[data] = stack = new Stack<NPC>();

        while (stack.Count > 0)
        {
            var pooled = stack.Pop();
            if (pooled != null) return pooled;
        }
        var npc = Instantiate(data.prefab, transform);
        npc.gameObject.SetActive(false);
        return npc;
    }

    void ReturnToPool(NPC npc)
    {
        npc.gameObject.SetActive(false);
        pools[npc.Data].Push(npc);
    }

    // ---------- Queries ----------
    public NPC GetNearest(Vector3 point, float maxDistance = float.MaxValue, string faction = null)
    {
        NPC best = null;
        float bestSqr = maxDistance * maxDistance;
        foreach (var npc in active)
        {
            if (!npc.IsAlive) continue;
            if (faction != null && npc.Data.faction != faction) continue;
            float sqr = (npc.transform.position - point).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; best = npc; }
        }
        return best;
    }

    /// <summary>Fills 'results' (cleared first) to avoid allocations.</summary>
    public void GetInRadius(Vector3 point, float radius, List<NPC> results, string faction = null)
    {
        results.Clear();
        float r2 = radius * radius;
        foreach (var npc in active)
        {
            if (!npc.IsAlive) continue;
            if (faction != null && npc.Data.faction != faction) continue;
            if ((npc.transform.position - point).sqrMagnitude <= r2) results.Add(npc);
        }
    }

    public void CommandAll(Action<NPC> action, string faction = null)
    {
        foreach (var npc in active)
            if (npc.IsAlive && (faction == null || npc.Data.faction == faction)) action(npc);
    }
}