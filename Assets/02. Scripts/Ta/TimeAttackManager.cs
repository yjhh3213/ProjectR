using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimeAttackManager : MonoBehaviour
{
    [Header("차량 프리팹 (로비 인덱스와 일치: 0, 1, 2, 3)")]
    public GameObject[] carPrefabs;

    [Header("스폰 위치")]
    public Transform spawnPoint;

    [Header("카메라 세팅")]
    public CameraFollow cameraFollowScript;

    [Header("섹터 및 목적지 설정 (순서대로 등록)")]
    public List<Transform> sectorCheckpoints; // 0번부터 마지막 목적지까지 순서대로 드래그앤드롭

    public int currentSectorIndex = 0;
    //public string Scenename;
    private float raceTimer = 0f;
    private bool isRaceStarted = false;
    private bool isRaceFinished = false;

    void Start()
    {
        SpawnPlayerCar();
        isRaceStarted = true;
    }

    void Update()
    {
        if (isRaceStarted && !isRaceFinished)
        {
            raceTimer += Time.deltaTime;
            // UI에 시간 갱신 로직을 여기에 연결할 수 있습니다. (예: 타임어택 UI 텍스트)
        }
    }

    // 플레이어 차량만 단독 스폰
    private void SpawnPlayerCar()
    {
        int playerCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);

        if (carPrefabs.Length <= playerCarIndex || carPrefabs[playerCarIndex] == null)
        {
            Debug.LogError("[TimeAttackManager] 잘못된 차량 인덱스이거나 프리팹이 비어있습니다!");
            return;
        }

        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion spawnRot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        GameObject spawnedCar = Instantiate(carPrefabs[playerCarIndex], spawnPos, spawnRot);
        spawnedCar.SetActive(true);
        spawnedCar.name = "PlayerCar";
        spawnedCar.tag = "Player";

        // 필수 컴포넌트 장착 및 연동
        if (spawnedCar.GetComponent<PlayerInput>() == null)
        {
            spawnedCar.AddComponent<PlayerInput>();
        }

        ArcadeCarController ac = spawnedCar.GetComponent<ArcadeCarController>();
        SpeedometerUI ui = FindObjectOfType<SpeedometerUI>();
        if (ui != null && ac != null)
        {
            ui.SetupUI(ac);
        }

        if (cameraFollowScript != null)
        {
            cameraFollowScript.target = spawnedCar.transform;
        }

        Debug.Log($"[TimeAttack] 차량 스폰 완료 (인덱스: {playerCarIndex})");
    }

    // 플레이어가 섹터 콜라이더를 통과했을 때 호출됨
    public void OnPassSector(int sectorIndex)
    {
        if (isRaceFinished) return;

        // 순서대로 통과했을 때만 인정 (역주행 방지)
        if (sectorIndex == currentSectorIndex)
        {
            Debug.Log($"섹터 {currentSectorIndex} 통과!");
            currentSectorIndex++;

            // 모든 섹터를 통과하고 마지막 목적지에 도달했을 때
            if (currentSectorIndex >= sectorCheckpoints.Count)
            {
                FinishTimeAttack();
            }
        }
    }

    private void FinishTimeAttack()
    {
        isRaceFinished = true;
        Debug.Log($"타임어택 종료! 최종 기록: {raceTimer:F2}초");

        // 결과창이 읽어갈 수 있도록 현재 기록 저장
        PlayerPrefs.SetFloat("LastRaceTime", raceTimer);
        PlayerPrefs.Save();

        // 결과 씬(ResultScene)으로 이동
        SceneManager.LoadScene("TAResultScene"); // 결과 씬 이름으로 변경
    }
}