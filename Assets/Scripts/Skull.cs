using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Skull : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Lava"))
        {
            if (GameManager.instance != null)
                GameManager.instance.SkullPassed();

            Destroy(gameObject);
        }
    }
    private void OnMouseDown()
    {
        if (GameManager.instance == null)
            return;
        Destroy(gameObject);
    }
}