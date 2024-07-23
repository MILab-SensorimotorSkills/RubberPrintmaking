#region Original Code
// using System;
// using Haply.HardwareAPI.Unity;
// using System.Collections.Generic;
// using UnityEngine;
// using System.IO;

// /// <summary>
// /// <b>EXPERIMENTAL: </b><br/>
// /// This is a more advanced example of using Unity's physics engine to implement haptics feedback in a complex scene.
// ///
// /// <para>
// /// The following two effectors are being used in the scene:
// /// </para>
// /// <list type="bullet">
// ///     <item>The Cursor (kinematic) directly controlled by the device on each haptic frame</item>
// ///     <item>The PhysicEffector (non-kinematic) with colliders linked to the Cursor by a joint
// /// attached to a sprint and damper configured with a large constant</item>
// /// </list>
// ///
// /// The haptic force is relative to the distance between the two effectors such that when the physics effector is blocked by
// /// an object in the scene, an opposing force proportional to the distance will be generated.
// ///
// /// <para>
// /// Thanks to the spring/damper, movable objects mass can be felt and Unity Physics Materials on scene objects can be used
// /// to have different haptic feelings.
// /// </para>
// ///
// /// <remarks>
// /// The haptics feeling of drag or friction that occurs on moving effector is caused by the difference in update frequency between
// /// Unity's physics engine (between 60Hz to 120Hz) and the haptics thread (~1000Hz). This difference means that the physics
// /// effector will always be lagging behind the Cursor's true position which leads to forces that resemble a step function
// /// instead of having continuous forces.
// /// 
// /// <para>Solutions :</para>
// /// <list type="bullet">
// ///     <item>Decrease the value of ProjectSettings.FixedTimestep as close to 0.001 as possible which will have
// ///     significant impact on performances for complex scenes.</item>
// ///     <item>Apply forces only when collisions occur (see <see cref="collisionDetection"/>)</item>
// ///     <item>Use a third-party physic/haptic engine like (TOIA, SOFA, etc...) as a middleware between the two physics
// ///     engine to simulate the contact points.</item>
// /// </list>
// /// </remarks>
// /// 
// /// </summary>
// public class AdvancedPhysicsHapticEffector : MonoBehaviour
// {
//     /// <summary>
//     /// Safe thread scene data
//     /// </summary>
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
//     // HAPTICS
//     [Header("Haptics")]
//     [Tooltip("Enable/Disable force feedback")]
//     public bool forceEnabled;

//     [Range(0, 800)]
//     public float stiffness = 400f;
//     [Range(0, 3)]
//     public float damping = 1;
//     public string collidingTag = string.Empty;
//     private HapticThread m_hapticThread;
//     public GameObject EndPoint;
//     // PHYSICS
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
//         // find the HapticThread object before the first FixedUpdate() call
//         m_hapticThread = FindObjectOfType<HapticThread>();

//         // create the physics link between physic effector and device cursor
//         AttachCursor(m_hapticThread.avatar.gameObject, EndPoint);
//         SetupCollisionDetection();
//     }

//     private void OnEnable()
//     {
//         // Run haptic loop with AdditionalData method to get initial values
//         if (m_hapticThread.isInitialized)
//             m_hapticThread.Run(ForceCalculation, GetAdditionalData());
//         else
//             m_hapticThread.onInitialized.AddListener(() => m_hapticThread.Run(ForceCalculation, GetAdditionalData()));
//     }
//     public Vector3 Gravitiy()
//     {
//         float weight = -(0.1391f + 0.1f); // 물체의 중력 가속도 계산

//         Vector3 gravityForce = Vector3.down * (weight);
//         //Debug.Log(gravityForce);

//         return gravityForce;
//     }
//     private Vector3 ForceCalculation(in Vector3 position, in Vector3 velocity, in AdditionalData additionalData)
//     {

//         var force = additionalData.physicsCursorPosition - position;
//         force *= stiffness;
//         force -= velocity * damping;

//         if (!forceEnabled || (collisionDetection && !additionalData.isTouching))
//         {
//             // Don't compute forces if there are no collisions which prevents feeling drag/friction while moving through air. 
//             force = new Vector3(forceX, forceY, forceZ);
//         }
//         force += Gravitiy();
//         MainForce = force.magnitude;

