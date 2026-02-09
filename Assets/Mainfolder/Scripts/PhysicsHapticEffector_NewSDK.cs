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
        Hybrid,
        Adaptive
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

    [Header("Progressive Mixture (Adaptive)")]
    public int level = 0;
    public int levelMax = 4;

    public float emaTau = 0.5f;          // 성능 EMA 시간상수(초)
    public float stableVarTh = 0.002f;   // 안정 판정 분산 임계
    public float goodHoldSec = 2.0f;     // good 유지시간
    public float badHoldSec = 1.0f;      // bad 유지시간

    public float goodTh0 = 0.40f;        // level0 good threshold
    public float goodThStep = 0.05f;     // level당 감소량
    public float badMargin = 0.20f;      // badTh = goodTh + margin

    public float alphaMax0 = 0.20f;
    public float alphaMaxStep = 0.15f;

    private float _eEma = 999f;
    private float _eVarEma = 999f;
    private float _goodTimer = 0f;
    private float _badTimer = 0f;

    [Header("Adaptive Debug Logging")]
    public bool enableAdaptiveLog = true;
    [Range(0.05f, 2f)] public float logIntervalSec = 0.2f;

    private float _logTimer = 0f;

    // 최근 Adaptive 계산 내부 값 저장용 (Update/FixedUpdate에서 출력)
    private float _dbg_goodTh;
    private float _dbg_badTh;
    private float _dbg_alphaMax;
    private float _dbg_alpha;
    private float _dbg_x;
    private float _dbg_t;
    private float _dbg_sd;
    private float _dbg_e = -1f; // distance_2d snapshot
    private float _dbg_eEma;
    private float _dbg_eVarEma;

    private Vector3 _dbg_gDir;
    private Vector3 _dbg_Fbase;
    private Vector3 _dbg_Fg;
    private Vector3 _dbg_Fd;
    private Vector3 _dbg_Ffinal;


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
        Vector3 cursorLocal = _inverse3.CursorLocalPosition;
        Vector3 effectorLocal = transform.localPosition;

        Vector3 diff = cursorLocal - effectorLocal;
        // Debug.Log($"CursorLocal: {cursorLocal}, EffectorLocal: {effectorLocal}, Diff: {diff}");

        // Time.fixedDeltaTime = 0.02f;
        // Cache unity scene data for the haptic thread
        SaveCached();

        // Your original logic that updates guidance direction & distance
        UpdateGuidanceTerms();

        // ✅ 성능 측정은 "유효한 task 중"에만(예: collision 중, 또는 onnx output이 Deforming일 때)
        bool valid = collisionDetection && touched.Count > 0; // + (newoutput==DEFORMING) 같은 조건 추천
        if (valid) UpdatePerformanceAndLevel(Time.fixedDeltaTime);

        // Adaptive 모드 Debugging
        if (enableAdaptiveLog && forceFeedbackType == ForceFeedbackType.Adaptive)
        {
            _logTimer += Time.fixedDeltaTime;
            if (_logTimer >= logIntervalSec)
            {
                _logTimer = 0f;
                float goodTh = goodTh0 - goodThStep * level;
                float badTh = goodTh + badMargin;

                Debug.Log(
                    $"[Adaptive] L={level}/{levelMax} " +
                    $"e={_dbg_e:F3} eEMA={_dbg_eEma:F3} var={_dbg_eVarEma:E3} " +
                    $"goodTh={_dbg_goodTh:F3} badTh={badTh:F3} " +
                    $"alpha={_dbg_alpha:F3} alphaMax={_dbg_alphaMax:F3} x={_dbg_x:F3} " +
                    $"sd={_dbg_sd:F3} " +
                    $"|F|={_dbg_Ffinal.magnitude:F3} " +
                    $"Fg={_dbg_Fg.magnitude:F3} Fd={_dbg_Fd.magnitude:F3} Fbase={_dbg_Fbase.magnitude:F3} " +
                    $"touching={(collisionDetection && touched.Count > 0)}"
                );
            }
        }


        // SaveSceneData();

        // (Optional) joint hot-reconfigure in editor (kept behavior)
#if UNITY_EDITOR
        if (needConfigure) ConfigureJoint();
