using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("추적 대상")]
    public Transform target; // 매니저가 여기에 플레이어 차를 넣어줄 거야

    [Header("카메라 세팅")]
    public Vector3 offset = new Vector3(0f, 3f, -7f); // 차체 중심으로부터의 거리 (위로 3, 뒤로 7)
    public float followSpeed = 10f; // 카메라가 쫓아가는 속도
    public float lookSpeed = 10f;   // 카메라가 고개를 돌리는 속도

    void FixedUpdate()
    {
        // 타겟이 없으면(아직 차가 선택 안됐으면) 작동 안 함
        if (target == null) return;

        // 1. 카메라가 가야 할 목표 위치 계산 (차의 로컬 좌표 기준)
        Vector3 targetPosition = target.TransformPoint(offset);

        // 2. 현재 위치에서 목표 위치로 부드럽게 이동 (Lerp)
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.fixedDeltaTime);

        // 3. 차의 방향을 부드럽게 바라보게 회전 (Slerp)
        // (단순히 LookAt을 쓰면 멀미가 날 수 있어서 부드럽게 회전시킴)
        Vector3 lookDirection = target.position - transform.position;
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookSpeed * Time.fixedDeltaTime);
        }
    }
}
