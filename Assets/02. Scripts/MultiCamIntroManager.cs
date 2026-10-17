using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MultiCamIntroManager : MonoBehaviour
{
    [Header("시네머신 인트로 카메라 목록 (1 -> 2 -> 3)")]
    public CinemachineVirtualCamera[] introCameras;

    [Header("각 카메라 위치에 '도착한 후' 멈춰있을 시간(초)")]
    public float timePerCamera = 1.5f;

    [Header("레이싱 시작 시 켜줄 요소")]
    public StartGridManager carSpawnerManager;
    public GameObject racingUI;

    private void Start()
    {
        if (carSpawnerManager != null) carSpawnerManager.gameObject.SetActive(false);
        if (racingUI != null) racingUI.SetActive(false);

        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        for (int i = 0; i < introCameras.Length; i++)
        {
            SetCameraPriority(i);
            yield return new WaitForSeconds(timePerCamera);
        }

        StartRace();
    }

    private void SetCameraPriority(int activeIndex)
    {
        for (int i = 0; i < introCameras.Length; i++)
        {
            if (introCameras[i] != null)
            {
                introCameras[i].Priority = (i == activeIndex) ? 20 : 0;
            }
        }
    }

    private void StartRace()
    {
        // 1. 인트로 카메라 꺼서 메인 카메라로 시점 전환
        foreach (var cam in introCameras)
        {
            if (cam != null)
            {
                cam.Priority = 0;
                cam.gameObject.SetActive(false);
            }
        }

        // 2. 스폰 매니저 & UI 활성화
        if (carSpawnerManager != null) carSpawnerManager.gameObject.SetActive(true);
        if (racingUI != null) racingUI.SetActive(true);

        // 3. 내가 선택해서 스폰된 플레이어 차로 카메라 고정
        StartCoroutine(AttachPlayerCameraRoutine());
    }

    private IEnumerator AttachPlayerCameraRoutine()
    {
        yield return new WaitForEndOfFrame(); // 차량 스폰 대기

        Transform playerTransform = null;

        // 스폰 매니저에서 실제로 생성된 차량 중 플레이어 차량(isAI == false)을 직접 검색
        if (carSpawnerManager != null && carSpawnerManager.spawnedControllers != null)
        {
            foreach (var controller in carSpawnerManager.spawnedControllers)
            {
                if (controller != null && !controller.isAI)
                {
                    playerTransform = controller.transform;
                    break;
                }
            }
        }

        // 이름 기반 예비 탐색 (PlayerCar로 지정된 경우)
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.Find("PlayerCar");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        // 카메라 연결
        if (playerTransform != null)
        {
            CameraFollow camFollow = null;
            if (Camera.main != null)
            {
                camFollow = Camera.main.GetComponent<CameraFollow>();
            }
            if (camFollow == null)
            {
                camFollow = FindObjectOfType<CameraFollow>();
            }

            if (camFollow != null)
            {
                camFollow.target = playerTransform;
                Debug.Log($"선택한 플레이어 차량 ({playerTransform.name})에 카메라 고정 완료!");
            }
        }
    }
}