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
    private int interpolationPixelCount = 1;

    [SerializeField]
    float drawInterval = 0.02f; 

    delegate void setPixelForCanvas(int x, int y);
    setPixelForCanvas canvasDrawOrEraseAt;
    bool erase;

    #region ray
    public GameObject targetObj;
    Vector3 originPos;
    Vector3 originDir;
    private float maxRayDistance = 0.05f;
    
    #endregion

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

    void Update()
    {
        Vector3 originPos = targetObj.transform.position;
        Vector3 originDir = targetObj.transform.forward;

        RaycastHit hit; // RaycastHit 변수 선언

        Ray ray = new Ray(originPos, originDir);

        Debug.DrawRay(originPos, originDir * maxRayDistance, Color.blue); // Ray의 경로를 시각적으로 표시********

        if (Physics.Raycast(ray, out hit, maxRayDistance))
        {

            Debug.Log($"Hit at: {hit.point}, Hit Object: {hit.transform.name}"); //************

            Transform hitobj = hit.transform;
            if (hitobj.CompareTag(Drawable.Tag))
            {
                drawingCanvas = hitobj.GetComponent<Drawable>();
                if (drawingCanvas != null)
                {
                    // drawpos = new Vector2Int();
                    // drawpos.x = (int)(hit.textureCoord.x * drawingCanvas.GetTextureSizeX());
                    // drawpos.y = (int)(hit.textureCoord.y * drawingCanvas.GetTextureSizeY());

                    // //xz대칭이 일어나는 문제 발생
                    // drawpos.x = drawingCanvas.GetTextureSizeX() - drawpos.x - 1;
                    // drawpos.y = drawingCanvas.GetTextureSizeY() - drawpos.y - 1;

                    Vector2Int drawPos = CalculateDrawPosition(hit);
                    Debug.Log($"Draw Position: {drawPos}");

                    AddDrawPositions(drawpos);
                    setBrushToEraseorDraw(erase);
                    SetPixelsBetweenDrawPoints();

                }
                
                Debug.DrawLine(originPos, hit.point, Color.green);
            }
            else
            {   
                drawPoints.Clear();
                Vector3 endPoint = originPos + originDir * maxRayDistance;
                Debug.DrawLine(originPos, endPoint, Color.red);
            }
        }
        else
        {
            SetPixelsBetweenDrawPoints();
            drawPoints.Clear();
            Vector3 endPoint = originPos + originDir * maxRayDistance;
            Debug.DrawLine(originPos, endPoint, Color.red);
        }
    }

    Vector2Int CalculateDrawPosition(RaycastHit hit)
{
    // 충돌 지점의 월드 좌표를 로컬 좌표계로 변환
    Vector3 localHitPoint = hit.transform.InverseTransformPoint(hit.point);
    Debug.Log($"Local Hit Point: {localHitPoint}");

    // 텍스처 크기를 가져옴
    int textureSizeX = Mathf.RoundToInt(drawingCanvas.GetTextureSizeX());
    int textureSizeY = Mathf.RoundToInt(drawingCanvas.GetTextureSizeY());

    // 로컬 좌표계를 기준으로 텍스처 좌표를 계산
    float normalizedX = (localHitPoint.x / hit.transform.lossyScale.x) + 0.5f;
    float normalizedY = (localHitPoint.y / hit.transform.lossyScale.y) + 0.5f;

    // 텍스처 좌표계에서의 X, Y 위치를 정밀하게 계산
    int x = Mathf.Clamp(Mathf.RoundToInt(normalizedX * textureSizeX), 0, textureSizeX - 1);
    int y = Mathf.Clamp(Mathf.RoundToInt(normalizedY * textureSizeY), 0, textureSizeY - 1);

    Debug.Log($"Normalized X: {normalizedX}, Normalized Y: {normalizedY}");
    Debug.Log($"Calculated Draw Position - X: {x}, Y: {y}");

    return new Vector2Int(x, y);
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