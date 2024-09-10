// // 회전불가능코드
// // using System.Collections;
// // using System.Collections.Generic;
// // using UnityEngine;
// // using DiggingTest;

// // public class VirtualKnife : MonoBehaviour
// // {
// //     public GameObject virtualObject;
// //     public GameObject Cube;

// //     private AdvancedPhysicsHapticEffector advancedHapticEffector;
// //     private Rigidbody virtualObjectRb;

// //     public Vector3 initialPosition;
// //     private Shovel knifeShovel;

// //     private List<Vector3> recordedPositions = new List<Vector3>();
// //     private float angle_Condition;
// //     public float maximunDepth;
// //     private float minimumY;

// //     void Start()
// //     {
// //         advancedHapticEffector = GetComponent<AdvancedPhysicsHapticEffector>();
// //         knifeShovel = GetComponent<Shovel>();

// //         if (virtualObject != null)
// //         {
// //             virtualObjectRb = virtualObject.GetComponent<Rigidbody>();
// //             if (virtualObjectRb == null)
// //             {
// //                 virtualObjectRb = virtualObject.AddComponent<Rigidbody>();
// //             }
// //             initialPosition = virtualObject.transform.position;
// //             minimumY = initialPosition.y;
// //         }
// //     }

// //     void Update()
// //     {
// //         if (virtualObject != null)
// //         {
// //             float mainForce = advancedHapticEffector.MainForce;
// //             float mainForceY = advancedHapticEffector.MainForceY;
// //             float mainForceX = advancedHapticEffector.MainForceX;
// //             float mainForceZ = advancedHapticEffector.MainForceZ;
// //             float yPosition = initialPosition.y;
// //             CheckObjectColor();

// //             if (transform.position.y >= initialPosition.y + 0.05f)
// //             {
// //                 yPosition = initialPosition.y;
// //                 minimumY = initialPosition.y;
// //             }
// //             else
// //             {
// //                 float previousLowestY = GetPreviousLowestY(transform.position);
// //                 yPosition = Mathf.Min(previousLowestY, minimumY);

// //                 if ((mainForce > 7f && mainForce <= 8f) || angle_Condition == 0f)
// //                 {
// //                     yPosition = Mathf.Min(yPosition, previousLowestY);
// //                 }
// //                  else if (mainForce > 8f && angle_Condition != 0f )
// //                  //&& Mathf.Abs(mainForceX) < 0.1f || Mathf.Abs(mainForceZ) < 0.1f 
// //                 {
// //                     float depthAdjustment = 0f;

// //                     if (mainForceY >= 100f)
// //                     {
// //                         depthAdjustment = maximunDepth;
// //                     }
// //                     else if (mainForceY >= 50f)
// //                     {
// //                         depthAdjustment = maximunDepth * 0.4f;
// //                     }
// //                     else if (mainForceY >= 20f)
// //                     {
// //                         depthAdjustment = maximunDepth * 0.2f;
// //                     }
// //                     else if (mainForceY >= 10f)
// //                     {
// //                         depthAdjustment = maximunDepth * 0.1f;
// //                     }
// //                     else if (mainForceY >= 8f)
// //                     {
// //                         depthAdjustment = maximunDepth * 0.05f;
// //                     }

// //                     yPosition = Mathf.Lerp(previousLowestY, Mathf.Clamp(initialPosition.y - mainForce / 60.0f, maximunDepth, initialPosition.y), Mathf.Clamp(mainForce / 60.0f, 0, 1));
// //                     //yPosition = Mathf.Min(previousLowestY, initialPosition.y - depthAdjustment);

// //                     RecordPosition(transform.position);
// //                 }

// //                 minimumY = yPosition;
// //             }

// //             virtualObjectRb.MovePosition(new Vector3(
// //                 initialPosition.x,
// //                 yPosition,
// //                 initialPosition.z
// //             ));
// //         }
// //     }

// //     void RecordPosition(Vector3 position)
// //     {
// //         Vector3 virtualObjectPosition = virtualObject.transform.position;
// //         bool positionUpdated = false;

// //         for (int i = 0; i < recordedPositions.Count; i++)
// //         {
// //             Vector3 recordedPosition = recordedPositions[i];

// //             if (virtualObjectPosition.y < recordedPosition.y - 0.002f)
// //             {
// //                 recordedPositions[i] = new Vector3(recordedPosition.x, Mathf.Max(virtualObjectPosition.y, maximunDepth), recordedPosition.z);
// //                 positionUpdated = true;
// //             }
// //         }

