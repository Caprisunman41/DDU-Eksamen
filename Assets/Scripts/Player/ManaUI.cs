using UnityEngine;
using UnityEngine.UI;

public class ManaUI : MonoBehaviour
{
    [SerializeField] private Image[] _manaOrbs;
    [SerializeField] private GameObject[] _manaBackgrounds;
    [SerializeField] private Image _killProgressFill;

    private Color _activeColor = Color.white;
    private Color _emptyColor = new Color(0.25f, 0.25f, 0.25f, 1f);

    private void Start()
    {
        int max = ManaManager.Instance.MaxMana;
        for (int i = 0; i < _manaOrbs.Length; i++)
            _manaOrbs[i].gameObject.SetActive(i < max);

        if (_manaBackgrounds != null)
            for (int i = 0; i < _manaBackgrounds.Length; i++)
                _manaBackgrounds[i].SetActive(i < max);
    }

    private void Update()
    {
        if (ManaManager.Instance == null) { Debug.LogError("ManaManager mangler"); return; }
        if (_manaOrbs == null || _manaOrbs.Length == 0) { Debug.LogError("Mana Orbs ikke assigned i Inspector"); return; }

        int current = ManaManager.Instance.CurrentMana;
        float killProgress = (float)ManaManager.Instance.KillsTowardNextMana / ManaManager.KillsPerMana;

        for (int i = 0; i < _manaOrbs.Length; i++)
        {
            if (i < current)
            {
                _manaOrbs[i].fillAmount = 1f;
                _manaOrbs[i].color = _activeColor;
            }
            else if (i == current && current < ManaManager.Instance.MaxMana)
            {
                _manaOrbs[i].fillAmount = killProgress;
                _manaOrbs[i].color = _activeColor;
            }
            else
            {
                _manaOrbs[i].fillAmount = 0f;
                _manaOrbs[i].color = _emptyColor;
            }
        }
    }
}
