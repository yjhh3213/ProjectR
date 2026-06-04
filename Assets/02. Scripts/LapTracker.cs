using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LapTracker : MonoBehaviour
{
    [Header("UI 설정")]
    public Image lapImage;
    public TextMeshProUGUI rankText;

    [Header("레이스 설정")]
    public int totalLaps = 3;
    private int completedLaps = 0;
    private bool isFinished = false;

    private float lastTriggerTime = 0f;

    private ItemManager myItemManager;
    private float raceStartTime;
    private Coroutine rainbowCoroutine; // 무지개 코루틴 제어용

    void Start()
    {
        myItemManager = GetComponent<ItemManager>();
        raceStartTime = Time.time;

        StartCoroutine(InitUIWithDelay());
    }

    IEnumerator InitUIWithDelay()
    {
        yield return null;

        if (gameObject.name.Contains("PlayerCar"))
        {
            FindAndSetupSharedUI();
        }
    }

    void FindAndSetupSharedUI()
    {
        if (lapImage == null)
        {
            GameObject imgObj = GameObject.Find("LapImage");
            if (imgObj != null) lapImage = imgObj.GetComponent<Image>();
        }

        if (rankText == null)
        {
            GameObject textObj = GameObject.Find("RankText");
            if (textObj != null) rankText = textObj.GetComponent<TextMeshProUGUI>();
        }

        if (lapImage != null)
        {
            lapImage.type = Image.Type.Filled;
            lapImage.fillMethod = Image.FillMethod.Radial360;
            lapImage.fillAmount = 0f;
            lapImage.color = Color.white;
        }
    }

    void Update()
    {
        if (isFinished) return;

        if (gameObject.CompareTag("Player"))
        {
            UpdateSharedRankUI();
        }
    }

    void UpdateSharedRankUI()
    {
        if (rankText == null || myItemManager == null) return;

        int currentRank = myItemManager.currentRank;

        if (currentRank < 1) currentRank = 1;

        string rankSuffix = "th";
        if (currentRank == 1) rankSuffix = "st";
        else if (currentRank == 2) rankSuffix = "nd";
        else if (currentRank == 3) rankSuffix = "rd";

        rankText.text = $"{currentRank}{rankSuffix}";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - lastTriggerTime < 3f) return;

        if (other.gameObject.name == "FinishLine" || other.CompareTag("FinishLine"))
        {
            lastTriggerTime = Time.time;
            OnPassFinishLine();
        }
    }

    public void OnPassFinishLine()
    {
        if (isFinished) return;

        completedLaps++;

        RaceParticipant participant = GetComponent<RaceParticipant>();
        if (participant != null) participant.currentLap = completedLaps + 1;

        UpdateVisuals();

        // ★ [수정됨] 2바퀴를 완료하고 3바퀴째에 돌입하는 순간 무지개 발동
        if (completedLaps == 2 && gameObject.CompareTag("Player"))
        {
            if (rainbowCoroutine == null)
            {
                rainbowCoroutine = StartCoroutine(RainbowRoutine());
            }
        }

        if (completedLaps >= totalLaps)
        {
            CompleteRace();
        }
    }

    void UpdateVisuals()
    {
        if (!gameObject.CompareTag("Player")) return;

        if (lapImage == null) FindAndSetupSharedUI();

        if (lapImage != null)
        {
            // ★ [수정됨] 무조건 2로 나누어 1바퀴 통과 시 0.5(반원), 2바퀴 통과 시 1.0(원)을 만듭니다.
            lapImage.fillAmount = (float)completedLaps / 2f;
        }
    }

    void CompleteRace()
    {
        isFinished = true;

        if (gameObject.CompareTag("Player"))
        {
            if (rankText != null) rankText.text = $"<color=yellow>FINISHED!</color>";

            float playerTime = Time.time - raceStartTime;
            int playerRank = (myItemManager != null) ? myItemManager.currentRank : 1;

            RaceResultData.savedResults.Clear();

            LapTracker[] allCars = FindObjectsByType<LapTracker>(FindObjectsSortMode.None);

            List<string> aiCarNames = new List<string>();
            foreach (var car in allCars)
            {
                if (car != this)
                {
                    aiCarNames.Add(car.gameObject.name.Replace("(Clone)", "").Trim());
                }
            }

            while (aiCarNames.Count < 3)
            {
                aiCarNames.Add("AICar_" + aiCarNames.Count);
            }

            if (playerRank == 1)
            {
                AddResultSlot(gameObject.name, playerTime);
                AddResultSlot(aiCarNames[0], playerTime + 2f);
                AddResultSlot(aiCarNames[1], playerTime + 5f);
                AddResultSlot(aiCarNames[2], playerTime + 9f);
            }
            else
            {
                int aiIndex = 0;

                if (playerRank == 2) { AddResultSlot(gameObject.name, playerTime); }
                else
                {
                    float aiTime = playerTime - Random.Range(3f, 7f);
                    if (aiTime < 1f) aiTime = 1f;
                    AddResultSlot(aiCarNames[aiIndex++], aiTime);
                }

                if (playerRank == 3) { AddResultSlot(gameObject.name, playerTime); }
                else if (playerRank > 2) { AddResultSlot(aiCarNames[aiIndex++], playerTime - Random.Range(1f, 3f)); }
                else { AddResultSlot(aiCarNames[aiIndex++], playerTime + Random.Range(1f, 10f)); }

                if (playerRank == 4) { AddResultSlot(gameObject.name, playerTime); }
                else
                {
                    if (playerRank > 3) { AddResultSlot(aiCarNames[aiIndex++], playerTime - Random.Range(0.5f, 1.5f)); }
                    else { AddResultSlot(aiCarNames[aiIndex++], playerTime + Random.Range(1f, 10f)); }
                }

                if (RaceResultData.savedResults.Count < 4)
                {
                    AddResultSlot(aiCarNames[aiIndex++], playerTime + Random.Range(1f, 10f));
                }
            }

            SceneManager.LoadScene("ResultScene");
        }
    }

    void AddResultSlot(string carName, float totalTime)
    {
        int minutes = Mathf.FloorToInt(totalTime / 60f);
        int seconds = Mathf.FloorToInt(totalTime % 60f);
        int milliseconds = Mathf.FloorToInt((totalTime * 1000f) % 1000f);
        string formattedTime = string.Format("{0:00}'{1:00}''{2:000}", minutes, seconds, milliseconds);

        ParticipantResult result = new ParticipantResult();
        result.carName = carName.Replace("(Clone)", "").Trim();
        result.finalTime = formattedTime;

        RaceResultData.savedResults.Add(result);
    }

    IEnumerator RainbowRoutine()
    {
        while (true)
        {
            if (lapImage != null)
            {
                float hue = Mathf.Repeat(Time.time * 0.5f, 1f);
                lapImage.color = Color.HSVToRGB(hue, 1f, 1f);
            }
            yield return null;
        }
    }
}