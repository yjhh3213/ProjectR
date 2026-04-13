using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    public float rotateSpeed = 10f; // 회전 속도

    void Update()
    {
        // Y축을 기준으로 매 프레임 부드럽게 회전
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
    }
}