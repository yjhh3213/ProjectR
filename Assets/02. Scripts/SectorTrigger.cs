using UnityEngine;

public class SectorTrigger : MonoBehaviour
{
    [Header("섹터 번호 (0번부터 순서대로 입력)")]
    public int sectorIndex;

    private void OnTriggerEnter(Collider other)
    {
        // 1. 무언가 트리거에 닿았는지 확인하는 로그
        Debug.Log($"[SectorTrigger] 무언가 닿음! 오브젝트 이름: {other.name}, 태그: {other.tag}");

        // 2. 플레이어 태그 검사
        if (other.CompareTag("Player"))
        {
            TimeAttackManager taManager = FindAnyObjectByType<TimeAttackManager>();
            if (taManager != null)
            {
                Debug.Log($"[SectorTrigger] 타임어택 매니저 발견! 섹터 {sectorIndex} 통과 시도.");
                taManager.OnPassSector(sectorIndex);
            }
            else
            {
                Debug.LogError("[SectorTrigger] 씬 내에 TimeAttackManager가 존재하지 않습니다!");
            }
        }
        else
        {
            Debug.LogWarning($"[SectorTrigger] 닿은 오브젝트의 태그가 'Player'가 아닙니다. 현재 태그: {other.tag}");
        }
    }
}