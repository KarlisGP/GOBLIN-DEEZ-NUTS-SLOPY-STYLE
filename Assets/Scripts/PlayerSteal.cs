using UnityEngine;

public class PlayerSteal : MonoBehaviour
{
    public Camera playerCamera;
    public Inventory inventory;

    public float stealDistance = 3f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TrySteal();
        }
    }

    void TrySteal()
    {
        Ray ray = playerCamera.ScreenPointToRay(
            new Vector3(Screen.width / 2f, Screen.height / 2f)
        );

        if (Physics.Raycast(ray, out RaycastHit hit, stealDistance))
        {
            Stealable stealable = hit.collider.GetComponentInParent<Stealable>();

            if (stealable != null)
            {
                if (stealable.TrySteal(out string itemName, out int amount))
                {
                    inventory.AddItem(itemName, amount);
                }
            }
        }
    }
}