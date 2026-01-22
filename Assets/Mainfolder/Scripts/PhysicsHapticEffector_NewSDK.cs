/*
 * Updated for Haply Inverse SDK style:
 * - Uses Inverse3Controller.DeviceStateChanged as the haptic loop callback (~1000Hz)
 * - Caches scene data in FixedUpdate (Unity thread) and reads it from haptic thread safely
 */

using System;
using System.Collections.Generic;
using System.Threading;
using Haply.Inverse.DeviceControllers;
using Haply.Inverse.DeviceData;
using UnityEngine;

public class AdvancedPhysicsHapticEffector_NewSDK : MonoBehaviour
{
    public enum ForceFeedbackType
    {
        Default,
        Disturbance,
        Guidance,
        Hybrid
    }

    [Header("Force Feedback Type")]
    public ForceFeedbackType forceFeedbackType = ForceFeedbackType.Default;

    // -----------------------------
    // Thread-safe cached scene data
    // -----------------------------
    private struct AdditionalData
    {
        public Vector3 physicsCursorLocalPosition;
        public bool isTouching;
    }

    private AdditionalData _cached;
    private readonly ReaderWriterLockSlim _cacheLock = new();

    private AdditionalData GetCached()
    {
        _cacheLock.EnterReadLock();
        try { return _cached; }
        finally { _cacheLock.ExitReadLock(); }
    }

    private void SaveCached()
    {
        _cacheLock.EnterWriteLock();
        try
        {
            _cached.physicsCursorLocalPosition = transform.localPosition;
            _cached.isTouching = collisionDetection && touched.Count > 0;
        }
        finally { _cacheLock.ExitWriteLock(); }
    }

    // -----------------------------
    // Public params (kept)
    // -----------------------------
    [Header("Debug / Zero force output")]
    [Range(-2, 2)] public float forceX;
    [Range(-2, 2)] public float forceY;
    [Range(-2, 2)] public float forceZ;

    [Header("Haptics")]
    public bool forceEnabled = true;

    [SerializeField, Range(0, 800)]
    private float stiffness = 400f;

    [SerializeField, Range(0, 3)]
    private float damping = 1f;

    [Header("Physics")]
    [Tooltip("Use it to enable friction and mass force feeling")]
    public bool complexJoint = true;

    public float drag = 20f;
    public float linearLimit = 0.001f;
    public float limitSpring = 500000f;
    public float limitDamper = 10000f;

    [Header("Collision detection")]
    public bool collisionDetection = true;
    public List<Collider> touched = new();

    [Header("Optional: external guidance")]
    public Transform sphereTransform;
    public PointMover pointMover;
    public OnnxInference onnxInference;

    // Runtime debug outputs
    public float MainForce;
    public float MainForceX;
    public float MainForceY;
    public float MainForceZ;
    public float distance_2d;

    // -----------------------------
    // Internal states
    // -----------------------------
    private Inverse3Controller _inverse3;

    private ConfigurableJoint _joint;
    private Rigidbody _rigidbody;

    private bool isColliding;
    public string collidingTag = string.Empty;

    private int newoutput;
    private Vector3 NoforceDirection;

    private const float MinimumReconfigureDelta = 0.5f;

    private bool needConfigure =>
        _joint != null && _rigidbody != null && (
            (complexJoint && _joint.zMotion != ConfigurableJointMotion.Limited)
            || Mathf.Abs(_joint.linearLimit.limit - linearLimit) > MinimumReconfigureDelta
            || Mathf.Abs(_joint.linearLimitSpring.spring - limitSpring) > MinimumReconfigureDelta
            || Mathf.Abs(_joint.linearLimitSpring.damper - limitDamper) > MinimumReconfigureDelta
            || Mathf.Abs(_rigidbody.drag - drag) > MinimumReconfigureDelta
        );

