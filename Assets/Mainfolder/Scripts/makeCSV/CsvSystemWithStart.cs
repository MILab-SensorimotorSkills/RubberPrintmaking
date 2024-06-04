using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DiggingTest;
using TMPro;

public class CsvSystemWithStart : MonoBehaviour
{
    #region Object
    public TMP_Text startTime;
    public GameObject textBox;
    public GameObject rubber;
    private float rubberHeight;
    private float endPointHeight;
    public GameObject knife;
    private Shovel knifeShovel;
    #endregion

    private CSV_Making csv;
    private int count = 3;
    private bool WriteData = false;

    #region Data
    private float playTime;
    private int frameCount;
    private float depth;
    //도안으로부터 떨어진 정도
    private float distance;
    private float hitPoint;
    #endregion

    void Start(){
        csv = GetComponent<CSV_Making>();
        knifeShovel = knife.GetComponent<Shovel>();
        rubberHeight = rubber.transform.position.y;
    }

    void Update(){
        
        if(Input.GetKeyDown(KeyCode.S) && !WriteData){
            //변수 초기화
            playTime = 0f;
            frameCount = 0;
            depth = 0f;
            distance = 0;

            StartCoroutine(CountdownCoroutine());
        }

        //데이터 쓰기를 멈추는 시점
        if(Input.GetKeyDown(KeyCode.Q) && WriteData){
            WriteData = false;
            startTime.text = "End";
            textBox.SetActive(true);
            
        }

        if(WriteData){
            CulculatePlayTime();
            CountFrame();
            CulculateDepth();

            //데이터를 작성하는 중
            csv.WriteData(playTime, frameCount, depth);
        }
    }

    IEnumerator CountdownCoroutine(){
        while(count >= 0){
            switch(count){
                case 3:
                case 2:
                case 1:
                    startTime.text = count.ToString();
                    break;
                case 0:
                    startTime.text = "Start";
                    break;
            }
            count--;
            yield return new WaitForSeconds(1);
        }
        textBox.SetActive(false);
        //데이터를 쓰기 시작해야하는 시점
        WriteData = true;
    }

    void CountFrame(){
        frameCount++;
    }
    
    void CulculatePlayTime(){
        playTime += Time.deltaTime;
    }

    void CulculateDepth(){
        /*hitPoint = knifeShovel.hitPoint.y;
        depth = hitPoint - rubberHeight; */

    }

    // void CulculateDistance(){

    // }


}
