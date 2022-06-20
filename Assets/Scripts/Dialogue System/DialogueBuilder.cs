using Leguar.TotalJSON;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;

public class DialogueBuilder : MonoBehaviour
{
    ///<summary>Direct reference to the camera for performance. Camera.main every frame is horrible. </summary>
    Camera _mainCamera;

    [Header("Node Editor")]
    public GameObject TextEditor;
    public GameObject DialogueNodeBox;
    public GameObject DialogueConnectionNodeBox;
    public GameObject DialogueResponseNodeBox;
    public GameObject PreviewBox;
    public TMP_Text CoordinateText;
    List<DialogueEntry> _currentNodes = new List<DialogueEntry>();
    List<GameObject> _connectionNodes = new List<GameObject>();
    ///<summary>A list containing all of the currently present gameobjects that represent the dialogue nodes.</summary>
    List<GameObject> _dialogueNodes = new List<GameObject>();
    Vector2 _currentNodePosition;

    [Header("Creation Popup")]
    public GameObject CreationPopup;
    public TMP_InputField IDField;
    public TMP_Text ErrorDisplay;

    [Header("Edit Menu")]
    public GameObject EditUI;
    ///<summary>The current selected node.</summary>
    DialogueEntry _currentNode;

    [Header("Response Menu")]
    public GameObject ResponseUI;
    public TMP_Text ResponseErrorDisplay;

    [Header("Save Menu")]
    public GameObject SaveUI;
    public TMP_InputField SaveFileNameField;

    [Header("Read Menu")]
    public GameObject ReadUI;
    public TMP_InputField ReadFileNameField;

    [Header("Edit Response Menu")]
    public GameObject EditResponseUI;
    public TMP_InputField ToIDField;
    int _currentResponseIndex;

    void Start()
    {
        _mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            IDField.text = "";
            TextEditor.GetComponent<TMP_InputField>().text = "";
            SaveFileNameField.text = "";
            ReadFileNameField.text = "";

            foreach(GameObject i in _mainCamera.GetComponent<DialogueBuilderCamera>().blockerObjects)
            {
                i.SetActive(false);
            }
        }

        foreach (GameObject i in _mainCamera.GetComponent<DialogueBuilderCamera>().blockerObjects)
        {
            if (i.activeInHierarchy) return;
        }

        if(Input.GetKeyDown(KeyCode.RightShift))
        {
            SaveUI.SetActive(true);
        }

        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 clampedHit = new Vector3(Mathf.Round(hit.point.x), Mathf.Round(hit.point.y), 0);
            PreviewBox.transform.position = clampedHit;
            CoordinateText.text = "X: " + clampedHit.x + " Y: " + clampedHit.y;

            if (Input.GetMouseButtonDown(0))
            {
                OnClick(clampedHit);
            }

            else if (Input.GetMouseButtonDown(1))
            {
                OnResponseClick(clampedHit);
            }

            else if(Input.GetKeyDown(KeyCode.Delete))
            {
                DeleteNode(clampedHit);
            }

