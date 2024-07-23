using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawManager : MonoBehaviour
{
    //public Drawable drawingCanvas;
    Drawer drawingAgent;

    void Start()
    {
        drawingAgent = GetComponent<Drawer>();
    }

    public void setBrush_erasing(bool erase)
    {
        drawingAgent.setBrushEraser(erase);
    }

}
