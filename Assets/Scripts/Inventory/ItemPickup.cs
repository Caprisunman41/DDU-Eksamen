using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemData item;
    [Tooltip("Hvilket slot index (0-based) dette item lægges i")]
    public int slotIndex;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (InventoryManager.Instance.AddItemToSlot(item, slotIndex))
            Destroy(gameObject);
    }
}
