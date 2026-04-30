using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilitySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string abilityName;
    [SerializeField, TextArea] private string description;

    private Image _icon;
    private Action<string, string> _onHoverEnter;
    private Action _onHoverExit;

    private void Awake()
    {
        _icon = GetComponent<Image>();
    }

    public void Setup(bool unlocked, Action<string, string> onHoverEnter, Action onHoverExit)
    {
        _onHoverEnter = onHoverEnter;
        _onHoverExit = onHoverExit;
        _icon.enabled = unlocked;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_icon.enabled)
            _onHoverEnter?.Invoke(abilityName, description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _onHoverExit?.Invoke();
    }
}
