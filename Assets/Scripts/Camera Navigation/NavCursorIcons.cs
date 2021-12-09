using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
