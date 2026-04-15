using UnityEngine;

[RequireComponent(typeof(ArcadeCarController))]
public class PlayerInput : MonoBehaviour
{
    private ArcadeCarController carController;

    void Start()
    {
        carController = GetComponent<ArcadeCarController>();
    }

    void Update()
    {
        // 1. 앞뒤, 좌우 조작 전달
        carController.verticalInput = Input.GetAxis("Vertical");
        carController.horizontalInput = Input.GetAxis("Horizontal");

        // 2. L-Shift 키를 누르는 순간 -> 드리프트 시작!
        if (Input.GetKeyDown(KeyCode.LeftShift)&& !Input.GetKey(KeyCode.S))
        {
            carController.StartDrift();
        }

        // 3. L-Shift 키를 떼는 순간 -> 드리프트 끝! (부스트 발동)
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            carController.EndDriftAndBoost();
        }
    }
}