// //         if (!positionUpdated)
// //         {
// //             recordedPositions.Add(new Vector3(position.x, Mathf.Max(virtualObjectPosition.y, maximunDepth), position.z));
// //         }
// //     }

// //     float GetPreviousLowestY(Vector3 currentPosition)
// //     {
// //         Vector3 virtualObjectPosition = virtualObject.transform.position;
// //         float lowestY = initialPosition.y;

// //         foreach (Vector3 recordedPosition in recordedPositions)
// //         {
// //             if (Mathf.Abs(recordedPosition.x - currentPosition.x) <= 0.01f && Mathf.Abs(recordedPosition.z - currentPosition.z) <= 0.01f)
// //             {
// //                 if (recordedPosition.y < lowestY)
// //                 {
// //                     lowestY = recordedPosition.y;
// //                 }
// //             }
// //         }
// //         return Mathf.Max(lowestY, maximunDepth);
// //     }

// //     void CheckObjectColor()
// //     {
// //         if (Cube != null)
// //         {
// //             Renderer objectRenderer = Cube.GetComponent<Renderer>();
// //             if (objectRenderer != null)
// //             {
// //                 Color objectColor = objectRenderer.material.color;
// //                 if (objectColor == Color.green)
// //                 {
// //                     angle_Condition = 3f;
// //                 }
// //                 else if (objectColor == new Color(1f, 0.5f, 0f))
// //                 {
// //                     angle_Condition = 2f;
// //                 }
// //                 else if (objectColor == Color.blue)
// //                 {
// //                     angle_Condition = 1f;
// //                 }
// //                 else if (objectColor == Color.red)
// //                 {
// //                     angle_Condition = 0f;
// //                 }
// //             }
// //         }
// //     }

// //     public List<Vector3> GetRecordedPositions()
// //     {
// //         return recordedPositions;
// //     }

// //     public float GetMinimumY()
// //     {
// //         return minimumY;
// //     }
// // }


// //회전가능코드
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using DiggingTest;

// public class VirtualKnife : MonoBehaviour
// {
//     public GameObject virtualObject;
//     public GameObject Cube;

//     private AdvancedPhysicsHapticEffector advancedHapticEffector;
//     private Rigidbody virtualObjectRb;

//     public Vector3 initialPosition;
//     private Shovel knifeShovel;

//     private List<Vector3> recordedPositions = new List<Vector3>();
//     private float angle_Condition;
//     public float maximunDepth;
//     private float minimumY;

//     void Start()
//     {
//         advancedHapticEffector = GetComponent<AdvancedPhysicsHapticEffector>();
//         knifeShovel = GetComponent<Shovel>();

//         if (virtualObject != null)
//         {
//             virtualObjectRb = virtualObject.GetComponent<Rigidbody>();
//             if (virtualObjectRb == null)
//             {
//                 virtualObjectRb = virtualObject.AddComponent<Rigidbody>();
//             }
//             initialPosition = virtualObject.transform.position;
//             minimumY = initialPosition.y;
//         }
//     }

//     void Update()
//     {
//         if (virtualObject != null)
//         {
//             float mainForce = advancedHapticEffector.MainForce;
//             float mainForceY = advancedHapticEffector.MainForceY;
//             float mainForceX = advancedHapticEffector.MainForceX;
//             float mainForceZ = advancedHapticEffector.MainForceZ;
//             float yPosition = initialPosition.y;
//             CheckObjectColor();

//             if (transform.position.y >= initialPosition.y + 0.05f)
//             {
//                 yPosition = initialPosition.y;
//                 minimumY = initialPosition.y;
//             }
//             else
//             {
//                 float previousLowestY = GetPreviousLowestY(transform.position);
//                 yPosition = Mathf.Min(previousLowestY, minimumY);

//                 if ((mainForce > 7f && mainForce <= 8f) || angle_Condition == 0f)
//                 {
//                     yPosition = Mathf.Min(yPosition, previousLowestY);
//                 }
//                  else if (mainForce > 8f && angle_Condition != 0f )
//                  //&& Mathf.Abs(mainForceX) < 0.1f || Mathf.Abs(mainForceZ) < 0.1f 
//                 {
//                     float depthAdjustment = 0f;

