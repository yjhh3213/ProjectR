using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private ItemManager itemManager;
    private ArcadeCarController arcadeCar;

    [Header("제어 하드웨어 설정")]
    [Tooltip("체크하면 레이싱 휠이 작동합니다. 체크 여부와 상관없이 키보드(W,A,S,D)는 항상 작동합니다.")]
    public bool useRacingWheel = true;

    void Start()
    {
        itemManager = GetComponent<ItemManager>();
        arcadeCar = GetComponent<ArcadeCarController>();
    }

    void Update()
    {
        if (arcadeCar == null) return;

        // 1. 조향 (좌/우 방향)
        // 유니티의 "Horizontal"은 기본적으로 키보드 A/D(또는 좌우 화살표)와 조이스틱 X축(핸들)을 동시에 인식합니다.
        float horizontal = Input.GetAxis("Horizontal");

        // 2. 가속 및 후진 (앞/뒤 방향)
        float vertical = 0f;

        // [키보드 입력] W 누르면 1, S 누르면 -1
        float keyboardVertical = Input.GetAxis("Vertical");

        // [레이싱 휠 입력] 페달 밟는 값 계산
        float wheelVertical = 0f;
        if (useRacingWheel)
        {
            float rawAccel = 0f;
            float rawBrake = 0f;

            try
            {
                rawAccel = Input.GetAxis("AccelWheel");
                rawBrake = Input.GetAxis("BrakeWheel");
            }
            catch { } // 휠 연결이 안 되어 있거나 설정이 없어도 에러가 나지 않도록 방어

            // 휠의 날것(-1 ~ 1) 신호를 0 ~ 1 엔진 파워로 변환
            float finalAccel = (rawAccel + 1f) / 2f;
            float finalBrake = (rawBrake + 1f) / 2f;

            // 가속 파워에서 브레이크 파워를 뺌
            wheelVertical = finalAccel - finalBrake;
        }

        // [핵심] 키보드와 휠 자동 전환 로직
        // 플레이어가 키보드(W나 S)를 조금이라도 누르고 있다면 키보드 입력을 우선으로 적용하고,
        // 키보드에서 손을 떼고 있다면 레이싱 휠 페달의 입력을 적용합니다.
        if (Mathf.Abs(keyboardVertical) > 0.05f)
        {
            vertical = keyboardVertical;
        }
        else
        {
            vertical = wheelVertical;
        }

        // [대마왕 저주 (키보드 반전)]
        if (itemManager != null && itemManager.IsDevilAffected)
        {
            horizontal *= -1f;
        }

        // [미사일 스턴 (완전 마비)]
        if (itemManager != null && itemManager.IsStunned)
        {
            horizontal = 0f;
            vertical = 0f;
        }

        // [드리프트 조작 (L-Shift 또는 휠 패들/LB 버튼)]
        // 둘 중 하나라도 누르면 작동합니다.
        bool isDriftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.JoystickButton4);
        bool isDriftReleased = Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.JoystickButton4);

        if (isDriftHeld && (itemManager == null || !itemManager.IsStunned))
        {
            if (!arcadeCar.isDrifting) arcadeCar.StartDrift();
        }
        else if (isDriftReleased || (itemManager != null && itemManager.IsStunned))
        {
            if (arcadeCar.isDrifting) arcadeCar.EndDriftAndBoost();
        }

        // [아이템 사용 (Ctrl 키 또는 Xbox A 버튼)]
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            if (itemManager != null && !itemManager.IsStunned)
            {
                Debug.Log("아이템 사용 버튼 입력됨! (키보드 Ctrl 또는 휠 A 버튼)");

                itemManager.UseItem(); // 구현해둔 아이템 사용 함수 호출!
            }
        }

        // 3. 최종 계산된 완벽한 수치를 자동차 물리 엔진에 전달
        arcadeCar.horizontalInput = horizontal;
        arcadeCar.verticalInput = vertical;
    }
}