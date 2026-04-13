using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour
{
    [Header("아이템 리스트")]
    public List<ItemData> allItems; // 모든 ItemData 등록
    public ItemData inventoryItem;  // 현재 소지 아이템
    public bool isRolling = false;  // 4.6.2 상태 확인

    [Header("현재 상태")]
    public int currentRank = 1;     // 레이싱 시스템 연동

    // --- 아이템 획득 로직 ---
    public void StartGetItemRoutine()
    {
        StartCoroutine(GetItemCoroutine());
    }

    IEnumerator GetItemCoroutine()
    {
        isRolling = true;
        // 4.6.1 연출: 지붕 위 큐브에서 그림이 순차적으로 바뀌는 연출 (생략)
        yield return new WaitForSeconds(2f);

        // 1. 등수별 타입 결정 (A, D, N)
        ItemType selectedType = DecideTypeByRank(currentRank);
        // 2. 타입 내 세부 아이템 결정
        inventoryItem = DecideSpecificItem(currentRank, selectedType);

        Debug.Log($"아이템 결정: {inventoryItem.itemName}");
        isRolling = false;
    }

    // --- 4.2 ~ 4.5 등수별 확률 변동 테이블 ---
    ItemType DecideTypeByRank(int rank)
    {
        float r = Random.Range(0, 10); // 0~9 범위
        switch (rank)
        {
            case 1: return (r < 9) ? ItemType.Defense : ItemType.Neutral; // D:9, N:1
            case 2: return (r < 2) ? ItemType.Attack : (r < 8) ? ItemType.Defense : ItemType.Neutral; // A:2, D:6, N:2
            case 3: return (r < 4) ? ItemType.Attack : (r < 6) ? ItemType.Defense : ItemType.Neutral; // A:4, D:2, N:4
            case 4: return (r < 4) ? ItemType.Attack : ItemType.Neutral; // A:4, N:6
            default: return ItemType.Neutral;
        }
    }

    ItemData DecideSpecificItem(int rank, ItemType type)
    {
        float r = Random.value;
        string targetName = "";

        if (type == ItemType.Attack)
        {
            float missileProb = (rank == 2) ? 0.5f : (rank == 3) ? 0.6f : 0.2f;
            targetName = (r < missileProb) ? "Missile" : "Devil";
        }
        else if (type == ItemType.Defense)
        {
            float shieldProb = (rank == 1) ? 0.3f : (rank == 2) ? 0.6f : 0.8f;
            targetName = (r < shieldProb) ? "Shield" : "Banana";
        }
        else { targetName = "Booster"; }

        return allItems.Find(x => x.itemName == targetName);
    }

    // --- 4.1 아이템 실행 로직 ---
    public void UseItem()
    {
        if (inventoryItem == null || isRolling) return;

        // 사운드 재생 (2.2.x.3)
        AudioSource.PlayClipAtPoint(inventoryItem.soundEffect, transform.position);

        switch (inventoryItem.itemName)
        {
            case "Missile": ExecuteMissile(); break;
            case "Devil": ExecuteDevil(); break;
            case "Booster": ExecuteBooster(); break;
            case "Shield": ExecuteShield(); break;
            case "Banana": ExecuteBanana(); break;
        }
        inventoryItem = null; // 사용 후 제거
    }

    // --- 개별 효과 구현 (4.1.1 ~ 4.1.5) ---
    void ExecuteMissile() { /* 앞 차량 조준, 2초 스턴, 검은 연기 연출 */ }
    void ExecuteDevil() { /* 전원 보라색 오버레이, 키 반전 5~10초 */ }
    void ExecuteBooster() { /* 즉시 최대 속도, 3~4초 배기구 파란 이펙트 */ }
    void ExecuteShield() { /* 3초간 모든 충돌/아이템 면역, 파란 보호막 생성 */ }
    void ExecuteBanana() { /* 후방 설치, 밟으면 스키드마크와 함께 속도 0 */ }
}