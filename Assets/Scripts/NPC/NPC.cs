using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class NPC : MonoBehaviour, IInteractable
{
    public NPCDialogue dialogueData;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;

    private int dialogueIndex;
    private bool isTyping, isDialogueActive;

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        // Find pausemenu så man kan referere til isPaused
        PauseMenu pauseMenu = Object.FindAnyObjectByType<PauseMenu>();
        // if no dialogue data or the game is paused and no dialogue is active
        if (dialogueData == null || (pauseMenu.isPaused && !isDialogueActive))
        return;

        if(isDialogueActive)
        {
            NextLine();
        }
        else
        {
            StartDialogue();
        }

    }

    void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;

        nameText.SetText(dialogueData.npcName);

        dialoguePanel.SetActive(true);

        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.SetText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
        }

        else if (++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            // if another line, type next line
            StartCoroutine(TypeLine());
        }

        else
        {
            EndDialogue();
        }
    }
    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.SetText("");

        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(dialogueData.typingSpeed);  
        }

        isTyping = false;

        if(dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueText.SetText("");
        dialoguePanel.SetActive(false);
    }
}
