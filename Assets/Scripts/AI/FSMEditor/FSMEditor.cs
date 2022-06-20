using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;

public class FSMEditor : EditorWindow
{
    bool _creatingNode = false;
    byte _currentID = 0;
    bool _dragging = false;
    readonly float _editMenuLabelWidth = 100f;
    readonly float _editMenuLabelHeight = 50f;
    readonly float _editMenuButtonWidth = 100f;
    readonly float _editMenuButtonHeight = 20f;
    GUIStyle _normalNodeGUIStyle;
    GUIStyle _selectedNodeGUIStyle;
    int _creatingConnectionFrom = -1;
    int _draggedNode = -1;
    int _selectedNodeIndex = -1;
    List<Rect> _occludingRects = new List<Rect>();
    List<UINode> _currentNodes = new List<UINode>();
    string _currentSavePath = "";
    string _nameFieldText = "";
    Texture2D _backgroundTexture;
    Texture2D _normalNodeTexture;
    Texture2D _selectedNodeTexture;
    Vector2 _ctxMenuPosition;
    Vector2 _offset = Vector2.zero;

    [MenuItem("Window/AI/FSM Editor")]
    static void OnOpen()
    {
        FSMEditor window = GetWindow<FSMEditor>();
        window.titleContent = new GUIContent("FSM Editor");
    }

    void OnEnable()
    {
        _selectedNodeIndex = -1;
        _creatingConnectionFrom = -1;

        _currentID = 0;
    }

