using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DiggingTest;

public class VirtualKnife : MonoBehaviour
{
    public GameObject virtualObject;
    public GameObject Cube;

    private AdvancedPhysicsHapticEffector advancedHapticEffector;
    //private Disturbance advancedHapticEffector;

    private Vector3 initialPosition;
    private Shovel knifeShovel;

    private List<Vector3> recordedPositions = new List<Vector3>();
    private float angle_Condition;
        private float zPositionLock;
    private bool isZLocked = false;
    public PhysicMaterial defaultMaterial;
    public PhysicMaterial changedMaterial;

    void Start()
    {
        advancedHapticEffector = GetComponent<AdvancedPhysicsHapticEffector>();           
        //advancedHapticEffector = GetComponent<Disturbance>();

        knifeShovel = GetComponent<Shovel>();

        if (virtualObject != null)
        {
            initialPosition = virtualObject.transform.position;
        }
    }

    void Update()
    {
        if (virtualObject != null)  //고무판일 경우만 되도록 설정 추가.
        {   
            if (virtualObject.transform.position.y < -0.155f)
            {
                // LockZPosition(transform.position.z);
                SetPhysicalProperties(true);
            }
            else
            {
                SetPhysicalProperties(false);
                // UnlockZPosition();

            }

            CheckObjectColor();

            float mainForce = advancedHapticEffector.MainForce;

            float yPosition = initialPosition.y; 

            if (mainForce > 3f && advancedHapticEffector.collidingTag == "Ground")
            {
                float previousLowestY = GetPreviousLowestY(transform.position);
                //이전 최저 위치에서부터 다시 누르기 시작
                yPosition = Mathf.Lerp(previousLowestY, Mathf.Clamp(initialPosition.y - mainForce / 20.0f, -0.217f, initialPosition.y), Mathf.Clamp(mainForce / 20.0f, 0, 1));

                //angle_Condition이 0이면 더 이상 눌러지지 않게
                if (angle_Condition == 0f)
                {
                    yPosition = Mathf.Max(yPosition, previousLowestY);
                }

                RecordPosition(transform.position); //위치 기록

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
        bool positionUpdated = false;

        for (int i = 0; i < recordedPositions.Count; i++)
        {
            Vector3 recordedPosition = recordedPositions[i];

            if (position.y < recordedPosition.y - 0.002f) // 최소 변화 기준을 0.002로 설정
            {
                recordedPositions[i] = new Vector3(recordedPosition.x, position.y, recordedPosition.z);
                //Debug.Log($"Position updated: {recordedPositions[i]}");
                positionUpdated = true;
            }
        }

        if (!positionUpdated)
        {
            recordedPositions.Add(position);
            //Debug.Log($"Position recorded: {position}");
        }
    }

    float GetPreviousLowestY(Vector3 currentPosition)
    {
        float lowestY = initialPosition.y;

        foreach (Vector3 recordedPosition in recordedPositions)
        {
            if (Mathf.Abs(recordedPosition.x - currentPosition.x) <= 0.02 && Mathf.Abs(recordedPosition.z - currentPosition.z) <= 0.02f)
            {
                if (recordedPosition.y < lowestY)
                {
                    lowestY = recordedPosition.y;
                }
            }
        }

        return lowestY;
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

                }else if (objectColor == Color.blue) 
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
    void SetPhysicalProperties(bool isBelowThreshold)
    {
        Collider collider = GetComponent<Collider>();

        if (isBelowThreshold)
        {
            // 설정된 다른 피직스 머터리얼 적용
            if (collider != null)
            {
                collider.material = changedMaterial;
            }
        }
        else
        {
            // 기존 피직스 머터리얼로 되돌리기
            if (collider != null)
            {
                collider.material = defaultMaterial;
            }
        }
    }
    void LockZPosition(float zPosition)
    {
        if (!isZLocked)
        {
            zPositionLock = zPosition;
            isZLocked = true;
        }

        // z축 위치를 고정하기 위한 힘 피드백 적용
        float zDifference = zPositionLock - transform.position.z;
        advancedHapticEffector.forceZ = zDifference * 1;
    }

    void UnlockZPosition()
    {
        if (isZLocked && virtualObject.transform.position.y >= initialPosition.y)
        {
            isZLocked = false;
            advancedHapticEffector.forceZ = 0f;
        }
    }
       
}
