using UnityEngine;
using System.Collections; // 코루틴(타이머) 사용을 위해 추가

[RequireComponent(typeof(Rigidbody))]
public class ArcadeCarController : MonoBehaviour
{
    [Header("바퀴 모델")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform rearLeftWheel;
    public Transform rearRightWheel;

    [Header("아케이드 조작 세팅")]
    public float maxSpeedKmh = 250f;    // 인스펙터에서 설정할 최고 속도 (km/h)
    public float motorForce = 3000f;
    public float brakeForce = 4000f;
    public float reverseForce = 1500f;
    public float turnSpeed = 100f;
    public float moveSpeed = 50f;

    [Header("드리프트 & 그립 세팅")]
    public float driftAccelPenalty = 0.4f; // 3.15.1 드리프트 중 가속 더딤 (기본 가속의 40%만 적용)
    public float normalGrip = 0.95f;       // (평소 접지력)
    public float driftGrip = 0.2f;         // (드리프트 시 미끄러짐)
    public float driftBoostForce = 15f;    // 3.15.3 탈출 부스터 힘 (속도 10 정도 상승)
    public TrailRenderer[] skidMarks;      // 3.15.2 뒷바퀴 스키드마크 연결용

    // 외부(PlayerInput이나 AIInput)에서 값을 넣어줄 수 있도록 public으로 변경합니다.
    [HideInInspector] public float verticalInput;
    [HideInInspector] public float horizontalInput;
    [HideInInspector] public bool isDrifting;
    [HideInInspector] public float currentGrip; // private에서 변경, 1.0이면 정상 그립, 낮으면 미끄러짐 (3.15.4)

    [HideInInspector] public float moveInput;
    [HideInInspector] public float turnInput;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentGrip = normalGrip; // 게임 시작 시 평소 접지력으로 초기화

        // 스키드마크 초기화 (처음엔 안 그려지게)
        foreach (TrailRenderer tr in skidMarks)
        {
            tr.emitting = false;
            tr.time = 3f; // 3초 뒤 사라지게 설정 (3.15.2)
        }
    }

    void Update()
    {
        // 뇌(PlayerInput 또는 AIInput)에서 전달해준 verticalInput, horizontalInput 값을 바탕으로
        // 매 프레임 바퀴 모델을 굴리고 핸들을 꺾어줍니다!
        RotateAndSteerWheels();
    }