//                     if (mainForceY >= 100f)
//                     {
//                         depthAdjustment = maximunDepth;
//                     }
//                     else if (mainForceY >= 50f)
//                     {
//                         depthAdjustment = maximunDepth * 0.4f;
//                     }
//                     else if (mainForceY >= 20f)
//                     {
//                         depthAdjustment = maximunDepth * 0.2f;
//                     }
//                     else if (mainForceY >= 10f)
//                     {
//                         depthAdjustment = maximunDepth * 0.1f;
//                     }
//                     else if (mainForceY >= 8f)
//                     {
//                         depthAdjustment = maximunDepth * 0.05f;
//                     }

//                     yPosition = Mathf.Lerp(previousLowestY, Mathf.Clamp(initialPosition.y - mainForce / 60.0f, maximunDepth, initialPosition.y), Mathf.Clamp(mainForce / 60.0f, 0, 1));
//                     //yPosition = Mathf.Min(previousLowestY, initialPosition.y - depthAdjustment);

//                     RecordPosition(transform.position);
//                 }

//                 minimumY = yPosition;
//             }

//             // virtualObjectRb.MovePosition(new Vector3(
//             //     initialPosition.x,
//             //     yPosition,
//             //     initialPosition.z
//             // ));

//             // 위치 차이를 절대값으로 비교하여 업데이트
//             if (Mathf.Abs(virtualObject.transform.position.y - yPosition) > 0.0001f)
//             {
//                 Vector3 targetPosition = new Vector3(
//                     virtualObject.transform.position.x,  // x 위치는 그대로 유지
//                     yPosition,                           // y 위치만 업데이트
//                     virtualObject.transform.position.z  // z 위치는 그대로 유지
//                 );

//                 virtualObjectRb.MovePosition(targetPosition);
//             }
//         }
//     }

//     void RecordPosition(Vector3 position)
//     {
//         Vector3 virtualObjectPosition = virtualObject.transform.position;
//         bool positionUpdated = false;

//         for (int i = 0; i < recordedPositions.Count; i++)
//         {
//             Vector3 recordedPosition = recordedPositions[i];

//             if (virtualObjectPosition.y < recordedPosition.y - 0.002f)
//             {
//                 recordedPositions[i] = new Vector3(recordedPosition.x, Mathf.Max(virtualObjectPosition.y, maximunDepth), recordedPosition.z);
//                 positionUpdated = true;
//             }
//         }

//         if (!positionUpdated)
//         {
//             recordedPositions.Add(new Vector3(position.x, Mathf.Max(virtualObjectPosition.y, maximunDepth), position.z));
//         }
//     }

//     float GetPreviousLowestY(Vector3 currentPosition)
//     {
//         Vector3 virtualObjectPosition = virtualObject.transform.position;
//         float lowestY = initialPosition.y;

//         foreach (Vector3 recordedPosition in recordedPositions)
//         {
//             if (Mathf.Abs(recordedPosition.x - currentPosition.x) <= 0.01f && Mathf.Abs(recordedPosition.z - currentPosition.z) <= 0.01f)
//             {
//                 if (recordedPosition.y < lowestY)
//                 {
//                     lowestY = recordedPosition.y;
//                 }
//             }
//         }
//         return Mathf.Max(lowestY, maximunDepth);
//     }

//     void CheckObjectColor()
//     {
//         if (Cube != null)
//         {
//             Renderer objectRenderer = Cube.GetComponent<Renderer>();
//             if (objectRenderer != null)
//             {
//                 Color objectColor = objectRenderer.material.color;
//                 if (objectColor == Color.green)
//                 {
//                     angle_Condition = 3f;
//                 }
//                 else if (objectColor == new Color(1f, 0.5f, 0f))
//                 {
//                     angle_Condition = 2f;
//                 }
//                 else if (objectColor == Color.blue)
//                 {
//                     angle_Condition = 1f;
//                 }
//                 else if (objectColor == Color.red)
//                 {
//                     angle_Condition = 0f;
//                 }
//             }
//         }
//     }

//     public List<Vector3> GetRecordedPositions()
//     {
//         return recordedPositions;
//     }

