using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneManager : MonoBehaviour
{
    [Header("이동할 씬 이름")]
    public string lobbySceneName = "LobbyScene";    // 수정 가능

    // Update is called once per frame
    void Update()
    {
        // 아무 키(키보드, 마우스, 조이패드 버튼 등)나 눌렸는지 감지
        if (Input.anyKeyDown)
        {
            LoadLobby();
        }
    }

    private void LoadLobby()
    {
        Debug.Log("Load Lobby");
        SceneManager.LoadScene(lobbySceneName);
    }
}
