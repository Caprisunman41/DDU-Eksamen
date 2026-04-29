using System;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    private ItemData _item;
    private Action<ItemData> _onClick;

    public void Setup(ItemData item, int count, Action<ItemData> onClick)
    {
        _item = item;
        _onClick = onClick;

        if (item != null && item.icon != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }

        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => _onClick?.Invoke(_item));
        }
    }
}
