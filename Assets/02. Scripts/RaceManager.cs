using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class RaceManager : MonoBehaviour
{
    [Header("트랙에 배치된 4대의 차량")]
    // 0: GTR, 1: 카마로, 2: 알파 로메오, 3: i20N 순서대로 인스펙터에서 넣습니다.
    public GameObject[] raceCars;

    [Header("AI가 따라갈 첫 번째 웨이포인트")]
    public Transform firstWaypoint;

    void Start()
    {
        AssignBrainsToCars();
    }

    private void AssignBrainsToCars()
    {
        int playerCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);

        for (int i = 0; i < raceCars.Length; i++)
        {
            GameObject currentCar = raceCars[i];

            if (i == playerCarIndex)
            {
                // 1. 플레이어 조작 할당
                currentCar.AddComponent<PlayerInput>();
                Debug.Log(currentCar.name + "에 플레이어 조작이 할당되었습니다.");

                // ★ 2. 카메라 추적 타겟 설정 (여기 추가!) ★
                CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
                if (camFollow != null)
                {
                    camFollow.target = currentCar.transform; // 카메라야, 이 차를 쫓아가!
                }
            }
            else
            {
                // AI 조작 할당
                AIInput aiBrain = currentCar.AddComponent<AIInput>();
                aiBrain.targetWaypoint = firstWaypoint;
            }
        }
    }
}
