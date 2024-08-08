using UnityEngine;
using Unity.Barracuda;
using System.Linq;

public class OnnxInference : MonoBehaviour
{
    public NNModel modelAsset; 
    private Model runtimeModel;
    private IWorker worker;

    private int timeSteps = 10;
    private int inputSize = 3;
    private int batchSize = 1; 

    void Start()
    {
        // ONNX 모델 로드 및 초기화
        runtimeModel = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Compute, runtimeModel); // GPU를 사용하도록 변경
    }

    // 실시간 데이터 처리
    public int ProcessRealtimeData(float[] xData, float[] yData, float[] zData)
    {
        // 데이터가 타임스텝 수에 맞는지 확인
        if (xData.Length != timeSteps || yData.Length != timeSteps || zData.Length != timeSteps)
        {
            Debug.LogError("Input data does not match the required time steps.");
            return -1;
        }

        // 입력 데이터 디버깅 출력
        Debug.Log("Input Data:");
        for (int i = 0; i < timeSteps; i++)
        {
            Debug.Log($"t={i}, x={xData[i]}, y={yData[i]}, z={zData[i]}");
        }

        // 입력 텐서 생성
        float[] inputData = new float[batchSize * timeSteps * inputSize];
        for (int i = 0; i < timeSteps; i++)
        {
            inputData[i * inputSize] = xData[i];
            inputData[i * inputSize + 1] = yData[i];
            inputData[i * inputSize + 2] = zData[i];
        }

        // TensorShape를 사용하여 텐서 생성
        Tensor inputTensor = new Tensor(new TensorShape(batchSize, 1, timeSteps, inputSize), inputData);

        // 텐서 정보 디버깅 출력
        Debug.Log("Input Tensor Shape: " + string.Join(", ", inputTensor.shape));
        Debug.Log("Input Tensor Data: " + string.Join(", ", inputData));

        worker.Execute(inputTensor);
        Tensor outputTensor = worker.PeekOutput();

        // 모델 추론 결과 디버깅 출력
        float[] output = outputTensor.ToReadOnlyArray();
        Debug.Log("Output Tensor Data: " + string.Join(", ", output));

        int predictedClass = System.Array.IndexOf(output, output.Max());

        // 결과 출력
        Debug.Log("Predicted Class: " + predictedClass);

        // 텐서 해제
        inputTensor.Dispose();
        outputTensor.Dispose();

        return predictedClass;
    }

    void OnDestroy()
    {
        worker.Dispose();
    }
}
