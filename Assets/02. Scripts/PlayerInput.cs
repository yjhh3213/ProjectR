using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private ItemManager itemManager;
    private ArcadeCarController arcadeCar;
    private Rigidbody rb;

    [Header("제어 하드웨어 설정")]
    [Tooltip("로비 설정에 따라 자동으로 켜고 꺼집니다.")]
    public bool useRacingWheel = true;
    private bool isAutoShift = false; // 자동 변속 여부

    void Start()
    {
        itemManager = GetComponent<ItemManager>();
        arcadeCar = GetComponent<ArcadeCarController>();
        rb = GetComponent<Rigidbody>();

        // 로비에서 설정한 입력 방식 불러오기 (0: 키보드/자동, 1: 레이싱휠/수동)
        int inputType = PlayerPrefs.GetInt("InputType", 0);
        if (inputType == 0)
        {
            useRacingWheel = false;
            isAutoShift = true; // 키보드 유저는 자동 변속
        }
        else
        {
            useRacingWheel = true;
            isAutoShift = false; // 휠 유저는 수동 변속
        }
    }

    void Update()
    {
        if (arcadeCar == null) return;

        // 1. 조향 (좌/우 방향)
        float horizontal = Input.GetAxis("Horizontal");

        // 2. 가속 및 후진 (앞/뒤 방향)
        // 유니티 기본 설정상 Input.GetAxis("Vertical")은 'S' 키 또는 '아래 화살표'를 누르면 마이너스 값이 나옵니다.
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

        // 키보드와 휠 자동 전환 로직
        if (Mathf.Abs(keyboardVertical) > 0.05f) vertical = keyboardVertical;
        else vertical = wheelVertical;

        // [대마왕 저주]
        if (itemManager != null && itemManager.IsDevilAffected) horizontal *= -1f;

        // [미사일 스턴]
        if (itemManager != null && itemManager.IsStunned)
        {
            horizontal = 0f;
            vertical = 0f;
        }

        // [드리프트 조작]
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

        // [아이템 사용]
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            if (itemManager != null && !itemManager.IsStunned) itemManager.UseItem();
        }

        // ----------------------------------------------------------------------------------
        // [드라이빙 감성 - 기어 변속 시스템]
        if (isAutoShift)
        {
            HandleAutoShift();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.JoystickButton14))
            {
                arcadeCar.ShiftUp();
                Debug.Log("수동 변속 업: " + arcadeCar.currentGear + "단");
            }
            if (Input.GetKeyDown(KeyCode.JoystickButton13))
            {
                arcadeCar.ShiftDown();
                Debug.Log("수동 변속 다운: " + arcadeCar.currentGear + "단");
            }
        }
        // ----------------------------------------------------------------------------------

        arcadeCar.horizontalInput = horizontal;
        arcadeCar.verticalInput = vertical;
    }

    // ★ [수정됨] 후진 방어 코드가 추가된 자동 변속 로직
    private void HandleAutoShift()
    {
        if (arcadeCar == null || rb == null || arcadeCar.gearRatios == null || arcadeCar.gearRatios.Length == 0) return;

        // [핵심 추가] 자동차가 현재 앞으로 가는지 뒤로 가는지(후진) 판별합니다.
        float forwardVelocity = Vector3.Dot(transform.forward, rb.velocity);

        // 만약 차가 멈춰있거나 후진 중이라면(속도가 0 이하), 기어를 올리지 않고 강제로 1단으로 맞춥니다.
        if (forwardVelocity < 1.0f)
        {
            while (arcadeCar.currentGear > 1)
            {
                arcadeCar.ShiftDown();
            }
            return; // 후진 중일 때는 아래의 변속 로직을 무시합니다.
        }

        float currentSpeedKmh = rb.velocity.magnitude * 3.6f;
        int currentGear = arcadeCar.currentGear;

        // 최고 단수의 기어비 가져오기 (배열의 마지막 값)
        float topGearRatio = arcadeCar.gearRatios[arcadeCar.gearRatios.Length - 1];

        // 1. 기어 상승 (현재 기어 최고속도의 95% 도달 시)
        if (currentGear < arcadeCar.gearRatios.Length)
        {
            float currentGearRatio = arcadeCar.gearRatios[currentGear - 1];
            float currentGearMaxSpeed = arcadeCar.maxSpeedKmh * (topGearRatio / currentGearRatio);

            if (currentSpeedKmh >= currentGearMaxSpeed * 0.95f)
            {
                arcadeCar.ShiftUp();
            }
        }

        // 2. 기어 하강 (이전 단수 최고속도의 80% 이하로 떨어질 시)
        if (currentGear > 1)
        {
            float prevGearRatio = arcadeCar.gearRatios[currentGear - 2];
            float prevGearMaxSpeed = arcadeCar.maxSpeedKmh * (topGearRatio / prevGearRatio);

            if (currentSpeedKmh < prevGearMaxSpeed * 0.80f)
            {
                arcadeCar.ShiftDown();
            }
        }
    }
}