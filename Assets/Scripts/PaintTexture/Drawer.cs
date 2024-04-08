using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Drawer : MonoBehaviour
{
    public Drawable drawingCanvas;
    Vector2Int drawpos;    
    private Queue<Vector2Int> drawPoints = new Queue<Vector2Int>();
    public float smoothingFactor = 0.1f;
    private int interpolationPixelCount = 1; //the number of pixels after which to draw new pixel between the drawPoints
                                 //lower the number higher is smoothing (lowest value is one pixel)

    [SerializeField]
    float drawInterval = 0.02f; //the interval of time(in seconds) after which to periodically set pixels of the canvas 

    delegate void setPixelForCanvas(int x, int y);
    setPixelForCanvas canvasDrawOrEraseAt;
    bool erase;   //brush operation whether to erase or draw

    void Start(){
        StartCoroutine(DrawToCanvas());
        erase = false;
    }
    public void setBrushEraser(bool erasing)
    {
        erase = erasing;
    }

    private void setBrushToEraseorDraw(bool erase)
    {
        if (drawingCanvas == null)
            return;
        if (erase)
        { canvasDrawOrEraseAt = drawingCanvas.erasePixels; }
        else
        { canvasDrawOrEraseAt = drawingCanvas.SetPixels; }
    }

    public void SetInterpolationPixelCount(int brushSize)
    {
        interpolationPixelCount = ((int)(brushSize * smoothingFactor)) + 1;  //minimum value is one pixel
        //Debug.Log("IntPix" + interpolationPixelCount);
    }

    void Update(){
        if(Input.GetMouseButtonUp(0)){
            SetPixelsBetweenDrawPoints();
            drawPoints.Clear();
        }else if(Input.GetMouseButton(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()){
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit)){
                Transform hitobj = hit.transform;
                if(hitobj.CompareTag(Drawable.Tag)){
                    drawingCanvas = hitobj.GetComponent<Drawable>();
                    if(drawingCanvas != null){
                        drawpos = new Vector2Int();
                        drawpos.x =(int)(hit.textureCoord.x * drawingCanvas.GetTextureSizeX());
                        drawpos.y = (int)(hit.textureCoord.y * drawingCanvas.GetTextureSizeY());
                        AddDrawPositions(drawpos);
                        setBrushToEraseorDraw(erase);
                    }
                }
            }
        }
    }
        
    private void SetPixelsBetweenDrawPoints()
    {
        while (drawPoints.Count > 1)
        {
            Vector2Int startPos = drawPoints.Dequeue();
            Vector2Int endPos = drawPoints.Peek();
            InterpolateDrawPositions(startPos, endPos);
        }
    }

    IEnumerator DrawToCanvas()
    {//actually responsible for setting the pixels of the canvas
        while(true)
        {
            SetPixelsBetweenDrawPoints();
            //check after every fixed periods to see if new pixels need to be set
            yield return new WaitForSeconds(drawInterval);
        }
    }

    void InterpolateDrawPositions(Vector2Int startPos, Vector2Int endPos)
    {//using DDA algorithm #use bresham's algorithm for more performance
        int dx = endPos.x - startPos.x;
        int dy = endPos.y - startPos.y;
        float xinc, yinc, x, y;
        int steps = (Math.Abs(dx) > Math.Abs(dy)) ? Math.Abs(dx) : Math.Abs(dy);
        xinc = ((float)dx / steps) * interpolationPixelCount;
        yinc = ((float)dy / steps) * interpolationPixelCount;
        x = startPos.x;
        y = startPos.y;

        for(int k=0; k < steps; k += interpolationPixelCount)
        {
            canvasDrawOrEraseAt((int)Math.Round(x), (int)Math.Round(y));
            x += xinc;
            y += yinc;
        }
        canvasDrawOrEraseAt(endPos.x, endPos.y);
    }

    void AddDrawPositions(Vector2Int newDrawPos)
    {
        drawPoints.Enqueue(newDrawPos);
    }
}
