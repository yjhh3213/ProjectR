using System.Collections.Generic;
using UnityEngine;

public class MinimapManager : MonoBehaviour
{
    [Header("UI 점 아이콘 (미리 할당)")]
    public RectTransform playerDot; // 인스펙터에서 Player_Dot 할당
    public RectTransform[] aiDots;  // 인스펙터에서 AI_Dot 1~3 할당

    private Transform playerCar;
    private List<Transform> aiCars = new List<Transform>();

    // StartGridManager에서 차를 다 만든 후 호출할 함수
    public void SetupMinimap(Transform pCar, List<Transform> aCars)
    {
        playerCar = pCar;
        aiCars = aCars;

        Debug.Log("미니맵: 모든 차량 연결 완료!");
    }

    [Header("맵 동기화 세팅 (매우 중요!)")]
    // 3D 맵의 중심점과 크기를 2D UI 크기(300x300)에 맞게 변환하기 위한 수치입니다.
    // 맵을 테스트하며 이 수치를 조절해야 정확히 도로 위를 달립니다.
    public float worldMapSize = 500f; // 실제 3D 맵의 전체 가로/세로 길이
    public float uiMapSize = 300f;    // UI 상의 미니맵 크기 (유저 기획: 300)
    public Vector3 worldCenterOffset; // 3D 맵의 정중앙 좌표 (0,0,0이 아닐 경우 조정)

    [Header("미니맵 이미지 보정 (영점 조절)")]
    public Vector2 uiOffset = new Vector2(160f, 50f);

    void Update()
    {
        // 플레이어 위치 업데이트
        if (playerCar != null)
        {
            UpdateDotPosition(playerCar, playerDot);
        }

        // AI 위치 업데이트
        for (int i = 0; i < aiCars.Count; i++)
        {
            if (aiCars[i] != null && i < aiDots.Length)
            {
                UpdateDotPosition(aiCars[i], aiDots[i]);
            }
        }
    }

    // 핵심: 3D 좌표를 2D UI 좌표로 변환하는 마법 공식
    private void UpdateDotPosition(Transform car, RectTransform dot)
    {
        // 1. 맵의 중심점을 기준으로 차가 얼마나 떨어져 있는지 계산
        Vector3 relativePosition = car.position - worldCenterOffset;

        // 2. 3D 크기를 2D UI 크기로 압축하는 비율 계산
        float scaleRatio = uiMapSize / worldMapSize;

        // 3. X(가로)와 Z(세로) 좌표를 UI의 X, Y로 변환
        float mapX = relativePosition.x * scaleRatio;
        float mapY = relativePosition.z * scaleRatio;

        // 4. 점의 위치를 이동!
        dot.anchoredPosition = new Vector2(mapX, mapY) + uiOffset;
    }
}