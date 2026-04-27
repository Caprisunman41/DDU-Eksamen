using UnityEngine;

public class ManaManager : MonoBehaviour
{
    public static ManaManager Instance { get; private set; }

    public int MaxMana = 2;
    public int CurrentMana { get; private set; }
    public int KillsTowardNextMana { get; private set; }
    public const int KillsPerMana = 4;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        CurrentMana = MaxMana;
    }

    public void ResetMana()
    {
        CurrentMana = MaxMana;
        KillsTowardNextMana = 0;
    }

    public bool TryUseMana()
    {
        if (CurrentMana <= 0) return false;
        CurrentMana--;
        return true;
    }

    public void OnEnemyKilled()
    {
        if (CurrentMana >= MaxMana)
        {
            Debug.Log("Mana allerede fuld");
            return;
        }

        KillsTowardNextMana++;
        Debug.Log($"Kill registreret: {KillsTowardNextMana}/{KillsPerMana} mod næste mana. Nuværende mana: {CurrentMana}");

        if (KillsTowardNextMana >= KillsPerMana)
        {
            KillsTowardNextMana = 0;
            CurrentMana = Mathf.Min(CurrentMana + 1, MaxMana);
            Debug.Log($"Mana øget til: {CurrentMana}");
        }
    }
}
