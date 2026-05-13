using UnityEngine;

// 각 차량(플레이어 및 AI)에 부착하여 현재 위치 정보를 저장합니다.
public class RaceParticipant : MonoBehaviour
{
    public int currentLap = 1;
    public int lastCheckpointIndex = -1;
    public float distanceToNextCheckpoint;
    public int finalRank;

    // ItemManager가 이 스크립트를 참조하여 순위를 가져갑니다.
    public ItemManager itemManager;

    void Awake()
    {
        itemManager = GetComponent<ItemManager>();
    }
}