using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Drawable : MonoBehaviour
{
    public Color brushColor = Color.black;
    [Range(1f, 10f)] public int brushSize = 1;

    private Texture2D originalTexture;
    public enum BrushType{Square, Circle};
    public BrushType brushType = BrushType.Circle;
    [HideInInspector] public Vector2Int brushPosition;
    private List<Vector2Int> brushCoords = new List<Vector2Int>();
    Renderer canvasRenderer;
    Texture2D canvasTexture;
    [SerializeField] private int textureSizeX = 1024;
    [SerializeField] private int textureSizeY = 1024;    
    public static string Tag = "Ground";
    private bool canvasUpdateRequired = false;
    [SerializeField]
    private float canvasUpdatePeriod = 0.1f;
    private int MIN_BRUSH_SIZE = 1;
    private int MAX_BRUSH_SIZE = 20;
    
    // Start is called before the first frame update
    void Start()
    {   
        canvasRenderer = GetComponent<Renderer>();

        if(canvasRenderer.material.mainTexture == null){
            canvasTexture = new Texture2D(textureSizeX, textureSizeY);
        }
        else{
            if(canvasRenderer.material.mainTexture is Texture2D){
                originalTexture = (Texture2D)canvasRenderer.material.mainTexture;
                textureSizeX = originalTexture.width;
                textureSizeY = originalTexture.height;
                canvasTexture = new Texture2D(originalTexture.width, originalTexture.height);
                canvasTexture.SetPixels32(originalTexture.GetPixels32());
                canvasTexture.Apply();

            }else{
                Debug.LogError("Provided texture is not of type Texture2D");
            }
        }
        canvasRenderer.material.mainTexture = canvasTexture;

        brushCoords = new List<Vector2Int>();
        SetBrushSize(0.1f);

        gameObject.tag = Tag;
        StartCoroutine(UpdateCanvas());
    }
    public int GetBrushSizePixel()
    {
        return brushSize;
    }

    public void SetBrushSize(float scale)
    {//sets brush size within the minimum and maximum range specified by 'scale'
        brushSize = (int) Mathf.Lerp(MIN_BRUSH_SIZE ,MAX_BRUSH_SIZE, scale);
        SetBrush(); //call this method less frequently or call SetBrush seperately if performance is an issue
    }
    private void SetVerticalLineBrush(int starty, int endy, int x)
    {//helper method for SetBrush
        for (int y = starty; y <= endy; y++)
            brushCoords.Add(new Vector2Int(x,y));
    }
    private void SetSquareBrush(){
        for(int x = -brushSize; x<=brushSize; x++){
            SetVerticalLineBrush(-brushSize, brushSize, x);
        }
    }    
    
    private void SetCircleBrush()
    {
        //midpoint circle drawing algorithm
        //**Debug.Log("Circle brush");
        int x = 0;
        int y = brushSize;
        int p = 1 - brushSize;
        SetVerticalLineBrush(-y, y, x);
        while (x < y)
        {
            x++;
            if (p < 0)
            { p += 2 * x + 1; }
            else
            {
                y--;
                p += 2 * x + 1 - 2 * y;
            }
            SetVerticalLineBrush(-y, y, x);
            SetVerticalLineBrush(-y, y, -x);
            SetVerticalLineBrush(-x, x, y);
            SetVerticalLineBrush(-x, x, -y);
            
        }
    }
        

    public void SetBrush(){
        //비어있지 않을 때만 clear하도록
        if(brushCoords.Count > 0){
            brushCoords.Clear(); 
        }

        switch(brushType)
        {
            case BrushType.Square:
                SetSquareBrush();
                break;
            case BrushType.Circle:
                SetCircleBrush();
                break;
        }
    }
    public void SetPixels(){//sets the pixels around brushPos(x,y) given by adding brushPos(x,y) to each brushCoords to the color of brushColor
        int pixx, pixy;
        foreach (Vector2Int brushCoord in brushCoords)
        {
            pixx = brushCoord.x + brushPosition.x;
            pixy = brushCoord.y + brushPosition.y;
            if (pixx >= 0 && pixy >= 0 && pixx < textureSizeX && pixy < textureSizeY)
                canvasTexture.SetPixel(pixx, pixy, brushColor);
        }

        canvasUpdateRequired = true;
    }    
    
    public void SetPixels(int x, int y)
    {//sets the pixels around (x,y) given by adding (x,y) to each brushCoords to the color of brushColor
        //**Debug.Log("Center: (" + x + ", " + y + ")");
        int pixx, pixy;
        foreach (Vector2Int brushCoord in brushCoords)
        {
            pixx = brushCoord.x + x;
            pixy = brushCoord.y + y;
            if (pixx >= 0 && pixy >= 0 && pixx < textureSizeX && pixy < textureSizeY)
            {
                canvasTexture.SetPixel(pixx, pixy, brushColor);
                //**Debug.Log("Pixel set at ("+pixx+", "+pixy+")");
            }
        }

        canvasUpdateRequired = true;
    }

    public void erasePixels(int x, int y)
    {//uses the same brush as painting brush to set the texture to that of original
        if (originalTexture == null)
            return;
        int pixx, pixy;
        foreach (Vector2Int brushCoord in brushCoords)
        {
            pixx = brushCoord.x + x;
            pixy = brushCoord.y + y;
            if (pixx >= 0 && pixy >= 0 && pixx < textureSizeX && pixy < textureSizeY)
            {
                canvasTexture.SetPixel(pixx, pixy, originalTexture.GetPixel(pixx, pixy));
                //**Debug.Log("Pixel set at ("+pixx+", "+pixy+")");
            }
        }
        canvasUpdateRequired = true;
    }    
    public void erasePixels()
    {//uses the same brush as painting brush to set the texture to that of original
        if (originalTexture == null)
            return;
        int pixx, pixy;
        foreach (Vector2Int brushCoord in brushCoords)
        {
            pixx = brushCoord.x + brushPosition.x;
            pixy = brushCoord.y + brushPosition.y;
            if (pixx >= 0 && pixy >= 0 && pixx < textureSizeX && pixy < textureSizeY)
            {
                canvasTexture.SetPixel(pixx, pixy, originalTexture.GetPixel(pixx, pixy));
                //**Debug.Log("Pixel set at ("+pixx+", "+pixy+")");
            }
        }
        canvasUpdateRequired = true;
    }

    
    private IEnumerator UpdateCanvas(){ //check after fixed interval if update is required (for performance reason)
      //i.e. call to Texture2D.Apply() should be infrequent 
        while (true)
        {
            if (canvasUpdateRequired)
            {
                canvasTexture.Apply();
                canvasUpdateRequired = false;
            }
            yield return new WaitForSeconds(canvasUpdatePeriod);
        }
    }

    public int GetTextureSizeX()
    { return textureSizeX; }

    public int GetTextureSizeY()
    { return textureSizeY; }

    public void SetBrushColor(in Color color)
    {
        brushColor = color;
    }

    //Inspector 창 변경사항이 있을 시 호출되는 내장 함수
    //BrushType만 해당 함수로 호출
    private void OnValidate(){
        if(Time.timeScale > 0){
            SetBrush();
        }
    }


}
