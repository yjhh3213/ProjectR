using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class ArcadeCarController : MonoBehaviour
{
    [Header("바퀴 모델")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform rearLeftWheel;
    public Transform rearRightWheel;

    [Header("아케이드 조작 세팅")]
    public float maxSpeedKmh = 250f;

    // ★ 주의: ForceMode.Force를 사용하므로, 이제 차량 무게(예: 1500kg)를 밀어내려면 힘이 훨씬 커야 합니다!
    // 인스펙터에서 값을 아래처럼 10배 이상 뻥튀기해 주세요.
    public float motorForce = 15000f;
    public float brakeForce = 25000f;
    public float reverseForce = 10000f;

    public float turnSpeed = 100f;
    public float moveSpeed = 50f;

    // 관성 주행 시 자연스럽게 감속되는 저항력 (엔진 브레이크 느낌)
    public float coastingFriction = 500f;

    [Header("드리프트 & 그립 세팅")]
    public float driftAccelPenalty = 0.4f;
    public float normalGrip = 0.95f;
    public float driftGrip = 0.2f;
    public float driftBoostForce = 15f;
    public TrailRenderer[] skidMarks;

    [Header("안정성 세팅 (이륙 방지)")]
    public float downforce = 50f;
    public Vector3 centerOfMassOffset = new Vector3(0, -0.5f, 0);

    [Header("기어 시스템 (이니셜 D 스타일)")]
    public float[] gearRatios = { 4.056f, 2.301f, 1.595f, 1.248f, 1.001f, 0.796f };
    public float finalDriveRatio = 3.7f;
    public int currentGear = 1;
    public float engineRPM = 0f;

    [HideInInspector] public float verticalInput;
    [HideInInspector] public float horizontalInput;
    [HideInInspector] public bool isDrifting;
    [HideInInspector] public float currentGrip;

    [HideInInspector] public float moveInput;
    [HideInInspector] public float turnInput;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMassOffset;
        currentGrip = normalGrip;

        foreach (TrailRenderer tr in skidMarks)
        {
            tr.emitting = false;
            tr.time = 3f;
        }
    }

    void Update()
    {
        RotateAndSteerWheels();
    }

    void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        ApplyLateralFriction();
        ApplyDownforce();
    }

    public void ApplyDriftBoost()
    {
        rb.AddForce(transform.forward * driftBoostForce, ForceMode.VelocityChange);
    }

    public void ShiftUp()
    {
        if (currentGear == -1)
        {
            currentGear = 1; // 후진에서 업시프트하면 1단으로 복귀
            Debug.Log("기어 변속: 1단");
        }
        else if (currentGear < gearRatios.Length)
        {
            currentGear++;
            Debug.Log("기어 변속 업: " + currentGear + "단");
        }
    }

    public void ShiftDown()
    {
        if (currentGear > 1)
        {
            currentGear--;
            Debug.Log("기어 변속 다운: " + currentGear + "단");
        }
        else if (currentGear == 1)
        {
            currentGear = -1; // 1단에서 다운시프트하면 후진(-1단)으로 진입!
            Debug.Log("기어 변속: 후진(-1단)");
        }
    }

    private void HandleMotor()
    {
        float forwardVelocity = Vector3.Dot(transform.forward, rb.velocity);
        float currentSpeedKmh = forwardVelocity * 3.6f;

        float currentGearMaxSpeed = 250f;

        switch (currentGear)
        {
            case -1: currentGearMaxSpeed = 45f; break;  // 후진 최대 속도 45km/h 제한
            case 1: currentGearMaxSpeed = 45f; break;   // 1단: 45km/h 제한
            case 2: currentGearMaxSpeed = 85f; break;   // 2단
            case 3: currentGearMaxSpeed = 130f; break;  // 3단
            case 4: currentGearMaxSpeed = 175f; break;  // 4단
            case 5: currentGearMaxSpeed = 215f; break;  // 5단
            case 6: currentGearMaxSpeed = maxSpeedKmh; break; // 6단 (최고속도)
            default: currentGearMaxSpeed = 0f; break;
        }

        float currentMotorPower = motorForce;

        if (isDrifting)
        {
            float turnPenalty = 1f - (Mathf.Abs(horizontalInput) * 0.5f);
            currentMotorPower *= (driftAccelPenalty * turnPenalty);
        }

        // 1. 후진 기어(-1단) 상태에서 S키(음수 입력)를 누를 때
        if (currentGear == -1 && verticalInput < 0)
        {
            float reverseSpeedKmh = Mathf.Abs(currentSpeedKmh);

            // 후진 최대 속도(45km/h) 미만일 때만 뒤로 가속
            if (reverseSpeedKmh < currentGearMaxSpeed)
            {
                float speedRatio = 1f - (reverseSpeedKmh / currentGearMaxSpeed);
                speedRatio = Mathf.Clamp(speedRatio, 0.1f, 1f);

                // 뒤쪽 방향(-transform.forward)으로 힘을 가함 (S키 입력이 음수이므로 절댓값 활용)
                rb.AddForce(-transform.forward * currentMotorPower * Mathf.Abs(verticalInput) * speedRatio, ForceMode.Force);
            }
        }
        // 2. 전진 주행 중일 때 (W키 또는 기본 가속)
        else if (verticalInput > 0)
        {
            if (currentSpeedKmh < currentGearMaxSpeed)
            {
                float speedRatio = 1f - (currentSpeedKmh / currentGearMaxSpeed);
                speedRatio = Mathf.Clamp(speedRatio, 0.1f, 1f);

                rb.AddForce(transform.forward * currentMotorPower * verticalInput * speedRatio, ForceMode.Force);
            }
        }
        // 3. 브레이크를 밟을 때
        else if (verticalInput < 0 && currentGear != -1)
        {
            if (forwardVelocity > 1.0f)
            {
                rb.AddForce(-transform.forward * brakeForce, ForceMode.Force);
            }
        }
        // 4. 발을 뗐을 때 (관성 주행)
        else
        {
            if (Mathf.Abs(forwardVelocity) > 0.1f)
            {
                rb.AddForce(-rb.velocity.normalized * coastingFriction, ForceMode.Force);
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

    public void StartDrift()
    {
        isDrifting = true;
        currentGrip = driftGrip;
        SetSkidMarks(true);
    }

    public void EndDriftAndBoost()
    {
        isDrifting = false;
        SetSkidMarks(false);
        ApplyDriftBoost();
        StartCoroutine(RestoreGripRoutine());
    }

    private void ApplyLateralFriction()
    {
        Vector3 forwardVelocity = transform.forward * rb.velocity.magnitude;
        rb.velocity = Vector3.Lerp(rb.velocity, forwardVelocity, currentGrip * Time.fixedDeltaTime * 5f);
    }

    private IEnumerator RestoreGripRoutine()
    {
        float timeElapsed = 0f;
        float startGrip = currentGrip;
        while (timeElapsed < 2f)
        {
            timeElapsed += Time.deltaTime;
            currentGrip = Mathf.Lerp(startGrip, normalGrip, timeElapsed / 2f);
            yield return null;
        }
        currentGrip = normalGrip;
    }

    private void SetSkidMarks(bool state)
    {
        foreach (TrailRenderer tr in skidMarks) { tr.emitting = state; }
    }

    private void RotateAndSteerWheels()
    {
        float wheelSpin = verticalInput * moveSpeed * 15f * Time.deltaTime;
        if (frontLeftWheel) frontLeftWheel.Rotate(wheelSpin, 0, 0);
        if (frontRightWheel) frontRightWheel.Rotate(wheelSpin, 0, 0);
        if (rearLeftWheel) rearLeftWheel.Rotate(wheelSpin, 0, 0);
        if (rearRightWheel) rearRightWheel.Rotate(wheelSpin, 0, 0);

        if (frontLeftWheel && frontRightWheel)
        {
            float steerAngle = horizontalInput * 30f;
            frontLeftWheel.localEulerAngles = new Vector3(frontLeftWheel.localEulerAngles.x, steerAngle, 0);
            frontRightWheel.localEulerAngles = new Vector3(frontRightWheel.localEulerAngles.x, steerAngle, 0);
        }
    }

    private void ApplyDownforce()
    {
        float currentSpeed = rb.velocity.magnitude;
        rb.AddForce(-transform.up * downforce * currentSpeed, ForceMode.Force);
    }
}