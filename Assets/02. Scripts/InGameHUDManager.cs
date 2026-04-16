using UnityEngine;
using TMPro;

public class InGameHUDManager : MonoBehaviour
{
    [Header("UI 연결")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI rankText;

    private float elapsedTime = 0f;
    private bool isRacing = false;
    private int currentRank = 1;
    private int totalPlayers = 4;

    void Start()
    {
        // 게임 시작 시 타이머 작동 (나중에 카운트다운 완료 후 실행되게 변경 가능)
        isRacing = true;
    }

    void Update()
    {
        if (isRacing)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    // 5.1.4: 시간 표시 로직 (분:초:밀리초)
    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        int milliseconds = Mathf.FloorToInt((elapsedTime * 100f) % 100f);

        // 00:00.00 포맷으로 변환
        timeText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }

    // 5.1.2: 등수 업데이트 (외부 RaceManager에서 호출)
    public void UpdateRank(int newRank)
    {
        currentRank = newRank;
        // 카트라이더 스타일: "1 / 4" 형태로 표시
        rankText.text = currentRank.ToString() + " <size=50%>/ " + totalPlayers.ToString() + "</size>";
    }
}