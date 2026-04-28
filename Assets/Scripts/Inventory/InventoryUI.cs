using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text itemsText;
    [SerializeField] private TMP_Text dashText;
    [SerializeField] private TMP_Text wallJumpText;
    [SerializeField] private TMP_Text spellText;
    [SerializeField] private TMP_Text bulletBlockText;

    private bool _isOpen;

    private void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
            Toggle();
    }

    private void Toggle()
    {
        _isOpen = !_isOpen;
        panel.SetActive(_isOpen);
        if (_isOpen) Refresh();
    }

    private void Refresh()
    {
        InventoryManager inv = InventoryManager.Instance;
        if (inv == null) return;

        goldText.text = $"Guld: {inv.Gold}";

        SetAbility(dashText,        "Dash",         inv.HasDash);
        SetAbility(wallJumpText,    "Wall Jump",    inv.HasWallJump);
        SetAbility(spellText,       "Spell",        inv.HasSpell);
        SetAbility(bulletBlockText, "Bullet Block", inv.HasBulletBlock);

        itemsText.text = inv.Items.Count == 0
            ? "Ingen items"
            : string.Join("\n", inv.Items.Select(i => i.itemName));
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
