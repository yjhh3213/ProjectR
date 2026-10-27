using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LapCounterUI : MonoBehaviour
{
    [Header("UI 연결")]
    public List<Image> segment; // 이미지 컴포넌트 리스트
    public List<GameObject> Segment; // 활성화/비활성화할 UI 게임오브젝트 리스트

    [Header("색상 설정")]
    public Color offColor = new Color(0.2f, 0.2f, 0.2f, 0.5f); // 꺼진 색상 (반투명 회색)
    public Color onColor = Color.cyan; // 달리고 있을 때 색상
    public Color finishColor = Color.yellow; // 완주 색상

    private TimeAttackManager timeAttackManager;

    void Start()
    {
        // 씬에 있는 TimeAttackManager를 찾아 연결합니다.
        timeAttackManager = FindAnyObjectByType<TimeAttackManager>();

        if (timeAttackManager == null)
        {
            Debug.LogError("[LapCounterUI] 씬에서 TimeAttackManager를 찾을 수 없습니다!");
        }

        // 시작할 때 모든 세그먼트 비활성화
        if (Segment != null)
        {
            foreach (var seg in Segment)
            {
                if (seg != null) seg.SetActive(false);
            }
        }
    }

    void LateUpdate()
    {
        if (timeAttackManager != null)
        {
            // TimeAttackManager의 현재 섹터 인덱스를 가져와서 UI 갱신
            UpdateLap(timeAttackManager.currentSectorIndex);
        }
    }

    public void UpdateLap(int currentLap)
    {
        if (Segment == null || segment == null) return;

        // currentLap(통과한 섹터 수)에 맞춰 순차적으로 활성화
        for (int i = 0; i < Segment.Count; i++)
        {
            if (Segment[i] != null && i <= currentLap - 1)
            {
                Segment[i].SetActive(true); // 이제 정상적으로 켜집니다!

                if (i < segment.Count && segment[i] != null)
                {
                    segment[i].color = onColor;
                }
            }
        }
    }
}