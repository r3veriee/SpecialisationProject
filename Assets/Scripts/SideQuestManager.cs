using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

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

    void Start()
    {
        if (questTimerText != null) questTimerText.gameObject.SetActive(false);
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
            else
            {
                UpdateQuestUI();
            }
        }
    }

    public void StartNPCQuest(float timeLimit, GameObject npc)
    {
        questTimeLeft = timeLimit;
        questActive = true;
        if (questTimerText != null) questTimerText.gameObject.SetActive(true);

        activeNPC = npc;
        if (activeNPC != null) activeNPC.SetActive(false);

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
        if (!questActive) return;
        questActive = false;
        if (questTimerText != null)
        {
            questTimerText.color = Color.green;
            questTimerText.text = "TRIAL COMPLETE";
        }
        activeNPC = null;
        collectedRings.Clear();
        StartCoroutine(HideQuestTextAfterDelay());
    }

    public void FailQuest()
    {
        if (!questActive) return;

        questActive = false;
        if (questTimerText != null)
        {
            questTimerText.color = panicColor;
            questTimerText.text = "TRIAL FAILED";
        }

        if (activeNPC != null) activeNPC.SetActive(true);

        foreach (GameObject ring in collectedRings)
        {
            if (ring != null) ring.SetActive(true);
        }

        collectedRings.Clear();
    }

    private IEnumerator HideQuestTextAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        if (questTimerText != null) questTimerText.gameObject.SetActive(false);
    }

    void UpdateQuestUI()
    {
            if (questTimerText == null) return;
            questTimerText.color = questTimeLeft <= 5f ? panicColor : normalColor;

            int minutes = Mathf.FloorToInt(questTimeLeft / 60f);
            int seconds = Mathf.FloorToInt(questTimeLeft % 60f);
            float milliseconds = (questTimeLeft * 1000f) % 1000f;
            questTimerText.text = string.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, milliseconds);
    }
}