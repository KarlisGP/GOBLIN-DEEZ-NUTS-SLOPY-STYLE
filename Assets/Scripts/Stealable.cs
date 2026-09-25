using UnityEngine;

public class Stealable : MonoBehaviour
{
    public string itemName = "Item";
    public int amount = 1;

    public bool removeAfterStealing = true;

    private bool hasBeenStolen = false;

    public bool TrySteal(out string stolenItem, out int stolenAmount)
    {
        stolenItem = "";
        stolenAmount = 0;

        if (hasBeenStolen)
            return false;

        hasBeenStolen = true;

        stolenItem = itemName;
        stolenAmount = amount;

        if (removeAfterStealing)
        {
            gameObject.SetActive(false);
        }

        return true;
    }
}