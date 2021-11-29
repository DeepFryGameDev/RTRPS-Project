using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveCamera : MonoBehaviour
{
    bool isPanning;

    enum panDirections
    {
        UP,
        DOWN,
        LEFT,
        RIGHT,
        IDLE
    }

    panDirections panDirection;

    void Update()
    {
        if (isPanning)
            PanCamera();
    }

    public void MouseEntered(int direction)
    {
        switch (direction)
        {
            case 0:
                panDirection = panDirections.UP;
                break;
            case 1:
                panDirection = panDirections.DOWN;
                break;
            case 2:
                panDirection = panDirections.LEFT;
                break;
            case 3:
                panDirection = panDirections.RIGHT;
                break;
        }

        isPanning = true;
    }

    public void MouseExited()
    {
        isPanning = false;

        panDirection = panDirections.IDLE;
    }

    void PanCamera()
    {
        Debug.Log("Panning " + panDirection);
    }
}
