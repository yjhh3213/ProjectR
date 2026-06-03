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

    [Header("종료 확인 UI 패널")]
    public GameObject quitPanel; // [추가됨] 종료 확인 패널 연결용

    [Header("버튼 색상 설정")]
    public Color normalColor = new Color(1f, 1f, 1f, 0f);       // 평소 (투명)
    public Color selectedColor = new Color(1f, 0.16f, 0.16f, 1f); // 선택됨 (예: 레이싱 레드)

    // 각 섹션의 현재 선택된 버튼을 기억할 변수
    private Image currentMatchBtnImage;
    private Image currentCarBtnImage;

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
        // 1. 현재 버튼 찾기 (이 함수를 호출한 버튼의 Image 컴포넌트를 넘겨받아야 함)
        // 아래 '유니티 세팅' 설명 참고
        PlayerPrefs.SetInt("MatchType", matchType);
        isMatchSelected = true;
        CheckReadyToStart();
    }

    // 가독성을 위해 추천하는 방식: 버튼 이미지를 직접 제어하는 함수
    public void SetMatchButtonVisual(Image clickedImage)
    {
        // 기존에 선택됐던 버튼이 있다면 평소 색으로 되돌림
        if (currentMatchBtnImage != null) currentMatchBtnImage.color = normalColor;

        // 새로 클릭한 버튼을 선택 색상으로 변경
        currentMatchBtnImage = clickedImage;
        currentMatchBtnImage.color = selectedColor;

        isMatchSelected = true;
        CheckReadyToStart();
    }

    public void SetCarButtonVisual(Image clickedImage)
    {
        if (currentCarBtnImage != null) currentCarBtnImage.color = normalColor;

        currentCarBtnImage = clickedImage;
        currentCarBtnImage.color = selectedColor;

        isCarSelected = true;
        CheckReadyToStart();
    }

    // 차량 버튼을 누를 때마다 호출됨 (버튼 Inspector에서 0, 1, 2, 3 할당)
    public void SelectCar(int carIndex)
    {
        PlayerPrefs.SetInt("SelectedCarIndex", carIndex);
        isCarSelected = true;
        // 배경의 3D 자동차 모델 교체!
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

    // [추가됨] SelectionPanel 닫기 (X 버튼용)
    public void CloseSelectionPanel()
    {
        selectionPanel.SetActive(false);
        flagButton.SetActive(true); // 다시 화면의 깃발 버튼을 표시

        // (선택 사항) 닫을 때 선택 상태를 초기화하고 싶다면 아래 주석을 해제하세요.
        
        isModeSelected = true;
        isMatchSelected = false;
        isCarSelected = false;
        startEngineButton.SetActive(false);
        if (currentMatchBtnImage != null) currentMatchBtnImage.color = normalColor;
        if (currentCarBtnImage != null) currentCarBtnImage.color = normalColor;
        
    }

    // [추가됨] 종료 확인 패널 열기 (로비 화면의 '종료' 버튼에 연결)
    public void OpenQuitPanel()
    {
        quitPanel.SetActive(true);
    }

    // [추가됨] 종료 확인 패널 닫기 (되돌아가기 버튼용)
    public void CloseQuitPanel()
    {
        quitPanel.SetActive(false);
    }

    // [추가됨] 게임 완전히 종료하기 (종료 버튼용)
    public void QuitGame()
    {
        Debug.Log("게임을 종료합니다.");

        // 유니티 에디터 환경에서도 플레이 모드가 꺼지도록 처리
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 실제 빌드된 게임에서 작동
#endif
    }
}