            else if(Input.GetKeyDown(KeyCode.RightControl))
            {
                ReadUI.SetActive(true);
            }
        }
    }

    ///<summary>When the create button is pressed on the creation popup. </summary>
    public void CreateNode()
    {
        TMP_InputField dialogueTextField = TextEditor.GetComponent<TMP_InputField>();
        string chosenID = IDField.text;

        if (_currentNodes.Exists(x => x.ID == chosenID))
        {
            ErrorDisplay.text = "A node with this ID already exists.";
            return;
        }

        DialogueEntry newEntry = new DialogueEntry();
        newEntry.DialogueTexts = new string[1];
        newEntry.DialogueTexts[0] = dialogueTextField.text;
        newEntry.EditorPosition = _currentNodePosition;
        newEntry.ID = chosenID;
        newEntry.Responses = new (string, string, Float2)[3];
        _currentNodes.Add(newEntry);

        CreationPopup.SetActive(false);
        TextEditor.SetActive(false);
        GameObject nodeInst = Instantiate(DialogueNodeBox, _currentNodePosition, Quaternion.identity);
        nodeInst.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "ID\n" + newEntry.ID;
        _dialogueNodes.Add(nodeInst);

        UpdateConnectionNodes();
    }

    public void OnResponseClick(Vector2 nodePosition)
    {
        //-250, -250 is a coordinate that can't be chosen for a node, so it can be used to identify if the variable has been set.
        (string, string, Vector2) selectedResponse = (null,null,new Vector2(-250,-250));
        foreach(DialogueEntry x in _currentNodes)
        {
            foreach(var y in x.Responses)
            {
                if(y.Item3 == nodePosition)
                {
                    selectedResponse = y;
                }
            }
        }

        if(selectedResponse.Item3 == new Vector2(-250,-250))
        {
            ResponseUI.SetActive(true);
            TextEditor.SetActive(true);

            TextEditor.GetComponent<TMP_InputField>().text = "";
            selectedResponse.Item3 = nodePosition;
        }

        _currentNodePosition = nodePosition;
    }

    public void CreateResponse()
    {
        ResponseErrorDisplay.text = "";
        string fromID = ResponseUI.transform.GetChild(1).GetComponent<TMP_InputField>().text;
        string toID = ResponseUI.transform.GetChild(2).GetComponent<TMP_InputField>().text;

        int fromEntryIndex = _currentNodes.FindIndex(x => x.ID == fromID);
        int toEntryIndex = _currentNodes.FindIndex(x => x.ID == toID);

        if (fromEntryIndex == -1)
        {
            ResponseErrorDisplay.text = "The from entry doesn't exist yet.";
            return;
        }

        //Reason that is defined before is so that it can be checked if it found an entry, without using an ugly solution like a boolean for this.
        int i = 0;
        for(i = 0; i < _currentNodes[fromEntryIndex].Responses.Count(); i++)
        {
            if (_currentNodes[fromEntryIndex].Responses[i].Item1 == null)
            {
                _currentNodes[fromEntryIndex].Responses[i] = (TextEditor.GetComponent<TMP_InputField>().text, toID, _currentNodePosition);
                break;
            }
        }

        if(i == 3)
        {
            ResponseErrorDisplay.text = "All response slots are filled up";
            return;
        }

        TextEditor.SetActive(false);
        ResponseUI.SetActive(false);
        for(i = 1; i < 3; i++)
        {
            ResponseUI.transform.GetChild(i).GetComponent<TMP_InputField>().text = "";
        }


        GameObject nodeInst = Instantiate(DialogueResponseNodeBox, _currentNodePosition, Quaternion.identity);
        nodeInst.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = $"FROM ID: {fromID} TO ID: {toID}";

        _dialogueNodes.Add(nodeInst);

        UpdateConnectionNodes();
    }

    public void EditNode()
    {
        TMP_InputField dialogueTextField = TextEditor.GetComponent<TMP_InputField>();

        _currentNode.DialogueTexts = dialogueTextField.text.Split('\n');
        dialogueTextField.text = "";

        for (int i = 0; i < _currentNodes.Count; i++)
        {
            if (_currentNodes[i].ID == _currentNode.ID)
            {
                Debug.Log(JSON.Serialize(_currentNodes[i]).CreatePrettyString());
                _currentNodes[i] = _currentNode;
                break;
            }
        }

        TextEditor.SetActive(false);
        EditUI.SetActive(false);
    }

    void OnClick(Vector2 nodePosition)
    {
        foreach (DialogueEntry x in _currentNodes)
        {
            for (int y = 0; y < 3; y++)
            {
                if (x.Responses[y].Item3 == nodePosition)
                {
                    _currentNode = x;
                    _currentNodePosition = x.EditorPosition;
                    TextEditor.SetActive(true);
                    EditResponseUI.SetActive(true);

                    ToIDField.text = x.Responses[y].Item2;
                    TextEditor.GetComponent<TMP_InputField>().text = x.Responses[y].Item1;
                    return;
                }
            }
        }
        if (_currentNodes.Exists(x =>
        {
            if (x.EditorPosition == nodePosition)
            {
                _currentNode = x;
                _currentNodePosition = x.EditorPosition;
                TextEditor.SetActive(true);
                EditUI.SetActive(true);


                TMP_InputField dialogueTextField = TextEditor.GetComponent<TMP_InputField>();
                dialogueTextField.text = string.Join("\n", _currentNode.DialogueTexts);

                return true;
            }
            else
                return false;
        }
        )) return;
        else
        {
            _currentNodePosition = nodePosition;
            EditUI.SetActive(false);
            TextEditor.SetActive(true);
            CreationPopup.SetActive(true);

            TextEditor.GetComponent<TMP_InputField>().text = "";
            IDField.text = "";
        }
    }

    void UpdateConnectionNodes()
    {
        _connectionNodes.ForEach(x =>
        {
            Destroy(x);
        });
        _connectionNodes.Clear();

        foreach (DialogueEntry x in _currentNodes)
        {
            for (int i = 0; i < 3; i++)
            {
                if(x.Responses[i].Item1 != null)
                {
                    CreateConnectionNode(x.EditorPosition, x.Responses[i].Item3);
                }

                foreach (DialogueEntry y in _currentNodes.FindAll(z => z != x))
                {
                    if (y.ID == x.Responses[i].Item2)
                    {
                        CreateConnectionNode(x.Responses[i].Item3, y.EditorPosition);
                        break;
                    }
                }
            }
        }
    }

    void CreateConnectionNode(Vector2 fromPos, Vector2 toPos)
    {
        Vector3 nodePosition = (fromPos + toPos) / 2;
        GameObject newConnectionNode = Instantiate(DialogueConnectionNodeBox);

        newConnectionNode.transform.position = nodePosition;
        newConnectionNode.transform.LookAt(toPos);
        newConnectionNode.transform.localScale = new Vector3(0.4f, 0.4f, Vector3.Distance(fromPos, toPos));

        _connectionNodes.Add(newConnectionNode);
    }

    void DeleteNode(Vector2 nodePosition)
    {
        foreach (DialogueEntry i in _currentNodes)
        {
            //Finds the node that the user was trying to delete.
            if (i.EditorPosition == nodePosition)
            {
                //First removes it from the list containing the entries.
                _currentNodes.Remove(i);

                //Deletes the representation of its responses.
                for (int x = 0; x < 3; x++)
                {
                    if (i.Responses[x].Item1 == null) continue;

                    GameObject _responseNodeRepres = _dialogueNodes.Find(z => i.Responses[x].Item3 == (Vector2)z.transform.position);
                    _dialogueNodes.Remove(_responseNodeRepres);
                    Destroy(_responseNodeRepres);
                }

                //Destroys the representation of the entry.
                GameObject _dialogueNodeRepres = _dialogueNodes.Find(x => (Vector2)x.transform.position == nodePosition);
                _dialogueNodes.Remove(_dialogueNodeRepres);
                Destroy(_dialogueNodeRepres);

                //Destroys the responses connecting to it.
                for(int x = 0; x < 3; x++)
                {
                    foreach(DialogueEntry y in _currentNodes)
                    {
                        if(i.ID == y.Responses[x].Item2)
                        {
                            //Sets the response as not existing. (the string is the only nullable type inside the response so it is used to determine if the response is marked as existing.)
                            _currentNodes[_currentNodes.FindIndex(z => z.ID == y.ID)].Responses[x].Item1 = null;

                            //Destroys the representation of the response.
                            GameObject _responseNodeRepres = _dialogueNodes.Find(z => y.Responses[x].Item3 == (Vector2)z.transform.position);
                            _dialogueNodes.Remove(_responseNodeRepres);
                            Destroy(_responseNodeRepres);
                        }
                    }
                }

                UpdateConnectionNodes();
                return;
            }

            int currentNodeIndex = _currentNodes.FindIndex(y => y.EditorPosition == (Vector2)i.EditorPosition);
            for (int x = 0; x < i.Responses.Count(); x++)
            {
                if (_currentNodes[currentNodeIndex].Responses[x].Item3 == nodePosition)
                {
                    _currentNodes[currentNodeIndex].Responses[x].Item1 = null;

                    GameObject _dialogueNodeRepres = _dialogueNodes.Find(y => y.transform.position == (Vector3)nodePosition);
                    _dialogueNodes.Remove(_dialogueNodeRepres);
                    Destroy(_dialogueNodeRepres);

                    UpdateConnectionNodes();
                    return;
                }
            }
        }
    }

    public void Save()
    {
        BinaryFormatter formatter = new BinaryFormatter();

        string path = "";

        if (Application.isEditor)
        {
            path = $"Assets/Resources/{SaveFileNameField.text}.talk";
        }
        else
        {
            path = $"{Application.persistentDataPath}/{SaveFileNameField.text}.talk";
        }
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, _currentNodes);
        stream.Close();

        SaveFileNameField.text = "";
        SaveUI.SetActive(false);
    }

    public void Read()
    {
        BinaryFormatter formatter = new BinaryFormatter();

        string path = "";

        if (Application.isEditor)
        {
            path = $"Assets/Resources/{ReadFileNameField.text}.talk";
        }
        else
        {
            path = $"{Application.persistentDataPath}/{ReadFileNameField.text}.talk";
        }
        FileStream stream = new FileStream(path, FileMode.Open);

        _currentNodes = (List<DialogueEntry>)formatter.Deserialize(stream);
        stream.Close();

        ReadFileNameField.text = "";
        ReadUI.SetActive(false);

        foreach(DialogueEntry i in _currentNodes)
        {
            GameObject inst = Instantiate(DialogueNodeBox, (Vector2)i.EditorPosition, Quaternion.identity);
            inst.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "ID\n" + i.ID;

            for (int x = 0; x < 3; x++)
            {
                if(i.Responses[x].Item1 != null)
                {
                    inst = Instantiate(DialogueResponseNodeBox, (Vector2)i.Responses[x].Item3, Quaternion.identity);
                    inst.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = $"FROM ID: {i.ID} TO ID: {i.Responses[x].Item2}";
                }
            }
        }

        UpdateConnectionNodes();
    }

    public void EditResponse()
    {
        _currentNode.Responses[_currentResponseIndex].Item2 = ToIDField.text;
        _currentNode.Responses[_currentResponseIndex].Item1 = TextEditor.GetComponent<TMP_InputField>().text;

        for (int i = 0; i < _currentNodes.Count; i++)
        {
            if (_currentNodes[i].ID == _currentNode.ID)
            {
                Debug.Log(JSON.Serialize(_currentNodes[i]).CreatePrettyString());
                _currentNodes[i] = _currentNode;
                break;
            }
        }

        TextEditor.GetComponent<TMP_InputField>().text = "";

        EditResponseUI.SetActive(false);
        TextEditor.SetActive(false);

        UpdateConnectionNodes();
    }
}