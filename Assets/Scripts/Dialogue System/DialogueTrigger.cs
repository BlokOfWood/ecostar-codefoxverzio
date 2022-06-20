using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Leguar.TotalJSON;

public class DialogueTrigger : MonoBehaviour
{
    [Tooltip("The dialogue entity this game object represents.")]
    public DialogueEntity Entity;
    public List<DialogueEntry> DialogueTree;

    void Start()
    {
        DialogueTree = new List<DialogueEntry>();

        DialogueTree = DialogueTree.OrderBy(x => x.ID).ToList();
    }
}
