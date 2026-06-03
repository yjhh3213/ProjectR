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
        public Image carIcon;
        public TextMeshProUGUI timeText;
    }

    [Header("순위별 UI 슬롯 (1st ~ 4th 순서대로 넣어주세요)")]
    public List<ResultSlotUI> rankSlots = new List<ResultSlotUI>();

    [Header("차량 종류별 스프라이트 에셋 등록 (0:알파 1:머스탱 2:카마로 3:GTR)")]
    public List<Sprite> carSprites = new List<Sprite>();
    public List<string> carNames = new List<string>();

    private Dictionary<string, Sprite> carSpriteDict = new Dictionary<string, Sprite>();

    void Start()
    {
        // 1. AI 차량 스프라이트 매칭용 딕셔너리 구축
        for (int i = 0; i < carSprites.Count && i < carNames.Count; i++)
        {
            carSpriteDict[carNames[i]] = carSprites[i];
        }

        // 2. 전달받은 데이터 기반 UI 출력
        DisplayResults();

        // 3. 1분(60초) 후 LobbyScene 복귀 타이머 가동
        StartCoroutine(ReturnToLobbyRoutine(60f));
    }

    void DisplayResults()
    {
        var results = RaceResultData.savedResults;

        // AI 이름 매칭 실패 시 사용할 예비 인덱스
        int defaultSpriteIndex = 1;

        for (int i = 0; i < rankSlots.Count; i++)
        {
            if (i < results.Count)
            {
                rankSlots[i].timeText.text = results[i].finalTime;
                string vehicleName = results[i].carName;

                // ★ 1. 플레이어 차량인 경우: 로비에서 선택한 차량 인덱스를 가져와서 아이콘 반영
                if (vehicleName == "PlayerCar")
                {
                    int playerSelectedCar = PlayerPrefs.GetInt("SelectedCarIndex", 0);
                    if (playerSelectedCar < carSprites.Count)
                    {
                        rankSlots[i].carIcon.sprite = carSprites[playerSelectedCar];
                    }
                    else if (carSprites.Count > 0)
                    {
                        rankSlots[i].carIcon.sprite = carSprites[0];
                    }
                    rankSlots[i].carIcon.gameObject.SetActive(true);
                }
                // ★ 2. AI 차량인 경우: 딕셔너리에서 이름으로 검색
                else if (carSpriteDict.TryGetValue(vehicleName, out Sprite matchedSprite))
                {
                    rankSlots[i].carIcon.sprite = matchedSprite;
                    rankSlots[i].carIcon.gameObject.SetActive(true);
                }
                // ★ 3. 매칭되는 이름이 아예 없는 경우: 남은 이미지를 겹치지 않게 순서대로 배분
                else
                {
                    if (carSprites.Count > 0)
                    {
                        int spriteToUse = defaultSpriteIndex % carSprites.Count;
                        rankSlots[i].carIcon.sprite = carSprites[spriteToUse];
                        defaultSpriteIndex++;
                    }
                    rankSlots[i].carIcon.gameObject.SetActive(true);
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
        RaceResultData.savedResults.Clear();
        SceneManager.LoadScene("LobbyScene");
    }
}