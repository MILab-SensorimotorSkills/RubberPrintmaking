using UnityEngine;
using UnityEngine.UI;

public class AngleUpdater : MonoBehaviour
{
    #region output
    public Text OutputText;
    private static string OUTPUT_TEXT = "Angle: {0}\nRotation: {1}\n";
    #endregion

    #region rotation_angle
    private int angle = 0;
    private int rotation = 0;
    private int frameCount = 0;
    #endregion

    public GameObject Cube;
    private Renderer renderer;
    private Color orange = new Color(1f, 0.5f, 0f);

    private void Start()
    {
        // OutputText가 null인 경우에만 GameObject에서 찾아옴
        if (OutputText == null)
        {
            Debug.Log("OutputText is not assigned.");
        }

        renderer = Cube.GetComponent<Renderer>();

    }

    void Update(){
        if(frameCount<10){
            rotation += (int)transform.rotation.eulerAngles.z;
            angle += (int)transform.rotation.eulerAngles.x;
            frameCount++;  
        }else{
            rotation = (int)(rotation/frameCount);
            angle = (int)(angle/frameCount);
            OutputText.text = string.Format(OUTPUT_TEXT, angle, rotation);
            ChangeCube();
            frameCount = 0;
            rotation = 0;
            angle = 0;
        }

    }

    void ChangeCube(){
    
        if(((rotation >= 0 && rotation <= 50) || (rotation >= 310 && rotation <=360)) && (angle >= 0 && angle<=20)){
            renderer.material.color = Color.green;
        }else if(((rotation > 0 && rotation <= 80) || (rotation >= 280 && rotation < 360)) && (angle >=0 && angle <= 50)){
            renderer.material.color = Color.blue;
        }else if(((rotation > 0 && rotation <= 110) || (rotation >= 250 && rotation < 360)) && (angle >=0 && angle <= 80)){
            renderer.material.color = orange;
        }else{
            renderer.material.color = Color.red;
        }
    }
}
