using System.Collections;
using UnityEngine;

public class RotateRubber : MonoBehaviour
{
    public float[] rotationSpeeds;  // 회전 속도 배열
    public float[] rotateAngles;    // 회전할 각도 배열
    public float[] waitTimes;       // 회전할 시간 간격 배열

    private int currentIndex = 0;   // 현재 회전 인덱스
    private bool isRotating = false; // 현재 회전 중인지 확인하는 플래그
    private bool isManualStart = false; // P키를 눌러 수동으로 회전이 시작되었는지 여부
    private Coroutine rotateCoroutine; // 코루틴을 제어하기 위한 참조
    public OnnxInference onnxInference; // OnnxInference 참조

    void Start()
    {
        // OnnxInference에서 출력된 값을 구독하여 처리
        onnxInference.OnOutputCalculated += HandleOnnxOutput;
    }

    void Update()
    {
        // P 키를 눌렀을 때 실행/일시정지 상태 전환
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isRotating)
            {
                StopRotation(); // 회전 일시정지
                isManualStart = false; // 수동 시작을 초기화
            }
            else
            {
                isManualStart = true; // 수동 시작 플래그 활성화
                StartRotation(); // 회전 시작
            }
        }
    }

    void HandleOnnxOutput(int output)
    {
        // 수동으로 시작되었을 때만 output 값에 따라 제어
        if (isManualStart)
        {
            // output이 0이 아닐 경우 회전 일시정지
            if (output != 0)
            {
                StopRotation();
            }
            else if (!isRotating)
            {
                StartRotation(); // output이 0일 때 회전 재개
            }
        }
    }

    void StartRotation()
    {
        isRotating = true;
        // 회전 코루틴을 시작
        rotateCoroutine = StartCoroutine(RotateAtIntervals());
    }

    void StopRotation()
    {
        isRotating = false;
        // 현재 진행 중인 회전 코루틴을 멈춤
        if (rotateCoroutine != null)
        {
            StopCoroutine(rotateCoroutine);
        }
    }

    IEnumerator RotateAtIntervals()
    {
        while (isRotating)
        {
            if (currentIndex >= rotateAngles.Length)
            {
                currentIndex = 0; // 배열의 끝에 도달하면 처음으로 돌아감
            }

            // 회전 대기 시간
            float waitTime = waitTimes[currentIndex];
            yield return new WaitForSeconds(waitTime);

            // 현재 인덱스에 해당하는 회전 각도와 속도로 회전
            float rotationSpeed = rotationSpeeds[currentIndex];
            float rotateAngle = rotateAngles[currentIndex];

            // 회전 실행
            yield return StartCoroutine(RotateByAngle(rotationSpeed, rotateAngle));

            // 다음 회전을 위해 인덱스를 증가
            currentIndex++;
        }
    }

    IEnumerator RotateByAngle(float rotationSpeed, float angle)
    {
        float rotatedAmount = 0f; // 회전한 각도

        while (rotatedAmount < angle)
        {
            float step = rotationSpeed * Time.deltaTime;
            transform.Rotate(0, step, 0, Space.World);
            rotatedAmount += step;
            yield return null;
        }

        // 회전이 완료되면 남은 각도 조정
        transform.Rotate(0, angle - rotatedAmount, 0, Space.World);
    }
}
