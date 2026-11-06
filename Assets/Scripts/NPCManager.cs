using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central NPC manager: spawning, pooling, movement, staggered thinking, and queries.
/// Add to one GameObject in your scene.
/// </summary>
public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("Max NPCs 'thinking' per frame. Movement still runs every frame.")]
    [SerializeField] int ticksPerFrame = 50;
    [SerializeField] int maxNPCs = 200;
    [Tooltip("Seconds a dead NPC stays in the scene (lets the death animation play).")]
    [SerializeField] float deathDespawnDelay = 3f;

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
        float dt = Time.deltaTime;
        for (int i = 0; i < active.Count; i++)
            active[i].Move(dt);          // smooth movement every frame

        TickNPCs();                      // decisions, staggered
        FlushDespawns();
    }

    // ---------- Thinking (round-robin so cost is bounded) ----------
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

        if (data.groundLayers.value != 0 && NPC.TryGetGround(position, data.groundLayers, out var ground))
            position = ground;

        NPC npc = GetFromPool(data);
        npc.transform.SetPositionAndRotation(position, rotation ?? Quaternion.identity);
        npc.gameObject.SetActive(true);

        npc.Initialize(data);
        npc.LastTickTime = Time.time;
        npc.Died += OnNPCDied;

        active.Add(npc);
        NPCSpawned?.Invoke(npc);
        return npc;
    }

    /// <summary>Safe to call from anywhere; removal is deferred to end of frame.</summary>
    public void Despawn(NPC npc)
    {
        if (npc != null && active.Contains(npc) && !pendingDespawn.Contains(npc))
            pendingDespawn.Add(npc);
    }

    public void DespawnAll()
    {
        foreach (var npc in active) Despawn(npc);
    }

    void OnNPCDied(NPC npc) => StartCoroutine(DespawnAfterDelay(npc));

    IEnumerator DespawnAfterDelay(NPC npc)
    {
        if (deathDespawnDelay > 0f) yield return new WaitForSeconds(deathDespawnDelay);
        // Skip if it was already despawned and reused while we waited.
        if (npc != null && !npc.IsAlive) Despawn(npc);
    }

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