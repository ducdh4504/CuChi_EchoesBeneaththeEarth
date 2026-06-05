using System;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    [Header("Speaker")]
    public string speakerName;

    [TextArea(2, 5)]
    public string content;
}