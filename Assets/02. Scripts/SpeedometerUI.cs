using UnityEngine;
using TMPro;
using UnityEngine.UI; // 이미지 컴포넌트 제어용 추가

public class SpeedometerUI : MonoBehaviour
{
    public TextMeshProUGUI speedText;

    [Tooltip("올려주신 SpeedUI4 이미지를 배치할 Image 컴포넌트입니다. (선택사항)")]
    public Image speedometerBG;

    private ArcadeCarController targetCar;
    public float maxSpeed = 300f; // 기준이 되는 최고 속도

    [Header("기획서 7단계 색상 설정")]
    [Tooltip("인스펙터 그라데이션에서 보라색부터 빨간색까지 7개의 색상 포인트를 지정하세요.")]
    public Gradient speedColors;

    public void SetupUI(ArcadeCarController playerCar)
    {
        targetCar = playerCar;
        maxSpeed = playerCar.maxSpeedKmh; // 차량에서 직접 최고 속도 값을 가져옴
    }

    void Update()
    {
        if (targetCar == null) return;

        // 현재 속도 계산 (m/s -> km/h)
        float currentSpeed = targetCar.GetComponent<Rigidbody>().velocity.magnitude * 3.6f;
        UpdateSpeedometer(currentSpeed);
    }

    // 현재 속도를 이 함수로 계속 넘겨주면 됩니다.
    public void UpdateSpeedometer(float currentSpeed)
    {
        // 1. 텍스트 업데이트 
        // [수정] SpeedUI4 이미지 내부에 km/h가 이미 그려져 있으므로, 
        // 여기서는 숫자만 3자리 정수 형태("D3", 예: 009, 125)로 채워주는 것이 연출상 가장 깔끔합니다.
        speedText.text = Mathf.FloorToInt(currentSpeed).ToString("D3");

        // 2. 색상 변경 로직
        float speedRatio = Mathf.Clamp01(currentSpeed / maxSpeed);
        Color targetColor;

        if (speedRatio >= 0.98f) // 98% 이상 (최댓값 근접 및 부스터 상태): 기존의 진한 무지개색 유지
        {
            // Mathf.Repeat와 Time.time을 이용해 무지개색(HSV)이 계속 춤추게 만듭니다.
            targetColor = Color.HSVToRGB(Mathf.Repeat(Time.time * 2f, 1f), 1f, 1f);
        }
        else
        {
            // [요구사항 5.1.5.1] 0% ~ 98% 구간은 인스펙터에서 설정한 7단계 그라데이션 색상을 따라갑니다.
            targetColor = speedColors.Evaluate(speedRatio);
        }

        // 3. UI에 색상 적용
        speedText.color = targetColor;

        // [추가] 만약 숫자뿐만 아니라 SpeedUI4 이미지의 네온 테두리(Glow) 색상도 
        // 속도에 맞춰 보라색->빨간색->무지개색으로 같이 인스펙션하게 하려면 아래 코드를 사용합니다.
        if (speedometerBG != null)
        {
            speedometerBG.color = targetColor;
        }
    }
}