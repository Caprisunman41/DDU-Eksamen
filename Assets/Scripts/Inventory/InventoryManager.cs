using System.Collections.Generic;
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

    public int Gold { get; private set; }
    public IReadOnlyList<ItemData> Items => _items;

    private readonly List<ItemData> _items = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        Gold = startingGold;
    }

    public bool TrySpendGold(int amount)
    {
        if (Gold < amount) return false;
        Gold -= amount;
        return true;
    }

    public void AddGold(int amount) { Gold += amount; }
    public void AddItem(ItemData item) { _items.Add(item); }

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
