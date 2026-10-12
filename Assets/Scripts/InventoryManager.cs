using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public Inventory inventory;

    public GameObject inventoryPanel;
    public TMP_Text inventoryText;

    private bool inventoryOpen = false;

    void Start()
    {
        inventoryPanel.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.tabKey.wasPressedThisFrame)
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
            UpdateInventoryText();
        }
    }

    void UpdateInventoryText()
    {
        inventoryText.text = "INVENTORY\n\n";

        foreach (var item in inventory.GetItems())
        {
            inventoryText.text += item.Key + " x" + item.Value + "\n";
        }
    }
}