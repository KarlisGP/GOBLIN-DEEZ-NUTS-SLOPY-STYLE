using UnityEngine;

/// <summary>Optional quick test: spawns NPCs around this object on Start. Press F to make them follow the player.</summary>
public class NPCSpawnTest : MonoBehaviour
{
    public NPCData data;
    public int count = 10;
    public float radius = 15f;
    public Transform player;

    void Start()
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 p = Random.insideUnitCircle * radius;
            NPCManager.Instance.Spawn(data, transform.position + new Vector3(p.x, 0, p.y));
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && player != null)
            NPCManager.Instance.CommandAll(n => n.Follow(player));
    }
}