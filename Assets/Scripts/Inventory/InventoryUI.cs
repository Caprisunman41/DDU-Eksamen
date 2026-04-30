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

    [Header("Abilities & Gold")]
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text dashText;
    [SerializeField] private TMP_Text wallJumpText;
    [SerializeField] private TMP_Text spellText;
    [SerializeField] private TMP_Text bulletBlockText;

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
    }

    private void Refresh()
    {
        InventoryManager inv = InventoryManager.Instance;
        if (inv == null) return;

        goldText.text = inv.Gold.ToString();

        SetAbility(dashText,        "Dash",         inv.HasDash);
        SetAbility(wallJumpText,    "Wall Jump",    inv.HasWallJump);
        SetAbility(spellText,       "Spell",        inv.HasSpell);
        SetAbility(bulletBlockText, "Bullet Block", inv.HasBulletBlock);

        for (int i = 0; i < _slots.Length; i++)
        {
            int index = i;
            _slots[i].Setup(inv.GetSlot(i), inv.GetStackCount(i), item => ShowDescription(item));
        }
    }

    private void ShowDescription(ItemData item)
    {
        if (item == null)
        {
            descriptionPanel.SetActive(false);
            return;
        }

        descriptionPanel.SetActive(true);
        descriptionNameText.text = item.itemName;
        descriptionBodyText.text = item.description;
        descriptionIcon.sprite = item.icon;
        descriptionIcon.enabled = item.icon != null;
    }

    private void SetAbility(TMP_Text label, string name, bool unlocked)
    {
        Color c = unlocked ? Color.white : new Color(0.35f, 0.35f, 0.35f, 1f);
        label.text = name;
        label.color = c;
        label.faceColor = c;
        label.ForceMeshUpdate();
    }

    public void CloseInventory() => Toggle();
}
