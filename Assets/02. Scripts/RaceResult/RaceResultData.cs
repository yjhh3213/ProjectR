using System.Collections.Generic;

public class ParticipantResult
{
    public string carName;   // 차량 이름 또는 종류 식별자
    public string finalTime; // 완주 기록 (예: 1'23''456)
}

public static class RaceResultData
{
    // 완주 순서대로 정렬되어 저장될 결과 리스트
    public static List<ParticipantResult> savedResults = new List<ParticipantResult>();
}