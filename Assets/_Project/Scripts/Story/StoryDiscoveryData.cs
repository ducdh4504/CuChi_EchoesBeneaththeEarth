using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(
    fileName = "NewStoryDiscoveryData",
    menuName = "CuChi/Story/Story Discovery Data")]
public class StoryDiscoveryData : ScriptableObject
{
    [Header("Discovery Info")]
    public string discoveryId;
    public string discoveryTitle;

    [Header("Story Content")]
    [TextArea(3, 8)]
    public string monologue;

    [Header("Images")]
    [FormerlySerializedAs("secretOrderImage")]
    public Sprite secretLetterImage;
    public Sprite morseCodeImage;

    [Header("UI Text")]
    //public string continueHint = "Nhấn E / Space để tiếp tục";
    public string continueHint = "Đang hội thoại... Giữ Space để bỏ qua";

    [Header("After Discovery")]
    [TextArea(2, 4)]
    public string objectiveAfterDiscovery;
}