using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CSV_Making : MonoBehaviour
{
    //사용자 이름 및 날짜 시간 작성
    private string Tester = "Test";
    private float currentTime;
    private string filePath;

    void Start(){
        filePath = Path.Combine(Application.dataPath, "Mainfolder/CSV/" + Tester + ".csv");
    }

    public void WriteCol(){
        using (StreamWriter sw = new StreamWriter(filePath, true)){

            sw.WriteLine("");
            sw.WriteLine("");
            sw.WriteLine("Task Start");
            sw.WriteLine("Time,Frame,Depth");
        }
    }

    public void WriteData(float time, int frame, float depth){

        using (StreamWriter sw = new StreamWriter(filePath, true)){
            sw.WriteLine($"{time}, {frame}, {depth}");
        }
    }
}
