using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyUIManager : MonoBehaviour
{
    [Header("로비 3D 차량 모델 (Index 순서대로 0, 1, 2, 3 넣기)")]
    public GameObject[] showroomCars; // 알파로메오, 머스탱, 카마로, GTR 순서

    [Header("UI 패널 연결")]
    public GameObject flagButton;
    public GameObject selectionPanel;
    public GameObject startEngineButton;

    private bool isModeSelected = true; // 아케이드는 고정
    private bool isMatchSelected = false;
    private bool isCarSelected = false;

    void Start()
    {
        flagButton.SetActive(true);
        selectionPanel.SetActive(false);
        startEngineButton.SetActive(false);

        // 처음에 모든 차를 숨기고, 0번 차(알파 로메오)만 보여주기
        UpdateShowroomCar(0);
    }

    public void OnFlagClicked()
    {
        flagButton.SetActive(false);
        selectionPanel.SetActive(true);
    }

    public void SelectMatchType(int matchType)
    {
        PlayerPrefs.SetInt("MatchType", matchType);
        isMatchSelected = true;
        CheckReadyToStart();
    }

    // 차량 버튼을 누를 때마다 호출됨 (버튼 Inspector에서 0, 1, 2, 3 할당)
    public void SelectCar(int carIndex)
    {
        PlayerPrefs.SetInt("SelectedCarIndex", carIndex);
        isCarSelected = true;

        // 🌟 배경의 3D 자동차 모델 교체!
        UpdateShowroomCar(carIndex);

        CheckReadyToStart();
    }

    // 선택한 번호의 차만 켜고 나머지는 끄는 함수
    private void UpdateShowroomCar(int indexToShow)
    {
        for (int i = 0; i < showroomCars.Length; i++)
        {
            if (i == indexToShow)
                showroomCars[i].SetActive(true); // 선택된 차 켜기
            else
                showroomCars[i].SetActive(false); // 나머지 끄기
        }
    }

    private void CheckReadyToStart()
    {
        if (isModeSelected && isMatchSelected && isCarSelected)
        {
            startEngineButton.SetActive(true); // TODO: Animator Trriger로 스르륵 나타나게 하면 더 좋습니다.
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("MainRacingScene");
    }
}