using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class NPC : MonoBehaviour, IInteractable
{
    public NPCDialogue dialogueData;
    private DialogueController dialogueUI;

    private int dialogueIndex;
    private bool isTyping, isDialogueActive;
    private bool isChoosingOption;

    private void Start()
    {
      dialogueUI = DialogueController.Instance;
    }

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        PauseMenu pauseMenu = Object.FindAnyObjectByType<PauseMenu>();
        if (dialogueData == null || (pauseMenu.isPaused && !isDialogueActive))
            return;

        if (!isDialogueActive)
            StartDialogue();
    }

    private float advanceCooldown;

    public void AdvanceDialogue()
    {
        PauseMenu pauseMenu = Object.FindAnyObjectByType<PauseMenu>();
        if (dialogueData == null || pauseMenu.isPaused || !isDialogueActive || isChoosingOption || Time.time < advanceCooldown)
            return;

        NextLine();
    }

    void ChooseOption(int nextIndex)
    {
        advanceCooldown = Time.time + 0.5f;
        dialogueIndex = nextIndex;
        dialogueUI.ClearChoices();
        DisplayCurrentLine();
    }

  
    void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;

        
        dialogueUI.SetNPCInfo(dialogueData.npcName);
        dialogueUI.showDialogueUI(true); 

        
        DisplayCurrentLine();
    }

    void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueUI.SetDialogueText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
        }

        //clear choices
        dialogueUI.ClearChoices();
        //check end dialogue lines
        if(dialogueData.endDialogueLines.Length > dialogueIndex && dialogueData.endDialogueLines[dialogueIndex])
        {
            EndDialogue();
            return;
        }

        // check if choices, and display
        foreach(DialogueChoice dialogueChoice in dialogueData.dialogueChoices)
        {
            if(dialogueChoice.dialogueIndex == dialogueIndex)
            {
                //display choices
                DisplayChoices(dialogueChoice);
                return;
            }
        }


        if(++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            // if another line, type next line
            DisplayCurrentLine();
        }

        else
        {
            EndDialogue();
        }
    }
    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueUI.SetDialogueText("");

        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueUI.SetDialogueText(dialogueUI.dialogueText.text += letter);
           yield return new WaitForSeconds(dialogueData.typingSpeed);  
        }

        isTyping = false;

        if(dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    void DisplayChoices(DialogueChoice choice)
    {
        isChoosingOption = true;
        for (int i = 0; i < choice.choices.Length; i++)
        {
            int nextIndex = choice.nextDialogueIndexes[i];
            dialogueUI.CreateChoiceButton(choice.choices[i], () => ChooseOption(nextIndex));
        }
    }

    

    void DisplayCurrentLine()
    {
        isChoosingOption = false;
        StopAllCoroutines();
        StartCoroutine(TypeLine());
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueUI.SetDialogueText("");
        dialogueUI.showDialogueUI(false);
    }
}
