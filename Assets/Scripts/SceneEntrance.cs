using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneEntrance : MonoBehaviour, IInteractable
{
    [SerializeField] private string sceneName = "Temple Map";
    [SerializeField] private GameObject interactIndicator;
    [Tooltip("Hvilket item der kræves for at gå ind. Lad stå tomt hvis ingen nøgle kræves.")]
    [SerializeField] private ItemData requiredItem;
    [Tooltip("Slot index hvor nøglen skal sidde (-1 = tjek alle slots)")]
    [SerializeField] private int requiredSlotIndex = -1;
    [Tooltip("NPC dialogue der vises når spilleren ikke har nøglen")]
    [SerializeField] private NPC lockedDialogueNPC;

    public bool CanInteract() => true;

    public void Interact()
    {
        if (requiredItem != null && !HasRequiredItem())
        {
            lockedDialogueNPC?.Interact();
            return;
        }

        PlayerHealth ph = FindAnyObjectByType<PlayerHealth>();
        if (ph != null) PlayerHealth.SavedHealth = ph.health;

        SceneManager.LoadScene(sceneName);
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
