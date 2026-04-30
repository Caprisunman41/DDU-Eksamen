using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneEntrance : MonoBehaviour, IInteractable
{
    [SerializeField] private string sceneName = "Temple Map";
    [SerializeField] private GameObject interactIndicator;

    public bool CanInteract() => true;

    public void Interact()
    {
        SceneManager.LoadScene(sceneName);
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