//         if (isColliding)
//         {
//             //Debug.Log($"Calculated Force: {MainForce} Newtons");
//         }
//         return force;
//     }




//     private void FixedUpdate() =>
//         // Update AdditionalData 
//         m_hapticThread.SetAdditionalData(GetAdditionalData());

//     private void Update()
//     {
// #if UNITY_EDITOR
//         if (needConfigure)
//         {
//             ConfigureJoint();
//         }
// #endif
//     }

//     //PHYSICS
//     #region Physics Joint

//     /// <summary>
//     /// Attach the current physics effector to device end-effector with a joint
//     /// </summary>
//     /// <param name="cursor">Cursor to attach with</param>
//     private void AttachCursor(GameObject cursor, GameObject cursor2)
//     {
//         // Add kinematic rigidbody to cursor
//         var rbCursor = cursor.GetComponent<Rigidbody>();
//         if (!rbCursor)
//         {
//             rbCursor = cursor.AddComponent<Rigidbody>();
//             rbCursor.useGravity = false;
//             rbCursor.isKinematic = true;
//         }

//         // Add non-kinematic rigidbody to self
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

//         // Connect with cursor rigidbody with a spring/damper joint and locked rotation
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
//             // limited linear movements
//             m_joint.xMotion = m_joint.yMotion = m_joint.zMotion = ConfigurableJointMotion.Limited;

//             // lock rotation to avoid sphere roll caused by physics material friction instead of feel it
//             m_joint.angularXMotion = m_joint.angularYMotion = m_joint.angularZMotion = ConfigurableJointMotion.Locked;

//             // configure limit, spring and damper
//             m_joint.linearLimit = new SoftJointLimit()
//             {
//                 limit = linearLimit
//             };
//             m_joint.linearLimitSpring = new SoftJointLimitSpring()
//             {
//                 spring = limitSpring,
//                 damper = limitDamper
//             };

//             // stabilize spring connection 
//             m_rigidbody.drag = drag;
//         }
//     }

//     #endregion

//     // HAPTICS
//     #region Haptics

//     /// <summary>
//     /// Method used by <see cref="HapticThread.Run(Haply.HardwareAPI.Unity.ForceCalculation1)"/>
//     /// and <see cref="HapticThread.GetAdditionalData{T}"/>
//     /// to transfer dynamic data between the unity scene and the haptic thread 
//     /// </summary>
//     /// <returns>Updated AdditionalData struct</returns>
//     private AdditionalData GetAdditionalData()
//     {
//         AdditionalData additionalData;
//         additionalData.physicsCursorPosition = transform.localPosition;
//         additionalData.isTouching = collisionDetection && touched.Count > 0;
//         return additionalData;
//     }

//     /// <summary>
//     /// Calculate the force to apply based on the cursor position and the scene data
//     /// <para>This method is called once per haptic frame (~1000Hz) and needs to be efficient</para>
//     /// </summary>
//     /// <param name="position">cursor position</param>
//     /// <param name="velocity">cursor velocity</param>
//     /// <param name="additionalData">additional scene data synchronized by <see cref="GetAdditionalData"/> method</param>
//     /// <returns>Force to apply</returns>

//     #endregion

//     // COLLISION DETECTION
//     #region Collision Detection

//     private void SetupCollisionDetection()
//     {
//         // Add collider if not exists
//         var col = gameObject.GetComponent<Collider>();
//         if (!col)
//         {
//             col = gameObject.AddComponent<BoxCollider>();
//         }

//         // Neutral PhysicMaterial to interact with others 
//         if (!col.material)
//         {
//             col.material = new PhysicMaterial { dynamicFriction = 0, staticFriction = 0 };

//         }

//         collisionDetection = true;
//     }

//     /// <summary>
//     /// Called when effector touch other game object
//     /// </summary>
//     /// <param name="collision">collision information</param>
//     private void OnCollisionEnter(Collision collision)
//     {
//         if (forceEnabled && collisionDetection && !touched.Contains(collision.collider))
//         {
//             // store touched object
//             touched.Add(collision.collider);
//             isColliding = true;
//             collidingTag = collision.collider.tag;

//         }
//     }

