using UnityEngine;

public class Banana : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 달리던 차량(Player 태그)과 부딪히면 미끄러짐 효과 발동
        if (other.CompareTag("Player"))
        {
            ItemManager carItems = other.GetComponent<ItemManager>();
            if (carItems != null)
            {
                carItems.OnStepBanana();
                Debug.Log($"<color=yellow>[바나나 트랩 작동]</color> {other.name}이 트랩에 걸렸습니다!");
            }

            // 트랩 발동 후 맵에서 소멸 처리
            Destroy(gameObject);
        }
    }
}