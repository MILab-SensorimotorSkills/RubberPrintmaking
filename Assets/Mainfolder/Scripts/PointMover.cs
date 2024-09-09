using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointMover : MonoBehaviour
{
    public Transform pointToMove; // 이동할 점
    public Transform[] pathPoints; // 경로를 구성하는 포인트들
    public float[] durations; // 각 구간별 소요 시간
    public bool[] isRotation; // 각 구간에서 회전 여부
    public Vector3[] rotations; // 각 구간에서의 회전 각도

    private bool isPlaying = false;
    private bool isPaused = false; // 일시정지 상태인지 여부
    private Coroutine currentCoroutine; // 현재 진행 중인 코루틴을 저장
    private Vector3 currentDirection; // 현재 이동 방향

    public Vector3 CurrentDirection => currentDirection; // 외부에서 현재 방향을 가져오기 위한 속성
    public Vector3 PointToMovePosition => pointToMove.position; // 외부에서 pointToMove 위치를 가져오기 위한 속성
    public GameObject Direction;
    public OnnxInference onnxInference;
    private Vector3 lastPosition;

    private bool directionStarted = false; // DirectionUpdater가 시작되었는지 여부 확인

    void Start()
    {
        lastPosition = pointToMove.position;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            HandleMovementToggle();
        }
    }

    private void HandleMovementToggle()
    {
        if (!isPlaying)
        {
            if (pathPoints.Length != durations.Length || pathPoints.Length != isRotation.Length || pathPoints.Length != rotations.Length)
            {
                Debug.LogError("Path points, durations, isRotation, and rotations arrays must have the same length");
                return;
            }
            Debug.Log("Starting movement");
            isPaused = false; // 재생 상태로 설정
            isPlaying = true;
            currentCoroutine = StartCoroutine(MoveAlongPath());

            if (!directionStarted)
            {
                // 화살표가 아직 시작되지 않았다면 동시에 시작
                Debug.Log("Starting DirectionUpdater");
                Direction.GetComponent<DirectionUpdater>().PlayDirection();
                directionStarted = true; // 시작되었음을 표시
            }
        }
        else if (isPaused)
        {
            // 일시정지 상태에서 P를 누르면 재개
            Debug.Log("Resuming movement");
            isPaused = false;
            Direction.GetComponent<DirectionUpdater>().PlayDirection(); // 화살표도 재개
        }
        else
        {
            // 진행 중일 때 P를 누르면 일시정지
            Debug.Log("Pausing movement");
            isPaused = true;
            Direction.GetComponent<DirectionUpdater>().PlayDirection(); // 화살표도 일시정지
        }
    }

    IEnumerator MoveAlongPath()
    {
        for (int i = 0; i < pathPoints.Length; i++)
        {
            // 이동
            while (isPaused) // 일시정지 상태면 기다림
            {
                yield return null;
            }

            yield return StartCoroutine(MoveToPoint(pathPoints[i], durations[i]));

            // 이동이 끝난 후, 회전이 필요한지 확인
            if (isRotation[i])
            {
                yield return StartCoroutine(RotateTo(rotations[i]));
            }
        }
        isPlaying = false;
        currentDirection = Vector3.zero; // 이동이 끝나면 방향을 초기화
    }

    IEnumerator MoveToPoint(Transform target, float duration)
    {
        Vector3 startPosition = pointToMove.position;
        Vector3 endPosition = target.position;
        currentDirection = (endPosition - startPosition).normalized; // 이동 방향 업데이트
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            while (isPaused) // 일시정지 상태면 기다림
            {
                yield return null;
                startPosition = pointToMove.position; // 재개 시 시작 위치를 현재 위치로 업데이트
                endPosition = target.position;
                currentDirection = (endPosition - startPosition).normalized;
            }

            float t = Mathf.Clamp01(elapsedTime / duration);
            pointToMove.position = Vector3.Lerp(startPosition, endPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        pointToMove.position = endPosition;
        currentDirection = Vector3.zero; // 이동이 끝나면 방향을 초기화
    }

    IEnumerator RotateTo(Vector3 rotationAngles)
    {
        // 회전 시작 전에 DirectionUpdater를 일시정지
        Direction.GetComponent<DirectionUpdater>().PlayDirection(); // 일시정지

        float rotationDuration = 1.0f; // 회전 시간 (임의로 설정)
        float elapsedTime = 0;
        Vector3 currentRotation = pointToMove.parent.parent.eulerAngles; // 현재 월드 좌표 회전값

        // 각 축의 회전 차이를 계산 (목표 각도 - 현재 각도)
        Vector3 rotationDifference = rotationAngles - currentRotation;

        while (elapsedTime < rotationDuration)
        {
            while (isPaused) // 일시정지 상태면 기다림
            {
                yield return null;
            }

            float t = Mathf.Clamp01(elapsedTime / rotationDuration);

            // 월드 좌표 기준으로 일정량씩 회전 (각 프레임마다 회전 차이의 일부만 적용)
            Vector3 stepRotation = rotationDifference * (Time.deltaTime / rotationDuration);
            pointToMove.parent.parent.Rotate(stepRotation, Space.World);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 최종적으로 정확한 목표 회전 각도 설정
        pointToMove.parent.parent.rotation = Quaternion.Euler(rotationAngles);

        // 회전이 끝난 후 DirectionUpdater를 재개
        Direction.GetComponent<DirectionUpdater>().PlayDirection(); // 재개
    }


}





// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class PointMover : MonoBehaviour
// {
//     public Transform pointToMove; // 이동할 점
//     public Transform[] pathPoints; // 경로를 구성하는 포인트들
//     public float[] durations; // 각 구간별 소요 시간
//     public bool[] isArc; // 각 구간이 원호 이동인지 여부를 나타내는 배열
//     public Quaternion[] parentRotations; // 각 지점에서의 부모 오브젝트 회전 각도 저장 (pointToMove.parent.parent에 대한 회전)

//     private bool isPlaying = false;
//     private bool isPaused = false; // 일시정지 상태인지 여부
//     private Coroutine currentCoroutine; // 현재 진행 중인 코루틴을 저장
//     private bool isClockwiseDirection = false; // 원 그릴 때, 시계 방향인지 반시계방향인지 확인
//     private Vector3 currentDirection; // 현재 이동 방향

//     public Vector3 CurrentDirection => currentDirection; // 외부에서 현재 방향을 가져오기 위한 속성
//     public Vector3 PointToMovePosition => pointToMove.position; // 외부에서 pointToMove 위치를 가져오기 위한 속성
//     public GameObject Direction;
//     public OnnxInference onnxInference;
//     private Vector3 lastPosition;

//     private bool directionStarted = false; // DirectionUpdater가 시작되었는지 여부 확인

//     void Start()
//     {
//         lastPosition = pointToMove.position;
//     }

//     void Update()
//     {
//         if (Input.GetKeyDown(KeyCode.P))
//         {
//             HandleMovementToggle();
//         }
//     }

//     private void HandleMovementToggle()
//     {
//         if (!isPlaying)
//         {
//             if (pathPoints.Length != durations.Length || pathPoints.Length != isArc.Length)
//             {
//                 Debug.LogError("Path points, durations, isArc arrays must have the same length");
//                 return;
//             }
//             Debug.Log("Starting movement");
//             isPaused = false; // 재생 상태로 설정
//             isPlaying = true;
//             currentCoroutine = StartCoroutine(MoveAlongPath());

//             if (!directionStarted)
//             {
//                 // 화살표가 아직 시작되지 않았다면 동시에 시작
//                 Debug.Log("Starting DirectionUpdater");
//                 Direction.GetComponent<DirectionUpdater>().PlayDirection();
//                 directionStarted = true; // 시작되었음을 표시
//             }
//         }
//         else if (isPaused)
//         {
//             // 일시정지 상태에서 P를 누르면 재개
//             Debug.Log("Resuming movement");
//             isPaused = false;
//             Direction.GetComponent<DirectionUpdater>().PlayDirection(); // 화살표도 재개
//         }
//         else
//         {
//             // 진행 중일 때 P를 누르면 일시정지
//             Debug.Log("Pausing movement");
//             isPaused = true;
//             Direction.GetComponent<DirectionUpdater>().PlayDirection(); // 화살표도 일시정지
//         }
//     }

//     IEnumerator MoveAlongPath()
//     {
//         for (int i = 0; i < pathPoints.Length; i++)
//         {
//             while (isPaused) // 일시정지 상태면 기다림
//             {
//                 yield return null;
//             }

//             // 포인트 이동
//             if (isArc[i])
//             {
//                 yield return StartCoroutine(MoveAlongArc(pathPoints[i], durations[i]));
//             }
//             else
//             {
//                 yield return StartCoroutine(MoveToPoint(pathPoints[i], durations[i]));
//             }

//             // 포인트 이동이 완료된 후 회전 적용
//             if (parentRotations.Length > i && pointToMove.parent.parent != null)
//             {
//                 yield return StartCoroutine(RotateParent(parentRotations[i]));
//                 Debug.Log("Advanced (1) rotation 완료");
//             }
//         }
//         isPlaying = false;
//         currentDirection = Vector3.zero; // 이동이 끝나면 방향을 초기화
//     }

//     IEnumerator MoveToPoint(Transform target, float duration)
//     {
//         Vector3 startPosition = pointToMove.position; 
//         Vector3 endPosition = target.position;
//         currentDirection = (endPosition - startPosition).normalized; // 이동 방향 업데이트
//         float elapsedTime = 0;

//         while (elapsedTime < duration)
//         {
//             while (isPaused) // 일시정지 상태면 기다림
//             {
//                 yield return null;
//                 startPosition = pointToMove.position; // 재개 시 시작 위치를 현재 위치로 업데이트
//                 endPosition = target.position;
//                 currentDirection = (endPosition - startPosition).normalized;
//             }

//             float t = Mathf.Clamp01(elapsedTime / duration);
//             pointToMove.position = Vector3.Lerp(startPosition, endPosition, t);
//             elapsedTime += Time.deltaTime;
//             yield return null;
//         }

//         pointToMove.position = endPosition;
//         currentDirection = Vector3.zero; // 이동이 끝나면 방향을 초기화
//     }

//     IEnumerator MoveAlongArc(Transform target, float duration)
//     {
//         Vector3 startPosition = pointToMove.position;
//         Vector3 endPosition = target.position;
//         currentDirection = (endPosition - startPosition).normalized; // 이동 방향 업데이트
//         float radius = 0.739f / 2.0f; // 반지름 설정

//         // 중심 계산
//         Vector3 center = (startPosition + endPosition) / 2.0f;

//         // 방향 벡터 계산
//         Vector3 direction = (endPosition - startPosition).normalized;
//         // 90도 회전 적용 (오른쪽으로 90도 회전)
//         direction = Quaternion.Euler(0, 90, 0) * direction;
//         // 중심에서 수직 벡터 계산
//         Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;

//         // 두 점 사이의 거리를 절반으로 나눈 값을 기준으로 높이를 계산하여 중심을 조정합니다.
//         float halfDistance = Vector3.Distance(startPosition, endPosition) / 2.0f;
//         float height = Mathf.Sqrt(Mathf.Max(0, radius * radius - halfDistance * halfDistance)); // 높이를 계산할 때 음수 방지
//         center += perpendicular * height;

//         float elapsedTime = 0;
//         while (elapsedTime < duration)
//         {
//             while (isPaused) // 일시정지 상태면 기다림
//             {
//                 yield return null;
//             }

//             float t = elapsedTime / duration;
//             float angle = Mathf.PI * t; // 0에서 π까지의 각도를 사용하여 반원을 그립니다.

//             Vector3 offset = isClockwiseDirection
//                 ? new Vector3(Mathf.Cos(angle) * radius, 0, -Mathf.Sin(angle) * radius) // 시계 방향
//                 : new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius); // 반시계 방향

//             pointToMove.position = center + Quaternion.LookRotation(direction) * offset;
//             elapsedTime += Time.deltaTime;
//             yield return null;
//         }
//         isClockwiseDirection = !isClockwiseDirection;
//         pointToMove.position = endPosition;
//         currentDirection = Vector3.zero; // 이동이 끝나면 방향을 초기화
//     }

//     // 부모 오브젝트 회전을 담당하는 코루틴
//     IEnumerator RotateParent(Quaternion targetRotation)
//     {
//         Quaternion initialRotation = pointToMove.parent.parent.rotation;
//         float rotationTime = 100f; // 회전하는 데 걸리는 시간
//         float elapsedTime = 0;

//         while (elapsedTime < rotationTime)
//         {
//             while (isPaused) // 일시정지 상태면 기다림
//             {
//                 yield return null;
//             }

//             pointToMove.parent.parent.rotation = Quaternion.Slerp(initialRotation, targetRotation, elapsedTime / rotationTime);
//             elapsedTime += Time.deltaTime;
//             yield return null;
//         }

//         pointToMove.parent.parent.rotation = targetRotation; // 회전 완료 후 목표 각도에 맞춤
//     }
// }
