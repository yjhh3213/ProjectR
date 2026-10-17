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

    public void ShiftUp() { if (currentGear < gearRatios.Length) currentGear++; }
    public void ShiftDown() { if (currentGear > 1) currentGear--; }

    private void HandleMotor()
    {
        float forwardVelocity = Vector3.Dot(transform.forward, rb.velocity);
        float currentSpeedKmh = forwardVelocity * 3.6f;

        float topGearRatio = gearRatios[gearRatios.Length - 1];
        float currentGearRatio = gearRatios[currentGear - 1];

        float currentGearMaxSpeed = maxSpeedKmh * (topGearRatio / currentGearRatio);
        float gearAccelMultiplier = Mathf.Clamp(currentGearRatio / topGearRatio, 1f, 2.5f);

        float currentMotorPower = motorForce * gearAccelMultiplier;

        if (isDrifting)
        {
            float turnPenalty = 1f - (Mathf.Abs(horizontalInput) * 0.5f);
            currentMotorPower *= (driftAccelPenalty * turnPenalty);
        }

        // 1. 악셀을 밟을 때 (전진)
        if (verticalInput > 0)
        {
            if (currentSpeedKmh < currentGearMaxSpeed)
            {
                float speedRatio = 1f - (currentSpeedKmh / currentGearMaxSpeed);
                speedRatio = Mathf.Clamp(speedRatio, 0.1f, 1f);

                // ForceMode.Force 적용: 이제 질량(Mass)의 영향을 받아 서서히 가속됩니다!
                rb.AddForce(transform.forward * currentMotorPower * verticalInput * speedRatio, ForceMode.Force);
            }
        }
        // 2. 브레이크를 밟을 때 (감속 및 후진)
        else if (verticalInput < 0)
        {
            if (forwardVelocity > 1.0f)
            {
                rb.AddForce(-transform.forward * brakeForce, ForceMode.Force);
            }
            else
            {
                rb.AddForce(transform.forward * reverseForce * verticalInput, ForceMode.Force);
            }
        }
        // 3. 발을 뗐을 때 (관성 주행 / 엔진 브레이크)
        else
        {
            if (forwardVelocity > 0.1f)
            {
                // Drag 값을 낮춘 대신, 여기서 부드럽게 감속시켜 진짜 자동차가 굴러가는 느낌을 줍니다.
                rb.AddForce(-transform.forward * coastingFriction, ForceMode.Force);
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