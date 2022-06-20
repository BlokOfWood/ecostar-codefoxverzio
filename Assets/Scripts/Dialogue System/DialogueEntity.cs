using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Entity", menuName = "Dialogue/Dialogue Entity")]
public class DialogueEntity : ScriptableObject
{
    public string EntityName;
    public string EntityDialogueFile;
}

[System.Serializable]
public class DialogueEntry 
{
    public Float2 EditorPosition;
    public string ID;
    public string[] DialogueTexts;
    public (string, string, Float2)[] Responses;
}

[System.Serializable]
public struct Float2
{
    public float X;
    public float Y;

    public Float2(float _x, float _y)
    {
        X = _x;
        Y = _y;
    }

    public static implicit operator Vector2(Float2 float2)
    {
        return new Vector2(float2.X, float2.Y);
    }

    public static implicit operator Float2(Vector2 vector2)
    {
        return new Float2(vector2.x, vector2.y);
    }
    public static implicit operator Float2((float,float) tuple)
    {
        return new Float2(tuple.Item1, tuple.Item2);
    }
    
    public override string ToString()
    {
        return $"X Value: {X} Y Value: {Y}";
    }
}