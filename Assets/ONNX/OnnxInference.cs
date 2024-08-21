using UnityEngine;
// using Unity.Barracuda;
using System.Linq;
using System.Collections.Generic;
using Unity.XR.Oculus;

public class OnnxInference : MonoBehaviour
{
    [SerializeField] private string modelPath;
    public string modelAsset;
    // private Model runtimeModel; //barracuda 모델 불러오기
    // private IWorker worker;

    private int timeSteps = 100; // 모델이 기대하는 시퀀스 길이
    private int inputSize = 3;  // 입력 특징 수
    // private int batchSize = 1;
    private Models models;
    // private static float[] GenerateInputData()
    //     {
    //         return new float[]
    //         {
    //             1.0f, 2.0f, 3.0f,
    //             7.0f, 8.0f, 9.0f,
    //             13.0f, 14.0f, 15.0f

    //         };
    //     }

    void Start()
    {
        models = new Models(modelPath);
        // models = new Models("C:\\Users\\jy\\Desktop\\RubberPrintmaking\\Assets\\ONNX\\lstm_test4.onnx");
        // ONNX 모델 로드 및  초기화
        // runtimeModel = ModelLoader.Load(modelAsset);
        // // worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Compute, runtimeModel); //GPU를 사용
        // worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, runtimeModel);
    }

    // 실시간 데이터 처리
    public int ProcessRealtimeData(Queue<float[]> queue)
    {
        // Debug.Log(queue.ToArray());

        // float[] inputData = GenerateInputData();
        float[][] arrayOfArrays = queue.ToArray();
        float[] a = arrayOfArrays.SelectMany(x => x).ToArray();

        var output = models.Predict(a, timeSteps, inputSize);
        // Debug.Log("결과 : "+ output);
        Debug.Log("결과 : "+ Utils.myDictionary[output]);



        // 큐의 데이터가 timeSteps와 맞는지 확인
        // if (forceQueue.Count != timeSteps)
        // {
        //     Debug.LogError("Input data does not match the required time steps.");
        //     return -1;
        // }

        // // 입력 데이터 배열 초기화
        // float[] inputData = new float[batchSize * timeSteps * inputSize];
        // int index = 0;

        // // foreach (var forceData in forceQueue)
        // // {
        // //     for (int i = 0; i < batchSize; i++)
        // //     {
        // //         inputData[i * timeSteps * inputSize + index * inputSize + 0] = forceData["MainForceX"];
        // //         inputData[i * timeSteps * inputSize + index * inputSize + 1] = forceData["MainForceY"];
        // //         inputData[i * timeSteps * inputSize + index * inputSize + 2] = forceData["MainForceZ"];
        // //     }
        // //     index++;
        // // }

        // foreach (var forceData in forceQueue)
        // {
        //     inputData[index * inputSize + 0] = forceData["MainForceX"];
        //     inputData[index * inputSize + 1] = forceData["MainForceY"];
        //     inputData[index * inputSize + 2] = forceData["MainForceZ"];
        //     index++;
        // }

        // // TensorShape를 사용하여 텐서 생성
        // Tensor inputTensor = new Tensor(new TensorShape(batchSize, timeSteps, inputSize), inputData);

        // // 모델 실행
        // // worker.Execute(inputTensor);
        // // Tensor outputTensor = worker.PeekOutput();

        // // // 모델 추론 결과
        // // float[] output = outputTensor.ToReadOnlyArray();
        // // int predictedClass = System.Array.IndexOf(output, output.Max());

        // // // 텐서 해제
        // // inputTensor.Dispose();
        // // outputTensor.Dispose();

        return 1;
    }

    // void OnDestroy()
    // {
    //     worker.Dispose();
    // }
}

