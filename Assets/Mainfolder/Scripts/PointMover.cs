using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointMover : MonoBehaviour
{
    public Transform pointToMove; // 이동할 점
    public Transform[] pathPoints; // 경로를 구성하는 포인트들
    public float[] durations; // 각 구간별 소요 시간
    public bool[] isArc; // 각 구간이 원호 이동인지 여부를 나타내는 배열

    private bool isMoving = false;
    private bool isClockwiseDirection = false; // 원 그릴 때, 시계 방향인지 반시계방향인지 확인
    private Vector3 currentDirection; // 현재 이동 방향

    public Vector3 CurrentDirection => currentDirection; // 외부에서 현재 방향을 가져오기 위한 속성
    public Vector3 PointToMovePosition => pointToMove.position; // 외부에서 pointToMove 위치를 가져오기 위한 속성
    public GameObject Direction;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && !isMoving)
        {
            if (pathPoints.Length != durations.Length || pathPoints.Length != isArc.Length)
            {
                //Debug.LogError("Path points, durations, and isArc arrays must have the same length");
                return;
            }
            Debug.Log("Starting movement");
            StartCoroutine(MoveAlongPath());
            Direction.GetComponent<DirectionUpdater>().PlayDirection();
        }
    }

    private void FixedUpdate()
    {
        //Debug.Log($"FixedUpdate - CurrentDirection: {currentDirection}, pointToMove position: {PointToMovePosition}");
    }

    IEnumerator MoveAlongPath()
    {
        isMoving = true;
        for (int i = 0; i < pathPoints.Length; i++)
        {
            if (isArc[i])
            {
                yield return StartCoroutine(MoveAlongArc(pathPoints[i], durations[i]));
            }
            else
            {
                yield return StartCoroutine(MoveToPoint(pathPoints[i], durations[i]));
            }
        }
        isMoving = false;
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
            pointToMove.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        pointToMove.position = endPosition;
        currentDirection = Vector3.zero; // 이동이 끝나면 방향을 초기화
    }

   

    IEnumerator MoveAlongArc(Transform target, float duration)
    {
        Vector3 startPosition = pointToMove.position;
        Vector3 endPosition = target.position;
        currentDirection = (endPosition - startPosition).normalized; // 이동 방향 업데이트
        float radius = 0.739f / 2.0f; // 반지름 설정

        // 중심 계산
        Vector3 center = (startPosition + endPosition) / 2.0f;

        // 방향 벡터 계산
        Vector3 direction = (endPosition - startPosition).normalized;
        // 90도 회전 적용 (오른쪽으로 90도 회전)
        direction = Quaternion.Euler(0, 90, 0) * direction;
        // 중심에서 수직 벡터 계산
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;

        // 두 점 사이의 거리를 절반으로 나눈 값을 기준으로 높이를 계산하여 중심을 조정합니다.
        float halfDistance = Vector3.Distance(startPosition, endPosition) / 2.0f;
        float height = Mathf.Sqrt(Mathf.Max(0, radius * radius - halfDistance * halfDistance)); // 높이를 계산할 때 음수 방지
        center += perpendicular * height;

        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float angle = Mathf.PI * t; // 0에서 π까지의 각도를 사용하여 반원을 그립니다.

            Vector3 offset;

            if (isClockwiseDirection)
            {
                offset = new Vector3(Mathf.Cos(angle) * radius, 0, -Mathf.Sin(angle) * radius); // 시계 방향
            }
            else
            {
                offset = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius); // 반시계 방향
            }

            pointToMove.position = center + Quaternion.LookRotation(direction) * offset;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        isClockwiseDirection = !isClockwiseDirection;
        pointToMove.position = endPosition;
        currentDirection = Vector3.zero; // 이동이 끝나면 방향을 초기화
    }

}

