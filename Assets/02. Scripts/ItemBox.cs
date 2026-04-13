using UnityEngine;
using System.Collections;

public class ItemBox : MonoBehaviour
{
    [Header("2.1 큐브 설정")]
    public GameObject cubeMesh; // 은은한 무지개색 메시
    public float rotationSpeed = 100f;

    private bool isActive = true;

    void Update()
    {
        // 2.1.1.1.1 x축 시계방향 회전
        if (isActive)
            cubeMesh.transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActive && other.CompareTag("Player"))
        {
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