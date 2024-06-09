// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using System.IO;

// public class AdvancedPhysicsHapticEffector : MonoBehaviour
// {
//     private struct AdditionalData
//     {
//         public Vector3 physicsCursorPosition;
//         public bool isTouching;
//     }
//     private bool isColliding = false;

//     [Range(-2, 2)]
//     public float forceX;
//     [Range(-2, 2)]
//     public float forceY;
//     [Range(-2, 2)]
//     public float forceZ;

//     [Header("Haptics")]
//     [Tooltip("Enable/Disable force feedback")]
//     public bool forceEnabled;

//     [Range(0, 800)]
//     public float stiffness = 400f;
//     [Range(0, 3)]
//     public float damping = 1;

//     private HapticThread m_hapticThread;
//     public GameObject EndPoint;
//     [Header("Physics")]
//     [Tooltip("Use it to enable friction and mass force feeling")]
//     public bool complexJoint;
//     public float drag = 20f;
//     public float linearLimit = 0.001f;
//     public float limitSpring = 500000f;
//     public float limitDamper = 10000f;
//     public float MainForce = 0;

//     private ConfigurableJoint m_joint;
//     private Rigidbody m_rigidbody;

//     private const float MinimumReconfigureDelta = 0.5f;
//     private bool needConfigure =>
//         (complexJoint && m_joint.zMotion != ConfigurableJointMotion.Limited)
//         || Mathf.Abs(m_joint.linearLimit.limit - linearLimit) > MinimumReconfigureDelta
//         || Mathf.Abs(m_joint.linearLimitSpring.spring - limitSpring) > MinimumReconfigureDelta
//         || Mathf.Abs(m_joint.linearLimitSpring.damper - limitDamper) > MinimumReconfigureDelta
//         || Mathf.Abs(m_rigidbody.drag - drag) > MinimumReconfigureDelta;

//     [Header("Collision detection")]
//     [Tooltip("Apply force only when a collision is detected (prevent air friction feeling)")]
//     public bool collisionDetection;
//     public List<Collider> touched = new();
//     public float mass;

//     private List<CsvData> csvData;
//     private int currentIndex = 0;

//     private void Awake()
//     {
//         m_hapticThread = FindObjectOfType<HapticThread>();
//         AttachCursor(m_hapticThread.avatar.gameObject, EndPoint);
//         SetupCollisionDetection();
//         csvData = CsvReader.ReadCsv("/mnt/data/image.png"); // Replace with your actual CSV file path
//     }

//     private void OnEnable()
//     {
//         if (m_hapticThread.isInitialized)
//             m_hapticThread.Run(ForceCalculation, GetAdditionalData());
//         else
//             m_hapticThread.onInitialized.AddListener(() => m_hapticThread.Run(ForceCalculation, GetAdditionalData()));
//     }

//     public Vector3 Gravitiy()
//     {
//         float weight = -(0.1391f + 0.1f); // Weight of the object
//         Vector3 gravityForce = Vector3.down * (weight);
//         return gravityForce;
//     }

//     private Vector3 ForceCalculation(in Vector3 position, in Vector3 velocity, in AdditionalData additionalData)
//     {
//         var force = additionalData.physicsCursorPosition - position;
//         force *= stiffness;
//         force -= velocity * damping;

//         // CSV 데이터에서 힘을 가져와서 추가
//         if (currentIndex < csvData.Count)
//         {
//             var csvForce = new Vector3(csvData[currentIndex].force_x, csvData[currentIndex].force_y, csvData[currentIndex].force_z);
//             force += csvForce;
//             currentIndex++;
//         }

//         // 충돌 감지 여부에 관계없이 force를 계산
//         if (!forceEnabled)
//         {
//             force = new Vector3(forceX, forceY, forceZ);
//         }

//         force += Gravitiy();
//         MainForce = force.magnitude;

//         if (isColliding)
//         {
//             Debug.Log($"Calculated Force: {MainForce} Newtons");
//         }
//         return force;
//     }

