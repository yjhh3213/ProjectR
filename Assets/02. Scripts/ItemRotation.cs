using UnityEngine;

public class ItemRotation : MonoBehaviour
{
    public float rotationSpeed = 100f; // 회전 속도
    public float bobbingSpeed = 2f;    // 위아래 흔들림 속도
    public float bobbingAmount = 0.1f; // 위아래 흔들림 폭

    Vector3 startPos;

    void Start()
    {
        // 생성된 시점의 상대적인 시작 위치 저장
        startPos = transform.localPosition;
    }

    void Update()
    {
        // 1. 좌우 회전 (Y축 기준)
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        // 2. 위아래로 둥실둥실 (카트라이더 느낌 연출)
        float newY = Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmount;
        transform.localPosition = startPos + new Vector3(0, newY, 0);
    }
}