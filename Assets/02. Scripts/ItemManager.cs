using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour
{
    [Header("연결된 컴포넌트")]
    private CarController carController;
    private ArcadeCarController arcadeCar;
    private MeshRenderer[] carRenderers;

    [Header("아이템 데이터 & 상태")]
    public List<ItemData> allItems;
    public ItemData inventoryItem;
    public bool isRolling = false;

    [Header("실시간 등수 (StartGridManager 연동)")]
    public int currentRank = 1;

    [Header("이펙트 설정")]
    public GameObject smokeEffect;
    public GameObject blueBoostEffect;
    public GameObject bananaPrefab;
    public Transform itemSpawnPoint;
    private GameObject currentItemObject;

    public bool IsShielded { get; private set; } = false;
    public bool IsDevilAffected { get; private set; } = false;
    public bool IsStunned { get; private set; } = false;

    void Start()
    {
        carController = GetComponent<CarController>();
        arcadeCar = GetComponent<ArcadeCarController>();
        carRenderers = GetComponentsInChildren<MeshRenderer>();
        if (smokeEffect) smokeEffect.SetActive(false);
        if (blueBoostEffect) blueBoostEffect.SetActive(false);
    }

    void Update()
    {
        if (carController != null && !carController.isAI)
        {
            if (Input.GetKeyDown(KeyCode.LeftControl)) UseItem();
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
        yield return new WaitForSeconds(2f);

        ItemType selectedType = DecideTypeByRank(currentRank);
        inventoryItem = DecideSpecificItem(currentRank, selectedType);

        if (inventoryItem != null)
        {
            Debug.Log($"<color=lime>[아이템 획득]</color> 이름: {gameObject.name} | 실시간 등수: {currentRank}등 | 뽑힌 아이템: {inventoryItem.itemName}");

            if (itemSpawnPoint != null && inventoryItem.worldPrefab != null)
            {
                if (currentItemObject != null) Destroy(currentItemObject);
                currentItemObject = Instantiate(inventoryItem.worldPrefab, itemSpawnPoint.position, itemSpawnPoint.rotation);
                currentItemObject.transform.SetParent(itemSpawnPoint);
                currentItemObject.transform.localPosition = Vector3.zero;
            }
        }
        isRolling = false;

        if (carController != null && carController.isAI && inventoryItem != null)
        {
            StartCoroutine(AIWaitAndUseItem(5f));
        }
    }

    IEnumerator AIWaitAndUseItem(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (inventoryItem != null) UseItem();
    }

    public void UseItem()
    {
        if (inventoryItem == null || isRolling) return;
        if (inventoryItem.soundEffect != null) AudioSource.PlayClipAtPoint(inventoryItem.soundEffect, transform.position);
        if (currentItemObject != null) { Destroy(currentItemObject); currentItemObject = null; }

        switch (inventoryItem.itemName)
        {
            case "Missile": ExecuteMissile(); break;
            case "Devil": ExecuteDevil(); break;
            case "Booster": StartCoroutine(BoosterRoutine()); break;
            case "Shield": StartCoroutine(ShieldRoutine()); break;
            case "Banana": ExecuteBanana(); break;
        }
        inventoryItem = null;
    }

    void ExecuteMissile()
    {
        GameObject target = FindClosestForwardVehicle();
        if (target != null) target.GetComponent<ItemManager>().ApplyMissileHit();
    }

    public void ApplyMissileHit()
    {
        if (IsShielded) return;
        StartCoroutine(StunRoutine(2f));
    }

    IEnumerator StunRoutine(float duration)
    {
        IsStunned = true;
        if (smokeEffect) smokeEffect.SetActive(true);
        float originalSpeed = arcadeCar.maxSpeedKmh;
        arcadeCar.maxSpeedKmh = 0f;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) { rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
        if (carController != null && carController.isAI) { arcadeCar.verticalInput = 0f; arcadeCar.horizontalInput = 0f; }

        yield return new WaitForSeconds(duration);
        arcadeCar.maxSpeedKmh = originalSpeed;
        if (smokeEffect) smokeEffect.SetActive(false);
        IsStunned = false;
    }

    void ExecuteDevil()
    {
        ItemManager[] allManagers = FindObjectsByType<ItemManager>(FindObjectsSortMode.None);
        foreach (var im in allManagers) if (im != this) im.StartCoroutine(im.DevilRoutine());
    }

    public IEnumerator DevilRoutine()
    {
        if (IsShielded) yield break;
        IsDevilAffected = true;
        ChangeCarColor(new Color(0.6f, 0f, 0.6f));
        yield return new WaitForSeconds(Random.Range(5f, 10f));
        ChangeCarColor(Color.white);
        IsDevilAffected = false;
    }

    IEnumerator BoosterRoutine()
    {
        if (blueBoostEffect) { blueBoostEffect.SetActive(true); blueBoostEffect.transform.localPosition = new Vector3(0f, 0.5f, -2f); }
        float originalMax = arcadeCar.maxSpeedKmh;
        arcadeCar.maxSpeedKmh *= 1.5f;
        yield return new WaitForSeconds(Random.Range(3f, 4f));
        arcadeCar.maxSpeedKmh = originalMax;
        if (blueBoostEffect) blueBoostEffect.SetActive(false);
    }

    IEnumerator ShieldRoutine()
    {
        IsShielded = true;
        ChangeCarColor(Color.blue);
        yield return new WaitForSeconds(3f);
        ChangeCarColor(Color.white);
        IsShielded = false;
    }

    // ★ 바나나 생성 위치를 완전히 뒤로 빼주는 핵심 수정 구간
    void ExecuteBanana()
    {
        if (bananaPrefab != null)
        {
            // 기존 4.5f에서 7.0f로 거리를 대폭 늘려 차체에 걸리지 않도록 방지합니다.
            Vector3 spawnPos = transform.position - (transform.forward * 7.0f);

            // 허공에 뜨지 않도록 스폰 포인트의 높이(y)를 참조합니다.
            if (itemSpawnPoint != null)
            {
                spawnPos.y = itemSpawnPoint.position.y;
            }
            else
            {
                spawnPos.y = 0.1f;
            }

            Instantiate(bananaPrefab, spawnPos, Quaternion.identity);
        }
    }

    public void OnStepBanana()
    {
        if (IsShielded) return;
        StartCoroutine(BananaSlippingRoutine());
    }

    IEnumerator BananaSlippingRoutine()
    {
        if (arcadeCar == null) yield break;
        float originalSpeed = arcadeCar.maxSpeedKmh;
        float originalGrip = arcadeCar.currentGrip;
        arcadeCar.maxSpeedKmh = 10f;
        arcadeCar.currentGrip = 0.02f;
        arcadeCar.SendMessage("SetSkidMarks", true, SendMessageOptions.DontRequireReceiver);
        yield return new WaitForSeconds(Random.Range(2f, 3f));
        arcadeCar.SendMessage("SetSkidMarks", false, SendMessageOptions.DontRequireReceiver);
        arcadeCar.maxSpeedKmh = originalSpeed;
        arcadeCar.currentGrip = originalGrip;
    }

    ItemType DecideTypeByRank(int rank)
    {
        int r = Random.Range(0, 100);

        if (rank <= 1)
        {
            return (r < 85) ? ItemType.Defense : ItemType.Neutral;
        }
        else if (rank == 2)
        {
            if (r < 40) return ItemType.Attack;
            if (r < 70) return ItemType.Defense;
            return ItemType.Neutral;
        }
        else
        {
            return (r < 70) ? ItemType.Attack : ItemType.Neutral;
        }
    }

    ItemData DecideSpecificItem(int rank, ItemType type)
    {
        float r = Random.value;

        if (rank >= 3)
        {
            if (type == ItemType.Attack)
            {
                return (rank == 4 && r < 0.7f) ? allItems.Find(x => x.itemName == "Devil") : allItems.Find(x => x.itemName == "Missile");
            }
            return allItems.Find(x => x.itemName == "Booster");
        }

        string targetName = "Booster";

        if (type == ItemType.Attack)
        {
            targetName = "Missile";
        }
        else if (type == ItemType.Defense)
        {
            targetName = (rank == 1 && r < 0.75f) ? "Banana" : "Shield";
        }

        return allItems.Find(x => x.itemName == targetName);
    }

    void ChangeCarColor(Color color)
    {
        if (carRenderers != null)
        {
            foreach (var r in carRenderers) if (r != null && r.material.HasProperty("_Color")) r.material.color = color;
        }
    }

    GameObject FindClosestForwardVehicle()
    {
        GameObject[] cars = GameObject.FindGameObjectsWithTag("Player");
        GameObject closest = null;
        MinDistanceLogic(cars, ref closest);
        return closest;
    }

    private void MinDistanceLogic(GameObject[] cars, ref GameObject closest)
    {
        float minDistance = Mathf.Infinity;
        foreach (GameObject car in cars)
        {
            if (car == this.gameObject) continue;
            float distance = Vector3.Distance(transform.position, car.transform.position);
            Vector3 targetDir = car.transform.position - transform.position;
            float angle = Vector3.Angle(transform.forward, targetDir);
            if (angle < 45f && distance < minDistance) { minDistance = distance; closest = car; }
        }
    }
}