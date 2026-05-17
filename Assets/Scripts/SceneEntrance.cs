using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneEntrance : MonoBehaviour, IInteractable
{
    [Header("Scene Transition")]
    [SerializeField] private string sceneName = "Temple Map";
    [Tooltip("Unik ID for denne dør i den nuværende scene. Skal matche targetEntranceId fra den dør spilleren kom ind af.")]
    [SerializeField] private string entranceId = "main";
    [Tooltip("ID på den dør spilleren skal spawne ved i målscenen.")]
    [SerializeField] private string targetEntranceId = "main";
    [Tooltip("Hvor spilleren placeres når de spawner ved denne dør. Bruger transform hvis tom.")]
    [SerializeField] private Transform spawnPoint;

    [Header("Locking")]
    [Tooltip("Unik ID for låsen — bruges til at huske at døren er låst op permanent.")]
    [SerializeField] private string doorId = "";
    [Tooltip("Item der kræves for at gå ind. Lad stå tomt hvis ingen nøgle kræves.")]
    [SerializeField] private ItemData requiredItem;
    [Tooltip("Slot index hvor nøglen skal sidde (-1 = tjek alle slots)")]
    [SerializeField] private int requiredSlotIndex = -1;
    [Tooltip("Skal nøglen forbruges når den bruges?")]
    [SerializeField] private bool consumeKey = true;
    [Tooltip("NPC dialogue der vises når spilleren ikke har nøglen")]
    [SerializeField] private NPC lockedDialogueNPC;

    [Header("UI")]
    [SerializeField] private GameObject interactIndicator;

    public string EntranceId => entranceId;
    public Transform SpawnPoint => spawnPoint != null ? spawnPoint : transform;

    public bool CanInteract() => true;

    public void Interact()
    {
        if (requiredItem != null && !IsUnlocked() && !HasRequiredItem())
        {
            lockedDialogueNPC?.Interact();
            return;
        }

        if (requiredItem != null && !IsUnlocked())
        {
            if (!string.IsNullOrEmpty(doorId) && GameStateManager.Instance != null)
                GameStateManager.Instance.UnlockedDoors.Add(doorId);

            if (consumeKey && InventoryManager.Instance != null)
                InventoryManager.Instance.RemoveItem(requiredItem, 1);
        }

        PlayerHealth ph = FindAnyObjectByType<PlayerHealth>();
        if (ph != null) PlayerHealth.SavedHealth = ph.health;

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.PendingEntranceId = targetEntranceId;

        SceneManager.LoadScene(sceneName);
    }

    private bool IsUnlocked()
    {
        if (string.IsNullOrEmpty(doorId)) return false;
        return GameStateManager.Instance != null && GameStateManager.Instance.UnlockedDoors.Contains(doorId);
    }

    private bool HasRequiredItem()
    {
        InventoryManager inv = InventoryManager.Instance;
        if (inv == null) return false;

        if (requiredSlotIndex >= 0)
            return inv.GetSlot(requiredSlotIndex) == requiredItem;

        for (int i = 0; i < inv.SlotCount; i++)
            if (inv.GetSlot(i) == requiredItem) return true;

        return false;
    }

    public void AdvanceDialogue() { }

    public void OnPlayerEnter()
    {
        if (interactIndicator != null) interactIndicator.SetActive(true);
    }

    public void OnPlayerExit()
    {
        if (interactIndicator != null) interactIndicator.SetActive(false);
    }
}
