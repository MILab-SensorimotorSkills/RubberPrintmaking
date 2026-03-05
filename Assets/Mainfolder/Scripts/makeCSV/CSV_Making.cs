using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class CSV_Making : MonoBehaviour
{
    // 사용자 이름 및 날짜 시간 작성
    private string Tester;
    private string filePath;

    void Start()
    {
        Tester = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        filePath = Path.Combine(Application.dataPath, "Mainfolder/CSV/Data_" + Tester + ".csv");
    }

    // public void WriteCol()
    // {
    //     using (StreamWriter sw = new StreamWriter(filePath, true))
    //     {
    //         sw.WriteLine("Time, Frame, Distance, Depth, DisAvg, DisVar, DepthAvg, DepthVar, DisAvg0x, DisVar0x, DepthAvg0x, DepthVar0x");
    //         sw.WriteLine("");
    //     }
    // }
    public void WriteCol()
    {
        using (StreamWriter sw = new StreamWriter(filePath, true))
        {
            sw.WriteLine(
                "Time,Frame,FeedbackType,Distance,Depth,DisAvg,DisVar,DepthAvg,DepthVar,DisAvg0x,DisVar0x,DepthAvg0x,DepthVar0x," +
                "Level,Alpha,AlphaMax,GoodTh,BadTh,eEMA,eVar,x,sd,Fmag,FbaseMag,FgMag,FdMag"
            );
            sw.WriteLine("");
        }
    }


    // public void WriteData(float time, int frame, float? distance, float? depth, float disAvg, float disVar, float depthAvg, float depthVar, float disAvg0x, float disVar0x, float depthAvg0x, float depthVar0x)
    // {
    //     using (StreamWriter sw = new StreamWriter(filePath, true))
    //     {
    //         sw.WriteLine($"{time}, {frame}, {distance}, {depth}, {disAvg}, {disVar}, {depthAvg}, {depthVar}, {disAvg0x}, {disVar0x}, {depthAvg0x}, {depthVar0x}");
    //     }
    // }
    public void WriteData(
        float time, int frame, string feedbackType, float? distance, float? depth,
        float disAvg, float disVar, float depthAvg, float depthVar,
        float disAvg0x, float disVar0x, float depthAvg0x, float depthVar0x,

        int level, float alpha, float alphaMax, float goodTh, float badTh,
        float eEma, float eVar, float x, float sd,
        float Fmag, float FbaseMag, float FgMag, float FdMag
    )
    {
        using (StreamWriter sw = new StreamWriter(filePath, true))
        {
            sw.WriteLine(
                $"{time},{frame},{feedbackType},{distance},{depth}," +
                $"{disAvg},{disVar},{depthAvg},{depthVar}," +
                $"{disAvg0x},{disVar0x},{depthAvg0x},{depthVar0x}," +
                $"{level},{alpha},{alphaMax},{goodTh},{badTh}," +
                $"{eEma},{eVar},{x},{sd},{Fmag},{FbaseMag},{FgMag},{FdMag}"
            );
        }
    }


    public void WriteAccuracy(float matchingAccuracy)
    {
        using (StreamWriter sw = new StreamWriter(filePath, true))
        {
            sw.WriteLine($"Matching Accuracy: {matchingAccuracy}%");
        }
    }


    public void CloseAndSave()
    {
        Debug.Log("CSV file saved to: " + filePath);
    }
}
