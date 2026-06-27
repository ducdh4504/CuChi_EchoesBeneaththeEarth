using System;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    [Header("Speaker")]
    public string speakerName;

    [TextArea(2, 5)]
    public string content;

    //Voice
    [Header("Voice")]
    public AudioClip voiceClip;

    [Tooltip("Thời gian nghỉ thêm sau khi audio của line này phát xong.")]
    [Min(0f)]
    public float delayAfterLine = 0.15f;

    [Tooltip("Nếu line này chưa có audio, hệ thống sẽ dùng thời gian này để tự chuyển line.")]
    [Min(0.5f)]
    public float fallbackDuration = 2.5f;
}