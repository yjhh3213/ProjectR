using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private ItemManager itemManager;
    private ArcadeCarController arcadeCar;

    void Start()
    {
        itemManager = GetComponent<ItemManager>();
        arcadeCar = GetComponent<ArcadeCarController>();
    }

    void Update()
    {
        if (arcadeCar == null) return;

        // 키보드 입력을 받아옵니다.
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 1. [대마왕 처리] 저주 상태라면 핸들링 조작 방향을 좌우 정반대로 반전시킵니다.
        if (itemManager != null && itemManager.IsDevilAffected)
        {
            horizontal *= -1f;
        }

        // 2. [미사일 스턴 처리] 격추 피격 상태라면 가속/조향 키보드 조작을 완벽하게 마비(0)시킵니다.
        if (itemManager != null && itemManager.IsStunned)
        {
            horizontal = 0f;
            vertical = 0f;
        }

        // 3. [드리프트 조작 완벽 대응] 물리 마찰력 시스템과 연동해 조작감을 완벽 보존합니다.
        if (Input.GetKey(KeyCode.LeftShift) && !itemManager.IsStunned)
        {
            arcadeCar.isDrifting = true;
            arcadeCar.currentGrip = arcadeCar.driftGrip;
            arcadeCar.SendMessage("SetSkidMarks", true, SendMessageOptions.DontRequireReceiver);
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift) || (itemManager != null && itemManager.IsStunned))
        {
            arcadeCar.isDrifting = false;
            arcadeCar.currentGrip = arcadeCar.normalGrip;
            arcadeCar.SendMessage("SetSkidMarks", false, SendMessageOptions.DontRequireReceiver);
        }

        // 입력 수치를 자동차 엔진에 전달합니다.
        arcadeCar.horizontalInput = horizontal;
        arcadeCar.verticalInput = vertical;
    }
}