    // -----------------------------
    // Unity lifecycle
    // -----------------------------
    private void Awake()
    {
        _inverse3 = GetComponentInParent<Inverse3Controller>();
        if (_inverse3 == null)
        {
            Debug.LogError("Inverse3Controller not found in parent.");
            enabled = false;
            return;
        }

        if (pointMover == null) pointMover = FindObjectOfType<PointMover>();
        if (onnxInference == null) onnxInference = FindObjectOfType<OnnxInference>();

        AttachToInverseCursor();
        SetupCollisionDetection();
    }

    private volatile bool _allowForce; // thread-safe enough for bool

    private void OnEnable()
    {
        _inverse3.DeviceStateChanged += OnDeviceStateChanged;

        if (onnxInference != null)
            onnxInference.OnOutputCalculated += HandleOutputCalculated;
    }

    private void OnDisable()
    {
        if (_inverse3 != null)
            _inverse3.DeviceStateChanged -= OnDeviceStateChanged;

        if (onnxInference != null)
            onnxInference.OnOutputCalculated -= HandleOutputCalculated;
    }

    private void FixedUpdate()
    {
        // Cache unity scene data for the haptic thread
        SaveCached();

        // Your original logic that updates guidance direction & distance
        UpdateGuidanceTerms();

        // SaveSceneData();

        // (Optional) joint hot-reconfigure in editor (kept behavior)
#if UNITY_EDITOR
        if (needConfigure) ConfigureJoint();
#endif
    }

    private void Update()
    {
        // Clean null colliders (original behavior)
        for (int i = touched.Count - 1; i >= 0; i--)
            if (touched[i] == null) touched.RemoveAt(i);

        // If more than 1 collider touched, keep only the first (original behavior)
        if (touched.Count > 1)
        {
            for (int i = touched.Count - 1; i > 0; i--)
                RemoveCollider(touched[i]);
        }
    }

    private void HandleOutputCalculated(int output) => newoutput = output;

    // -----------------------------
    // Haptics loop (NEW SDK)
    // -----------------------------
    private void OnDeviceStateChanged(object sender, Inverse3EventArgs args)
    {
        var inverse3 = args.DeviceController;
        
        if (inverse3 == null || inverse3.Cursor == null) return;
        if (!inverse3.IsReady) return;


        // Read cached scene data (thread-safe)
        var data = GetCached();

        if (!forceEnabled || (collisionDetection && !data.isTouching))
        {
            // Debug.Log("No force applied (disabled or no collision)");
            inverse3.SetCursorLocalForce(Vector3.zero);
            return;
        }

        // Compute force in device local space:
        // - position/velocity are already in local coordinates (CursorLocalPosition, CursorLocalVelocity)
        var force = ForceCalculation(
            inverse3.CursorLocalPosition,
            inverse3.CursorLocalVelocity,
            data
        );
        // Debug.Log($"Applied Force: {force}");

        inverse3.SetCursorLocalForce(force);
        // Debug.Log("Force applied");
    }

    private Vector3 ForceCalculation(in Vector3 position, in Vector3 velocity, in AdditionalData data)
    {
        switch (forceFeedbackType)
        {
            case ForceFeedbackType.Default:
                return CalculateDefaultForce(position, velocity, data);
            case ForceFeedbackType.Disturbance:
                return CalculateDisturbanceForce(position, velocity, data);
            case ForceFeedbackType.Guidance:
                return CalculateGuidanceForce(position, velocity, data);
            case ForceFeedbackType.Hybrid:
                return CalculateHybridForce(position, velocity, data, newoutput);
            default:
                return Vector3.zero;
        }
    }

    // -----------------------------
    // Forces (kept close to original)
    // -----------------------------
    private Vector3 Gravity()
    {
        // NOTE: your original value was a constant "weight" term.
        float weight = -(0.1391f + 0.1f);
        return Vector3.down * weight;
    }

    
    // private struct PhysicsCursorData
    //     {
    //         public Vector3 position;
    //         public bool collision;
    //     }

    // private PhysicsCursorData _cachedPhysicsCursorData;

    // private PhysicsCursorData GetSceneData()
    //     {
    //         _cacheLock.EnterReadLock();
    //         try
    //         {
    //             return _cachedPhysicsCursorData;
    //         }
    //         finally
    //         {
    //             _cacheLock.ExitReadLock();
    //         }
    //     }


