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
        // 1. 씬에 배치된 RankManager(심판)를 미리 찾습니다.
        RankManager rankManager = FindObjectOfType<RankManager>();

        for (int i = 0; i < carPrefabs.Length; i++)
        {
            if (carPrefabs[i] == null) continue;

            // 2. 차량 생성 (설정된 스폰 포인트 위치에 생성)
            GameObject spawnedCar = Instantiate(carPrefabs[i], spawnPoints[i].position, spawnPoints[i].rotation);

            // 로비에서 꺼져있던 프리팹일 수 있으므로 강제 활성화
            spawnedCar.SetActive(true);

            // 3. 순위 시스템 등록 (핵심 수정 사항)
            // 생성된 자동차에 붙은 RaceParticipant 컴포넌트를 가져와 RankManager의 리스트에 넣습니다.
            RaceParticipant participant = spawnedCar.GetComponent<RaceParticipant>();
            if (rankManager != null && participant != null)
            {
                rankManager.participants.Add(participant); // 심판 리스트에 자동차 등록
                Debug.Log($"{spawnedCar.name}가 순위 시스템(RankManager)에 등록되었습니다.");
            }
            else
            {
                Debug.LogWarning($"{spawnedCar.name}에 RaceParticipant가 없거나 RankManager를 찾을 수 없습니다.");
            }

            // 4. 자동차 컨트롤러 참조 가져오기
            CarController controller = spawnedCar.GetComponent<CarController>();

            if (i == playerIndex)
            {
                // --- 플레이어(나) 설정 ---
                spawnedCar.name = "PlayerCar_" + i;

                // 플레이어 조작 입력을 위해 PlayerInput 컴포넌트 추가
                spawnedCar.AddComponent<PlayerInput>();

                // UI 및 카메라 연결
                ArcadeCarController AC = spawnedCar.GetComponent<ArcadeCarController>();
                SpeedometerUI ui = FindObjectOfType<SpeedometerUI>();
                if (ui != null)
                {
                    ui.SetupUI(AC);
                }

                if (controller != null) controller.isAI = false; // 플레이어가 직접 조종

                if (cameraFollowScript != null)
                {
                    cameraFollowScript.target = spawnedCar.transform;
                }

                Debug.Log(carPrefabs[i].name + "가 플레이어로 배정되었습니다.");
            }
            else
            {
                // --- AI 설정 ---
                spawnedCar.name = "AICar_" + i;

                if (controller != null)
                {
                    controller.isAI = true; // AI가 조종
                    controller.waypoints = waypointList; // AI 주행 경로 전달
                }

                Debug.Log(carPrefabs[i].name + "가 AI로 배정되었습니다.");
            }
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