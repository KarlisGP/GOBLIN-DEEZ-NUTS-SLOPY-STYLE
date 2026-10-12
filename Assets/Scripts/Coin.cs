using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Coin : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (GameManager.instance == null)
            return;

        if (GameManager.instance.gameState != GameState.gamePlay)
            return;

        GameManager.instance.AddScore();
        Destroy(gameObject);
    }
}