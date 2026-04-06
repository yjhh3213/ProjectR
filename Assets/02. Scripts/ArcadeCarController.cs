using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ArcadeCarController : MonoBehaviour
{
    [Header("주행 설정")]
    public float moveSpeed = 50f;   // 전진/후진 속도
    public float turnSpeed = 100f;  // 회전 속도

    [Header("바퀴 모델 연결")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform rearLeftWheel;
    public Transform rearRightWheel;

    private Rigidbody rb;
    private float moveInput;
    private float turnInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 1. WASD (또는 방향키) 입력 받기
        moveInput = Input.GetAxis("Vertical");   // W/S 키 (-1.0 ~ 1.0)
        turnInput = Input.GetAxis("Horizontal"); // A/D 키 (-1.0 ~ 1.0)

        // 2. 바퀴 시각적 굴림 & 조향 (보너스)
        RotateAndSteerWheels();
    }

    void FixedUpdate()
    {
        // 3. 물리적 이동 (가속도 적용)
        Vector3 moveForce = transform.forward * moveInput * moveSpeed;
        rb.AddForce(moveForce, ForceMode.Acceleration);

        // 4. 물리적 회전 (차가 움직일 때만 회전하도록 처리)
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            // 후진할 때는 핸들 방향이 반대로 적용되도록 부호 계산
            float direction = Mathf.Sign(moveInput);
            Quaternion turnRotation = Quaternion.Euler(0f, turnInput * turnSpeed * direction * Time.fixedDeltaTime, 0f);
            rb.MoveRotation(rb.rotation * turnRotation);
        }
    }

    private void RotateAndSteerWheels()
    {
        // 바퀴 X축 회전 (굴러가는 모션)
        float wheelSpin = moveInput * moveSpeed * 15f * Time.deltaTime;

        if (frontLeftWheel) frontLeftWheel.Rotate(wheelSpin, 0, 0);
        if (frontRightWheel) frontRightWheel.Rotate(wheelSpin, 0, 0);
        if (rearLeftWheel) rearLeftWheel.Rotate(wheelSpin, 0, 0);
        if (rearRightWheel) rearRightWheel.Rotate(wheelSpin, 0, 0);

        // 앞바퀴 Y축 회전 (핸들 조향 모션 - 최대 30도 꺾임)
        if (frontLeftWheel && frontRightWheel)
        {
            float steerAngle = turnInput * 30f;
            frontLeftWheel.localEulerAngles = new Vector3(frontLeftWheel.localEulerAngles.x, steerAngle, 0);
            frontRightWheel.localEulerAngles = new Vector3(frontRightWheel.localEulerAngles.x, steerAngle, 0);
        }
    }
}