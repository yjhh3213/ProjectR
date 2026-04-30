using UnityEngine;

public class MinimapManager : MonoBehaviour
{
    [Header("UI 연결 (점 아이콘)")]
    public RectTransform playerDot;
    public RectTransform[] aiDots;

    [Header("추적할 실제 3D 자동차들")]
    public Transform playerCar;
    public Transform[] aiCars;

    [Header("맵 동기화 세팅 (매우 중요!)")]
    // 3D 맵의 중심점과 크기를 2D UI 크기(300x300)에 맞게 변환하기 위한 수치입니다.
    // 맵을 테스트하며 이 수치를 조절해야 정확히 도로 위를 달립니다.
    public float worldMapSize = 500f; // 실제 3D 맵의 전체 가로/세로 길이
    public float uiMapSize = 300f;    // UI 상의 미니맵 크기 (유저 기획: 300)
    public Vector3 worldCenterOffset; // 3D 맵의 정중앙 좌표 (0,0,0이 아닐 경우 조정)

    // 외부(StartGridManager)에서 차가 스폰되면 이 함수를 불러서 연결해줍니다.
    public void SetupMinimap(Transform pCar, Transform[] aCars)
    {
        playerCar = pCar;
        aiCars = aCars;

        // 플레이어 점 색상 설정 (예: 노란색이나 연두색)
        playerDot.GetComponent<UnityEngine.UI.Image>().color = Color.green;

        // AI 점 색상 설정 (예: 붉은색 계열)
        foreach (var dot in aiDots)
        {
            dot.GetComponent<UnityEngine.UI.Image>().color = Color.red;
        }
    }

    void Update()
    {
        if (playerCar != null)
        {
            UpdateDotPosition(playerCar, playerDot);
        }

        for (int i = 0; i < aiCars.Length; i++)
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
        dot.anchoredPosition = new Vector2(mapX, mapY);
    }
}