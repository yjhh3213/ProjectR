using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RankManager : MonoBehaviour
{
    public List<RaceParticipant> participants = new List<RaceParticipant>();
    public List<Transform> checkpoints; // 트랙에 배치된 체크포인트들

    [Header("HUD UI 연동")]
    public InGameHUDManager hudManager; // HUD 매니저 참조

    void Start()
    {
        // 게임 시작 시점에 참가자 리스트의 개수를 HUD에 보냅니다. (예: 4명 가입 시 1 / 4 표시)
        if (hudManager != null)
        {
            hudManager.SetupTotalPlayers(participants.Count);
        }
    }

    void Update()
    {
        UpdateRanks();
    }

    void UpdateRanks()
    {
        // 1. 다음 체크포인트까지의 거리 업데이트
        foreach (var p in participants)
        {
            int nextIdx = (p.lastCheckpointIndex + 1) % checkpoints.Count;
            p.distanceToNextCheckpoint = Vector3.Distance(p.transform.position, checkpoints[nextIdx].position);
        }

        // 2. 정렬 (바퀴 수 내림차순 -> 체크포인트 내림차순 -> 거리 오름차순)
        var sortedList = participants
            .OrderByDescending(p => p.currentLap)
            .ThenByDescending(p => p.lastCheckpointIndex)
            .ThenBy(p => p.distanceToNextCheckpoint)
            .ToList();

        // 3. 순위 할당 및 ItemManager, HUD에 전달
        for (int i = 0; i < sortedList.Count; i++)
        {
            int rank = i + 1; // 1등부터 시작
            sortedList[i].finalRank = rank;

            // 중요: 자동차에 붙은 ItemManager를 찾아 등수를 직접 꽂아줍니다.
            ItemManager im = sortedList[i].GetComponent<ItemManager>();
            if (im != null)
            {
                im.currentRank = rank;
            }

            // 정렬된 리스트 중 현재 등수를 부여받은 오브젝트의 태그가 "Player"인 경우 HUD UI를 갱신합니다.
            if (sortedList[i].gameObject.CompareTag("Player"))
            {
                if (hudManager != null)
                {
                    hudManager.UpdateRank(rank);

                    // --- [추가된 거리 계산 로직] ---
                    float distAhead = float.MaxValue;
                    float distBehind = float.MaxValue;
                    Vector3 playerPos = sortedList[i].transform.position;

                    // 앞차가 있는 경우 (1등이 아님)
                    if (i > 0)
                    {
                        distAhead = Vector3.Distance(playerPos, sortedList[i - 1].transform.position);
                    }

                    // 뒤차가 있는 경우 (꼴찌가 아님)
                    if (i < sortedList.Count - 1)
                    {
                        distBehind = Vector3.Distance(playerPos, sortedList[i + 1].transform.position);
                    }

                    float displayDistance = 0f;

                    if (i == 0 && sortedList.Count > 1)
                    {
                        // 플레이어가 1등일 때: 무조건 뒤차와의 거리 표시
                        displayDistance = distBehind;
                    }
                    else if (i == sortedList.Count - 1 && sortedList.Count > 1)
                    {
                        // 플레이어가 꼴찌일 때: 무조건 앞차와의 거리 표시
                        displayDistance = distAhead;
                    }
                    else if (sortedList.Count > 2)
                    {
                        // 플레이어가 중간 등수일 때
                        // 앞차와 뒷차 모두 거리가 50 이상일 경우: 앞차 거리 표시 (추격 상황)
                        if (distAhead >= 50f && distBehind >= 50f)
                        {
                            displayDistance = distAhead;
                        }
                        // 그 외: 앞차, 뒷차 중 제일 가까운 거리 표시
                        else
                        {
                            displayDistance = Mathf.Min(distAhead, distBehind);
                        }
                    }

                    hudManager.UpdateDistanceDisplay(displayDistance);
                }
            }
        }
    }
}