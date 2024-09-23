using UnityEngine;
// using Unity.Barracuda;
using System.Linq;
using System.Collections.Generic;
using Unity.XR.Oculus;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;

public class OnnxInference : MonoBehaviour
{
    [SerializeField] private string modelPath;
    public string modelAsset;
    // private Model runtimeModel; //barracuda 모델 불러오기
    // private IWorker worker;

    private int timeSteps = 10; // 모델이 기대하는 시퀀스 길이
    private int inputSize = 3;  // 입력 특징 수
    // private int batchSize = 1;
    private Models models;  
    // private bool isInferenceRunning = false;

    void Start()
    {
        models = new Models(modelPath);
        // models = new Models("C:\\Users\\jy\\Desktop\\RubberPrintmaking\\Assets\\ONNX\\lstm_test4.onnx");
        // ONNX 모델 로드 및  초기화
        // runtimeModel = ModelLoader.Load(modelAsset);
        // // worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Compute, runtimeModel); //GPU를 사용
        // worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, runtimeModel);
    }

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.I)) //I를 눌렀을 때 추론
        // {
        //     if(isInferenceRunning)
        //     {
        //         StopInference();
        //     }
        //     else{
        //         StartInference();
        //     }
        // }
    }

    public event System.Action<int> OnOutputCalculated;
    // 실시간 데이터 처리
    public int ProcessRealtimeData(Queue<float[]> queue)
    {
        // Debug.Log(queue.ToArray());

        // float[] inputData = GenerateInputData();
        float[][] arrayOfArrays = queue.ToArray();
        float[] a = arrayOfArrays.SelectMany(x => x).ToArray();

        int output = models.Predict(a, timeSteps, inputSize);
        // Debug.Log("결과 : "+ output);
        // Debug.Log("결과 : "+ Utils.myDictionary[output]);
        // Debug.Log(output);

        OnOutputCalculated?.Invoke(output);

        return output;
    }

    // private void StartInference()
    // {
    //     isInferenceRunning = true;
    // }

    // private void StopInference()
    // {
    //     isInferenceRunning = false;
    // }

}

