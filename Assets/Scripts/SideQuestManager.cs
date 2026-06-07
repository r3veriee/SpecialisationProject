//using UnityEngine;
//using TMPro;

//public class SideQuestManager : MonoBehaviour
//{
//    public static SideQuestManager Instance;

//    [Header("UI Reference")]
//    public TextMeshProUGUI questTimerText; // A separate UI text element for quests

//    [Header("Colors")]
//    public Color normalColor = Color.cyan;
//    public Color panicColor = Color.red;

//    private float questTimeLeft = 0f;
//    private bool questActive = false;

//    void Awake()
//    {
//        if (Instance == null) Instance = this;
//        else Destroy(gameObject);
//    }

//    void Update()
//    {
//        if (questActive)
//        {
//            questTimeLeft -= Time.deltaTime;

//            if (questTimeLeft <= 0)
//            {
//                questTimeLeft = 0;
//                FailQuest();
//            }

//            UpdateQuestUI();
//        }
//    }

//    public void StartNPCQuest(float timeLimit)
//    {
//        questTimeLeft = timeLimit;
//        questActive = true;
//        questTimerText.gameObject.SetActive(true);
//    }

//    public void AddQuestTime(float bonusTime)
//    {
//        if (questActive) questTimeLeft += bonusTime;
//    }

//    public void CompleteQuest()
//    {
//        questActive = false;
//        questTimerText.color = Color.green;
//        questTimerText.text = "SIDE QUEST COMPLETE";
//    }

//    void FailQuest()
//    {
//        questActive = false;
//        questTimerText.color = panicColor;
//        questTimerText.text = "QUEST FAILED";

//        // Snap them back to the NPC or checkpoint
//        FindObjectOfType<CheckpointSystem>().Respawn();
//    }

//    void UpdateQuestUI()
//    {
//        if (questTimerText == null) return;

//        questTimerText.color = questTimeLeft <= 5f ? panicColor : normalColor;

//        int seconds = Mathf.FloorToInt(questTimeLeft);
//        float milliseconds = (questTimeLeft % 1f) * 1000f;
//        questTimerText.text = string.Format("QUEST: {0:00}.{1:000}", seconds, milliseconds);
//    }
//}


using UnityEngine;
using TMPro;

public class SideQuestManager : MonoBehaviour
{
    public static SideQuestManager Instance;

    public TextMeshProUGUI questTimerText;
    public Color normalColor = Color.cyan;
    public Color panicColor = Color.red;

    private float questTimeLeft = 0f;
    private bool questActive = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (questActive)
        {
            questTimeLeft -= Time.deltaTime;
            if (questTimeLeft <= 0)
            {
                questTimeLeft = 0;
                FailQuest();
            }
            UpdateQuestUI();
        }
    }

    public void StartNPCQuest(float timeLimit)
    {
        questTimeLeft = timeLimit;
        questActive = true;
        questTimerText.gameObject.SetActive(true);
    }

    public void AddQuestTime(float bonusTime)
    {
        if (questActive) questTimeLeft += bonusTime;
    }

    public void CompleteQuest()
    {
        questActive = false;
        questTimerText.color = Color.green;
        questTimerText.text = "SIDE QUEST COMPLETE";
    }

    void FailQuest()
    {
        questActive = false;
        questTimerText.color = panicColor;
        questTimerText.text = "QUEST FAILED";
        FindObjectOfType<CheckpointSystem>().Respawn();
    }

    void UpdateQuestUI()
    {
        if (questTimerText == null) return;
        questTimerText.color = questTimeLeft <= 5f ? panicColor : normalColor;
        int seconds = Mathf.FloorToInt(questTimeLeft);
        float milliseconds = (questTimeLeft % 1f) * 1000f;
        questTimerText.text = string.Format("QUEST: {0:00}.{1:000}", seconds, milliseconds);
    }
}