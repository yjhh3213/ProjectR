using UnityEngine;
using Cinemachine;

public class DollyIntroController : MonoBehaviour
{
    [Header("시네머신 카메라 설정")]
    public CinemachineVirtualCamera introCamera;
    public CinemachineDollyCart dollyCart;

    [Header("끝났을 때 켜줄 매니저 (유니티 에디터에서 미리 꺼두세요!)")]
    public GameObject carSpawnerManager;

    [Header("레이싱 시작 시 활성화할 대상")]
    public GameObject racingUI;

    private MonoBehaviour originalCameraFollow; // 기존 메인 카메라의 움직임 스크립트

    private bool isIntroFinished = false;

    void Start()
    {
        // 1. 레이싱 UI는 처음엔 꺼둡니다.
        if (racingUI != null) racingUI.SetActive(false);

        // 2. 메인 카메라에 붙어있는 기존 'CameraFollow' 혹은 'Camera Follow' 스크립트를 강제로 찾아냅니다.
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            // 클래스 이름이 'CameraFollow'인지 인스펙터 창 이름과 완전히 일치해야 합니다.
            // 만약 클래스명이 다를 경우를 대비하여 컴포넌트로 안전하게 검색합니다.
            originalCameraFollow = mainCam.GetComponent("CameraFollow") as MonoBehaviour;
            if (originalCameraFollow == null)
            {
                originalCameraFollow = mainCam.GetComponent("Camera Follow") as MonoBehaviour;
            }

            // 소개 카메라가 도는 동안은 기존 스크립트가 카메라를 강제로 붙잡지 못하도록 꺼줍니다.
            if (originalCameraFollow != null)
            {
                originalCameraFollow.enabled = false;
                Debug.Log("기존 카메라 스크립트 일시 정지 성공!");
            }
            else
            {
                Debug.LogWarning("메인 카메라에서 CameraFollow 스크립트를 찾지 못했습니다. 스크립트 이름을 꼭 확인해주세요.");
            }
        }

        // 3. 소개 카메라의 우선순위를 최상위로 높여 화면을 독점합니다.
        if (introCamera != null)
        {
            introCamera.gameObject.SetActive(true); // 혹시 꺼져있다면 켜줍니다.
            introCamera.Priority = 99;
        }

        // 4. 카트 시작 위치 초기화
        if (dollyCart != null)
        {
            dollyCart.m_Position = 0f;
        }
    }

    void Update()
    {
        if (isIntroFinished) return;

        // 5. 기차가 끝까지 다 달렸는지 검사합니다.
        if (dollyCart != null && dollyCart.m_Path != null)
        {
            float currentPos = dollyCart.m_Position;
            float maxPathLength = dollyCart.m_Path.PathLength;

            // 경로 끝에 거의 도달했을 때 (끝에서 2단위 거리 이하로 남았을 때)
            if (currentPos >= maxPathLength - 2f)
            {
                StartRacingGame();
            }
        }
    }

    void StartRacingGame()
    {
        isIntroFinished = true;

        // 6. ★ 가장 확실한 방법 ★ 소개 카메라 오브젝트 자체를 완전히 꺼버립니다!
        // 이렇게 하면 시네머신 브레인이 화면 제어권을 완전히 상실하여 메인 카메라 본래의 앵글로 강제 복귀합니다.
        if (introCamera != null)
        {
            introCamera.gameObject.SetActive(false);
        }

        // 7. 대기하고 있던 스포너를 켜서 자동차들을 정상 소환합니다.
        if (carSpawnerManager != null)
        {
            carSpawnerManager.SetActive(true);
        }

        // 8. 꺼두었던 기존 메인카메라 추적 스크립트를 다시 활성화합니다.
        // 이제 소환된 자동차 뒤를 카메라가 예전처럼 똑같이 부드럽게 쫓아가게 됩니다.
        if (originalCameraFollow != null)
        {
            originalCameraFollow.enabled = true;
            Debug.Log("기존 차량 추적 카메라 복구 완료!");
        }

        // 9. 레이싱 UI도 켭니다.
        if (racingUI != null)
        {
            racingUI.SetActive(true);
        }

        Debug.Log("?? 인트로 종료! 레이싱을 시작합니다!");
    }
}