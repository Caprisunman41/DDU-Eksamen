using UnityEngine;

[CreateAssetMenu(fileName ="NewNPCDialogue", menuName ="NPC Dialogue")]
public class NPCDialogue : ScriptableObject
{
    public string npcName;
    public Sprite npcPortrait;
    public string[] dialogueLines;
    public bool[] autoProgressLines;
    public bool[] endDialogueLines;
    public float autoProgressDelay = 1.5f;
    public float typingSpeed = 0.05f;

    public DialogueChoice[] dialogueChoices;

    [Tooltip("Start dialogue here after player has paid. -1 = always start from 0")]
    public int paidDialogueStartIndex = -1;
}

[System.Serializable]
public class DialogueChoice
{
    public int dialogueIndex;
    public string[] choices;
    public int[] nextDialogueIndexes;
    public int[] goldCosts; // Gold deducted when choosing this option (0 = free)
    public int[] insufficientGoldIndexes; // Dialogue index to jump to if player can't afford (parallel to goldCosts)
}
    