#endif
    }

    private string AdaptiveStateLabel()
    {
        if (level <= 1) return "Assist";
        if (level <= 3) return "Mixed";
        return "Challenge";
    }


    private void UpdatePerformanceAndLevel(float dt)
    {
        float e = distance_2d;

        float k = 1f - Mathf.Exp(-dt / Mathf.Max(0.001f, emaTau));
        float prev = _eEma;
        _eEma = Mathf.Lerp(_eEma, e, k);
        float diff = e - _eEma;
        _eVarEma = Mathf.Lerp(_eVarEma, diff * diff, k);

        float goodTh = goodTh0 - goodThStep * level;
        float badTh = goodTh + badMargin;

        // timers
        if (_eEma < goodTh && _eVarEma < stableVarTh) { _goodTimer += dt; _badTimer = 0f; }
        else if (_eEma > badTh) { _badTimer += dt; _goodTimer = 0f; }
        else { _goodTimer = Mathf.Max(0f, _goodTimer - dt); _badTimer = Mathf.Max(0f, _badTimer - dt); }

        if (_goodTimer > goodHoldSec) { level = Mathf.Min(levelMax, level + 1); _goodTimer = 0f; }
        if (_badTimer  > badHoldSec)  { level = Mathf.Max(0, level - 1); _badTimer = 0f; }
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
            // inverse3.SetCursorLocalForce(Vector3.zero);
            inverse3.SetCursorLocalForce(new Vector3(forceX, forceY, forceZ));
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
            case ForceFeedbackType.Adaptive:
                return CalculateAdaptiveForce(position, velocity, data, newoutput);
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
        var baseforce = force;

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
    
    private Vector3 CalculateAdaptiveForce(Vector3 position, Vector3 velocity, AdditionalData data, int output)
    {
        // var force = BaseSpringDamper(position, velocity, data);
        Vector3 baseForce = BaseSpringDamper(position, velocity, data);
        Vector3 force = baseForce;

        // 방향 벡터 확보(너 코드 그대로)
        Vector3 gDir = (pointMover != null && pointMover.CurrentDirection != Vector3.zero)
            ? pointMover.CurrentDirection.normalized
            : NoforceDirection.normalized;

        float goodTh = goodTh0 - goodThStep * level;
        goodTh = Mathf.Max(0.05f, goodTh);

        float alphaMax = alphaMax0 + alphaMaxStep * level;

        // e가 작을수록 disturbance 비중↑ (0~alphaMax)
        float x = Mathf.Clamp01(_eEma / goodTh);
        float alpha = alphaMax * (1f - x) * (1f - x) * (3f - 2f * (1f - x)); // SmoothStep(1-x)

        // guidance: error 커질수록 선형 증가
        Vector3 Fg = gDir * Mathf.Clamp(distance_2d, 0f, 5f);

        // disturbance: error 작을수록 비선형 강해짐, 방향은 -gDir 쪽(논문은 minus):contentReference[oaicite:8]{index=8}
        float sd = Mathf.Clamp(1.5f / (distance_2d + 0.8f), 0f, 1.5f);
        Vector3 Fd = -gDir * sd;

        force += (1f - alpha) * Fg + alpha * Fd;
        force += Gravity();

        UpdateForceDebug(force);

        // ---- debug snapshot (no logging here) ----
        _dbg_goodTh = goodTh;
        _dbg_alphaMax = alphaMax;
        _dbg_x = x;
        _dbg_t = 1f - x;
        _dbg_alpha = alpha;

        _dbg_e = distance_2d;
        _dbg_eEma = _eEma;
        _dbg_eVarEma = _eVarEma;

        _dbg_gDir = gDir;
        _dbg_sd = sd;

        _dbg_Fbase = force - ((1f - alpha) * Fg + alpha * Fd) - Gravity(); // 또는 아래처럼 따로 저장 추천
        _dbg_Fg = Fg;
        _dbg_Fd = Fd;
        _dbg_Ffinal = force;

        LastAdaptive = new AdaptiveSnapshot
        {
            level = level,
            e = distance_2d,
            eEma = _eEma,
            eVar = _eVarEma,
            goodTh = goodTh,
            badTh = goodTh + badMargin,
            alpha = alpha,
            alphaMax = alphaMax,
            x = x,
            sd = sd,
            Fmag = force.magnitude,
            FbaseMag = baseForce.magnitude,
            FgMag = Fg.magnitude,
            FdMag = Fd.magnitude
        };


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

    public struct CsvData1
    {
        public float forceX;
        public float forceY;
        public float forceZ;
    }

    public struct AdaptiveSnapshot
    {
        public int level;
        public float e;        // distance_2d
        public float eEma;
        public float eVar;
        public float goodTh;
        public float badTh;
        public float alpha;
        public float alphaMax;
        public float x;
        public float sd;
        public float Fmag;
        public float FbaseMag;
        public float FgMag;
        public float FdMag;
    }

    public AdaptiveSnapshot LastAdaptive;

}
