using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Starting Abilities")]
    public bool HasDash = true;
    public bool HasWallJump = true;
    public bool HasSpell = true;
    public bool HasBulletBlock = true;

    [Header("Starting Gold")]
    [SerializeField] private int startingGold = 10;

    [Header("Inventory")]
    [SerializeField] private int slotCount = 20;

    public int Gold { get; private set; }
    public int SlotCount => slotCount;

    private ItemData[] _slots;
    private int[] _stackCounts;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        Gold = startingGold;
        _slots = new ItemData[slotCount];
        _stackCounts = new int[slotCount];
    }

    public ItemData GetSlot(int index) => _slots[index];
    public int GetStackCount(int index) => _stackCounts[index];

    public bool AddItemToSlot(ItemData item, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotCount) return false;

        if (_slots[slotIndex] == null)
        {
            _slots[slotIndex] = item;
            _stackCounts[slotIndex] = 1;
            return true;
        }

        if (_slots[slotIndex] == item && _stackCounts[slotIndex] < item.maxStackSize)
        {
            _stackCounts[slotIndex]++;
            return true;
        }

        return false;
    }

    public bool TrySpendGold(int amount)
    {
        if (Gold < amount) return false;
        Gold -= amount;
        return true;
    }

    public void AddGold(int amount) { Gold += amount; }

    public void UnlockAbility(string ability)
    {
        switch (ability)
        {
            case "Dash":        HasDash = true;        break;
            case "WallJump":    HasWallJump = true;    break;
            case "Spell":       HasSpell = true;       break;
            case "BulletBlock": HasBulletBlock = true; break;
        }
    }
}
