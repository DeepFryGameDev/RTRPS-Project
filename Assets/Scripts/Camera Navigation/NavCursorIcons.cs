using UnityEngine;

// This script holds the cursor icons for camera navigation, hovering over resources, and processing animations at the mouse cursor when moving to a task
public class NavCursorIcons : MonoBehaviour
{
    [Tooltip("Default cursor graphic")]
    public Texture2D idle;
    [Tooltip("Cursor graphic for panning at upper border")]
    public Texture2D panUp;
    [Tooltip("Cursor graphic for panning at bottom border")]
    public Texture2D panDown;
    [Tooltip("Cursor graphic for panning at left border")]
    public Texture2D panLeft;
    [Tooltip("Cursor graphic for panning at right border")]
    public Texture2D panRight;
    [Tooltip("Cursor graphic for panning both at upper and right border")]
    public Texture2D panDiagUR;
    [Tooltip("Cursor graphic for panning both at upper and left border")]
    public Texture2D panDiagUL;
    [Tooltip("Cursor graphic for panning both at bottom and left border")]
    public Texture2D panDiagDL;
    [Tooltip("Cursor graphic for panning both at bottom and right border")]
    public Texture2D panDiagDR;
    [Tooltip("Cursor graphic for drag panning with right mouse click")]
    public Texture2D panDrag;
    [Tooltip("Cursor graphic for rotating camera with scroll wheel click")]
    public Texture2D rotate;

    [Tooltip("Cursor graphic for hovering over a wood resource")]
    public Texture2D woodResource;
    [Tooltip("Cursor graphic for hovering over an ore resource")]
    public Texture2D oreResource;
    [Tooltip("Cursor graphic for hovering over a food resource")]
    public Texture2D foodResource;

    [Tooltip("Cursor graphic for hovering over a buildable object")]
    public Texture2D buildBuilding;

    [Tooltip("Cursor animation for moving to a task")]
    public GameObject taskTargetPrefab;
    [Tooltip("Larger number results in quicker fade time for task cursor anim")]
    [Range(1, 3)] public float taskAnimFadeFactor;
    [Tooltip("Larger number results in larger graphic at start of instantiation")]
    [Range(1, 5)] public float taskAnimBaseScale;
}
