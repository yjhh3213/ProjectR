using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class ResultManager : MonoBehaviour
{
    [System.Serializable]
    public struct ResultSlotUI
    {
        public Image carIcon;             // Hierarchy의 'Icon' 매핑용
        public TextMeshProUGUI timeText;  // Hierarchy의 'Time' 매핑용
    }

    [Header("순위별 UI 슬롯 (1st ~ 4th 순서대로 넣어주세요)")]
    public List<ResultSlotUI> rankSlots = new List<ResultSlotUI>();

    [Header("차량 종류별 스프라이트 에셋 등록")]
    public List<Sprite> carSprites = new List<Sprite>();
    public List<string> carNames = new List<string>();

    private Dictionary<string, Sprite> carSpriteDict = new Dictionary<string, Sprite>();

    void Start()
    {
        // 1. 차량 스프라이트 조회용 딕셔너리 구축
        for (int i = 0; i < carSprites.Count && i < carNames.Count; i++)
        {
            carSpriteDict[carNames[i]] = carSprites[i];
        }

        // 2. 전달받은 데이터 기반 UI 출력
        DisplayResults();

        // 3. 1분(60초) 후 LobbyScene 복귀 타이머 가동
        StartCoroutine(ReturnToLobbyRoutine(3f));
    }

    void DisplayResults()
    {
        var results = RaceResultData.savedResults;

        for (int i = 0; i < rankSlots.Count; i++)
        {
            if (i < results.Count)
            {
                // 시간 표시 업데이트
                rankSlots[i].timeText.text = results[i].finalTime;

                // 차량 이름에 매칭되는 아이콘 띄우기
                string vehicleName = results[i].carName;
                if (carSpriteDict.TryGetValue(vehicleName, out Sprite matchedSprite))
                {
                    rankSlots[i].carIcon.sprite = matchedSprite;
                    rankSlots[i].carIcon.gameObject.SetActive(true);
                }
                else
                {
                    // 예외 처리: 매칭 데이터가 없으면 첫 번째 스프라이트 기본 적용
                    if (carSprites.Count > 0) rankSlots[i].carIcon.sprite = carSprites[0];
                }
            }
            else
            {
                // 참가자가 4명 미만일 경우 빈 슬롯 처리
                rankSlots[i].timeText.text = "--'--''---";
                rankSlots[i].carIcon.gameObject.SetActive(false);
            }
        }
    }

    IEnumerator ReturnToLobbyRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 데이터 초기화 후 로비 씬 로드
        RaceResultData.savedResults.Clear();
        SceneManager.LoadScene("LobbyScene");
    }
}