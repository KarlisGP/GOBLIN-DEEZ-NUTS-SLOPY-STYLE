using UnityEngine;

public class MoveDown : MonoBehaviour
{
    [SerializeField] private float speed = 30f;

    void Update()
    {
        if (GameManager.instance != null &&
            GameManager.instance.gameState == GameState.gamePlay)
        {
            transform.Translate(Vector3.down * speed * Time.deltaTime);
        }
    }
}