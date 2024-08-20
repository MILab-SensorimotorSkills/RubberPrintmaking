using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateRubber : MonoBehaviour
{
    public float rotationSpeed = 100f;

    void Update()
    {
        // 좌우 방향키 입력을 받습니다.
        float horizontalInput = Input.GetAxis("Horizontal");

        // 오브젝트를 Y축으로 회전시킵니다.
        transform.Rotate(0, horizontalInput * rotationSpeed * Time.deltaTime, 0, Space.World);
    }
}
