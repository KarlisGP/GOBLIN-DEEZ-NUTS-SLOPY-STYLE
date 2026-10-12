using UnityEngine;

public class Stealable : MonoBehaviour
{
    public string itemName = "Item";
    public int amount = 1;

    public void Steal(Inventory inventory)
    {
        inventory.AddItem(itemName, amount);

        Destroy(gameObject);
    }
}