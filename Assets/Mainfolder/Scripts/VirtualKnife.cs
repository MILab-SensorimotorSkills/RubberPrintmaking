using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DiggingTest;

public class VirtualKnife : MonoBehaviour
{
    public GameObject virtualObject;
    public GameObject Cube;

    private AdvancedPhysicsHapticEffector advancedHapticEffector;

    private Vector3 initialPosition;
    private Shovel knifeShovel;

    private List<Vector3> recordedPositions = new List<Vector3>();
    private float angle_Condition;
    public float maximunDepth;
    private float minimumY;
        private bool isXZLocked = false;
    private Vector3 lockedXZPosition;


    void Start()
    {
        advancedHapticEffector = GetComponent<AdvancedPhysicsHapticEffector>();
        knifeShovel = GetComponent<Shovel>();

        if (virtualObject != null)
        {
            initialPosition = virtualObject.transform.position;
            minimumY = initialPosition.y;
        }
    }

    void Update()
    {
        if (virtualObject != null)  // 고무판일 경우만 되도록 설정 추가.
        {
            float mainForce = advancedHapticEffector.MainForce;
            float yPosition = initialPosition.y;
            CheckObjectColor();

            
            // 오브젝트의 위치가 초기 위치보다 높아지면 가상 오브젝트의 위치를 초기 위치로 리셋
            if (transform.position.y >= initialPosition.y + 0.05f)
            {
                yPosition = initialPosition.y;
                minimumY = initialPosition.y; // minimumY도 초기 위치로 리셋
            }
            else
            {
                float previousLowestY = GetPreviousLowestY(transform.position); //사용
                yPosition = Mathf.Min(previousLowestY, minimumY);

                // 메인 로직
                if ((mainForce > 0.25f && mainForce <= 4f) || angle_Condition == 0f)
                {
                    yPosition = Mathf.Min(yPosition, previousLowestY);
                    if (transform.position.y <= initialPosition.y - 0.05f)
                    {
                        Debug.Log(123);
                        if (!isXZLocked)
                        {
                            // X, Z 좌표를 고정
                            lockedXZPosition = new Vector3(transform.position.x, 0, transform.position.z);
                            isXZLocked = true;
                        }

                        // X, Z 좌표에 대한 힘 피드백 적용
                        float xDifference = lockedXZPosition.x - transform.position.x;
                        float zDifference = lockedXZPosition.z - transform.position.z;

                        advancedHapticEffector.forceX = xDifference * 5; // 반향 힘을 줌
                        advancedHapticEffector.forceZ = zDifference * 5; // 반향 힘을 줌
                    }
                    else
                    {
                        // X, Z 좌표에 대한 힘 피드백 제거
                        advancedHapticEffector.forceX = 0;
                        advancedHapticEffector.forceZ = 0;
                    }
                }
                else if (mainForce > 4f && angle_Condition != 0f)
                {
                    yPosition = Mathf.Lerp(previousLowestY, Mathf.Clamp(initialPosition.y - mainForce / 40.0f, maximunDepth, initialPosition.y), Mathf.Clamp(mainForce / 40.0f, 0, 1));
                    RecordPosition(transform.position); //위치기록
                }

                // 현재 위치와 최소 y값을 비교하여 yPosition 업데이트
                minimumY = yPosition;
            }

            virtualObject.transform.position = new Vector3(
                initialPosition.x,
                yPosition,
                initialPosition.z
            );

        }
    }

    void RecordPosition(Vector3 position)
    {
        Vector3 virtualObjectPosition = virtualObject.transform.position; // 가상 오브젝트의 위치 사용
        bool positionUpdated = false;

        for (int i = 0; i < recordedPositions.Count; i++)
        {
            Vector3 recordedPosition = recordedPositions[i];

            if (virtualObjectPosition.y < recordedPosition.y - 0.002f) // 최소 변화 기준을 0.002로 설정
            {
                recordedPositions[i] = new Vector3(recordedPosition.x, Mathf.Max(virtualObjectPosition.y, maximunDepth), recordedPosition.z);
                positionUpdated = true;
            }
        }

        if (!positionUpdated)
        {
            recordedPositions.Add(new Vector3(position.x, Mathf.Max(virtualObjectPosition.y, maximunDepth), position.z));
        }
    }

    float GetPreviousLowestY(Vector3 currentPosition)
    {
        Vector3 virtualObjectPosition = virtualObject.transform.position; // 가상 오브젝트의 위치 사용
        float lowestY = initialPosition.y;

        foreach (Vector3 recordedPosition in recordedPositions)
        {
            if (Mathf.Abs(recordedPosition.x - currentPosition.x) <= 0.01f && Mathf.Abs(recordedPosition.z - currentPosition.z) <= 0.01f)
            {
                if (recordedPosition.y < lowestY)
                {
                    lowestY = recordedPosition.y;
                }
            }
        }
        return Mathf.Max(lowestY, maximunDepth);
    }


    void CheckObjectColor()
    {
        if (Cube != null)
        {
            Renderer objectRenderer = Cube.GetComponent<Renderer>();
            if (objectRenderer != null)
            {
                Color objectColor = objectRenderer.material.color;
                if (objectColor == Color.green)
                {
                    angle_Condition = 3f;
                }
                else if (objectColor == new Color(1f, 0.5f, 0f))
                {
                    angle_Condition = 2f;
                }
                else if (objectColor == Color.blue)
                {
                    angle_Condition = 1f;
                }
                else if (objectColor == Color.red)
                {
                    angle_Condition = 0f;
                }
            }
        }
    }
}
