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
        if (!Mathf.Approximately(_fillImage.fillAmount, progress))
            _fillImage.fillAmount = progress;
        Color target = progress >= 1f ? _readyColor : _cooldownColor;
        if (_iconImage.color != target)
            _iconImage.color = target;
    }
}
