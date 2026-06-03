using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class LapTracker : MonoBehaviour
{
    [Header("UI 설정 (코드가 자동으로 찾으므로 비워두셔도 됩니다)")]
    public Image lapImage;
    public TextMeshProUGUI lapText;

    [Header("레이스 설정")]
    public int totalLaps = 3;
    private int completedLaps = 0;
    private bool isFinished = false;

    private float lastTriggerTime = 0f; // 미세한 떨림으로 인한 중복 카운트 방지용 쿨타임
    private static List<string> finishOrder = new List<string>();

    void Start()
    {
        // StartGridManager가 차를 생성하고 이름을 바꾸는 타이밍을 안전하게 대기합니다.
        StartCoroutine(InitUIWithDelay());
    }

    IEnumerator InitUIWithDelay()
    {
        yield return null;

        // 내가 플레이어 차량("PlayerCar")일 때만 씬에서 UI를 찾아 연결합니다.
        if (gameObject.name == "PlayerCar")
        {
            FindAndSetupUI();
        }
    }

    void FindAndSetupUI()
    {
        if (lapImage == null)
        {
            GameObject imgObj = GameObject.Find("LapImage");
            if (imgObj != null) lapImage = imgObj.GetComponent<Image>();
        }

        if (lapText == null)
        {
            GameObject textObj = GameObject.Find("LapText");
            if (textObj != null) lapText = textObj.GetComponent<TextMeshProUGUI>();
        }

        // UI 에셋 초기 세팅
        if (lapImage != null)
        {
            lapImage.type = Image.Type.Filled;
            lapImage.fillMethod = Image.FillMethod.Radial360;
            lapImage.fillAmount = 0f;
            lapImage.color = Color.white;
        }

        if (lapText != null)
        {
            lapText.text = $"Lap: 0 / {totalLaps}";
        }
    }

    // ★ [핵심 추가] 피니시 라인(Trigger)을 통과하는 순간을 물리적으로 감지합니다.
    private void OnTriggerEnter(Collider other)
    {
        // 3초 이내에 연속으로 감지되는 중복 버그 방지
        if (Time.time - lastTriggerTime < 3f) return;

        // 부딪힌 오브젝트의 이름이 "FinishLine" 이거나 태그가 "FinishLine" 일 때 작동
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
        UpdateVisuals();

        if (completedLaps >= totalLaps)
        {
            CompleteRace();
        }
    }

    void UpdateVisuals()
    {
        if (gameObject.name != "PlayerCar") return;

        if (lapImage == null || lapText == null) FindAndSetupUI();

        if (lapText != null)
        {
            lapText.text = $"Lap: {completedLaps} / {totalLaps}";
        }

        if (lapImage != null)
        {
            // [기획 의도 완벽 반영 수학 공식]
            // 1바퀴 완료 = 1 / 2 = 0.5 (절반 채워짐)
            // 2바퀴 완료 = 2 / 2 = 1.0 (꽉 채워짐)
            lapImage.fillAmount = (float)completedLaps / 2f;
        }
    }

    void CompleteRace()
    {
        isFinished = true;
        finishOrder.Add(gameObject.name);

        int rank = finishOrder.Count;
        Debug.Log($"{gameObject.name} 완주! 순위: {rank}");

        if (gameObject.name == "PlayerCar")
        {
            if (lapText != null)
            {
                lapText.text = $"<color=yellow>FINISHED!</color>\n<size=80%>{rank}위</size>";
            }
            StartCoroutine(RainbowRoutine());
        }
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

    public static void ResetRankings()
    {
        finishOrder.Clear();
    }
}