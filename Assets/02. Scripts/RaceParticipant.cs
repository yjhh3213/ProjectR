using UnityEngine;
using System.Collections.Generic;

public class RaceParticipant : MonoBehaviour
{
    [Header("레이스 시스템 변수 (프레임 단위 갱신)")]
    public int currentLap = 1;
    public float distanceToNextCheckpoint;
    public int lastCheckpointIndex = -1;
    public int finalRank;

    [Header("낙하 리스폰 설정")]
    [SerializeField] private float fallThresholdY = -20f;
    [SerializeField] private string deathZoneTag = "DeathZone";

    private Vector3 lastCheckpointPosition;
    private Quaternion lastCheckpointRotation;
    private Rigidbody rb;

    [HideInInspector] public int currentRank = 4;
    [HideInInspector] public List<Transform> waypoints = new List<Transform>();
    private CarController carController;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        carController = GetComponent<CarController>();

        // 시작할 때의 처음 위치와 회전을 초기 리스폰 값으로 고정
        lastCheckpointPosition = transform.position;
        lastCheckpointRotation = transform.rotation;
    }

    private void Update()
    {
        if (transform.position.y < fallThresholdY)
        {
            Respawn();
        }

        UpdateDistanceToNextTarget();
    }

    private void UpdateDistanceToNextTarget()
    {
        if (carController != null && carController.waypoints != null && carController.waypoints.Count > 0)
        {
            float minDst = float.MaxValue;
            foreach (var wp in carController.waypoints)
            {
                if (wp == null) continue;
                float dst = Vector3.Distance(transform.position, wp.position);
                if (dst < minDst) minDst = dst;
            }
            distanceToNextCheckpoint = minDst;
        }
        else if (waypoints != null && waypoints.Count > 0)
        {
            float minDst = float.MaxValue;
            foreach (var wp in waypoints)
            {
                if (wp == null) continue;
                float dst = Vector3.Distance(transform.position, wp.position);
                if (dst < minDst) minDst = dst;
            }
            distanceToNextCheckpoint = minDst;
        }
    }

    // Checkpoint 스크립트에서 안전하게 호출해 줄 데이터 설정용 메서드
    public void SetNewCheckpoint(int index, Vector3 position, Quaternion rotation)
    {
        lastCheckpointIndex = index;
        lastCheckpointPosition = position;
        lastCheckpointRotation = rotation;
        Debug.Log($"[체크포인트 저장 성공] {gameObject.name} -> {index}번 지점 백업 완료");
    }

    private void OnTriggerEnter(Collider other)
    {
        // 낙하 구역 충돌 체크
        if (other.gameObject.tag == deathZoneTag)
        {
            Respawn();
        }
    }

    public void Respawn()
    {
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 바닥 파묻힘을 방지하기 위해 위로 0.6m 보정하여 이동
        transform.position = lastCheckpointPosition + (Vector3.up * 0.6f);
        transform.rotation = lastCheckpointRotation;

        Debug.Log($"[리스폰 완료] {gameObject.name}가 {lastCheckpointIndex}번 체크포인트로 복귀했습니다.");
    }
}