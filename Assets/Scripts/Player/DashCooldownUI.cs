using UnityEngine;
using UnityEngine.UI;

public class DashCooldownUI : MonoBehaviour
{
    [SerializeField] private CharacterController _player;
    [SerializeField] private Image _fillImage;
    [SerializeField] private Image _iconImage;

    private Color _readyColor = Color.white;
    private Color _cooldownColor = new Color(0.4f, 0.4f, 0.4f, 1f);

    private void Update()
    {
        float progress = _player.DashCooldownProgress;
        _fillImage.fillAmount = progress;
        _iconImage.color = progress >= 1f ? _readyColor : _cooldownColor;
    }
}