    //     private void SaveSceneData()
    //     {
    //         _cacheLock.EnterWriteLock();
    //         try
    //         {
    //             _cachedPhysicsCursorData.position = transform.localPosition;
    //             _cachedPhysicsCursorData.collision = collisionDetection && touched.Count > 0;
    //         }
    //         finally
    //         {
    //             _cacheLock.ExitWriteLock();
    //         }
    //     }

    private Vector3 BaseSpringDamper(Vector3 position, Vector3 velocity, AdditionalData data)
    {
        var force = data.physicsCursorLocalPosition - position;
        // var physicsCursorData = GetSceneData();
        // var cursordata = physicsCursorData.position;
        // Debug.Log($"newsdk: {cursordata}");
        // Debug.Log($"old version: {data.physicsCursorLocalPosition}");
        // Debug.Log($"[Effector] Cursor Local Position: {position}");
        force *= stiffness;
        force -= velocity * damping;
        // Debug.Log($"[Effector] SpringDamper Force: {force}");
        return force;
    }

    private Vector3 CalculateDefaultForce(Vector3 position, Vector3 velocity, AdditionalData data)
    {
        var force = BaseSpringDamper(position, velocity, data);

        // You were outputting (forceX,Y,Z) when "no collision".
        // In NEW flow, this method only runs when collision exists.
        // If you still want the same behavior inside the force mode:
        // keep this as a "baseline offset" instead of a replacement:
        // force += new Vector3(forceX, forceY, forceZ);
        force += Gravity();

        UpdateForceDebug(force);
        return force;
    }

    private Vector3 CalculateDisturbanceForce(Vector3 position, Vector3 velocity, AdditionalData data)
    {
        var force = BaseSpringDamper(position, velocity, data);

        if (pointMover != null && pointMover.CurrentDirection != Vector3.zero)
        {
            float scalingFactor = Mathf.Clamp(1.5f / (distance_2d + 0.8f), 0, 1.5f);
            force += -pointMover.CurrentDirection * scalingFactor;
        }
        else
        {
            force += NoforceDirection * 1.1f;
        }

        force += Gravity();

        UpdateForceDebug(force);
        return force;
    }

    private Vector3 CalculateGuidanceForce(Vector3 position, Vector3 velocity, AdditionalData data)
    {
        var force = BaseSpringDamper(position, velocity, data);

        if (pointMover != null && pointMover.CurrentDirection != Vector3.zero)
        {
            float scalingFactor = Mathf.Clamp(distance_2d, 1.2f, 5.0f);
            force += NoforceDirection * scalingFactor;
        }
        else
        {
            force += NoforceDirection * 1.1f;
        }

        force += Gravity();

        UpdateForceDebug(force);
        return force;
    }

    private Vector3 CalculateHybridForce(Vector3 position, Vector3 velocity, AdditionalData data, int output)
    {
        var force = BaseSpringDamper(position, velocity, data);
        force += NoforceDirection * 1.1f;

        if (pointMover != null && pointMover.CurrentDirection != Vector3.zero)
        {
            var guidanceDirection = pointMover.CurrentDirection;

            if (distance_2d < 0.3f)
            {
                float scalingFactor = Mathf.Clamp(-1.5f / (distance_2d + 0.8f), -1.5f, 0);
                force += guidanceDirection.normalized * scalingFactor; // disturbance-like
            }
            else
            {
                float scalingFactor = Mathf.Clamp(distance_2d, 0, 5.0f);
                force += NoforceDirection * scalingFactor; // guidance-like
            }
        }

        force += Gravity();

        UpdateForceDebug(force);
        return force;
    }

    private void UpdateForceDebug(Vector3 force)
    {
        MainForceX = force.x;
        MainForceY = force.y;
        MainForceZ = force.z;
        MainForce = force.magnitude;
    }

