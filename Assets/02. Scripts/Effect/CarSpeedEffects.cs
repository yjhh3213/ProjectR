using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ArcadeCarController))]
public class CarSpeedEffects : MonoBehaviour
{
    [Header("Player Check")]
    [Tooltip("체크하면 오직 태그가 'Player'인 차량만 이 효과를 작동시킵니다.")]
    public bool requirePlayerTag = true;

    [Header("References (자동 연동)")]
    public Camera mainCamera;
    public Volume postProcessVolume;
    private ArcadeCarController carController;

    [Header("Speed Settings")]
    public bool useControllerMaxSpeed = true;
    public float manualMaxSpeedKmh = 250f;

    [Header("1. Dynamic FOV Settings")]
    public float minFOV = 60f;
    public float maxFOV = 85f;
    public float fovLerpSpeed = 5f;

    [Header("2. Motion Blur Settings")]
    public float minBlurIntensity = 0.1f;
    public float maxBlurIntensity = 0.8f;

    [Header("3. Chromatic Aberration Settings")]
    public float minChromatic = 0.0f;
    public float maxChromatic = 0.7f;

    [Header("4. Vignette Settings (터널 시야)")]
    public float minVignette = 0.1f;
    public float maxVignette = 0.4f;

    [Header("5. Jitter Prevention (떨림 방지 세팅)")]
    public float speedSmoothSpeed = 4f;

    private Rigidbody rb;
    private MotionBlur motionBlur;
    private ChromaticAberration chromaticAberration;
    private Vignette vignette;

    private float smoothedSpeedFactor = 0f;
    private bool isPlayerCar = true;

    // LobbyUIManager에 저장되어있는 인덱스 값 불러오기 (멤버 변수)
    private int playerCarIndex;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        carController = GetComponent<ArcadeCarController>();

        // ★ 수정 포인트: 앞에 'int'를 제거하여 클래스 상단의 멤버 변수에 값을 할당합니다.
        playerCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);

        // 만약 플레이어 태그 검사가 켜져 있는데, 이 차가 Player 태그가 아니라면? -> AI 차량이므로 스크립트를 꺼버립니다!
        if (requirePlayerTag && !CompareTag("Player"))
        {
            isPlayerCar = false;
            enabled = false; // Update 함수를 아예 실행하지 않음 (카메라 충돌 원천 차단)
            return;
        }

        if (mainCamera == null) mainCamera = Camera.main;
        if (postProcessVolume == null) postProcessVolume = FindAnyObjectByType<Volume>();

        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGet(out motionBlur);
            postProcessVolume.profile.TryGet(out chromaticAberration);
            postProcessVolume.profile.TryGet(out vignette);
        }
    }

    void Update()
    {
        if (!isPlayerCar || rb == null || mainCamera == null) return;

        // 1. 차량별 최고속도 가져오기 (플레이어 인덱스 switch문 활용)
        float targetMaxSpeed = manualMaxSpeedKmh;
        if (useControllerMaxSpeed)
        {
            switch (playerCarIndex)
            {
                case 0: // 알파 로메오 8C 콤페티치오네
                    targetMaxSpeed = 292f;
                    break;
                case 1: // 포드 머스탱 마하 1
                    targetMaxSpeed = 210f;
                    break;
                case 2: // 쉐보레 카마로
                    targetMaxSpeed = 250f;
                    break;
                case 3: // 닛산 GTR
                    targetMaxSpeed = 311f;
                    break;
                default:
                    targetMaxSpeed = 250f;
                    break;
            }
        }
        if (targetMaxSpeed <= 0) targetMaxSpeed = 250f;

        // 2. 현재 속도 계산 (km/h)
        float currentSpeedKmh = rb.velocity.magnitude * 3.6f;

        // ★ [핵심] 정지 상태 떨림 방지: 속도가 1km/h 미만이면 아예 0으로 강제 고정
        if (currentSpeedKmh < 1.0f)
        {
            currentSpeedKmh = 0f;
        }

        // 3. 비율 정규화 및 클램프
        float rawSpeedFactor = Mathf.InverseLerp(0f, targetMaxSpeed, currentSpeedKmh);
        rawSpeedFactor = Mathf.Clamp01(rawSpeedFactor);

        // 4. 부드러운 스무딩
        smoothedSpeedFactor = Mathf.Lerp(smoothedSpeedFactor, rawSpeedFactor, Time.deltaTime * speedSmoothSpeed);

        // 5. FOV 및 포스트 프로세싱 적용
        float targetFOV = Mathf.Lerp(minFOV, maxFOV, smoothedSpeedFactor);
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * fovLerpSpeed);

        if (motionBlur != null)
            motionBlur.intensity.value = Mathf.Lerp(minBlurIntensity, maxBlurIntensity, smoothedSpeedFactor);

        if (chromatisAberrationCheck(chromaticAberration, smoothedSpeedFactor)) { }

        if (vignette != null)
            vignette.intensity.value = Mathf.Lerp(minVignette, maxVignette, smoothedSpeedFactor);
    }

    // 코드 가독성을 위한 메서드 분기
    private bool chromatisAberrationCheck(ChromaticAberration ca, float factor)
    {
        if (ca != null)
        {
            ca.intensity.value = Mathf.Lerp(minChromatic, maxChromatic, factor);
        }
        return true;
    }
}