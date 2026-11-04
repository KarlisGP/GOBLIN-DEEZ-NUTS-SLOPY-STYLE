using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Drives the villager's Animator from the NavMeshAgent's speed and the NPC's state.
/// Put on the same root object as NPC. Animator controller needs:
///   float "Speed"  and  trigger "Die"  (names configurable below).
/// </summary>
[RequireComponent(typeof(NavMeshAgent), typeof(NPC))]
public class NPCAnimator : MonoBehaviour
{
    [Tooltip("Leave empty to auto-find the Animator on the model child.")]
    [SerializeField] Animator animator;
    [SerializeField] string speedParam = "Speed";
    [SerializeField] string dieTrigger = "Die";
    [SerializeField] float dampTime = 0.1f;

    NavMeshAgent agent;
    NPC npc;
    int speedHash, dieHash;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        npc = GetComponent<NPC>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        speedHash = Animator.StringToHash(speedParam);
        dieHash = Animator.StringToHash(dieTrigger);
    }

    void OnEnable() => npc.StateChanged += OnStateChanged;
    void OnDisable() => npc.StateChanged -= OnStateChanged;

    void Update()
    {
        if (animator == null || !npc.IsAlive) return;
        animator.SetFloat(speedHash, agent.velocity.magnitude, dampTime, Time.deltaTime);
    }

    void OnStateChanged(NPC _, NPCState prev, NPCState next)
    {
        if (animator == null) return;

        if (next == NPCState.Dead)
        {
            animator.SetTrigger(dieHash);
        }
        else if (prev == NPCState.Dead)
        {
            // Reused from the pool: reset the animator out of the death pose.
            animator.Rebind();
            animator.Update(0f);
        }
    }
}