    // -----------------------------
    // Guidance terms (moved from FixedUpdate)
    // -----------------------------
    private void UpdateGuidanceTerms()
    {
        if (pointMover == null || sphereTransform == null) return;

        Vector3 spherePosition = sphereTransform.position;
        Vector3 targetPosition = pointMover.PointToMovePosition;

        NoforceDirection = (targetPosition - spherePosition).normalized;

        // XZ plane distance
        Vector3 sphereXZ = new Vector3(spherePosition.x, 0, spherePosition.z);
        Vector3 targetXZ = new Vector3(targetPosition.x, 0, targetPosition.z);
        distance_2d = Vector3.Distance(sphereXZ, targetXZ);
    }

    // -----------------------------
    // Physics joint (NEW SDK cursor)
    // -----------------------------
    private void AttachToInverseCursor()
    {
        // Cursor rigidbody (kinematic)
        var cursorGo = _inverse3.Cursor.gameObject;
        var rbCursor = cursorGo.GetComponent<Rigidbody>();
        if (!rbCursor)
        {
            rbCursor = cursorGo.AddComponent<Rigidbody>();
            rbCursor.useGravity = false;
            rbCursor.isKinematic = true;
        }

        // Effector rigidbody (dynamic)
        _rigidbody = GetComponent<Rigidbody>();
        if (!_rigidbody)
        {
            _rigidbody = gameObject.AddComponent<Rigidbody>();
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = false;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rigidbody.constraints = RigidbodyConstraints.None;
            // 아래대로 하면 조각도 Rotation Freeze가 되어버림
                // RigidbodyConstraints.FreezeRotationX |
                // RigidbodyConstraints.FreezeRotationY |
                // RigidbodyConstraints.FreezeRotationZ;
        }

        // Joint
        _joint = GetComponent<ConfigurableJoint>();
        if (!_joint) _joint = gameObject.AddComponent<ConfigurableJoint>();

        _joint.connectedBody = rbCursor;
        _joint.autoConfigureConnectedAnchor = false;
        _joint.anchor = _joint.connectedAnchor = Vector3.zero;
        _joint.axis = _joint.secondaryAxis = Vector3.zero;

        ConfigureJoint();
    }

    private void ConfigureJoint()
    {
        if (_joint == null || _rigidbody == null) return;

        if (!complexJoint)
        {
            _joint.xMotion = _joint.yMotion = _joint.zMotion = ConfigurableJointMotion.Locked;
            _joint.angularXMotion = _joint.angularYMotion = _joint.angularZMotion = ConfigurableJointMotion.Locked;
            _rigidbody.drag = 20f;
        }
        else
        {
            _joint.xMotion = _joint.yMotion = _joint.zMotion = ConfigurableJointMotion.Limited;
            _joint.angularXMotion = _joint.angularYMotion = _joint.angularZMotion = ConfigurableJointMotion.Locked;

            _joint.linearLimit = new SoftJointLimit { limit = linearLimit };
            _joint.linearLimitSpring = new SoftJointLimitSpring { spring = limitSpring, damper = limitDamper };

            _rigidbody.drag = drag;
        }
    }

    // -----------------------------
    // Collision detection (same)
    // -----------------------------
    private void SetupCollisionDetection()
    {
        var col = GetComponent<Collider>();
        if (!col) col = gameObject.AddComponent<BoxCollider>();

        if (!col.material)
            col.material = new PhysicMaterial { dynamicFriction = 0, staticFriction = 0 };

        collisionDetection = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Debug.Log($"[Effector] CollisionEnter with {collision.collider.name}, tag={collision.collider.tag}");
        if (forceEnabled && collisionDetection && !touched.Contains(collision.collider))
        {
            touched.Add(collision.collider);
            isColliding = true;
            collidingTag = collision.collider.tag;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (forceEnabled && collisionDetection && touched.Contains(collision.collider))
            RemoveCollider(collision.collider);
    }

    private void RemoveCollider(Collider collider)
    {
        if (!touched.Contains(collider)) return;

        touched.Remove(collider);
        if (touched.Count == 0)
        {
            collidingTag = string.Empty;
            isColliding = false;
        }
    }
}
