using UnityEngine;

public class SimpleAudioManager : MonoBehaviour
{
    // 어디서든 이 매니저를 쉽게 부를 수 있게 해주는 마법의 단어 (싱글톤)
    public static SimpleAudioManager instance;

    [Header("오디오 재생기 (인스펙터에서 연결)")]
    public AudioSource bgmPlayer; // 배경음악을 틀어줄 카세트테이프 플레이어 역할
    public AudioSource sfxPlayer; // 효과음을 틀어줄 플레이어 역할

    [Header("아이템 효과음 파일들 (인스펙터에서 넣기)")]
    public AudioClip bananaSound; // 바나나 떨어뜨릴 때 소리
    public AudioClip rocketSound; // 부스터 사용할 때 소리
    public AudioClip bombSound;   // 폭탄 터질 때 소리

    void Awake()
    {
        // 게임이 시작될 때 나 자신을 instance로 등록합니다.
        instance = this;
    }

    // 아이템을 사용할 때 부를 함수들
    // 아이템을 사용할 때 부를 함수들 (안전장치 추가 버전)
    public void PlayBananaSound()
    {
        // sfxPlayer와 오디오 파일이 둘 다 존재할 때만 재생합니다.
        if (sfxPlayer != null && bananaSound != null)
        {
            sfxPlayer.PlayOneShot(bananaSound);
        }
    }

    public void PlayRocketSound()
    {
        if (sfxPlayer != null && rocketSound != null)
        {
            sfxPlayer.PlayOneShot(rocketSound);
        }
    }

    public void PlayBombSound()
    {
        if (sfxPlayer != null && bombSound != null)
        {
            sfxPlayer.PlayOneShot(bombSound);
        }
        else
        {
            Debug.LogWarning("SimpleAudioManager: sfxPlayer 또는 bombSound가 인스펙터에서 지정되지 않았습니다!");
        }
    }
}