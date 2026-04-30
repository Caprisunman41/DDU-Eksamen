using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("Grid")]
    [SerializeField] private Transform gridParent;
    [SerializeField] private GameObject slotPrefab;

    [Header("Gold")]
    [SerializeField] private TMP_Text goldText;

    [Header("Ability Icons")]
    [SerializeField] private AbilitySlot dashSlot;
    [SerializeField] private AbilitySlot wallJumpSlot;
    [SerializeField] private AbilitySlot spellSlot;
    [SerializeField] private AbilitySlot bulletBlockSlot;

    [Header("Description")]
    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private Image descriptionIcon;
    [SerializeField] private TMP_Text descriptionNameText;
    [SerializeField] private TMP_Text descriptionBodyText;

    private bool _isOpen;
    private InventorySlot[] _slots;

    private void Start()
    {
        BuildGrid();
        panel.SetActive(false);
        descriptionPanel.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
            Toggle();
    }

    private void BuildGrid()
    {
        int count = InventoryManager.Instance.SlotCount;
        _slots = new InventorySlot[count];

        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(slotPrefab, gridParent);
            _slots[i] = go.GetComponent<InventorySlot>();
        }
    }

    private void Toggle()
    {
        _isOpen = !_isOpen;
        panel.SetActive(_isOpen);
        InputManager.IsBlocked = _isOpen;
        if (_isOpen)
        {
            descriptionPanel.SetActive(false);
            Refresh();
        }
        else
        {
            descriptionPanel.SetActive(false);
        }
    }

    private void Refresh()
    {
        InventoryManager inv = InventoryManager.Instance;
        if (inv == null) return;

        goldText.text = inv.Gold.ToString();

        SetupAbilitySlot(dashSlot,        inv.HasDash);
        SetupAbilitySlot(wallJumpSlot,    inv.HasWallJump);
        SetupAbilitySlot(spellSlot,       inv.HasSpell);
        SetupAbilitySlot(bulletBlockSlot, inv.HasBulletBlock);

        for (int i = 0; i < _slots.Length; i++)
            _slots[i].Setup(inv.GetSlot(i), inv.GetStackCount(i), item => ShowDescription(item));
    }

    private void SetupAbilitySlot(AbilitySlot slot, bool unlocked)
    {
        if (slot == null) return;
        slot.Setup(unlocked, ShowAbilityDescription, HideDescription);
    }

    private void ShowDescription(ItemData item)
    {
        if (item == null) { descriptionPanel.SetActive(false); return; }
        descriptionPanel.SetActive(true);
        descriptionNameText.text = item.itemName;
        descriptionBodyText.text = item.description;
        descriptionIcon.sprite = item.icon;
        descriptionIcon.enabled = item.icon != null;
    }

    private void ShowAbilityDescription(string abilityName, string description)
    {
        descriptionPanel.SetActive(true);
        descriptionNameText.text = abilityName;
        descriptionBodyText.text = description;
        descriptionIcon.enabled = false;
    }

    private void HideDescription()
    {
        descriptionPanel.SetActive(false);
    }

    public void CloseInventory() => Toggle();
}
