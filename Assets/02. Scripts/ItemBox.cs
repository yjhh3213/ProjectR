using UnityEngine;
using System.Collections;

public class ItemBox : MonoBehaviour
{
    [Header("2.1 큐브 설정")]
    public GameObject cubeMesh; 
    public float rotationSpeed = 100f;

    private bool isActive = true;

    void Update()
    {
        // 상자 전체가 제자리에서 팽이처럼 좌우로 돌게 합니다.
        if (isActive && cubeMesh != null)
        {
            cubeMesh.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. 일단 충돌하면 무조건 이름을 찍습니다.
        Debug.Log("어라? 무언가 닿았습니다! 이름: " + other.gameObject.name);

        // 2. 태그가 일치하는지 확인합니다.
        ItemManager im = other.GetComponent<ItemManager>(); //
        if (isActive && im != null)
        { // 아이템 매니저가 있다면 플레이어든 AI든 OK!
            if (!im.isRolling && im.inventoryItem == null)
            {
                im.StartGetItemRoutine(); //
                StartCoroutine(RespawnRoutine()); //
            }
        }
        else
        {
            Debug.Log("닿긴 했는데 태그가 '" + other.tag + "' 라서 무시합니다.");
        }
        if (isActive && other.CompareTag("Player"))
        {
            Debug.Log("무언가와 충돌함: " + other.name);
            ItemManager player = other.GetComponent<ItemManager>();
            // 4.6.2 하나만 돌아가게 체크
            if (player != null && !player.isRolling && player.inventoryItem == null)
            {
                player.StartGetItemRoutine();
                StartCoroutine(RespawnRoutine());
            }
        }
    }

    IEnumerator RespawnRoutine()
    {
        isActive = false;
        cubeMesh.SetActive(false);
        // 2.1.2.1 5~10초 사이 간격 재생성
        yield return new WaitForSeconds(Random.Range(5f, 10f));
        cubeMesh.SetActive(true);
        isActive = true;
    }
}