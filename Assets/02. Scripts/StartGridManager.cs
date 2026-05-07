using System.Collections.Generic;
using UnityEngine;

public class StartGridManager : MonoBehaviour
{
    //-------------------------------- 추가
    [Header("차량 프리팹 (0:car1, 1:car2, 2:car4, 3:car8)")]
    public GameObject[] carPrefabs; // 0:car1, 1:car2, 2:car4, 3:car8

    [Header("카메라 세팅")]
    public CameraFollow cameraFollowScript; // 메인 카메라의 스크립트를 연결

    //준석이 파트
    [Header("웨이포인트 설정")]
    // 165개의 웨이포인트를 담고 있는 부모 오브젝트의 이름을 적어주세요.
    public string waypointGroupName = "Waypoints_Group";
    private List<Transform> waypointList = new List<Transform>();
    //--------------------------------
    public List<Transform> spawnPoints;

    void Start()
    {

        //준석이 파트
        // 1. 165개의 웨이포인트를 자동으로 리스트에 담기
        SetupWaypoints();

        AssignGridPositions();

        // 7.1: 로비에서 저장한 플레이어 차량 인덱스 불러오기 (기본값 0)
        int playerCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);

        SpawnVehicles(playerCarIndex);
    }

    private void SpawnVehicles(int playerIndex)
    {
        Transform playerCarTransform = null;
        List<Transform> aiCarTransforms = new List<Transform>();

        for (int i = 0; i < carPrefabs.Length; i++)
        {
            if (carPrefabs[i] == null) continue;

            // 1. 차량 생성 (에러가 있던 빈칸 부분에 섞인 스폰 위치 적용)
            GameObject spawnedCar = Instantiate(carPrefabs[i], spawnPoints[i].position, spawnPoints[i].rotation);

            // 로비에서 꺼져있던 프리팹일 수 있으므로 강제 활성화
            spawnedCar.SetActive(true);

            // 2. 방금 생성된 복제본 차량의 CarController 가져오기
            CarController controller = spawnedCar.GetComponent<CarController>();

            if (i == playerIndex)
            {
                // --- 플레이어(나) 설정 ---
                spawnedCar.name = "PlayerCar_" + i;

                playerCarTransform = spawnedCar.transform;
                // 여기서 PlayerInput을 복제본에 동적으로 추가해줍니다!
                spawnedCar.AddComponent<PlayerInput>();
                ArcadeCarController AC = spawnedCar.GetComponent<ArcadeCarController>();

                SpeedometerUI ui = FindObjectOfType<SpeedometerUI>();
                if (ui != null)
                {
                    ui.SetupUI(AC);
                }
                if (controller != null) controller.isAI = false; // 뇌 제어권: 플레이어
                if (cameraFollowScript != null)
                {
                    cameraFollowScript.target = spawnedCar.transform;
                }
                else
                {
                    Debug.LogError("RaceManager에 CameraFollow 스크립트가 연결되지 않았습니다!");
                }
                Debug.Log(carPrefabs[i].name + "가 플레이어로 배정되었습니다.");
            }
            else
            {
                // --- AI 설정 (친구분 파트 정상화) ---
                spawnedCar.name = "AICar_" + i;

                aiCarTransforms.Add(spawnedCar.transform);

                if (controller != null)
                {
                    controller.isAI = true; // 뇌 제어권: AI
                    controller.waypoints = waypointList; // 웨이포인트 경로 전달
                }

                // 만약 AI도 PlayerInput처럼 별도의 스크립트가 필요하다면 아래 줄의 주석을 해제하세요.
                // spawnedCar.AddComponent<AIInput>(); 

                Debug.Log(carPrefabs[i].name + "가 AI로 배정되었습니다.");
            }
        }

        MinimapManager minimap = FindObjectOfType<MinimapManager>();
        if (minimap != null)
        {
            minimap.SetupMinimap(playerCarTransform, aiCarTransforms);
        }
    }
    //준석이 파트
    private void SetupWaypoints()
    {
        // 이름으로 부모 오브젝트를 찾습니다.
        GameObject group = GameObject.Find(waypointGroupName);
        if (group != null)
        {
            // 부모 안의 모든 자식(웨이포인트)을 순서대로 리스트에 추가합니다.
            foreach (Transform child in group.transform)
            {
                waypointList.Add(child);
            }
            Debug.Log(waypointList.Count + "개의 웨이포인트를 등록했습니다.");
        }
        else
        {
            Debug.LogError(waypointGroupName + " 오브젝트를 찾을 수 없습니다! 이름을 확인해주세요.");
        }
    }

    private void AssignGridPositions()
    {
        // 1. 그리드 위치 리스트를 랜덤하게 섞기 (Fisher-Yates Shuffle)
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            int randomIndex = Random.Range(i, spawnPoints.Count);
            Transform temp = spawnPoints[i];
            spawnPoints[i] = spawnPoints[randomIndex];
            spawnPoints[randomIndex] = temp;
        }
    }
}