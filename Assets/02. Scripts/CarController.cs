using UnityEngine;
using System.Collections.Generic;

public class CarController : MonoBehaviour
{
    [Header("역할 설정")]
    public bool isAI = true;

    [Header("AI 주행 설정")]
    public List<Transform> waypoints;
    public float arrivalDistance = 20f;
    private int currentIdx = 0;

    // ★ [핵심 추가] 레이스 시작 후 총 몇 개의 체크포인트를 통과했는지 기록 (등수 유지용)
    [HideInInspector]
    public int totalWaypointsPassed = 0;

    [Header("AI 자동 변속 설정 (AI 전용)")]
    [Tooltip("각 기어 단수에서 다음 단수로 넘어가는 속도 기준 (km/h) \n예: [1]은 1단->2단 변속 속도")]
    public float[] aiGearShiftSpeeds = { 0f, 40f, 80f, 120f, 160f, 200f };
    public float shiftCooldown = 0.5f; // 너무 잦은 변속(핑퐁 현상) 방지용 쿨타임
    private float lastShiftTime = 0f;

    private ArcadeCarController arcadeCar;
    private ItemManager itemManager;
    private Rigidbody rb;

    void Start()
    {
        arcadeCar = GetComponent<ArcadeCarController>();
        itemManager = GetComponent<ItemManager>();
        rb = GetComponent<Rigidbody>();
        gameObject.tag = "Player";
    }

    void Update()
    {
        if (itemManager != null && itemManager.IsStunned)
        {
            StopCar();
            return;
        }

        UpdateWaypointCheck();

        if (isAI)
        {
            HandleAIMovement();
        }
    }

    void UpdateWaypointCheck()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        Vector3 targetPos = waypoints[currentIdx].position;

        Vector3 playerPosXZ = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 targetPosXZ = new Vector3(targetPos.x, 0, targetPos.z);

        float distance = Vector3.Distance(playerPosXZ, targetPosXZ);

        if (distance < arrivalDistance)
        {
            Vector3 targetDir = (targetPosXZ - playerPosXZ).normalized;
            float dot = Vector3.Dot(transform.forward, targetDir);

            if (dot <= 0.0f || distance < 5f)
            {
                currentIdx = (currentIdx + 1) % waypoints.Count;

                // ★ [핵심 추가] 체크포인트를 하나 통과할 때마다 누적 점수를 1씩 올립니다.
                totalWaypointsPassed++;
            }
        }
    }

    void HandleAIMovement()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        Vector3 targetPos = waypoints[currentIdx].position;
        Vector3 localTarget = transform.InverseTransformPoint(targetPos);
        localTarget.y = 0;

        float relativeAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        float absAngle = Mathf.Abs(relativeAngle);

        if (arcadeCar != null && rb != null)
        {
            float currentSpeed = rb.velocity.magnitude * 3.6f;

            // ★ [핵심 추가] AI 자동 변속 실행
            HandleAIGearShift(currentSpeed);

            if (absAngle > 25f)
            {
                arcadeCar.verticalInput = (currentSpeed > 70f) ? -0.4f : 0.2f;
                arcadeCar.horizontalInput = Mathf.Clamp(relativeAngle / 15f, -1.0f, 1.0f);
            }
            else if (absAngle > 8f)
            {
                arcadeCar.verticalInput = (currentSpeed > 130f) ? 0.1f : 0.7f;
                arcadeCar.horizontalInput = Mathf.Clamp(relativeAngle / 25f, -1.0f, 1.0f);
            }
            else
            {
                arcadeCar.verticalInput = 1.0f;
                arcadeCar.horizontalInput = Mathf.Clamp(relativeAngle / 40f, -1.0f, 1.0f);
            }
        }
    }

    // ★ [핵심 추가] AI 전용 자동 변속 로직
    private void HandleAIGearShift(float currentSpeed)
    {
        // 쿨타임이 안 지났으면 변속하지 않음
        if (Time.time - lastShiftTime < shiftCooldown) return;

        int currentGear = arcadeCar.currentGear;

        // 인덱스 범위 초과 방지
        if (currentGear > 0 && currentGear < aiGearShiftSpeeds.Length)
        {
            // 업시프트 (현재 속도가 다음 단수 요구 속도보다 높을 때)
            if (currentSpeed > aiGearShiftSpeeds[currentGear])
            {
                arcadeCar.ShiftUp();
                lastShiftTime = Time.time;
            }
            // 다운시프트 (현재 속도가 이전 단수 요구 속도보다 많이 낮아졌을 때)
            // -5f를 빼주는 이유는 속도 경계선에서 기어가 무한히 오르락내리락하는 현상(핑퐁)을 막기 위한 여유값입니다.
            else if (currentGear > 1 && currentSpeed < aiGearShiftSpeeds[currentGear - 1] - 5f)
            {
                arcadeCar.ShiftDown();
                lastShiftTime = Time.time;
            }
        }
    }

    private void StopCar()
    {
        if (arcadeCar != null)
        {
            arcadeCar.verticalInput = 0f;
            arcadeCar.horizontalInput = 0f;
        }
    }

    public int GetCurrentWaypointIndex() { return currentIdx; }

    public void SetCurrentWaypointIndex(int index)
    {
        if (waypoints != null && index < waypoints.Count)
        {
            currentIdx = index;
        }
    }

    public Vector3 GetTargetWaypointPosition()
    {
        if (waypoints != null && waypoints.Count > currentIdx)
            return waypoints[currentIdx].position;
        return transform.position;
    }
}