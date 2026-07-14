using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("체크포인트 설정")]
    public int checkpointIndex; // 이 체크포인트의 번호 (0, 1, 2, 3...)

    private void OnTriggerEnter(Collider other)
    {
        // 결승선(FinishLine) 오브젝트 자체이거나 이름에 Finish가 포함되어 있다면 
        // 일반 체크포인트 연산을 수행하지 않고 패스합니다.
        if (gameObject.name.Contains("Finish") || gameObject.CompareTag("FinishLine"))
        {
            return;
        }

        // 충돌한 물체에서 RaceParticipant 컴포넌트를 찾습니다.
        RaceParticipant participant = other.GetComponentInParent<RaceParticipant>();

        if (participant != null)
        {
            // 첫 체크포인트 통과(-1 상태)이거나, 이전 통과한 인덱스보다 큰 번호일 때 위치를 세팅합니다.
            if (participant.lastCheckpointIndex == -1 || checkpointIndex > participant.lastCheckpointIndex)
            {
                // 위치와 회전을 RaceParticipant에 직접 강제 주입하여 업데이트합니다.
                participant.SetNewCheckpoint(checkpointIndex, transform.position, transform.rotation);
            }
        }
    }
}