/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointMover : MonoBehaviour
{
    public Transform pointToMove; // 이동할 점
    public Transform[] pathPoints; // 경로를 구성하는 포인트들
    public float[] durations; // 각 구간별 소요 시간
    public bool[] isArc; // 각 구간이 원호 이동인지 여부를 나타내는 배열
    public bool[] isCurve; // 각 구간이 곡선 이동인지 여부를 나타내는 배열
    public Transform[] controlPoints; // 각 구간에 사용할 제어점(곡선의 경우 필요)

    private bool isMoving = false;
    private bool isClockwiseDirection = false; // 원 그릴 때, 시계 방향인지 반시계방향인지 확인
    private Vector3 currentDirection; // 현재 이동 방향

    public Vector3 CurrentDirection => currentDirection; // 외부에서 현재 방향을 가져오기 위한 속성
    public Vector3 PointToMovePosition => pointToMove.position; // 외부에서 pointToMove 위치를 가져오기 위한 속성
    public GameObject Direction;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && !isMoving)
        {
            if (pathPoints.Length != durations.Length || pathPoints.Length != isArc.Length || pathPoints.Length != isCurve.Length)
            {
                // Debug.LogError("Path points, durations, isArc, and isCurve arrays must have the same length");
                return;
            }
            Debug.Log("Starting movement");
            StartCoroutine(MoveAlongPath());
            Direction.GetComponent<DirectionUpdater>().PlayDirection();
        }
    }

    private void FixedUpdate()
    {
        // Debug.Log($"FixedUpdate - CurrentDirection: {currentDirection}, pointToMove position: {PointToMovePosition}");
    }

    IEnumerator MoveAlongPath()
    {
        isMoving = true;
        for (int i = 0; i < pathPoints.Length; i++)
        {
            if (isCurve[i])
            {
                yield return StartCoroutine(MoveAlongCurve(pathPoints[i], controlPoints[i], durations[i]));
            }
            else if (isArc[i])
            {
                yield return StartCoroutine(MoveAlongArc(pathPoints[i], durations[i]));
            }
            else
            {
                yield return StartCoroutine(MoveToPoint(pathPoints[i], durations[i]));
            }
        }
        isMoving = false;
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
            pointToMove.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        pointToMove.position = endPosition;
        currentDirection = Vector3.zero; // 이동이 끝나면 방향을 초기화
    }

    IEnumerator MoveAlongArc(Transform target, float duration)
    {
        Vector3 startPosition = pointToMove.position;
        Vector3 endPosition = target.position;
        currentDirection = (endPosition - startPosition).normalized; // 이동 방향 업데이트
        float radius = 0.739f / 2.0f; // 반지름 설정

        // 중심 계산
        Vector3 center = (startPosition + endPosition) / 2.0f;

        // 방향 벡터 계산
        Vector3 direction = (endPosition - startPosition).normalized;
        // 90도 회전 적용 (오른쪽으로 90도 회전)
        direction = Quaternion.Euler(0, 90, 0) * direction;
        // 중심에서 수직 벡터 계산
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;

        // 두 점 사이의 거리를 절반으로 나눈 값을 기준으로 높이를 계산하여 중심을 조정합니다.
        float halfDistance = Vector3.Distance(startPosition, endPosition) / 2.0f;
        float height = Mathf.Sqrt(Mathf.Max(0, radius * radius - halfDistance * halfDistance)); // 높이를 계산할 때 음수 방지
        center += perpendicular * height;

        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float angle = Mathf.PI * t; // 0에서 π까지의 각도를 사용하여 반원을 그립니다.

            Vector3 offset;

            if (isClockwiseDirection)
            {
                offset = new Vector3(Mathf.Cos(angle) * radius, 0, -Mathf.Sin(angle) * radius); // 시계 방향
            }
            else
            {
                offset = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius); // 반시계 방향
            }

            pointToMove.position = center + Quaternion.LookRotation(direction) * offset;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        isClockwiseDirection = !isClockwiseDirection;
        pointToMove.position = endPosition;
        currentDirection = Vector3.zero; // 이동이 끝나면 방향을 초기화
    }

    IEnumerator MoveAlongCurve(Transform target, Transform controlPoint, float duration)
    {
        Vector3 startPosition = pointToMove.position;
        Vector3 endPosition = target.position;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            // 3차 베지어 곡선을 사용하여 점의 위치를 계산합니다.
            pointToMove.position = Mathf.Pow(1 - t, 2) * startPosition +
                                   2 * (1 - t) * t * controlPoint.position +
                                   Mathf.Pow(t, 2) * endPosition;

            // 현재 방향 업데이트
            currentDirection = (pointToMove.position - startPosition).normalized;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        pointToMove.position = endPosition;
        currentDirection = Vector3.zero; // 이동이 끝나면 방향을 초기화
    }
}
*/