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

    [Header("Item Description")]
    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private Image descriptionIcon;
    [SerializeField] private TMP_Text descriptionNameText;
    [SerializeField] private TMP_Text descriptionBodyText;

    [Header("Ability Tooltip")]
    [SerializeField] private RectTransform tooltipPanel;
    [SerializeField] private TMP_Text tooltipNameText;
    [SerializeField] private TMP_Text tooltipBodyText;
    [SerializeField] private RectTransform canvasRect;

    private bool _isOpen;
    private InventorySlot[] _slots;

    private void Start()
    {
        BuildGrid();
        panel.SetActive(false);
        descriptionPanel.SetActive(false);
        if (tooltipPanel != null)
        {
            tooltipPanel.gameObject.SetActive(false);
            CanvasGroup cg = tooltipPanel.gameObject.GetComponent<CanvasGroup>();
            if (cg == null) cg = tooltipPanel.gameObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }
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
        slot.Setup(unlocked, ShowAbilityTooltip, HideTooltip);
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

    private void ShowAbilityTooltip(string abilityName, string description, RectTransform iconRect)
    {
        if (tooltipPanel == null) return;
        tooltipNameText.text = abilityName;
        tooltipBodyText.text = description;
        tooltipPanel.gameObject.SetActive(true);

        Canvas.ForceUpdateCanvases();

        Canvas canvas = canvasRect != null ? canvasRect.GetComponent<Canvas>() : GetComponentInParent<Canvas>();
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        // Get icon's screen position
        Vector3[] corners = new Vector3[4];
        iconRect.GetWorldCorners(corners);
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, corners[1]); // top-left corner

        // Convert to canvas local position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, cam, out Vector2 localPos);

        float tooltipW = tooltipPanel.rect.width;
        float tooltipH = tooltipPanel.rect.height;
        float canvasW = canvasRect.rect.width;
        float canvasH = canvasRect.rect.height;

        float x = localPos.x;
        float y = localPos.y + 10f;

        // Clamp accounting for pivot
        float pivotX = tooltipPanel.pivot.x;
        float pivotY = tooltipPanel.pivot.y;
        x = Mathf.Clamp(x, -canvasW / 2f + tooltipW * pivotX, canvasW / 2f - tooltipW * (1f - pivotX));
        if (y + tooltipH * (1f - pivotY) > canvasH / 2f)
            y = localPos.y - tooltipH - iconRect.rect.height - 10f;
        y = Mathf.Clamp(y, -canvasH / 2f + tooltipH * pivotY, canvasH / 2f - tooltipH * (1f - pivotY));

        tooltipPanel.anchoredPosition = new Vector2(x, y);
    }

    private void HideTooltip()
    {
        if (tooltipPanel != null) tooltipPanel.gameObject.SetActive(false);
    }

    public void CloseInventory() => Toggle();
}
