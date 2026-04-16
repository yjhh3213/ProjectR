using UnityEngine;
using TMPro; // TextMeshPro 사용 필수

public class SpeedometerUI : MonoBehaviour
{
    public TextMeshProUGUI speedText;
    private ArcadeCarController targetCar;
    public float maxSpeed = 300f; // 기준이 되는 최고 속도

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
        // 1. 텍스트 업데이트 (소수점 버림)
        speedText.text = Mathf.FloorToInt(currentSpeed).ToString() + " km/h";

        // 2. 색상 변경 로직
        float speedRatio = currentSpeed / maxSpeed;

        if (speedRatio < 0.8f) // 0% ~ 80%: 어두운 보라색에서 점점 밝은 빨간색으로
        {
            // Lerp를 사용하여 보라색(Purple)에서 빨간색(Red)으로 자연스럽게 전환
            speedText.color = Color.Lerp(new Color(0.5f, 0f, 0.5f, 1f), Color.red, speedRatio / 0.8f);
        }
        else if (speedRatio < 0.98f) // 80% ~ 98%: 강렬한 빨간색 유지
        {
            speedText.color = Color.red;
        }
        else // 98% 이상 (최댓값 근접 및 부스터 상태): 진한 무지개색
        {
            // Mathf.PingPong과 Time.time을 이용해 무지개색(HSV)이 계속 춤추게 만듭니다.
            speedText.color = Color.HSVToRGB(Mathf.Repeat(Time.time * 2f, 1f), 1f, 1f);
        }
    }
}