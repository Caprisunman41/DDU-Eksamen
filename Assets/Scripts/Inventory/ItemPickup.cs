using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemData item;
    [Tooltip("Hvilket slot index (0-based) dette item lægges i")]
    public int slotIndex;
    [Tooltip("Unik ID for dette pickup. Hvis sat, husker spillet at det er samlet op og det respawner ikke.")]
    public string pickupId;

    private void Start()
    {
        if (string.IsNullOrEmpty(pickupId)) return;
        if (GameStateManager.Instance != null && GameStateManager.Instance.CollectedPickups.Contains(pickupId))
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (InventoryManager.Instance.AddItemToSlot(item, slotIndex))
        {
            if (!string.IsNullOrEmpty(pickupId) && GameStateManager.Instance != null)
                GameStateManager.Instance.CollectedPickups.Add(pickupId);
            Destroy(gameObject);
        }
    }
}
