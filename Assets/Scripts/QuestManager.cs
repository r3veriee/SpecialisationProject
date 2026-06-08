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