//     public float GetMinimumY()
//     {
//         return minimumY;
//     }
// }


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 추가
using DiggingTest;
public class VirtualKnife : MonoBehaviour
{
    public GameObject virtualObject;
    public GameObject Cube;
    private AdvancedPhysicsHapticEffector advancedHapticEffector;
    private Rigidbody virtualObjectRb;
    public Vector3 initialPosition;
    private Shovel knifeShovel;
    private List<Vector3> recordedPositions = new List<Vector3>();
    private float angle_Condition;
    public float maximunDepth;
    private float minimumY;
    public AudioClip beepSound; // 비프음 클립
    private AudioSource audioSource; // 비프음을 재생할 오디오 소스
    public TextMeshProUGUI warningText; // 경고 메시지를 표시할 TextMeshProUGUI
    void Start()
    {
        advancedHapticEffector = GetComponent<AdvancedPhysicsHapticEffector>();
        knifeShovel = GetComponent<Shovel>();
        if (virtualObject != null)
        {
            virtualObjectRb = virtualObject.GetComponent<Rigidbody>();
            if (virtualObjectRb == null)
            {
                virtualObjectRb = virtualObject.AddComponent<Rigidbody>();
            }
            initialPosition = virtualObject.transform.position;
            minimumY = initialPosition.y;
        }
        // 오디오 소스 초기화
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = beepSound;
        warningText.enabled = false; // 초기에는 경고 메시지를 보이지 않게 설정
    }
    void Update()
    {
        if (virtualObject != null)
        {
            float mainForce = advancedHapticEffector.MainForce;
            float mainForceY = advancedHapticEffector.MainForceY;
            float mainForceX = advancedHapticEffector.MainForceX;
            float mainForceZ = advancedHapticEffector.MainForceZ;
            float yPosition = initialPosition.y;
            CheckObjectColor();
            if (transform.position.y >= initialPosition.y + 0.05f)
            {
                yPosition = initialPosition.y;
                minimumY = initialPosition.y;
            }
            else
            {
                float previousLowestY = GetPreviousLowestY(transform.position);
                yPosition = Mathf.Min(previousLowestY, minimumY);
                if ((mainForce > 7f && mainForce <= 8f) || angle_Condition == 0f)
                {
                    yPosition = Mathf.Min(yPosition, previousLowestY);
                }
                else if (mainForce > 8f && angle_Condition != 0f)
                {
                    float depthAdjustment = 0f;
                    if (mainForceY >= 100f)
                    {
                        depthAdjustment = maximunDepth;
                    }
                    else if (mainForceY >= 50f)
                    {
                        depthAdjustment = maximunDepth * 0.4f;
                    }
                    else if (mainForceY >= 20f)
                    {
                        depthAdjustment = maximunDepth * 0.2f;
                    }
                    else if (mainForceY >= 10f)
                    {
                        depthAdjustment = maximunDepth * 0.1f;
                    }
                    else if (mainForceY >= 8f)
                    {
                        depthAdjustment = maximunDepth * 0.05f;
                    }
                    yPosition = Mathf.Lerp(previousLowestY, Mathf.Clamp(initialPosition.y - mainForce / 60.0f, maximunDepth, initialPosition.y), Mathf.Clamp(mainForce / 60.0f, 0, 1));
                    RecordPosition(transform.position);
                }
                minimumY = yPosition;
            }
            if (Mathf.Abs(virtualObject.transform.position.y - yPosition) > 0.0001f)
            {
                Vector3 targetPosition = new Vector3(
                    virtualObject.transform.position.x,
                    yPosition,
                    virtualObject.transform.position.z
                );
                virtualObjectRb.MovePosition(targetPosition);
            }
            // MainforceY가 50f를 초과할 경우 경고음을 재생하고 메시지를 표시
            if (mainForceY > 40f)
            {
                TriggerWarning();
                // Debug.Log("힘 그만!!!");
            }
            else
            {
                if (audioSource.isPlaying) // 소리가 재생 중일 때만 멈춤
                {
                    audioSource.Stop(); // 소리 멈춤
                }
                warningText.enabled = false; // 힘이 정상 범위에 있으면 경고 메시지를 숨김
            }
        }
    }
    void RecordPosition(Vector3 position)
    {
        Vector3 virtualObjectPosition = virtualObject.transform.position;
        bool positionUpdated = false;
        for (int i = 0; i < recordedPositions.Count; i++)
        {
            Vector3 recordedPosition = recordedPositions[i];
            if (virtualObjectPosition.y < recordedPosition.y - 0.002f)
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
        Vector3 virtualObjectPosition = virtualObject.transform.position;
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
    void TriggerWarning()
    {
        if (!audioSource.isPlaying) // 비프음이 이미 재생 중이 아니면 재생
        {
            audioSource.Play();
        }
        warningText.text = "너무 강하게 힘을 주면 고무판이 뚫립니다";
        warningText.enabled = true; // 경고 메시지 표시
    }
    public List<Vector3> GetRecordedPositions()
    {
        return recordedPositions;
    }
    public float GetMinimumY()
    {
        return minimumY;
    }
}