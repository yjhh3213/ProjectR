using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarController : MonoBehaviour
{
    public bool isAI = true;

    [Header("AI 주행")]
    public List<Transform> waypoints;
    public float speed = 15f;
    public float rotSpeed = 5f;
    private int currentIdx = 0;

    // --- 추가된 변수 ---
    private float sideOffset;      // 좌우로 치우칠 정도 (-3 ~ 3 사이 추천)
    private float lookAheadDist;   // 각 차마다 다른 전방 주시 거리
    // ------------------

    private ItemManager itemManager;
    private bool isWaitingToUseItem = false;

    void Start()
    {
        itemManager = GetComponent<ItemManager>();
        gameObject.tag = "Player";

        // [핵심] 차마다 개성을 부여합니다.
        // 이 값 덕분에 3대의 차가 서로 다른 위치를 향해 달려 일렬 주행이 사라집니다.
        sideOffset = Random.Range(-3.5f, 3.5f);
        lookAheadDist = Random.Range(2.0f, 5.0f); // 어떤 차는 더 멀리, 어떤 차는 더 가까이 보고 꺾음
    }

    void Update()
    {
        if (isAI)
        {
            HandleAIMovement();
            HandleAIItemUsage();
        }
        else
        {
            // 플레이어 조작 (기존과 동일)
            if (Input.GetKeyDown(KeyCode.LeftControl)) itemManager.UseItem();
        }
    }

    void HandleAIMovement()
    {
        if (waypoints.Count == 0) return;

        // [수정] 단순 웨이포인트 위치가 아닌, 좌우 편차가 적용된 타겟 위치 계산
        Vector3 targetPos = waypoints[currentIdx].position;

        // 웨이포인트의 오른쪽 방향벡터(right)를 기준으로 sideOffset만큼 밀어줍니다.
        targetPos += waypoints[currentIdx].right * sideOffset;

        Vector3 targetDir = targetPos - transform.position;
        targetDir.y = 0;

        if (targetDir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetDir), rotSpeed * Time.deltaTime);

        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // [수정] lookAheadDist를 활용해 다음 지점으로 넘어가는 타이밍을 다르게 줍니다.
        if (Vector3.Distance(transform.position, targetPos) < lookAheadDist)
            currentIdx = (currentIdx + 1) % waypoints.Count;
    }

    // 아이템 사용 로직 (기존과 동일)
    void HandleAIItemUsage()
    {
        if (itemManager.inventoryItem != null && !itemManager.isRolling && !isWaitingToUseItem)
        {
            float randomDelay = Random.Range(3f, 7f);
            StartCoroutine(WaitAndUseItem(randomDelay));
        }
    }

    IEnumerator WaitAndUseItem(float delay)
    {
        isWaitingToUseItem = true;
        yield return new WaitForSeconds(delay);
        if (itemManager.inventoryItem != null) itemManager.UseItem();
        isWaitingToUseItem = false;
    }
}