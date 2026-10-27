using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private ItemManager itemManager;
    private ArcadeCarController arcadeCar;
    private Rigidbody rb;

    [Header("제어 하드웨어 설정")]
    public bool useRacingWheel = true;

    [Header("휠 조향 감도 설정")]
    public float physicalWheelAngle = 450f;
    public float targetSteeringAngle = 90f;

    void Start()
    {
        itemManager = GetComponent<ItemManager>();
        arcadeCar = GetComponent<ArcadeCarController>();
        rb = GetComponent<Rigidbody>();

        // 로비에서 설정한 입력 방식 불러오기 (0: 키보드, 1: 레이싱휠)
        int inputType = PlayerPrefs.GetInt("InputType", 0);
        Debug.Log("게임 씬에서 불러온 InputType 값: " + inputType);

        if (inputType == 0)
        {
            useRacingWheel = false; // 키보드 모드
        }
        else
        {
            useRacingWheel = true;  // 휠 모드
        }
    }

    void Update()
    {
        if (arcadeCar == null || rb == null) return;

        // 1. 조향 처리
        float horizontal = Input.GetAxis("Horizontal");
        if (useRacingWheel)
        {
            float steeringMultiplier = physicalWheelAngle / targetSteeringAngle;
            horizontal *= steeringMultiplier;
            horizontal = Mathf.Clamp(horizontal, -1f, 1f);
        }

        // 2. 가속 및 브레이크 처리
        float vertical = 0f;
        float keyboardVertical = Input.GetAxis("Vertical");
        float wheelVertical = 0f;

        if (useRacingWheel)
        {
            float rawAccel = 1f;
            float rawBrake = -1f;

            try
            {
                rawAccel = Input.GetAxis("AccelWheel");
                rawBrake = Input.GetAxis("BrakeWheel");
            }
            catch { }

            float finalAccel = (1f - rawAccel) / 2f;
            float finalBrake = (rawBrake + 1f) / 2f;
            wheelVertical = finalAccel - finalBrake;
        }

        if (Mathf.Abs(keyboardVertical) > 0.05f) vertical = keyboardVertical;
        else vertical = wheelVertical;

        // 아이템 및 스턴 처리
        if (itemManager != null && itemManager.IsDevilAffected) horizontal *= -1f;
        if (itemManager != null && itemManager.IsStunned)
        {
            horizontal = 0f;
            vertical = 0f;
        }

        // 아이템 사용 (LeftControl 또는 JoystickButton0)
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            if (itemManager != null && !itemManager.IsStunned)
            {
                itemManager.UseItem();
                Debug.Log("아이템 사용!");
            }
        }

        // 3. 변속 및 주행 방식 분기 (키보드 vs 휠)
        if (!useRacingWheel)
        {
            // ==========================================
            // [키보드 유저 전용 로직] 
            // ==========================================
            float forwardVelocity = Vector3.Dot(transform.forward, rb.velocity);

            if (vertical < -0.05f)
            {
                // 전진 중일 때 S키를 누르면 먼저 브레이크(1단)로 감속 유도
                if (forwardVelocity > 0.5f)
                {
                    arcadeCar.currentGear = 1; // 브레이크 역할
                }
                else
                {
                    // 차가 거의 멈췄거나 후진 중일 때 S키를 유지하면 -1단(후진)으로 설정
                    arcadeCar.currentGear = -1;
                }
            }
            else if (vertical > 0.05f || forwardVelocity >= 0.5f)
            {
                // W키를 누르거나 전진 중일 때는 기어를 최고단수(6단)로 고정하여 풀가속!
                if (arcadeCar.currentGear == -1) arcadeCar.currentGear = 1;
                else arcadeCar.currentGear = 6;
            }
        }
        else
        {
            // ==========================================
            // [휠 유저 전용 로직] 수동 패들 변속 (Initial D 스타일)
            // ==========================================
            if (Input.GetKeyDown(KeyCode.JoystickButton4))
            {
                arcadeCar.ShiftUp();
                Debug.Log("수동 변속 업: " + arcadeCar.currentGear + "단");
            }
            if (Input.GetKeyDown(KeyCode.JoystickButton5))
            {
                arcadeCar.ShiftDown();
                Debug.Log("수동 변속 다운: " + arcadeCar.currentGear + "단");
            }
        }

        arcadeCar.horizontalInput = horizontal;
        arcadeCar.verticalInput = vertical;
    }
}