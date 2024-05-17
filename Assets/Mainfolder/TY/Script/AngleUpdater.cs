using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class AngleUpdater : MonoBehaviour
{   
    #region OutPut
    public Text OutputText;
    private Vector3 VectorOfRubber;
    private Vector3 direction;
    private Quaternion rotation;
    private Vector3 angles;
    public float Angle;
    public float Rotation;
    #endregion

    #region Ray
    public GameObject targetObj;
    Vector3 originPos;
    Vector3 originDir;
    private float maxRayDistance = 0.18f;
    #endregion

    private static string OUTPUT_TEXT = " Angle: {0}\n Rotation: {1}";

    // Start is called before the first frame update
    void Start()
    {
        if(OutputText == null){
            OutputText = GameObject.Find("OutputText").GetComponent<Text>();
        }
    }

    void Update(){
        
        originPos = targetObj.transform.position;
        originDir = targetObj.transform.up;

        RaycastHit hit;
        Ray ray = new Ray(originPos, originDir);

        if(Physics.Raycast(ray, out hit, maxRayDistance)){

            Transform hitobj = hit.transform;
            
            if(hitobj.CompareTag("Ground")){

                direction = hitobj.transform.position - originPos;
                rotation = Quaternion.LookRotation(direction);
                Vector3 angles = rotation.eulerAngles;
                Angle = angles.y;
                Rotation = transform.rotation.eulerAngles.y;

                VectorOfRubber = hitobj.transform.up;
                Angle = Vector3.Angle(transform.forward, VectorOfRubber);

                OutputText.text = string.Format(OUTPUT_TEXT, Angle, Rotation);

                Debug.DrawLine(originPos, hit.point, Color.green);

            }else{

                OutputText.text = string.Format(OUTPUT_TEXT, null, null);
            }
        }else{
                Vector3 endPoint = originPos + originDir * maxRayDistance;
                Debug.DrawLine(originPos, endPoint, Color.red);
        }
    }
}
