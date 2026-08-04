using UnityEngine;

public abstract class RoomModifierSO : ScriptableObject
{
    [Header("Info")]
    public string modifierName;
    [TextArea] public string description;

    public abstract void Apply();
    public abstract void Remove();
}