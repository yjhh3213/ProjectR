using UnityEngine;

public class JoystickTester : MonoBehaviour
{
    void OnGUI()
    {
        // 화면에 글씨를 크게 띄우기 위한 스타일 세팅
        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = Color.green;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        GUILayout.BeginArea(new Rect(20, 20, 600, 400));
        GUILayout.Label($"====== PXN V9 축 실시간 모니터 ======", style);
        GUILayout.Label($"[Horizontal 값 (좌/우 예상)] : {h:F2}", style);
        GUILayout.Label($"[Vertical 값 (앞/뒤 예상)] : {v:F2}", style);
        GUILayout.Label($"", style);
        GUILayout.Label($" 체크리스트 :", style);
        GUILayout.Label($"1. 핸들을 가만히 두고 엑셀만 밟았을 때 어떤 값이 변하나요?", style);
        GUILayout.Label($"2. 핸들만 돌렸을 때 어떤 값이 변하나요?", style);
        GUILayout.EndArea();
    }
}