//     private void FixedUpdate() => m_hapticThread.SetAdditionalData(GetAdditionalData());

//     private void Update()
//     {
// #if UNITY_EDITOR
//         if (needConfigure)
//         {
//             ConfigureJoint();
//         }
// #endif
//     }

//     #region Physics Joint

//     private void AttachCursor(GameObject cursor, GameObject cursor2)
//     {
//         var rbCursor = cursor.GetComponent<Rigidbody>();
//         if (!rbCursor)
//         {
//             rbCursor = cursor.AddComponent<Rigidbody>();
//             rbCursor.useGravity = false;
//             rbCursor.isKinematic = true;
//         }

//         m_rigidbody = gameObject.GetComponent<Rigidbody>();
//         if (!m_rigidbody)
//         {
//             m_rigidbody = gameObject.AddComponent<Rigidbody>();
//             m_rigidbody.useGravity = false;
//             m_rigidbody.isKinematic = false;
//             m_rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

//             m_rigidbody.constraints = 
//                           RigidbodyConstraints.FreezeRotationX |
//                           RigidbodyConstraints.FreezeRotationY |
//                           RigidbodyConstraints.FreezeRotationZ;
//         }

//         m_joint = gameObject.GetComponent<ConfigurableJoint>();
//         if (!m_joint)
//         {
//             m_joint = gameObject.AddComponent<ConfigurableJoint>();
//             m_joint.connectedBody = rbCursor;
//             m_joint.autoConfigureConnectedAnchor = false;
//             m_joint.anchor = m_joint.connectedAnchor = Vector3.zero;
//             m_joint.axis = m_joint.secondaryAxis = Vector3.zero;
//         }

//         ConfigureJoint();
//     }

//     private void ConfigureJoint()
//     {
//         if (!complexJoint)
//         {
//             m_joint.xMotion = m_joint.yMotion = m_joint.zMotion = ConfigurableJointMotion.Locked;
//             m_joint.angularXMotion = m_joint.angularYMotion = m_joint.angularZMotion = ConfigurableJointMotion.Locked;
//             m_rigidbody.drag = 0;
//         }
//         else
//         {
//             m_joint.xMotion = m_joint.yMotion = m_joint.zMotion = ConfigurableJointMotion.Limited;
//             m_joint.angularXMotion = m_joint.angularYMotion = m_joint.angularZMotion = ConfigurableJointMotion.Locked;

//             m_joint.linearLimit = new SoftJointLimit() { limit = linearLimit };
//             m_joint.linearLimitSpring = new SoftJointLimitSpring() { spring = limitSpring, damper = limitDamper };
//             m_rigidbody.drag = drag;
//         }
//     }

//     #endregion

//     #region Haptics

//     private AdditionalData GetAdditionalData()
//     {
//         AdditionalData additionalData;
//         additionalData.physicsCursorPosition = transform.localPosition;
//         additionalData.isTouching = collisionDetection && touched.Count > 0;
//         return additionalData;
//     }

//     #endregion

//     #region Collision Detection

//     private void SetupCollisionDetection()
//     {
//         var col = gameObject.GetComponent<Collider>();
//         if (!col)
//         {
//             col = gameObject.AddComponent<BoxCollider>();
//         }

//         if (!col.material)
//         {
//             col.material = new PhysicMaterial { dynamicFriction = 0, staticFriction = 0 };
//         }

//         collisionDetection = true;
//     }

//     private void OnCollisionEnter(Collision collision)
//     {
//         if (forceEnabled && collisionDetection && !touched.Contains(collision.collider))
//         {
//             touched.Add(collision.collider);
//             isColliding = true;
//         }
//     }

//     private void OnCollisionExit(Collision collision)
//     {
//         if (forceEnabled && collisionDetection && touched.Contains(collision.collider))
//         {
//             touched.Remove(collision.collider);
//             isColliding = false;
//         }
//     }

//     #endregion
// }
