using UnityEngine;

public class RaceManager : MonoBehaviour
{
    [Header("차량 프리팹 (0:car1, 1:car2, 2:car4, 3:car8)")]
    public GameObject[] carPrefabs; // 0:car1, 1:car2, 2:car4, 3:car8

    [Header("카메라 세팅")]
    public CameraFollow cameraFollowScript; // 메인 카메라의 스크립트를 연결

    [Header("스폰 위치 세팅 (7.4)")]
    public Vector3 playerSpawnPos = new Vector3(0f, 1.3f, -40f);
    public Vector3[] aiSpawnPositions; // 나머지 3대 AI의 스폰 위치 설정

    void Start()
    {
        // 7.1: 로비에서 저장한 플레이어 차량 인덱스 불러오기 (기본값 0)
        int playerCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);

        SpawnVehicles(playerCarIndex);
    }

    private void SpawnVehicles(int playerIndex)
    {
        int aiSpawnIndex = 0;

        if (aiSpawnPositions.Length < 3)
        {
            Debug.LogWarning("경고: AI Spawn Positions 배열의 크기가 3보다 작습니다!");
        }

        for (int i = 0; i < carPrefabs.Length; i++)
        {
            if (i == playerIndex)
            {
                // 플레이어 생성
                GameObject playerCar = Instantiate(carPrefabs[i], playerSpawnPos, Quaternion.identity);

                // 핵심 해결책: 로비에서 꺼져있던 상태를 무시하고 강제로 활성화!
                playerCar.SetActive(true);

                playerCar.name = "PlayerCar_" + i;
                playerCar.AddComponent<PlayerInput>();

                if (cameraFollowScript != null) cameraFollowScript.target = playerCar.transform;
            }
            else
            {
                // AI 생성
                if (aiSpawnIndex < aiSpawnPositions.Length)
                {
                    GameObject aiCar = Instantiate(carPrefabs[i], aiSpawnPositions[aiSpawnIndex], Quaternion.identity);

                    // 핵심 해결책: AI 차량도 무조건 강제 활성화!
                    aiCar.SetActive(true);

                    aiCar.name = "AICar_" + i;
                    aiCar.AddComponent<AIInput>();
                    aiSpawnIndex++;
                }
            }
        }
    }
}