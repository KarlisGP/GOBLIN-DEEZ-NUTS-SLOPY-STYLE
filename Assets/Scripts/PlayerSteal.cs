using UnityEngine;

public class PlayerSteal : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public Inventory inventory;
    public InventoryUI inventoryUI;
    public UIManager uiManager;

    [Header("Stealing")]
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
        if (playerCamera == null)
        {
            Debug.LogError("PlayerSteal: Player Camera is not assigned!");
            return;
        }

        if (inventory == null)
        {
            Debug.LogError("PlayerSteal: Inventory is not assigned!");
            return;
        }

        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        if (Physics.Raycast(ray, out RaycastHit hit, stealDistance))
        {
            Debug.Log("Ray hit: " + hit.collider.gameObject.name);

            Stealable item = hit.collider.GetComponentInParent<Stealable>();

            if (item != null)
            {
                string itemName = item.itemName;
                int amount = item.amount;

                // Add item to inventory
                item.Steal(inventory);

                // Update inventory UI
                if (inventoryUI != null)
                {
                    inventoryUI.RefreshInventory();
                }

                // Show popup
                if (uiManager != null)
                {
                    uiManager.ShowPopup(
                        "Stole " + itemName + " x" + amount
                    );
                }

                Debug.Log("Successfully stole " + itemName + " x" + amount);
            }
            else
            {
                Debug.Log("Object is not stealable.");
            }
        }
        else
        {
            Debug.Log("Ray did not hit anything.");
        }
    }
}