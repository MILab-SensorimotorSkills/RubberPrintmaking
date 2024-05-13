using UnityEngine;

/// <summary>
/// Configures a joint between two GameObjects.
/// </summary>
public class JointConfigurator : MonoBehaviour
{
    [Tooltip("The cursor object to which the current object will be attached.")]
    public GameObject cursor;

    [Tooltip("End point object if needed for additional configurations.")]
    public GameObject endPoint;

    private ConfigurableJoint m_joint;
    private Rigidbody m_rigidbody;

    [Header("Joint Settings")]
    public bool complexJoint;
    public float drag = 20f;
    public float linearLimit = 0.001f;
    public float limitSpring = 500000f;
    public float limitDamper = 10000f;

    private const float MinimumReconfigureDelta = 0.5f;

    private bool needConfigure =>
        (complexJoint && m_joint.zMotion != ConfigurableJointMotion.Limited)
        || Mathf.Abs(m_joint.linearLimit.limit - linearLimit) > MinimumReconfigureDelta
        || Mathf.Abs(m_joint.linearLimitSpring.spring - limitSpring) > MinimumReconfigureDelta
        || Mathf.Abs(m_joint.linearLimitSpring.damper - limitDamper) > MinimumReconfigureDelta
        || Mathf.Abs(m_rigidbody.drag - drag) > MinimumReconfigureDelta;

    void Start()
    {
        AttachCursor(cursor, endPoint);
    }

    private void Update()
    {
        if (needConfigure)
        {
            ConfigureJoint();
        }
    }

    private void AttachCursor(GameObject cursor, GameObject cursor2)
    {
        // Add kinematic rigidbody to cursor
        var rbCursor = cursor.GetComponent<Rigidbody>();
        if (rbCursor == null)
        {
            rbCursor = cursor.AddComponent<Rigidbody>();
            rbCursor.useGravity = false;
            rbCursor.isKinematic = true;
        }

        // Add non-kinematic rigidbody to self
        m_rigidbody = GetComponent<Rigidbody>();
        if (m_rigidbody == null)
        {
            m_rigidbody = gameObject.AddComponent<Rigidbody>();
            m_rigidbody.useGravity = false;
            m_rigidbody.isKinematic = false;
            m_rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            m_rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }

        // Connect with cursor rigidbody with a spring/damper joint and locked rotation
        m_joint = GetComponent<ConfigurableJoint>();
        if (m_joint == null)
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
            m_rigidbody.drag = 0;
        }
        else
        {
            m_joint.xMotion = m_joint.yMotion = m_joint.zMotion = ConfigurableJointMotion.Limited;
            m_joint.angularXMotion = m_joint.angularYMotion = m_joint.angularZMotion = ConfigurableJointMotion.Locked;
            m_joint.linearLimit = new SoftJointLimit { limit = linearLimit };
            m_joint.linearLimitSpring = new SoftJointLimitSpring { spring = limitSpring, damper = limitDamper };
            m_rigidbody.drag = drag;
        }
    }
}
