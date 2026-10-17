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
    public GameObject quitPanel; // 종료 확인 패널 연결용

    [Header("버튼 색상 설정")]
    public Color normalColor = new Color(1f, 1f, 1f, 0f);       // 평소 (투명)
    public Color selectedColor = new Color(1f, 0.16f, 0.16f, 1f); // 선택됨 (예: 레이싱 레드)

    // ★ [핵심 추가] 매치 타입별로 이동할 씬(Scene) 이름 설정
    [Header("이동할 씬(Scene) 이름 설정")]
    public string aiModeSceneName = "MainRacingScene"; // AI 모드 씬 이름 (인스펙터에서 수정 가능)
    public string taModeSceneName = "MainRacingTimeAttackScene"; // 타임어택 모드 씬 이름 (인스펙터에서 수정 가능)

    // 각 섹션의 현재 선택된 버튼을 기억할 변수
    private Image currentMatchBtnImage;
    private Image currentCarBtnImage;
    private Image currentInputBTnImage;

    private bool isInputSelected = true; // 아케이드는 고정
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

    public void SelectInputType(int inputType)
    {
        // inputType == 0 : 키보드 (자동 변속)
        // inputType == 1 : 레이싱 휠 (수동 변속)
        PlayerPrefs.SetInt("InputType", inputType);

        isInputSelected = true;
        CheckReadyToStart();
    }

    public void SetInputButtonVisual(Image clickedImage)
    {
        if (currentInputBTnImage != null) currentInputBTnImage.color = normalColor;

        currentInputBTnImage = clickedImage;
        currentInputBTnImage.color = selectedColor;

        isInputSelected = true;
        CheckReadyToStart();
    }

    public void SelectMatchType(int matchType)
    {
        // matchType == 0 : AI전
        // matchType == 1 : TA (타임어택)
        PlayerPrefs.SetInt("MatchType", matchType);
        isMatchSelected = true;
        CheckReadyToStart();
    }

    public void SetMatchButtonVisual(Image clickedImage)
    {
        if (currentMatchBtnImage != null) currentMatchBtnImage.color = normalColor;

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

    public void SelectCar(int carIndex)
    {
        PlayerPrefs.SetInt("SelectedCarIndex", carIndex);
        isCarSelected = true;

        UpdateShowroomCar(carIndex);
        CheckReadyToStart();
    }

    private void UpdateShowroomCar(int indexToShow)
    {
        for (int i = 0; i < showroomCars.Length; i++)
        {
            if (i == indexToShow)
                showroomCars[i].SetActive(true);
            else
                showroomCars[i].SetActive(false);
        }
    }

    private void CheckReadyToStart()
    {
        if (isInputSelected && isMatchSelected && isCarSelected)
        {
            startEngineButton.SetActive(true);
        }
    }

    // ★ [핵심 수정] 매치 타입(0 또는 1)을 불러와 해당하는 씬으로 전환합니다.
    public void StartGame()
    {
        // 저장된 매치 타입을 불러옵니다. (기본값은 0)
        int matchType = PlayerPrefs.GetInt("MatchType", 0);

        if (matchType == 0) // AI 모드
        {
            Debug.Log("AI 모드로 진입합니다: " + aiModeSceneName);
            SceneManager.LoadScene(aiModeSceneName);
        }
        else if (matchType == 1) // TA (타임어택) 모드
        {
            Debug.Log("TA 모드로 진입합니다: " + taModeSceneName);
            SceneManager.LoadScene(taModeSceneName);
        }
    }

    public void CloseSelectionPanel()
    {
        selectionPanel.SetActive(false);
        flagButton.SetActive(true);

        // 닫을 때 선택 상태 초기화
        isInputSelected = false;
        isMatchSelected = false;
        isCarSelected = false;
        startEngineButton.SetActive(false);

        if (currentMatchBtnImage != null) currentMatchBtnImage.color = normalColor;
        if (currentCarBtnImage != null) currentCarBtnImage.color = normalColor;
        if (currentInputBTnImage != null) currentInputBTnImage.color = normalColor;
    }

    public void OpenQuitPanel()
    {
        quitPanel.SetActive(true);
    }

    public void CloseQuitPanel()
    {
        quitPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("게임을 종료합니다.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}