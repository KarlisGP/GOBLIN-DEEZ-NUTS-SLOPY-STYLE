using UnityEngine;

/// <summary>
/// Viens krītošs objekts. Pats nezina punktu skaitu -
/// tikai paziņo GameManager, kas ar to noticis.
/// Prasa: Collider2D un Rigidbody2D (Body Type = Dynamic).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Coin : MonoBehaviour
{
    // Peles klikšķis uz objekta
    private void OnMouseDown()
    {
        GameManager.instance.AddScore();
        Destroy(gameObject);
    }

    // Objekts nokrita uz grīdas
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Lava"))
        {
            GameManager.instance.RemoveScore();
            Destroy(gameObject);
        }
    }
}
