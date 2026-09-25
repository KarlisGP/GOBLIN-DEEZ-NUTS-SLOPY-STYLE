using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Inventory inventory;
    public TMP_Text inventoryText;

    public bool IsOpen { get; private set; }

    void Start()
    {
        IsOpen = false;
        gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
        IsOpen = !IsOpen;

        gameObject.SetActive(IsOpen);

        if (IsOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            UpdateInventoryText();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void UpdateInventoryText()
    {
        inventoryText.text = "INVENTORY\n\n";

        Dictionary<string, int> items = inventory.GetItems();

        foreach (var item in items)
        {
            inventoryText.text += item.Key + " x" + item.Value + "\n";
        }
    }
}