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