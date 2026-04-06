using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIInput : MonoBehaviour
{
    private ArcadeCarController controller;
    public Transform targetWaypoint; // AI가 쫓아갈 목표 지점

    void Start()
    {
        controller = GetComponent<ArcadeCarController>();
    }

    void Update()
    {
        if (targetWaypoint == null) return;

        // 1. 목표까지의 방향 계산
        Vector3 relativePos = transform.InverseTransformPoint(targetWaypoint.position);

        // 2. 조향(Turn) 결정: 목표가 왼쪽에 있으면 -1, 오른쪽에 있으면 100
        controller.turnInput = relativePos.x / relativePos.magnitude;

        // 3. 가속(Move) 결정: 일단 무조건 전진 (1.0)
        controller.moveInput = 1.0f;

        // 4. 거리가 가까워지면 다음 웨이포인트로 넘어가게 하는 로직이 추가로 필요함
    }
}
