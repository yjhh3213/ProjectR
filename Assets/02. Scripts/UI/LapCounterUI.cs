using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LapCounterUI : MonoBehaviour
{
    [Header("UI 연결")]
    public Image segment1; // 오른쪽 반원 (Lap 1)
    public Image segment2; // 왼쪽 반원 (Lap 2)
    //public TextMeshProUGUI lapText; // "LAP 1 / 2" 텍스트

    [Header("색상 설정")]
    public Color offColor = new Color(0.2f, 0.2f, 0.2f, 0.5f); // 꺼진 색상 (반투명 회색)
    public Color onColor = Color.cyan; // 달리고 있을 때 색상 (예: 시안 블루)
    public Color finishColor = Color.yellow; // 완주했을 때 터지는 노란색

    void Start()
    {
        // 게임 시작 시 1랩 상태로 셋업
        UpdateLap(1);
    }

    // 외부(RaceManager 등)에서 플레이어가 체크포인트를 지나 랩이 오를 때마다 이 함수를 호출합니다.
    public void UpdateLap(int currentLap)
    {
        if (currentLap == 1)
        {
            segment1.color = onColor;   // 1분할 빛남
            segment2.color = offColor;  // 나머지 꺼짐
            //lapText.text = "LAP 1 / 2";
        }
        else if (currentLap == 2)
        {
            segment1.color = onColor;   // 1분할 유지
            segment2.color = onColor;   // 2분할도 빛남
            //lapText.text = "LAP 2 / 2";
        }
        /*else if (currentLap > 2)
        {
            // 완주 연출: 전체가 노란색으로 변함
            segment1.color = finishColor;
            segment2.color = finishColor;
            lapText.text = "FINISH!";
            lapText.color = finishColor; // 글씨도 노란색으로 통일하면 더 멋집니다!
        }*/
    }
}