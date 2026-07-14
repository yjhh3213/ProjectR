using UnityEngine;

public class PurePedalScanner : MonoBehaviour
{
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 40;
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;

        GUILayout.BeginArea(new Rect(20, 20, 1000, 800));
        GUILayout.Label(" [모든 계산을 끈 순수 원본 스캐너]", style);
        GUILayout.Label("----------------------------------", style);

        try
        {
            float rawAccel = Input.GetAxisRaw("AccelWheel");
            float rawBrake = Input.GetAxisRaw("BrakeWheel");

            // 악셀 값 출력
            GUILayout.Label($"악셀(AccelWheel) 원본: {rawAccel:F2}", style);

            // 브레이크 값 출력
            GUILayout.Label($"브레이크(BrakeWheel) 원본: {rawBrake:F2}", style);
        }
        catch
        {
            //GUILayout.Label("Input Manager 설정 오류 (이름 확인 필요)", style, style.normal.textColor = Color.red);
        }

        GUILayout.EndArea();
    }
}