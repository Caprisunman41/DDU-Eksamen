using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null; //Closest interactable 

    // Update is called once per frame
    void OnInteract(InputValue value)
    {
        interactableInRange?.Interact();
        interactableInRange?.AdvanceDialogue(); // E advances dialogue if already active
    }

    void OnClick(InputValue value)
    {
        if (value.isPressed)
            interactableInRange?.AdvanceDialogue();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
        {
            interactableInRange = interactable;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange)
        {
            interactableInRange = null;
        }
    }
    

}
