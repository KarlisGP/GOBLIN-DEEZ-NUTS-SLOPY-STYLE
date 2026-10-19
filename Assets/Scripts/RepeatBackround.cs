using UnityEngine;

public class RepeatBackground : MonoBehaviour
{
    private Vector3 startPos;
    private float repeatHeight;

    void Start()
    {
        startPos = transform.position;

        // Get half the background's height
        repeatHeight = GetComponent<BoxCollider2D>().bounds.size.y / 5f;
    }

    void Update()
    {
        if (GameManager.instance == null)
            return;

        if (GameManager.instance.gameState != GameState.gamePlay)
            return;

        // When the background moves below its starting point,
        // move it back up to repeat.
        if (transform.position.y < startPos.y - repeatHeight * 2f)
        {
            transform.position = startPos;
        }
    }
}