using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour
{
    [Header("아이템 리스트")]
    public List<ItemData> allItems;
    public ItemData inventoryItem;
    public bool isRolling = false;

    [Header("3D 연출 및 컴포넌트")]
    public Transform itemSpawnPoint;
    private GameObject currentItemObject;
    private ArcadeCarController carController; // 차량 물리 제어용
    private MeshRenderer[] carRenderers;       // 색상 변경용 (대마왕/쉴드)

    [Header("현재 상태")]
    public int currentRank = 1; // 실시간 순위 시스템에서 업데이트되어야 함

    void Awake()
    {
        carController = GetComponent<ArcadeCarController>();
        carRenderers = GetComponentsInChildren<MeshRenderer>();
    }

    void Update()
    {
        // 왼쪽 Ctrl 키로 아이템 사용
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            UseItem();
        }
    }

    public void StartGetItemRoutine()
    {
        if (isRolling || inventoryItem != null) return;
        StartCoroutine(GetItemCoroutine());
    }

    IEnumerator GetItemCoroutine()
    {
        isRolling = true;
        yield return new WaitForSeconds(2f); // 4.6.1 연출 시간

        // 1. 등수별 타입 결정
        ItemType selectedType = DecideTypeByRank(currentRank);
        // 2. 타입 내 세부 아이템 결정
        inventoryItem = DecideSpecificItem(currentRank, selectedType);

        if (inventoryItem != null)
        {
            if (itemSpawnPoint != null && inventoryItem.worldPrefab != null)
            {
                if (currentItemObject != null) Destroy(currentItemObject);
                currentItemObject = Instantiate(inventoryItem.worldPrefab, itemSpawnPoint.position, itemSpawnPoint.rotation);
                currentItemObject.transform.SetParent(itemSpawnPoint);
                currentItemObject.transform.localPosition = Vector3.zero;
            }
        }
        isRolling = false;
    }

    // --- 4.2 ~ 4.5 기획서 확률 테이블 반영 ---
    ItemType DecideTypeByRank(int rank)
    {
        float r = Random.value; // 0.0 ~ 1.0

        switch (rank)
        {
            case 1: // 4.2 : 공 0 / 방 9 / 중 1
                if (r < 0.9f) return ItemType.Defense;
                else return ItemType.Neutral;

            case 2: // 4.3 : 공 2 / 방 6 / 중 2
                if (r < 0.2f) return ItemType.Attack;
                else if (r < 0.8f) return ItemType.Defense;
                else return ItemType.Neutral;

            case 3: // 4.4 : 공 4 / 방 2 / 중 4
                if (r < 0.4f) return ItemType.Attack;
                else if (r < 0.6f) return ItemType.Defense;
                else return ItemType.Neutral;

            case 4: // 4.5 : 공 4 / 방 0 / 중 6
                if (r < 0.4f) return ItemType.Attack;
                else return ItemType.Neutral;

            default: return ItemType.Neutral;
        }
    }

    ItemData DecideSpecificItem(int rank, ItemType type)
    {
        float r = Random.value;
        string targetName = "";

        if (type == ItemType.Attack)
        {
            // 4.3.1(2등 5:5), 4.4.1(3등 6:4), 4.5.1(4등 2:8)
            float missileProb = (rank == 2) ? 0.5f : (rank == 3) ? 0.6f : 0.2f;
            targetName = (r < missileProb) ? "Missile" : "Devil";
        }
        else if (type == ItemType.Defense)
        {
            // 4.2.1(1등 3:7), 4.3.2(2등 6:4), 4.4.2(3등 8:2)
            float shieldProb = (rank == 1) ? 0.3f : (rank == 2) ? 0.6f : 0.8f;
            targetName = (r < shieldProb) ? "Shield" : "Banana";
        }
        else
        {
            targetName = "Booster";
        }

        return allItems.Find(x => x.itemName == targetName);
    }

    public void UseItem()
    {
        if (inventoryItem == null || isRolling) return;

        if (inventoryItem.soundEffect != null)
            AudioSource.PlayClipAtPoint(inventoryItem.soundEffect, transform.position);

        if (currentItemObject != null)
        {
            Destroy(currentItemObject);
            currentItemObject = null;
        }

        switch (inventoryItem.itemName)
        {
            case "Missile": ExecuteMissile(); break;
            case "Devil": ExecuteDevil(); break;
            case "Booster": ExecuteBooster(); break;
            case "Shield": ExecuteShield(); break;
            case "Banana": ExecuteBanana(); break;
        }

        inventoryItem = null;
    }

    // --- 4.1.1 미사일: 바로 앞 사람 타겟팅 (추가 로직 필요) ---
    void ExecuteMissile()
    {
        Debug.Log("미사일 발사! (상대 2초 스턴 및 검은 연기 연출)");
        // Raycast나 순위 시스템을 이용해 앞 차를 찾아 'ApplyStun(2.0f)' 호출 로직 필요
    }

    // --- 4.1.2 대마왕: 전체 키 반전 및 보라색 연출 ---
    void ExecuteDevil()
    {
        Debug.Log("대마왕: 전체 키 반전 5~10초");
        // 모든 차량의 PlayerInput을 찾아 반전시키거나, 전역 매니저에서 호출
    }

    // --- 4.1.3 부스터: 최대 속도 즉시 도달 ---
    void ExecuteBooster()
    {
        if (carController != null) StartCoroutine(BoosterRoutine());
    }
    IEnumerator BoosterRoutine()
    {
        float originalSpeed = carController.maxSpeedKmh;
        carController.maxSpeedKmh *= 1.5f; // 예시: 최대속도 증가
        // 파란 배기구 이펙트 활성화 코드
        yield return new WaitForSeconds(3.5f); // 3~4초 지속
        carController.maxSpeedKmh = originalSpeed;
    }

    // --- 4.1.4 쉴드: 모든 아이템 면역 ---
    void ExecuteShield()
    {
        Debug.Log("쉴드 활성화: 3초간 무적");
        // IsInvincible = true 설정 및 파란색 머티리얼 적용
    }

    // --- 4.1.5 바나나: 밟으면 최소 속도 및 스키드마크 ---
    void ExecuteBanana()
    {
        Debug.Log("바나나 설치");
        // 현재 위치 뒤쪽에 바나나 프리팹 생성
    }
}