using UnityEngine;
using System.Collections; // 코루틴(타이머) 사용을 위해 추가

[RequireComponent(typeof(Rigidbody))]
public class ArcadeCarController : MonoBehaviour
{
    [Header("주행 설정")]
    public float moveSpeed = 50f;
    public float turnSpeed = 100f;

    [Header("드리프트 설정 (3.15)")]
    public float driftAccelPenalty = 0.4f; // 3.15.1 드리프트 중 가속 더딤 (기본 가속의 40%만 적용)
    public float driftBoostForce = 15f;    // 3.15.3 탈출 부스터 힘 (속도 10 정도 상승)
    public TrailRenderer[] skidMarks;      // 3.15.2 뒷바퀴 스키드마크 연결용

    [Header("바퀴 모델")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform rearLeftWheel;
    public Transform rearRightWheel;

    [HideInInspector] public float moveInput;
    [HideInInspector] public float turnInput;

    // 드리프트 상태 변수
    private bool isDrifting = false;
    private float currentGrip = 1.0f; // 1.0이면 정상 그립, 낮으면 미끄러짐 (3.15.4)

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 스키드마크 초기화 (처음엔 안 그려지게)
        foreach (TrailRenderer tr in skidMarks)
        {
            tr.emitting = false;
            tr.time = 3f; // 3초 뒤 사라지게 설정 (3.15.2)
        }
    }

    void Update()
    {
        // 입력은 PlayerInput에서 넘어오지만, 드리프트 키(L-Shift) 확인은 여기서 직접 처리 (AI는 별도 처리 필요)
        CheckDriftInput();
        RotateAndSteerWheels();
    }

    void FixedUpdate()
    {
        ApplyPhysics();
        ApplyLateralFriction(); // 횡방향 마찰력(그립) 계산 적용
    }

    private void CheckDriftInput()
    {
        // 1. 좌측 시프트 누름: 드리프트 시작
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isDrifting = true;
            currentGrip = 0.3f; // 3.15.4 마찰력을 확 낮춰서 미끄러지게 만듦
            SetSkidMarks(true);
        }

        // 2. 좌측 시프트 뗌: 드리프트 종료 및 부스터 발동
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isDrifting = false;
            SetSkidMarks(false);

            // 3.15.3 순간 반동 부스터
            rb.AddForce(transform.forward * driftBoostForce, ForceMode.VelocityChange);

            // 3.15.4 2초에 걸쳐 서서히 그립력 회복하는 코루틴 실행
            StartCoroutine(RestoreGripRoutine());
        }
    }

    private void ApplyPhysics()
    {
        // 3.15.1 드리프트 중이면서 회전 중일 때 가속력 감소 로직
        float currentAccel = moveSpeed;
        if (isDrifting)
        {
            // 회전(turnInput)이 클수록 속도가 더 깎이도록 수식 적용
            float turnPenalty = 1f - (Mathf.Abs(turnInput) * 0.5f);
            currentAccel = moveSpeed * driftAccelPenalty * turnPenalty;
        }

        Vector3 moveForce = transform.forward * moveInput * currentAccel;
        rb.AddForce(moveForce, ForceMode.Acceleration);

        if (Mathf.Abs(moveInput) > 0.1f)
        {
            float direction = Mathf.Sign(moveInput);
            Quaternion turnRotation = Quaternion.Euler(0f, turnInput * turnSpeed * direction * Time.fixedDeltaTime, 0f);
            rb.MoveRotation(rb.rotation * turnRotation);
        }
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
    private IEnumerator RestoreGripRoutine()
    {
        float timeElapsed = 0f;
        float startGrip = currentGrip;

        while (timeElapsed < 2f) // 2초 동안
        {
            timeElapsed += Time.deltaTime;
            currentGrip = Mathf.Lerp(startGrip, 1.0f, timeElapsed / 2f); // 점진적 회복
            yield return null;
        }
        currentGrip = 1.0f;
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
        // 기존 바퀴 회전 로직 유지
        float wheelSpin = moveInput * moveSpeed * 15f * Time.deltaTime;
        if (frontLeftWheel) frontLeftWheel.Rotate(wheelSpin, 0, 0);
        if (frontRightWheel) frontRightWheel.Rotate(wheelSpin, 0, 0);
        if (rearLeftWheel) rearLeftWheel.Rotate(wheelSpin, 0, 0);
        if (rearRightWheel) rearRightWheel.Rotate(wheelSpin, 0, 0);

        if (frontLeftWheel && frontRightWheel)
        {
            float steerAngle = turnInput * 30f;
            frontLeftWheel.localEulerAngles = new Vector3(frontLeftWheel.localEulerAngles.x, steerAngle, 0);
            frontRightWheel.localEulerAngles = new Vector3(frontRightWheel.localEulerAngles.x, steerAngle, 0);
        }
    }
}