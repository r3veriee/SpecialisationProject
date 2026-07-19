using UnityEngine;
using TMPro;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Quest Settings")]
    public int totalShardsRequired = 3;
    private int currentShards = 0;

    [Header("UI & Portals")]
    public TextMeshProUGUI questText;
    public GameObject spirePortal;
    public GameObject finalTerminalDoor;

    [Header("Level Geometry Swapping")]
    [Tooltip("Drag the 3 Trial paths here so we can REVEAL them!")]
    public GameObject[] trialPathways;

    [Tooltip("Drag the bridge/track they used to get here so we can HIDE it!")]
    public GameObject[] approachPathways;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Start the game with no quest text and the Spire Portal OFF
        if (questText != null) questText.text = "REACH THE TOWER.";
        if (spirePortal != null) spirePortal.SetActive(false);

        // Hide the trials so the player only focuses on the tower
        foreach (GameObject trial in trialPathways)
        {
            if (trial != null) trial.SetActive(false);
        }
    }

    public void RevealTrials()
    {
        if (questText != null)
        {
            questText.text = "TOWER LOCKED. GATHER PRISM SHARDS: 0 / 3";
            questText.color = Color.red;
        }

        foreach (GameObject trial in trialPathways)
        {
            if (trial != null) trial.SetActive(true);
        }

        foreach (GameObject approach in approachPathways)
        {
            if (approach != null) approach.SetActive(false);
        }
    }

    public void CollectShard()
    {
        currentShards++;

        if (questText != null)
        {
            questText.text = $"PRISM SHARDS: {currentShards} / {totalShardsRequired}";
            questText.color = Color.white;
        }

        if (currentShards >= totalShardsRequired) CompleteQuest();
    }

    void CompleteQuest()
    {
        if (questText != null)
        {
            questText.text = "TOWER UNLOCKED.";
            questText.color = Color.green;
        }

        // Turn on the portal
        if (spirePortal != null) spirePortal.SetActive(true);
        if (finalTerminalDoor != null) finalTerminalDoor.SetActive(false);
    }
}