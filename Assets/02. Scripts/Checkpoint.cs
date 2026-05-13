using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("체크포인트 설정")]
    public int checkpointIndex; // 이 체크포인트의 번호 (0, 1, 2...)

    private void OnTriggerEnter(Collider other)
    {
        // 1. 충돌한 물체에서 RaceParticipant 컴포넌트를 찾습니다.
        // 부모나 자식 오브젝트에 있을 수 있으므로 GetComponentInParent를 권장합니다.
        RaceParticipant participant = other.GetComponentInParent<RaceParticipant>();

        if (participant != null)
        {
            // 2. 차량의 마지막 체크포인트 인덱스를 업데이트합니다.
            // 보통은 순서대로 통과해야 하므로 로직을 추가할 수 있습니다.
            participant.lastCheckpointIndex = checkpointIndex;

            // 3. 만약 마지막 체크포인트(결승점 전)를 통과하고 다시 0번을 통과하면 Lap을 올리는 로직
            // (이 부분은 트랙 설계에 따라 RankManager에서 처리해도 됩니다.)
            Debug.Log($"{other.name} 차량이 {checkpointIndex}번 체크포인트를 통과함");
        }
    }
}