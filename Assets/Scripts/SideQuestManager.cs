using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class SideQuestManager : MonoBehaviour
{
    public static SideQuestManager Instance;

    public TextMeshProUGUI questTimerText;
    public Color normalColor = Color.cyan;
    public Color panicColor = Color.red;

    private float questTimeLeft = 0f;
    private bool questActive = false;

    private GameObject activeNPC;
    private List<GameObject> collectedRings = new List<GameObject>();

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
                FindObjectOfType<CheckpointSystem>().Respawn();
            }
            UpdateQuestUI();
        }
    }

    public void StartNPCQuest(float timeLimit, GameObject npc)
    {
        questTimeLeft = timeLimit;
        questActive = true;
        questTimerText.gameObject.SetActive(true);

        activeNPC = npc;
        activeNPC.SetActive(false);

        collectedRings.Clear();
    }

    public void AddQuestTime(float bonusTime, GameObject ring)
    {
        if (questActive)
        {
            questTimeLeft += bonusTime;

            collectedRings.Add(ring);
            ring.SetActive(false);
        }
    }

    public void CompleteQuest()
    {
        questActive = false;
        if (questTimerText != null)
        {
            questTimerText.color = Color.green;
            questTimerText.text = "SIDE QUEST COMPLETE";
        }

        activeNPC = null;
        collectedRings.Clear();
    }

    public void FailQuest()
    {
        if (!questActive) return;

        questActive = false;
        if (questTimerText != null)
        {
            questTimerText.color = panicColor;
            questTimerText.text = "QUEST FAILED";
        }

        if (activeNPC != null) activeNPC.SetActive(true);

        foreach (GameObject ring in collectedRings)
        {
            if (ring != null) ring.SetActive(true);
        }
        collectedRings.Clear();
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