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
        ManaManager mm = ManaManager.Instance;
        if (mm == null || _manaOrbs == null || _manaOrbs.Length == 0) return;

        int current = mm.CurrentMana;
        int max = mm.MaxMana;
        float killProgress = (float)mm.KillsTowardNextMana / ManaManager.KillsPerMana;

        for (int i = 0; i < _manaOrbs.Length; i++)
        {
            Image orb = _manaOrbs[i];
            float targetFill;
            Color targetColor;
            if (i < current) { targetFill = 1f; targetColor = _activeColor; }
            else if (i == current && current < max) { targetFill = killProgress; targetColor = _activeColor; }
            else { targetFill = 0f; targetColor = _emptyColor; }

            if (!Mathf.Approximately(orb.fillAmount, targetFill)) orb.fillAmount = targetFill;
            if (orb.color != targetColor) orb.color = targetColor;
        }
    }
}
