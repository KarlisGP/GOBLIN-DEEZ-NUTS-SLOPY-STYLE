using System;
using UnityEngine;

public enum NPCState { Idle, Wander, Follow, Dead }

/// <summary>
/// Individual NPC, moved directly by code (no NavMesh).
/// It has no Update(): NPCManager calls Move() every frame and Tick() staggered.
/// </summary>
public class NPC : MonoBehaviour
{
    public NPCData Data { get; private set; }
    public NPCState State { get; private set; } = NPCState.Idle;
    public float Health { get; private set; }
    public bool IsAlive => State != NPCState.Dead;
    public Transform FollowTarget { get; private set; }
    public Vector3 Home { get; private set; }
    /// <summary>Current movement speed in units/second (for animation).</summary>
    public float CurrentSpeed { get; private set; }

    [NonSerialized] public float LastTickTime;

    public event Action<NPC, NPCState, NPCState> StateChanged;
    public event Action<NPC> Died;

    Vector3 destination;
    bool hasDestination;
    float idleTimer;

    public void Initialize(NPCData data)
    {
        Data = data;
        Health = data.maxHealth;
        Home = transform.position;
        FollowTarget = null;
        hasDestination = false;
        CurrentSpeed = 0f;
        idleTimer = UnityEngine.Random.Range(data.idleTimeRange.x, data.idleTimeRange.y);

        var prev = State;
        State = NPCState.Idle;
        if (prev != NPCState.Idle) StateChanged?.Invoke(this, prev, State);
    }

    // ---------- Commands ----------
    public void Idle() { FollowTarget = null; hasDestination = false; SetState(NPCState.Idle); }

    public void Follow(Transform target)
    {
        FollowTarget = target;
        SetState(NPCState.Follow);
    }

    public void MoveTo(Vector3 position)
    {
        FollowTarget = null;
        destination = position;
        hasDestination = true;
        SetState(NPCState.Wander);
    }

    public void Wander()
    {
        FollowTarget = null;
        SetState(NPCState.Wander);

        Vector2 c = UnityEngine.Random.insideUnitCircle * Data.wanderRadius;
        Vector3 point = Home + new Vector3(c.x, 0f, c.y);

        if (Data.groundLayers.value != 0)
        {
            if (!TryGetGround(point, Data.groundLayers, out point)) { SetState(NPCState.Idle); return; }
        }
        destination = point;
        hasDestination = true;
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;
        Health -= amount;
        if (Health <= 0f) Die();
    }

    public void Die()
    {
        if (!IsAlive) return;
        hasDestination = false;
        CurrentSpeed = 0f;
        SetState(NPCState.Dead);
        Died?.Invoke(this);
    }

    // ---------- Called by NPCManager every frame ----------
    public void Move(float dt)
    {
        if (!IsAlive || dt <= 0f) return;

        if (State == NPCState.Follow && FollowTarget != null)
        {
            destination = FollowTarget.position;
            hasDestination = true;
        }

        if (!hasDestination) { CurrentSpeed = 0f; return; }

        Vector3 pos = transform.position;
        Vector3 to = destination - pos;
        to.y = 0f;
        float dist = to.magnitude;
        float stop = State == NPCState.Follow ? Data.followDistance : Data.stoppingDistance;

        if (dist <= stop) { Arrive(); return; }

        Vector3 dir = to / dist;

        if (Data.obstacleLayers.value != 0 &&
            Physics.Raycast(pos + Vector3.up * 0.5f, dir, Data.obstacleCheckDistance,
                            Data.obstacleLayers, QueryTriggerInteraction.Ignore))
        {
            Arrive(); // blocked: stop here
            return;
        }

        float step = Mathf.Min(Data.moveSpeed * dt, dist);
        pos += dir * step;

        if (Data.groundLayers.value != 0 && TryGetGround(pos, Data.groundLayers, out var ground))
            pos.y = ground.y;

        transform.position = pos;
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, Quaternion.LookRotation(dir, Vector3.up), Data.turnSpeed * dt);

        CurrentSpeed = step / dt;
    }

    // ---------- Called by NPCManager, staggered across frames ----------
    public void Tick(float dt)
    {
        switch (State)
        {
            case NPCState.Idle:
                idleTimer -= dt;
                if (idleTimer <= 0f) Wander();
                break;

            case NPCState.Wander:
                if (!hasDestination) SetState(NPCState.Idle);
                break;

            case NPCState.Follow:
                if (FollowTarget == null) Idle();
                break;
        }
    }

    // ---------- Helpers ----------
    void Arrive() { hasDestination = false; CurrentSpeed = 0f; }

    public static bool TryGetGround(Vector3 point, LayerMask mask, out Vector3 ground)
    {
        if (Physics.Raycast(point + Vector3.up * 3f, Vector3.down, out var hit, 10f,
                            mask, QueryTriggerInteraction.Ignore))
        {
            ground = hit.point;
            return true;
        }
        ground = point;
        return false;
    }

    void SetState(NPCState next)
    {
        if (State == next) return;
        var prev = State;
        State = next;
        if (next == NPCState.Idle)
            idleTimer = UnityEngine.Random.Range(Data.idleTimeRange.x, Data.idleTimeRange.y);
        StateChanged?.Invoke(this, prev, next);
    }
}