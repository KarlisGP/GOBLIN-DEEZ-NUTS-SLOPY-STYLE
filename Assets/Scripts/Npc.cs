using System;
using UnityEngine;
using UnityEngine.AI;

public enum NPCState { Idle, Wander, Follow, Dead }

/// <summary>
/// Individual NPC. It has no Update(): NPCManager calls Tick() so updates
/// can be centralised and staggered across frames.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class NPC : MonoBehaviour
{
    public NPCData Data { get; private set; }
    public NPCState State { get; private set; } = NPCState.Idle;
    public float Health { get; private set; }
    public bool IsAlive => State != NPCState.Dead;
    public Transform FollowTarget { get; private set; }

    /// <summary>Used by the manager for staggered ticking.</summary>
    [NonSerialized] public float LastTickTime;

    public event Action<NPC, NPCState, NPCState> StateChanged;
    public event Action<NPC> Died;

    NavMeshAgent agent;
    float idleTimer;

    void Awake() => agent = GetComponent<NavMeshAgent>();

    public void Initialize(NPCData data)
    {
        Data = data;
        Health = data.maxHealth;
        agent.speed = data.moveSpeed;
        agent.enabled = true;
        FollowTarget = null;
        SetState(NPCState.Idle);
    }

    // ---------- Commands ----------
    public void Idle() { FollowTarget = null; agent.ResetPath(); SetState(NPCState.Idle); }
    public void Wander() { FollowTarget = null; SetState(NPCState.Wander); PickWanderPoint(); }
    public void Follow(Transform target) { FollowTarget = target; SetState(NPCState.Follow); }
    public void MoveTo(Vector3 pos) { FollowTarget = null; SetState(NPCState.Wander); agent.SetDestination(pos); }

    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;
        Health -= amount;
        if (Health <= 0f) Die();
    }

    public void Die()
    {
        if (!IsAlive) return;
        agent.ResetPath();
        SetState(NPCState.Dead);
        Died?.Invoke(this);
    }

    // ---------- Called by NPCManager ----------
    public void Tick(float dt)
    {
        switch (State)
        {
            case NPCState.Idle:
                idleTimer -= dt;
                if (idleTimer <= 0f) Wander();
                break;

            case NPCState.Wander:
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f)
                {
                    idleTimer = UnityEngine.Random.Range(Data.idleTimeRange.x, Data.idleTimeRange.y);
                    SetState(NPCState.Idle);
                }
                break;

            case NPCState.Follow:
                if (FollowTarget == null) { Idle(); break; }
                agent.SetDestination(FollowTarget.position);
                break;
        }
    }

    // ---------- Helpers ----------
    void PickWanderPoint()
    {
        Vector3 random = transform.position + UnityEngine.Random.insideUnitSphere * Data.wanderRadius;
        if (NavMesh.SamplePosition(random, out var hit, Data.wanderRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    void SetState(NPCState next)
    {
        if (State == next) return;
        var prev = State;
        State = next;
        if (next == NPCState.Idle && prev != NPCState.Idle)
            idleTimer = UnityEngine.Random.Range(Data.idleTimeRange.x, Data.idleTimeRange.y);
        StateChanged?.Invoke(this, prev, next);
    }
}