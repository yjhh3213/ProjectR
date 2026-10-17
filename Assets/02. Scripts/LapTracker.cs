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

    private CarController myCarController; // AI 유무 판별을 위한 컴포넌트 변수 추가

    void Start()
    {
        myItemManager = GetComponent<ItemManager>();
        myCarController = GetComponent<CarController>();
        raceStartTime = Time.time;

        StartCoroutine(InitUIWithDelay());
    }

    IEnumerator InitUIWithDelay()
    {
        yield return null;

        // 태그 대신 오브젝트 이름 또는 CarController의 AI 여부로 플레이어 본인만 UI를 할당받도록 설정
        if (gameObject.name.Contains("PlayerCar") || (myCarController != null && !myCarController.isAI))
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

        // AI 차량들이 플레이어 UI를 건드리지 못하도록 차단
        if (gameObject.name.Contains("PlayerCar") || (myCarController != null && !myCarController.isAI))
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
        // 결승선 중복 트리거 방지
        if (Time.time - lastTriggerTime < 3f) return;

        // 대소문자나 언더바(_) 유무에 상관없이 결승선 오브젝트 및 태그 인식
        if (other.gameObject.name.ToLower().Contains("finishline") ||
            other.gameObject.name.ToLower().Contains("finish_line") ||
            other.CompareTag("FinishLine"))
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
        if (participant != null)
        {
            participant.currentLap = completedLaps + 1;
            participant.lastCheckpointIndex = -1;
        }

        // 플레이어 본인일 때만 UI 비주얼 갱신
        if (gameObject.name.Contains("PlayerCar") || (myCarController != null && !myCarController.isAI))
        {
            UpdateVisuals();

            // 2바퀴 완료 후 최종 3바퀴째(무지개 레이스 단계)에 돌입하는 순간 무지개 효과 발동
            if (completedLaps == 2)
            {
                if (rainbowCoroutine == null)
                {
                    rainbowCoroutine = StartCoroutine(RainbowRoutine());
                }
            }
        }

        // 설정된 totalLaps(3바퀴)를 완전히 다 채웠을 때만 최종 완주 처리
        if (completedLaps >= totalLaps)
        {
            CompleteRace();
        }
    }

    void UpdateVisuals()
    {
        if (lapImage == null) FindAndSetupSharedUI();

        if (lapImage != null)
        {
            // totalLaps가 3일 때 분모를 2로 만들어 1바퀴=0.5(반원), 2바퀴=1.0(큰원)이 되도록 유연하게 연산
            float denominator = Mathf.Max(1f, totalLaps - 1f);
            lapImage.fillAmount = (float)completedLaps / denominator;
        }
    }

    void CompleteRace()
    {
        isFinished = true;

        if (gameObject.name.Contains("PlayerCar") || (myCarController != null && !myCarController.isAI))
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