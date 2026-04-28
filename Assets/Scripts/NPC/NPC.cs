using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class NPC : MonoBehaviour, IInteractable
{
    public NPCDialogue dialogueData;
    private DialogueController dialogueUI;

    private int dialogueIndex;
    private bool isTyping, isDialogueActive;
    private bool isChoosingOption;
    private bool _hasPaid;

    private CharacterController _playerController;
    private PlayerAttack _playerAttack;
    private Rigidbody2D _playerRb;

    private void Start()
    {
        dialogueUI = DialogueController.Instance;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            _playerController = player.GetComponent<CharacterController>();
            _playerAttack = player.GetComponent<PlayerAttack>();
            _playerRb = player.GetComponent<Rigidbody2D>();
        }
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

    private void Update()
    {
        if (!isDialogueActive || !isChoosingOption) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame) SelectChoice(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SelectChoice(1);
    }

    private void SelectChoice(int index)
    {
        foreach (DialogueChoice choice in dialogueData.dialogueChoices)
        {
            if (choice.dialogueIndex == dialogueIndex && index < choice.choices.Length)
            {
                int cost = (choice.goldCosts != null && index < choice.goldCosts.Length) ? choice.goldCosts[index] : 0;
                if (cost > 0 && InventoryManager.Instance != null)
                {
                    if (!InventoryManager.Instance.TrySpendGold(cost))
                    {
                        int fallback = (choice.insufficientGoldIndexes != null && index < choice.insufficientGoldIndexes.Length)
                            ? choice.insufficientGoldIndexes[index] : -1;
                        if (fallback >= 0) ChooseOption(fallback);
                        return;
                    }
                    _hasPaid = true;
                }
                ChooseOption(choice.nextDialogueIndexes[index]);
                return;
            }
        }
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
        dialogueIndex = (_hasPaid && dialogueData.paidDialogueStartIndex >= 0)
            ? dialogueData.paidDialogueStartIndex
            : 0;

        if (_playerController != null) _playerController.enabled = false;
        if (_playerAttack != null) _playerAttack.enabled = false;
        if (_playerRb != null) _playerRb.linearVelocity = Vector2.zero;

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
            return;
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
            int cost = (choice.goldCosts != null && i < choice.goldCosts.Length) ? choice.goldCosts[i] : 0;
            int fallback = (choice.insufficientGoldIndexes != null && i < choice.insufficientGoldIndexes.Length) ? choice.insufficientGoldIndexes[i] : -1;
            string label = cost > 0 ? $"{choice.choices[i]} ({cost} guld)" : choice.choices[i];
            dialogueUI.CreateChoiceButton(label, () =>
            {
                if (cost > 0 && InventoryManager.Instance != null)
                {
                    if (!InventoryManager.Instance.TrySpendGold(cost))
                    {
                        if (fallback >= 0) ChooseOption(fallback);
                        return;
                    }
                    _hasPaid = true;
                }
                ChooseOption(nextIndex);
            });
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

        if (_playerController != null) _playerController.enabled = true;
        if (_playerAttack != null) _playerAttack.enabled = true;

        dialogueUI.SetDialogueText("");
        dialogueUI.showDialogueUI(false);
    }
}
