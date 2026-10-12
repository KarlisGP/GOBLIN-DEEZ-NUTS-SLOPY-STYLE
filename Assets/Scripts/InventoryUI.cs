using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("Inventory")]
    public Inventory inventory;

    [Header("UI")]
    public GameObject inventoryPanel;
    public TMP_Text inventoryText;

    private bool inventoryOpen = false;

    void Start()
    {
        inventoryPanel.SetActive(false);

        RefreshInventory();
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
        inventoryOpen = !inventoryOpen;

        inventoryPanel.SetActive(inventoryOpen);

        if (inventoryOpen)
        {
            RefreshInventory();
        }
    }

    // This can be called by other scripts whenever the inventory changes
    public void RefreshInventory()
    {
        if (inventory == null)
        {
            Debug.LogError("InventoryUI: Inventory is not assigned!");
            return;
        }

        if (inventoryText == null)
        {
            Debug.LogError("InventoryUI: Inventory Text is not assigned!");
            return;
        }

        inventoryText.text = "INVENTORY\n\n";

        foreach (var item in inventory.GetItems())
        {
            inventoryText.text += item.Key + " x" + item.Value + "\n";
        }
    }
}