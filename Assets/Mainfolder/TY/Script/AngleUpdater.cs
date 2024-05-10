using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AngleUpdater : MonoBehaviour
{   
    public Text OutputText;
    private Vector3 VectorOfGomu;

    public float Angle;
    public float Rotation;

    private static string OUTPUT_TEXT = " Angle: {0}\n Rotation: {1}";

    // Start is called before the first frame update
    void Start()
    {
        if(OutputText == null){
            OutputText = GameObject.Find("OutputText").GetComponent<Text>();
        }
    }

    void OnTriggerStay(Collider other){
        Vector3 direction  = other.transform.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        Vector3 angles = rotation.eulerAngles;
        Angle = angles.y;
        Rotation = transform.rotation.eulerAngles.y;
        
        VectorOfGomu = other.transform.up;
        Angle = Vector3.Angle(transform.forward, VectorOfGomu);

        OutputText.text = string.Format(OUTPUT_TEXT, Angle, Rotation);
    }

    void OnTriggerExit(Collider other){
        OutputText.text = string.Format(OUTPUT_TEXT, null, null);
    }

}
