public interface IInteractable
{
    void Interact();
    bool CanInteract();
    void AdvanceDialogue();
    void OnPlayerEnter();
    void OnPlayerExit();
}