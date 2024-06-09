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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && !isMoving)
        {
            if (pathPoints.Length != durations.Length || pathPoints.Length != isArc.Length)
            {
                Debug.LogError("Path points, durations, and isArc arrays must have the same length");
                return;
            }

            StartCoroutine(MoveAlongPath());
        }
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
    }

    IEnumerator MoveToPoint(Transform target, float duration)
    {
        Vector3 startPosition = pointToMove.position;
        Vector3 endPosition = target.position;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            pointToMove.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        pointToMove.position = endPosition;
    }

    IEnumerator MoveAlongArc(Transform target, float duration)
    {
        Vector3 startPosition = pointToMove.position;
        Vector3 endPosition = target.position;
        float radius = 0.739f / 2.0f; // 반지름 설정
        Vector3 center = (startPosition + endPosition) / 2.0f;

        // 중심에서 수직 벡터 계산
        Vector3 direction = (endPosition - startPosition).normalized;
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized;
        
        // 중심점 조정
        float halfDistance = Vector3.Distance(startPosition, endPosition) / 2.0f;
        float height = Mathf.Sqrt(radius * radius - halfDistance * halfDistance);
        center += perpendicular * height;

        float startAngle = Mathf.Atan2(startPosition.y - center.y, startPosition.x - center.x);
        float endAngle = Mathf.Atan2(endPosition.y - center.y, endPosition.x - center.x);

        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            float angle = Mathf.Lerp(startAngle, endAngle, elapsedTime / duration);
            pointToMove.position = new Vector3(center.x + Mathf.Cos(angle) * radius, center.y + Mathf.Sin(angle) * radius, pointToMove.position.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        pointToMove.position = endPosition;
    }
}
