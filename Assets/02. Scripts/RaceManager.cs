using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    [Header("트랙에 배치된 4대의 차량")]
    // 인스펙터에서 차량 4대를 순서대로 넣어주세요.
    public GameObject[] raceCars;

    [Header("웨이포인트 설정")]
    // 165개의 웨이포인트를 담고 있는 부모 오브젝트의 이름을 적어주세요.
    public string waypointGroupName = "Waypoints_Group";

    private List<Transform> waypointList = new List<Transform>();

    void Start()
    {
        // 1. 165개의 웨이포인트를 자동으로 리스트에 담기
        SetupWaypoints();

        // 2. 플레이어와 AI 배정하기
        AssignBrainsToCars();
    }

    private void SetupWaypoints()
    {
        // 이름으로 부모 오브젝트를 찾습니다.
        GameObject group = GameObject.Find(waypointGroupName);
        if (group != null)
        {
            // 부모 안의 모든 자식(웨이포인트)을 순서대로 리스트에 추가합니다.
            foreach (Transform child in group.transform)
            {
                waypointList.Add(child);
            }
            Debug.Log(waypointList.Count + "개의 웨이포인트를 등록했습니다.");
        }
        else
        {
            Debug.LogError(waypointGroupName + " 오브젝트를 찾을 수 없습니다! 이름을 확인해주세요.");
        }
    }

    private void AssignBrainsToCars()
    {
        // 로비에서 저장된 인덱스 가져오기 (기본값 0)
        int playerCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);

        for (int i = 0; i < raceCars.Length; i++)
        {
            GameObject currentCar = raceCars[i];
            if (currentCar == null) continue;

            // 모든 차의 CarController 컴포넌트를 가져옵니다.
            CarController controller = currentCar.GetComponent<CarController>();

            if (i == playerCarIndex)
            {
                // --- 플레이어 설정 ---
                if (controller != null)
                {
                    controller.isAI = false; // AI 끄기
                }

                // 카메라가 플레이어를 쫓아가도록 설정
                CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
                if (camFollow != null)
                {
                    camFollow.target = currentCar.transform;
                }

                Debug.Log(currentCar.name + "가 플레이어로 배정되었습니다.");
            }
            else
            {
                // --- AI 설정 ---
                if (controller != null)
                {
                    controller.isAI = true; // AI 켜기
                    controller.waypoints = waypointList; // 165개 웨이포인트 전달
                }
                Debug.Log(currentCar.name + "가 AI로 배정되었습니다.");
            }
        }
    }
}