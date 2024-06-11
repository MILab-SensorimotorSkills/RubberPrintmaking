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

            float previousLowestY = GetPreviousLowestY(transform.position);
            
            // 오브젝트의 위치가 초기 위치보다 높아지면 가상 오브젝트의 위치를 초기 위치로 리셋
            if (transform.position.y >= initialPosition.y + 0.05f)
            {
                yPosition = initialPosition.y;
                minimumY = initialPosition.y; // minimumY도 초기 위치로 리셋
            }
            else
            {
                // 메인 로직
                if ((mainForce > 0.25f && mainForce <= 3f) || angle_Condition == 0f)
                {
                    yPosition = Mathf.Min(yPosition, previousLowestY);
                }
                else if (mainForce > 3f && angle_Condition != 0f)
                {
                    yPosition = Mathf.Lerp(previousLowestY, Mathf.Clamp(initialPosition.y - mainForce / 20.0f, maximunDepth, initialPosition.y), Mathf.Clamp(mainForce / 20.0f, 0, 1));
                    RecordPosition(transform.position); // 위치 기록
                }

                // 현재 위치와 최소 y값을 비교하여 yPosition 업데이트
                yPosition = Mathf.Min(yPosition, minimumY);
                minimumY = yPosition;
            }

            virtualObject.transform.position = new Vector3(
                initialPosition.x,
                yPosition,
                initialPosition.z
            );

            Debug.Log("Minimum Y: " + minimumY);
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
            recordedPositions.Add(new Vector3(virtualObjectPosition.x, Mathf.Max(virtualObjectPosition.y, maximunDepth), virtualObjectPosition.z));
        }
    }

    float GetPreviousLowestY(Vector3 currentPosition)
    {
        Vector3 virtualObjectPosition = virtualObject.transform.position; // 가상 오브젝트의 위치 사용
        float lowestY = initialPosition.y;

        foreach (Vector3 recordedPosition in recordedPositions)
        {
            if (Mathf.Abs(recordedPosition.x - virtualObjectPosition.x) <= 0.01f && Mathf.Abs(recordedPosition.z - virtualObjectPosition.z) <= 0.01f)
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
