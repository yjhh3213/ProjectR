using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "RacingGame/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName; // 미사일, 대마왕, 부스터, 쉴드, 바나나
    public ItemType type;   // Attack, Defense, Neutral
    //public Sprite icon;     // UI에 표시될 이미지 (제공해주신 이미지들)
    public GameObject worldPrefab; // 설치/발사될 3D 오브젝트
    public AudioClip soundEffect;  // 2.2.x.3 BGM/효과음
    public float duration;  // 지속 시간 (스턴, 쉴드 등)
}

public enum ItemType { Attack, Defense, Neutral }