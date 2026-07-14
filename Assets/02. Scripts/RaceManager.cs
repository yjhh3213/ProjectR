using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RaceManager : MonoBehaviour
{
    [Header("트랙에 배치된 4대의 차량")]
    public GameObject[] raceCars;

    [Header("웨이포인트 설정")]
    public string waypointGroupName = "Waypoints_Group";

    private List<Transform> waypointList = new List<Transform>();
    private List<RaceParticipant> participantComponents = new List<RaceParticipant>();

    void Start()
    {
        SetupWaypoints();
        AssignBrainsToCars();
        CacheParticipants();
    }

    void Update()
    {
        // 매 프레임(프레임 단위) 순위 재집계 연산 실행
        CalculateRealtimeRankings();
    }

    private void SetupWaypoints()
    {
        GameObject group = GameObject.Find(waypointGroupName);
        if (group != null)
        {
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

    private void AssignBrainsToCars()
    {
        int playerCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);

        for (int i = 0; i < raceCars.Length; i++)
        {
            GameObject currentCar = raceCars[i];
            if (currentCar == null) continue;

            CarController controller = currentCar.GetComponent<CarController>();

            if (i == playerCarIndex)
            {
                if (controller != null) controller.isAI = false;

                CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
                if (camFollow != null) camFollow.target = currentCar.transform;
            }
            else
            {
                if (controller != null)
                {
                    controller.isAI = true;
                    controller.waypoints = waypointList;
                }
            }
        }
    }

    private void CacheParticipants()
    {
        foreach (GameObject car in raceCars)
        {
            if (car != null)
            {
                RaceParticipant p = car.GetComponent<RaceParticipant>();
                if (p == null) p = car.AddComponent<RaceParticipant>();

                participantComponents.Add(p);
            }
        }
    }

    private void CalculateRealtimeRankings()
    {
        if (participantComponents.Count == 0) return;

        // 프레임 단위 다중 스코어 연산 정렬법
        var sortedList = participantComponents
            .OrderByDescending(p =>
            {
                // 1. 바퀴 수 (최우선순위 가중치)
                float lapScore = p.currentLap * 1000000f;

                // 2. 마지막 통과 상자 인덱스 (차선순위 가중치)
                float checkpointScore = Mathf.Max(0, p.lastCheckpointIndex) * 10000f;

                // 3. 다음 상자까지 남은 거리 패널티 (동률일 때 미세한 거리 앞선 차량 우대)
                float distancePenalty = p.distanceToNextCheckpoint;

                // 세 가지 요소를 연산하여 최종 진행 스코어 리턴
                return lapScore + checkpointScore - distancePenalty;
            })
            .ToList();

        // 실시간 연산된 등수를 정렬 순서대로 1~4등 주입
        for (int i = 0; i < sortedList.Count; i++)
        {
            int assignedRank = i + 1;
            sortedList[i].currentRank = assignedRank;

            // 아이템 확률에 영향을 주는 ItemManager에도 실시간 동기화
            ItemManager itemManager = sortedList[i].GetComponent<ItemManager>();
            if (itemManager != null)
            {
                itemManager.currentRank = assignedRank;
            }
        }
    }
}