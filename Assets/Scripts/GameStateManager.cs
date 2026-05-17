using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class GameStateData
{
    public List<string> unlockedDoors = new List<string>();
    public List<string> collectedPickups = new List<string>();
    public List<string> paidNpcs = new List<string>();
    public List<string> talkedNpcs = new List<string>();
    public List<string> npcDialogueChoiceKeys = new List<string>();
    public List<int> npcDialogueChoiceValues = new List<int>();

    public string lastCheckpointScene;
    public Vector3 lastCheckpointPos;
    public bool hasCheckpoint;

    public int gold;
    public List<string> slotItemNames = new List<string>();
    public List<int> slotStackCounts = new List<int>();

    public bool hasDash;
    public bool hasWallJump;
    public bool hasSpell;
    public bool hasBulletBlock;
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public HashSet<string> UnlockedDoors = new HashSet<string>();
    public HashSet<string> CollectedPickups = new HashSet<string>();
    public HashSet<string> PaidNpcs = new HashSet<string>();
    public HashSet<string> TalkedNpcs = new HashSet<string>();
    public Dictionary<string, int> NpcDialogueChoices = new Dictionary<string, int>();

    public string LastCheckpointScene;
    public Vector3 LastCheckpointPos;
    public bool HasCheckpoint;

    public string PendingEntranceId;
    public string PendingNextScene;
    public bool PendingCheckpointRespawn;

    private GameStateData _pendingInventoryData;
    private ItemData[] _itemCache;

    private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;
        GameObject go = new GameObject("GameStateManager");
        Instance = go.AddComponent<GameStateManager>();
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_pendingInventoryData != null && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RestoreFromSave(_pendingInventoryData);
            _pendingInventoryData = null;
        }

        GameObject player = GameObject.FindWithTag("Player");

        if (PendingCheckpointRespawn)
        {
            if (player != null && HasCheckpoint) player.transform.position = LastCheckpointPos;
            PendingCheckpointRespawn = false;
            PendingEntranceId = null;
            return;
        }

        if (!string.IsNullOrEmpty(PendingEntranceId))
        {
            SceneEntrance[] entrances = Object.FindObjectsByType<SceneEntrance>(FindObjectsSortMode.None);
            foreach (SceneEntrance e in entrances)
            {
                if (e.EntranceId == PendingEntranceId)
                {
                    if (player != null) player.transform.position = e.SpawnPoint.position;
                    break;
                }
            }
            PendingEntranceId = null;
        }
    }

    public ItemData LookupItem(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return null;
        if (_itemCache == null) _itemCache = Resources.LoadAll<ItemData>("");
        foreach (ItemData it in _itemCache)
            if (it != null && it.itemName == itemName) return it;
        return null;
    }

    public void Save()
    {
        GameStateData data = new GameStateData
        {
            unlockedDoors = new List<string>(UnlockedDoors),
            collectedPickups = new List<string>(CollectedPickups),
            paidNpcs = new List<string>(PaidNpcs),
            talkedNpcs = new List<string>(TalkedNpcs),
            lastCheckpointScene = LastCheckpointScene,
            lastCheckpointPos = LastCheckpointPos,
            hasCheckpoint = HasCheckpoint
        };

        foreach (var kvp in NpcDialogueChoices)
        {
            data.npcDialogueChoiceKeys.Add(kvp.Key);
            data.npcDialogueChoiceValues.Add(kvp.Value);
        }

        InventoryManager inv = InventoryManager.Instance;
        if (inv != null)
        {
            data.gold = inv.Gold;
            for (int i = 0; i < inv.SlotCount; i++)
            {
                ItemData it = inv.GetSlot(i);
                data.slotItemNames.Add(it != null ? it.itemName : "");
                data.slotStackCounts.Add(inv.GetStackCount(i));
            }
            data.hasDash = inv.HasDash;
            data.hasWallJump = inv.HasWallJump;
            data.hasSpell = inv.HasSpell;
            data.hasBulletBlock = inv.HasBulletBlock;
        }

        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
    }

    public bool HasSaveFile() => File.Exists(SavePath);

    public void ClearSave()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
        UnlockedDoors.Clear();
        CollectedPickups.Clear();
        PaidNpcs.Clear();
        TalkedNpcs.Clear();
        NpcDialogueChoices.Clear();
        PendingNextScene = "";
        LastCheckpointScene = null;
        HasCheckpoint = false;
    }

    public void ContinueFromSave()
    {
        if (!HasSaveFile()) return;
        GameStateData data = JsonUtility.FromJson<GameStateData>(File.ReadAllText(SavePath));
        if (data == null) return;

        UnlockedDoors = new HashSet<string>(data.unlockedDoors);
        CollectedPickups = new HashSet<string>(data.collectedPickups);
        PaidNpcs = new HashSet<string>(data.paidNpcs);
        TalkedNpcs = new HashSet<string>(data.talkedNpcs);
        LastCheckpointScene = data.lastCheckpointScene;
        LastCheckpointPos = data.lastCheckpointPos;
        HasCheckpoint = data.hasCheckpoint;

        NpcDialogueChoices.Clear();
        for (int i = 0; i < data.npcDialogueChoiceKeys.Count; i++)
            NpcDialogueChoices[data.npcDialogueChoiceKeys[i]] = data.npcDialogueChoiceValues[i];

        _pendingInventoryData = data;
        PendingCheckpointRespawn = HasCheckpoint;

        string targetScene = !string.IsNullOrEmpty(LastCheckpointScene) ? LastCheckpointScene : SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(targetScene);
    }
}