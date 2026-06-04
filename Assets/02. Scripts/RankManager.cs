using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RankManager : MonoBehaviour
{
    public List<RaceParticipant> participants = new List<RaceParticipant>();
    public List<Transform> checkpoints; // 트랙에 배치된 체크포인트들

    [Header("HUD UI 연동")]
    public InGameHUDManager hudManager; // HUD 매니저 참조

    private Dictionary<RaceParticipant, ItemManager> itemManagerCache = new Dictionary<RaceParticipant, ItemManager>();

    void Start()
    {
        participants = new List<RaceParticipant>(FindObjectsOfType<RaceParticipant>());

        // 참가자 수 HUD 전달
        if (hudManager != null)
        {
            hudManager.SetupTotalPlayers(participants.Count);
        }

        // 최적화를 위한 ItemManager 캐싱 로직 (최적화 버전을 쓰셨다면)
        foreach (var p in participants)
        {
            ItemManager im = p.GetComponent<ItemManager>();
            if (im != null)
            {
                itemManagerCache.Add(p, im);
            }
        }
    }

    void Update()
    {
        UpdateRanks();
    }

    void UpdateRanks()
    {

        if (participants.Count == 0)
        {
            participants = FindObjectsByType<RaceParticipant>(FindObjectsSortMode.None).ToList();

            // 그래도 0명이면 아직 클론 생성이 덜 된 것이므로 에러 방지를 위해 이번 프레임은 넘깁니다.
            if (participants.Count == 0) return;

            // 참가자를 찾았으니 HUD에 총 인원수 갱신
            if (hudManager != null) hudManager.SetupTotalPlayers(participants.Count);
        }

        if (checkpoints == null || checkpoints.Count == 0)
        {
            Debug.LogError("RankManager에 체크포인트가 등록되지 않았습니다! 인스펙터를 확인하세요.");
            return;
        }

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

            // =======================================================
            // [여기에 디버깅용 로그 추가] 
            // 참가자들의 실제 이름과 HUD 연결 상태를 콘솔에 띄워봅니다.
            //Debug.Log($"[{i}번 순위] 오브젝트 이름: {sortedList[i].gameObject.name} | HUD 연결됨?: {hudManager != null}");
            // =======================================================

            // 정렬된 리스트 중 현재 등수를 부여받은 오브젝트의 태그가 "Player"인 경우 HUD UI를 갱신합니다.
            if (sortedList[i].gameObject.name.Contains("PlayerCar"))
            {
                //Debug.Log("PlayerCar 인식 성공! 거리 계산 진입"); // [성공 확인 로그]

                if (hudManager != null)
                {
                    hudManager.UpdateRank(rank);

                    // --- [거리 계산 로직] ---
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

                    if (i == 0 && sortedList.Count > 1) { displayDistance = distBehind; }
                    else if (i == sortedList.Count - 1 && sortedList.Count > 1) { displayDistance = distAhead; }
                    else if (sortedList.Count > 2)
                    {
                        if (distAhead >= 50f && distBehind >= 50f) { displayDistance = distAhead; }
                        else { displayDistance = Mathf.Min(distAhead, distBehind); }
                    }

                    //Debug.Log($"계산된 최종 거리: {displayDistance}m"); // [값 확인 로그]

                    // 107번째 줄
                    hudManager.UpdateDistanceDisplay(displayDistance);
                }
            }
        }
    }
}