    void OnFocus()
    {
        _normalNodeTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        _normalNodeTexture.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.5f));
        _normalNodeTexture.Apply();

        _normalNodeGUIStyle = new GUIStyle();
        _normalNodeGUIStyle.normal.background = _normalNodeTexture;
        _normalNodeGUIStyle.alignment = TextAnchor.MiddleCenter;

        _selectedNodeTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        _selectedNodeTexture.SetPixel(0, 0, new Color(1, 0.5f, 0));
        _selectedNodeTexture.Apply();

        _selectedNodeGUIStyle = new GUIStyle();
        _selectedNodeGUIStyle.normal.background = _selectedNodeTexture;
        _selectedNodeGUIStyle.alignment = TextAnchor.MiddleCenter;
    }

    void OnGUI()
    {
        _backgroundTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        _backgroundTexture.SetPixel(0, 0, new Color(0.2030527f, 0.2197486f, 0.3679245f));
        _backgroundTexture.Apply();

        GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), _backgroundTexture, ScaleMode.StretchToFill);

        DrawGrid(20, 0.2f, Color.gray);
        HandleEvents();

        DrawTransitions();
        for (int i = 0; i < _currentNodes.Count; i++)
        {
            DrawNode(i);
        }

        if (_selectedNodeIndex != -1)
        {
            EditMenu();
        }

        if (_creatingNode)
        {
            _nameFieldText = GUI.TextField(new Rect(0, 0, 100, 20), _nameFieldText);

            if (GUI.Button(new Rect(100, 0, 50, 20), "Create Node"))
            {
                CreateNode();
            }
        }

        if(_creatingConnectionFrom != -1)
        {
            UINode fromNode = _currentNodes[_creatingConnectionFrom];
            Vector2 fromPos = fromNode.Rect.position + fromNode.Offset + fromNode.Rect.size/2;
            DrawLine(fromPos, Event.current.mousePosition, 2f);
            Repaint();
        }
    }

    void EditMenu()
    {
        Rect rect = new Rect(position.width - _editMenuLabelWidth, 0, _editMenuLabelWidth, _editMenuLabelHeight);

        GUIStyle editStyle = new GUIStyle
        {
            fontSize = 25,
            padding = new RectOffset(0, 10, 10, 0),
            border = new RectOffset(0, 10, 10, 0),
            alignment = TextAnchor.UpperRight
        };

        EditMenuLabel(ref rect, "ID: " + _currentNodes[_selectedNodeIndex].ContainedState.ID, editStyle);

        /* Actions */
        EditMenuLabel(ref rect, "Actions", editStyle);

        EditMenuLabel(ref rect, "Enter Actions", editStyle);
        /* Enter Actions */
        for (int i = 0; i < _currentNodes[_selectedNodeIndex].ContainedState.EnterActions.Count; i++)
        {
            int newValue =
                EditMenuDropDown(ref rect, _currentNodes[_selectedNodeIndex].ContainedState.EnterActions[i], ClassRefs.AIActions.Keys.ToArray());

            if (newValue == ClassRefs.AIActions.Count - 1)
            {
                _currentNodes[_selectedNodeIndex].ContainedState.EnterActions.Add(0);
            }
            else
            {
                _currentNodes[_selectedNodeIndex].ContainedState.EnterActions[i] = newValue;
            }
        }

        EditMenuLabel(ref rect, "Tick Actions", editStyle);
        /* Tick Actions */
        for (int i = 0; i < _currentNodes[_selectedNodeIndex].ContainedState.TickActions.Count; i++)
        {
            int newValue =
                EditMenuDropDown(ref rect, _currentNodes[_selectedNodeIndex].ContainedState.TickActions[i], ClassRefs.AIActions.Keys.ToArray());

            if (newValue == ClassRefs.AIActions.Count - 1)
            {
                _currentNodes[_selectedNodeIndex].ContainedState.TickActions.Add(0);
            }
            else
            {
                _currentNodes[_selectedNodeIndex].ContainedState.TickActions[i] = newValue;
            }
        }

        EditMenuLabel(ref rect, "Exit Actions", editStyle);
        /* Exit Actions */
        for (int i = 0; i < _currentNodes[_selectedNodeIndex].ContainedState.ExitActions.Count; i++)
        {
            int newValue =
                EditMenuDropDown(ref rect, _currentNodes[_selectedNodeIndex].ContainedState.ExitActions[i], ClassRefs.AIActions.Keys.ToArray());

            if (newValue == ClassRefs.AIActions.Count - 1)
            {
                _currentNodes[_selectedNodeIndex].ContainedState.ExitActions.Add(0);
            }
            else
            {
                _currentNodes[_selectedNodeIndex].ContainedState.ExitActions[i] = newValue;
            }
        }



        /* Transitions */
        EditMenuLabel(ref rect, "Transitions", editStyle);

        for (int i = 0; i < _currentNodes[_selectedNodeIndex].ContainedState.Transitions.Count; i++)
        {
            AITransition curr = _currentNodes[_selectedNodeIndex].ContainedState.Transitions[i];

            EditMenuLabel(ref rect, $"To ID: {curr.ToID}", editStyle);

            _currentNodes[_selectedNodeIndex].ContainedState.Transitions[i].Condition = EditMenuDropDown(ref rect, _currentNodes[_selectedNodeIndex].ContainedState.Transitions[i].Condition, ClassRefs.AIConditions.Keys.ToArray());
            
        }
    }

    void OpenContextMenu(Vector2 mousePosition)
    {
        GenericMenu cxtMenu = new GenericMenu();
        for (int i = 0; i < _currentNodes.Count; i++)
        {
            Rect offsetRect = new Rect(_currentNodes[i].Rect.position + _currentNodes[i].Offset, _currentNodes[i].Rect.size);
            if (offsetRect.Contains(mousePosition))
            {
                cxtMenu.AddItem(new GUIContent("Remove Node"), false, () => RemoveNode(i));
                cxtMenu.AddItem(new GUIContent("Make Transition"), false, () => StartMakingTransition(i));
                cxtMenu.ShowAsContext();
                return;
            }
        }
        cxtMenu.AddItem(new GUIContent("Create Node"), false, StartCreateNode);
        cxtMenu.AddItem(new GUIContent("New"), false, Reset);
        cxtMenu.AddItem(new GUIContent("Save"), false, Save);
        cxtMenu.AddItem(new GUIContent("Save As"), false, SaveAs);
        cxtMenu.AddItem(new GUIContent("Load"), false, Load);
        cxtMenu.ShowAsContext();
    }

    void Reset()
    {
        if (!EditorUtility.DisplayDialog("New FSM", "Are you sure you want to start a new FSM? All unsaved changes will be lost", "Yes", "No")) return;

        _currentNodes = new List<UINode>();
        _offset = Vector2.zero;
        for (int i = 0; i < _currentNodes.Count; i++)
        {
            _currentNodes[i].Offset = Vector2.zero;
        }
        _selectedNodeIndex = -1;
    }

    void HandleEvents()
    {
        Event e = Event.current;
        switch (e.type)
        {
            case EventType.MouseDown:
                _dragging = false;
                break;

            case EventType.MouseDrag:
                if (e.button == 0)
                {
                    if (_draggedNode != -1)
                    {
                        _currentNodes[_draggedNode].Rect.position += e.delta;
                        GUI.changed = true;
                        return;
                    }
                    for (int i = 0; i < _currentNodes.Count; i++)
                    {
                        Rect offsetRect = new Rect(_currentNodes[i].Rect.position + _currentNodes[i].Offset, _currentNodes[i].Rect.size);
                        if (offsetRect.Contains(e.mousePosition))
                        {
                            _currentNodes[i].Rect.position += e.delta;
                            GUI.changed = true;
                            _draggedNode = i;
                            return;
                        }
                    }
                    _offset += e.delta;
                    _dragging = true;
                    for (int i = 0; i < _currentNodes.Count; i++)
                    {
                        _currentNodes[i].Offset += e.delta;
                    }
                    GUI.changed = true;
                }
                break;

            case EventType.MouseUp:
                if (_creatingConnectionFrom != -1)
                {
                    for (int i = 0; i < _currentNodes.Count; i++)
                    {
                        Rect offsetRect = new Rect(_currentNodes[i].Rect.position + _currentNodes[i].Offset, _currentNodes[i].Rect.size);
                        if (offsetRect.Contains(e.mousePosition))
                        {
                            _currentNodes[_creatingConnectionFrom].ContainedState.Transitions.Add(new AITransition(_currentNodes[i].ContainedState.ID, -1));
                            _creatingConnectionFrom = -1;
                            return;
                        }
                    }
                }
                _draggedNode = -1;

                if (e.button == 0)
                {
                    //If the button is above an occluding rect like for example the dropdown lists, then that shouldn't be taken as a sign of selection.
                    foreach(Rect i in _occludingRects)
                    {
                        if(i.Contains(e.mousePosition))
                        {
                            return;
                        }
                    }
                    //If the user is currently dragging the node then no selection logic should execute.
                    if (_dragging) return;
                    //Runs through the list of current nodes and checks each of them for overlap with the cursor.
                    for (int i = 0; i < _currentNodes.Count; i++)
                    {
                        //Calculates the Rect currently representing the node.
                        Rect offsetRect = new Rect(_currentNodes[i].Rect.position + _currentNodes[i].Offset, _currentNodes[i].Rect.size);
                        if (offsetRect.Contains(e.mousePosition))
                        {
                            //If the user clicks on the already selected node, then this code unselects the node.
                            if(_selectedNodeIndex == i)
                            {
                                UnselectCurrentNode();
                                return;
                            }
                            //If the user clicks on an unselected node, that node should be selected.
                            SelectNode(i);
                            return;
                        }
                    }
                    //If the user clicks on the empty abyss, then that should also be taken as an unselection.
                    UnselectCurrentNode();
                }    
                else if (e.button == 1)
                {
                    OpenContextMenu(e.mousePosition);
                    _ctxMenuPosition = e.mousePosition;
                }
                break;

            case EventType.KeyDown:
                if (e.keyCode == KeyCode.Delete && _selectedNodeIndex != -1)
                {
                    RemoveNode(_selectedNodeIndex);
                }
                if (e.keyCode == KeyCode.Escape && _creatingNode)
                {
                    _creatingNode = false;
                    GUI.changed = true;
                }
                break;
        }
    }

    void StartCreateNode()
    {
        _creatingNode = true;
        _selectedNodeIndex = -1;
    }

    void CreateNode()
    {
        _currentNodes.Add(new UINode(new AIState(_nameFieldText, _currentID), new Rect(_ctxMenuPosition - new Vector2(200, 50) / 2, new Vector2(200, 50))));
        _currentNodes[_currentNodes.Count - 1].ContainedState.TickActions.Add(0);
        _currentNodes[_currentNodes.Count - 1].ContainedState.EnterActions.Add(0);
        _currentNodes[_currentNodes.Count - 1].ContainedState.ExitActions.Add(0);

        _currentID++;
        _creatingNode = false;
        _nameFieldText = "";
        GUI.changed = true;
    }

    void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        Vector3 newOffset = new Vector3(_offset.x % gridSpacing, _offset.y % gridSpacing, 0);

        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
        }

        for (int i = 0; i < heightDivs; i++)
        {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * i, 0) + newOffset, new Vector3(position.width, gridSpacing * i, 0f) + newOffset);
        }

        Handles.color = Color.black;
        Handles.EndGUI();
    }

    void DrawNode(int nodeIndex)
    {
        UINode node = _currentNodes[nodeIndex];
        GUIStyle chosenStyle = nodeIndex == _selectedNodeIndex ? _selectedNodeGUIStyle : _normalNodeGUIStyle;

        GUI.Box(new Rect(node.Rect.position + node.Offset, node.Rect.size), node.ContainedState.Name, chosenStyle);

    }

    void DrawTransitions()
    {
        foreach (UINode node in _currentNodes)
        {
            if (node.ContainedState.Transitions.Count() != 0)
                foreach (AITransition i in node.ContainedState.Transitions)
                {
                    UINode toNode = _currentNodes.Find(x => x.ContainedState.ID == i.ToID);

                    DrawLine(node.Rect.position + node.Rect.size / 2 + node.Offset, toNode.Rect.position + toNode.Rect.size / 2 + toNode.Offset, 2f);
                }
        }
    }

    void DrawLine(Vector2 from, Vector2 to, float width) => 
        Handles.DrawBezier(from, to, from - Vector2.left * 50f, to + Vector2.left * 50f, Color.white, null, width);

    void RemoveNode(int nodeIndex)
    {
        if (!EditorUtility.DisplayDialog("Remove Node", "Are you sure you want to delete the node?", "Yes", "No"))
            return;

        if (_selectedNodeIndex == nodeIndex)
        {
            UnselectCurrentNode();
        }
        _currentNodes.RemoveAt(nodeIndex);

        for(int i = 0; i < _currentNodes.Count(); i++)
        {
            for(int x = 0; x < _currentNodes[i].ContainedState.Transitions.Count(); x++)
            {
                if(_currentNodes[i].ContainedState.Transitions[x].ToID == nodeIndex)
                {
                    _currentNodes[i].ContainedState.Transitions.RemoveAt(x);
                }
                //Compensates for something I literally chose not to do. 
                //This is either me being big brain or stupid.
                //Specifically it was to basically defragment the node IDs so when a node is removed the nodes above in the ID order are moved one down to fill in the space.
                /*else
                {
                    if(_currentNodes[i].ContainedState.Transitions[x].ToID > nodeIndex)
                    {
                        _currentNodes[i].ContainedState.Transitions[x].ToID--;
                    }
                }*/
            }
        }

        GUI.changed = true;
    }

    void StartMakingTransition(int fromNode)
    {
        _creatingConnectionFrom = fromNode;
        _selectedNodeIndex = -1;
    }

    //Sets the selected node to the one found at the provided index.
    void SelectNode(int nodeIndex)
    {
        if (_selectedNodeIndex != -1)
        {
            /*
             * Stars from 1 so that there is always one entry remaining, 
             * if it were otherwise the node's actions could become uneditable
             * as there would be no entries to click "add new entry" on.
            */
            for(int i = 1; i < _currentNodes[nodeIndex].ContainedState.TickActions.Count; i++)
            {
                if(_currentNodes[nodeIndex].ContainedState.TickActions[i] == 0)
                {
                    _currentNodes[nodeIndex].ContainedState.TickActions.RemoveAt(i);
                    i--;
                }
            }
        }

        _creatingNode = false;

        //Sets the selected node index to the chosen node's.
        _selectedNodeIndex = nodeIndex;
        GUI.changed = true;
    }

    void UnselectCurrentNode()
    {
        if (_selectedNodeIndex != -1)
        {
            /*
             * Stars from 1 so that there is always one entry remaining, 
             * if it were otherwise the node's actions could become uneditable
             * as there would be no entries to click "add new entry" on.
            */
            for (int i = 1; i < _currentNodes[_selectedNodeIndex].ContainedState.EnterActions.Count; i++)
            {
                if (_currentNodes[_selectedNodeIndex].ContainedState.EnterActions[i] == 0)
                {
                    _currentNodes[_selectedNodeIndex].ContainedState.EnterActions.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 1; i < _currentNodes[_selectedNodeIndex].ContainedState.TickActions.Count; i++)
            {
                if (_currentNodes[_selectedNodeIndex].ContainedState.TickActions[i] == 0)
                {
                    _currentNodes[_selectedNodeIndex].ContainedState.TickActions.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 1; i < _currentNodes[_selectedNodeIndex].ContainedState.ExitActions.Count; i++)
            {
                if (_currentNodes[_selectedNodeIndex].ContainedState.ExitActions[i] == 0)
                {
                    _currentNodes[_selectedNodeIndex].ContainedState.ExitActions.RemoveAt(i);
                    i--;
                }
            }
        }

        _selectedNodeIndex = -1;
        GUI.changed = true;
    }

    void Load()
    {
        string path = EditorUtility.OpenFilePanel("Open AI File", Application.dataPath, "asset");
        path = path.Replace(Application.dataPath, "Assets/");

        if (path == "") return;
        if (!EditorUtility.DisplayDialog("Load File", "Are you sure you want to load a new file? This will erase all unsaved changes.", "Yes", "No")) return;

        _currentNodes = AssetDatabase.LoadAssetAtPath<AIObject>(path).EditorInfo.ToList();
    }

    void Save()
    {
        for(int x = 0; x < _currentNodes.Count; x++)
        {
            for(int y = 0; y < _currentNodes[x].ContainedState.Transitions.Count; y++)
            {
                if(_currentNodes[x].ContainedState.Transitions[y].Condition == -1)
                {
                    _currentNodes[x].ContainedState.Transitions.RemoveAt(y);
                    y--;
                }
            }
        }

        AIObject lecsó = CreateInstance<AIObject>();
        lecsó.EditorInfo = _currentNodes.ToArray();
        lecsó.StateMachine = _currentNodes.ConvertAll(x => x.ContainedState).ToArray();
        if (_currentSavePath == "")
        {
            _currentSavePath = EditorUtility.SaveFilePanel("Save AI File", Application.dataPath, "NewFSMAsset", "asset");
            _currentSavePath = _currentSavePath.Replace(Application.dataPath, "Assets/");
        }
        AssetDatabase.CreateAsset(lecsó, _currentSavePath);
        AssetDatabase.SaveAssets();
    }

    void SaveAs()
    {
        _currentSavePath = "";
        Save();
    }

    void EditMenuLabel(ref Rect rect, string text, GUIStyle editStyle)
    {
        rect.width = _editMenuLabelWidth;
        rect.height = _editMenuLabelHeight;
        GUI.Label(rect, text, editStyle);
        rect.y += _editMenuLabelHeight;
    }

    int EditMenuDropDown(ref Rect rect, int currentIndex, string[] options)
    {
        rect.width = _editMenuButtonWidth;
        rect.height = _editMenuButtonHeight;
        int returnValue = EditorGUI.Popup(rect, currentIndex, options);

        if (!_occludingRects.Contains(rect))
        {
            _occludingRects.Add(rect);
        }

        rect.y += _editMenuButtonHeight;

        return returnValue;
    }
}