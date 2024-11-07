using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Dialogue/DialogueData")]
public class DialogueData : ScriptableObject
{
    [TextArea(3, 10)]
    public List<string> dialogueTexts; // Store multiple dialogues
    public List<float> intervals; // Store custom interval for each line
}
