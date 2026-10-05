using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public Inventory inventory;
    public TMP_Text inventoryText;

    void Start()
    {
        // Inventory starts closed
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    void ToggleInventory()
    {
        bool open = !gameObject.activeSelf;

        gameObject.SetActive(open);

        if (open)
        {
            UpdateInventory();
        }
    }

    void UpdateInventory()
    {
        
        inventoryText.text = "INVENTORY\n\n";
    }
}