using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ArcadeCarController : MonoBehaviour
{
    [Header("주행 설정")]
    public float moveSpeed = 50f;
    public float turnSpeed = 100f;

    [Header("바퀴 모델")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform rearLeftWheel;
    public Transform rearRightWheel;

    // 이제 이 변수들은 외부(PlayerInput 또는 AIInput)에서 조종합니다.
    [HideInInspector] public float moveInput;
    [HideInInspector] public float turnInput;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update에서는 '입력'을 받지 않고 오직 '시각적 연출'만 담당합니다.
    void Update()
    {
        RotateAndSteerWheels();
    }

    void FixedUpdate()
    {
        ApplyPhysics();
    }

    private void ApplyPhysics()
    {
        Vector3 moveForce = transform.forward * moveInput * moveSpeed;
        rb.AddForce(moveForce, ForceMode.Acceleration);

        if (Mathf.Abs(moveInput) > 0.1f)
        {
            float direction = Mathf.Sign(moveInput);
            Quaternion turnRotation = Quaternion.Euler(0f, turnInput * turnSpeed * direction * Time.fixedDeltaTime, 0f);
            rb.MoveRotation(rb.rotation * turnRotation);
        }
    }

    private void RotateAndSteerWheels() { /* 기존 코드와 동일 */ }
}