using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Unity.Barracuda;
using UnityEngine;


public class Models : IDisposable
    {
    private readonly InferenceSession _session;
    private NNModel modelAsset;

    // 생성자: 모델 초기화
    public Models(string modelPath)
    {
        _session = new InferenceSession(modelPath);
    }

    public Models(NNModel modelAsset)
    {
        this.modelAsset = modelAsset;
    }

    // 예측 메서드
    public int Predict(float[] inputData, int sequenceLength, int inputSize)
    {
        // ONNX 텐서 생성 (1, sequenceLength, inputSize)의 크기로 초기화
        var inputTensor = new DenseTensor<float>(inputData, new[] { 1, sequenceLength, inputSize });

        // 입력 데이터 준비
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input", inputTensor)
        };

        // 예측 수행 및 결과 반환
        using var results = _session.Run(inputs);
        var output = results.First().AsEnumerable<float>().ToArray();

        // 최댓값의 인덱스 찾기 (LINQ 사용)
        int maxIndex = output.Select((value, index) => new { Value = value, Index = index })
                            .OrderByDescending(x => x.Value)
                            .First().Index;

        return maxIndex;
    }

    // 자원 해제 메서드
    public void Dispose()
    {
        _session.Dispose();
    }

    public InferenceSession getSession(){
        return this._session;
    }
}