//     /// <summary>
//     /// Called when effector move away from another game object 
//     /// </summary>
//     /// <param name="collision">collision information</param>
//     private void OnCollisionExit(Collision collision)
//     {

//         if (forceEnabled && collisionDetection && touched.Contains(collision.collider))
//         {
//             touched.Remove(collision.collider);
//             isColliding = false;
//             if (touched.Count == 0)
//             {
//                 collidingTag = string.Empty;
//             }
//         }
//     }

//     #endregion
// }
#endregion

#region Feedback Type Select Code

using System;
using Haply.HardwareAPI.Unity;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class AdvancedPhysicsHapticEffector : MonoBehaviour
{
    public enum ForceFeedbackType
    {
        Default,
        Disturbance,
        Guidance
    }

    [Header("Force Feedback Type")]
    [Tooltip("Select the type of force feedback to apply: Default, Disturbance, or Guidance.")]
    public ForceFeedbackType forceFeedbackType = ForceFeedbackType.Default;

    /// <summary>
    /// Safe thread scene data
    /// </summary>
    private struct AdditionalData
    {
        public Vector3 physicsCursorPosition;
        public bool isTouching;
    }
    private bool isColliding = false;

    [Range(-2, 2)]
    public float forceX;
    [Range(-2, 2)]
    public float forceY;
    [Range(-2, 2)]
    public float forceZ;
    // HAPTICS
    [Header("Haptics")]
    [Tooltip("Enable/Disable force feedback")]
    public bool forceEnabled;

    [Range(0, 800)]
    public float stiffness = 400f;
    [Range(0, 3)]
    public float damping = 1;
    public string collidingTag = string.Empty;
    private HapticThread m_hapticThread;
    public GameObject EndPoint;
    // PHYSICS
    [Header("Physics")]
    [Tooltip("Use it to enable friction and mass force feeling")]
    public bool complexJoint;
    public float drag = 20f;
    public float linearLimit = 0.001f;
    public float limitSpring = 500000f;
    public float limitDamper = 10000f;
    public float MainForce = 0;
    public float MainForceX = 0;
    public float MainForceY = 0;
    public float MainForceZ = 0;


    private ConfigurableJoint m_joint;
    private Rigidbody m_rigidbody;

    private PointMover pointMover;

    private const float MinimumReconfigureDelta = 0.5f;
    private bool needConfigure =>
        (complexJoint && m_joint.zMotion != ConfigurableJointMotion.Limited)
        || Mathf.Abs(m_joint.linearLimit.limit - linearLimit) > MinimumReconfigureDelta
        || Mathf.Abs(m_joint.linearLimitSpring.spring - limitSpring) > MinimumReconfigureDelta
        || Mathf.Abs(m_joint.linearLimitSpring.damper - limitDamper) > MinimumReconfigureDelta
        || Mathf.Abs(m_rigidbody.drag - drag) > MinimumReconfigureDelta;

    [Header("Collision detection")]
    [Tooltip("Apply force only when a collision is detected (prevent air friction feeling)")]
    public bool collisionDetection;
    public List<Collider> touched = new();
    public float mass;

    //[Header("Perlin Noise")]
    //public float perlinNoiseIntensity = 1f; // Perlin noise intensity

    // Random noise parameters
    private System.Random random = new System.Random();
    public float randomNoiseIntensity = 1f; // Random noise intensity

    private List<CsvData1> csvData = new List<CsvData1>();
    private int currentIndex = 0;

    private Vector3 targetPosition;


    private void Awake()
    {
        // find the HapticThread object before the first FixedUpdate() call
        m_hapticThread = FindObjectOfType<HapticThread>();

        // Find the PointMover object
        pointMover = FindObjectOfType<PointMover>();
        if (pointMover == null)
        {
            Debug.LogError("PointMover not found in the scene.");
        }
        else
        {
            Debug.Log("PointMover successfully found.");
        }

        // create the physics link between physic effector and device cursor
        AttachCursor(m_hapticThread.avatar.gameObject, EndPoint);
        SetupCollisionDetection();

        // Load CSV data
        //LoadCsvData("Assets/Mainfolder/Force Data/scaling1.csv"); // 경로를 실제 CSV 파일 경로로 변경하세요.
    }

    private void OnEnable()
    {
        // Run haptic loop with AdditionalData method to get initial values
        if (m_hapticThread.isInitialized)
            m_hapticThread.Run(ForceCalculation, GetAdditionalData());
        else
            m_hapticThread.onInitialized.AddListener(() => m_hapticThread.Run(ForceCalculation, GetAdditionalData()));
    }

    public Vector3 Gravitiy()
    {
        float weight = -(0.1391f + 0.1f); // 물체의 중력 가속도 계산

        Vector3 gravityForce = Vector3.down * (weight);
        return gravityForce;
    }

    private Vector3 ForceCalculation(in Vector3 position, in Vector3 velocity, in AdditionalData additionalData)
    {
        switch (forceFeedbackType)
        {
            case ForceFeedbackType.Default:
                return CalculateDefaultForce(position, velocity, additionalData);
            case ForceFeedbackType.Disturbance:
                return CalculateDisturbanceForce(position, velocity, additionalData);
            case ForceFeedbackType.Guidance:
                return CalculateGuidanceForce(position, velocity, additionalData);
            default:
                return Vector3.zero;
        }
    }

    private Vector3 CalculateDefaultForce(Vector3 position, Vector3 velocity, AdditionalData additionalData)
    {
        var force = additionalData.physicsCursorPosition - position;
        force *= stiffness;
        force -= velocity * damping;
        //Debug.Log(force);
        if (!forceEnabled || (collisionDetection && !additionalData.isTouching))
        {
            // Don't compute forces if there are no collisions which prevents feeling drag/friction while moving through air. 
            force = new Vector3(forceX, forceY, forceZ);
        }
        
        force += Gravitiy();
        MainForceX = force.x;
        MainForceY = force.y;
        MainForceZ = force.z; 
        MainForce = force.magnitude;

        //Debug.Log($"DefaultForce Y: {force.y}");

        if (isColliding)
        {
            // Debug.Log($"Calculated Force: {MainForce} Newtons");
        }
        return force;
    }


    private Vector3 CalculateDisturbanceForce(Vector3 position, Vector3 velocity, AdditionalData additionalData)
    {
        var force = additionalData.physicsCursorPosition - position;
        force *= stiffness;
        force -= velocity * damping;

        // // Apply a small offset to the position to create disturbance
        // Vector3 disturbanceOffset = new Vector3(
        //     (float)(random.NextDouble() * 0.002 - 0.0005), // Small disturbance in X axis
        //     (float)(random.NextDouble() * 0.002 - 0.0005), // Small disturbance in Y axis
        //     (float)(random.NextDouble() * 0.002 - 0.0005)  // Small disturbance in Z axis
        // );

        // force += disturbanceOffset * stiffness;
        if (pointMover != null)
        {
            Vector3 guidanceDirection = pointMover.CurrentDirection;
            if (guidanceDirection != Vector3.zero)
            {
                // Apply force in the opposite direction of guidanceDirection
                force += -guidanceDirection.normalized * 1.0f; // 반대 방향으로 0.5N의 힘 추가
            }
        }

        if (!forceEnabled || (collisionDetection && !additionalData.isTouching))
        {
            // Don't compute additional forces if there are no collisions which prevents feeling drag/friction while moving through air. 
            force = new Vector3(forceX, forceY, forceZ);
        }
        
        force += Gravitiy();
        MainForceX = force.x;
        MainForceY = force.y;
        MainForceZ = force.z; 
        MainForce = force.magnitude;

        if (isColliding)
        {
            // Debug.Log($"Calculated Force: {MainForce} Newtons");
        }
        return force;
    }

    

    private Vector3 CalculateGuidanceForce(Vector3 position, Vector3 velocity, AdditionalData additionalData)
    {
        var force = additionalData.physicsCursorPosition - position;
        force *= stiffness;
        force -= velocity * damping;

        if (pointMover != null)
        {
            Vector3 guidanceDirection = pointMover.CurrentDirection;

            //Debug.Log($"guidanceDirection: {guidanceDirection}, targetPosition: {targetPosition}, current position: {position}");

            // if (guidanceDirection != Vector3.zero && position != targetPosition)
            // {
            //     float distanceToTarget = Vector3.Distance(position, targetPosition);
            //     // Calculate the user's force in the direction of guidanceDirection
            //     float userForceInGuidanceDirection = Vector3.Dot(force, guidanceDirection.normalized);

            //     // Add additional force only if the user's force in the guidance direction is less than 1.0N
            //     if (userForceInGuidanceDirection < 1.0f)
            //     {
            //         // Calculate the scaling factor for the guidance force based on the distance to the target
            //         float scalingFactor = Mathf.Clamp01(distanceToTarget / 1.0f); // 1.0f은 최대 거리를 의미합니다. 필요에 따라 조정 가능합니다.
            //         force += guidanceDirection.normalized * scalingFactor;
            //     }
            // }
            if (guidanceDirection != Vector3.zero && position != targetPosition)
            {
                float userForceInGuidanceDirection = Vector3.Dot(force, guidanceDirection.normalized);
                if (userForceInGuidanceDirection < 0.1f)
                {
                    force += guidanceDirection.normalized * 1.0f;
                }
            }
        }
        else
        {
            //Debug.LogError("PointMover is null");
        }

        if (!forceEnabled || (collisionDetection && !additionalData.isTouching))
        {
            force = new Vector3(forceX, forceY, forceZ);
        }

        force += Gravitiy();
        MainForceX = force.x;
        MainForceY = force.y;
        MainForceZ = force.z;
        MainForce = force.magnitude;

        if (isColliding)
        {
            // Debug.Log($"Calculated Force: {MainForce} Newtons");
        }
        return force;
    }


    private void FixedUpdate()
    {
        m_hapticThread.SetAdditionalData(GetAdditionalData());
        if (pointMover != null)
        {
            targetPosition = pointMover.PointToMovePosition;
            //Debug.Log($"아제발 - GuidanceDirection: {pointMover.CurrentDirection}, PointToMovePosition: {pointMover.PointToMovePosition}");
    
        }
    }

    private void Update()
    {
        for (int i = touched.Count - 1; i >= 0; i--)
        {
            if (touched[i] == null)
            {
                touched.RemoveAt(i);
            }
        }
        if (touched.Count > 1)
        {
            for (int i = touched.Count - 1; i > 0; i--)
            {
                RemoveCollider(touched[i]);
            }
        }
    #if UNITY_EDITOR
        if (needConfigure)
        {
            ConfigureJoint();
        }
    #endif
    }



    //PHYSICS
    #region Physics Joint

    /// <summary>
    /// Attach the current physics effector to device end-effector with a joint
    /// </summary>
    /// <param name="cursor">Cursor to attach with</param>
    private void AttachCursor(GameObject cursor, GameObject cursor2)
    {
        // Add kinematic rigidbody to cursor
        var rbCursor = cursor.GetComponent<Rigidbody>();
        if (!rbCursor)
        {
            rbCursor = cursor.AddComponent<Rigidbody>();
            rbCursor.useGravity = false;
            rbCursor.isKinematic = true;
        }

        // Add non-kinematic rigidbody to self
        m_rigidbody = gameObject.GetComponent<Rigidbody>();
        if (!m_rigidbody)
        {
            m_rigidbody = gameObject.AddComponent<Rigidbody>();
            m_rigidbody.useGravity = false;
            m_rigidbody.isKinematic = false;
            m_rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            m_rigidbody.constraints = 
                          RigidbodyConstraints.FreezeRotationX |
                          RigidbodyConstraints.FreezeRotationY |
                          RigidbodyConstraints.FreezeRotationZ;
        }

        // Connect with cursor rigidbody with a spring/damper joint and locked rotation
        m_joint = gameObject.GetComponent<ConfigurableJoint>();
        if (!m_joint)
        {
            m_joint = gameObject.AddComponent<ConfigurableJoint>();
            m_joint.connectedBody = rbCursor;
            m_joint.autoConfigureConnectedAnchor = false;
            m_joint.anchor = m_joint.connectedAnchor = Vector3.zero;
            m_joint.axis = m_joint.secondaryAxis = Vector3.zero;
        }

        ConfigureJoint();
    }

    private void ConfigureJoint()
    {
        if (!complexJoint)
        {
            m_joint.xMotion = m_joint.yMotion = m_joint.zMotion = ConfigurableJointMotion.Locked;
            m_joint.angularXMotion = m_joint.angularYMotion = m_joint.angularZMotion = ConfigurableJointMotion.Locked;
            m_rigidbody.drag = 20;
        }
        else
        {
            // limited linear movements
            m_joint.xMotion = m_joint.yMotion = m_joint.zMotion = ConfigurableJointMotion.Limited;

            // lock rotation to avoid sphere roll caused by physics material friction instead of feel it
            m_joint.angularXMotion = m_joint.angularYMotion = m_joint.angularZMotion = ConfigurableJointMotion.Locked;

            // configure limit, spring and damper
            m_joint.linearLimit = new SoftJointLimit()
            {
                limit = linearLimit
            };
            m_joint.linearLimitSpring = new SoftJointLimitSpring()
            {
                spring = limitSpring,
                damper = limitDamper
            };

            // stabilize spring connection 
            m_rigidbody.drag = drag;
        }
    }

    #endregion

    // HAPTICS
    #region Haptics

    /// <summary>
    /// Method used by <see cref="HapticThread.Run(Haply.HardwareAPI.Unity.ForceCalculation1)"/>
    /// and <see cref="HapticThread.GetAdditionalData{T}"/>
    /// to transfer dynamic data between the unity scene and the haptic thread 
    /// </summary>
    /// <returns>Updated AdditionalData struct</returns>
    private AdditionalData GetAdditionalData()
    {
        AdditionalData additionalData;
        additionalData.physicsCursorPosition = transform.localPosition;
        additionalData.isTouching = collisionDetection && touched.Count > 0;
        return additionalData;
    }

    #endregion

    // COLLISION DETECTION
    #region Collision Detection

    private void SetupCollisionDetection()
    {
        // Add collider if not exists
        var col = gameObject.GetComponent<Collider>();
        if (!col)
        {
            col = gameObject.AddComponent<BoxCollider>();
        }

        // Neutral PhysicMaterial to interact with others 
        if (!col.material)
        {
            col.material = new PhysicMaterial { dynamicFriction = 0, staticFriction = 0 };

        }

        collisionDetection = true;
    }

    /// <summary>
    /// Called when effector touch other game object
    /// </summary>
    /// <param name="collision">collision information</param>
        // Collision detection
        private void OnCollisionEnter(Collision collision)
        {
            if (forceEnabled && collisionDetection && !touched.Contains(collision.collider))
            {
                // 충돌한 오브젝트 추가
                touched.Add(collision.collider);
                isColliding = true;
                collidingTag = collision.collider.tag;

                // 파괴될 때 콜라이더 제거 리스너 추가
                OnDestroyListener listener = collision.collider.gameObject.GetComponent<OnDestroyListener>();
                if (listener == null)
                {
                    listener = collision.collider.gameObject.AddComponent<OnDestroyListener>();
                }
                listener.OnDestroyEvent += () => RemoveCollider(collision.collider);
            }
        }

    /// <summary>
    /// Called when effector move away from another game object 
    /// </summary>
    /// <param name="collision">collision information</param>
        private void OnCollisionExit(Collision collision)
        {
            if (forceEnabled && collisionDetection && touched.Contains(collision.collider))
            {
                RemoveCollider(collision.collider);
            }
        }

        private void RemoveCollider(Collider collider)
        {
            if (touched.Contains(collider))
            {
                touched.Remove(collider);
                if (touched.Count == 0)
                {
                    collidingTag = string.Empty;
                    isColliding = false;
                }
            }
        }

    #endregion

    // CSV Data loading
    private void LoadCsvData(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("CSV file not found at path: " + path);
            return;
        }

        var lines = File.ReadAllLines(path);
        foreach (var line in lines)
        {
            var values = line.Split(',');
            if (values.Length >= 6 &&
                float.TryParse(values[3], out float forceZ) &&
                float.TryParse(values[4], out float forceX) &&
                float.TryParse(values[5], out float forceY))
            {
                csvData.Add(new CsvData1 { forceX = forceX, forceY = forceY, forceZ = forceZ });
            }
        }
    }
}

public struct CsvData1
{
    public float forceX;
    public float forceY;
    public float forceZ;
}

#endregion