using UnityEngine;

[CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "NPC Dialogue")]
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
    public int paidDialogueStartIndex = -1;
    public DialogueSceneRoute[] sceneRoutes;
}

[System.Serializable]
public class DialogueChoice
{
    public int dialogueIndex;
    public string[] choices;
    public int[] nextDialogueIndexes;
    public int[] goldCosts;
    public int[] insufficientGoldIndexes;
}

[System.Serializable]
public class DialogueSceneRoute
{
    public int dialogueIndex;
    public string sceneName;
}