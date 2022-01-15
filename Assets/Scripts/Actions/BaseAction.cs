using UnityEngine;

[System.Serializable]
public class BaseAction
{
    [Tooltip("Name of action")]
    public string name;
    [Tooltip("Icon to be shown in action panel")]
    public Sprite icon;
    [Tooltip("Name of script to be ran when performing action")]
    public string actionScript;
    [Tooltip("Unit's level required to be able to perform the action")]
    public int levelRequired;
    [Tooltip("Shortcut key to be pressed to perform the action")]
    public KeyCode shortcutKey;
}
