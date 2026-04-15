using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour
{
    [Header("아이템 리스트")]
    public List<ItemData> allItems; // 모든 ItemData 등록
    public ItemData inventoryItem;  // 현재 소지 아이템
    public bool isRolling = false;  // 4.6.2 상태 확인

    [Header("3D 연출 설정 (추가된 부분)")]
    public Transform itemSpawnPoint;    // 자동차 위 빈 오브젝트 연결
    private GameObject currentItemObject; // 자동차 위 생성된 물체 저장 변수

    [Header("현재 상태")]
    public int currentRank = 1;     // 레이싱 시스템 연동

    // 업데이트 문에서 사용 키 입력 감지 (추가)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl)) // 왼쪽 Ctrl 키로 사용
        {
            UseItem();
        }
    }

    // --- 아이템 획득 로직 ---
    public void StartGetItemRoutine()
    {
        if (isRolling || inventoryItem != null) return;
        StartCoroutine(GetItemCoroutine());
    }

    IEnumerator GetItemCoroutine()
    {
        isRolling = true;
        Debug.Log("아이템 추첨 중...");

        // 4.6.1 연출시간(2초)
        yield return new WaitForSeconds(2f);

        // 1. 등수별 타입 결정
        ItemType selectedType = DecideTypeByRank(currentRank);
        // 2. 타입 내 세부 아이템 결정
        inventoryItem = DecideSpecificItem(currentRank, selectedType);

        if (inventoryItem != null)
        {
            Debug.Log($"아이템 결정: {inventoryItem.itemName}");

            // --- [수정된 부분] 3D 프리팹 생성 및 지붕 부착 ---
            if (itemSpawnPoint != null && inventoryItem.worldPrefab != null)
            {
                // 이미 머리 위에 뭐가 있다면 삭제
                if (currentItemObject != null) Destroy(currentItemObject);

                // 프리팹 생성
                currentItemObject = Instantiate(inventoryItem.worldPrefab, itemSpawnPoint.position, itemSpawnPoint.rotation);

                // 자동차의 자식으로 설정 (그래야 같이 움직임)
                currentItemObject.transform.SetParent(itemSpawnPoint);

                // 위치 초기화 (스폰포인트 정중앙)
                currentItemObject.transform.localPosition = Vector3.zero;
            }
        }

        isRolling = false;
    }

    // --- 4.2 ~ 4.5 등수별 확률 변동 테이블 (이전과 동일) ---
    ItemType DecideTypeByRank(int rank)
    {
        float r = Random.Range(0, 10);
        switch (rank)
        {
            case 1: return (r < 9) ? ItemType.Defense : ItemType.Neutral;
            case 2: return (r < 2) ? ItemType.Attack : (r < 8) ? ItemType.Defense : ItemType.Neutral;
            case 3: return (r < 4) ? ItemType.Attack : (r < 6) ? ItemType.Defense : ItemType.Neutral;
            case 4: return (r < 4) ? ItemType.Attack : ItemType.Neutral;
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

        // 사운드 재생
        if (inventoryItem.soundEffect != null)
            AudioSource.PlayClipAtPoint(inventoryItem.soundEffect, transform.position);

        // --- [수정된 부분] 사용 시 머리 위 3D 오브젝트 삭제 ---
        if (currentItemObject != null)
        {
            Destroy(currentItemObject);
            currentItemObject = null;
        }

        // 아이템별 효과 함수 호출
        switch (inventoryItem.itemName)
        {
            case "Missile": ExecuteMissile(); break;
            case "Devil": ExecuteDevil(); break;
            case "Booster": ExecuteBooster(); break;
            case "Shield": ExecuteShield(); break;
            case "Banana": ExecuteBanana(); break;
        }

        inventoryItem = null; // 소지 아이템 데이터 초기화
    }

    // --- 개별 효과 구현 ---
    void ExecuteMissile() { Debug.Log("미사일 발사 기능 실행"); }
    void ExecuteDevil() { Debug.Log("대마왕 효과 실행"); }
    void ExecuteBooster() { Debug.Log("부스터 효과 실행"); }
    void ExecuteShield() { Debug.Log("쉴드 효과 실행"); }
    void ExecuteBanana() { Debug.Log("바나나 설치 기능 실행"); }
}