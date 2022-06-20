using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;
using Leguar.TotalJSON;

public class DialogueUI : MonoBehaviour
{
    public DialogueEntity CurrentEntity { get; private set; }
    public List<DialogueEntry> DialogueTree;
    public GameObject[] ResponseButtons;
    public TMP_Text SpeakerNameText;
    public TMP_Text DialogueText;
    DialogueEntry currentEntry;

    public void SetDialogue(DialogueEntity entity)
    {
        CurrentEntity = entity;
        SpeakerNameText.text = entity.EntityName;


        BinaryFormatter formatter = new BinaryFormatter();

        string path = null;
        #if UNITY_EDITOR
            path = $"Assets/Resources/{entity.EntityDialogueFile}.talk";
        #endif

        FileStream stream = new FileStream(path, FileMode.Open);
        DialogueTree = formatter.Deserialize(stream) as List<DialogueEntry>;
        stream.Close();

        foreach(DialogueEntry i in DialogueTree)
        Debug.Log(JSON.Serialize(i).CreatePrettyString());

        DialogueText.text = DialogueTree[0].DialogueTexts[0];

        currentEntry = DialogueTree[0];
        for (int i = 0; i < 3; i++)
        {
            if (i < DialogueTree.Count && currentEntry.Responses[i].Item1 != "" && currentEntry.Responses[i].Item1 != null)
            {
                ResponseButtons[i].transform.GetChild(0).GetComponent<TMP_Text>().text = currentEntry.Responses[i].Item1;
                ResponseButtons[i].SetActive(true);
            }
            else
            {
                ResponseButtons[i].SetActive(false);
            }
        }
    }

    public void onResponse(int buttonIndex)
    {
        (string, string, Vector2) displayedResponse = currentEntry.Responses[buttonIndex];
        foreach (DialogueEntry i in DialogueTree)
        {
            if (i.ID == displayedResponse.Item2)
            {
                DialogueText.text = i.DialogueTexts[0];
                currentEntry = i;
                for (int x = 0; x < 3; x++)
                {
                    if (x < DialogueTree.Count && currentEntry.Responses[x].Item1 != "" && currentEntry.Responses[x].Item1 != null)
                    {
                        ResponseButtons[x].transform.GetChild(0).GetComponent<TMP_Text>().text = currentEntry.Responses[x].Item1;
                        ResponseButtons[x].SetActive(true);
                    }
                    else
                    {
                        ResponseButtons[x].SetActive(false);
                    }
                }
                break;
            }
        }
    }
}
