using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialoguePlayer : MonoBehaviour
{
    public List<GameObject> SpeakersInRange;
    public GameObject DialoguePrompt;
    public GameObject DialogueBox;
    public TMP_Text PromptText;
    DialogueUI _dialogueUI;

    void Start()
    {
        SpeakersInRange = new List<GameObject>();
        _dialogueUI = DialogueBox.GetComponent<DialogueUI>();

        DialogueBox.SetActive(false);
        DialoguePrompt.SetActive(false);
    }

    public bool TriggerDialogue()
    {
        if(SpeakersInRange.Count > 0)
        {
            DialogueBox.SetActive(true);
            DialoguePrompt.SetActive(false);
            _dialogueUI.SetDialogue(SpeakersInRange[0].GetComponent<DialogueTrigger>().Entity);

            //Unlocks the cursor from the window.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return true;
        }
        return false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("NPC"))
        {
            SpeakersInRange.Add(other.gameObject);
            if(!DialoguePrompt.activeInHierarchy && !DialogueBox.activeInHierarchy)
            {
                DialoguePrompt.SetActive(true);
                PromptText.text = "Press E to talk to " + SpeakersInRange[0].GetComponent<DialogueTrigger>().Entity.EntityName;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (SpeakersInRange.Contains(other.gameObject))
        {
            SpeakersInRange.Remove(other.gameObject);

            if (SpeakersInRange.Count > 0)
            {
                if (_dialogueUI.CurrentEntity == other.GetComponent<DialogueTrigger>().Entity)
                {
                    DialogueBox.SetActive(false);
                    DialoguePrompt.SetActive(true);

                    //Locks the cursor to the window.
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }  
                PromptText.text = "Press E to talk to " + SpeakersInRange[0].GetComponent<DialogueTrigger>().Entity.EntityName;
            }
            else
            {
                DialogueBox.SetActive(false);
                DialoguePrompt.SetActive(false);

                //Locks the cursor to the window.
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    public void LeaveBox()
    {
        if (DialogueBox.activeInHierarchy)
        {
            DialogueBox.SetActive(false);
            DialoguePrompt.SetActive(true);

            //Locks the cursor to the window.
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}