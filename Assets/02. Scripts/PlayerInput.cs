using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private ArcadeCarController controller;

    void Start()
    {
        controller = GetComponent<ArcadeCarController>();
    }

    void Update()
    {
        // 키보드 입력을 받아서 자동차 컨트롤러에 전달합니다.
        controller.moveInput = Input.GetAxis("Vertical");
        controller.turnInput = Input.GetAxis("Horizontal");
    }
}
