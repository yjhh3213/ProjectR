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

    private ItemManager itemManager;
    private bool isWaitingToUseItem = false;

    void Start()
    {
        itemManager = GetComponent<ItemManager>();
        gameObject.tag = "Player"; // 상자 충돌을 위해 모두 Player 태그 권장
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
            if (Input.GetKeyDown(KeyCode.LeftControl)) itemManager.UseItem();
        }
    }

    void HandleAIMovement()
    {
        if (waypoints.Count == 0) return;
        Vector3 targetDir = waypoints[currentIdx].position - transform.position;
        targetDir.y = 0;
        if (targetDir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetDir), rotSpeed * Time.deltaTime);

        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, waypoints[currentIdx].position) < 3f)
            currentIdx = (currentIdx + 1) % waypoints.Count;
    }

    void HandleAIItemUsage()
    {
        // 아이템이 있고, 대기 중이 아닐 때 5초 예약
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