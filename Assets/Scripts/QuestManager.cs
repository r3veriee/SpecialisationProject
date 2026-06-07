//using UnityEngine;
//using TMPro; // Use this if you are using TextMeshPro for UI

//public class QuestManager : MonoBehaviour
//{
//    // Singleton pattern so the Shards can easily find this script
//    public static QuestManager Instance;

//    [Header("Quest Settings")]
//    public int totalShardsRequired = 3;
//    private int currentShards = 0;

//    [Header("UI References")]
//    public TextMeshProUGUI questText; // Drag your UI Text here

//    [Header("Level Unlocks")]
//    public GameObject finalTerminalDoor; // The object that blocks the end level

//    void Awake()
//    {
//        if (Instance == null) Instance = this;
//        else Destroy(gameObject);
//    }

//    void Start()
//    {
//        UpdateQuestUI();
//    }

//    public void CollectShard()
//    {
//        currentShards++;
//        UpdateQuestUI();

//        if (currentShards >= totalShardsRequired)
//        {
//            CompleteQuest();
//        }
//    }

//    void UpdateQuestUI()
//    {
//        if (questText != null)
//        {
//            questText.text = $"PRISM SHARDS: {currentShards} / {totalShardsRequired}";
//        }
//    }

//    void CompleteQuest()
//    {
//        if (questText != null)
//        {
//            questText.text = "SYSTEM POWERED. ACCESS THE TERMINAL.";
//            questText.color = Color.green;
//        }

//        // Unlock the door or activate the final terminal!
//        if (finalTerminalDoor != null)
//        {
//            finalTerminalDoor.SetActive(false);
//        }
//    }
//}

using UnityEngine;
using TMPro;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public int totalShardsRequired = 3;
    private int currentShards = 0;

    public TextMeshProUGUI questText;
    public GameObject finalTerminalDoor;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start() => UpdateQuestUI();

    public void CollectShard()
    {
        currentShards++;
        UpdateQuestUI();
        if (currentShards >= totalShardsRequired) CompleteQuest();
    }

    void UpdateQuestUI()
    {
        if (questText != null) questText.text = $"PRISM SHARDS: {currentShards} / {totalShardsRequired}";
    }

    void CompleteQuest()
    {
        if (questText != null)
        {
            questText.text = "SYSTEM POWERED. ACCESS THE TERMINAL.";
            questText.color = Color.green;
        }
        if (finalTerminalDoor != null) finalTerminalDoor.SetActive(false);
    }
}