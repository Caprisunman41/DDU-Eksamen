using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneEntrance : MonoBehaviour, IInteractable
{
    [Header("Scene Transition")]
    [SerializeField] private string sceneName = "Temple Map";
    [SerializeField] private string entranceId = "main";
    [SerializeField] private string targetEntranceId = "main";
    [SerializeField] private Transform spawnPoint;

    [Header("Locking")]
    [SerializeField] private string doorId = "";
    [SerializeField] private ItemData requiredItem;
    [SerializeField] private int requiredSlotIndex = -1;
    [SerializeField] private bool consumeKey = true;
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

        string sceneToLoad = sceneName;
        if (GameStateManager.Instance != null && !string.IsNullOrEmpty(GameStateManager.Instance.PendingNextScene))
        {
            sceneToLoad = GameStateManager.Instance.PendingNextScene;
            GameStateManager.Instance.PendingNextScene = "";
            GameStateManager.Instance.Save();
        }

        SceneManager.LoadScene(sceneToLoad);
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