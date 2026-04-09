using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null; //Closest interactable 

    // Update is called once per frame
    void OnInteract(InputValue value)
    {
        interactableInRange?.Interact(); // E - only opens
    }

    void OnClick(InputValue value)
    {
        interactableInRange?.AdvanceDialogue(); // Click - only advances
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