    void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        ApplyLateralFriction(); // 3.15.4 횡방향 마찰력은 여기서 독립적으로 실행!
    }

    // PlayerInput에서 호출할 수 있도록 부스트 함수를 public으로 바꿔주세요.
    public void ApplyDriftBoost()
    {
        rb.AddForce(transform.forward * driftBoostForce, ForceMode.VelocityChange);
    }

    private void HandleMotor()
    {
        // 1. 현재 앞방향 속도 (m/s)
        float forwardVelocity = Vector3.Dot(transform.forward, rb.velocity);

        // 2. m/s를 우리가 아는 km/h로 변환 (1 m/s = 3.6 km/h)
        float currentSpeedKmh = forwardVelocity * 3.6f;

        // 드리프트 패널티 계산
        float currentMotorPower = motorForce;
        if (isDrifting)
        {
            float turnPenalty = 1f - (Mathf.Abs(horizontalInput) * 0.5f);
            currentMotorPower = motorForce * driftAccelPenalty * turnPenalty;
        }

        // 전진 가속 로직
        if (verticalInput > 0)
        {
            // 3. 현재 속도가 최고 속도(maxSpeedKmh)보다 낮을 때만 가속!
            if (currentSpeedKmh < maxSpeedKmh)
            {
                // [가속도 곡선 마법] 
                // 속도가 0일 때는 speedRatio가 1.0 (100% 힘)
                // 속도가 최고 속도에 근접하면 speedRatio가 0.1 (10% 힘)으로 줄어듦
                float speedRatio = 1f - (currentSpeedKmh / maxSpeedKmh);

                // 힘이 완전히 0이 되면 도달하지 못하므로 최소 10%의 밀어주는 힘은 남겨둠
                speedRatio = Mathf.Clamp(speedRatio, 0.1f, 1f);

                // 최종적으로 변동하는 힘을 가해줌
                rb.AddForce(transform.forward * currentMotorPower * verticalInput * speedRatio, ForceMode.Acceleration);
            }
        }
        // 브레이크 및 후진
        else if (verticalInput < 0)
        {
            if (forwardVelocity > 1.0f)
            {
                rb.AddForce(-transform.forward * brakeForce, ForceMode.Acceleration);
            }
            else
            {
                rb.AddForce(transform.forward * reverseForce * verticalInput, ForceMode.Acceleration);
            }
        }
    }

    private void HandleSteering()
    {
        float currentSpeed = rb.velocity.magnitude;

        if (currentSpeed > 0.1f)
        {
            float dir = Mathf.Sign(Vector3.Dot(transform.forward, rb.velocity));
            float turnMultiplier = horizontalInput * turnSpeed * dir * Time.fixedDeltaTime;
            transform.Rotate(0, turnMultiplier, 0);
        }
    }

    // 기존에 있던 private void CheckDriftInput() 함수는 통째로 지워주세요!

    // 1. PlayerInput(뇌)에서 L-Shift를 눌렀을 때 호출할 함수
    public void StartDrift()
    {
        isDrifting = true;
        // 0.3f로 고정되어 있던 값을 인스펙터에서 설정한 driftGrip 값으로 변경
        currentGrip = driftGrip;
        SetSkidMarks(true);
    }

    // 2. PlayerInput(뇌)에서 L-Shift를 뗐을 때 호출할 함수
    public void EndDriftAndBoost()
    {
        isDrifting = false;
        SetSkidMarks(false);

        // 3.15.3 순간 반동 부스터 발동 (만들어두신 public 함수 재사용)
        ApplyDriftBoost();

        // 3.15.4 2초에 걸쳐 서서히 그립력 회복하는 코루틴 실행
        StartCoroutine(RestoreGripRoutine());
    }

    // 3.15.4 아케이드 특유의 쫀득한 횡방향 마찰력 구현
    private void ApplyLateralFriction()
    {
        // 현재 차량의 속도 벡터를, 차량이 바라보는 정면 방향으로 억지로 꺾어주는(Lerp) 로직
        // currentGrip이 낮으면 코너 바깥으로 쭉 밀려나고, 높으면 기차처럼 레일을 따라갑니다.
        Vector3 forwardVelocity = transform.forward * rb.velocity.magnitude;
        rb.velocity = Vector3.Lerp(rb.velocity, forwardVelocity, currentGrip * Time.fixedDeltaTime * 5f);
    }

    // 3.15.4 그립 회복 타이머 (2초)
    // 3. 코루틴 수정: 1.0f로 고정되어 있던 부분을 normalGrip으로 모두 변경
    private IEnumerator RestoreGripRoutine()
    {
        float timeElapsed = 0f;
        float startGrip = currentGrip;

        while (timeElapsed < 2f) // 2초 동안
        {
            timeElapsed += Time.deltaTime;
            // 점진적 회복: 1.0f 대신 각 차량에 맞는 normalGrip 변수 사용
            currentGrip = Mathf.Lerp(startGrip, normalGrip, timeElapsed / 2f);
            yield return null;
        }
        // 최종 회복 완료 시 1.0f 대신 normalGrip 대입
        currentGrip = normalGrip;
    }

    private void SetSkidMarks(bool state)
    {
        foreach (TrailRenderer tr in skidMarks)
        {
            tr.emitting = state;
        }
    }

    private void RotateAndSteerWheels()
    {
        // moveInput -> verticalInput으로 변경
        float wheelSpin = verticalInput * moveSpeed * 15f * Time.deltaTime;
        if (frontLeftWheel) frontLeftWheel.Rotate(wheelSpin, 0, 0);
        if (frontRightWheel) frontRightWheel.Rotate(wheelSpin, 0, 0);
        if (rearLeftWheel) rearLeftWheel.Rotate(wheelSpin, 0, 0);
        if (rearRightWheel) rearRightWheel.Rotate(wheelSpin, 0, 0);

        if (frontLeftWheel && frontRightWheel)
        {
            // turnInput -> horizontalInput으로 변경
            float steerAngle = horizontalInput * 30f;
            frontLeftWheel.localEulerAngles = new Vector3(frontLeftWheel.localEulerAngles.x, steerAngle, 0);
            frontRightWheel.localEulerAngles = new Vector3(frontRightWheel.localEulerAngles.x, steerAngle, 0);
        }
    }
}