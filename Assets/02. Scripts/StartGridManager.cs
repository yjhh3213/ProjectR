using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class StartGridManager : MonoBehaviour
{
    [Header("차량 프리팹 (0:car1, 1:car2, 2:car4, 3:car8)")]
    public GameObject[] carPrefabs;

    [Header("카메라 세팅")]
    public CameraFollow cameraFollowScript;

    [Header("웨이포인트 설정")]
    public string waypointGroupName = "Waypoints_Group";
    private List<Transform> waypointList = new List<Transform>();
    public List<Transform> spawnPoints;

    private List<CarController> spawnedControllers = new List<CarController>();

    void Start()
    {
        SetupWaypoints();
        AssignGridPositions();

        int playerCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        SpawnVehicles(playerCarIndex);
    }

    void Update()
    {
        if (spawnedControllers.Count < 4) return;

        UpdateRanks();
    }

    private void UpdateRanks()
    {
        foreach (var c in spawnedControllers)
        {
            if (c == null || c.waypoints == null || c.waypoints.Count == 0) return;
        }

        // ★ [등수 버그 완벽 해결 버전]
        var sorted = spawnedControllers.OrderByDescending(car =>
        {
            int currentIdx = car.GetCurrentWaypointIndex();
            int safeIdx = Mathf.Clamp(currentIdx, 0, waypointList.Count - 1);
            Vector3 targetPos = waypointList[safeIdx].position;

            float distanceToNext = Vector3.Distance(car.transform.position, targetPos);

            // ★ [핵심 변경] 현재 바퀴의 인덱스 대신 '누적 통과 체크포인트 수(totalWaypointsPassed)'를 곱해줍니다.[cite: 1]
            // 바퀴를 넘어가더라도 이 점수는 계속 누적되므로 역전 버그가 완벽히 치료됩니다.[cite: 1]
            return (car.totalWaypointsPassed * 10000f) - distanceToNext;
        }).ToList();

        for (int i = 0; i < sorted.Count; i++)
        {
            if (sorted[i] != null)
            {
                var im = sorted[i].GetComponent<ItemManager>();
                if (im != null)
                {
                    im.currentRank = i + 1;
                }
            }
        }
    }

    private void SpawnVehicles(int playerIndex)
    {
        for (int i = 0; i < carPrefabs.Length; i++)
        {
            if (carPrefabs[i] == null) continue;

            GameObject spawnedCar = Instantiate(carPrefabs[i], spawnPoints[i].position, spawnPoints[i].rotation);
            spawnedCar.SetActive(true);

            CarController controller = spawnedCar.GetComponent<CarController>();

            // ===================================================================
            // ★ [추가] 실시간 등수 UI(RankManager)와의 연동을 위해 RaceParticipant 컴포넌트를 강제 확보합니다.
            // ===================================================================
            RaceParticipant participant = spawnedCar.GetComponent<RaceParticipant>();
            if (participant == null)
            {
                participant = spawnedCar.AddComponent<RaceParticipant>();
            }
            // ===================================================================

            if (controller != null)
            {
                controller.waypoints = waypointList;
                spawnedControllers.Add(controller);
            }

            if (i == playerIndex)
            {
                spawnedCar.name = "PlayerCar";

                if (spawnedCar.GetComponent<PlayerInput>() == null)
                {
                    spawnedCar.AddComponent<PlayerInput>();
                }

                ArcadeCarController AC = spawnedCar.GetComponent<ArcadeCarController>();
                SpeedometerUI ui = FindObjectOfType<SpeedometerUI>();
                if (ui != null) ui.SetupUI(AC);

                if (controller != null) controller.isAI = false;
                if (cameraFollowScript != null) cameraFollowScript.target = spawnedCar.transform;

                spawnedCar.tag = "Player";
            }
            else
            {
                spawnedCar.name = "AICar_" + i;
                if (controller != null) controller.isAI = true;

                spawnedCar.tag = "Player";
            }
        }
    }

    private void SetupWaypoints()
    {
        GameObject group = GameObject.Find(waypointGroupName);
        if (group != null)
        {
            waypointList.Clear();
            foreach (Transform child in group.transform)
            {
                waypointList.Add(child);
            }
        }
        else
        {
            Debug.LogError($"[StartGridManager] '{waypointGroupName}' 오시나 오브젝트를 씬에서 찾을 수 없습니다!");
        }
    }

    private void AssignGridPositions()
    {
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            int randomIndex = Random.Range(i, spawnPoints.Count);
            Transform temp = spawnPoints[i];
            spawnPoints[i] = spawnPoints[randomIndex];
            spawnPoints[randomIndex] = temp;
        }
    }
}