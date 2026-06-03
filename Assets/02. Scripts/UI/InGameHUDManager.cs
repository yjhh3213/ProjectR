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
    private int totalPlayers = 4; // 기본값, Start 시점에 자동으로 갱신됨

    void Start()
    {
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

    // 시간 표시 로직 (0'00''000 포맷 반영)
    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        int milliseconds = Mathf.FloorToInt((elapsedTime * 1000f) % 1000f); // 3자리 밀리초

        timeText.text = string.Format("{0:0}'{1:00}''{2:000}", minutes, seconds, milliseconds);
    }

    // [추가] 참가자 명단 수를 기반으로 총 인원수를 동기화하는 함수
    public void SetupTotalPlayers(int count)
    {
        totalPlayers = count;
    }

    // 등수 업데이트 (RankManager에서 호출됨)
    public void UpdateRank(int newRank)
    {
        currentRank = newRank;
        rankText.text = currentRank.ToString() + " <size=50%>/ " + totalPlayers.ToString() + "</size>";
    }
}