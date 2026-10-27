using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class TARESULT : MonoBehaviour
{
    [Header("타임어택 결과 UI 연결")]
    public Image playerCarIcon;         // 플레이어가 탄 차량 아이콘 이미지
    public TextMeshProUGUI finalTimeText;   // 측정된 최종 기록 텍스트 (예: "01:23.45")

    [Header("차량 종류별 스프라이트 에셋 (로비 인덱스와 일치: 0, 1, 2, 3...)")]
    public List<Sprite> carSprites = new List<Sprite>();

    [Header("로비 복귀 대기 시간")]
    public float returnDelay = 5.0f; // 결과 확인 후 몇 초 뒤에 로비로 갈지 설정

    void Start()
    {
        DisplayTimeAttackResult();

        // 일정 시간 후 로비(LobbyScene)로 복귀하는 타이머 가동
        StartCoroutine(ReturnToLobbyRoutine(returnDelay));
    }

    void DisplayTimeAttackResult()
    {
        // 1. 로비에서 선택한 차량 인덱스에 맞는 스프라이트(아이콘) 출력
        int playerSelectedCar = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        if (playerCarIcon != null && carSprites.Count > 0)
        {
            if (playerSelectedCar < carSprites.Count && carSprites[playerSelectedCar] != null)
            {
                playerCarIcon.sprite = carSprites[playerSelectedCar];
            }
            else
            {
                playerCarIcon.sprite = carSprites[0]; // 예비 기본 이미지
            }
            playerCarIcon.gameObject.SetActive(true);
        }

        // 2. 타임어택에서 측정된 최종 기록 불러오기
        // (TimeAttackManager에서 PlayerPrefs나 static 변수로 저장한 값을 가져옵니다)
        float finalTime = PlayerPrefs.GetFloat("LastRaceTime", 0f);

        if (finalTimeText != null)
        {
            // 분과 초 형식으로 예쁘게 변환 (예: 01:25.30)
            int minutes = Mathf.FloorToInt(finalTime / 60f);
            int seconds = Mathf.FloorToInt(finalTime % 60f);
            int milliseconds = Mathf.FloorToInt((finalTime * 100f) % 100f);

            finalTimeText.text = $"{minutes:00}:{seconds:00}.{milliseconds:00}";
        }

        // 최고 기록(Best Time) 갱신 로직 예시
        float bestTime = PlayerPrefs.GetFloat("BestTime", 9999f);
        if (finalTime < bestTime && finalTime > 0f)
        {
            PlayerPrefs.SetFloat("BestTime", finalTime);
            PlayerPrefs.Save();
            Debug.Log("신기록 달성!");
        }
    }

    IEnumerator ReturnToLobbyRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("LobbyScene"); // 로비 씬 이름 (상황에 맞게 수정 가능)
    }
}